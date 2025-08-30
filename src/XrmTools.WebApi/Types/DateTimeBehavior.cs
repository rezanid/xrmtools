namespace XrmTools.WebApi.Types;

using System;
using System.Linq;

public sealed class DateTimeBehavior : ConstantsBase<string>
{
    public static readonly DateTimeBehavior UserLocal;

    public static readonly DateTimeBehavior DateOnly;

    public static readonly DateTimeBehavior TimeZoneIndependent;

    static DateTimeBehavior()
    {
        UserLocal = Add<DateTimeBehavior>("UserLocal");
        DateOnly = Add<DateTimeBehavior>("DateOnly");
        TimeZoneIndependent = Add<DateTimeBehavior>("TimeZoneIndependent");
    }

    /// <summary>
    /// Implicity converts a string to Behavior
    /// </summary>
    /// <param name="behavior">String value to convert</param>
    public static implicit operator DateTimeBehavior(string behavior) =>
        new()
        {
            Value = behavior
        };

    protected override bool ValueExistsInList(string value)
    {
        return ValidValues.Contains(value, StringComparer.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
        {
            return false;
        }

        if (obj is string strB)
        {
            return string.Compare(Value, strB, StringComparison.OrdinalIgnoreCase) == 0;
        }

        DateTimeBehavior? dateTimeBehavior = obj as DateTimeBehavior;
        if (dateTimeBehavior == null)
        {
            return false;
        }

        return string.Compare(Value, dateTimeBehavior.Value, StringComparison.OrdinalIgnoreCase) == 0;
    }

    public static bool operator ==(DateTimeBehavior? behaviorA, DateTimeBehavior? behaviorB)
    {
        if ((object?)behaviorA == behaviorB)
        {
            return true;
        }

        if (behaviorA is null || behaviorB is null)
        {
            return false;
        }

        return behaviorA.Equals(behaviorB);
    }

    public static bool operator !=(DateTimeBehavior behaviorA, DateTimeBehavior behaviorB)
    {
        return !(behaviorA == behaviorB);
    }

    public override int GetHashCode() => Value?.GetHashCode() ?? 0;
}
