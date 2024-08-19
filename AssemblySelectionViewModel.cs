using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using XrmGen.Xrm;
using XrmGen.Xrm.Model;

namespace XrmGen;

public class AssemblySelectionViewModel : INotifyPropertyChanged
{
    private string _filterText;
    private ObservableCollection<PluginAssemblyConfig> _assemblies;
    private ICollectionView _filteredAssemblies;

    public string FilterText
    {
        get => _filterText;
        set
        {
            _filterText = value;
            OnPropertyChanged(nameof(FilterText));
            FilterAssemblies();
        }
    }

    public ObservableCollection<PluginAssemblyConfig> Assemblies
    {
        get => _assemblies;
        set
        {
            _assemblies = value;
            OnPropertyChanged(nameof(Assemblies));
            FilteredAssemblies = CollectionViewSource.GetDefaultView(_assemblies);
            FilterAssemblies();
        }
    }

    public ICollectionView FilteredAssemblies
    {
        get => _filteredAssemblies;
        private set
        {
            _filteredAssemblies = value;
            OnPropertyChanged(nameof(FilteredAssemblies));
        }
    }

    public PluginAssemblyConfig CurrentAssembly { get; set; }

    public AssemblySelectionViewModel()
    {
        Assemblies = [];
    }

    private void FilterAssemblies()
    {
        if (FilteredAssemblies == null) return;

        FilteredAssemblies.Filter = item =>
        {
            if (item is PluginAssemblyConfig assembly)
            {
                return string.IsNullOrEmpty(FilterText) || assembly.Name.Contains(FilterText);
            }
            return false;
        };
        FilteredAssemblies.Refresh();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName) 
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}