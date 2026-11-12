#nullable enable
namespace XrmTools.DataverseSolutions;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

internal sealed class PcfCreateRequest
{
    public string ParentDirectory { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string ControlName { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string Template { get; set; } = "field";
    public string Framework { get; set; } = "none";
    public bool InstallDependencies { get; set; }
}

internal sealed class PcfPushRequest
{
    public string ProjectFilePath { get; set; } = string.Empty;
    public string EnvironmentUrl { get; set; } = string.Empty;
    public string PublisherPrefix { get; set; } = string.Empty;
    public string? SolutionUniqueName { get; set; }
    public bool Incremental { get; set; }
}

internal static class PcfArguments
{
    public static IReadOnlyList<string> Create(PcfCreateRequest request)
    {
        var location = request.ParentDirectory ?? string.Empty;
        var root = string.IsNullOrWhiteSpace(location) ? null : Path.GetPathRoot(location);
        if (root is null || root.Length == 0 || root == "\\" || root == "/" || root.EndsWith(":", StringComparison.Ordinal))
            throw new ArgumentException("Enter an absolute project location.");
        var name = request.ProjectName ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name) || name != name.Trim() || name == "." || name == ".." || name.EndsWith(".", StringComparison.Ordinal) || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            throw new ArgumentException("Enter a valid project folder name.");
        var controlName = request.ControlName ?? string.Empty;
        var controlNamespace = request.Namespace ?? string.Empty;
        if (!Regex.IsMatch(controlName, @"\A[A-Za-z][A-Za-z0-9]*\z")) throw new ArgumentException("Control name must start with a letter and contain only letters and digits.");
        if (!Regex.IsMatch(controlNamespace, @"\A[A-Za-z][A-Za-z0-9]*(\.[A-Za-z][A-Za-z0-9]*)*\z")) throw new ArgumentException("Namespace segments must start with a letter and contain only letters and digits.");
        if (request.Template != "field" && request.Template != "dataset") throw new ArgumentException("Select a field or dataset template.");
        if (request.Framework != "none" && request.Framework != "react") throw new ArgumentException("Select the none or React framework.");
        var args = new List<string> { "pcf", "init", "--namespace", controlNamespace, "--name", controlName, "--template", request.Template, "--framework", request.Framework, "--outputDirectory", Path.GetFullPath(Path.Combine(request.ParentDirectory, name)) };
        if (request.InstallDependencies) args.Add("--run-npm-install");
        return args;
    }

    public static IReadOnlyList<string> Push(PcfPushRequest request)
    {
        if (!Uri.TryCreate(request.EnvironmentUrl, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps || !string.IsNullOrEmpty(uri.UserInfo) || !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment))
            throw new ArgumentException("A valid HTTPS environment URL is required.");
        var prefix = request.PublisherPrefix?.Trim() ?? string.Empty;
        var solution = request.SolutionUniqueName?.Trim() ?? string.Empty;
        if ((prefix.Length == 0) == (solution.Length == 0))
            throw new ArgumentException("Choose either a publisher prefix or an existing solution unique name, not both.");
        var args = new List<string> { "pcf", "push", "--environment", request.EnvironmentUrl };
        if (solution.Length > 0)
        {
            args.Add("--solution-unique-name");
            args.Add(solution);
        }
        else
        {
            if (!Regex.IsMatch(prefix, @"\A[A-Za-z][A-Za-z0-9]{1,7}\z") || prefix.StartsWith("mscrm", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Publisher prefix must be 2–8 alphanumeric characters, start with a letter, and not start with mscrm.");
            args.Add("--publisher-prefix");
            args.Add(prefix);
        }
        if (request.Incremental) args.Add("--incremental");
        return args;
    }
}
#nullable restore
