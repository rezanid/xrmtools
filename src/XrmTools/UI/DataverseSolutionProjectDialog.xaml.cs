#nullable enable
namespace XrmTools.UI;

using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using XrmTools.DataverseSolutions;
using XrmTools.Xrm.Repositories;

internal partial class DataverseSolutionProjectDialog : DialogWindow
{
    private DataverseSolutionProjectDialog()
    {
        InitializeComponent();
    }

    internal static async Task<DataverseSolutionProjectCreationRequest?> ShowDialogAsync(
        string initialParentDirectory,
        IRepositoryFactory repositoryFactory,
        CancellationToken cancellationToken)
    {
        var viewModel = new DataverseSolutionProjectDialogViewModel(initialParentDirectory);
        try
        {
            var repository = repositoryFactory.CreateRepository<ISolutionRepository>();
            var solutions = await repository.GetUnmanagedAsync(cancellationToken).ConfigureAwait(false);
            viewModel.SetSolutions(solutions);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            viewModel.SetSolutionLoadError($"Could not load unmanaged solutions: {ex.Message}");
        }

        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
        var dialog = new DataverseSolutionProjectDialog
        {
            DataContext = viewModel
        };

        return dialog.ShowModal() == true
            ? viewModel.TryCreateRequest(out var request, out _) ? request : null
            : null;
    }

    private void OnBrowseClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is not DataverseSolutionProjectDialogViewModel viewModel)
        {
            return;
        }

        using var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "Select the parent folder for the Dataverse solution project",
            SelectedPath = viewModel.ParentDirectory,
            ShowNewFolderButton = true
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            viewModel.ParentDirectory = dialog.SelectedPath;
        }
    }

    private async void OnCreateClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is not DataverseSolutionProjectDialogViewModel viewModel)
        {
            return;
        }

        if (!viewModel.TryCreateRequest(out _, out var validationError))
        {
            await VS.MessageBox.ShowErrorAsync("Add Dataverse Solution Project", validationError);
            return;
        }

        DialogResult = true;
        Close();
    }
}
#nullable restore
