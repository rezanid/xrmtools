using Microsoft.VisualStudio.ProjectSystem.VS;
using Microsoft.VisualStudio.Shell;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using XrmTools.DataverseSolutions;
using XrmTools;

[assembly: AssemblyTitle(Vsix.Name)]
[assembly: AssemblyDescription(Vsix.Description)]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany(Vsix.Author)]
[assembly: AssemblyProduct(Vsix.Name)]
[assembly: AssemblyCopyright("")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: NeutralResourcesLanguage(Vsix.Language)]
[assembly: ComVisible(false)]
[assembly: AssemblyVersion(Vsix.Version)]
[assembly: AssemblyFileVersion(Vsix.Version)]
[assembly: InternalsVisibleTo("XrmTools.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)]
[assembly: ProjectTypeRegistration(
    projectTypeGuid: CdsProjectRegistration.ProjectTypeGuid,
    displayName: "Dataverse Solution Project",
    displayProjectFileExtensions: "Dataverse Solution Project Files (*.cdsproj);*.cdsproj",
    defaultProjectExtension: CdsProjectRegistration.ProjectExtension,
    language: CdsProjectRegistration.Language,
    resourcePackageGuid: PackageGuids.XrmToolsPackageIdString,
    PossibleProjectExtensions = CdsProjectRegistration.ProjectExtension,
    Capabilities = CdsProjectRegistration.Capabilities)]

[assembly: ProvideCodeBase(
    AssemblyName = @"XrmTools.WebResourceProjectTemplate",
    CodeBase = @"$PackageFolder$\ProjectTemplates\XrmTools.WebResourceProjectTemplate.dll")]
