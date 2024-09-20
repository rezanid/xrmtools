#nullable enable
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
using Nito.AsyncEx.Synchronous;

namespace XrmTools.Xrm.CodeCompletion;

[Export(typeof(IAsyncCompletionSourceProvider))]
[ContentType("JSON")]
[Name(nameof(XrmPluginDefCompletionProvider))]
//[TextViewRole(PredefinedTextViewRoles.Editable)]
public class XrmPluginDefCompletionProvider : IAsyncCompletionSourceProvider
{
    private IVsRunningDocumentTable? _RunningDocumenTable;

    //[Import]
    //[SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "MEF sets this property.")]
    ITextStructureNavigatorSelectorService StructureNavigatorSelector;

    //[Import]
    public IXrmSchemaProviderFactory XrmSchemaProviderFactory;

    //[Import(typeof(SVsServiceProvider))]
    public IServiceProvider ServiceProvider;

    public IEnvironmentProvider EnvironmentProvider;

    [ImportingConstructor]
    public XrmPluginDefCompletionProvider(
        [Import(typeof(SVsServiceProvider))]IServiceProvider serviceProvider,
        [Import]IEnvironmentProvider environmentProvider,
        [Import]IXrmSchemaProviderFactory xrmSchemaProviderFactory,
        [Import]ITextStructureNavigatorSelectorService structureNavigatorSelector)
    {
        ServiceProvider = serviceProvider;
        EnvironmentProvider = environmentProvider;
        XrmSchemaProviderFactory = xrmSchemaProviderFactory;
        StructureNavigatorSelector = structureNavigatorSelector;
    }

    public IAsyncCompletionSource? GetOrCreate(ITextView textView)
    {
        if (textView is null) { return null; }
        if (XrmSchemaProviderFactory is null)
        {
            throw new InvalidOperationException($"XrmSchemaProviderFactory is missing the in {nameof(XrmPluginDefCompletionProvider)}.");
        }
        return textView.Properties.GetOrCreateSingletonProperty(() =>
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            // Get EnvironmentUrl and ApplicationId from project properties.
            if (textView.TextBuffer.Properties.GetProperty(typeof(ITextDocument)) is ITextDocument document)
            {
                var environment = EnvironmentProvider?.GetActiveEnvironmentAsync().WaitAndUnwrapException();
                if (environment is null || !environment.IsValid())
                {
                    return null;
                }
                var schemaProvider = XrmSchemaProviderFactory.Get(environment);
                if (schemaProvider is null) return null;
                var xrmProvider = XrmSchemaProviderFactory.GetOrNew(environment);
                return new XrmPluginDefCompletionSource(xrmProvider, StructureNavigatorSelector, textView.TextBuffer);
            }
            return null;
        });
    }

    private IVsRunningDocumentTable? RunningDocumenTable
        => _RunningDocumenTable ??= ServiceProvider?.GetService<SVsRunningDocumentTable, IVsRunningDocumentTable>();

    private string? GetProjectProperty(string inputFilePath, string propertyName) => RunningDocumenTable?.TryGetHierarchyAndItemID(inputFilePath, out var hierarchy, out _) == true
        ? hierarchy.GetProjectProperty(propertyName)
        : null;
}
#nullable restore