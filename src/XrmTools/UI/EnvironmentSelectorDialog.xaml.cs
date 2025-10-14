#nullable enable
namespace XrmTools.UI;

using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using XrmTools.Logging.Compatibility;
using XrmTools.Options;
using XrmTools.Settings;
using XrmTools.Xrm.Repositories;

/// <summary>
/// Interaction logic for EnvironmentSelectorDialog.xaml
/// </summary>
internal partial class EnvironmentSelectorDialog : DialogWindow
{
    private EnvironmentSelectorDialog()//(object dataContext)
    {
        EnsureReferencedAssembliesInMarkupAreLoaded();
        InitializeComponent();
    }

    internal static async Task<DataverseEnvironment?> ShowDialogAsync(
        SettingsStorageTypes storageType, ISettingsProvider settingsProvider, SolutionItem solutionItem, IRepositoryFactory? repositoryFactory, ILogger? logger)
    {
        var dialog = new EnvironmentSelectorDialog();

        var viewModel =  new EnvironmentSelectorViewModel(storageType, solutionItem, settingsProvider, dialog.OnSelect, dialog.OnCancel, repositoryFactory, logger);
        await viewModel.InitializeAsync();

        dialog.DataContext = viewModel;

        if (dialog.ShowModal() == true)
        {
            return viewModel.Environment;
        }
        return null;
    }

    private void OnSelect()
    {
        DialogResult = true;
        Close();
    }

    private void OnCancel()
    {
        DialogResult = false;
        Close();
    }

    private void EnsureReferencedAssembliesInMarkupAreLoaded()
    {
        var requiredAssemblyNames = new[] { "Microsoft.Xaml.Behaviors", "XrmTools.UI.Controls" };
        var loadedAssemblyNames = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetName().Name);
        var notLoadedAssemblyNames = requiredAssemblyNames.Except(loadedAssemblyNames).ToList();
        notLoadedAssemblyNames.ForEach(a => Assembly.Load(a));
    }
}
#nullable restore