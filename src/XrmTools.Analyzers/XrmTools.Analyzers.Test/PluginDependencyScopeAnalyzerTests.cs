namespace XrmTools.Analyzers.Test;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using System.Threading.Tasks;
using Xunit;
using VerifyCS = CSharpAnalyzerVerifier<XrmTools.Analyzers.PluginDependencyScopeAnalyzer>;

public class PluginDependencyScopeAnalyzerTests
{
    private const string Preamble = @"
namespace Microsoft.Xrm.Sdk
{
    public interface IPlugin { void Execute(System.IServiceProvider serviceProvider); }
}

namespace XrmTools.Meta.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class DependencyAttribute : System.Attribute { }
}

public abstract class PluginBase : Microsoft.Xrm.Sdk.IPlugin
{
    public abstract void Execute(System.IServiceProvider serviceProvider);

    protected static T Require<T>() => DependencyScope<PluginBase>.Current.Require<T>();
    protected static T Require<T>(string name) => DependencyScope<PluginBase>.Current.Require<T>(name);
}

public sealed class DependencyScope<T> : System.IDisposable
{
    public static DependencyScope<T> Current => new();
    public U Require<U>() => default!;
    public U Require<U>(string name) => default!;
    public void Dispose() { }
}
";

    [Fact]
    public async Task DependencyReadOutsideUsing_ReportsXrmTools005()
    {
        var test = Preamble + @"
public sealed class MyPlugin : PluginBase
{
    [XrmTools.Meta.Attributes.Dependency]
    public string Dep => Require<string>();

    protected System.IDisposable CreateScope(System.IServiceProvider serviceProvider) => null;

    public override void Execute(System.IServiceProvider serviceProvider)
    {
        _ = {|#0:Dep|};
    }
}
";

        var expected = new DiagnosticResult(PluginDependencyScopeAnalyzer.DependencyOutsideScopeId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Dep");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task DependencyReadInsideUsing_IsAllowed()
    {
        var test = Preamble + @"
public sealed class MyPlugin : PluginBase
{
    [XrmTools.Meta.Attributes.Dependency]
    public string Dep => Require<string>();

    protected System.IDisposable CreateScope(System.IServiceProvider serviceProvider) => null;

    public override void Execute(System.IServiceProvider serviceProvider)
    {
        using (CreateScope(serviceProvider))
        {
            _ = Dep;
        }
    }
}
";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task UsingCallsHelper_HelperReadsDependency_IsAllowed()
    {
        var test = Preamble + @"
public sealed class MyPlugin : PluginBase
{
    [XrmTools.Meta.Attributes.Dependency]
    public string Dep => Require<string>();

    protected System.IDisposable CreateScope(System.IServiceProvider serviceProvider) => null;

    public override void Execute(System.IServiceProvider serviceProvider)
    {
        using (CreateScope(serviceProvider))
        {
            DoWork();
        }
    }

    private void DoWork()
    {
        _ = Dep;
    }
}
";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task HelperCalledInsideAndOutsideUsing_Strictness_ReportsXrmTools005()
    {
        var test = Preamble + @"
public sealed class MyPlugin : PluginBase
{
    [XrmTools.Meta.Attributes.Dependency]
    public string Dep => Require<string>();

    protected System.IDisposable CreateScope(System.IServiceProvider serviceProvider) => null;

    public override void Execute(System.IServiceProvider serviceProvider)
    {
        DoWork();

        using (CreateScope(serviceProvider))
        {
            DoWork();
        }
    }

    private void DoWork()
    {
        _ = {|#0:Dep|};
    }
}
";

        var expected = new DiagnosticResult(PluginDependencyScopeAnalyzer.DependencyOutsideScopeId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Dep");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task ScopeCreatedOnOneBranch_Strictness_ReportsXrmTools005()
    {
        var test = Preamble + @"
public sealed class MyPlugin : PluginBase
{
    [XrmTools.Meta.Attributes.Dependency]
    public string Dep => Require<string>();

    protected System.IDisposable CreateScope(System.IServiceProvider serviceProvider) => null;

    public override void Execute(System.IServiceProvider serviceProvider)
    {
        if (serviceProvider != null)
        {
            using (CreateScope(serviceProvider))
            {
            }
        }

        _ = {|#0:Dep|};
    }
}
";

        var expected = new DiagnosticResult(PluginDependencyScopeAnalyzer.DependencyOutsideScopeId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Dep");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task DependencyGetterWithExtraLogic_AndRequireWithParameter_IsDetected()
    {
        var test = Preamble + @"
public sealed class MyPlugin : PluginBase
{
    public string Dep
    {
        get
        {
            var name = ""dep"";
            return Require<string>(name);
        }
    }

    protected System.IDisposable CreateScope(System.IServiceProvider serviceProvider) => null;

    public override void Execute(System.IServiceProvider serviceProvider)
    {
        _ = {|#0:Dep|};
    }
}
";

        var expected = new DiagnosticResult(XrmTools.Analyzers.PluginDependencyScopeAnalyzer.DependencyOutsideScopeId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Dep");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task DependencyGetterWithExtraLogic_AndRequireWithParameter_ReadInsideUsing_IsAllowed()
    {
        var test = Preamble + @"
public sealed class MyPlugin : PluginBase
{
    public string Dep
    {
        get
        {
            var name = ""dep"";
            return Require<string>(name);
        }
    }

    protected System.IDisposable CreateScope(System.IServiceProvider serviceProvider) => null;

    public override void Execute(System.IServiceProvider serviceProvider)
    {
        using (CreateScope(serviceProvider))
        {
            _ = Dep;
        }
    }
}
";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task DependencyMethodCallOutsideUsing_ReportsXrmTools005()
    {
        var test = Preamble + @"
public sealed class MyPlugin : PluginBase
{
    [XrmTools.Meta.Attributes.Dependency]
    public System.Collections.Generic.List<string> Deps => Require<System.Collections.Generic.List<string>>();

    protected System.IDisposable CreateScope(System.IServiceProvider serviceProvider) => null;

    public override void Execute(System.IServiceProvider serviceProvider)
    {
        {|#0:Deps|}.Add(""value"");
    }
}
";

        var expected = new DiagnosticResult(PluginDependencyScopeAnalyzer.DependencyOutsideScopeId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Deps");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task DependencyPropertyAccessOutsideUsing_ReportsXrmTools005()
    {
        var test = Preamble + @"
namespace System { public class Uri { public string Host { get; } } }

public sealed class MyPlugin : PluginBase
{
    [XrmTools.Meta.Attributes.Dependency]
    public System.Uri ServiceUri => Require<System.Uri>();

    protected System.IDisposable CreateScope(System.IServiceProvider serviceProvider) => null;

    public override void Execute(System.IServiceProvider serviceProvider)
    {
        var host = {|#0:ServiceUri|}.Host;
    }
}
";

        var expected = new DiagnosticResult(PluginDependencyScopeAnalyzer.DependencyOutsideScopeId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("ServiceUri");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task DependencyMethodCallInsideUsing_IsAllowed()
    {
        var test = Preamble + @"
public sealed class MyPlugin : PluginBase
{
    [XrmTools.Meta.Attributes.Dependency]
    public System.Collections.Generic.List<string> Deps => Require<System.Collections.Generic.List<string>>();

    protected System.IDisposable CreateScope(System.IServiceProvider serviceProvider) => null;

    public override void Execute(System.IServiceProvider serviceProvider)
    {
        using (CreateScope(serviceProvider))
        {
            Deps.Add(""value"");
        }
    }
}
";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }
}
