namespace XrmTools.Tests.PluginTrace;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.PluginTrace;
using Xunit;

public class TraceDefinitionNavigatorTests
{
    [Fact]
    public async Task AssemblyQualifiedNameFindsTheCorrectSourceAndIdentifierPosition()
    {
        using var workspace = new AdhocWorkspace();
        var solution = AddProject(workspace.CurrentSolution, "Other", "namespace Contoso { public class Plugin {} }");
        solution = AddProject(solution, "Plugins", "namespace Contoso\n{\n    public class Plugin {}\n}");
        var targets = await TraceDefinitionNavigator.FindAsync(solution, "Contoso.Plugin, Plugins, Version=1.0.0.0, Culture=neutral", TestContext.Current.CancellationToken);
        var target = Assert.Single(targets);
        Assert.Equal(@"C:\Plugins.cs", target.FilePath);
        Assert.Equal(2, target.Line);
        Assert.Equal(17, target.Column);
    }

    [Fact]
    public async Task DoesNotFallBackToAnUnrelatedTypeWithTheSameSimpleName()
    {
        using var workspace = new AdhocWorkspace();
        var solution = AddProject(workspace.CurrentSolution, "Plugins", "namespace Other { public class Plugin {} }");
        Assert.Empty(await TraceDefinitionNavigator.FindAsync(solution, "Contoso.Plugin", TestContext.Current.CancellationToken));
        Assert.Empty(await TraceDefinitionNavigator.FindAsync(solution, "Other.Plugin, AbsentAssembly", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ReportsMultipleProjectsInsteadOfArbitrarilyChoosingOne()
    {
        using var workspace = new AdhocWorkspace();
        var solution = AddProject(workspace.CurrentSolution, "First", "namespace Contoso { public class Plugin {} }");
        solution = AddProject(solution, "Second", "namespace Contoso { public class Plugin {} }");
        Assert.Equal(2, (await TraceDefinitionNavigator.FindAsync(solution, "Contoso.Plugin", TestContext.Current.CancellationToken)).Count);
    }

    [Fact]
    public async Task ResolvesNestedMetadataNamesAndPartialDeclarations()
    {
        using var workspace = new AdhocWorkspace();
        var solution = AddProject(workspace.CurrentSolution, "Plugins", "namespace Contoso { public class Outer { public partial class Plugin {} public partial class Plugin {} } }");
        Assert.Single(await TraceDefinitionNavigator.FindAsync(solution, "Contoso.Outer+Plugin, Plugins", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task CancelledSearchDoesNotResolveASourceTarget()
    {
        using var workspace = new AdhocWorkspace();
        var solution = AddProject(workspace.CurrentSolution, "Plugins", "class Plugin {}");
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => TraceDefinitionNavigator.FindAsync(solution, "Plugin", cancellation.Token));
    }

    private static Solution AddProject(Solution solution, string assembly, string source)
    {
        var projectId = ProjectId.CreateNewId();
        return solution.AddProject(projectId, assembly, assembly, LanguageNames.CSharp)
            .AddMetadataReference(projectId, MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .AddDocument(DocumentId.CreateNewId(projectId), assembly + ".cs", SourceText.From(source), filePath: @"C:\" + assembly + ".cs");
    }
}
