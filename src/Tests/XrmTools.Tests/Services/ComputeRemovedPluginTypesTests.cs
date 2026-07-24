namespace XrmTools.Tests.Services;

using FluentAssertions;
using System.Collections.Generic;
using XrmTools.Services;
using XrmTools.WebApi.Entities;
using Xunit;

public class ComputeRemovedPluginTypesTests
{
    [Fact]
    public void Returns_Empty_When_Assembly_Set_Is_Null()
    {
        var existing = new[]
        {
            Plugin("Contoso.Plugins.FooPlugin"),
            Plugin("Contoso.Plugins.BarPlugin"),
        };

        var removed = PluginRegistrationService.ComputeRemovedPluginTypes(existing, null);

        removed.Should().BeEmpty();
    }

    [Fact]
    public void Does_Not_Remove_Types_Still_Present_In_Assembly_Even_When_Unannotated()
    {
        var existing = new[]
        {
            Plugin("Contoso.Plugins.FooPlugin"),
            Plugin("Contoso.Plugins.BarPlugin"),
        };

        // Both types are still compiled into the assembly (annotated or not).
        var assemblyTypes = new HashSet<string>
        {
            "Contoso.Plugins.FooPlugin",
            "Contoso.Plugins.BarPlugin",
        };

        var removed = PluginRegistrationService.ComputeRemovedPluginTypes(existing, assemblyTypes);

        removed.Should().BeEmpty();
    }

    [Fact]
    public void Removes_Only_Types_Absent_From_Assembly()
    {
        var foo = Plugin("Contoso.Plugins.FooPlugin");
        var renamed = Plugin("Contoso.Plugins.OldNamePlugin");
        var existing = new[] { foo, renamed };

        // FooPlugin still exists; OldNamePlugin was renamed/removed in code.
        var assemblyTypes = new HashSet<string>
        {
            "Contoso.Plugins.FooPlugin",
            "Contoso.Plugins.NewNamePlugin",
        };

        var removed = PluginRegistrationService.ComputeRemovedPluginTypes(existing, assemblyTypes);

        removed.Should().ContainSingle().Which.Should().BeSameAs(renamed);
    }

    [Fact]
    public void Ignores_Existing_Types_With_Missing_Name_Or_TypeName()
    {
        var noName = new PluginType { Name = null, TypeName = "Contoso.Plugins.NoName" };
        var noTypeName = new PluginType { Name = "NoTypeName", TypeName = null };
        var existing = new[] { noName, noTypeName };

        var assemblyTypes = new HashSet<string>();

        var removed = PluginRegistrationService.ComputeRemovedPluginTypes(existing, assemblyTypes);

        removed.Should().BeEmpty();
    }

    private static PluginType Plugin(string typeName)
        => new() { Name = typeName, TypeName = typeName };
}
