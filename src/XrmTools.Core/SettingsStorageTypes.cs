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
    // Due to a bug in Visual Studio that makes it impossible to remove a setting from
    // project user file once it is added, we will not provide this option yet. User
    // can still manually override properties in the user file if required.
    [Browsable(false)]
    [Description("Project (User)")]
    ProjectUser
}
#nullable restore