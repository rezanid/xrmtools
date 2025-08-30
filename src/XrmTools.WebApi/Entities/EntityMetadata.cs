namespace XrmTools.WebApi.Entities;

using System;
using System.Collections.Generic;
using XrmTools.WebApi.Entities.Attributes;
using XrmTools.WebApi.Types;

[EntityMetadata("EntityMetadata", "EntityDefinitions")]
public class EntityMetadata(string LogicalName, string EntitySetName) : MetadataBase
{
    public int? ActivityTypeMask { get; set; }
    public string LogicalName { get; set; } = LogicalName;
    public string EntitySetName { get; set; } = EntitySetName;
    public Label? DisplayName { get; set; }
    public Label? Description { get; set; }
    public bool IsCustomEntity { get; set; }
    public bool IsIntersect { get; set; }
    public bool IsValidForAdvancedFind { get; set; }
    public bool IsValidForFormAssistant { get; set; }
    public AttributeMetadata[]? Attributes { get; set; }
    public bool? AutoRouteToOwnerQueue { get; set; }
    public bool? CanTriggerWorkflow { get; set; }
    public Label? DisplayCollectionName { get; set; }
    public bool? EntityHelpUrlEnabled { get; set; }
    public string? EntityHelpUrl { get; set; }
    public bool? IsDocumentManagementEnabled { get; set; }
    public bool? IsOneNoteIntegrationEnabled { get; set; }
    public bool? IsInteractionCentricEnabled { get; set; }
    public bool? IsKnowledgeManagementEnabled { get; set; }
    public bool? IsSLAEnabled { get; set; }
    public bool? IsBPFEntity { get; set; }
    public bool? IsDocumentRecommendationsEnabled { get; set; }
    public bool? IsMSTeamsIntegrationEnabled { get; set; }
    public string? SettingOf { get; set; }
    public Guid? DataProviderId { get; set; }
    public Guid? DataSourceId { get; set; }
    public bool? AutoCreateAccessTeams { get; set; }
    public bool? IsActivity { get; set; }
    public bool? IsActivityParty { get; set; }
    public bool? IsRetrieveAuditEnabled { get; set; }
    public bool? IsRetrieveMultipleAuditEnabled { get; set; }
    public bool? IsArchivalEnabled { get; set; }
    public bool? IsRetentionEnabled { get; set; }
    public EntityClusterMode? ClusterMode { get; set; }
    public bool? IsAvailableOffline { get; set; }
    public bool? IsChildEntity { get; set; }
    public bool? IsAIRUpdated { get; set; }
    public string? IconLargeName { get; set; }
    public string? IconMediumName { get; set; }
    public string? IconSmallName { get; set; }
    public string? IconVectorName { get; set; }
    public bool? IsBusinessProcessEnabled { get; set; }
    public bool? IsImportable { get; set; }
    public bool? IsManaged { get; set; }
    public bool? IsEnabledForCharts { get; set; }
    public bool? IsEnabledForTrace { get; set; }
    public ManagedBooleanProperty? IsAuditEnabled { get; set; }
    public ManagedBooleanProperty? IsValidForQueue { get; set; }
    public ManagedBooleanProperty? IsConnectionsEnabled { get; set; }
    public ManagedBooleanProperty? IsCustomizable { get; set; }
    public ManagedBooleanProperty? IsRenameable { get; set; }
    public ManagedBooleanProperty? IsMappable { get; set; }
    public ManagedBooleanProperty? IsDuplicateDetectionEnabled { get; set; }
    public ManagedBooleanProperty? CanCreateAttributes { get; set; }
    public ManagedBooleanProperty? CanCreateForms { get; set; }
    public ManagedBooleanProperty? CanCreateViews { get; set; }
    public ManagedBooleanProperty? CanCreateCharts { get; set; }
    public ManagedBooleanProperty? CanBeRelatedEntityInRelationship { get; set; }
    public ManagedBooleanProperty? CanBePrimaryEntityInRelationship { get; set; }
    public ManagedBooleanProperty? CanBeInManyToMany { get; set; }
    public ManagedBooleanProperty? CanBeInCustomEntityAssociation { get; set; }
    public ManagedBooleanProperty? CanEnableSyncToExternalSearchIndex { get; set; }
    public bool? SyncToExternalSearchIndex { get; set; }
    public ManagedBooleanProperty? CanModifyAdditionalSettings { get; set; }
    public ManagedBooleanProperty? CanChangeHierarchicalRelationship { get; set; }
    public bool? IsOptimisticConcurrencyEnabled { get; set; }
    public bool? ChangeTrackingEnabled { get; set; }
    public ManagedBooleanProperty? CanChangeTrackingBeEnabled { get; set; }
    public string? EntityColor { get; set; }
    public EntityKeyMetadata[]? Keys { get; set; }
    public string[]? PrimaryKey { get; set; }
    public string? LogicalCollectionName { get; set; }
    public string? ExternalCollectionName { get; set; }
    public string? CollectionSchemaName { get; set; }
    public int? DaysSinceRecordLastModified { get; set; }
    public string? MobileOfflineFilters { get; set; }
    public bool? IsReadingPaneEnabled { get; set; }
    public bool? IsQuickCreateEnabled { get; set; }
    public ManyToManyRelationshipMetadata[]? ManyToManyRelationships { get; set; }
    public OneToManyRelationshipMetadata[]? ManyToOneRelationships { get; set; }
    public OneToManyRelationshipMetadata[]? OneToManyRelationships { get; set; }
    public int? ObjectTypeCode { get; set; }
    public OwnershipTypes? OwnershipType { get; set; }
    public string? PrimaryNameAttribute { get; set; }
    public string? PrimaryImageAttribute { get; set; }
    public string? PrimaryIdAttribute { get; set; }
    public SecurityPrivilegeMetadata[]? Privileges { get; set; }
    public string? RecurrenceBaseEntityLogicalName { get; set; }
    public string? ReportViewName { get; set; }
    public string? SchemaName { get; set; }
    public string? IntroducedVersion { get; set; }
    public bool? IsStateModelAware { get; set; }
    public bool? EnforceStateTransitions { get; set; }
    public string? ExternalName { get; set; }
    public bool? IsEnabledForExternalChannels { get; set; }
    public bool? IsPrivate { get; set; }
    public bool? UsesBusinessDataLabelTable { get; set; }
    public bool? IsLogicalEntity { get; set; }
    public bool? HasNotes { get; set; }
    public bool? HasActivities { get; set; }
    public bool? HasFeedback { get; set; }
    public bool? IsSolutionAware { get; set; }
    public EntitySetting[]? Settings { get; set; }
    public DateTime? CreatedOn { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public bool? HasEmailAddresses { get; set; }
    public Guid? OwnerId { get; set; }
    public int? OwnerIdType { get; set; }
    public Guid? OwningBusinessUnit { get; set; }
    public string? TableType { get; set; }
}
#nullable restore