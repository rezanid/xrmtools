#nullable enable
namespace XrmTools.Options;
using System.ComponentModel;

public enum SettingsStorageTypes
{
    [Description("Global (Visual Studio options)")]
    Options = 1,
    [Description("Solution (.sln) file")]
    Solution = 2,
    [Description("Project (.csproj) file")]
    Project = 3,
    [Description("Solution User Options (.suo) file")]
    SolutionUser = 4,
    // Due to a bug in Visual Studio that makes it impossible to remove a setting from
    // project user file once it is added, we will not provide this option yet. User
    // can still manually override properties in the user file if required.
    [Browsable(false)]
    [Description("Project (User)")]
    ProjectUser = 5
}
#nullable restore