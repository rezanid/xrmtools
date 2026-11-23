#nullable enable
namespace XrmTools.OData;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

internal sealed class ODataRequest
{
    public int Line { get; set; }
    public int StartLine { get; set; }
    public int EndLine { get; set; }
    public string Name { get; set; } = "";
    public string RequestName { get; set; } = "";
    public string Method { get; set; } = "GET";
    public string Target { get; set; } = "";
    public string Body { get; set; } = "";
    public string ExecutableText { get; set; } = "";
    public List<KeyValuePair<string, string>> Headers { get; } = [];
    public string? Error { get; set; }
}

/// <summary>A deliberately bounded HTTP-like format. Parsing never authenticates or executes requests.</summary>
internal sealed class ODataDocument
{
    internal static readonly Regex Variable = new(@"\{\{\s*([^{}]+?)\s*\}\}", RegexOptions.Compiled);
    public List<ODataRequest> Requests { get; } = [];
    public Dictionary<string, string> Variables { get; } = new(StringComparer.Ordinal);
    public Dictionary<string, string> EnvironmentVariables { get; } = new(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> EffectiveVariables
    {
        get
        {
            var variables = new Dictionary<string, string>(EnvironmentVariables, StringComparer.Ordinal);
            foreach (var variable in Variables) variables[variable.Key] = variable.Value;
            return variables;
        }
    }
    public List<string> Errors { get; } = [];

    public void SetEnvironmentVariables(IReadOnlyDictionary<string, string> variables)
    {
        EnvironmentVariables.Clear();
        foreach (var variable in variables) EnvironmentVariables[variable.Key] = variable.Value;
    }

    public static ODataDocument Parse(string text, Action<int, int, string>? scanReferences = null,
        Action<int, int, string>? duplicateName = null, Action<int, int, string, bool>? declaration = null)
    {
        var document = new ODataDocument();
        var lines = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        ODataRequest? request = null;
        bool body = false;
        var bodyLines = new List<string>();
        var executableLines = new List<string>();
        string name = "";
        string requestName = "";
        var names = new HashSet<string>(StringComparer.Ordinal);
        int sectionLine = 0;
        void Finish(int endLine)
        {
            if (request != null)
            {
                request.Body = string.Join("\n", bodyLines).TrimEnd('\n');
                request.ExecutableText = string.Join("\n", executableLines).TrimEnd('\n');
                request.EndLine = endLine;
            }
            request = null;
            body = false;
            bodyLines.Clear();
            executableLines.Clear();
        }
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var trimmed = line.Trim();
            int indentation = scanReferences == null && duplicateName == null && declaration == null
                ? 0 : line.IndexOf(trimmed, StringComparison.Ordinal);
            if (ODataSyntax.IsHeading(trimmed))
            {
                Finish(i);
                sectionLine = i;
                name = trimmed.Substring(3).Trim();
                continue;
            }
            if (request == null)
            {
                if (ODataSyntax.IsComment(trimmed))
                {
                    var comment = trimmed.StartsWith("#") ? trimmed.Substring(1).TrimStart() : trimmed.Substring(2).TrimStart();
                    if (ODataSyntax.IsNameDirective(comment))
                    {
                        var matchName = Regex.Match(comment, @"^@name\s+([A-Za-z_][A-Za-z0-9_]*)\s*$", RegexOptions.CultureInvariant);
                        if (!matchName.Success) document.Errors.Add($"Line {i + 1}: expected # @name identifier (letters, digits, underscores; starting with a letter or underscore).");
                        else if (requestName.Length != 0)
                        {
                            document.Errors.Add($"Line {i + 1}: only one @name is allowed per request.");
                            duplicateName?.Invoke(i, indentation + trimmed.Length - comment.Length + matchName.Groups[1].Index, matchName.Groups[1].Value);
                        }
                        else
                        {
                            requestName = matchName.Groups[1].Value;
                            declaration?.Invoke(i, indentation + trimmed.Length - comment.Length + matchName.Groups[1].Index, requestName, true);
                            if (!names.Add(requestName))
                            {
                                document.Errors.Add($"Line {i + 1}: duplicate request name '{requestName}'. Request names are case-sensitive and must be unique.");
                                duplicateName?.Invoke(i, indentation + trimmed.Length - comment.Length + matchName.Groups[1].Index, requestName);
                            }
                        }
                    }
                    continue;
                }
                if (trimmed.Length == 0) continue;
                if (trimmed.StartsWith("@"))
                {
                    var equals = trimmed.IndexOf('=');
                    var key = equals > 1 ? trimmed.Substring(1, equals - 1).Trim() : "";
                    if (!Regex.IsMatch(key, @"^[A-Za-z_][A-Za-z0-9_]*$")) document.Errors.Add($"Line {i + 1}: expected @name = value.");
                    else
                    {
                        document.Variables[key] = trimmed.Substring(equals + 1).Trim();
                        declaration?.Invoke(i, indentation + trimmed.IndexOf(key, 1, StringComparison.Ordinal), key, false);
                        scanReferences?.Invoke(i, indentation + equals + 1, trimmed.Substring(equals + 1));
                    }
                    continue;
                }
                var match = ODataSyntax.MatchRequestLine(trimmed);
                request = new ODataRequest { Line = i, StartLine = sectionLine, Name = name, RequestName = requestName };
                requestName = "";
                executableLines.Add(line);
                document.Requests.Add(request);
                if (!match.Success) request.Error = $"Line {i + 1}: expected METHOD URL. Supported methods: GET, POST, PUT, PATCH, DELETE, HEAD, OPTIONS.";
                else
                {
                    request.Method = match.Groups[1].Value;
                    request.Target = match.Groups[2].Value;
                    scanReferences?.Invoke(i, indentation + match.Groups[2].Index, request.Target);
                    if (match.Groups[3].Success && match.Groups[3].Value != "1.1")
                        request.Error = $"Line {i + 1}: only HTTP/1.1 is supported.";
                }
                continue;
            }
            if (body) { bodyLines.Add(line); executableLines.Add(line); scanReferences?.Invoke(i, 0, line); continue; }
            if (trimmed.Length == 0) { body = true; executableLines.Add(line); continue; }
            if (ODataSyntax.IsComment(trimmed)) continue;
            executableLines.Add(line);
            int colon = line.IndexOf(':');
            if (colon <= 0) request.Error = $"Line {i + 1}: expected a header, blank line before a body, or ### before another request.";
            else
            {
                request.Headers.Add(new(line.Substring(0, colon).Trim(), line.Substring(colon + 1).Trim()));
                scanReferences?.Invoke(i, colon + 1, line.Substring(colon + 1));
            }
        }
        Finish(lines.Length);
        if (requestName.Length != 0) document.Errors.Add("@name must precede a request.");
        return document;
    }

