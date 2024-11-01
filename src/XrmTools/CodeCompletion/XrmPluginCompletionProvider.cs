#nullable enable
namespace XrmTools.CodeCompletion;

using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using XrmTools.Helpers;
using Microsoft.VisualStudio.Text;
using System;
using XrmTools.Logging;
using XrmTools.Xrm.CodeCompletion;
using XrmTools.Xrm;

[Export(typeof(IAsyncCompletionSourceProvider))]
[ContentType("CSharp")]
[Name(nameof(XrmPluginCompletionProvider))]
//[TextViewRole(PredefinedTextViewRoles.Editable)]
[method: ImportingConstructor]
public class XrmPluginCompletionProvider(
    [Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider,
    [Import] IOutputLoggerService logger,
    [Import] IXrmSchemaProviderFactory xrmSchemaProviderFactory,
    [Import] ITextStructureNavigatorSelectorService structureNavigatorSelector) : IAsyncCompletionSourceProvider
{
    private IVsRunningDocumentTable? _RunningDocumenTable;

    public IAsyncCompletionSource? GetOrCreate(ITextView textView)
    {
        if (textView is null) { return null; }
        if (xrmSchemaProviderFactory is null)
        {
            throw new InvalidOperationException($"XrmSchemaProviderFactory is missing the in {nameof(XrmPluginDefCompletionProvider)}.");
        }
        return textView.Properties.GetOrCreateSingletonProperty(() =>
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (textView.TextBuffer.Properties.GetProperty(typeof(ITextDocument)) is ITextDocument document)
            {
                return new XrmPluginDefCompletionSource(logger, xrmSchemaProviderFactory, structureNavigatorSelector, textView.TextBuffer);
            }
            return null;
        });
    }

    private IVsRunningDocumentTable? RunningDocumenTable
        => _RunningDocumenTable ??= serviceProvider?.GetService<SVsRunningDocumentTable, IVsRunningDocumentTable>();

    private string? GetProjectProperty(string inputFilePath, string propertyName) => RunningDocumenTable?.TryGetHierarchyAndItemID(inputFilePath, out var hierarchy, out _) == true
        ? hierarchy.GetProjectProperty(propertyName)
        : null;
}
#nullable restore
