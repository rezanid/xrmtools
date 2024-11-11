namespace XrmTools.UI.Converters;

using System;
using System.Globalization;
using System.Windows.Data;
using XrmTools.Helpers;

public class ObjectToJsonConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) 
        => StringHelpers.SerializeJson(value);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException($"{nameof(ObjectToJsonConverter)} is a one-way converter.");
}