    public ODataRequest? FindRequestAtLine(int line) => Requests.FirstOrDefault(r => r.StartLine <= line && line < r.EndLine);

    public string Expand(string value) => ExpandVariables(value, new HashSet<string>(StringComparer.Ordinal), false, null);

    internal string ExpandForInspection(string value)
        => ExpandVariables(value, new HashSet<string>(StringComparer.Ordinal), true, null, evaluateDynamic: false, limit: 4096);

    internal IReadOnlyCollection<string> GetDynamicDependencies(string name, CancellationToken token)
    {
        var dependencies = new HashSet<string>(StringComparer.Ordinal);
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var pending = new Stack<string>();
        var variables = EffectiveVariables;
        pending.Push(name);
        while (pending.Count != 0)
        {
            token.ThrowIfCancellationRequested();
            string key = pending.Pop();
            if (!visited.Add(key)) continue;
            if (ODataBuiltInVariables.All.ContainsKey(key)) dependencies.Add(key);
            else if (variables.TryGetValue(key, out var value))
                foreach (Match match in Variable.Matches(value))
                {
                    token.ThrowIfCancellationRequested();
                    pending.Push(match.Groups[1].Value.Trim());
                }
        }
        return dependencies;
    }

    private string ExpandVariables(string value, HashSet<string> stack, bool allowResponses, Dictionary<string, string>? used, bool evaluateDynamic = true, int limit = int.MaxValue)
    {
        if (value.Length > limit) throw new FormatException("The variable is too large to expand for inspection.");
        var result = new StringBuilder();
        int last = 0;
        foreach (Match match in Variable.Matches(value))
        {
            result.Append(value, last, match.Index - last);
            string replacement = ExpandMatch(match);
            if (result.Length + replacement.Length > limit) throw new FormatException("The variable is too large to expand for inspection.");
            result.Append(replacement);
            last = match.Index + match.Length;
        }
        if (result.Length + value.Length - last > limit) throw new FormatException("The variable is too large to expand for inspection.");
        return result.Append(value, last, value.Length - last).ToString();

        string ExpandMatch(Match match)
        {
            string key = match.Groups[1].Value.Trim();
            if (ODataBuiltInVariables.All.TryGetValue(key, out var builtIn))
                return evaluateDynamic ? builtIn.Evaluate() : match.Value;
            if (allowResponses && key.Contains(".response")) return match.Value;
            if (!EffectiveVariables.TryGetValue(key, out var replacement)) throw new FormatException($"Undefined variable '{key}'. Define it with @name = value, provide it in the selected environment profile, or use a supported named response reference.");
            if (stack.Count >= 32 || !stack.Add(key)) throw new FormatException($"Cyclic or excessively nested variable '{key}'.");
            if (used != null) used[key] = replacement;
            try { return ExpandVariables(replacement, stack, allowResponses, used, evaluateDynamic, limit); }
            finally { stack.Remove(key); }
        }
    }

