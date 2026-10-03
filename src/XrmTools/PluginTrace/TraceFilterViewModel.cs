#nullable enable
namespace XrmTools.PluginTrace;

using System;
using System.Globalization;
using XrmTools.UI;

internal sealed class TraceFilterViewModel : ViewModelBase
{
    private string name = "", typeName = "", expression = "", fromText, toText;
    private int minutes = 30;
    private bool errorsOnly, fullControl, updating;
    private readonly Func<DateTimeOffset> now;
    public event Action? Changed;

    public TraceFilterViewModel(Func<DateTimeOffset> now)
    {
        this.now = now;
        fromText = Format(now().AddHours(-1));
        toText = Format(now());
    }

    public string Name { get => name; set { if (SetProperty(ref name, value)) NotifyChanged(); } }
    public string TypeName { get => typeName; set { if (SetProperty(ref typeName, value)) NotifyChanged(); } }
    public string Expression { get => expression; set { if (SetProperty(ref expression, value)) NotifyChanged(); } }
    public string FromText { get => fromText; set { if (SetProperty(ref fromText, value)) NotifyChanged(); } }
    public string ToText { get => toText; set { if (SetProperty(ref toText, value)) NotifyChanged(); } }
    public int Minutes { get => minutes; set { if (SetProperty(ref minutes, value)) NotifyChanged(); } }
    public bool ErrorsOnly { get => errorsOnly; set { if (SetProperty(ref errorsOnly, value)) NotifyChanged(); } }
    public bool FullControl { get => fullControl; set { if (SetProperty(ref fullControl, value)) NotifyChanged(); } }
    public bool QuickFiltersEnabled => !FullControl;
    public bool ShowCustomRange => Minutes == 0 && !FullControl;
    public string Preview
    {
        get { try { return Snapshot().Build(now()); } catch (FormatException ex) { return ex.Message; } }
    }

    public TraceFilter Snapshot() => new()
    {
        Name = Name.Trim(), Minutes = Minutes, TypeName = TypeName, Expression = Expression,
        ErrorsOnly = ErrorsOnly, FullControl = FullControl,
        From = Parse(FromText), To = Parse(ToText)
    };

    public void Set(TraceFilter filter)
    {
        updating = true;
        try
        {
            Name = filter.Name; Minutes = filter.Minutes; TypeName = filter.TypeName;
            ErrorsOnly = filter.ErrorsOnly; FullControl = filter.FullControl; Expression = filter.Expression;
            FromText = filter.From.HasValue ? Format(filter.From.Value) : "";
            ToText = filter.To.HasValue ? Format(filter.To.Value) : "";
        }
        finally { updating = false; }
        NotifyChanged();
    }

    private void NotifyChanged()
    {
        if (updating) return;
        OnPropertyChanged(nameof(Preview)); OnPropertyChanged(nameof(QuickFiltersEnabled)); OnPropertyChanged(nameof(ShowCustomRange));
        Changed?.Invoke();
    }

    private static DateTimeOffset? Parse(string text) =>
        DateTimeOffset.TryParse(text, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out var value) ? value : null;
    private static string Format(DateTimeOffset value) => value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
}
