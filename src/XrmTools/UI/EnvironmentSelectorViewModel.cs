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
using XrmTools.Logging.Compatibility;
using XrmTools.WebApi.Messages;

internal class EnvironmentSelectorViewModel : ViewModelBase
{
    private readonly IRepositoryFactory? repositoryFactory;
    private readonly ILogger? logger;
    private DataverseEnvironment? _environment;
    private SolutionItem _solutionItem = null!;
    public DataverseEnvironment? Environment 
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

    public EnvironmentSelectorViewModel(
        SettingsStorageTypes storageType,
        SolutionItem solutionItem, 
        ISettingsProvider settingsProvider, 
        Action onSelect, 
        Action onCancel, 
        IRepositoryFactory? repositoryFactory,
        ILogger? logger)
    {
        this.repositoryFactory = repositoryFactory;
        this.settingsProvider = settingsProvider;
        this.logger = logger;
        StorageType = storageType;
        SolutionItem = solutionItem;
        Environments = [];
        SelectCommand = new RelayCommand(onSelect);
        CancelCommand = new RelayCommand(onCancel);
        TestCommand = new AsyncRelayCommand<DataverseEnvironment>(TestEnvironmentAsync); 
    }

    public async Task InitializeAsync()
    {
        var options = await GeneralOptions.GetLiveInstanceAsync().ConfigureAwait(false);
        var environments = new ObservableCollection<DataverseEnvironment>(GeneralOptions.Instance.Environments);
        Environments.Clear();
        foreach (var env in environments)
        {
            Environments.Add(env);
        }
        Environment = StorageType switch
        {
            SettingsStorageTypes.Solution => GetSolutionEnvironment(),
            SettingsStorageTypes.SolutionUser => GetSolutionUserEnvironment(),
            SettingsStorageTypes.Project => await GetProjectEnvironmentAsync(),
            SettingsStorageTypes.ProjectUser => await GetProjectUserEnvironmentAsync(),
            _ => options.CurrentEnvironment
        };
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
    private async Task<DataverseEnvironment> GetProjectEnvironmentAsync()
    {
        var url = await settingsProvider.ProjectSettings.EnvironmentUrlAsync()
            .ConfigureAwait(false);
        return Environments.FirstOrDefault(e => e.Url == url);
    }
    private async Task<DataverseEnvironment> GetProjectUserEnvironmentAsync()
    {
        var url = await settingsProvider.ProjectUserSettings.EnvironmentUrlAsync()
            .ConfigureAwait(false);
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
            logger.LogWarning(message);
            _ = VS.MessageBox.ShowAsync(Strings.EnvironmentConnectionErrorUITitle, string.Format(Strings.EnvironmentConnectionErrorUIDesc, environment), OLEMSGICON.OLEMSGICON_WARNING, OLEMSGBUTTON.OLEMSGBUTTON_OK);
        }
    }

    private async Task<(bool, string)> TestConnectionAsync(DataverseEnvironment environment)
    {
        if (repositoryFactory == null) return (false, "Testing not available.");
        if (environment is null) return (false, "Environment is not selected. Please select an environment first.");
        if (!environment.IsValid) return (false, string.Format(Strings.EnvironmentConnectionStringError, environment.Name));
        WhoAmIResponse? response = null;
        try
        {
            using var systemRepository = await repositoryFactory.CreateRepositoryAsync<ISystemRepository>(environment);
            response = await systemRepository.WhoAmIAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            return (false, string.Format(Strings.EnvironmentConnectionError, environment, ex));
        }
        return response is not null ?
            (true, string.Format(Strings.EnvironmentConnectionSuccess, environment)) :
            (false, string.Format("WhoAmI request failed.", environment));
    }

    //TODO: Add more tests here:

}
#nullable restore