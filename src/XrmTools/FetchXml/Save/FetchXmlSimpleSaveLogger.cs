#nullable enable
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using XrmTools.FetchXml.CodeGen;
using XrmTools.Logging.Compatibility;

namespace XrmTools.FetchXml;

[Export(typeof(IWpfTextViewCreationListener))]
[ContentType(FetchXmlContentTypeDefinitions.ContentTypeName)]
[TextViewRole(PredefinedTextViewRoles.Document)]
internal sealed class FetchXmlSimpleSaveLogger : IWpfTextViewCreationListener
{
    private const string SubscribedKey = "__fetchxml.save.simple.subscribed";

    [Import] private ITextDocumentFactoryService DocumentFactory { get; set; } = null!;
    [Import] private ILogger<FetchXmlSimpleSaveLogger> Logger { get; set; } = null!;

    public void TextViewCreated(IWpfTextView textView)
    {
        var buffer = textView?.TextBuffer;
        if (buffer == null) return;

        // Ensure we only subscribe once per buffer
        if (buffer.Properties.ContainsProperty(SubscribedKey)) return;
        buffer.Properties.AddProperty(SubscribedKey, true);

        ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            // If the ITextDocument already exists, attach now; otherwise, attach when it's created.
            if (DocumentFactory.TryGetTextDocument(buffer, out var doc))
            {
                AttachToDocument(buffer, doc);
            }
            else
            {
                void createdHandler(object s, TextDocumentEventArgs e)
                {
                    ThreadHelper.ThrowIfNotOnUIThread();
                    if (e?.TextDocument?.TextBuffer == buffer)
                    {
                        try { DocumentFactory.TextDocumentCreated -= createdHandler; } catch { }
                        AttachToDocument(buffer, e.TextDocument);
                    }
                }

                DocumentFactory.TextDocumentCreated += createdHandler;
            }
        }).FireAndForget();
    }

    private void AttachToDocument(ITextBuffer buffer, ITextDocument doc)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        void saveHandler(object s, TextDocumentFileActionEventArgs e)
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                var a = e.FileActionType;
                bool isSaving = a.HasFlag(FileActionTypes.ContentSavedToDisk);

                if (!isSaving) return;

                try
                {
                    // Get the FetchXmlDocument bound to this buffer
                    var fetchDoc = buffer.GetFetchXmlDocument(Logger);

                    Logger.LogDebug($"FetchXML saved: {fetchDoc.FileName} (entity='{fetchDoc.EntityName}')" + DateTimeOffset.Now.ToString("O"));

                    //var inputFile = await PhysicalFile.FromFileAsync(fetchDoc.FileName);

                    //if (inputFile is null)
                    //{
                    //    Logger.LogWarning($"Input file {fetchDoc.FileName} was not found, code generation will be skipped.");
                    //    return;
                    //}

                    //var lastGenFileName = await inputFile.GetAttributeAsync("LastGenOutput");
                    //if (string.IsNullOrWhiteSpace(lastGenFileName))
                    //{
                    //    lastGenFileName = Path.ChangeExtension(Path.GetFileName(fetchDoc.FileName), ".g.cs");
                    //    await inputFile.TrySetAttributeAsync(PhysicalFileAttribute.LastGenOutput, lastGenFileName);
                    //}
                    //var lastGenFilePath = Path.Combine(Path.GetDirectoryName(fetchDoc.FileName), lastGenFileName);
                    //using var stream = File.OpenWrite(lastGenFilePath);

                    //var codeGen = new FetchXmlCodeGeneratorAsync();
                    //var defaultNamespace = await GetDefaultNamespaceAsync(fetchDoc.FileName);

                    //await codeGen.GenerateAsync(fetchDoc, fetchDoc.FileName, default, stream, null, default);
                }
                catch (Exception ex)
                {
                    Logger.LogError($"FetchXML save logging failed: {ex.Message}");
                }
            });
        }

        void loadedHandler(object s, TextDocumentFileActionEventArgs e)
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                var a = e.FileActionType;
                bool isLoaded = a.HasFlag(FileActionTypes.ContentLoadedFromDisk);

                if (!isLoaded) return;

                try
                {
                    // Get the FetchXmlDocument bound to this buffer
                    var fetchDoc = buffer.GetFetchXmlDocument(Logger);

                    Logger.LogDebug($"FetchXML loaded: {fetchDoc.FileName} (entity='{fetchDoc.EntityName}')" + DateTimeOffset.Now.ToString("O"));

                }
                catch (Exception ex)
                {
                    Logger.LogError($"FetchXML save logging failed: {ex.Message}");
                }
            });
        }

        // Detach cleanly when the text document is disposed
        void disposedHandler(object s, TextDocumentEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (e?.TextDocument == doc)
            {
                try { doc.FileActionOccurred -= saveHandler; } catch { }
                try { DocumentFactory.TextDocumentDisposed -= disposedHandler; } catch { }
                try { buffer.Properties.RemoveProperty(SubscribedKey); } catch { }
            }
        }

        doc.FileActionOccurred += saveHandler;
        DocumentFactory.TextDocumentDisposed += disposedHandler;
    }

    private async Task<string> GetDefaultNamespaceAsync(string inputFilePath)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        if (await VS.GetServiceAsync<IVsHierarchy, IVsHierarchy>() is IVsHierarchy hierarchy)
        {
            // Get the current item ID
            hierarchy.ParseCanonicalName(inputFilePath, out var itemId);

            if (hierarchy.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_DefaultNamespace, out object defaultNamespace) == VSConstants.S_OK)
            {
                return defaultNamespace as string ?? string.Empty;
            }
        }
        return string.Empty;
    }
}
#nullable restore