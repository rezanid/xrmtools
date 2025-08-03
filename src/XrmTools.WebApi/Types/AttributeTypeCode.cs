namespace XrmTools.WebApi.Types;
using System.Runtime.Serialization;

public enum AttributeTypeCode
{
    [EnumMember(Value = "Boolean")]
    Boolean,
    [EnumMember(Value = "Customer")]
    Customer,
    [EnumMember(Value = "DateTime")]
    DateTime,
    [EnumMember(Value = "Decimal")]
    Decimal,
    [EnumMember(Value = "Double")]
    Double,
    [EnumMember(Value = "Integer")]
    Integer,
    [EnumMember(Value = "Lookup")]
    Lookup,
    [EnumMember(Value = "Memo")]
    Memo,
    [EnumMember(Value = "Money")]
    Money,
    [EnumMember(Value = "Owner")]
    Owner,
    [EnumMember(Value = "PartyList")]
    PartyList,
    [EnumMember(Value = "Picklist")]
    Picklist,
    [EnumMember(Value = "State")]
    State,
    [EnumMember(Value = "Status")]
    Status,
    [EnumMember(Value = "String")]
    String,
    [EnumMember(Value = "Uniqueidentifier")]
    Uniqueidentifier,
    [EnumMember(Value = "CalendarRules")]
    CalendarRules,
    [EnumMember(Value = "Virtual")]
    Virtual,
    [EnumMember(Value = "BigInt")]
    BigInt,
    [EnumMember(Value = "ManagedProperty")]
    ManagedProperty,
    [EnumMember(Value = "EntityName")]
    EntityName
}