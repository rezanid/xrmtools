namespace XrmTools.UI;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using XrmTools.Core.Helpers;
using XrmTools.Core.Repositories;
using XrmTools.Meta.Model.Configuration;
using XrmTools.Xrm;
using XrmTools.Xrm.Repositories;

internal class AssemblySelectionViewModel : ViewModelBase
{
    //private readonly IPluginAssemblyRepository _assemblyRepository;
    //private readonly IPluginTypeRepository _typeRepository;
    private readonly IRepositoryFactory _repositoryFactory;
    private readonly IXrmSchemaProvider _schemaProvider;
    private PluginAssemblyConfig _selectedAssembly;
    private object _generatedCode;
    private string _filterText;
    private bool _isLoading;

    public ObservableCollection<PluginAssemblyConfig> Assemblies { get; }
    public ICollectionView FilteredAssemblies { get; }
    public ObservableCollection<PluginTypeConfig> SelectedPluginTypes { get; }
    public PluginAssemblyConfig SelectedAssembly
    {
        get => _selectedAssembly;
        set
        {
            if (SetProperty(ref _selectedAssembly, value))
            {
                OnSelectedAssemblyChanged();
            }
        }
    }
    public object GeneratedCode
    {
        get => _generatedCode;
        set => SetProperty(ref _generatedCode, value);
    }
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }
    public string FilterText
    {
        get => _filterText;
        set
        {
            if (SetProperty(ref _filterText, value))
            {
                FilteredAssemblies.Refresh();
            }
            _filterText = value;
        }
    }
    public string FileName { get; set; }
    public IAsyncRelayCommand LoadAssembliesCommand { get; }
    public IAsyncRelayCommand LoadAssemblyDetailsCommand { get; }
    public ICommand ChooseAssemblyCommand { get; }
    public ICommand GenerateCodeCommand { get; }

    public AssemblySelectionViewModel(IXrmSchemaProvider schemaProvider)
    {
        _schemaProvider = schemaProvider;
        Assemblies = [];
        FilteredAssemblies = CollectionViewSource.GetDefaultView(Assemblies);
        FilteredAssemblies.Filter = FilterAssemblies;
        //SelectedPluginTypes = [];

        LoadAssembliesCommand = new AsyncRelayCommand(LoadAssembliesAsync);
        LoadAssemblyDetailsCommand = new AsyncRelayCommand(LoadAssemblyDetailsAsync, CanSelectAssembly);
        ChooseAssemblyCommand = new RelayCommand(SelectAssembly, CanSelectAssembly);
        GenerateCodeCommand = new RelayCommand<object>(Serialize);
    }

    internal AssemblySelectionViewModel(IRepositoryFactory repositoryFactory)
    {
        //_assemblyRepository = assemblyRepository;
        //_typeRepository = typeRepository;
        _repositoryFactory = repositoryFactory;
        Assemblies = [];
        FilteredAssemblies = CollectionViewSource.GetDefaultView(Assemblies);
        FilteredAssemblies.Filter = FilterAssemblies;
        //SelectedPluginTypes = [];

        LoadAssembliesCommand = new AsyncRelayCommand(LoadAssembliesAsync);
        LoadAssemblyDetailsCommand = new AsyncRelayCommand(LoadAssemblyDetailsAsync, CanSelectAssembly);
        ChooseAssemblyCommand = new RelayCommand(SelectAssembly, CanSelectAssembly);
        GenerateCodeCommand = new RelayCommand<object>(Serialize);
    }

    private bool FilterAssemblies(object item)
    {
        if (item is PluginAssemblyConfig assembly)
        {
            return string.IsNullOrWhiteSpace(FilterText) || assembly.Name.IndexOf(FilterText, StringComparison.OrdinalIgnoreCase) >= 0;
        }
        return false;
    }

    private async Task LoadAssembliesAsync()
    {
        IsLoading = true;
        using CancellationTokenSource cancellationTokenSource = new(60000);
        IEnumerable<PluginAssemblyConfig> assemblies = null;
        if (_repositoryFactory != null)
        {
            using var assemblyRepository = _repositoryFactory.CreateRepository<IPluginAssemblyRepository>();
            assemblies = await assemblyRepository.GetAsync(cancellationTokenSource.Token);
        }
        else
        {
            assemblies = await _schemaProvider.GetPluginAssembliesAsync(cancellationTokenSource.Token);
        }
        Assemblies.Clear();
        foreach (var assembly in assemblies)
        {
            Assemblies.Add(assembly);
        }
        FilteredAssemblies.Refresh();
        IsLoading = false;
    }

    private async Task LoadAssemblyDetailsAsync()
    {
        if (SelectedAssembly != null && SelectedAssembly.PluginTypes.Count == 0)
        {
            IsLoading = true;
            using CancellationTokenSource cancellationTokenSource = new(10000);
            IEnumerable<PluginTypeConfig> pluginTypes = null;
            if (_repositoryFactory != null)
            {
                using var typeRepository = _repositoryFactory.CreateRepository<IPluginTypeRepository>();
                pluginTypes = await typeRepository.GetAsync(SelectedAssembly.Id.Value, cancellationTokenSource.Token);
            }
            else
            {
                pluginTypes = await _schemaProvider.GetPluginTypesAsync(SelectedAssembly.Id.Value, cancellationTokenSource.Token);
            }
            SelectedAssembly.PluginTypes = new ObservableCollection<PluginTypeConfig>(pluginTypes);
            Serialize(SelectedAssembly);
            IsLoading = false;
        }
    }

    private void Serialize(object input)
    {
        if (input is null) { return; }
        GeneratedCode = "JSON Serialization not supported anymore. In future C# code preview will be displated.";// StringHelper.SerializeJson(input);
    }

    private bool CanSelectAssembly() => SelectedAssembly != null;

    private void SelectAssembly() { }

    private void OnSelectedAssemblyChanged()
    {
        // Notify that CanExecute changed for SelectCommand
        ((RelayCommand)ChooseAssemblyCommand).NotifyCanExecuteChanged();
        LoadAssemblyDetailsCommand.Execute(null);
    }
}