#nullable enable
namespace XrmTools.UI;

using XrmTools.Shell.Controls;
using System;
using System.Windows.Threading;

/// <summary>
/// Interaction logic for EnvironmentEditorDialog.xaml
/// </summary>
public partial class EnvironmentEditorDialog : DialogWindow
{
    public EnvironmentEditorDialog()
    {
        InitializeComponent();

        Loaded += OnLoaded;
        Closed += (_, _) =>
        {
            if (DataContext is EnvironmentEditorViewModel viewModel)
            {
                viewModel.RequestFocusOnName = null;
                viewModel.RequestFocusOnUrl = null;
            }
        };
    }

    private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is not EnvironmentEditorViewModel vm) return;
        // Let bindings re-enable and lay out the editor after an async validation attempt.
        vm.RequestFocusOnName = () => Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
        {
            NameTextBox.Focus();
            NameTextBox.SelectAll();
        }));
        vm.RequestFocusOnUrl = () => Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
        {
            UrlTextBox.Focus();
            UrlTextBox.SelectAll();
        }));
        if (vm.Environments.Count == 0)
        {
            vm.AddEnvironmentCommand.Execute(null);
        }
    }

}
#nullable restore
