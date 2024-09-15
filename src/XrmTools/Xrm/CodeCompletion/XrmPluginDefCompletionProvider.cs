#nullable enable
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using XrmGen.Helpers;
using Microsoft.VisualStudio.Text;
using System.Diagnostics;
using System;
using System.Runtime.InteropServices;

namespace XrmGen.Xrm.CodeCompletion;

[Export(typeof(IAsyncCompletionSourceProvider))]
[ContentType("JSON")]
[Name(nameof(XrmPluginDefCompletionProvider))]
//[TextViewRole(PredefinedTextViewRoles.Editable)]
public class XrmPluginDefCompletionProvider : IAsyncCompletionSourceProvider
{
    private IVsRunningDocumentTable? _RunningDocumenTable;

    [Import]
    [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "MEF sets this property.")]
    ITextStructureNavigatorSelectorService? StructureNavigatorSelector = null;

    [Import]
    public IXrmSchemaProviderFactory? XrmSchemaProviderFactory = null;

    [Import]
    public SVsServiceProvider? ServiceProvider;

    private IVsRunningDocumentTable? RunningDocumenTable 
        => _RunningDocumenTable ??= ServiceProvider?.GetService(typeof(SVsRunningDocumentTable)) as IVsRunningDocumentTable;

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
                string? environmentUrl = GetProjectProperty(document.FilePath, "EnvironmentUrl");
                string? applicationId = GetProjectProperty(document.FilePath, "ApplicationId");

                if (string.IsNullOrEmpty(environmentUrl)) 
                {
                    // Write to debug that the properties are not set.
                    Debugger.Log(0, "XrmPluginDefCompletionProvider", "EnvironmentUrl or ApplicationId is not set in project properties.");
                    return null; 
                }

                var xrmProvider = XrmSchemaProviderFactory.GetOrNew(environmentUrl!);
                return new XrmPluginDefCompletionSource(xrmProvider, StructureNavigatorSelector, textView.TextBuffer);
            }
            return null;
        });
    }

    private string? GetProjectProperty(string inputFilePath, string propertyName) => RunningDocumenTable?.TryGetHierarchyAndItemID(inputFilePath, out var hierarchy, out _) == true
        ? hierarchy.GetProjectProperty(propertyName)
        : null;
}
#nullable restore