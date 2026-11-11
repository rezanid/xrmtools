namespace XrmTools.Tests.DataverseSolutions;

using FluentAssertions;
using Microsoft.VisualStudio.ProjectSystem;
using System;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using Xunit;
using XrmTools.DataverseSolutions;

public sealed class CdsPackageReferenceCapabilitiesProviderTests
{
    [Theory]
    [InlineData("PackageReference", false, true)]
    [InlineData("packagereference", false, true)]
    [InlineData("PackageReference", true, false)]
    [InlineData("PackagesConfig", false, false)]
    [InlineData("PackagesConfig", true, false)]
    [InlineData("", false, false)]
    [InlineData("", true, false)]
    public void GetCapabilities_OnlyOptsExplicitPackageReferenceProjectsWithoutPackagesConfigIntoNuGet(
        string style, bool packagesConfig, bool expected)
    {
        var directory = Path.Combine(Path.GetTempPath(), "xrmtools-cds-nuget-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var projectPath = Path.Combine(directory, "solution.cdsproj");
        var projectXml = "<Project><Import Project=\"restore.props\" /><ItemGroup>"
            + "<PackageReference Include=\"Microsoft.PowerApps.MSBuild.Solution\" Version=\"1.*\" />"
            + "</ItemGroup></Project>";
        File.WriteAllText(projectPath, projectXml);
        if (packagesConfig) File.WriteAllText(Path.Combine(directory, "packages.config"), "<packages />");
        try
        {
            var capabilities = CdsPackageReferenceCapabilitiesProvider.GetCapabilities(style, projectPath);

            capabilities.Contains("PackageReferences").Should().Be(expected);
            capabilities.Count.Should().Be(expected ? 1 : 0);
            File.ReadAllText(projectPath).Should().Be(projectXml);
            File.Exists(Path.Combine(directory, "packages.config")).Should().Be(packagesConfig);
            if (packagesConfig) File.ReadAllText(Path.Combine(directory, "packages.config")).Should().Be("<packages />");
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void Export_OnlySuppliesConfiguredCdsProjectCapabilities()
    {
        var type = typeof(CdsPackageReferenceCapabilitiesProvider);
        var export = type.GetCustomAttribute<ExportAttribute>();
        export!.ContractType.Should().Be(typeof(IProjectCapabilitiesProvider));
        export.ContractName.Should().Be(ExportContractNames.Scopes.ConfiguredProject);
        type.GetCustomAttribute<SupportsFileExtensionAttribute>()!.SupportedExtension.Should().Be(".cdsproj");
        type.Should().BeDerivedFrom<ConfiguredProjectCapabilitiesProviderBase>();
    }

    [Theory]
    [InlineData("XrmToolsCdsProject", true)]
    [InlineData("XrmToolsCdsProject;.NET", false)]
    [InlineData("XrmToolsPcfProject", false)]
    [InlineData(".NET", false)]
    [InlineData("", false)]
    public void AppliesTo_OnlyNativeCdsProjects(string capabilities, bool expected)
    {
        var appliesTo = typeof(CdsPackageReferenceCapabilitiesProvider).GetCustomAttribute<AppliesToAttribute>();
        var capabilitySet = capabilities.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
            .ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);

        CommonProjectSystemTools.IsCapabilityMatch(appliesTo!.AppliesTo, capabilitySet).Should().Be(expected);
    }
}
