#nullable enable
namespace XrmTools.OData;

using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

internal sealed record ODataInspection(string Source, string Preview)
{
    public bool IsValue { get; init; } = true;
    public string? Description { get; init; }
    public Func<bool>? IsCurrent { get; init; }
}

internal static class ODataInspector
{
    internal const int PreviewLimit = 2048;
    internal const int PreviewLineLimit = 20;
    internal const string TruncationNotice = "... (truncated)";
    internal const string ResponseTruncationNotice = "... (truncated; remaining data not inspected)";

    public static Task<ODataInspection> InspectAsync(ODataDocument document, string expression, string? environment,
        Func<string, ODataCapturedResponse?> captures, Action<ODataCapturedResponse> used, CancellationToken token)
        => Task.Run(() => Inspect(document, expression, environment, captures, used, token), token);

    private static ODataInspection Inspect(ODataDocument document, string expression, string? environment,
        Func<string, ODataCapturedResponse?> captures, Action<ODataCapturedResponse> used, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (ODataBuiltInVariables.All.TryGetValue(expression, out var builtIn))
            return new("Dynamic variable " + expression, builtIn.Description + " Hover does not evaluate it.") { IsValue = false };

        bool isValue = true;
        bool declared = document.Variables.TryGetValue(expression, out var value);
        bool profile = document.EnvironmentVariables.ContainsKey(expression);
        string source = declared
            ? "Document variable " + expression + (profile ? " (overrides environment variable in " + environment + ")" : "")
            : profile ? "Environment variable " + expression + " (" + environment + ")"
            : "Named request response " + expression;
        try
        {
            if (declared || profile)
            {
                value = document.EffectiveVariables[expression];
                if (!ODataDocument.Variable.IsMatch(value)) return new(source, Bound(value));
                var dependencies = document.GetDynamicDependencies(expression, token);
                if (dependencies.Count != 0)
                {
                    string preview;
                    try { preview = document.ExpandForInspection("{{" + expression + "}}"); }
                    catch (FormatException ex)
                    {
                        preview = "Expansion unavailable: " + ex.Message + " Declaration: " + value;
                        isValue = false;
                    }
                    return new(source, Bound(preview))
                    {
                        IsValue = isValue,
                        Description = Bound("Depends on " + string.Join(", ", dependencies) + "; evaluated for each occurrence when sending, not on hover.")
                    };
                }
                string expanded = document.ExpandForInspection("{{" + expression + "}}");
                expanded = ODataDocument.Variable.Replace(expanded, match => Resolve(match.Groups[1].Value.Trim()));
                return new(source, Bound(expanded)) { IsValue = isValue };
            }
            if (!expression.Contains(".response"))
                return new("Unknown variable " + expression, "Define a document variable or provide it in the selected environment profile.") { IsValue = false };
            string responsePreview = Resolve(expression);
            return new(source, responsePreview) { IsValue = isValue };
        }
        catch (FormatException ex) { return new(source, Bound(ex.Message)) { IsValue = false }; }
        catch (System.IO.IOException) { return new(source, "The captured response body could not be read. Send the named request again.") { IsValue = false }; }
        catch (UnauthorizedAccessException) { return new(source, "The captured response body could not be read. Send the named request again.") { IsValue = false }; }

        string Resolve(string reference)
        {
            token.ThrowIfCancellationRequested();
            string name = ODataResponseReferences.ValidateSelector(reference);
            if (document.Requests.Count(request => request.RequestName == name) != 1)
                throw new FormatException("Define one uniquely named request '" + name + "' and send it first.");
            var capture = captures(name);
            if (capture == null)
                throw new FormatException("No valid captured response for '" + name + "'. Send that named request and obtain a completed 2xx response in the selected environment. Reruns, relevant edits, environment changes, and eviction can clear captures.");
            used(capture);
            return ODataResponseReferences.Preview(capture, reference, token, literal: declared || profile, unavailable: () => isValue = false);
        }
    }

    internal static string Bound(string value, string? truncationNotice = null)
    {
        int length = Math.Min(value.Length, PreviewLimit);
        int lines = 1;
        for (int index = 0; index < length; index++)
        {
            char c = value[index];
            if ((c == '\r' || c == '\n' && (index == 0 || value[index - 1] != '\r')) && ++lines > PreviewLineLimit)
            {
                length = index;
                break;
            }
        }
        bool truncated = length < value.Length || truncationNotice != null;
        if (truncated && length > 0 && char.IsHighSurrogate(value[length - 1])) length--;
        if (truncated && length > 0 && value[length - 1] == '\r') length--;
        var text = new StringBuilder(length);
        for (int index = 0; index < length; index++)
        {
            char c = value[index];
            text.Append(char.IsControl(c) && c != '\r' && c != '\n' && c != '\t' ? '?' : c);
        }
        if (truncated) text.Append(Environment.NewLine).Append(truncationNotice ?? TruncationNotice);
        return text.ToString();
    }
}
