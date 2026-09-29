#nullable enable
namespace XrmTools.UI;

using Community.VisualStudio.Toolkit;
using XrmTools.Shell.Controls;
using Microsoft.VisualStudio.Shell;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using XrmTools.DataverseSolutions;
using XrmTools.Xrm.Repositories;

internal partial class DataverseSolutionProjectDialog : DialogWindow
{
    private Func<DataverseSolutionProjectCreationRequest, CancellationToken, Task<string>> createProject = null!;
    private CancellationToken cancellationToken;
    private string? projectFilePath;
    private DataverseSolutionProjectDialog()
    {
        InitializeComponent();
    }

    internal static async Task<string?> ShowDialogAsync(
        string initialParentDirectory,
        IRepositoryFactory repositoryFactory,
        Func<DataverseSolutionProjectCreationRequest, CancellationToken, Task<string>> createProject,
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
            DataContext = viewModel,
            createProject = createProject,
            cancellationToken = cancellationToken
        };

        return dialog.ShowModal() == true ? dialog.projectFilePath : null;
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

        if (dialog.ShowDialog(new DialogOwner(new WindowInteropHelper(this).Handle)) == System.Windows.Forms.DialogResult.OK)
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

        projectFilePath = await viewModel.CreateAsync(createProject, cancellationToken);
        if (projectFilePath != null)
            DialogResult = true;
    }

    private sealed class DialogOwner(IntPtr handle) : System.Windows.Forms.IWin32Window
    {
        public IntPtr Handle { get; } = handle;
    }
}
#nullable restore
