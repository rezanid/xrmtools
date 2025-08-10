namespace XrmTools.WebApi.Types;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

public sealed class AttributeTypeDisplayName : ConstantsBase<string>
{
    public static readonly AttributeTypeDisplayName BooleanType;

    public static readonly AttributeTypeDisplayName CustomerType;

    public static readonly AttributeTypeDisplayName DateTimeType;

    public static readonly AttributeTypeDisplayName DecimalType;

    public static readonly AttributeTypeDisplayName DoubleType;

    public static readonly AttributeTypeDisplayName IntegerType;

    public static readonly AttributeTypeDisplayName LookupType;

    public static readonly AttributeTypeDisplayName MemoType;

    public static readonly AttributeTypeDisplayName MoneyType;

    public static readonly AttributeTypeDisplayName OwnerType;

    public static readonly AttributeTypeDisplayName PartyListType;

    public static readonly AttributeTypeDisplayName PicklistType;

    public static readonly AttributeTypeDisplayName StateType;

    public static readonly AttributeTypeDisplayName StatusType;

    public static readonly AttributeTypeDisplayName StringType;

    public static readonly AttributeTypeDisplayName UniqueidentifierType;

    public static readonly AttributeTypeDisplayName CalendarRulesType;

    public static readonly AttributeTypeDisplayName VirtualType;

    public static readonly AttributeTypeDisplayName BigIntType;

    public static readonly AttributeTypeDisplayName ManagedPropertyType;

    public static readonly AttributeTypeDisplayName EntityNameType;

    public static readonly AttributeTypeDisplayName ImageType;

    //
    // Summary:
    //     Attribute type to allow multiple values to be selected on Picklist
    public static readonly AttributeTypeDisplayName MultiSelectPicklistType;

    public static readonly AttributeTypeDisplayName FileType;

    public static readonly AttributeTypeDisplayName CustomType;

    static AttributeTypeDisplayName()
    {
        BooleanType = Add<AttributeTypeDisplayName>("BooleanType");
        CustomerType = Add<AttributeTypeDisplayName>("CustomerType");
        DateTimeType = Add<AttributeTypeDisplayName>("DateTimeType");
        DecimalType = Add<AttributeTypeDisplayName>("DecimalType");
        DoubleType = Add<AttributeTypeDisplayName>("DoubleType");
        IntegerType = Add<AttributeTypeDisplayName>("IntegerType");
        LookupType = Add<AttributeTypeDisplayName>("LookupType");
        MemoType = Add<AttributeTypeDisplayName>("MemoType");
        MoneyType = Add<AttributeTypeDisplayName>("MoneyType");
        OwnerType = Add<AttributeTypeDisplayName>("OwnerType");
        PartyListType = Add<AttributeTypeDisplayName>("PartyListType");
        PicklistType = Add<AttributeTypeDisplayName>("PicklistType");
        StateType = Add<AttributeTypeDisplayName>("StateType");
        StatusType = Add<AttributeTypeDisplayName>("StatusType");
        StringType = Add<AttributeTypeDisplayName>("StringType");
        UniqueidentifierType = Add<AttributeTypeDisplayName>("UniqueidentifierType");
        CalendarRulesType = Add<AttributeTypeDisplayName>("CalendarRulesType");
        VirtualType = Add<AttributeTypeDisplayName>("VirtualType");
        BigIntType = Add<AttributeTypeDisplayName>("BigIntType");
        ManagedPropertyType = Add<AttributeTypeDisplayName>("ManagedPropertyType");
        EntityNameType = Add<AttributeTypeDisplayName>("EntityNameType");
        ImageType = Add<AttributeTypeDisplayName>("ImageType");
        MultiSelectPicklistType = Add<AttributeTypeDisplayName>("MultiSelectPicklistType");
        FileType = Add<AttributeTypeDisplayName>("FileType");
        CustomType = Add<AttributeTypeDisplayName>("CustomType");
    }

    //
    // Summary:
    //     Implicity converts a string to AttributeTypeDisplayName
    //
    // Parameters:
    //   formatName:
    //     String value to convert
    [SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Justification = "Clients who do not support operator overloading can use the constructor")]
    public static implicit operator AttributeTypeDisplayName(string attributeTypeDisplayName)
    {
        return new AttributeTypeDisplayName
        {
            Value = attributeTypeDisplayName
        };
    }

    protected override bool ValueExistsInList(string value)
    {
        return ValidValues.Contains(value, StringComparer.OrdinalIgnoreCase);
    }

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

        AttributeTypeDisplayName attributeTypeDisplayName = obj as AttributeTypeDisplayName;
        if (attributeTypeDisplayName == null)
        {
            return false;
        }

        return string.Compare(Value, attributeTypeDisplayName.Value, StringComparison.OrdinalIgnoreCase) == 0;
    }

    public static bool operator ==(AttributeTypeDisplayName attributeTypeDisplayNameA, AttributeTypeDisplayName attributeTypeDisplayNameB)
    {
        if ((object)attributeTypeDisplayNameA == attributeTypeDisplayNameB)
        {
            return true;
        }

        if ((object)attributeTypeDisplayNameA == null || (object)attributeTypeDisplayNameB == null)
        {
            return false;
        }

        return attributeTypeDisplayNameA.Equals(attributeTypeDisplayNameB);
    }

    public static bool operator !=(AttributeTypeDisplayName attributeTypeDisplayNameA, AttributeTypeDisplayName attributeTypeDisplayNameB)
    {
        return !(attributeTypeDisplayNameA == attributeTypeDisplayNameB);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}
