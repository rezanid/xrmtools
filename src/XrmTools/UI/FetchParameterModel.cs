namespace XrmTools.UI;

/// <summary>
/// View model for a single FetchXML parameter in the UI
/// </summary>
internal class FetchParameterModel : ViewModelBase
{
    private string _name;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private string _value;
    public string Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }

    private string _defaultValue;
    public string DefaultValue
    {
        get => _defaultValue;
        set => SetProperty(ref _defaultValue, value);
    }

    private bool _isElementParameter;
    public bool IsElementParameter
    {
        get => _isElementParameter;
        set => SetProperty(ref _isElementParameter, value);
    }

    private bool _hasDefaultValue;
    public bool HasDefaultValue
    {
        get => _hasDefaultValue;
        set => SetProperty(ref _hasDefaultValue, value);
    }

    public FetchParameterModel()
    {
    }

    public FetchParameterModel(string name, string defaultValue, bool isElementParameter)
    {
        Name = name;
        DefaultValue = defaultValue;
        Value = defaultValue; // Start with default value
        IsElementParameter = isElementParameter;
        HasDefaultValue = !string.IsNullOrEmpty(defaultValue);
    }
}
