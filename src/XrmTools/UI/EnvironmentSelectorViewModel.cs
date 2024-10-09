#nullable enable
namespace XrmTools.UI;

using Community.VisualStudio.Toolkit;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Input;
using XrmTools.Options;
using XrmTools.Settings;

internal class EnvironmentSelectorViewModel : ViewModelBase
{
    private DataverseEnvironment _environment;
    private SolutionItem _solutionItem;
    public DataverseEnvironment Environment 
    { 
        get => _environment;
        set => SetProperty(ref _environment, value);
    }
    public ObservableCollection<DataverseEnvironment> Environments { get; }
    public SolutionItem SolutionItem
    {
        get => _solutionItem;
        set => SetProperty(ref _solutionItem, value);
    }
    public SettingsStorageTypes StorageType { get; }

    public ICommand SelectCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand TestCommand { get; }

    private readonly ISettingsProvider settingsProvider;

#pragma warning disable CS8618 // False-positive: _environment is set via Environment property.
    public EnvironmentSelectorViewModel(
#pragma warning restore CS8618 // False-positive: _environment is set via Environment property.
        SettingsStorageTypes storageType,
        SolutionItem solutionItem, 
        ISettingsProvider settingsProvider, 
        Action onSelect, Action onCancel, Action onTest)
    {
        this.settingsProvider = settingsProvider;
        StorageType = storageType;
        SolutionItem = solutionItem;
        Environments = new ObservableCollection<DataverseEnvironment>(GeneralOptions.Instance.Environments);
        Environment = storageType switch
        {
            SettingsStorageTypes.Solution => GetSolutionEnvironment(),
            SettingsStorageTypes.SolutionUser => GetSolutionUserEnvironment(),
            SettingsStorageTypes.Project => GetProjectEnvironment(),
            SettingsStorageTypes.ProjectUser => GetProjectUserEnvironment(),
            _ => GeneralOptions.Instance.CurrentEnvironment
        };
        SelectCommand = new RelayCommand(onSelect);
        CancelCommand = new RelayCommand(onCancel);
        TestCommand = new RelayCommand(onTest);
    }

    private DataverseEnvironment GetSolutionEnvironment()
    {
        var url = settingsProvider.SolutionSettings.EnvironmentUrl();
        return Environments.FirstOrDefault(e => e.Url == url);
    }

    private DataverseEnvironment GetSolutionUserEnvironment()
    {
        var url = settingsProvider.SolutionUserSettings.EnvironmentUrl();
        return Environments.FirstOrDefault(e => e.Url == url);
    }

    private DataverseEnvironment GetProjectEnvironment()
    {
        var url = settingsProvider.ProjectSettings.EnvironmentUrlAsync()
            .ConfigureAwait(false).GetAwaiter().GetResult();
        return Environments.FirstOrDefault(e => e.Url == url);
    }

    private DataverseEnvironment GetProjectUserEnvironment()
    {
        var url = settingsProvider.ProjectUserSettings.EnvironmentUrlAsync()
            .ConfigureAwait(false).GetAwaiter().GetResult();
        return Environments.FirstOrDefault(e => e.Url == url);
    }
}
#nullable restore