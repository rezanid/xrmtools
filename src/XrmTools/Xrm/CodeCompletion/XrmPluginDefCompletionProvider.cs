#nullable enable
namespace XrmTools.Xrm.CodeCompletion;

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
using XrmTools.Xrm.Repositories;

[Export(typeof(IAsyncCompletionSourceProvider))]
[ContentType("JSON")]
[Name(nameof(XrmPluginDefCompletionProvider))]
//[TextViewRole(PredefinedTextViewRoles.Editable)]
[method: ImportingConstructor]
internal class XrmPluginDefCompletionProvider(
    [Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider,
    [Import] IOutputLoggerService logger,
    [Import] IRepositoryFactory repositoryFactory,
    [Import] ITextStructureNavigatorSelectorService structureNavigatorSelector) : IAsyncCompletionSourceProvider
{
    private IVsRunningDocumentTable? _RunningDocumenTable;

    public IAsyncCompletionSource? GetOrCreate(ITextView textView)
    {
        if (textView is null) { return null; }
        if (repositoryFactory is null)
        {
            throw new InvalidOperationException($"XrmSchemaProviderFactory is missing the in {nameof(XrmPluginDefCompletionProvider)}.");
        }
        return textView.Properties.GetOrCreateSingletonProperty(() =>
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (textView.TextBuffer.Properties.GetProperty(typeof(ITextDocument)) is ITextDocument document)
            {
                return new XrmPluginDefCompletionSource(logger, repositoryFactory, structureNavigatorSelector, textView.TextBuffer);
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