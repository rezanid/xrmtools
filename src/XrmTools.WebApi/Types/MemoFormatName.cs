namespace XrmTools.WebApi.Types;
using System;
using System.Linq;

public sealed class MemoFormatName : ConstantsBase<string>
{
    public static readonly MemoFormatName Text;

    public static readonly MemoFormatName Email;

    public static readonly MemoFormatName TextArea;

    public static readonly MemoFormatName Json;

    public static readonly MemoFormatName RichText;

    static MemoFormatName()
    {
        Text = Add<MemoFormatName>("Text");
        Email = Add<MemoFormatName>("Email");
        TextArea = Add<MemoFormatName>("TextArea");
        Json = Add<MemoFormatName>("Json");
        RichText = Add<MemoFormatName>("RichText");
    }

    /// <summary>
    /// Implicity converts a string to MemoFormatName
    /// </summary>
    /// <param name="formatName"></param>
    public static implicit operator MemoFormatName(string formatName)
        => new()
        {
            Value = formatName
        };

    protected override bool ValueExistsInList(string value) => ValidValues.Contains(value, StringComparer.OrdinalIgnoreCase);

    public override bool Equals(object obj)
    {
        if (obj == null)
        {
            return false;
        }

        if (obj is string strB)
        {
            return string.Compare(Value, strB, StringComparison.OrdinalIgnoreCase) == 0;
        }

        if (obj is not MemoFormatName memoFormatName)
        {
            return false;
        }

        return string.Compare(Value, memoFormatName.Value, StringComparison.OrdinalIgnoreCase) == 0;
    }

    public static bool operator ==(MemoFormatName stringFormatA, MemoFormatName stringFormatB)
    {
        if ((object)stringFormatA == stringFormatB)
        {
            return true;
        }

        if (stringFormatA is null || (object)stringFormatB == null)
        {
            return false;
        }

        return stringFormatA.Equals(stringFormatB);
    }

    public static bool operator !=(MemoFormatName stringFormatA, MemoFormatName stringFormatB)
    {
        return !(stringFormatA == stringFormatB);
    }

    public override int GetHashCode() => Value?.GetHashCode() ?? 0;
}
