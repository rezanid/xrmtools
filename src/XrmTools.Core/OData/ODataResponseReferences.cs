#nullable enable
namespace XrmTools.OData;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

internal sealed class ODataCapturedResponse : IDisposable
{
    private IReadOnlyDictionary<string, string> headers = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
    public string Body { get; set; } = "";
    public ODataStoredBody? StoredBody { get; set; }
    public long ContentByteLength { get; set; }
    public TextReader OpenBody() => StoredBody?.OpenText() ?? new StringReader(Body);
    public void Dispose() => StoredBody?.Release();
    private readonly Dictionary<string, string[]> completion = new(StringComparer.Ordinal);

    public string[] GetBodyCompletion(string path, CancellationToken token)
    {
        lock (completion)
            if (completion.TryGetValue(path, out var cached)) return cached;
        var items = ODataResponseReferences.DescribePath(this, path, token);
        lock (completion)
        {
            if (completion.Count >= 32) completion.Clear();
            completion[path] = items;
        }
        return items;
    }
    public IReadOnlyDictionary<string, string> Headers
    {
        get => headers;
        set
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            var copy = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var header in value)
            {
                if (IsCredentialHeader(header.Key)) throw new ArgumentException($"Credential header '{header.Key}' cannot be captured.", nameof(value));
                copy.Add(header.Key, header.Value);
            }
            headers = new ReadOnlyDictionary<string, string>(copy);
        }
    }
    public string? Principal { get; set; }

    public ODataCapturedResponse() { }

    public ODataCapturedResponse(string body, IReadOnlyDictionary<string, string> headers, string? principal)
    {
        Body = body ?? throw new ArgumentNullException(nameof(body));
        Headers = headers;
        Principal = principal;
    }

    public static bool IsCredentialHeader(string name) =>
        name.Equals("Authorization", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("Proxy-Authorization", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("WWW-Authenticate", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("Proxy-Authenticate", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("Authentication-Info", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("Proxy-Authentication-Info", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("Cookie", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("X-Api-Key", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("X-Auth-Token", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("X-Access-Token", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("X-Ms-Authorization-Auxiliary", StringComparison.OrdinalIgnoreCase);
}

internal static class ODataResponseReferences
{
    private const int ExpansionLimit = 4 * 1024 * 1024;
    private const int UrlExpansionLimit = 32 * 1024;
    private static readonly Regex Placeholder = new(@"\{\{\s*([^{}]+?)\s*\}\}", RegexOptions.Compiled);
    private static readonly Regex Prefix = new(@"^([A-Za-z_][A-Za-z0-9_]*)\.response\.(body|headers)(.*)$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex Identifier = new(@"\G[A-Za-z_][A-Za-z0-9_]*", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static string Substitute(string text, bool json, Func<string, ODataCapturedResponse?> resolver, Action<ODataCapturedResponse> onCaptureUsed, bool target = false, CancellationToken cancellationToken = default)
    {
        var result = new StringBuilder();
        int last = 0;
        bool inString = false, escaped = false;
        foreach (Match match in Placeholder.Matches(text))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (json)
                for (int i = last; i < match.Index; i++)
                {
                    char c = text[i];
                    if (inString && escaped) escaped = false;
                    else if (inString && c == '\\') escaped = true;
                    else if (c == '"') inString = !inString;
                }
            result.Append(text, last, match.Index - last);
            string expression = match.Groups[1].Value.Trim();
            var prefix = Prefix.Match(expression);
            if (!prefix.Success) throw new FormatException($"Unsupported response expression '{expression}'. Use name.response.body.$ or name.response.headers.Header-Name.");
            var capture = resolver(prefix.Groups[1].Value) ?? throw new FormatException($"No captured response for request '{prefix.Groups[1].Value}'. Run that named request first.");
            JToken selected = Select(capture, prefix.Groups[2].Value, prefix.Groups[3].Value, expression, cancellationToken);
            string replacement;
            if (target && selected is JArray array && IsQueryAliasValue(text, match))
            {
                foreach (var item in array) Scalar(item, expression);
                replacement = Uri.EscapeDataString(array.ToString(Formatting.None));
            }
            else if (!json) replacement = Scalar(selected, expression);
            else if (inString)
            {
                if (escaped) throw new FormatException($"Response reference '{expression}' cannot follow a JSON escape character.");
                int end = FindStringEnd(text, match.Index + match.Length);
                int after = end + 1;
                while (after < text.Length && char.IsWhiteSpace(text[after])) after++;
                if (end < 0 || (after < text.Length && text[after] == ':'))
                    throw new FormatException($"Response reference '{expression}' cannot occur in a JSON property name or unterminated string.");
                string encoded = JsonConvert.ToString(Scalar(selected, expression));
                replacement = encoded.Substring(1, encoded.Length - 2);
            }
            else
            {
                int before = match.Index - 1, after = match.Index + match.Length;
                while (before >= 0 && char.IsWhiteSpace(text[before])) before--;
                while (after < text.Length && char.IsWhiteSpace(text[after])) after++;
                if ((before >= 0 && ":[,".IndexOf(text[before]) < 0) || (after < text.Length && ",]}".IndexOf(text[after]) < 0))
                    throw new FormatException($"Response reference '{expression}' must be an entire JSON value, not a property name or partial value.");
                replacement = selected.ToString(Formatting.None);
            }
            onCaptureUsed(capture);
            if (result.Length + replacement.Length > (target ? UrlExpansionLimit : ExpansionLimit))
                throw new FormatException("The expanded request value is too large. Select fewer response values.");
            result.Append(replacement);
            last = match.Index + match.Length;
        }
        result.Append(text, last, text.Length - last);
        if (last != 0 && result.Length > (target ? UrlExpansionLimit : ExpansionLimit))
            throw new FormatException("The expanded request value is too large. Select fewer response values.");
        string expanded = result.ToString();
        if (json && last != 0)
        {
            try
            {
                using var reader = new JsonTextReader(new StringReader(expanded)) { DateParseHandling = DateParseHandling.None };
                JToken.ReadFrom(reader);
                if (reader.Read()) throw new JsonReaderException("Extra content after JSON value.");
            }
            catch (JsonException ex) { throw new FormatException("Response substitutions must form a valid JSON body with placeholders only in value positions.", ex); }
        }
        return expanded;
    }

    private static bool IsQueryAliasValue(string target, Match placeholder)
    {
        int query = target.IndexOf('?');
        if (query < 0 || query >= placeholder.Index) return false;
        int end = placeholder.Index + placeholder.Length;
        if (end < target.Length && target[end] != '&') return false;

        int start = query + 1;
        char quote = '\0';
        for (int i = start; i < placeholder.Index; i++)
        {
            char c = target[i];
            if (c == quote) quote = '\0';
            else if (quote == '\0' && (c == '\'' || c == '"')) quote = c;
            else if (quote == '\0' && c == '&') start = i + 1;
        }
        return quote == '\0' && Regex.IsMatch(target.Substring(start, placeholder.Index - start), @"\A@[A-Za-z_][A-Za-z0-9_]*=\z");
    }

    private static int FindStringEnd(string text, int start)
    {
        bool escaped = false;
        for (int i = start; i < text.Length; i++)
        {
            if (escaped) escaped = false;
            else if (text[i] == '\\') escaped = true;
            else if (text[i] == '"') return i;
        }
        return -1;
    }

    private static string Scalar(JToken value, string expression)
    {
        if (value.Type == JTokenType.String) return value.Value<string>()!;
        if (value.Type == JTokenType.Integer || value.Type == JTokenType.Float || value.Type == JTokenType.Boolean || value.Type == JTokenType.Null)
            return value.ToString(Formatting.None);
        throw new FormatException($"Response reference '{expression}' selects an object or array; a scalar is required here.");
    }

    private static JToken Select(ODataCapturedResponse capture, string part, string path, string expression, CancellationToken token)
    {
        if (part == "headers")
        {
            string header = ParseHeader(path, expression);
            if (!capture.Headers.TryGetValue(header, out var value)) throw new FormatException($"Response header '{header}' is missing in '{expression}'.");
            return new JValue(value);
        }
        var steps = ParsePath(path, expression);
        try
        {
            using var reader = new CancellableJsonReader(capture.OpenBody(), token);
            using var output = new BoundedJsonText();
            using var writer = new JsonTextWriter(output);
            if (!reader.Read() || !WriteSelection(reader, writer, steps, 0))
                throw new FormatException($"Response data selected by '{expression}' is missing or has the wrong type.");
            if (reader.Read()) throw new JsonReaderException("Extra content after JSON value.");
            using var selectedReader = new JsonTextReader(new StringReader(output.ToString())) { DateParseHandling = DateParseHandling.None };
            return JToken.ReadFrom(selectedReader);
        }
        catch (JsonException ex) { throw new FormatException($"Response body for '{expression}' is not valid JSON.", ex); }
    }

    internal static string ValidateSelector(string expression)
    {
        var match = Prefix.Match(expression);
        if (!match.Success) throw new FormatException("Use name.response.body.$ or name.response.headers.Header-Name.");
        if (match.Groups[2].Value == "body") ParsePath(match.Groups[3].Value, expression);
        else ParseHeader(match.Groups[3].Value, expression);
        return match.Groups[1].Value;
    }

    private static string ParseHeader(string path, string expression)
    {
        if (path.StartsWith(".", StringComparison.Ordinal) && Regex.IsMatch(path.Substring(1), @"^[A-Za-z0-9!#$%&'*+\.\-^_`|~]+$"))
            return path.Substring(1);
        if (TryQuotedKey(path, 0, out string header, out int end) && end == path.Length) return header;
        throw new FormatException($"Unsupported header selector in '{expression}'. Use .headers.Header-Name or .headers['Header-Name'].");
    }

    private static List<(string? Key, int Index)> ParsePath(string path, string expression)
    {
        if (!path.StartsWith(".$", StringComparison.Ordinal)) throw new FormatException($"Unsupported body selector in '{expression}'. Use .body.$ followed by properties or array indexes.");
        var steps = new List<(string?, int)>();
        bool projected = false;
        int pos = 2;
        while (pos < path.Length)
        {
            if (steps.Count == 128) throw new FormatException("The response path is too deeply nested.");
            string? key = null;
            int index = -1;
            if (path[pos] == '.')
            {
                var match = Identifier.Match(path, pos + 1);
                if (!match.Success) throw new FormatException($"Unsupported body selector in '{expression}' at '{path.Substring(pos)}'.");
                key = match.Value;
                pos = match.Index + match.Length;
            }
            else if (path[pos] == '[')
            {
                if (string.CompareOrdinal(path, pos, "[*]", 0, 3) == 0)
                {
                    if (projected) throw new FormatException($"Response reference '{expression}' supports only one array projection.");
                    projected = true;
                    steps.Add((null, -1));
                    pos += 3;
                    continue;
                }
                if (TryQuotedKey(path, pos, out var quoted, out var end)) { key = quoted; pos = end; }
                else
                {
                    int close = path.IndexOf(']', pos + 1);
                    string digits = close < 0 ? "" : path.Substring(pos + 1, close - pos - 1);
                    if (!Regex.IsMatch(digits, @"^[0-9]+$") || !int.TryParse(digits, NumberStyles.None, CultureInfo.InvariantCulture, out index))
                        throw new FormatException($"Unsupported array index in '{expression}'.");
                    pos = close + 1;
                }
            }
            else throw new FormatException($"Unsupported body selector in '{expression}' at '{path.Substring(pos)}'.");
            steps.Add((key, index));
        }
        return steps;
    }

    private static bool WriteSelection(JsonTextReader reader, JsonWriter writer, List<(string? Key, int Index)> steps, int depth)
    {
        if (depth == steps.Count)
        {
            writer.WriteToken(reader);
            return true;
        }

        var (key, index) = steps[depth];
        bool objectStep = key != null;
        if (reader.TokenType != (objectStep ? JsonToken.StartObject : JsonToken.StartArray)) return false;
        var end = objectStep ? JsonToken.EndObject : JsonToken.EndArray;
        bool project = !objectStep && index == -1;
        bool found = project;
        if (project) writer.WriteStartArray();
        int item = 0;
        while (reader.Read() && reader.TokenType != end)
        {
            if (reader.TokenType == JsonToken.Comment) continue;
            bool match = project || (!found && (objectStep ? Equals(reader.Value, key) : item == index));
            if (objectStep && !reader.Read()) throw new JsonReaderException("Missing property value.");
            if (match)
            {
                if (!WriteSelection(reader, writer, steps, depth + 1)) return false;
                found = true;
            }
            else reader.Skip();
            item++;
        }
        if (reader.TokenType != end) throw new JsonReaderException("Unterminated container.");
        if (project) writer.WriteEndArray();
        return found;
    }

    internal static string[] DescribePath(ODataCapturedResponse capture, string path, CancellationToken token)
    {
        var keys = new HashSet<string>(StringComparer.Ordinal);
        bool array = false;
        try
        {
            var steps = ParsePath("." + path, path);
            using var reader = new CancellableJsonReader(capture.OpenBody(), token);
            if (reader.Read()) Visit(0);

            void Visit(int depth)
            {
                token.ThrowIfCancellationRequested();
                if (depth == steps.Count)
                {
                    if (reader.TokenType == JsonToken.StartArray) array = true;
                    if (reader.TokenType != JsonToken.StartObject) return;
                    while (keys.Count < 128 && reader.Read() && reader.TokenType == JsonToken.PropertyName)
                    {
                        token.ThrowIfCancellationRequested();
                        if (reader.Value is string property && property.Length <= 256 && !property.Any(c => char.IsControl(c) || c is '{' or '}'))
                            keys.Add(property);
                        reader.Skip();
                    }
                    return;
                }
                var (key, index) = steps[depth];
                bool project = key == null && index < 0;
                if (reader.TokenType != (key != null ? JsonToken.StartObject : JsonToken.StartArray)) return;
                int item = 0;
                while (reader.Read() && reader.TokenType != (key != null ? JsonToken.EndObject : JsonToken.EndArray))
                {
                    token.ThrowIfCancellationRequested();
                    if (reader.TokenType == JsonToken.Comment) continue;
                    bool matches = project || (key != null ? Equals(reader.Value, key) : item == index);
                    if (key != null && !reader.Read()) return;
                    if (matches)
                    {
                        int valueDepth = reader.Depth;
                        Visit(depth + 1);
                        if (!project || item == 7 || keys.Count >= 128) return;
                        // Finish the sampled value if its visitor stopped inside it.
                        while (reader.Depth > valueDepth && reader.Read()) token.ThrowIfCancellationRequested();
                    }
                    reader.Skip();
                    item++;
                }
            }
        }
        catch (JsonException) { }
        catch (FormatException) { }
        catch (IOException) { }
        return keys.OrderBy(key => key, StringComparer.Ordinal)
            .Select(key => Identifier.Match(key) is { Success: true } match && match.Length == key.Length
                ? "." + key : "['" + key.Replace("\\", "\\\\").Replace("'", "\\'") + "']")
            .Concat(array ? new[] { "[0]", "[*]" } : Array.Empty<string>()).ToArray();
    }

    private sealed class CancellableJsonReader : JsonTextReader
    {
        private readonly CancellationToken token;

        public CancellableJsonReader(TextReader text, CancellationToken token) : base(text)
        {
            this.token = token;
            DateParseHandling = DateParseHandling.None;
        }

        public override bool Read()
        {
            token.ThrowIfCancellationRequested();
            return base.Read();
        }
    }

    private sealed class BoundedJsonText : StringWriter
    {
        private void Check(int length)
        {
            if (GetStringBuilder().Length + length > ExpansionLimit)
                throw new FormatException("The selected response data exceeds 4 MiB of JSON text. Select a smaller value.");
        }

        public override void Write(char value) { Check(1); base.Write(value); }
        public override void Write(char[] buffer, int index, int count) { Check(count); base.Write(buffer, index, count); }
        public override void Write(string? value) { Check(value?.Length ?? 0); base.Write(value); }
    }

    private static bool TryQuotedKey(string path, int start, out string key, out int end)
    {
        key = ""; end = start;
        if (start + 2 >= path.Length || path[start] != '[' || (path[start + 1] != '\'' && path[start + 1] != '"')) return false;
        char quote = path[start + 1];
        var value = new StringBuilder();
        int i = start + 2;
        for (; i < path.Length; i++)
        {
            if (path[i] == quote)
            {
                if (i + 1 >= path.Length || path[i + 1] != ']') return false;
                key = value.ToString(); end = i + 2; return true;
            }
            if (path[i] == '\\')
            {
                if (++i >= path.Length || (path[i] != quote && path[i] != '\\')) return false;
            }
            value.Append(path[i]);
        }
        return false;
    }
}
