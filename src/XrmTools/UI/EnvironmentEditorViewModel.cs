namespace XrmTools.UI;
#nullable enable
using Community.VisualStudio.Toolkit;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using XrmTools.Environments;
using XrmTools.Options;
using XrmTools.Resources;
using XrmTools.Http;
using XrmTools.WebApi.Messages;
using XrmTools.WebApi;

internal class EnvironmentEditorViewModel : ViewModelBase
{
    readonly IEnvironmentProvider _environmentProvider;
    //readonly IRepositoryFactory _repositoryFactory;
    readonly IXrmHttpClientFactory _httpClientFactory;

    public Action? RequestFocusOnName { get; set; }
    public Action? RequestFocusOnUrl { get; set; }

    public ObservableCollection<EnvironmentModel> Environments { get; }

    private EnvironmentModel? _selectedEnvironment;
    public EnvironmentModel? SelectedEnvironment
    {
        get => _selectedEnvironment;
        set => SetProperty(ref _selectedEnvironment, value);
    }

    private string? _testResult;
    public string? TestResult
    {
        get => _testResult;
        set => SetProperty(ref _testResult, value);
    }

    public string? ActiveEnvironmentScopeName { get; set; }

    public ICommand AddEnvironmentCommand { get; }
    public ICommand RemoveEnvironmentCommand { get; }
    public ICommand TestConnectionCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand SetActiveEnvironmentCommand { get; }

    public EnvironmentEditorViewModel() : this(null!, null!) 
    {
        Environments = [
            new EnvironmentModel { Name = "Default Environment", ConnectionString = "https://default.crm.dynamics.com" }
            ];
        SelectedEnvironment = Environments[0];
    }

    public EnvironmentEditorViewModel(IEnvironmentProvider environmentProvider, IXrmHttpClientFactory httpClientFactory)
    {
        _environmentProvider = environmentProvider;
        _httpClientFactory = httpClientFactory;
        Environments = [];
        AddEnvironmentCommand = new RelayCommand(AddEnvironment);
        RemoveEnvironmentCommand = new RelayCommand(RemoveEnvironment, () => SelectedEnvironment != null);
        TestConnectionCommand = new AsyncRelayCommand(TestConnectionAsync, () => SelectedEnvironment != null);
        SetActiveEnvironmentCommand = new RelayCommand<EnvironmentModel>(SetActiveEnvironment);
        SaveCommand = new AsyncRelayCommand<Window>(SaveAsync);
        CancelCommand = new RelayCommand<Window>(Cancel);
    }

    public async Task InitializeAsync(DataverseEnvironment? newEnvironment = null)
    {
        var options = await GeneralOptions.GetLiveInstanceAsync();
        ActiveEnvironmentScopeName = options.CurrentEnvironmentStorage switch
        {
            SettingsStorageTypes.Solution => "Solution (.sln) file",
            SettingsStorageTypes.Project => "Project (.csproj) file",
            SettingsStorageTypes.SolutionUser => "Solution User Options (.suo) file",
            SettingsStorageTypes.ProjectUser => "Project (User)",
            _ => "Global (Visual Studio options)",
        };

        var environments = await _environmentProvider.GetAvailableEnvironmentsAsync();
        var activeEnvironment = await _environmentProvider.GetActiveEnvironmentAsync();
        Environments.Clear();
        foreach (var environment in environments)
        {
            Environments.Add(new EnvironmentModel
            {
                Name = environment.Name,
                ConnectionString = environment.ConnectionString,
                IsChecked = environment.Url == activeEnvironment?.Url
            });
        }

        if (newEnvironment is not null)
        {
            Environments.Add(new EnvironmentModel
            {
                Name = newEnvironment.Name,
                ConnectionString = newEnvironment.ConnectionString
            });
        }

        OnPropertyChanged(nameof(Environments));
        if (Environments.Count == 0)
        {
            AddEnvironment();
        }
        else
        {
            SelectedEnvironment = newEnvironment is null ? Environments[0] : Environments[^1];
        }
    }

    private void AddEnvironment()
    {
        var env = new EnvironmentModel { Name = "New Environment" };
        Environments.Add(env);
        SelectedEnvironment = env;
        RequestFocusOnName?.Invoke();
    }

    private void RemoveEnvironment()
    {
        if (SelectedEnvironment != null)
        {
            var toRemove = SelectedEnvironment;
            Environments.Remove(toRemove);
            SelectedEnvironment = Environments.Count > 0 ? Environments[0] : null;
        }
    }

