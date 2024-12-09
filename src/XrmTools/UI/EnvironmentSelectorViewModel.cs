#nullable enable
namespace XrmTools.UI;

using Community.VisualStudio.Toolkit;
using CommunityToolkit.Mvvm.Input;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using XrmTools.Options;
using XrmTools.Resources;
using XrmTools.Settings;
using XrmTools.Xrm;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Xrm.Repositories;

internal class EnvironmentSelectorViewModel : ViewModelBase
{
    private readonly IRepositoryFactory? repositoryFactory;
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
    public IRelayCommand<DataverseEnvironment> TestCommand { get; }

    private readonly ISettingsProvider settingsProvider;

#pragma warning disable CS8618 // False-positive: _environment is set via Environment property.
    public EnvironmentSelectorViewModel(
#pragma warning restore CS8618 // False-positive: _environment is set via Environment property.
        SettingsStorageTypes storageType,
        SolutionItem solutionItem, 
        ISettingsProvider settingsProvider, 
        Action onSelect, 
        Action onCancel, 
        IRepositoryFactory? repositoryFactory)
    {
        this.repositoryFactory = repositoryFactory;
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
        TestCommand = new AsyncRelayCommand<DataverseEnvironment>(TestEnvironmentAsync); 
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

    private async Task TestEnvironmentAsync(DataverseEnvironment? environment)
    {
        if (environment is null)
        {
            _ = await VS.MessageBox.ShowAsync("No environment selected", "Please select and environment first.", OLEMSGICON.OLEMSGICON_WARNING, OLEMSGBUTTON.OLEMSGBUTTON_OK);
            return;
        }
        if (repositoryFactory is null)
        {
            _ = await VS.MessageBox.ShowAsync("Testing connection not available", "Testing connection is not available currently, please try again later.", OLEMSGICON.OLEMSGICON_WARNING, OLEMSGBUTTON.OLEMSGBUTTON_OK);
            return;
        }

        var (success, message) = await TestConnectionAsync(environment);

        if (success)
        {
            _ = VS.MessageBox.ShowAsync("Test Successful", message, OLEMSGICON.OLEMSGICON_INFO, OLEMSGBUTTON.OLEMSGBUTTON_OK);
        }
        else
        {
            _ = VS.MessageBox.ShowAsync("Test Failed", message, OLEMSGICON.OLEMSGICON_WARNING, OLEMSGBUTTON.OLEMSGBUTTON_OK);
        }
    }

    private async Task<(bool, string)> TestConnectionAsync(DataverseEnvironment environment)
    {
        if (repositoryFactory == null) return (false, "Testing not available.");
        if (environment is null) return (false, "Environment is not selected. Please select an environment first.");
        if (!environment.IsValid) return (false, string.Format(Strings.EnvironmentConnectionStringError, environment.Name));
        try
        {
            using var systemRepository = await repositoryFactory.CreateRepositoryAsync<ISystemRepository>(environment);
            var response = await systemRepository.WhoAmIAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            return (false, string.Format(Strings.EnvironmentConnectionError, environment, ex));
        }
        return (true, string.Format(Strings.EnvironmentConnectionSuccess, environment));
    }
}
#nullable restore