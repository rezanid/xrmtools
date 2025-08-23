#nullable enable
namespace XrmTools.Services;

public sealed class RegistrationInput
{
    public string ItemFullPath { get; }
    public bool IsProject { get; }
    public string? NugetPackagePath { get; }

    public RegistrationInput(string itemFullPath, bool isProject, string? nugetPackagePath)
    {
        ItemFullPath = itemFullPath;
        IsProject = isProject;
        NugetPackagePath = nugetPackagePath;
    }
}
#nullable restore