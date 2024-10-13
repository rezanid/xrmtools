#nullable enable
namespace XrmTools.Settings;
using Community.VisualStudio.Toolkit;
using System;
using System.Runtime.InteropServices;

[Guid(PackageGuids.guidSettingsProviderString)]
public interface ISettingsProvider
{
    SolutionSettings SolutionSettings { get; }
    SolutionSettings SolutionUserSettings { get; }
    ProjectSettings ProjectSettings { get; }
    ProjectSettings ProjectUserSettings { get; }
}

[ComVisible(true)]
public class SettingsProvider : ISettingsProvider
{
    // NOTE!
    // * Solution settings (.sln, .suo) are kept in dictionaries and loaded and replaced from ther package.
    // * Project settings are directly written to and read from the project (and project user) file.
    public SolutionSettings SolutionSettings { get; } = new SolutionSettings(SolutionStorageType.Solution);
    public SolutionSettings SolutionUserSettings { get; } = new SolutionSettings(SolutionStorageType.SolutionUser);
    public ProjectSettings ProjectSettings { get; } = new ProjectSettings(ProjectStorageType.ProjectFile);
    public ProjectSettings ProjectUserSettings { get; } = new ProjectSettings(ProjectStorageType.UserFile);
}
#nullable restore