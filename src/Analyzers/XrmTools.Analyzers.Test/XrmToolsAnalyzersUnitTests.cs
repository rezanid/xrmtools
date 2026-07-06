namespace XrmTools.Analyzers.Test;

using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = CSharpAnalyzerVerifier<PluginDependencyAnalyzer>;

public class XrmToolsAnalyzersUnitTest
{
    private const string Preamble = @"
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

    public enum Stages
    {
        PreValidation = 10,
        PreOperation = 20,
        MainOperation = 30,
        PostOperation = 40,
    }

    public enum ExecutionMode
    {
        Synchronous = 0,
        Asynchronous = 1,
    }

    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class StepAttribute : System.Attribute
    {
        public StepAttribute(string messageName, Stages stage, ExecutionMode mode) { }
        public StepAttribute(string messageName, string primaryEntityName, Stages stage, ExecutionMode mode) { }
        public StepAttribute(string messageName, string primaryEntityName, string filteringAttributes, Stages stage, ExecutionMode mode) { }
        public ExecutionMode Mode { get; set; }
    }

    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class ImageAttribute : System.Attribute { }
}

public abstract class PluginBase : Microsoft.Xrm.Sdk.IPlugin
{
    protected T Require<T>() => default!;
}
";

    [Fact]
    public async Task NonPluginType_IsIgnored()
    {
        var test = Preamble + @"
public class NotAPlugin
{
    protected T Require<T>() => default!;

    public string Foo => Require<string>();
}
";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task Plugin_DependencyProperty_WithAttribute_AndNoSetter_NoDiagnostics()
    {
        var test = Preamble + @"
public sealed class MyPlugin : PluginBase
{
    [XrmTools.Meta.Attributes.Dependency]
    public string Foo => Require<string>();
}
";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task Plugin_DependencyProperty_MissingAttribute_ReportsXrmTools001()
    {
        var test = Preamble + @"
public sealed class MyPlugin : PluginBase
{
    public string {|#0:Foo|} => Require<string>();
}
";

        var expected = new DiagnosticResult(PluginDependencyAnalyzer.MissingDependencyAttributeId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Foo");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task Plugin_DependencyProperty_WithSetter_ReportsXrmTools002_OnSetterKeyword()
    {
        var test = Preamble + @"
public sealed class MyPlugin : PluginBase
{
    [XrmTools.Meta.Attributes.Dependency]
    public string Foo
    {
        get => Require<string>();
        {|#0:set|} { }
    }
}
";

        var expected = new DiagnosticResult(PluginDependencyAnalyzer.HasSetterOrInitId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Foo");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task Plugin_DependencyProperty_MissingAttribute_AndHasSetter_ReportsBothDiagnostics()
    {
        var test = Preamble + @"
public sealed class MyPlugin : PluginBase
{
    public string {|#0:Foo|}
    {
        get => Require<string>();
        {|#1:set|} { }
    }
}
";

        var expected1 = new DiagnosticResult(PluginDependencyAnalyzer.MissingDependencyAttributeId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Foo");

        var expected2 = new DiagnosticResult(PluginDependencyAnalyzer.HasSetterOrInitId, DiagnosticSeverity.Warning)
            .WithLocation(1)
            .WithArguments("Foo");

        await VerifyCS.VerifyAnalyzerAsync(test, expected1, expected2);
    }

    [Fact]
    public async Task Plugin_PropertyNotCallingRequire_IsIgnored()
    {
        var test = Preamble + @"
public sealed class MyPlugin : PluginBase
{
    public string Foo => ""x"";
}
";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task Plugin_DependencyProperty_WithSet_ReportsXrmTools002_OnSetKeyword_SecondCase()
    {
        var test = Preamble + @"
public sealed class MyPlugin : PluginBase
{
    [XrmTools.Meta.Attributes.Dependency]
    public string Foo
    {
        get => Require<string>();
        {|#0:set|} { }
    }
}
";

        var expected = new DiagnosticResult(PluginDependencyAnalyzer.HasSetterOrInitId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Foo");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task Plugin_RequireWithArguments_IsNotDependencyProperty()
    {
        var test = Preamble + @"
public sealed class MyPlugin : PluginBase
{
    public T Require<T>(int x) => default!;

    public string {|#0:Foo|} => Require<string>(123);
}
";

        var expected = new DiagnosticResult(PluginDependencyAnalyzer.MissingDependencyAttributeId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Foo");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task Plugin_TwoDependencyProperties_ReportDiagnosticsForEach()
    {
        var test = Preamble + @"
public sealed class MyPlugin : PluginBase
{
    public string {|#0:Foo|} => Require<string>();
    public int {|#1:Bar|} => Require<int>();
}
";

        var expected1 = new DiagnosticResult(PluginDependencyAnalyzer.MissingDependencyAttributeId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Foo");

        var expected2 = new DiagnosticResult(PluginDependencyAnalyzer.MissingDependencyAttributeId, DiagnosticSeverity.Warning)
            .WithLocation(1)
            .WithArguments("Bar");

        await VerifyCS.VerifyAnalyzerAsync(test, expected1, expected2);
    }

    [Fact]
    public async Task Plugin_InheritsIPluginThroughBaseClass_IsAnalyzed()
    {
        var test = Preamble + @"
public abstract class BasePlugin : PluginBase
{
}

public sealed class MyPlugin : BasePlugin
{
    public string {|#0:Foo|} => Require<string>();
}
";

        var expected = new DiagnosticResult(PluginDependencyAnalyzer.MissingDependencyAttributeId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Foo");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task Plugin_InheritedDependencyProperty_IsAnalyzed()
    {
        var test = Preamble + @"
public abstract class BasePlugin : PluginBase
{
    public string {|#0:Foo|} => Require<string>();
}

public sealed class MyPlugin : BasePlugin
{
}
";

        var expected = new DiagnosticResult(PluginDependencyAnalyzer.MissingDependencyAttributeId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Foo");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task Plugin_UsesUnqualifiedDependencyAttribute_NoDiagnostics()
    {
        var test = @"using XrmTools.Meta.Attributes;
" + Preamble + @"
public sealed class MyPlugin : PluginBase
{
    [Dependency]
    public string Foo => Require<string>();
}
";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task Plugin_PluginAttributeMustComeBeforeStep_ReportsXrmTools004()
    {
        var test = Preamble + @"
[XrmTools.Meta.Attributes.Step(""Create"", XrmTools.Meta.Attributes.Stages.PostOperation, XrmTools.Meta.Attributes.ExecutionMode.Synchronous)]
[XrmTools.Meta.Attributes.Plugin]
public sealed class {|#0:MyPlugin|} : PluginBase
{
}
";

        var expected = new DiagnosticResult(PluginDependencyAnalyzer.PluginAttributeOrderId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("MyPlugin");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task Plugin_PluginAttributeMustComeBeforeCustomApi_ReportsXrmTools004()
    {
        var test = Preamble + @"
[XrmTools.Meta.Attributes.CustomApi]
[XrmTools.Meta.Attributes.Plugin]
public sealed class {|#0:MyPlugin|} : PluginBase
{
}
";

        var expected = new DiagnosticResult(PluginDependencyAnalyzer.PluginAttributeOrderId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("MyPlugin");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task Plugin_ImageBeforeStep_ReportsXrmTools004()
    {
        var test = Preamble + @"
[XrmTools.Meta.Attributes.Plugin]
[XrmTools.Meta.Attributes.Image]
[XrmTools.Meta.Attributes.Step(""Create"", XrmTools.Meta.Attributes.Stages.PostOperation, XrmTools.Meta.Attributes.ExecutionMode.Synchronous)]
public sealed class {|#0:MyPlugin|} : PluginBase
{
}
";

        var expected = new DiagnosticResult(PluginDependencyAnalyzer.PluginAttributeOrderId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("MyPlugin");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task Plugin_ImageAfterStep_IsAllowed()
    {
        var test = Preamble + @"
[XrmTools.Meta.Attributes.Plugin]
[XrmTools.Meta.Attributes.Step(""Create"", XrmTools.Meta.Attributes.Stages.PostOperation, XrmTools.Meta.Attributes.ExecutionMode.Synchronous)]
[XrmTools.Meta.Attributes.Image]
public sealed class MyPlugin : PluginBase
{
}
";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task Plugin_MultipleStepsAndImages_ImageAfterFirstStep_IsAllowed()
    {
        var test = Preamble + @"
[XrmTools.Meta.Attributes.Plugin]
[XrmTools.Meta.Attributes.Step(""Create"", XrmTools.Meta.Attributes.Stages.PostOperation, XrmTools.Meta.Attributes.ExecutionMode.Synchronous)]
[XrmTools.Meta.Attributes.Image]
[XrmTools.Meta.Attributes.Step(""Update"", ""account"", XrmTools.Meta.Attributes.Stages.PostOperation, XrmTools.Meta.Attributes.ExecutionMode.Synchronous)]
[XrmTools.Meta.Attributes.Image]
public sealed class MyPlugin : PluginBase
{
}
";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task Plugin_PreValidationAsyncStep_ReportsXrmTools006()
    {
        var test = Preamble + @"
[XrmTools.Meta.Attributes.Plugin]
[{|#0:XrmTools.Meta.Attributes.Step(""Create"", XrmTools.Meta.Attributes.Stages.PreValidation, XrmTools.Meta.Attributes.ExecutionMode.Asynchronous)|}]
public sealed class MyPlugin : PluginBase
{
}
";

        var expected = new DiagnosticResult(PluginDependencyAnalyzer.InvalidStepStageModeId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Stages.PreValidation");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task Plugin_PreOperationAsyncStep_WithPrimaryEntityConstructor_ReportsXrmTools006()
    {
        var test = Preamble + @"
[XrmTools.Meta.Attributes.Plugin]
[{|#0:XrmTools.Meta.Attributes.Step(""Update"", ""account"", XrmTools.Meta.Attributes.Stages.PreOperation, XrmTools.Meta.Attributes.ExecutionMode.Asynchronous)|}]
public sealed class MyPlugin : PluginBase
{
}
";

        var expected = new DiagnosticResult(PluginDependencyAnalyzer.InvalidStepStageModeId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Stages.PreOperation");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task Plugin_PreValidationAsyncStep_WithFilteringAttributesConstructor_ReportsXrmTools006()
    {
        var test = Preamble + @"
[XrmTools.Meta.Attributes.Plugin]
[{|#0:XrmTools.Meta.Attributes.Step(""Update"", ""account"", ""name"", XrmTools.Meta.Attributes.Stages.PreValidation, XrmTools.Meta.Attributes.ExecutionMode.Asynchronous)|}]
public sealed class MyPlugin : PluginBase
{
}
";

        var expected = new DiagnosticResult(PluginDependencyAnalyzer.InvalidStepStageModeId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Stages.PreValidation");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task Plugin_PreOperationAsyncStep_WithNamedModeOverride_ReportsXrmTools006()
    {
        var test = Preamble + @"
[XrmTools.Meta.Attributes.Plugin]
[{|#0:XrmTools.Meta.Attributes.Step(""Update"", XrmTools.Meta.Attributes.Stages.PreOperation, XrmTools.Meta.Attributes.ExecutionMode.Synchronous, Mode = XrmTools.Meta.Attributes.ExecutionMode.Asynchronous)|}]
public sealed class MyPlugin : PluginBase
{
}
";

        var expected = new DiagnosticResult(PluginDependencyAnalyzer.InvalidStepStageModeId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Stages.PreOperation");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task Plugin_PostOperationAsyncStep_IsAllowed()
    {
        var test = Preamble + @"
[XrmTools.Meta.Attributes.Plugin]
[XrmTools.Meta.Attributes.Step(""Create"", XrmTools.Meta.Attributes.Stages.PostOperation, XrmTools.Meta.Attributes.ExecutionMode.Asynchronous)]
public sealed class MyPlugin : PluginBase
{
}
";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }
}
