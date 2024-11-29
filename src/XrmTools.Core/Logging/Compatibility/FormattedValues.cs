namespace XrmTools.Logging.Compatibility;
using System;
using System.Collections.Concurrent;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// LogValues to enable formatting options supported by string.Format. This also
/// enables using {NamedformatItem} in the format string.
/// </summary>
internal readonly struct FormattedLogValues : IReadOnlyList<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>, IEnumerable, IReadOnlyCollection<KeyValuePair<string, object>>
{
    internal const int MaxCachedFormatters = 1024;

    private const string NullFormat = "[null]";

    private static int _count;

    private static ConcurrentDictionary<string, LogValuesFormatter> _formatters = new ConcurrentDictionary<string, LogValuesFormatter>();

    private readonly LogValuesFormatter _formatter;

    private readonly object[] _values;

    private readonly string _originalMessage;

    internal LogValuesFormatter Formatter => _formatter;

    public KeyValuePair<string, object> this[int index]
    {
        get
        {
            if (index < 0 || index >= Count)
            {
                throw new IndexOutOfRangeException("index");
            }

            if (index == Count - 1)
            {
                return new KeyValuePair<string, object>("{OriginalFormat}", _originalMessage);
            }

            return _formatter.GetValue(_values, index);
        }
    }

    public int Count
    {
        get
        {
            if (_formatter == null)
            {
                return 1;
            }

            return _formatter.ValueNames.Count + 1;
        }
    }

    public FormattedLogValues(string format, params object[] values)
    {
        if (values != null && values.Length != 0 && format != null)
        {
            if (_count >= 1024)
            {
                if (!_formatters.TryGetValue(format, out _formatter))
                {
                    _formatter = new LogValuesFormatter(format);
                }
            }
            else
            {
                _formatter = _formatters.GetOrAdd(format, delegate (string f)
                {
                    Interlocked.Increment(ref _count);
                    return new LogValuesFormatter(f);
                });
            }
        }
        else
        {
            _formatter = null;
        }

        _originalMessage = format ?? "[null]";
        _values = values;
    }

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        int i = 0;
        while (i < Count)
        {
            yield return this[i];
            int num = i + 1;
            i = num;
        }
    }

    public override string ToString()
    {
        if (_formatter == null)
        {
            return _originalMessage;
        }

        return _formatter.Format(_values);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}