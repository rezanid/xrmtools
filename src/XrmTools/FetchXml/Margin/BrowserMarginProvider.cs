namespace XrmTools.FetchXml.Margin;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;
using XrmTools.Logging.Compatibility;
using XrmTools.Options;
using XrmTools.WebApi;
using XrmTools.Xrm.Repositories;

[Export(typeof(IWpfTextViewMarginProvider))]
[Name(nameof(PreviewMarginVerticalProvider))]
[Order(After = PredefinedMarginNames.RightControl)]
[MarginContainer(PredefinedMarginNames.Right)]
[ContentType(FetchXmlContentTypeDefinitions.ContentTypeName)]
[TextViewRole(PredefinedTextViewRoles.Debuggable)] // This is to prevent the margin from loading in the diff view
[method:ImportingConstructor]
internal class PreviewMarginVerticalProvider(IWebApiService webApi, IRepositoryFactory repositoryFactory, ILogger<PreviewMarginVerticalProvider> logger) : IWpfTextViewMarginProvider
{
    private BrowserMargin _browserMargin;

    public IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin marginContainer)
    {
        if (FetchXmlOptions.Instance.PreviewWindowLocation == FetchXmlPreviewLocation.Horizontal)
        {
            return null;
        }

        FetchXmlOptions.Saved += Options_Saved;
        wpfTextViewHost.Closed += OnWpfTextViewHostClosed;
        _browserMargin = new BrowserMargin(wpfTextViewHost.TextView, webApi, repositoryFactory, logger);

        return wpfTextViewHost.TextView.Properties.GetOrCreateSingletonProperty(() => _browserMargin);
    }

    private void OnWpfTextViewHostClosed(object sender, EventArgs e)
    {
        IWpfTextViewHost host = (IWpfTextViewHost)sender;
        host.Closed -= OnWpfTextViewHostClosed;
        FetchXmlOptions.Saved -= Options_Saved;
    }

    private void Options_Saved(FetchXmlOptions options)
    {
        _browserMargin?.RefreshAsync().FireAndForget();
    }
}

[Export(typeof(IWpfTextViewMarginProvider))]
[Name(nameof(PreviewMarginHorizontalProvider))]
[Order(After = PredefinedMarginNames.BottomControl)]
[MarginContainer(PredefinedMarginNames.Bottom)]
[ContentType(FetchXmlContentTypeDefinitions.ContentTypeName)]
[TextViewRole(PredefinedTextViewRoles.Debuggable)] // This is to prevent the margin from loading in the diff view
[method:ImportingConstructor]
internal class PreviewMarginHorizontalProvider(IWebApiService webApi, IRepositoryFactory repositoryFactory, ILogger<PreviewMarginHorizontalProvider> logger) : IWpfTextViewMarginProvider
{
    private BrowserMargin _browserMargin;

    public IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin marginContainer)
    {
        if (FetchXmlOptions.Instance.PreviewWindowLocation == FetchXmlPreviewLocation.Vertical)
        {
            return null;
        }

        FetchXmlOptions.Saved += Options_Saved;
        wpfTextViewHost.Closed += OnWpfTextViewHostClosed;
        _browserMargin = new BrowserMargin(wpfTextViewHost.TextView, webApi, repositoryFactory, logger);

        return wpfTextViewHost.TextView.Properties.GetOrCreateSingletonProperty(() => _browserMargin);
    }

    private void OnWpfTextViewHostClosed(object sender, EventArgs e)
    {
        IWpfTextViewHost host = (IWpfTextViewHost)sender;
        host.Closed -= OnWpfTextViewHostClosed;
        FetchXmlOptions.Saved -= Options_Saved;
    }

    private void Options_Saved(FetchXmlOptions options)
    {
        _browserMargin?.RefreshAsync().FireAndForget();
    }
}