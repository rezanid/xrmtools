#nullable enable
namespace XrmTools.FetchXml;
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
using XrmTools.Logging.Compatibility;
using System.Text.RegularExpressions;
using XrmTools.Helpers;
using System.Threading;
using XrmTools.FetchXml.Margin;
using XrmTools.Options;

[Export(typeof(IWpfTextViewCreationListener))]
[ContentType(FetchXmlContentTypeDefinitions.ContentTypeName)]
[TextViewRole(PredefinedTextViewRoles.Document)]
internal sealed class FetchXmlDocumentInitializer : IWpfTextViewCreationListener
{
    private const string SubscribedKey = "__fetchxml.save.simple.subscribed";

    [Import] private ITextDocumentFactoryService DocumentFactory { get; set; } = null!;
    [Import] private ILogger<FetchXmlDocumentInitializer> Logger { get; set; } = null!;

    public void TextViewCreated(IWpfTextView textView)
    {
        if (textView == null) throw new ArgumentNullException(nameof(textView));
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
                AttachToDocument(textView!, buffer, doc);
            }
            else
            {
                void createdHandler(object s, TextDocumentEventArgs e)
                {
                    ThreadHelper.ThrowIfNotOnUIThread();
                    if (e?.TextDocument?.TextBuffer == buffer)
                    {
                        try { DocumentFactory.TextDocumentCreated -= createdHandler; } catch { }
                        AttachToDocument(textView!, buffer, e.TextDocument);
                    }
                }

                DocumentFactory.TextDocumentCreated += createdHandler;
            }
        }).FireAndForget();
    }

    private void AttachToDocument(IWpfTextView textView, ITextBuffer buffer, ITextDocument doc)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

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

                    // Wait for parsing to complete; then ensure header/schema attributes
                    await WaitForParsedAsync(fetchDoc, TimeSpan.FromSeconds(5));
                    await EnsureSchemaHeaderAsync(buffer, doc.FilePath);

                }
                catch (Exception ex)
                {
                    Logger.LogError($"FetchXML save logging failed: {ex.Message}");
                }
            });
        }

        void savedHandler(object s, TextDocumentFileActionEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                var a = e.FileActionType;
                bool isSaved = a.HasFlag(FileActionTypes.ContentSavedToDisk);
                if (!isSaved) return;

                var options = FetchXmlOptions.Instance;
                if (options.EnableFetchXmlPreviewWindow && options.FetchXmlExecution == FetchXmlExecutionMode.OnSave)
                {
                    if (textView?.Properties != null && textView.Properties.TryGetProperty(typeof(BrowserMargin), out BrowserMargin margin))
                    {
                        margin?.TriggerFetch(immediate: true);
                    }
                }
            }
            catch { }
        }

        // Detach cleanly when the text document is disposed
        void disposedHandler(object s, TextDocumentEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (e?.TextDocument == doc)
            {
                try { doc.FileActionOccurred -= loadedHandler; } catch { }
                try { doc.FileActionOccurred -= savedHandler; } catch { }
                try { DocumentFactory.TextDocumentDisposed -= disposedHandler; } catch { }
                try { buffer.Properties.RemoveProperty(SubscribedKey); } catch { }
            }
        }

        ThreadHelper.JoinableTaskFactory.Run(async () =>
        {
            // Get the FetchXmlDocument bound to this buffer
            var fetchDoc = buffer.GetFetchXmlDocument(Logger);
            // Wait for parsing to complete; then ensure header/schema attributes
            await WaitForParsedAsync(fetchDoc, TimeSpan.FromSeconds(5));
            await EnsureSchemaHeaderAsync(buffer, doc.FilePath);
        });

        doc.FileActionOccurred += loadedHandler;
        doc.FileActionOccurred += savedHandler;
        DocumentFactory.TextDocumentDisposed += disposedHandler;
    }

    private static async Task WaitForParsedAsync(FetchXmlDocument fetchDoc, TimeSpan timeout)
    {
        if (fetchDoc == null) return;

        // If already parsed and not parsing, return immediately
        if (!fetchDoc.IsParsing && fetchDoc.XmlDocument != null)
            return;

        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        void Handler(FetchXmlDocument _)
        {
            try { tcs.TrySetResult(true); } catch { }
        }

        try
        {
            fetchDoc.Parsed += Handler;

            // In case parsing was already done but XmlDocument is still null (empty file), poll for IsParsing=false
            using var cts = new CancellationTokenSource(timeout);
            try
            {
                while (!cts.IsCancellationRequested)
                {
                    if (!fetchDoc.IsParsing && fetchDoc.XmlDocument != null)
                    {
                        return;
                    }
                    if (tcs.Task.IsCompleted)
                    {
                        return;
                    }
                    await Task.WhenAny(tcs.Task, Task.Delay(50, cts.Token)).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) { }
        }
        finally
        {
            fetchDoc.Parsed -= Handler;
        }
    }

    private async Task EnsureSchemaHeaderAsync(ITextBuffer buffer, string filePath)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        if (buffer == null || string.IsNullOrWhiteSpace(filePath)) return;

        var snapshot = buffer.CurrentSnapshot;
        var text = snapshot.GetText();
        var original = text;

        // Determine newline style
        var newline = text.Contains("\r\n") ? "\r\n" : "\n";

        // 1) Ensure XML declaration with utf-8
        var xmlDeclRegex = new Regex(@"^\uFEFF?\s*<\?xml[^>]*\?>\r?\n?", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        var desiredDecl = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + newline;

        if (xmlDeclRegex.IsMatch(text))
        {
            // Replace entire declaration to normalize
            text = xmlDeclRegex.Replace(text, desiredDecl);
        }
        else
        {
            text = desiredDecl + text;
        }

        // 2) Ensure fetch attributes: xmlns:xsi and xsi:noNamespaceSchemaLocation
        var fetchOpenTagRegex = new Regex("<fetch\\b[^>]*>\r?\n?", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        var match = fetchOpenTagRegex.Match(text);
        if (match.Success)
        {
            var openTag = match.Value;

            string projectRoot = await GetProjectRootAsync(filePath).ConfigureAwait(true) ?? Path.GetDirectoryName(filePath);
            var schemaAbs = Path.Combine(projectRoot ?? string.Empty, ".xrmtools", "schemas", "Fetch.xsd");
            var relativeSchema = PathHelper.GetRelativePath(filePath, schemaAbs, '/');

            openTag = ReplaceOrAddAttribute(openTag, "xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
            openTag = ReplaceOrAddAttribute(openTag, "xsi:noNamespaceSchemaLocation", relativeSchema);

            // Replace in text
            text = text.Substring(0, match.Index) + openTag + text.Substring(match.Index + match.Length);
        }

        if (!string.Equals(original, text, StringComparison.Ordinal))
        {
            using var edit = buffer.CreateEdit();
            edit.Replace(0, snapshot.Length, text);
            edit.Apply();
        }
    }

    private static string ReplaceOrAddAttribute(string openTag, string attrName, string attrValue)
    {
        // Match attribute allowing optional leading whitespace/start; capture the leading part to preserve spacing
        var attrRegex = new Regex($"(?i)(^|\\s)" + Regex.Escape(attrName) + "\\s*=\\s*([\"'])(.*?)\\2");
        if (attrRegex.IsMatch(openTag))
        {
            openTag = attrRegex.Replace(openTag, m =>
            {
                var leading = m.Groups[1].Value; // either start or whitespace
                var quote = m.Groups[2].Value;
                return leading + attrName + "=" + quote + attrValue + quote;
            });
        }
        else
        {
            // Insert before closing '>'
            int insertPos = openTag.LastIndexOf('>');
            if (insertPos > -1)
            {
                openTag = openTag.Insert(insertPos, $" {attrName}=\"{attrValue}\"");
            }
        }
        return openTag;
    }

    private static async Task<string?> GetProjectRootAsync(string filePath)
    {
        // Try to find the containing project via Community.VisualStudio.Toolkit
        var file = await PhysicalFile.FromFileAsync(filePath);
        if (file?.FindParent(SolutionItemType.Project) is Community.VisualStudio.Toolkit.Project proj)
        {
            var fullPath = proj.FullPath; // Usually the project file path
            if (!string.IsNullOrEmpty(fullPath))
            {
                try { return Directory.Exists(fullPath) ? fullPath : Path.GetDirectoryName(fullPath); }
                catch { return Path.GetDirectoryName(fullPath); }
            }
        }
        return null;
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