using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using XrmGen.Extensions;
using Microsoft.VisualStudio.Text;



namespace XrmGen.Xrm.CodeCompletion;

[Export(typeof(IAsyncCompletionSourceProvider))]
[ContentType("JSON")]
[Name(nameof(XrmPluginDefCompletionProvider))]
//[TextViewRole(PredefinedTextViewRoles.Editable)]
public class XrmPluginDefCompletionProvider : IAsyncCompletionSourceProvider
{
    private IVsRunningDocumentTable _RunningDocumenTable;

    [Import]
    [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "MEF sets this property.")]
    ITextStructureNavigatorSelectorService StructureNavigatorSelector;

    [Import]
    public IXrmSchemaProviderFactory XrmSchemaProviderFactory;

    [Import]
    public SVsServiceProvider ServiceProvider;

    private IVsRunningDocumentTable RunningDocumenTable
    {
        get => _RunningDocumenTable ??= ServiceProvider.GetService(typeof(SVsRunningDocumentTable)) as IVsRunningDocumentTable;
    } 

    public IAsyncCompletionSource GetOrCreate(ITextView textView)
        => textView.Properties.GetOrCreateSingletonProperty(() =>
        {
            // Get EnvironmentUrl and ApplicationId from project properties.
            if (textView.TextBuffer.Properties.GetProperty(typeof(ITextDocument)) is ITextDocument document)
            {
                string environmentUrl = GetProjectProperty(document.FilePath, "EnvironmentUrl");
                string applicationId = GetProjectProperty(document.FilePath, "ApplicationId");

                var xrmProvider = XrmSchemaProviderFactory.Get(environmentUrl, applicationId);
                return new XrmPluginDefCompletionSource(xrmProvider, StructureNavigatorSelector, textView.TextBuffer);
            }
            return null;
        });


    private string GetProjectProperty(string filePath, string propertyName) => RunningDocumenTable.TryGetHierarchyAndItemID(filePath, out var hierarchy, out _)
            ? hierarchy.GetPropertyForProject(propertyName)
            : null;
}
