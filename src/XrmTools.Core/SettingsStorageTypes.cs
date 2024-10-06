#nullable enable
namespace XrmTools.Options;
using System.ComponentModel;

public enum SettingsStorageTypes
{
    [Description("Visual Studio Options")]
    Options,
    Solution,
    Project,
    [Description("Solution (User)")]
    SolutionUser,
    [Description("Project (User)")]
    ProjectUser
}
#nullable restore