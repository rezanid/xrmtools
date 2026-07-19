#nullable enable
namespace XrmTools.DataverseSolutions;

internal enum DataverseSolutionProjectCreationMode
{
    Empty,
    Clone
}

public sealed class PacSolutionInitRequest
{
    public string OutputDirectory { get; init; } = string.Empty;

    public string PublisherName { get; init; } = string.Empty;

    public string PublisherPrefix { get; init; } = string.Empty;
}

public sealed class PacSolutionCloneRequest
{
    public string OutputDirectory { get; init; } = string.Empty;

    public string SolutionUniqueName { get; init; } = string.Empty;

    public string? EnvironmentUrl { get; init; }

    public string? MapFilePath { get; init; }
}

internal sealed class DataverseSolutionProjectCreationRequest
{
    public DataverseSolutionProjectCreationMode Mode { get; init; }

    public string ProjectName { get; init; } = string.Empty;

    public string ParentDirectory { get; init; } = string.Empty;

    public string? PublisherName { get; init; }

    public string? PublisherPrefix { get; init; }

    public string? SolutionUniqueName { get; init; }
}
#nullable restore
