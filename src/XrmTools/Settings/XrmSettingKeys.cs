#nullable enable
namespace XrmTools.Settings;

public static class XrmSettingKeys
{
    public const string EnvironmentUrl = "EnvironmentUrl";
    public const string DataverseConnectionString = "DataverseConnectionString";
    public const string DataversePluginTemplateFilePath = "DataversePluginTemplateFilePath";
    public const string DataverseEntityTemplateFilePath = "DataverseEntityTemplateFilePath";
    public const string DataverseGlobalOptionSetsTemplateFilePath = "DataverseGlobalOptionSetsTemplateFilePath";
    public const string DataverseGlobalOptionSetsFilePath = "DataverseGlobalOptionSetsFilePath";
    public static string[] All = [ 
        EnvironmentUrl,  DataverseConnectionString, DataversePluginTemplateFilePath, DataverseEntityTemplateFilePath ];
}
#nullable restore