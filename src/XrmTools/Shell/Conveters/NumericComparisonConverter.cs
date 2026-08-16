namespace XrmTools.Shell.Conveters;

using System;
using System.Globalization;
using System.Windows.Data;
using XrmTools.Shell.Styles;

internal class NumericComparisonConverter : IValueConverter, IMultiValueConverter
{
    public Operation Operation { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool flag;
        switch (value)
        {
            case byte num1:
                flag = Compare(num1, System.Convert.ToByte(parameter));
                break;
            case double num2:
                flag = Compare(num2, System.Convert.ToDouble(parameter));
                break;
            case float num3:
                flag = Compare(num3, System.Convert.ToSingle(parameter));
                break;
            case int num4:
                flag = Compare(num4, System.Convert.ToInt32(parameter));
                break;
            case long num5:
                flag = Compare(num5, System.Convert.ToInt64(parameter));
                break;
            case sbyte num6:
                flag = Compare(num6, System.Convert.ToSByte(parameter));
                break;
            case short num7:
                flag = Compare(num7, System.Convert.ToInt16(parameter));
                break;
            case uint num8:
                flag = Compare(num8, System.Convert.ToUInt32(parameter));
                break;
            case ulong num9:
                flag = Compare(num9, System.Convert.ToUInt64(parameter));
                break;
            case ushort num10:
                flag = Compare(num10, System.Convert.ToUInt16(parameter));
                break;
            default:
                throw new NotSupportedException(value.GetType().ToString());
        }
        return flag;
    }

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length != 2)
            throw new ArgumentException(nameof(values));
        return Convert(values[0], targetType, values[1], culture);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public object[] ConvertBack(
      object value,
      Type[] targetTypes,
      object parameter,
      CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private bool Compare<T>(T value, T compareValue) where T : IComparable<T>
    {
        switch (Operation)
        {
            case Operation.IsEqual:
                return value.CompareTo(compareValue) == 0;
            case Operation.IsNotEqual:
                return value.CompareTo(compareValue) != 0;
            case Operation.IsGreaterThan:
                return value.CompareTo(compareValue) > 0;
            case Operation.IsGreaterThanOrEqual:
                return value.CompareTo(compareValue) >= 0;
            case Operation.IsLessThan:
                return value.CompareTo(compareValue) < 0;
            case Operation.IsLessThanOrEqual:
                return value.CompareTo(compareValue) <= 0;
            default:
                throw new NotSupportedException(Operation.ToString());
        }
    }
}
