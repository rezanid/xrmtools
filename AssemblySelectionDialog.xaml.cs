namespace XrmGen;

using Microsoft.VisualStudio.PlatformUI;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using XrmGen.Xrm;
using XrmGen.Xrm.Model;

public partial class AssemblySelectionDialog : DialogWindow
{
    private readonly IXrmSchemaProvider schemaProvider;

    public PluginAssemblyConfig SelectedAssembly { get; private set; }

    public AssemblySelectionDialog(IXrmSchemaProvider schemaProvider)
    {
        InitializeComponent();
        this.schemaProvider = schemaProvider;
        var viewModel = new AssemblySelectionViewModel();
        DataContext = viewModel;
        LoadAssemblies(viewModel);
    }

    private async void LoadAssemblies(AssemblySelectionViewModel viewModel)
    {
        using CancellationTokenSource cancellationTokenSource = new(10000);
        var assemblies = new ObservableCollection<PluginAssemblyConfig>(await schemaProvider.GetPluginAssembliesAsync(cancellationTokenSource.Token));
        viewModel.Assemblies = new ObservableCollection<PluginAssemblyConfig>();
    }

    private void SelectButton_Click(object sender, RoutedEventArgs e)
    {
        if (AssembliesListBox.SelectedItem is PluginAssemblyConfig selectedAssembly)
        {
            SelectedAssembly = selectedAssembly;
            DialogResult = true;
        }
        else
        {
            MessageBox.Show("Please select an assembly.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
