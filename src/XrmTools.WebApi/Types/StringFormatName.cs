namespace XrmTools.WebApi.Types;

using System;
using System.Linq;

public sealed class StringFormatName : ConstantsBase<string>
{
    public static readonly StringFormatName Email;

    public static readonly StringFormatName Text;

    public static readonly StringFormatName TextArea;

    public static readonly StringFormatName Url;

    public static readonly StringFormatName TickerSymbol;

    public static readonly StringFormatName PhoneticGuide;

    public static readonly StringFormatName VersionNumber;

    public static readonly StringFormatName Phone;

    public static readonly StringFormatName Json;

    public static readonly StringFormatName RichText;

    static StringFormatName()
    {
        Email = Add<StringFormatName>("Email");
        Text = Add<StringFormatName>("Text");
        TextArea = Add<StringFormatName>("TextArea");
        Url = Add<StringFormatName>("Url");
        TickerSymbol = Add<StringFormatName>("TickerSymbol");
        PhoneticGuide = Add<StringFormatName>("PhoneticGuide");
        VersionNumber = Add<StringFormatName>("VersionNumber");
        Phone = Add<StringFormatName>("Phone");
        Json = Add<StringFormatName>("Json");
        RichText = Add<StringFormatName>("RichText");
    }

    /// <summary>
    /// Implicitly converts a string to StringFormatName
    /// </summary>
    /// <param name="formatName">String value to convert</param>
    public static implicit operator StringFormatName(string formatName) =>
        new StringFormatName
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

        StringFormatName? stringFormatName = obj as StringFormatName;
        if (stringFormatName == null)
        {
            return false;
        }

        return string.Compare(Value, stringFormatName.Value, StringComparison.OrdinalIgnoreCase) == 0;
    }

    public static bool operator ==(StringFormatName? stringFormatA, StringFormatName? stringFormatB)
    {
        if ((object?)stringFormatA == stringFormatB)
        {
            return true;
        }

        if (stringFormatA is null || stringFormatB is null)
        {
            return false;
        }

        return stringFormatA.Equals(stringFormatB);
    }

    public static bool operator !=(StringFormatName stringFormatA, StringFormatName stringFormatB) => 
         !(stringFormatA == stringFormatB);

    public override int GetHashCode() => Value?.GetHashCode() ?? 0;
}