    private async Task TestConnectionAsync()
    {
        if (SelectedEnvironment == null)
        {
            TestResult = "No environment selected.";
            return;
        }

        if (_httpClientFactory == null) {
            TestResult = "Testing not available.";
            return;
        }
        if (!IsValidEnvironment(SelectedEnvironment))
        {
            TestResult = string.Format(Strings.EnvironmentConnectionStringError, SelectedEnvironment.Name);
            return;
        }
        Meta.Model.WhoAmIResponse? response = null;
        try
        {
            var environment = new DataverseEnvironment
            {
                Name = SelectedEnvironment.Name,
                ConnectionString = SelectedEnvironment.ConnectionString
            };
            using var client = await _httpClientFactory.CreateClientAsync(environment);
            var httpResponse = await client.SendAsync(new WhoAmIRequest());
            if (httpResponse == null)
            {
                TestResult = string.Format(Strings.EnvironmentConnectionError, SelectedEnvironment.Name);
                return;
            }
            response = await httpResponse!.CastAsync<Meta.Model.WhoAmIResponse>();
        }
        catch (Exception ex)
        {
            await VS.MessageBox.ShowWarningAsync("Test Environment", string.Format(Strings.EnvironmentConnectionError, SelectedEnvironment.Name, ex));
            return;
        }
        TestResult = response is not null ?
            string.Format(Strings.EnvironmentConnectionSuccess, SelectedEnvironment.Name) :
            string.Format("WhoAmI request failed.", SelectedEnvironment.Name);
    }

    private bool IsValidEnvironment(EnvironmentModel? environment)
        => environment != null && !string.IsNullOrWhiteSpace(environment.Name) && !string.IsNullOrWhiteSpace(environment.EnvironmentUrl) && Uri.TryCreate(environment.EnvironmentUrl, UriKind.Absolute, out _);

    private void SetActiveEnvironment(EnvironmentModel? environment)
    {
        if (!IsValidEnvironment(environment))
        {
            VS.MessageBox.ShowError("Set Environment in " + ActiveEnvironmentScopeName, "The environment needs to have a valid URL.");
            return;
        }
        foreach (var env in Environments)
        {
            env.IsChecked = env == environment;
        }
    }

    private async Task SaveAsync(Window? window)
    {
        var visitedNames = new HashSet<string>(Environments.Count);
        var options = await GeneralOptions.GetLiveInstanceAsync();
        if (options == null)
        {
            await VS.MessageBox.ShowErrorAsync("Saving environments", "Failed to retrieve existing environment from Visual Studio Options.");
            return;
        }
        foreach(var env in Environments)
        {
            if (!IsValidEnvironment(env))
            {
                SelectedEnvironment = env;
                var errorMessage = string.IsNullOrWhiteSpace(env?.Name) ? "Environment doesn't have a name" : $"The environment \"{env?.Name}\" is not valid or doesn't have a valid URL.";
                await VS.MessageBox.ShowErrorAsync("Saving environments", errorMessage);
                RequestFocusOnUrl?.Invoke();
                return;
            }
            if (!visitedNames.Add(env.Name))
            {
                SelectedEnvironment = env;
                await VS.MessageBox.ShowErrorAsync("Saving environments", $"The environment \"{env?.Name}\" is not unique. All environments should have unique names.");
                RequestFocusOnName?.Invoke();
                return;
            }
        }

        options.Environments.Clear();
        foreach (var env in Environments)
        {
            options.Environments.Add(new DataverseEnvironment
            {
                Name = env.Name,
                ConnectionString = env.ConnectionString
            });
            if (env.IsChecked)
            {
                await _environmentProvider.SetActiveEnvironmentAsync(options.Environments[^1]).ConfigureAwait(false);
            }
        }
        if (SelectedEnvironment != null)
        {
            options.CurrentEnvironment = options.Environments.FirstOrDefault(e => e.Name == SelectedEnvironment.Name) ?? DataverseEnvironment.Empty;
        }
        await options.SaveAsync();

        TestResult = "Environment saved.";

        if (window != null)
        {
            window.DialogResult = true;
            window.Close();
        }
    }

    private void Cancel(Window? window)
    {
        TestResult = "Operation cancelled.";

        if (window != null)
        {
            window.DialogResult = false;
            window.Close();
        }
    }

    public List<string> AuthTypes { get; } =
[
        AuthenticationTypes.ClientSecret,
        AuthenticationTypes.Certificate,
        AuthenticationTypes.Interactive
    ];
}

internal class AuthenticationTypes
{
    public const string ClientSecret = "Client Secret";
    public const string Certificate = "Certificate";
    public const string Interactive = "Interactive";
}
#nullable restore