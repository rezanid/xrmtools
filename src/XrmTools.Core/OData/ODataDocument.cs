#nullable enable
namespace XrmTools.OData;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

internal sealed class ODataRequest
{
    public int Line { get; set; }
    public int StartLine { get; set; }
    public int EndLine { get; set; }
    public string Name { get; set; } = "";
    public string Method { get; set; } = "GET";
    public string Target { get; set; } = "";
    public string Body { get; set; } = "";
    public List<KeyValuePair<string, string>> Headers { get; } = [];
    public string? Error { get; set; }
}

/// <summary>A deliberately bounded HTTP-like format. Parsing never authenticates or executes requests.</summary>
internal sealed class ODataDocument
{
    private static readonly Regex RequestLine = new(@"^(GET|POST|PUT|PATCH|DELETE|HEAD|OPTIONS)\s+(\S+)(?:\s+HTTP/(1\.1|2|3))?$", RegexOptions.Compiled);
    private static readonly Regex Variable = new(@"\{\{\s*([^{}]+?)\s*\}\}", RegexOptions.Compiled);
    public List<ODataRequest> Requests { get; } = [];
    public Dictionary<string, string> Variables { get; } = new(StringComparer.Ordinal);
    public List<string> Errors { get; } = [];

    public static ODataDocument Parse(string text)
    {
        var document = new ODataDocument();
        var lines = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        ODataRequest? request = null;
        bool body = false;
        var bodyLines = new List<string>();
        string name = "";
        int sectionLine = 0;
        void Finish(int endLine)
        {
            if (request != null)
            {
                request.Body = string.Join("\n", bodyLines).TrimEnd('\n');
                request.EndLine = endLine;
            }
            request = null;
            body = false;
            bodyLines.Clear();
        }
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var trimmed = line.Trim();
            if (trimmed.StartsWith("###", StringComparison.Ordinal))
            {
                Finish(i);
                sectionLine = i;
                name = trimmed.Substring(3).Trim();
                continue;
            }
            if (request == null)
            {
                if (trimmed.Length == 0 || trimmed.StartsWith("#") || trimmed.StartsWith("//")) continue;
                if (trimmed.StartsWith("@"))
                {
                    var equals = trimmed.IndexOf('=');
                    var key = equals > 1 ? trimmed.Substring(1, equals - 1).Trim() : "";
                    if (!Regex.IsMatch(key, @"^[A-Za-z_][A-Za-z0-9_]*$")) document.Errors.Add($"Line {i + 1}: expected @name = value.");
                    else document.Variables[key] = trimmed.Substring(equals + 1).Trim();
                    continue;
                }
                var match = RequestLine.Match(trimmed);
                request = new ODataRequest { Line = i, StartLine = sectionLine, Name = name };
                document.Requests.Add(request);
                if (!match.Success) request.Error = $"Line {i + 1}: expected METHOD URL. Supported methods: GET, POST, PUT, PATCH, DELETE, HEAD, OPTIONS.";
                else
                {
                    request.Method = match.Groups[1].Value;
                    request.Target = match.Groups[2].Value;
                    if (match.Groups[3].Success && match.Groups[3].Value != "1.1")
                        request.Error = $"Line {i + 1}: only HTTP/1.1 is supported.";
                }
                continue;
            }
            if (body) { bodyLines.Add(line); continue; }
            if (trimmed.Length == 0) { body = true; continue; }
            if (trimmed.StartsWith("#") || trimmed.StartsWith("//")) continue;
            int colon = line.IndexOf(':');
            if (colon <= 0) request.Error = $"Line {i + 1}: expected a header, blank line before a body, or ### before another request.";
            else request.Headers.Add(new(line.Substring(0, colon).Trim(), line.Substring(colon + 1).Trim()));
        }
        Finish(lines.Length);
        return document;
    }

    public ODataRequest? FindRequestAtLine(int line) => Requests.FirstOrDefault(r => r.StartLine <= line && line < r.EndLine);

    public string Expand(string value) => Expand(value, new HashSet<string>(StringComparer.Ordinal));
    private string Expand(string value, HashSet<string> stack)
        => Variable.Replace(value, match =>
        {
            string key = match.Groups[1].Value.Trim();
            if (!Variables.TryGetValue(key, out var replacement)) throw new FormatException($"Undefined variable '{key}'. Only @name variables are supported.");
            if (stack.Count >= 32 || !stack.Add(key)) throw new FormatException($"Cyclic or excessively nested variable '{key}'.");
            try { return Expand(replacement, stack); }
            finally { stack.Remove(key); }
        });

    public ODataRequest Resolve(ODataRequest source)
    {
        if (Errors.Count > 0) throw new FormatException(string.Join(Environment.NewLine, Errors));
        if (source.Error != null) throw new FormatException(source.Error);
        var result = new ODataRequest { Line = source.Line, Name = source.Name, Method = source.Method, Target = Expand(source.Target), Body = Expand(source.Body) };
        foreach (var header in source.Headers) result.Headers.Add(new(header.Key, Expand(header.Value)));
        if (result.Target.Any(char.IsWhiteSpace)) throw new FormatException("URL variables must be URL-encoded; whitespace is not allowed in the request URL.");
        if (result.Body.TrimStart().StartsWith("< ") || result.Body.TrimStart().StartsWith("> {%"))
            throw new FormatException("File includes and response scripts are not supported.");
        return result;
    }
}
