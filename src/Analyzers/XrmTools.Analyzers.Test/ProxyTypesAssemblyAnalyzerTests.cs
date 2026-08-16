namespace XrmTools.Analyzers.Test;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using System.Threading.Tasks;
using Xunit;
using VerifyCS = CSharpAnalyzerVerifier<ProxyTypesAssemblyAnalyzer>;

public class ProxyTypesAssemblyAnalyzerTests
{
    private const string Preamble = @"
namespace Microsoft.Xrm.Sdk
{
    public class Entity { }
}

namespace Microsoft.Xrm.Sdk.Client
{
    [System.AttributeUsage(System.AttributeTargets.Assembly)]
    public sealed class ProxyTypesAssemblyAttribute : System.Attribute
    {
    }
}
";

    [Fact]
    public async Task NoEntityTypes_NoDiagnostics()
    {
        var test = Preamble + @"
public sealed class Foo
{
}
";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task EntityTypeWithoutAssemblyAttribute_ReportsDiagnostic()
    {
        var test = Preamble + @"
public sealed class {|#0:Account|} : Microsoft.Xrm.Sdk.Entity
{
}
";

        var expected = new DiagnosticResult(ProxyTypesAssemblyAnalyzer.MissingProxyTypesAssemblyAttributeId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Account");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task EntityTypeWithAssemblyAttribute_NoDiagnostics()
    {
        var test = @"
[assembly: Microsoft.Xrm.Sdk.Client.ProxyTypesAssemblyAttribute]
" + Preamble + @"
public sealed class Account : Microsoft.Xrm.Sdk.Entity
{
}
";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }
}
