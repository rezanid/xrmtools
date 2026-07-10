#nullable enable
namespace XrmTools.DataverseSolutions;

public sealed class CdsProjectInfo
{
    public string ProjectFilePath { get; init; } = string.Empty;

    public string ProjectDirectory { get; init; } = string.Empty;

    public string ProjectName { get; init; } = string.Empty;

    public string ConfigurationName { get; init; } = string.Empty;

    public string? SolutionPackageMapFilePath { get; init; }

    public string SolutionRootPath { get; init; } = string.Empty;

    public string SolutionPackageZipFilePath { get; init; } = string.Empty;
}
#nullable restore
