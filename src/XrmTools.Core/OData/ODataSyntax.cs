#nullable enable
namespace XrmTools.OData;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

internal enum ODataSyntaxKind
{
    Heading,
    Comment,
    NameDirective,
    VariableDeclaration,
    VariableReference,
    HttpMethod,
    RequestTarget,
    HttpVersion,
    HeaderName
}

/// <summary>A nonempty, UTF-16 range in the original document (not normalized text).</summary>
internal readonly struct ODataSyntaxSpan(int start, int length, ODataSyntaxKind kind)
{
    public int Start { get; } = start;
    public int Length { get; } = length;
    public int End => Start + Length;
    public ODataSyntaxKind Kind { get; } = kind;
}

/// <summary>
/// Tolerant lexical scanning only; does not validate or execute requests. Section, header and
/// body transitions match ODataDocument, including malformed request lines and body delimiters.
/// </summary>
internal static class ODataSyntax
{
    private static readonly string[] Methods = ["GET", "POST", "PUT", "PATCH", "DELETE", "HEAD", "OPTIONS"];
    private static readonly Regex Reference = new(@"\{\{[^{}]*(?:\}\}|$)", RegexOptions.Compiled);
    private static readonly Regex Declaration = new(@"^@[A-Za-z_][A-Za-z0-9_]*", RegexOptions.Compiled);
    private static readonly Regex Directive = new(@"^@name(?![A-Za-z0-9_])", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex Version = new(@"\s+(HTTP/(?:\d+(?:\.\d*)?)?)$", RegexOptions.Compiled);
    private static readonly Regex RequestLine = new(@"^(GET|POST|PUT|PATCH|DELETE|HEAD|OPTIONS)\s+(\S.*?)(?:\s+HTTP/(\d+(?:\.\d+)?))?$", RegexOptions.Compiled);

    public static bool IsHeading(string trimmed) => trimmed.StartsWith("###", StringComparison.Ordinal);
    public static bool IsComment(string trimmed) => trimmed.StartsWith("#", StringComparison.Ordinal) || trimmed.StartsWith("//", StringComparison.Ordinal);
    public static bool IsNameDirective(string comment) => Directive.IsMatch(comment);
    public static Match MatchRequestLine(string trimmed) => RequestLine.Match(trimmed);

    /// <summary>
    /// Returns ordered, nonoverlapping spans. Incomplete declarations, method prefixes,
    /// versions, header names and placeholders are retained for editing. Placeholders take
    /// precedence over enclosing syntax, even in bodies, comments and headings.
    /// </summary>
    public static IReadOnlyList<ODataSyntaxSpan> Scan(string text)
    {
        if (text == null) throw new ArgumentNullException(nameof(text));
        var result = new List<ODataSyntaxSpan>();
        bool request = false, body = false;
        for (int offset = 0; offset < text.Length;)
        {
            int end = offset;
            while (end < text.Length && text[end] != '\r' && text[end] != '\n') end++;
            var line = text.Substring(offset, end - offset);
            int start = 0, length = line.Length;
            while (start < length && char.IsWhiteSpace(line[start])) start++;
            while (length > start && char.IsWhiteSpace(line[length - 1])) length--;
            var trimmed = line.Substring(start, length - start);
            var tokens = new List<ODataSyntaxSpan>();
            void Add(int at, int count, ODataSyntaxKind kind)
            {
                if (count > 0) tokens.Add(new(offset + at, count, kind));
            }

            if (IsHeading(trimmed))
            {
                Add(start, trimmed.Length, ODataSyntaxKind.Heading);
                request = body = false;
            }
            else if (body) { }
            else if (trimmed.Length == 0)
            {
                if (request) body = true;
            }
            else if (IsComment(trimmed))
            {
                int markerLength = trimmed[0] == '#' ? 1 : 2;
                int directiveStart = start + markerLength;
                while (directiveStart < length && char.IsWhiteSpace(line[directiveStart])) directiveStart++;
                var directive = Directive.Match(line.Substring(directiveStart, length - directiveStart));
                if (!request && directive.Success)
                {
                    Add(start, directiveStart - start, ODataSyntaxKind.Comment);
                    Add(directiveStart, directive.Length, ODataSyntaxKind.NameDirective);
                    Add(directiveStart + directive.Length, length - directiveStart - directive.Length, ODataSyntaxKind.Comment);
                }
                else Add(start, trimmed.Length, ODataSyntaxKind.Comment);
            }
            else if (!request && trimmed[0] == '@')
            {
                var declaration = Declaration.Match(trimmed);
                Add(start, declaration.Success ? declaration.Length : 1, ODataSyntaxKind.VariableDeclaration);
            }
            else if (!request)
            {
                // The parser enters headers even when the first request line is invalid.
                request = true;
                int methodEnd = start;
                while (methodEnd < length && !char.IsWhiteSpace(line[methodEnd])) methodEnd++;
                var method = line.Substring(start, methodEnd - start);
                if (Array.Exists(Methods, supported => supported.StartsWith(method, StringComparison.Ordinal)))
                {
                    Add(start, method.Length, ODataSyntaxKind.HttpMethod);
                    int targetStart = methodEnd;
                    while (targetStart < length && char.IsWhiteSpace(line[targetStart])) targetStart++;
                    int targetEnd = length;
                    var version = Version.Match(line.Substring(methodEnd, length - methodEnd));
                    if (version.Success)
                    {
                        targetEnd = methodEnd + version.Index;
                        Add(methodEnd + version.Groups[1].Index, version.Groups[1].Length, ODataSyntaxKind.HttpVersion);
                    }
                    while (targetEnd > targetStart && char.IsWhiteSpace(line[targetEnd - 1])) targetEnd--;
                    Add(targetStart, targetEnd - targetStart, ODataSyntaxKind.RequestTarget);
                }
            }
            else
            {
                int colon = line.IndexOf(':');
                if (colon > 0)
                {
                    int headerEnd = colon;
                    while (headerEnd > start && char.IsWhiteSpace(line[headerEnd - 1])) headerEnd--;
                    Add(start, headerEnd - start, ODataSyntaxKind.HeaderName);
                }
                else if (colon < 0 && IsHeaderName(trimmed))
                    Add(start, trimmed.Length, ODataSyntaxKind.HeaderName);
            }

            var references = Reference.Matches(line);
            foreach (var token in tokens)
            {
                int cursor = token.Start;
                foreach (Match match in references)
                {
                    int referenceStart = offset + match.Index, referenceEnd = referenceStart + match.Length;
                    if (referenceEnd <= cursor) continue;
                    if (referenceStart >= token.End) break;
                    if (referenceStart > cursor) result.Add(new(cursor, referenceStart - cursor, token.Kind));
                    cursor = Math.Min(token.End, referenceEnd);
                }
                if (cursor < token.End) result.Add(new(cursor, token.End - cursor, token.Kind));
            }
            foreach (Match match in references)
                result.Add(new(offset + match.Index, match.Length, ODataSyntaxKind.VariableReference));

            offset = end;
            if (offset < text.Length && text[offset] == '\r') offset++;
            if (offset < text.Length && text[offset] == '\n') offset++;
        }
        result.Sort((left, right) => left.Start.CompareTo(right.Start));
        return result.AsReadOnly();
    }

    private static bool IsHeaderName(string value)
    {
        foreach (char c in value)
            if (!(c >= 'A' && c <= 'Z') && !(c >= 'a' && c <= 'z') && !(c >= '0' && c <= '9') && "!#$%&'*+-.^_`|~".IndexOf(c) < 0)
                return false;
        return value.Length > 0;
    }
}
