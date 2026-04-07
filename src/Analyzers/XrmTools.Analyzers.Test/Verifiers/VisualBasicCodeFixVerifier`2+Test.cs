namespace XrmTools.Analyzers.Test
{
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Testing;
    using Microsoft.CodeAnalysis.VisualBasic.Testing;

    public static partial class VisualBasicCodeFixVerifier<TAnalyzer, TCodeFix>
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TCodeFix : CodeFixProvider, new()
    {
        public class Test : VisualBasicCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier>
        {
            public Test()
            {
                CodeFixTestBehaviors = CodeFixTestBehaviors.SkipFixAllCheck | CodeFixTestBehaviors.SkipLocalDiagnosticCheck;
            }
        }
    }
}
