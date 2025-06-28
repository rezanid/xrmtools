#nullable enable
namespace XrmTools.UI;

using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using XrmTools.Environments;
using XrmTools.Http;

internal interface IEnvironmentEditor
{
    /// <summary>
    /// Shows the Environment Editor dialog and returns true if the user saved changes.
    /// </summary>
    /// <returns>True if saved, false if cancelled.</returns>
    Task<bool> EditEnvironmentsAsync(DataverseEnvironment? newEnvironment = null);
}

[Export(typeof(IEnvironmentEditor))]
internal class EnvironmentEditor : IEnvironmentEditor
{
    public event EventHandler? EnvironmentsChanged;

    [Import]
    public IEnvironmentProvider EnvironmentProvider { get; set; } = null!;

    [Import]
    public IXrmHttpClientFactory HttpClientFactory { get; set; } = null!;

    public async Task<bool> EditEnvironmentsAsync(DataverseEnvironment? newEnvironment = null)
    {
        var viewModel = new EnvironmentEditorViewModel(EnvironmentProvider, HttpClientFactory);
        await viewModel.InitializeAsync(newEnvironment);

        var dialog = new EnvironmentEditorDialog
        {
            DataContext = viewModel
        };
        var result = dialog.ShowDialog();
        if (result == true)
        {
            EnvironmentsChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }
        return false;
    }
}
#nullable restore