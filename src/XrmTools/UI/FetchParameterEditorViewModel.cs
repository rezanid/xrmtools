#nullable enable
namespace XrmTools.UI;

using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

internal class FetchParameterEditorViewModel : ViewModelBase
{
    public ObservableCollection<FetchParameterModel> Parameters { get; }

    private FetchParameterModel? _selectedParameter;
    public FetchParameterModel? SelectedParameter
    {
        get => _selectedParameter;
        set
        {
            if (SetProperty(ref _selectedParameter, value) && value != null)
            {
                ParameterValue = value.Value;
            }
        }
    }

    private string? _parameterValue;
    public string? ParameterValue
    {
        get => _parameterValue;
        set
        {
            if (SetProperty(ref _parameterValue, value) && SelectedParameter != null)
            {
                SelectedParameter.Value = value ?? string.Empty;
            }
        }
    }

    public ICommand OkCommand { get; }
    public ICommand CancelCommand { get; }

    public FetchParameterEditorViewModel()
    {
        Parameters = new ObservableCollection<FetchParameterModel>();
        OkCommand = new RelayCommand<Window>(Ok);
        CancelCommand = new RelayCommand<Window>(Cancel);
    }

    public FetchParameterEditorViewModel(System.Collections.Generic.List<FetchXml.Model.FetchParameter> parameters) : this()
    {
        foreach (var param in parameters)
        {
            Parameters.Add(new FetchParameterModel(param.Name, param.DefaultValue, param.IsElementParameter));
        }

        // Select the first parameter by default
        if (Parameters.Count > 0)
        {
            SelectedParameter = Parameters[0];
            ParameterValue = SelectedParameter.Value;
        }
    }

    private void Ok(Window? window)
    {
        if (window != null)
        {
            window.DialogResult = true;
            window.Close();
        }
    }

    private void Cancel(Window? window)
    {
        if (window != null)
        {
            window.DialogResult = false;
            window.Close();
        }
    }

    /// <summary>
    /// Gets whether all required parameters (those without defaults) have values
    /// </summary>
    public bool AllRequiredParametersHaveValues()
    {
        return Parameters.All(p => p.HasDefaultValue || !string.IsNullOrWhiteSpace(p.Value));
    }
}
#nullable restore
