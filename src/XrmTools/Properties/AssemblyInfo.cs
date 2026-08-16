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
    AssemblyName = @"XrmTools.Core",
    CodeBase = @"$PackageFolder$\XrmTools.Core.dll")]

[assembly: ProvideCodeBase(
    AssemblyName = @"XrmTools.Meta",
    CodeBase = @"$PackageFolder$\XrmTools.Meta.dll")]

[assembly: ProvideCodeBase(
    AssemblyName = @"XrmTools.WebApi",
    CodeBase = @"$PackageFolder$\XrmTools.WebApi.dll")]

[assembly: ProvideCodeBase(
    AssemblyName = @"XrmTools.UI.Controls",
    CodeBase = @"$PackageFolder$\XrmTools.UI.Controls.dll")]

[assembly: ProvideCodeBase(
    AssemblyName = @"Microsoft.Bcl.TimeProvider",
    CodeBase = @"$PackageFolder$\Microsoft.Bcl.TimeProvider.dll")]

[assembly: ProvideCodeBase(
    AssemblyName = @"Microsoft.Identity.Client",
    CodeBase = @"$PackageFolder$\Microsoft.Identity.Client.dll")]

[assembly: ProvideCodeBase(
    AssemblyName = @"Microsoft.Identity.Client.Extensions.Msal",
    CodeBase = @"$PackageFolder$\Microsoft.Identity.Client.Extensions.Msal.dll")]

[assembly: ProvideCodeBase(
    AssemblyName = @"Polly",
    CodeBase = @"$PackageFolder$\Polly.dll")]

[assembly: ProvideCodeBase(
    AssemblyName = @"Polly.Core",
    CodeBase = @"$PackageFolder$\Polly.Core.dll")]

[assembly: ProvideCodeBase(
    AssemblyName = @"System.Net.Http.Formatting",
    CodeBase = @"$PackageFolder$\System.Net.Http.Formatting.dll")]

// The following is important for Microsoft.Identity.Client, when authenticating
// otherwise there is a chance of FileNotFound: Assembly not found exception.
[assembly: ProvideCodeBase(
    AssemblyName = @"Microsoft.IdentityModel.Abstractions",
    CodeBase = @"$PackageFolder$\Microsoft.IdentityModel.Abstractions.dll")]
