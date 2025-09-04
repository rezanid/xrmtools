#nullable enable
namespace XrmTools.UI.InfoBars;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using System.Threading.Tasks;

internal sealed class NugetUpdateInfoBar
{
    const string UpdateText = "Update…";
    const string DismissText = "Dismiss";

    private static InfoBar? InfoBarInstance;

    public required string PromptMessage { get; set; }

    public async Task TryShowAsync()
    {
        Close();

        var model = new InfoBarModel(
            [
                new InfoBarTextSpan(PromptMessage + " "),
                new InfoBarHyperlink(UpdateText),
                new InfoBarTextSpan("  "),
                new InfoBarHyperlink(DismissText)
            ],
            KnownMonikers.IntellisenseWarning,
            true);
        InfoBarInstance = await VS.InfoBar.CreateAsync(model);
        if (InfoBarInstance is null)
        {
            return;
        }
        InfoBarInstance.ActionItemClicked += (sender, args) =>
        {
            try
            {
                ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    if (args.ActionItem is InfoBarHyperlink link)
                    {
                        if (link.Text == UpdateText)
                        {
                            // Open the solution-level NuGet UI with a pre-filled search
                            var dte = await VS.GetServiceAsync<EnvDTE.DTE, EnvDTE.DTE>();
                            dte?.ExecuteCommand("Tools.ManageNuGetPackagesforSolution");
                        }
                        InfoBarInstance.Close();
                    }
                });
            }
            catch { }
        };
        await InfoBarInstance.TryShowInfoBarUIAsync();
    }

    public static void Close()
    {
        InfoBarInstance?.Close();
        InfoBarInstance = null;
    }
}
#nullable restore