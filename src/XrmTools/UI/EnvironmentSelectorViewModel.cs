namespace XrmTools.UI;

using Community.VisualStudio.Toolkit;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using XrmTools.Options;

internal class EnvironmentSelectorViewModel : ViewModelBase
{
    public DataverseEnvironment Environment { get; set; }
    public ObservableCollection<DataverseEnvironment> Environments { get; }
    public SolutionItem SolutionItem { get; set; }

    public ICommand SelectCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand TestCommand { get; }

    private readonly ISettingsProvider settingsProvider;

    public EnvironmentSelectorViewModel(
        SolutionItem solutionItem, 
        ISettingsProvider settingsProvider, 
        Action onSelect, Action onCancel, Action onTest,
        bool userMode = true)
    {
        this.settingsProvider = settingsProvider;
        SolutionItem = solutionItem;
        Environments = new ObservableCollection<DataverseEnvironment>(GeneralOptions.Instance.Environments);
        Environment = solutionItem.Type switch
        {
            SolutionItemType.Solution => userMode ? GetSolutionUserEnvironment() : GetSolutionEnvironment(),
            SolutionItemType.Project => userMode ? GetProjectUserEnvironment() : GetProjectEnvironment(),
            _ => GeneralOptions.Instance.CurrentEnvironment
        };
        SelectCommand = new RelayCommand(onSelect);
        CancelCommand = new RelayCommand(onCancel);
        TestCommand = new RelayCommand(onTest);
    }

    private DataverseEnvironment GetSolutionEnvironment()
    {
        var url = settingsProvider.SolutionSettings.EnvironmentUrl;
        return Environments.FirstOrDefault(e => e.Url == url);
    }

    private DataverseEnvironment GetSolutionUserEnvironment()
    {
        var url = settingsProvider.SolutionUserSettings.EnvironmentUrl;
        return Environments.FirstOrDefault(e => e.Url == url);
    }

    private DataverseEnvironment GetProjectEnvironment()
    {
        var url = ((Project)SolutionItem).GetAttributeAsync("EnvironmentUrl", ProjectStorageType.ProjectFile).ConfigureAwait(false).GetAwaiter().GetResult();
        return Environments.FirstOrDefault(e => e.Url == url);
    }

    private DataverseEnvironment GetProjectUserEnvironment()
    {
        var url = ((Project)SolutionItem).GetAttributeAsync("EnvironmentUrl", ProjectStorageType.UserFile).ConfigureAwait(false).GetAwaiter().GetResult();
        return Environments.FirstOrDefault(e => e.Url == url);
    }
}
