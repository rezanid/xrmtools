using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;

namespace XrmTraditionalPlugins
{
	/// <summary>
	/// Display Name: Task
	/// </summary>
	[GeneratedCode("TemplatedCodeGenerator", "1.4.3.0")]
	[EntityLogicalName("task")]
	[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public partial class Task : Microsoft.Xrm.Sdk.Entity
	{
		public partial class Meta 
		{
			public const string EntityLogicalName = "task";
			public const string EntityLogicalCollectionName = "tasks";
			public const string EntitySetName = "tasks";
			public const string PrimaryNameAttribute = "subject";
			public const string PrimaryIdAttribute = "activityid";

			public partial class Fields
			{
				public const string ActivityAdditionalParams = "activityadditionalparams";
				public const string ActualDurationMinutes = "actualdurationminutes";
				public const string ActualEnd = "actualend";
				public const string ActualStart = "actualstart";
				public const string Category = "category";
				public const string CreatedBy = "createdby";
				public static readonly ReadOnlyCollection<string> CreatedByTargets = new ReadOnlyCollection<string>(new string[] { "systemuser" });
				public const string CreatedOn = "createdon";
				public const string CreatedOnBehalfBy = "createdonbehalfby";
				public static readonly ReadOnlyCollection<string> CreatedOnBehalfByTargets = new ReadOnlyCollection<string>(new string[] { "systemuser" });
				public const string CrmTaskAssignedUniqueId = "crmtaskassigneduniqueid";
				public const string Description = "description";
				public const string ExchangeRate = "exchangerate";
				public const string ImportSequenceNumber = "importsequencenumber";
				public const string IsBilled = "isbilled";
				public const string IsRegularActivity = "isregularactivity";
				public const string IsWorkflowCreated = "isworkflowcreated";
				public const string LastOnHoldTime = "lastonholdtime";
				public const string ModifiedBy = "modifiedby";
				public static readonly ReadOnlyCollection<string> ModifiedByTargets = new ReadOnlyCollection<string>(new string[] { "systemuser" });
				public const string ModifiedOn = "modifiedon";
				public const string ModifiedOnBehalfBy = "modifiedonbehalfby";
				public static readonly ReadOnlyCollection<string> ModifiedOnBehalfByTargets = new ReadOnlyCollection<string>(new string[] { "systemuser" });
				public const string Msft_DataState = "msft_datastate";
				public const string OnHoldTime = "onholdtime";
				public const string OverriddenCreatedOn = "overriddencreatedon";
				public const string OwnerId = "ownerid";
				public const string OwningBusinessUnit = "owningbusinessunit";
				public static readonly ReadOnlyCollection<string> OwningBusinessUnitTargets = new ReadOnlyCollection<string>(new string[] { "businessunit" });
				public const string OwningTeam = "owningteam";
				public static readonly ReadOnlyCollection<string> OwningTeamTargets = new ReadOnlyCollection<string>(new string[] { "team" });
				public const string OwningUser = "owninguser";
				public static readonly ReadOnlyCollection<string> OwningUserTargets = new ReadOnlyCollection<string>(new string[] { "systemuser" });
				public const string PercentComplete = "percentcomplete";
				public const string PriorityCode = "prioritycode";
				public const string ProcessId = "processid";
				public const string RegardingObjectId = "regardingobjectid";
				public static readonly ReadOnlyCollection<string> RegardingObjectIdTargets = new ReadOnlyCollection<string>(new string[] { "account","adx_invitation","contact","knowledgearticle","knowledgebaserecord","mspp_adplacement","mspp_pollplacement","mspp_publishingstatetransitionrule","mspp_redirect","mspp_shortcut","mspp_website" });
				public const string ScheduledDurationMinutes = "scheduleddurationminutes";
				public const string ScheduledEnd = "scheduledend";
				public const string ScheduledStart = "scheduledstart";
				public const string SLAId = "slaid";
				public static readonly ReadOnlyCollection<string> SLAIdTargets = new ReadOnlyCollection<string>(new string[] { "sla" });
				public const string SLAInvokedId = "slainvokedid";
				public static readonly ReadOnlyCollection<string> SLAInvokedIdTargets = new ReadOnlyCollection<string>(new string[] { "sla" });
				public const string SortDate = "sortdate";
				public const string StageId = "stageid";
				public const string StateCode = "statecode";
				public const string StatusCode = "statuscode";
				public const string Subcategory = "subcategory";
				public const string Subject = "subject";
				public const string TimeZoneRuleVersionNumber = "timezoneruleversionnumber";
				public const string TransactionCurrencyId = "transactioncurrencyid";
				public static readonly ReadOnlyCollection<string> TransactionCurrencyIdTargets = new ReadOnlyCollection<string>(new string[] { "transactioncurrency" });
				public const string TraversedPath = "traversedpath";
				public const string UTCConversionTimeZoneCode = "utcconversiontimezonecode";
				public const string VersionNumber = "versionnumber";

				private static readonly Dictionary<string, string> _fieldMap = new Dictionary<string, string>
				{
					[nameof(ActivityAdditionalParams)] = ActivityAdditionalParams,
					[nameof(ActualDurationMinutes)] = ActualDurationMinutes,
					[nameof(ActualEnd)] = ActualEnd,
					[nameof(ActualStart)] = ActualStart,
					[nameof(Category)] = Category,
					[nameof(CreatedBy)] = CreatedBy,
					[nameof(CreatedOn)] = CreatedOn,
					[nameof(CreatedOnBehalfBy)] = CreatedOnBehalfBy,
					[nameof(CrmTaskAssignedUniqueId)] = CrmTaskAssignedUniqueId,
					[nameof(Description)] = Description,
					[nameof(ExchangeRate)] = ExchangeRate,
					[nameof(ImportSequenceNumber)] = ImportSequenceNumber,
					[nameof(IsBilled)] = IsBilled,
					[nameof(IsRegularActivity)] = IsRegularActivity,
					[nameof(IsWorkflowCreated)] = IsWorkflowCreated,
					[nameof(LastOnHoldTime)] = LastOnHoldTime,
					[nameof(ModifiedBy)] = ModifiedBy,
					[nameof(ModifiedOn)] = ModifiedOn,
					[nameof(ModifiedOnBehalfBy)] = ModifiedOnBehalfBy,
					[nameof(Msft_DataState)] = Msft_DataState,
					[nameof(OnHoldTime)] = OnHoldTime,
					[nameof(OverriddenCreatedOn)] = OverriddenCreatedOn,
					[nameof(OwnerId)] = OwnerId,
					[nameof(OwningBusinessUnit)] = OwningBusinessUnit,
					[nameof(OwningTeam)] = OwningTeam,
					[nameof(OwningUser)] = OwningUser,
					[nameof(PercentComplete)] = PercentComplete,
					[nameof(PriorityCode)] = PriorityCode,
					[nameof(ProcessId)] = ProcessId,
					[nameof(RegardingObjectId)] = RegardingObjectId,
					[nameof(ScheduledDurationMinutes)] = ScheduledDurationMinutes,
					[nameof(ScheduledEnd)] = ScheduledEnd,
					[nameof(ScheduledStart)] = ScheduledStart,
					[nameof(SLAId)] = SLAId,
					[nameof(SLAInvokedId)] = SLAInvokedId,
					[nameof(SortDate)] = SortDate,
					[nameof(StageId)] = StageId,
					[nameof(StateCode)] = StateCode,
					[nameof(StatusCode)] = StatusCode,
					[nameof(Subcategory)] = Subcategory,
					[nameof(Subject)] = Subject,
					[nameof(TimeZoneRuleVersionNumber)] = TimeZoneRuleVersionNumber,
					[nameof(TransactionCurrencyId)] = TransactionCurrencyId,
					[nameof(TraversedPath)] = TraversedPath,
					[nameof(UTCConversionTimeZoneCode)] = UTCConversionTimeZoneCode,
					[nameof(VersionNumber)] = VersionNumber,
				};

				public static bool TryGet(string logicalName, out string attribute)
				{
					return _fieldMap.TryGetValue(logicalName, out attribute);
				}

				public string this[string logicalName]
				{
					get => TryGet(logicalName, out var value)
						? value
						: throw new ArgumentException("Invalid attribute logical name.", nameof(logicalName));
				}
			}

			public partial class OptionSets
			{
				/// <summary>
				/// Type of activity.
				/// </summary>
				[DataContract]
				public enum ActivityType
				{
					[EnumMember] Fax = 4204,
					[EnumMember] PhoneCall = 4210,
					[EnumMember] Email = 4202,
					[EnumMember] Letter = 4207,
					[EnumMember] Appointment = 4201,
					[EnumMember] Task = 4212,
					[EnumMember] RecurringAppointment = 4251,
					[EnumMember] ActivityRecordForTheTeamsChat = 10226,
					[EnumMember] Meeting = 10304,
					[EnumMember] CustomerVoiceAlert = 10398,
					[EnumMember] CustomerVoiceSurveyInvite = 10408,
					[EnumMember] CustomerVoiceSurveyResponse = 10410,
					[EnumMember] InviteRedemption = 10521,
					[EnumMember] PortalComment = 10522,
				}
				[DataContract]
				public enum Datastate
				{
					[EnumMember] Default = 0,
					[EnumMember] Retain = 1,
				}
				/// <summary>
				/// Priority of the task.
				/// </summary>
				[DataContract]
				public enum Priority
				{
					[EnumMember] Low = 0,
					[EnumMember] Normal = 1,
					[EnumMember] High = 2,
				}
				/// <summary>
				/// Status of the task.
				/// </summary>
				[DataContract]
				public enum ActivityStatus
				{
					[EnumMember] Open = 0,
					[EnumMember] Completed = 1,
					[EnumMember] Canceled = 2,
				}
				/// <summary>
				/// Reason for the status of the task.
				/// </summary>
				[DataContract]
				public enum StatusReason
				{
					[EnumMember] NotStarted = 2,
					[EnumMember] InProgress = 3,
					[EnumMember] WaitingOnSomeoneElse = 4,
					[EnumMember] Completed = 5,
					[EnumMember] Canceled = 6,
					[EnumMember] Deferred = 7,
				}
			}
		}

		/// <summary>
		/// Max Length: 8192<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("activityadditionalparams")]
		public string ActivityAdditionalParams
		{
			get => TryGetAttributeValue("activityadditionalparams", out string value) ? value : null;
			set => this["activityadditionalparams"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("actualdurationminutes")]
		public int? ActualDurationMinutes
		{
			get => TryGetAttributeValue("actualdurationminutes", out int? value) ? value : null;
			set => this["actualdurationminutes"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("actualend")]
		public DateTime? ActualEnd
		{
			get => TryGetAttributeValue("actualend", out DateTime? value) ? value : null;
			set => this["actualend"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("actualstart")]
		public DateTime? ActualStart
		{
			get => TryGetAttributeValue("actualstart", out DateTime? value) ? value : null;
			set => this["actualstart"] = value;
		}
		/// <summary>
		/// Max Length: 250<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("category")]
		public string Category
		{
			get => TryGetAttributeValue("category", out string value) ? value : null;
			set => this["category"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// Targets: systemuser<br/>
		/// </summary>
		[AttributeLogicalName("createdby")]
		public EntityReference CreatedBy
		{
			get => TryGetAttributeValue("createdby", out EntityReference value) ? value : null;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("createdon")]
		public DateTime? CreatedOn
		{
			get => TryGetAttributeValue("createdon", out DateTime? value) ? value : null;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// Targets: systemuser<br/>
		/// </summary>
		[AttributeLogicalName("createdonbehalfby")]
		public EntityReference CreatedOnBehalfBy
		{
			get => TryGetAttributeValue("createdonbehalfby", out EntityReference value) ? value : null;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("crmtaskassigneduniqueid")]
		public Guid? CrmTaskAssignedUniqueId
		{
			get => TryGetAttributeValue("crmtaskassigneduniqueid", out Guid? value) ? value : null;
			set => this["crmtaskassigneduniqueid"] = value;
		}
		/// <summary>
		/// Max Length: 2000<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("description")]
		public string Description
		{
			get => TryGetAttributeValue("description", out string value) ? value : null;
			set => this["description"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("exchangerate")]
		public decimal? ExchangeRate
		{
			get => TryGetAttributeValue("exchangerate", out decimal? value) ? value : null;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Read<br/>
		/// </summary>
		[AttributeLogicalName("importsequencenumber")]
		public int? ImportSequenceNumber
		{
			get => TryGetAttributeValue("importsequencenumber", out int? value) ? value : null;
			set => this["importsequencenumber"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("isbilled")]
		public bool? IsBilled
		{
			get => TryGetAttributeValue("isbilled", out bool? value) ? value : null;
			set => this["isbilled"] = value;
		}
		/// <summary>
		/// Required Level: SystemRequired<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("isregularactivity")]
		public bool? IsRegularActivity
		{
			get => TryGetAttributeValue("isregularactivity", out bool? value) ? value : null;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("isworkflowcreated")]
		public bool? IsWorkflowCreated
		{
			get => TryGetAttributeValue("isworkflowcreated", out bool? value) ? value : null;
			set => this["isworkflowcreated"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("lastonholdtime")]
		public DateTime? LastOnHoldTime
		{
			get => TryGetAttributeValue("lastonholdtime", out DateTime? value) ? value : null;
			set => this["lastonholdtime"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// Targets: systemuser<br/>
		/// </summary>
		[AttributeLogicalName("modifiedby")]
		public EntityReference ModifiedBy
		{
			get => TryGetAttributeValue("modifiedby", out EntityReference value) ? value : null;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("modifiedon")]
		public DateTime? ModifiedOn
		{
			get => TryGetAttributeValue("modifiedon", out DateTime? value) ? value : null;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// Targets: systemuser<br/>
		/// </summary>
		[AttributeLogicalName("modifiedonbehalfby")]
		public EntityReference ModifiedOnBehalfBy
		{
			get => TryGetAttributeValue("modifiedonbehalfby", out EntityReference value) ? value : null;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("msft_datastate")]
		public Task.Meta.OptionSets.Datastate? Msft_DataState
		{
			get => TryGetAttributeValue("msft_datastate", out OptionSetValue opt) && opt != null ? (Task.Meta.OptionSets.Datastate?)opt.Value : null;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("onholdtime")]
		public int? OnHoldTime
		{
			get => TryGetAttributeValue("onholdtime", out int? value) ? value : null;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Read<br/>
		/// </summary>
		[AttributeLogicalName("overriddencreatedon")]
		public DateTime? OverriddenCreatedOn
		{
			get => TryGetAttributeValue("overriddencreatedon", out DateTime? value) ? value : null;
			set => this["overriddencreatedon"] = value;
		}
		/// <summary>
		/// Required Level: SystemRequired<br/>
		/// Valid for: Create Update Read<br/>
		/// Targets: systemuser,team<br/>
		/// </summary>
		[AttributeLogicalName("ownerid")]
		public EntityReference OwnerId
		{
			get => TryGetAttributeValue("ownerid", out EntityReference value) ? value : null;
			set => this["ownerid"] = value;
		}

		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// Targets: businessunit<br/>
		/// </summary>
		[AttributeLogicalName("owningbusinessunit")]
		public EntityReference OwningBusinessUnit
		{
			get => TryGetAttributeValue("owningbusinessunit", out EntityReference value) ? value : null;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// Targets: team<br/>
		/// </summary>
		[AttributeLogicalName("owningteam")]
		public EntityReference OwningTeam
		{
			get => TryGetAttributeValue("owningteam", out EntityReference value) ? value : null;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// Targets: systemuser<br/>
		/// </summary>
		[AttributeLogicalName("owninguser")]
		public EntityReference OwningUser
		{
			get => TryGetAttributeValue("owninguser", out EntityReference value) ? value : null;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("percentcomplete")]
		public int? PercentComplete
		{
			get => TryGetAttributeValue("percentcomplete", out int? value) ? value : null;
			set => this["percentcomplete"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("prioritycode")]
		public Task.Meta.OptionSets.Priority? PriorityCode
		{
			get => TryGetAttributeValue("prioritycode", out OptionSetValue opt) && opt != null ? (Task.Meta.OptionSets.Priority?)opt.Value : null;
			set => this["prioritycode"] = value == null ? null : new OptionSetValue((int)value);
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("processid")]
		public Guid? ProcessId
		{
			get => TryGetAttributeValue("processid", out Guid? value) ? value : null;
			set => this["processid"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// Targets: account,adx_invitation,contact,knowledgearticle,knowledgebaserecord,mspp_adplacement,mspp_pollplacement,mspp_publishingstatetransitionrule,mspp_redirect,mspp_shortcut,mspp_website<br/>
		/// </summary>
		[AttributeLogicalName("regardingobjectid")]
		public EntityReference RegardingObjectId
		{
			get => TryGetAttributeValue("regardingobjectid", out EntityReference value) ? value : null;
			set
			{
				if (!Task.Meta.Fields.RegardingObjectIdTargets.Contains(value.LogicalName))
				{
					throw new InvalidPluginExecutionException($"{value.LogicalName}:{value.Id} is not a valid RegardingObjectId. The only valid references are account, adx_invitation, contact, knowledgearticle, knowledgebaserecord, mspp_adplacement, mspp_pollplacement, mspp_publishingstatetransitionrule, mspp_redirect, mspp_shortcut, mspp_website");			
				}
				this["regardingobjectid"] = value;
			}
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("scheduleddurationminutes")]
		public int? ScheduledDurationMinutes
		{
			get => TryGetAttributeValue("scheduleddurationminutes", out int? value) ? value : null;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("scheduledend")]
		public DateTime? ScheduledEnd
		{
			get => TryGetAttributeValue("scheduledend", out DateTime? value) ? value : null;
			set => this["scheduledend"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("scheduledstart")]
		public DateTime? ScheduledStart
		{
			get => TryGetAttributeValue("scheduledstart", out DateTime? value) ? value : null;
			set => this["scheduledstart"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// Targets: sla<br/>
		/// </summary>
		[AttributeLogicalName("slaid")]
		public EntityReference SLAId
		{
			get => TryGetAttributeValue("slaid", out EntityReference value) ? value : null;
			set
			{
				if (!Task.Meta.Fields.SLAIdTargets.Contains(value.LogicalName))
				{
					throw new InvalidPluginExecutionException($"{value.LogicalName}:{value.Id} is not a valid SLAId. The only valid references are sla");			
				}
				this["slaid"] = value;
			}
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// Targets: sla<br/>
		/// </summary>
		[AttributeLogicalName("slainvokedid")]
		public EntityReference SLAInvokedId
		{
			get => TryGetAttributeValue("slainvokedid", out EntityReference value) ? value : null;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("sortdate")]
		public DateTime? SortDate
		{
			get => TryGetAttributeValue("sortdate", out DateTime? value) ? value : null;
			set => this["sortdate"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("stageid")]
		public Guid? StageId
		{
			get => TryGetAttributeValue("stageid", out Guid? value) ? value : null;
			set => this["stageid"] = value;
		}
		/// <summary>
		/// Required Level: SystemRequired<br/>
		/// Valid for: Update Read<br/>
		/// </summary>
		[AttributeLogicalName("statecode")]
		public Task.Meta.OptionSets.ActivityStatus? StateCode
		{
			get => TryGetAttributeValue("statecode", out OptionSetValue opt) && opt != null ? (Task.Meta.OptionSets.ActivityStatus?)Enum.ToObject(typeof(Task.Meta.OptionSets.ActivityStatus), opt.Value) : null;
			set => this["statecode"] = value == null ? null : new OptionSetValue(((IConvertible)value).ToInt32((IFormatProvider)CultureInfo.InvariantCulture));
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("statuscode")]
		public Task.Meta.OptionSets.StatusReason? StatusCode
		{
			get => TryGetAttributeValue("statuscode", out OptionSetValue opt) && opt != null ? (Task.Meta.OptionSets.StatusReason?)Enum.ToObject(typeof(Task.Meta.OptionSets.StatusReason), opt.Value) : null;
			set => this["statuscode"] = value == null ? null : new OptionSetValue(((IConvertible)value).ToInt32((IFormatProvider)CultureInfo.InvariantCulture));
		}
		/// <summary>
		/// Max Length: 250<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("subcategory")]
		public string Subcategory
		{
			get => TryGetAttributeValue("subcategory", out string value) ? value : null;
			set => this["subcategory"] = value;
		}
		/// <summary>
		/// Max Length: 200<br/>
		/// Required Level: ApplicationRequired<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("subject")]
		public string Subject
		{
			get => TryGetAttributeValue("subject", out string value) ? value : null;
			set => this["subject"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("timezoneruleversionnumber")]
		public int? TimeZoneRuleVersionNumber
		{
			get => TryGetAttributeValue("timezoneruleversionnumber", out int? value) ? value : null;
			set => this["timezoneruleversionnumber"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// Targets: transactioncurrency<br/>
		/// </summary>
		[AttributeLogicalName("transactioncurrencyid")]
		public EntityReference TransactionCurrencyId
		{
			get => TryGetAttributeValue("transactioncurrencyid", out EntityReference value) ? value : null;
			set
			{
				if (!Task.Meta.Fields.TransactionCurrencyIdTargets.Contains(value.LogicalName))
				{
					throw new InvalidPluginExecutionException($"{value.LogicalName}:{value.Id} is not a valid TransactionCurrencyId. The only valid references are transactioncurrency");			
				}
				this["transactioncurrencyid"] = value;
			}
		}
		/// <summary>
		/// Max Length: 1250<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("traversedpath")]
		public string TraversedPath
		{
			get => TryGetAttributeValue("traversedpath", out string value) ? value : null;
			set => this["traversedpath"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("utcconversiontimezonecode")]
		public int? UTCConversionTimeZoneCode
		{
			get => TryGetAttributeValue("utcconversiontimezonecode", out int? value) ? value : null;
			set => this["utcconversiontimezonecode"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("versionnumber")]
		public long? VersionNumber
		{
			get => TryGetAttributeValue("versionnumber", out long value) ? (long?)value : null;
		}
		public Task() : base(Meta.EntityLogicalName) { }
		public Task(Guid id) : base(Meta.EntityLogicalName, id) { }
		public Task(string keyName, object keyValue) : base(Meta.EntityLogicalName, keyName, keyValue) { }
		public Task(KeyAttributeCollection keyAttributes) : base(Meta.EntityLogicalName, keyAttributes) { }
	}
}