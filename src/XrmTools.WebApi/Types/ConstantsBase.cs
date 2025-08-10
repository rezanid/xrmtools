namespace XrmTools.WebApi.Types;

using System.Collections.Generic;
using System.Runtime.Serialization;

[KnownType(typeof(DateTimeBehavior))]
[KnownType(typeof(StringFormatName))]
[KnownType(typeof(AttributeTypeDisplayName))]
public abstract class ConstantsBase<T>
{
    protected static readonly IList<T> ValidValues = [];
    private static readonly object _lock = new();

    public T? Value { get; set; }

    protected abstract bool ValueExistsInList(T value);

    protected static T2 Create<T2>(T value) where T2 : ConstantsBase<T>, new()
    {
        return new T2
        {
            Value = value
        };
    }

    protected static T2 Add<T2>(T value) where T2 : ConstantsBase<T>, new()
    {
        lock (_lock)
        {
            ValidValues.Add(value);
        }

        return Create<T2>(value);
    }
}