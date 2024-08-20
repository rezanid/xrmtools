#nullable enable
namespace XrmGen.UI;

using Microsoft.VisualStudio.PlatformUI;
using System.Windows;
using XrmGen.UI;
using XrmGen.Xrm;

public partial class AssemblySelectionDialog : DialogWindow
{
    public AssemblySelectionDialog(IXrmSchemaProvider schemaProvider)
    {
        InitializeComponent();
        DataContext = new AssemblySelectionViewModel(schemaProvider);
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;
        ((AssemblySelectionViewModel)DataContext).LoadAssembliesCommand.Execute(null);
    }

    private void OnSelectClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }
}
#nullable restore