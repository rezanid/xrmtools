namespace XrmTools.UI.Converters;
using System;
using System.Globalization;
using System.Windows.Data;
using XrmTools.Core.Helpers;

public class ObjectToJsonConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) 
        => StringHelper.SerializeJson(value);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException($"{nameof(ObjectToJsonConverter)} is a one-way converter.");
}
