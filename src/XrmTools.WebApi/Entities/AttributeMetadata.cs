namespace XrmTools.WebApi.Entities;

using System;
using System.Runtime.Serialization;
using XrmTools.WebApi.Entities.Attributes;
using XrmTools.WebApi.Types;

[KnownType(typeof(MultiSelectPicklistAttributeMetadata))]
[KnownType(typeof(BooleanAttributeMetadata))]
[KnownType(typeof(DateTimeAttributeMetadata))]
[KnownType(typeof(DecimalAttributeMetadata))]
[KnownType(typeof(DoubleAttributeMetadata))]
[KnownType(typeof(EntityNameAttributeMetadata))]
[KnownType(typeof(ImageAttributeMetadata))]
[KnownType(typeof(IntegerAttributeMetadata))]
[KnownType(typeof(BigIntAttributeMetadata))]
[KnownType(typeof(LookupAttributeMetadata))]
[KnownType(typeof(MemoAttributeMetadata))]
[KnownType(typeof(MoneyAttributeMetadata))]
[KnownType(typeof(PicklistAttributeMetadata))]
[KnownType(typeof(StateAttributeMetadata))]
[KnownType(typeof(StatusAttributeMetadata))]
[KnownType(typeof(StringAttributeMetadata))]
[KnownType(typeof(ManagedPropertyAttributeMetadata))]
[KnownType(typeof(UniqueIdentifierAttributeMetadata))]
[KnownType(typeof(FileAttributeMetadata))]
[EntityMetadata("AttributeMetadata", "AttributeDefinitions")]
public class AttributeMetadata
{
    public string? AttributeOf { get; set; }
    public AttributeTypeCode AttributeType { get; set; }
    public AttributeTypeDisplayName? AttributeTypeName { get; set; }
    public int? ColumnNumber { get; set; }
    public Label? Description { get; set; }
    public Label? DisplayName { get; set; }
    public string? DeprecatedVersion { get; set; }
    public string? IntroducedVersion { get; set; }
    public string? EntityLogicalName { get; set; }
    public ManagedBooleanProperty? IsAuditEnabled { get; set; }
    public bool? IsCustomAttribute { get; set; }
    public bool? IsPrimaryId { get; set; }
    public bool? IsValidODataAttribute { get; set; }
    public bool? IsPrimaryName { get; set; }
    public bool IsValidForCreate { get; set; }
    public bool IsValidForRead { get; set; }
    public bool IsValidForUpdate { get; set; }
    public bool? CanBeSecuredForRead { get; set; }
    public bool? CanBeSecuredForCreate { get; set; }
    public bool? CanBeSecuredForUpdate { get; set; }
    public bool? IsSecured { get; set; }
    public bool? IsRetrievable { get; set; }
    public bool? IsFilterable { get; set; }
    public bool? IsSearchable { get; set; }
    public bool? IsManaged { get; set; }
    public ManagedBooleanProperty? IsGlobalFilterEnabled { get; set; }
    public ManagedBooleanProperty? IsSortableEnabled { get; set; }
    public Guid? LinkedAttributeId { get; set; }
    public string? LogicalName { get; set; }
    public ManagedBooleanProperty? IsCustomizable { get; set; }
    public ManagedBooleanProperty? IsRenameable { get; set; }
    public ManagedBooleanProperty? IsValidForAdvancedFind { get; set; }
    public bool? IsValidForForm { get; set; }
    public bool? IsRequiredForForm { get; set; }
    public bool? IsValidForGrid { get; set; }
    public AttributeRequiredLevelManagedProperty? RequiredLevel { get; set; }
    public ManagedBooleanProperty? CanModifyAdditionalSettings { get; set; }
    public string? SchemaName { get; set; }
    public string? ExternalName { get; set; }
    public bool? IsLogical { get; set; }
    public bool? IsDataSourceSecret { get; set; }
    public string? InheritsFrom { get; set; }
    public DateTime? CreatedOn { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public int? SourceType { get; set; }
    public string? AutoNumberFormat { get; set; }
    public EntitySetting[]? Settings { get; set; }

    public AttributeMetadata() { }

    protected AttributeMetadata(AttributeTypeCode attributeType)
        : this()
    {
        AttributeType = attributeType;
        AttributeTypeName = GetAttributeTypeDisplayName(attributeType);
    }

    protected AttributeMetadata(AttributeTypeCode attributeType, string? schemaName)
        : this(attributeType)
    {
        SchemaName = schemaName;
    }

    /// <summary>
    /// Please make sure add any new AttributeTypes in here so a given AttributeType
    /// is translated to AttributeTypeDisplayName and also make sure add the new AttributeType
    /// to src\managedplatform\sdk\metadata\metadatacache\MetadataCacheEnums.cs
    /// </summary>
    /// <param name="attributeType"></param>
    /// <returns></returns>
    private AttributeTypeDisplayName? GetAttributeTypeDisplayName(AttributeTypeCode attributeType)
        => attributeType switch
        {
            AttributeTypeCode.Boolean => AttributeTypeDisplayName.BooleanType,
            AttributeTypeCode.Customer => AttributeTypeDisplayName.CustomerType,
            AttributeTypeCode.DateTime => AttributeTypeDisplayName.DateTimeType,
            AttributeTypeCode.Decimal => AttributeTypeDisplayName.DecimalType,
            AttributeTypeCode.Double => AttributeTypeDisplayName.DoubleType,
            AttributeTypeCode.Integer => AttributeTypeDisplayName.IntegerType,
            AttributeTypeCode.Lookup => AttributeTypeDisplayName.LookupType,
            AttributeTypeCode.Memo => AttributeTypeDisplayName.MemoType,
            AttributeTypeCode.Money => AttributeTypeDisplayName.MoneyType,
            AttributeTypeCode.Owner => AttributeTypeDisplayName.OwnerType,
            AttributeTypeCode.PartyList => AttributeTypeDisplayName.PartyListType,
            AttributeTypeCode.Picklist => AttributeTypeDisplayName.PicklistType,
            AttributeTypeCode.State => AttributeTypeDisplayName.StateType,
            AttributeTypeCode.Status => AttributeTypeDisplayName.StatusType,
            AttributeTypeCode.String => AttributeTypeDisplayName.StringType,
            AttributeTypeCode.Uniqueidentifier => AttributeTypeDisplayName.UniqueidentifierType,
            AttributeTypeCode.CalendarRules => AttributeTypeDisplayName.CalendarRulesType,
            AttributeTypeCode.Virtual => AttributeTypeDisplayName.VirtualType,
            AttributeTypeCode.BigInt => AttributeTypeDisplayName.BigIntType,
            AttributeTypeCode.ManagedProperty => AttributeTypeDisplayName.ManagedPropertyType,
            AttributeTypeCode.EntityName => AttributeTypeDisplayName.EntityNameType,
            _ => null,
        };
}
