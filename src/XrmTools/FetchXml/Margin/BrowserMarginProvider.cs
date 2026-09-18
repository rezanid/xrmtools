namespace XrmTools.FetchXml.Margin;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
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
internal class PreviewMarginVerticalProvider(
    IWebApiService webApi,
    IRepositoryFactory repositoryFactory,
    ILogger<PreviewMarginVerticalProvider> logger) : IWpfTextViewMarginProvider
{
    public IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin marginContainer)
    {
        var margin = wpfTextViewHost.TextView.Properties.GetOrCreateSingletonProperty(
            () => new BrowserMargin(wpfTextViewHost.TextView, webApi, repositoryFactory, logger));
        if (margin.PreviewLocation == FetchXmlPreviewLocation.Horizontal)
        {
            return null;
        }

        return margin;
    }
}

[Export(typeof(IWpfTextViewMarginProvider))]
[Name(nameof(PreviewMarginHorizontalProvider))]
[Order(After = PredefinedMarginNames.BottomControl)]
[MarginContainer(PredefinedMarginNames.Bottom)]
[ContentType(FetchXmlContentTypeDefinitions.ContentTypeName)]
[TextViewRole(PredefinedTextViewRoles.Debuggable)] // This is to prevent the margin from loading in the diff view
[method:ImportingConstructor]
internal class PreviewMarginHorizontalProvider(
    IWebApiService webApi,
    IRepositoryFactory repositoryFactory,
    ILogger<PreviewMarginHorizontalProvider> logger) : IWpfTextViewMarginProvider
{
    public IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin marginContainer)
    {
        var margin = wpfTextViewHost.TextView.Properties.GetOrCreateSingletonProperty(
            () => new BrowserMargin(wpfTextViewHost.TextView, webApi, repositoryFactory, logger));
        if (margin.PreviewLocation == FetchXmlPreviewLocation.Vertical)
        {
            return null;
        }

        return margin;
    }
}