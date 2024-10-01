#nullable enable
namespace XrmTools.UI;

using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Linq;
using System.Reflection;

/// <summary>
/// Interaction logic for EnvironmentSelectorDialog.xaml
/// </summary>
internal partial class EnvironmentSelectorDialog : DialogWindow
{
    internal EnvironmentSelectorDialog(ISettingsProvider settingsProvider, SolutionItem solutionItem, bool userMode)
    {
        EnsureReferencedAssembliesInMarkupAreLoaded();
        DataContext = new EnvironmentSelectorViewModel(solutionItem, settingsProvider, OnSelect, OnCancel, OnTest, userMode);
        InitializeComponent();
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

    private void OnTest()
    {
        VS.MessageBox.Show("TEST", "TEST", OLEMSGICON.OLEMSGICON_INFO);
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