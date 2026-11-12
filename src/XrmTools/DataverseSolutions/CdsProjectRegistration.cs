#nullable enable
namespace XrmTools.DataverseSolutions;

internal static class CdsProjectRegistration
{
    public const string ProjectTypeGuid = "D3ACB8F3-9E41-48CD-A302-4B7A9D6A2A49";
    public const string ProjectExtension = "cdsproj";
    public const string Language = "csharp";
    public const string UniqueCapability = "XrmToolsCdsProject";
    public const string Capabilities = UniqueCapability + ";" + CpsProjectCapabilities.NonDotNetProject + ";" + CpsProjectCapabilities.Standard;
}

internal static class PcfProjectRegistration
{
    public const string ProjectTypeGuid = "2F8B96DF-A546-4F1E-B20B-30CFCD8043F8";
    public const string ProjectExtension = "pcfproj";
    public const string Language = "typescript";
    public const string UniqueCapability = "XrmToolsPcfProject";
    public const string Capabilities = UniqueCapability + ";" + CpsProjectCapabilities.NonDotNetProject + ";" + CpsProjectCapabilities.Standard;
}

internal static class CpsProjectCapabilities
{
    public const string NonDotNetProject = "XrmToolsNonDotNetProject";
    public const string Standard = "OpenProjectFile;HandlesOwnReload;ProjectConfigurationsDeclaredDimensions;DependenciesTree;UseProjectEvaluationCache;UseFileGlobs";
}
#nullable restore
