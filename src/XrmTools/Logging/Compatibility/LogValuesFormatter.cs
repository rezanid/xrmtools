namespace XrmTools.Logging.Compatibility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

/// <summary>
/// Formatter to convert the named format items like {NamedformatItem} to string.Format
/// format.
/// </summary>
internal class LogValuesFormatter
{
    private const string NullValue = "(null)";

    private static readonly object[] EmptyArray = new object[0];

    private static readonly char[] FormatDelimiters = new char[2] { ',', ':' };

    private readonly string _format;

    private readonly List<string> _valueNames = new List<string>();

    public string OriginalFormat { get; private set; }

    public List<string> ValueNames => _valueNames;

    public LogValuesFormatter(string format)
    {
        OriginalFormat = format;
        StringBuilder stringBuilder = new StringBuilder();
        int num = 0;
        int length = format.Length;
        while (num < length)
        {
            int num2 = FindBraceIndex(format, '{', num, length);
            int num3 = FindBraceIndex(format, '}', num2, length);
            if (num3 == length)
            {
                stringBuilder.Append(format, num, length - num);
                num = length;
                continue;
            }

            int num4 = FindIndexOfAny(format, FormatDelimiters, num2, num3);
            stringBuilder.Append(format, num, num2 - num + 1);
            stringBuilder.Append(_valueNames.Count.ToString(CultureInfo.InvariantCulture));
            _valueNames.Add(format.Substring(num2 + 1, num4 - num2 - 1));
            stringBuilder.Append(format, num4, num3 - num4 + 1);
            num = num3 + 1;
        }

        _format = stringBuilder.ToString();
    }

    private static int FindBraceIndex(string format, char brace, int startIndex, int endIndex)
    {
        int result = endIndex;
        int i = startIndex;
        int num = 0;
        for (; i < endIndex; i++)
        {
            if (num > 0 && format[i] != brace)
            {
                if (num % 2 != 0)
                {
                    break;
                }

                num = 0;
                result = endIndex;
            }
            else
            {
                if (format[i] != brace)
                {
                    continue;
                }

                if (brace == '}')
                {
                    if (num == 0)
                    {
                        result = i;
                    }
                }
                else
                {
                    result = i;
                }

                num++;
            }
        }

        return result;
    }

    private static int FindIndexOfAny(string format, char[] chars, int startIndex, int endIndex)
    {
        int num = format.IndexOfAny(chars, startIndex, endIndex - startIndex);
        if (num != -1)
        {
            return num;
        }

        return endIndex;
    }

    public string Format(object[] values)
    {
        if (values != null)
        {
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = FormatArgument(values[i]);
            }
        }

        return string.Format(CultureInfo.InvariantCulture, _format, values ?? EmptyArray);
    }

    internal string Format()
    {
        return _format;
    }

    internal string Format(object arg0)
    {
        return string.Format(CultureInfo.InvariantCulture, _format, FormatArgument(arg0));
    }

    internal string Format(object arg0, object arg1)
    {
        return string.Format(CultureInfo.InvariantCulture, _format, FormatArgument(arg0), FormatArgument(arg1));
    }

    internal string Format(object arg0, object arg1, object arg2)
    {
        return string.Format(CultureInfo.InvariantCulture, _format, FormatArgument(arg0), FormatArgument(arg1), FormatArgument(arg2));
    }

    public KeyValuePair<string, object> GetValue(object[] values, int index)
    {
        if (index < 0 || index > _valueNames.Count)
        {
            throw new IndexOutOfRangeException("index");
        }

        if (_valueNames.Count > index)
        {
            return new KeyValuePair<string, object>(_valueNames[index], values[index]);
        }

        return new KeyValuePair<string, object>("{OriginalFormat}", OriginalFormat);
    }

    public IEnumerable<KeyValuePair<string, object>> GetValues(object[] values)
    {
        KeyValuePair<string, object>[] array = new KeyValuePair<string, object>[values.Length + 1];
        for (int i = 0; i != _valueNames.Count; i++)
        {
            array[i] = new KeyValuePair<string, object>(_valueNames[i], values[i]);
        }

        array[array.Length - 1] = new KeyValuePair<string, object>("{OriginalFormat}", OriginalFormat);
        return array;
    }

    private object FormatArgument(object value)
    {
        if (value == null)
        {
            return "(null)";
        }

        if (value is string)
        {
            return value;
        }

        if (value is IEnumerable source)
        {
            return string.Join(", ", from object o in source
                                     select o ?? "(null)");
        }

        return value;
    }
}