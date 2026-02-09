namespace XrmTools.Analyzers.Test;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using VerifyCS = CSharpCodeFixVerifier<PluginDependencyAnalyzer, XrmToolsAnalyzersCodeFixProvider>;

public class XrmToolsAnalyzersCodeFixTests
{
    private const string Preamble = @"
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}

namespace Microsoft.Xrm.Sdk
{
    public interface IPlugin { }
}

namespace XrmTools.Meta.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public sealed class DependencyAttribute : System.Attribute
    {
        public DependencyAttribute() { }
        public DependencyAttribute(string name) { }
        public string Name { get; set; }
    }

    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class PluginAttribute : System.Attribute { }

    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class CustomApiAttribute : System.Attribute { }

    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class StepAttribute : System.Attribute { }

    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class ImageAttribute : System.Attribute { }
}

public abstract class PluginBase : Microsoft.Xrm.Sdk.IPlugin
{
    protected T Require<T>() => default!;
}
";

    [Fact]
    public async Task XrmTools001_AddDependencyAttribute_ExpressionBodiedProperty()
    {
        var before = Preamble + @"
public sealed class MyPlugin : PluginBase
{
    public string {|#0:Dep|} => Require<string>();
}
";

        var after = Preamble + @"
public sealed class MyPlugin : PluginBase
{
    [XrmTools.Meta.Attributes.Dependency]
    public string Dep => Require<string>();
}
";

        var expected = new DiagnosticResult(PluginDependencyAnalyzer.MissingDependencyAttributeId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Dep");

        await VerifyCS.VerifyCodeFixAsync(before, expected, after);
    }

    [Fact]
    public async Task XrmTools001_AddDependencyAttribute_GetterBasedProperty()
    {
        var before = Preamble + @"
public sealed class MyPlugin : PluginBase
{
    public string {|#0:Dep|}
    {
        get => Require<string>();
    }
}
";

        var after = Preamble + @"
public sealed class MyPlugin : PluginBase
{
    [XrmTools.Meta.Attributes.Dependency]
    public string Dep
    {
        get => Require<string>();
    }
}
";

        var expected = new DiagnosticResult(PluginDependencyAnalyzer.MissingDependencyAttributeId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Dep");

        await VerifyCS.VerifyCodeFixAsync(before, expected, after);
    }

    [Fact]
    public async Task XrmTools001_AddDependencyAttribute_GetterWithBody()
    {
        var before = Preamble + @"
public sealed class MyPlugin : PluginBase
{
    public string {|#0:Dep|}
    {
        get
        {
            return Require<string>();
        }
    }
}
";

        var after = Preamble + @"
public sealed class MyPlugin : PluginBase
{
    [XrmTools.Meta.Attributes.Dependency]
    public string Dep
    {
        get
        {
            return Require<string>();
        }
    }
}
";

        var expected = new DiagnosticResult(PluginDependencyAnalyzer.MissingDependencyAttributeId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Dep");

        await VerifyCS.VerifyCodeFixAsync(before, expected, after);
    }

    [Fact]
    public async Task XrmTools001_AddDependencyAttribute_ExistingAttributes()
    {
        var before = Preamble + @"
public sealed class MyPlugin : PluginBase
{
    [System.Obsolete]
    public string {|#0:Dep|} => Require<string>();
}
";

        var after = Preamble + @"
public sealed class MyPlugin : PluginBase
{
    [System.Obsolete]
    [XrmTools.Meta.Attributes.Dependency]
    public string Dep => Require<string>();
}
";

        var expected = new DiagnosticResult(PluginDependencyAnalyzer.MissingDependencyAttributeId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Dep");

        await VerifyCS.VerifyCodeFixAsync(before, expected, after);
    }

    [Fact]
    public async Task XrmTools002_RemoveSetter_SimpleProperty()
    {
        var before = Preamble + @"
public sealed class MyPlugin : PluginBase
{
    [XrmTools.Meta.Attributes.Dependency]
    public string Dep
    {
        get => Require<string>();
        {|#0:set|} { }
    }
}
";

        var after = Preamble + @"
public sealed class MyPlugin : PluginBase
{
    [XrmTools.Meta.Attributes.Dependency]
    public string Dep
    {
        get => Require<string>();
    }
}
";

        var expected = new DiagnosticResult(PluginDependencyAnalyzer.HasSetterOrInitId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Dep");

        await VerifyCS.VerifyCodeFixAsync(before, expected, after);
    }

    [Fact]
    public async Task XrmTools002_RemoveInit_SimpleProperty()
    {
        var before = Preamble + @"
public sealed class MyPlugin : PluginBase
{
    [XrmTools.Meta.Attributes.Dependency]
    public string Dep
    {
        get => Require<string>();
        {|#0:init|} { }
    }
}
";

        var after = Preamble + @"
public sealed class MyPlugin : PluginBase
{
    [XrmTools.Meta.Attributes.Dependency]
    public string Dep
    {
        get => Require<string>();
    }
}
";

        var expected = new DiagnosticResult(PluginDependencyAnalyzer.HasSetterOrInitId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Dep");

        await VerifyCS.VerifyCodeFixAsync(before, expected, after);
    }

    [Fact]
    public async Task XrmTools002_RemoveSetterWithBody()
    {
        var before = Preamble + @"
public sealed class MyPlugin : PluginBase
{
    [XrmTools.Meta.Attributes.Dependency]
    public string Dep
    {
        get
        {
            return Require<string>();
        }
        {|#0:set|}
        {
            // setter logic
        }
    }
}
";

        var after = Preamble + @"
public sealed class MyPlugin : PluginBase
{
    [XrmTools.Meta.Attributes.Dependency]
    public string Dep
    {
        get
        {
            return Require<string>();
        }
    }
}
";

        var expected = new DiagnosticResult(PluginDependencyAnalyzer.HasSetterOrInitId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Dep");

        await VerifyCS.VerifyCodeFixAsync(before, expected, after);
    }

    [Fact]
    public async Task XrmTools002_RemovePrivateSetter()
    {
        var before = Preamble + @"
public sealed class MyPlugin : PluginBase
{
    [XrmTools.Meta.Attributes.Dependency]
    public string Dep
    {
        get => Require<string>();
        private {|#0:set|} { }
    }
}
";

        var after = Preamble + @"
public sealed class MyPlugin : PluginBase
{
    [XrmTools.Meta.Attributes.Dependency]
    public string Dep
    {
        get => Require<string>();
    }
}
";

        var expected = new DiagnosticResult(PluginDependencyAnalyzer.HasSetterOrInitId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Dep");

        await VerifyCS.VerifyCodeFixAsync(before, expected, after);
    }

    [Fact]
    public async Task BothFixes_MissingAttributeAndHasSetter()
    {
        var before = Preamble + @"
public sealed class MyPlugin : PluginBase
{
    public string {|#0:Dep|}
    {
        get => Require<string>();
        {|#1:set|} { _ = value; }
    }
}
";

        var afterFix1 = Preamble + @"
public sealed class MyPlugin : PluginBase
{
    [XrmTools.Meta.Attributes.Dependency]
    public string Dep
    {
        get => Require<string>();
    }
}
";

        var expectedBefore = new DiagnosticResult[]
        {
            new DiagnosticResult(PluginDependencyAnalyzer.MissingDependencyAttributeId, DiagnosticSeverity.Warning)
                .WithLocation(0)
                .WithArguments("Dep"),
            new DiagnosticResult(PluginDependencyAnalyzer.HasSetterOrInitId, DiagnosticSeverity.Warning)
                .WithLocation(1)
                .WithArguments("Dep"),
        };

        var test = new VerifyCS.Test
        {
            TestCode = before,
            FixedCode = afterFix1,
            // Allow 2 iterations: one for adding attribute, one for removing setter
            NumberOfIncrementalIterations = 2,
            NumberOfFixAllIterations = 2
        };

        test.ExpectedDiagnostics.AddRange(expectedBefore);
        await test.RunAsync(CancellationToken.None);
    }
}