    public ODataRequest Resolve(ODataRequest source) => Resolve(source, _ => null, _ => { });

    public ODataRequest Resolve(ODataRequest source, Func<string, ODataCapturedResponse?> resolver, Action<ODataCapturedResponse> onCaptureUsed, CancellationToken cancellationToken = default)
    {
        Validate(source);
        if (resolver == null) throw new ArgumentNullException(nameof(resolver));
        if (onCaptureUsed == null) throw new ArgumentNullException(nameof(onCaptureUsed));
        string ExpandValue(string text, bool json, bool target = false) => ODataResponseReferences.Substitute(ExpandVariables(text, new HashSet<string>(StringComparer.Ordinal), true, null), json, resolver, onCaptureUsed, target, cancellationToken);
        var result = new ODataRequest { Line = source.Line, StartLine = source.StartLine, EndLine = source.EndLine, Name = source.Name, RequestName = source.RequestName, Method = source.Method, Target = ExpandValue(source.Target, false, target: true), Body = ExpandValue(source.Body, true) };
        foreach (var header in source.Headers) result.Headers.Add(new(header.Key, ExpandValue(header.Value, false)));
        if (result.Body.TrimStart().StartsWith("< ") || result.Body.TrimStart().StartsWith("> {%"))
            throw new FormatException("File includes and response scripts are not supported.");
        return result;
    }

    public string Fingerprint(ODataRequest request)
    {
        if (request.Error != null) throw new FormatException(request.Error);
        var used = new Dictionary<string, string>(StringComparer.Ordinal);
        var text = new StringBuilder();
        void Add(string value) => text.Append(value.Length.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append(':').Append(value);
        void AddSource(string value)
        {
            Add(value);
            ExpandVariables(value, new HashSet<string>(StringComparer.Ordinal), true, used, evaluateDynamic: false);
        }
        Add(request.ExecutableText);
        Add(request.Method);
        AddSource(request.Target);
        Add(request.Headers.Count.ToString(System.Globalization.CultureInfo.InvariantCulture));
        foreach (var header in request.Headers) { Add(header.Key); AddSource(header.Value); }
        AddSource(request.Body);
        foreach (var variable in used.OrderBy(pair => pair.Key, StringComparer.Ordinal)) { Add(variable.Key); Add(variable.Value); }
        using var sha = SHA256.Create();
        return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(text.ToString()))).Replace("-", "");
    }

    private void Validate(ODataRequest request)
    {
        if (Errors.Count > 0) throw new FormatException(string.Join(Environment.NewLine, Errors));
        if (request.Error != null) throw new FormatException(request.Error);
    }
}
