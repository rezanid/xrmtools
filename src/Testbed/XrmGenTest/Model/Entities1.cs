using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;

namespace XrmGenTest.Model;
[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
[EntityLogicalName("notification")]
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public partial class Notification : Entity
{
	public static class Meta 
	{
		public const string EntityLogicalName = "notification";
		public const string EntityLogicalCollectionName = "notifications";
		public const string EntitySetName = "notifications";
		public const string PrimaryNameAttribute = "";
		public const string PrimaryIdAttribute = "notificationid";

		public partial class Fields
		{
			public const string CreatedOn = "createdon";
			public const string EventId = "eventid";
			public const string NotificationId = "notificationid";
			public const string NotificationNumber = "notificationnumber";
		}

		public partial class Choices
		{
		}
	}

	/// <summary>
	/// Required Level: SystemRequired<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("createdon")]
	public DateTime? CreatedOn
	{
		get => TryGetAttributeValue("createdon", out DateTime value) ? value : null;
	}
	/// <summary>
	/// Required Level: SystemRequired<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("eventid")]
	public int? EventId
	{
		get => TryGetAttributeValue("eventid", out int value) ? value : null;
		set => this["eventid"] = value;
	}
	/// <summary>
	/// Required Level: SystemRequired<br/>
	/// Valid for: Create Read</br>
	/// </summary>
	
	/// <summary>
	/// Required Level: SystemRequired<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("notificationnumber")]
	public int? NotificationNumber
	{
		get => TryGetAttributeValue("notificationnumber", out int value) ? value : null;
	}
	public Notification() : base(Meta.EntityLogicalName) { }
    public Notification(string keyName, object keyValue) : base(Meta.EntityLogicalName, keyName, keyValue) { }
    public Notification(KeyAttributeCollection keyAttributes) : base(Meta.EntityLogicalName, keyAttributes) { }
}
[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
[EntityLogicalName("account")]
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public partial class Partner : Entity
{
	public static class Meta 
	{
		public const string EntityLogicalName = "account";
		public const string EntityLogicalCollectionName = "accounts";
		public const string EntitySetName = "accounts";
		public const string PrimaryNameAttribute = "name";
		public const string PrimaryIdAttribute = "accountid";

		public partial class Fields
		{
			public const string AccountNumber = "accountnumber";
			public const string Address1_City = "address1_city";
			public const string Name = "name";
			public const string PrimaryContactId = "primarycontactid";
			public static readonly ReadOnlyCollection<string> PrimaryContactIdTargets = new (["contact"]);
			public const string StateCode = "statecode";
			public const string StatusCode = "statuscode";
			public const string Telephone1 = "telephone1";
		}

		public partial class Choices
		{
			/// <summary>
			/// Status of the account.
			/// </summary>
			[DataContract]
			public enum Status
			{
				[EnumMember]
				Active = 0,
				[EnumMember]
				Inactive = 1,
			}
			/// <summary>
			/// Reason for the status of the account.
			/// </summary>
			[DataContract]
			public enum StatusReason
			{
				[EnumMember]
				Active = 1,
				[EnumMember]
				Inactive = 2,
			}
		}
	}

	/// <summary>
	/// Max Length: 20</br>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("accountnumber")]
	public string AccountNumber
	{
		get => TryGetAttributeValue("accountnumber", out string value) ? value : null;
		set => this["accountnumber"] = value;
	}
	/// <summary>
	/// Max Length: 80</br>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("address1_city")]
	public string Address1_City
	{
		get => TryGetAttributeValue("address1_city", out string value) ? value : null;
		set => this["address1_city"] = value;
	}
	/// <summary>
	/// Max Length: 160</br>
	/// Required Level: ApplicationRequired<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("name")]
	public string Name
	{
		get => TryGetAttributeValue("name", out string value) ? value : null;
		set => this["name"] = value;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// Targets: contact</br>
	/// </summary>
	[AttributeLogicalName("primarycontactid")]
	public EntityReference PrimaryContactId
	{
		get => TryGetAttributeValue("primarycontactid", out EntityReference value) ? value : null;
		set
		{
			if (!Partner.Meta.Fields.PrimaryContactIdTargets.Contains(value.LogicalName))
			{
				throw new InvalidPluginExecutionException($"{value.LogicalName}:{value.Id} is not a valid PrimaryContactId. The only valid references are contact");			
			}
			this["primarycontactid"] = value;
		}
	}
	/// <summary>
	/// Required Level: SystemRequired<br/>
	/// Valid for: Update Read</br>
	/// </summary>
	[AttributeLogicalName("statecode")]
	public Partner.Meta.Choices.Status? StateCode
	{
		get => TryGetAttributeValue("statecode", out OptionSetValue opt) && opt != null ? (Partner.Meta.Choices.Status)Enum.ToObject(typeof(Partner.Meta.Choices.Status), opt.Value) : null;
		set => this["statecode"] = value == null ? null : new OptionSetValue(((IConvertible)value).ToInt32((IFormatProvider)CultureInfo.InvariantCulture));
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("statuscode")]
	public Partner.Meta.Choices.StatusReason? StatusCode
	{
		get => TryGetAttributeValue("statuscode", out OptionSetValue opt) && opt != null ? (Partner.Meta.Choices.StatusReason)Enum.ToObject(typeof(Partner.Meta.Choices.StatusReason), opt.Value) : null;
		set => this["statuscode"] = value == null ? null : new OptionSetValue(((IConvertible)value).ToInt32((IFormatProvider)CultureInfo.InvariantCulture));
	}
	/// <summary>
	/// Max Length: 50</br>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("telephone1")]
	public string Telephone1
	{
		get => TryGetAttributeValue("telephone1", out string value) ? value : null;
		set => this["telephone1"] = value;
	}
	public Partner() : base(Meta.EntityLogicalName) { }
    public Partner(string keyName, object keyValue) : base(Meta.EntityLogicalName, keyName, keyValue) { }
    public Partner(KeyAttributeCollection keyAttributes) : base(Meta.EntityLogicalName, keyAttributes) { }
}
[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
[EntityLogicalName("contact")]
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public partial class Contact : Entity
{
	public static class Meta 
	{
		public const string EntityLogicalName = "contact";
		public const string EntityLogicalCollectionName = "contacts";
		public const string EntitySetName = "contacts";
		public const string PrimaryNameAttribute = "fullname";
		public const string PrimaryIdAttribute = "contactid";

		public partial class Fields
		{
			public const string AccountId = "accountid";
			public static readonly ReadOnlyCollection<string> AccountIdTargets = new (["account"]);
			public const string AccountIdYomiName = "accountidyominame";
			public const string AnnualIncome = "annualincome";
			public const string ParentContactId = "parentcontactid";
			public static readonly ReadOnlyCollection<string> ParentContactIdTargets = new (["contact"]);
			public const string TerritoryCode = "territorycode";
		}

		public partial class Choices
		{
			/// <summary>
			/// Unique identifier of the territory to which the contact is assigned.
			/// </summary>
			[DataContract]
			public enum Territory
			{
				[EnumMember]
				DefaultValue = 1,
			}
		}
	}

	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// Targets: account</br>
	/// </summary>
	[AttributeLogicalName("accountid")]
	public EntityReference AccountId
	{
		get => TryGetAttributeValue("accountid", out EntityReference value) ? value : null;
	}
	/// <summary>
	/// Attribute of: accountid</br>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("accountidyominame")]
	public string AccountIdYomiName
	{
		get => FormattedValues.Contains("accountid") ? FormattedValues["accountid"] : null;
	
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("annualincome")]
	public decimal? AnnualIncome
	{
		get => TryGetAttributeValue("annualincome", out Money money) ? money.Value : null;
		set => this["annualincome"] = value.HasValue ? new Money(value.Value) : null;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// Targets: contact</br>
	/// </summary>
	[AttributeLogicalName("parentcontactid")]
	public EntityReference ParentContactId
	{
		get => TryGetAttributeValue("parentcontactid", out EntityReference value) ? value : null;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("territorycode")]
	public Contact.Meta.Choices.Territory? TerritoryCode
	{
		get => TryGetAttributeValue("territorycode", out OptionSetValue opt) && opt == null ? null : (Contact.Meta.Choices.Territory)opt.Value;
		set => this["territorycode"] = value == null ? null : new OptionSetValue((int)value);
	}
	public Contact() : base(Meta.EntityLogicalName) { }
    public Contact(string keyName, object keyValue) : base(Meta.EntityLogicalName, keyName, keyValue) { }
    public Contact(KeyAttributeCollection keyAttributes) : base(Meta.EntityLogicalName, keyAttributes) { }
}
