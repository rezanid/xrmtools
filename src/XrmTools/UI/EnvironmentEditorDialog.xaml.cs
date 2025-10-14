#nullable enable
namespace XrmTools.UI;

using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using System;
using System.Linq;
using System.Reflection;

/// <summary>
/// Interaction logic for EnvironmentEditorDialog.xaml
/// </summary>
public partial class EnvironmentEditorDialog : DialogWindow
{
    public EnvironmentEditorDialog()
    {
        EnsureReferencedAssembliesInMarkupAreLoaded();
        InitializeComponent();

        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        var vm = (EnvironmentEditorViewModel)DataContext;
        vm.RequestFocusOnName = () => ThreadHelper.JoinableTaskFactory.Run(async () =>
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            NameTextBox.Focus();
            NameTextBox.SelectAll();
        });
        vm.RequestFocusOnUrl = () => ThreadHelper.JoinableTaskFactory.Run(async () =>
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            UrlTextBox.Focus();
            UrlTextBox.SelectAll();
        });
        if (vm.Environments.Count == 0)
        {
            vm.AddEnvironmentCommand.Execute(null);
        }
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