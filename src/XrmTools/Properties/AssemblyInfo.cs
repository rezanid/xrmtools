using Microsoft.VisualStudio.Shell;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using XrmTools;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle(Vsix.Name)]
[assembly: AssemblyDescription(Vsix.Description)]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany(Vsix.Author)]
[assembly: AssemblyProduct(Vsix.Name)]
[assembly: AssemblyCopyright("")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: NeutralResourcesLanguage(Vsix.Language)]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion(Vsix.Version)]
[assembly: AssemblyFileVersion(Vsix.Version)]
[assembly: InternalsVisibleTo("XrmTools.Tests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100fd44efb8aea3b4e708cc946d2fe54a405251a85b7f2bc21540c67ec59f62246a3b72834e1d633db6938ea37403501a16eb1b41fd19ef4f7cb909fcc763bb486defb132db7851e231837e70e6623c0daf66169e36df920ca59cbedc6085d1bbdd5b1572f07046e1a25be4ef6dfc80c6f2992200f9a83fb77da288719b63342bbb")]

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
    AssemblyName = @"Microsoft.Xrm.Sdk",
    CodeBase = @"$PackageFolder$\Microsoft.Xrm.Sdk.dll")]