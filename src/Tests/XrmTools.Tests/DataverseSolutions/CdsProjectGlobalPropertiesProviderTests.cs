namespace XrmTools.Tests.DataverseSolutions;

using FluentAssertions;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Build;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Globalization;
using System.IO;
using System.Reflection;
using Xunit;
using XrmTools.DataverseSolutions;

public sealed class CdsProjectGlobalPropertiesProviderTests
{
    [Fact]
    public void Compose_WithUnconfiguredProject_ResolvesTheGlobalPropertiesProvider()
    {
        using var context = Microsoft.VisualStudio.Threading.JoinableTaskContext.CreateNoOpContext();
        var project = new Mock<UnconfiguredProject> { DefaultValue = DefaultValue.Mock };
        project.Setup(value => value.Services.ProjectService.Services.ThreadingPolicy.JoinableTaskContext)
            .Returns(new Microsoft.VisualStudio.Threading.JoinableTaskContextNode(context));
        using var catalog = new TypeCatalog(typeof(CdsProjectGlobalPropertiesProvider));
        using var container = new CompositionContainer(catalog, CompositionOptions.DisableSilentRejection);
        container.ComposeExportedValue(project.Object);

        container.GetExportedValue<IProjectGlobalPropertiesProvider>().Should().BeOfType<CdsProjectGlobalPropertiesProvider>();
    }

    [Theory]
    [InlineData("en-US", "en-US", false)]
    [InlineData("fr-FR", "fr-FR", false)]
    [InlineData("fr-FR", "en-US", false)]
    [InlineData("", "en-US", false)]
    [InlineData("en-US", "en-US", true)]
    public void CreateGlobalProperties_UsesInstalledSchemasAndOnlyCommonCpsOptIns(string culture, string installedLanguage, bool architectureFolder)
    {
        var directory = Path.Combine(Path.GetTempPath(), "xrmtools-cds-schemas-" + Guid.NewGuid().ToString("N"));
        var resources = Path.Combine(directory, installedLanguage);
        Directory.CreateDirectory(resources);
        File.WriteAllText(Path.Combine(resources, "ProjectItemsSchema.xaml"), "<ProjectSchemaDefinitions />");
        try
        {
            var msbuildDirectory = architectureFolder ? Path.Combine(directory, "arm64") : directory;
            var properties = CdsProjectGlobalPropertiesProvider.CreateGlobalProperties(msbuildDirectory, CultureInfo.GetCultureInfo(culture));

            properties.Should().BeEquivalentTo(new Dictionary<string, string>
            {
                ["DefineCommonItemSchemas"] = "true",
                ["DefineCommonCapabilities"] = "true",
                ["CommonXamlResourcesDirectory"] = resources + Path.DirectorySeparatorChar
            });
            properties.ContainsKey("definecommonitemschemas").Should().BeTrue();
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void CreateGlobalProperties_MissingSchemas_ReportsTheMissingInstallationResource()
    {
        var directory = Path.Combine(Path.GetTempPath(), "xrmtools-missing-schemas-" + Guid.NewGuid().ToString("N"));

        Action create = () => CdsProjectGlobalPropertiesProvider.CreateGlobalProperties(directory, CultureInfo.GetCultureInfo("en-US"));

        create.Should().Throw<FileNotFoundException>()
            .Which.FileName.Should().Be(Path.Combine(directory, "en-US", "ProjectItemsSchema.xaml"));
    }

    [Theory]
    [InlineData("XrmToolsCdsProject;XrmToolsNonDotNetProject", true)]
    [InlineData("XrmToolsPcfProject;XrmToolsNonDotNetProject", true)]
    [InlineData("XrmToolsNonDotNetProject;.NET", false)]
    [InlineData(".NET", false)]
    [InlineData("", false)]
    public void AppliesTo_OnlyCdsProjectsWithoutSdkIntegration(string capabilities, bool expected)
    {
        var appliesTo = typeof(CdsProjectGlobalPropertiesProvider).GetCustomAttribute<AppliesToAttribute>();
        var capabilitySet = capabilities.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
            .ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);

        CommonProjectSystemTools.IsCapabilityMatch(appliesTo!.AppliesTo, capabilitySet).Should().Be(expected);
    }
}
