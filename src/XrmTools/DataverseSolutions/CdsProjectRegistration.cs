#nullable enable
namespace XrmTools.DataverseSolutions;

internal static class CdsProjectRegistration
{
    public const string ProjectTypeGuid = "D3ACB8F3-9E41-48CD-A302-4B7A9D6A2A49";
    public const string ProjectExtension = "cdsproj";
    public const string Language = "csharp";
    public const string UniqueCapability = "XrmToolsCdsProject";
    public const string Capabilities = UniqueCapability + ";OpenProjectFile;HandlesOwnReload;ProjectConfigurationsDeclaredDimensions;DependenciesTree;UseProjectEvaluationCache";
}
#nullable restore
