#nullable enable
namespace XrmTools.CodeRefactoringProviders;

using Community.VisualStudio.Toolkit;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using XrmTools.CodeGen.CustomApi;
using XrmTools.DataverseExplorer.Services;

[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(CustomApiOpenApiRefactoringProvider)), Shared]
public sealed class CustomApiOpenApiRefactoringProvider : CodeRefactoringProvider
{
    public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null) return;
        var node = root.FindToken(context.Span.Start).Parent;
        var declaration = node?.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().FirstOrDefault();
        if (declaration == null) return;
        var attribute = node?.AncestorsAndSelf().OfType<AttributeSyntax>().FirstOrDefault();
        if (attribute == null && !declaration.Identifier.Span.IntersectsWith(context.Span)) return;
        var model = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
        var symbol = model?.GetDeclaredSymbol(declaration, context.CancellationToken);
        if (symbol == null || CustomApiSourceReader.Attribute(symbol, "CustomApiAttribute") == null ||
            CustomApiSourceReader.Attribute(symbol, "PluginAttribute") == null) return;
        if (attribute != null && (model!.GetSymbolInfo(attribute, context.CancellationToken).Symbol as IMethodSymbol)?.ContainingType.ToDisplayString()
            != "XrmTools.Meta.Attributes.CustomApiAttribute") return;
        context.RegisterRefactoring(new GenerateAction(context.Document, declaration.Span));
    }

    private sealed class GenerateAction(Document document, TextSpan span) : CodeAction
    {
        public override string Title => "Generate OpenAPI specification from source";
        public override string EquivalenceKey => "GenerateCustomApiOpenApiFromSource";
        protected override Task<IEnumerable<CodeActionOperation>> ComputeOperationsAsync(CancellationToken cancellationToken)
            => Task.FromResult<IEnumerable<CodeActionOperation>>([new GenerateOperation(document, span)]);
        protected override Task<IEnumerable<CodeActionOperation>> ComputePreviewOperationsAsync(CancellationToken cancellationToken)
            => Task.FromResult<IEnumerable<CodeActionOperation>>([]);
    }

    private sealed class GenerateOperation(Document document, TextSpan span) : CodeActionOperation
    {
        public override void Apply(Workspace workspace, CancellationToken cancellationToken)
        {
            // Opening an untitled document inside Roslyn's edit/undo scope can cause
            // the host to persist it. Queue an independent UI command after Apply
            // returns, rather than enrolling the generated document in that scope.
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
                // Joining this independent command would re-enroll it in the host's
                // code-action scope. FileAndForget observes its completion instead.
#pragma warning disable VSSDK007
                QueueAfterApply(Dispatcher.CurrentDispatcher, () =>
                    ThreadHelper.JoinableTaskFactory.RunAsync(GenerateAsync).FileAndForget("XrmTools/OpenApi/GenerateFromSource"));
#pragma warning restore VSSDK007
            });
        }

        private async Task GenerateAsync()
        {
            // The code-action token belongs to the completed host operation. The
            // independent command must not inherit its lifetime or ambient scope.
            var token = CancellationToken.None;
            try
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(token);
                var components = await AsyncServiceProvider.GlobalProvider.GetServiceAsync(typeof(SComponentModel)) as IComponentModel;
                var generator = components?.GetService<ICustomApiSourceGenerator>()
                    ?? throw new InvalidOperationException("The Custom API OpenAPI generator is unavailable.");
                var result = await Task.Run(() => generator.GenerateAsync(document, span, token), token);
                await GeneratedCustomApiDocument.OpenAsync(result, "OpenAPI specification", token);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                await VS.MessageBox.ShowErrorAsync(Vsix.Name, "OpenAPI generation failed. " + ex.Message);
            }
        }
    }

    internal static void QueueAfterApply(Dispatcher dispatcher, Action command)
    {
        // This is intentionally a deferred UI command, not a thread switch. Awaiting
        // it here would keep the enclosing code-action scope active.
#pragma warning disable VSTHRD001
        if (ExecutionContext.IsFlowSuppressed())
            _ = dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, command);
        else
            using (ExecutionContext.SuppressFlow())
                _ = dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, command);
#pragma warning restore VSTHRD001
    }
}
