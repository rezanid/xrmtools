#nullable enable
namespace XrmTools.FetchXml.Completion;

using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Text.Editor;
using XrmTools.FetchXml;
using XrmTools.Logging;
using XrmTools.Xrm.Repositories;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.LanguageServices;

/// <summary>
/// MEF entry point for FetchXML async completion.
/// Creates a per-textView completion source.
/// </summary>
[Export(typeof(IAsyncCompletionSourceProvider))]
[Name(nameof(FetchXmlCompletionProvider))]
[ContentType(FetchXmlContentTypeDefinitions.ContentTypeName)]
[method: ImportingConstructor]
internal sealed class FetchXmlCompletionProvider(
    [Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider,
    [Import] IOutputLoggerService logger,
    [Import] IRepositoryFactory repositoryFactory) : IAsyncCompletionSourceProvider
{
    public IAsyncCompletionSource? GetOrCreate(ITextView textView)
    {
        if (textView == null) return null;
        return textView.Properties.GetOrCreateSingletonProperty(() =>
        {
            var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
            var workspace = componentModel.GetService<VisualStudioWorkspace>();
            return new FetchXmlCompletionSource(logger, repositoryFactory, workspace);
        });
    }
}
#nullable restore
