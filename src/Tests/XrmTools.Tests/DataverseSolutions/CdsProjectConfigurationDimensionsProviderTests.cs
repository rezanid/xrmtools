namespace XrmTools.Tests.DataverseSolutions;

using FluentAssertions;
using Microsoft.VisualStudio.ProjectSystem;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using Xunit;
using XrmTools.DataverseSolutions;

public sealed class CdsProjectConfigurationDimensionsProviderTests
{
    [Fact]
    public void MissingDimensions_PacDefaults_ExposeDebugReleaseAndAnyCpu()
    {
        var dimensions = GetDimensions(new Dictionary<string, string>
        {
            ["Configuration"] = "Debug",
            ["Platform"] = "AnyCPU"
        });

        dimensions.Should().HaveCount(2);
        dimensions["Configuration"].Should().Equal("Debug", "Release");
        dimensions["Platform"].Should().Equal("AnyCPU");
    }

    [Fact]
    public void MissingDimensions_ExplicitDimensions_LeaveThemToManagedCps()
    {
        var dimensions = GetDimensions(new Dictionary<string, string>
        {
            ["Configurations"] = "Debug;Release;Staging",
            ["Platforms"] = "AnyCPU;x64"
        });

        dimensions.Should().BeEmpty();
    }

    [Fact]
    public void MissingDimensions_OnlyPlatformMissing_DoNotOverrideConfigurations()
    {
        var dimensions = GetDimensions(new Dictionary<string, string>
        {
            ["Configurations"] = "Shipping;Validation",
            ["Platform"] = "ARM64"
        });

        dimensions.Should().ContainSingle();
        dimensions["Platform"].Should().Equal("ARM64");
    }

    [Fact]
    public void MissingDimensions_ConditionedCustomValues_PreserveThemInsteadOfInventingDefaults()
    {
        var dimensions = GetDimensions(new Dictionary<string, string>
        {
            ["Configuration"] = "Staging",
            ["Platform"] = "x64"
        }, new Dictionary<string, List<string>>
        {
            ["Configuration"] = new() { "Staging", "Production" },
            ["Platform"] = new() { "x64" }
        });

        dimensions["Configuration"].Should().Equal("Staging", "Production");
        dimensions["Platform"].Should().Equal("x64");
    }

    [Fact]
    public void Compose_ExportsFallbackDimensionsWithoutAdditionalImports()
    {
        using var catalog = new TypeCatalog(typeof(CdsProjectConfigurationDimensionsProvider));
        using var container = new CompositionContainer(catalog, CompositionOptions.DisableSilentRejection);

        container.GetExportedValue<IProjectConfigurationDimensionsProvider>()
            .Should().BeOfType<CdsProjectConfigurationDimensionsProvider>();
        typeof(CdsProjectConfigurationDimensionsProvider).GetCustomAttributes<ConfigurationDimensionDescriptionAttribute>()
            .Should().HaveCount(2);
    }

    [Theory]
    [InlineData("XrmToolsCdsProject;XrmToolsNonDotNetProject", true)]
    [InlineData("XrmToolsPcfProject;XrmToolsNonDotNetProject", true)]
    [InlineData("XrmToolsNonDotNetProject;.NET", false)]
    [InlineData(".NET", false)]
    [InlineData("", false)]
    public void AppliesTo_OnlyProjectsWithoutSdkDimensions(string capabilities, bool expected)
    {
        var appliesTo = typeof(CdsProjectConfigurationDimensionsProvider).GetCustomAttribute<AppliesToAttribute>();
        var capabilitySet = capabilities.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
            .ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);

        CommonProjectSystemTools.IsCapabilityMatch(appliesTo!.AppliesTo, capabilitySet).Should().Be(expected);
    }

    private static Dictionary<string, IEnumerable<string>> GetDimensions(Dictionary<string, string> properties,
        Dictionary<string, List<string>>? conditionedProperties = null)
        => CdsProjectConfigurationDimensionsProvider.GetMissingDimensions(
                name => properties.TryGetValue(name, out var value) ? value : string.Empty,
                conditionedProperties ?? new Dictionary<string, List<string>>())
            .ToDictionary(dimension => dimension.Key, dimension => dimension.Value);
}
