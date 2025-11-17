#nullable enable
namespace XrmTools.UI.InfoBars;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using System.Threading.Tasks;
using XrmTools.UI;

internal sealed class FoundNewEnvironmentInfoBar
{
    const string AddEnvironmentText = "Add Environment";

    private static InfoBar? InfoBarInstance;

    public required string PromptMessage { get; set; }
    public required string EnvironmentUrl { get; set; }

    public async Task TryShowAsync()
    {
        Close();

        var model = new InfoBarModel(
            [
                new InfoBarTextSpan(PromptMessage),
                new InfoBarHyperlink(AddEnvironmentText)
            ],
            KnownMonikers.Environment,
            true);

        var infoBar = await VS.InfoBar.CreateAsync(model);
        if (infoBar is null)
        {
            return;
        }
        infoBar.ActionItemClicked += (sender, args) =>
        {
            try
            {
                ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    var componentModel = await VS.GetServiceAsync<SComponentModel, IComponentModel>();
                    if (componentModel is null)
                    {
                        return;
                    }
                    var envEditor = componentModel.GetService<IEnvironmentEditor>();
                    if (envEditor is not null)
                    {
                        var result = await envEditor.EditEnvironmentsAsync(new DataverseEnvironment { ConnectionString = EnvironmentUrl });
                        if (result)
                        {
                            infoBar.Close();
                        }
                    }
                });
            }
            catch { }
        };
        await infoBar.TryShowInfoBarUIAsync();
    }

    public static void Close()
    {
        InfoBarInstance?.Close();
        InfoBarInstance = null;
    }
}
#nullable restore