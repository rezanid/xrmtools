using AG.RM.Xrm.Utility;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;

namespace XrmGenTest.PluginDefinitions;

[EntityLogicalName("crb4d_test")]
public class Test : Entity
{
	public const string EntityLogicalName = "crb4d_test";
	public const string EntityLogicalCollectionName = "crb4d_tests";
	public const string EntitySetName = "crb4d_tests";
	public const int EntityTypeCode = 11070;
	public const string PrimaryNameAttribute = "crb4d_name";
	public const string PrimaryIdAttribute = "crb4d_testid";

	public partial class Fields
	{
		public const string AutoNumber = "crb4d_autonumber";
		public const string ChoiceGlobal = "crb4d_choiceglobal";
		public const string ChoiceglobalName = "crb4d_choiceglobalname";
		public const string ChoiceLocalMulti = "crb4d_choicelocalmulti";
		public const string ChoicelocalmultiName = "crb4d_choicelocalmultiname";
		public const string Currency = "crb4d_currency";
		public const string Currency_Base = "crb4d_currency_base";
		public const string Customer = "crb4d_customer";
		public static readonly ReadOnlyCollection<string> CustomerTargets = new (["account","contact"]);
		public const string CustomerName = "crb4d_customername";
		public const string CustomerYomiName = "crb4d_customeryominame";
		public const string DateandTime = "crb4d_dateandtime";
		public const string DateOnly = "crb4d_dateonly";
		public const string Decimal = "crb4d_decimal";
		public const string Duration = "crb4d_duration";
		public const string File = "crb4d_file";
		public const string File_Name = "crb4d_file_name";
		public const string Float = "crb4d_float";
		public const string Formula = "crb4d_formula";
		public const string Image = "crb4d_image";
		public const string Image_Timestamp = "crb4d_image_timestamp";
		public const string Image_URL = "crb4d_image_url";
		public const string ImageId = "crb4d_imageid";
		public const string LanguageCode = "crb4d_languagecode";
		public const string Lookup = "crb4d_lookup";
		public static readonly ReadOnlyCollection<string> LookupTargets = new (["ag_acme"]);
		public const string LookupName = "crb4d_lookupname";
		public const string MultipleLineofTextPlain = "crb4d_multiplelineoftextplain";
		public const string MultipleLineofTextRich = "crb4d_multiplelineoftextrich";
		public const string Name = "crb4d_name";
		public const string SingleLineOfTest = "crb4d_singlelineoftest";
		public const string SingleLineOfTextArea = "crb4d_singlelineoftextarea";
		public const string SingleLineofTextEmail = "crb4d_singlelineoftextemail";
		public const string SingleLineofTextPhoneNumber = "crb4d_singlelineoftextphonenumber";
		public const string SingleLineofTextRich = "crb4d_singlelineoftextrich";
		public const string SingleLineofTextTickerSymbol = "crb4d_singlelineoftexttickersymbol";
		public const string SingleLineofTextURL = "crb4d_singlelineoftexturl";
		public const string TestId = "crb4d_testid";
		public const string TimeZone = "crb4d_timezone";
		public const string WholeNumber = "crb4d_wholenumber";
		public const string CreatedBy = "createdby";
		public static readonly ReadOnlyCollection<string> CreatedByTargets = new (["systemuser"]);
		public const string CreatedByName = "createdbyname";
		public const string CreatedByYomiName = "createdbyyominame";
		public const string CreatedOn = "createdon";
		public const string CreatedOnBehalfBy = "createdonbehalfby";
		public static readonly ReadOnlyCollection<string> CreatedOnBehalfByTargets = new (["systemuser"]);
		public const string CreatedOnBehalfByName = "createdonbehalfbyname";
		public const string CreatedOnBehalfByYomiName = "createdonbehalfbyyominame";
		public const string ExchangeRate = "exchangerate";
		public const string ImportSequenceNumber = "importsequencenumber";
		public const string ModifiedBy = "modifiedby";
		public static readonly ReadOnlyCollection<string> ModifiedByTargets = new (["systemuser"]);
		public const string ModifiedByName = "modifiedbyname";
		public const string ModifiedByYomiName = "modifiedbyyominame";
		public const string ModifiedOn = "modifiedon";
		public const string ModifiedOnBehalfBy = "modifiedonbehalfby";
		public static readonly ReadOnlyCollection<string> ModifiedOnBehalfByTargets = new (["systemuser"]);
		public const string ModifiedOnBehalfByName = "modifiedonbehalfbyname";
		public const string ModifiedOnBehalfByYomiName = "modifiedonbehalfbyyominame";
		public const string OverriddenCreatedOn = "overriddencreatedon";
		public const string OwnerId = "ownerid";
		public const string OwnerIdName = "owneridname";
		public const string OwnerIdYomiName = "owneridyominame";
		public const string OwningBusinessUnit = "owningbusinessunit";
		public static readonly ReadOnlyCollection<string> OwningBusinessUnitTargets = new (["businessunit"]);
		public const string OwningBusinessUnitName = "owningbusinessunitname";
		public const string OwningTeam = "owningteam";
		public static readonly ReadOnlyCollection<string> OwningTeamTargets = new (["team"]);
		public const string OwningUser = "owninguser";
		public static readonly ReadOnlyCollection<string> OwningUserTargets = new (["systemuser"]);
		public const string Statecode = "statecode";
		public const string StatecodeName = "statecodename";
		public const string Statuscode = "statuscode";
		public const string StatuscodeName = "statuscodename";
		public const string TimeZoneRuleVersionNumber = "timezoneruleversionnumber";
		public const string TransactionCurrencyId = "transactioncurrencyid";
		public static readonly ReadOnlyCollection<string> TransactionCurrencyIdTargets = new (["transactioncurrency"]);
		public const string TransactionCurrencyIdName = "transactioncurrencyidname";
		public const string UTCConversionTimeZoneCode = "utcconversiontimezonecode";
		public const string VersionNumber = "versionnumber";
	}

	public partial class Choices
	{
		[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
		[DataContract]
		public enum AdagioStatus
		{
			[EnumMember]
			Active = 140140000,
			[EnumMember]
			Inactive = 140140001,
		}
		[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
		[DataContract]
		public enum ChoiceLocalMulti
		{
			[EnumMember]
			_1stOption = 233840000,
			[EnumMember]
			_2ndOption = 233840001,
		}
		/// <summary>
		/// Status of the Test
		/// </summary>
		[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
		[DataContract]
		public enum Status
		{
			[EnumMember]
			Active = 0,
			[EnumMember]
			Inactive = 1,
		}
		/// <summary>
		/// Reason for the status of the Test
		/// </summary>
		[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
		[DataContract]
		public enum StatusReason
		{
			[EnumMember]
			Active = 1,
			[EnumMember]
			Inactive = 2,
		}
	}

	/// <summary>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// Auto Number Format: {SEQNUM:4}
	/// </summary>
	[AttributeLogicalName("crb4d_autonumber")]
	public string AutoNumber
	{
		get => TryGetAttributeValue("crb4d_autonumber", out string value) ? value : null;
		set => this["crb4d_autonumber"] = value;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("crb4d_choiceglobal")]
	public Test.Choices.AdagioStatus? ChoiceGlobal
	{
		get => TryGetAttributeValue("crb4d_choiceglobal", out OptionSetValue opt) && opt == null ? null : (Test.Choices.AdagioStatus)opt.Value;
		set => this["crb4d_choiceglobal"] = value == null ? null : new OptionSetValue((int)value);
	}
	/// <summary>
	/// Attribute of: crb4d_choiceglobal</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("crb4d_choiceglobalname")]
	public string ChoiceglobalName
	{
		get => FormattedValues.Contains("crb4d_choiceglobal") ? FormattedValues["crb4d_choiceglobalname"] : null;
	}

	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("crb4d_choicelocalmulti")]
	public IEnumerable<Test.Choices.ChoiceLocalMulti> ChoiceLocalMulti
	{
		get => TryGetAttributeValue("crb4d_choicelocalmulti", out OptionSetValueCollection opts) && opts != null ? opts.Select(opt => (Test.Choices.ChoiceLocalMulti)opt.Value) : [];
		set => this["crb4d_choicelocalmulti"] = value == null || !value.Any() ? null : new OptionSetValueCollection(value.Select(each => new OptionSetValue((int)each)).ToList());
	}

	/// <summary>
	/// Attribute of: crb4d_choicelocalmulti</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("crb4d_choicelocalmultiname")]
	public string ChoicelocalmultiName
	{
		get => FormattedValues.Contains("crb4d_choicelocalmulti") ? FormattedValues["crb4d_choicelocalmultiname"] : null;
	}

	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("crb4d_currency")]
	public decimal? Currency
	{
		get => TryGetAttributeValue("crb4d_currency", out Money money) ? money.Value : null;
		set => this["crb4d_currency"] = value.HasValue ? new Money(value.Value) : null;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("crb4d_currency_base")]
	public decimal? Currency_Base
	{
		get => TryGetAttributeValue("crb4d_currency_base", out Money money) ? money.Value : null;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// Targets: account,contact</br>
	/// </summary>
	[AttributeLogicalName("crb4d_customer")]
	public EntityReference Customer
	{
		get => TryGetAttributeValue("crb4d_customer", out EntityReference value) ? value : null;
		set
		{
			if (!Test.Fields.CustomerTargets.Contains(value.LogicalName))
			{
				throw new InvalidPluginExecutionException($"{value.LogicalName}:{value.Id} is not a valid Customer. The only valid references are account, contact");			
			}
			this["crb4d_customer"] = value;
		}
	}

	/// <summary>
	/// Attribute of: crb4d_customer</br>
	/// Max Length: 4000</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("crb4d_customername")]
	public string CustomerName
	{
		get => FormattedValues.Contains("crb4d_customer") ? FormattedValues["crb4d_customer"] : null;
	
	}
	/// <summary>
	/// Attribute of: crb4d_customer</br>
	/// Max Length: 4000</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("crb4d_customeryominame")]
	public string CustomerYomiName
	{
		get => FormattedValues.Contains("crb4d_customer") ? FormattedValues["crb4d_customer"] : null;
	
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("crb4d_dateandtime")]
	public DateTime? DateandTime
	{
		get => TryGetAttributeValue("crb4d_dateandtime", out DateTime value) ? value : null;
		set => this["crb4d_dateandtime"] = value;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("crb4d_dateonly")]
	public DateTime? DateOnly
	{
		get => TryGetAttributeValue("crb4d_dateonly", out DateTime value) ? value : null;
		set => this["crb4d_dateonly"] = value;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("crb4d_decimal")]
	public decimal? Decimal
	{
		get => TryGetAttributeValue("crb4d_decimal", out decimal value) ? value : null;
		set => this["crb4d_decimal"] = value;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("crb4d_duration")]
	public int? Duration
	{
		get => TryGetAttributeValue("crb4d_duration", out int value) ? value : null;
		set => this["crb4d_duration"] = value;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("crb4d_file")]
	public Guid? File
	{
		get => TryGetAttributeValue("crb4d_file", out Guid value) ? value : null;
	}

	/// <summary>
	/// Attribute of: crb4d_file</br>
	/// Max Length: 200</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("crb4d_file_name")]
	public string File_Name
	{
		get => TryGetAttributeValue("crb4d_file_name", out string value) ? value : null;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("crb4d_float")]
	public double? Float
	{
		get => TryGetAttributeValue("crb4d_float", out double value) ? value : null;
		set => this["crb4d_float"] = value;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("crb4d_formula")]
	public DateTime? Formula
	{
		get => TryGetAttributeValue("crb4d_formula", out DateTime value) ? value : null;
	}
	/// <summary>
	/// Attribute of: crb4d_imageid</br>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("crb4d_image")]
	public byte[] Image
	{
		get => TryGetAttributeValue("crb4d_image", out byte[] value) ? value : null;
		set => this["crb4d_image"] = value;
	}

	/// <summary>
	/// Attribute of: crb4d_imageid</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("crb4d_image_timestamp")]
	public long? Image_Timestamp
	{
		get => TryGetAttributeValue("crb4d_image_timestamp", out long value) ? value : null;
	}
	/// <summary>
	/// Attribute of: crb4d_imageid</br>
	/// Max Length: 200</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("crb4d_image_url")]
	public string Image_URL
	{
		get => TryGetAttributeValue("crb4d_image_url", out string value) ? value : null;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("crb4d_imageid")]
	public Guid? ImageId
	{
		get => TryGetAttributeValue("crb4d_imageid", out Guid value) ? value : null;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("crb4d_languagecode")]
	public int? LanguageCode
	{
		get => TryGetAttributeValue("crb4d_languagecode", out int value) ? value : null;
		set => this["crb4d_languagecode"] = value;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// Targets: ag_acme</br>
	/// </summary>
	[AttributeLogicalName("crb4d_lookup")]
	public EntityReference Lookup
	{
		get => TryGetAttributeValue("crb4d_lookup", out EntityReference value) ? value : null;
		set
		{
			if (!Test.Fields.LookupTargets.Contains(value.LogicalName))
			{
				throw new InvalidPluginExecutionException($"{value.LogicalName}:{value.Id} is not a valid Lookup. The only valid references are ag_acme");			
			}
			this["crb4d_lookup"] = value;
		}
	}

	/// <summary>
	/// Attribute of: crb4d_lookup</br>
	/// Max Length: 400</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("crb4d_lookupname")]
	public string LookupName
	{
		get => FormattedValues.Contains("crb4d_lookup") ? FormattedValues["crb4d_lookup"] : null;
	
	}
	/// <summary>
	/// Max Length: 2000</br>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("crb4d_multiplelineoftextplain")]
	public string MultipleLineofTextPlain
	{
		get => TryGetAttributeValue("crb4d_multiplelineoftextplain", out string value) ? value : null;
		set => this["crb4d_multiplelineoftextplain"] = value;
	}
	/// <summary>
	/// Max Length: 2000</br>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("crb4d_multiplelineoftextrich")]
	public string MultipleLineofTextRich
	{
		get => TryGetAttributeValue("crb4d_multiplelineoftextrich", out string value) ? value : null;
		set => this["crb4d_multiplelineoftextrich"] = value;
	}
	/// <summary>
	/// Max Length: 100</br>
	/// Required Level: ApplicationRequired<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("crb4d_name")]
	public string Name
	{
		get => TryGetAttributeValue("crb4d_name", out string value) ? value : null;
		set => this["crb4d_name"] = value;
	}
	/// <summary>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// Auto Number Format: 
	/// </summary>
	[AttributeLogicalName("crb4d_singlelineoftest")]
	public string SingleLineOfTest
	{
		get => TryGetAttributeValue("crb4d_singlelineoftest", out string value) ? value : null;
		set => this["crb4d_singlelineoftest"] = value;
	}
	/// <summary>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// Auto Number Format: 
	/// </summary>
	[AttributeLogicalName("crb4d_singlelineoftextarea")]
	public string SingleLineOfTextArea
	{
		get => TryGetAttributeValue("crb4d_singlelineoftextarea", out string value) ? value : null;
		set => this["crb4d_singlelineoftextarea"] = value;
	}
	/// <summary>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// Auto Number Format: 
	/// </summary>
	[AttributeLogicalName("crb4d_singlelineoftextemail")]
	public string SingleLineofTextEmail
	{
		get => TryGetAttributeValue("crb4d_singlelineoftextemail", out string value) ? value : null;
		set => this["crb4d_singlelineoftextemail"] = value;
	}
	/// <summary>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// Auto Number Format: 
	/// </summary>
	[AttributeLogicalName("crb4d_singlelineoftextphonenumber")]
	public string SingleLineofTextPhoneNumber
	{
		get => TryGetAttributeValue("crb4d_singlelineoftextphonenumber", out string value) ? value : null;
		set => this["crb4d_singlelineoftextphonenumber"] = value;
	}
	/// <summary>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// Auto Number Format: 
	/// </summary>
	[AttributeLogicalName("crb4d_singlelineoftextrich")]
	public string SingleLineofTextRich
	{
		get => TryGetAttributeValue("crb4d_singlelineoftextrich", out string value) ? value : null;
		set => this["crb4d_singlelineoftextrich"] = value;
	}
	/// <summary>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// Auto Number Format: 
	/// </summary>
	[AttributeLogicalName("crb4d_singlelineoftexttickersymbol")]
	public string SingleLineofTextTickerSymbol
	{
		get => TryGetAttributeValue("crb4d_singlelineoftexttickersymbol", out string value) ? value : null;
		set => this["crb4d_singlelineoftexttickersymbol"] = value;
	}
	/// <summary>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// Auto Number Format: 
	/// </summary>
	[AttributeLogicalName("crb4d_singlelineoftexturl")]
	public string SingleLineofTextURL
	{
		get => TryGetAttributeValue("crb4d_singlelineoftexturl", out string value) ? value : null;
		set => this["crb4d_singlelineoftexturl"] = value;
	}
	/// <summary>
	/// Required Level: SystemRequired<br/>
	/// Valid for: Create Read</br>
	/// </summary>
	
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("crb4d_timezone")]
	public int? TimeZone
	{
		get => TryGetAttributeValue("crb4d_timezone", out int value) ? value : null;
		set => this["crb4d_timezone"] = value;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("crb4d_wholenumber")]
	public int? WholeNumber
	{
		get => TryGetAttributeValue("crb4d_wholenumber", out int value) ? value : null;
		set => this["crb4d_wholenumber"] = value;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// Targets: systemuser</br>
	/// </summary>
	[AttributeLogicalName("createdby")]
	public EntityReference CreatedBy
	{
		get => TryGetAttributeValue("createdby", out EntityReference value) ? value : null;
	}

	/// <summary>
	/// Attribute of: createdby</br>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("createdbyname")]
	public string CreatedByName
	{
		get => FormattedValues.Contains("createdby") ? FormattedValues["createdby"] : null;
	
	}
	/// <summary>
	/// Attribute of: createdby</br>
	/// Max Length: 100</br>
	/// Required Level: SystemRequired<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("createdbyyominame")]
	public string CreatedByYomiName
	{
		get => FormattedValues.Contains("createdby") ? FormattedValues["createdby"] : null;
	
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("createdon")]
	public DateTime? CreatedOn
	{
		get => TryGetAttributeValue("createdon", out DateTime value) ? value : null;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// Targets: systemuser</br>
	/// </summary>
	[AttributeLogicalName("createdonbehalfby")]
	public EntityReference CreatedOnBehalfBy
	{
		get => TryGetAttributeValue("createdonbehalfby", out EntityReference value) ? value : null;
	}

	/// <summary>
	/// Attribute of: createdonbehalfby</br>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("createdonbehalfbyname")]
	public string CreatedOnBehalfByName
	{
		get => FormattedValues.Contains("createdonbehalfby") ? FormattedValues["createdonbehalfby"] : null;
	
	}
	/// <summary>
	/// Attribute of: createdonbehalfby</br>
	/// Max Length: 100</br>
	/// Required Level: SystemRequired<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("createdonbehalfbyyominame")]
	public string CreatedOnBehalfByYomiName
	{
		get => FormattedValues.Contains("createdonbehalfby") ? FormattedValues["createdonbehalfby"] : null;
	
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("exchangerate")]
	public decimal? ExchangeRate
	{
		get => TryGetAttributeValue("exchangerate", out decimal value) ? value : null;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Read</br>
	/// </summary>
	[AttributeLogicalName("importsequencenumber")]
	public int? ImportSequenceNumber
	{
		get => TryGetAttributeValue("importsequencenumber", out int value) ? value : null;
		set => this["importsequencenumber"] = value;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// Targets: systemuser</br>
	/// </summary>
	[AttributeLogicalName("modifiedby")]
	public EntityReference ModifiedBy
	{
		get => TryGetAttributeValue("modifiedby", out EntityReference value) ? value : null;
	}

	/// <summary>
	/// Attribute of: modifiedby</br>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("modifiedbyname")]
	public string ModifiedByName
	{
		get => FormattedValues.Contains("modifiedby") ? FormattedValues["modifiedby"] : null;
	
	}
	/// <summary>
	/// Attribute of: modifiedby</br>
	/// Max Length: 100</br>
	/// Required Level: SystemRequired<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("modifiedbyyominame")]
	public string ModifiedByYomiName
	{
		get => FormattedValues.Contains("modifiedby") ? FormattedValues["modifiedby"] : null;
	
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("modifiedon")]
	public DateTime? ModifiedOn
	{
		get => TryGetAttributeValue("modifiedon", out DateTime value) ? value : null;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// Targets: systemuser</br>
	/// </summary>
	[AttributeLogicalName("modifiedonbehalfby")]
	public EntityReference ModifiedOnBehalfBy
	{
		get => TryGetAttributeValue("modifiedonbehalfby", out EntityReference value) ? value : null;
	}

	/// <summary>
	/// Attribute of: modifiedonbehalfby</br>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("modifiedonbehalfbyname")]
	public string ModifiedOnBehalfByName
	{
		get => FormattedValues.Contains("modifiedonbehalfby") ? FormattedValues["modifiedonbehalfby"] : null;
	
	}
	/// <summary>
	/// Attribute of: modifiedonbehalfby</br>
	/// Max Length: 100</br>
	/// Required Level: SystemRequired<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("modifiedonbehalfbyyominame")]
	public string ModifiedOnBehalfByYomiName
	{
		get => FormattedValues.Contains("modifiedonbehalfby") ? FormattedValues["modifiedonbehalfby"] : null;
	
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Read</br>
	/// </summary>
	[AttributeLogicalName("overriddencreatedon")]
	public DateTime? OverriddenCreatedOn
	{
		get => TryGetAttributeValue("overriddencreatedon", out DateTime value) ? value : null;
		set => this["overriddencreatedon"] = value;
	}
	/// <summary>
	/// Required Level: SystemRequired<br/>
	/// Valid for: Create Update Read</br>
	/// Targets: systemuser,team</br>
	/// </summary>
	[AttributeLogicalName("ownerid")]
	public EntityReference OwnerId
	{
		get => TryGetAttributeValue("ownerid", out EntityReference value) ? value : null;
		set => this["ownerid"] = value;
	}

	/// <summary>
	/// Attribute of: ownerid</br>
	/// Max Length: 100</br>
	/// Required Level: SystemRequired<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("owneridname")]
	public string OwnerIdName
	{
		get => TryGetAttributeValue("owneridname", out string value) ? value : null;
	}
	/// <summary>
	/// Attribute of: ownerid</br>
	/// Max Length: 100</br>
	/// Required Level: SystemRequired<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("owneridyominame")]
	public string OwnerIdYomiName
	{
		get => TryGetAttributeValue("owneridyominame", out string value) ? value : null;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// Targets: businessunit</br>
	/// </summary>
	[AttributeLogicalName("owningbusinessunit")]
	public EntityReference OwningBusinessUnit
	{
		get => TryGetAttributeValue("owningbusinessunit", out EntityReference value) ? value : null;
	}

	/// <summary>
	/// Attribute of: owningbusinessunit</br>
	/// Max Length: 100</br>
	/// Required Level: SystemRequired<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("owningbusinessunitname")]
	public string OwningBusinessUnitName
	{
		get => FormattedValues.Contains("owningbusinessunit") ? FormattedValues["owningbusinessunit"] : null;
	
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// Targets: team</br>
	/// </summary>
	[AttributeLogicalName("owningteam")]
	public EntityReference OwningTeam
	{
		get => TryGetAttributeValue("owningteam", out EntityReference value) ? value : null;
	}

	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// Targets: systemuser</br>
	/// </summary>
	[AttributeLogicalName("owninguser")]
	public EntityReference OwningUser
	{
		get => TryGetAttributeValue("owninguser", out EntityReference value) ? value : null;
	}

	/// <summary>
	/// Required Level: SystemRequired<br/>
	/// Valid for: Update Read</br>
	/// </summary>
	[AttributeLogicalName("statecode")]
	public Test.Choices.Status? Statecode
	{
		get => TryGetAttributeValue("statecode", out OptionSetValue opt) && opt != null ? (Test.Choices.Status)Enum.ToObject(typeof(Test.Choices.Status), opt.Value) : null;
		set => this["statecode"] = value == null ? null : new OptionSetValue(((IConvertible)value).ToInt32((IFormatProvider)CultureInfo.InvariantCulture));
	}
	/// <summary>
	/// Attribute of: statecode</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("statecodename")]
	public string StatecodeName
	{
		get => FormattedValues.Contains("statecode") ? FormattedValues["statecodename"] : null;
	}

	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("statuscode")]
	public Test.Choices.StatusReason? Statuscode
	{
		get => TryGetAttributeValue("statuscode", out OptionSetValue opt) && opt != null ? (Test.Choices.StatusReason)Enum.ToObject(typeof(Test.Choices.StatusReason), opt.Value) : null;
		set => this["statuscode"] = value == null ? null : new OptionSetValue(((IConvertible)value).ToInt32((IFormatProvider)CultureInfo.InvariantCulture));
	}
	/// <summary>
	/// Attribute of: statuscode</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("statuscodename")]
	public string StatuscodeName
	{
		get => FormattedValues.Contains("statuscode") ? FormattedValues["statuscodename"] : null;
	}

	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("timezoneruleversionnumber")]
	public int? TimeZoneRuleVersionNumber
	{
		get => TryGetAttributeValue("timezoneruleversionnumber", out int value) ? value : null;
		set => this["timezoneruleversionnumber"] = value;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// Targets: transactioncurrency</br>
	/// </summary>
	[AttributeLogicalName("transactioncurrencyid")]
	public EntityReference TransactionCurrencyId
	{
		get => TryGetAttributeValue("transactioncurrencyid", out EntityReference value) ? value : null;
		set
		{
			if (!Test.Fields.TransactionCurrencyIdTargets.Contains(value.LogicalName))
			{
				throw new InvalidPluginExecutionException($"{value.LogicalName}:{value.Id} is not a valid TransactionCurrencyId. The only valid references are transactioncurrency");			
			}
			this["transactioncurrencyid"] = value;
		}
	}

	/// <summary>
	/// Attribute of: transactioncurrencyid</br>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("transactioncurrencyidname")]
	public string TransactionCurrencyIdName
	{
		get => FormattedValues.Contains("transactioncurrencyid") ? FormattedValues["transactioncurrencyid"] : null;
	
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("utcconversiontimezonecode")]
	public int? UTCConversionTimeZoneCode
	{
		get => TryGetAttributeValue("utcconversiontimezonecode", out int value) ? value : null;
		set => this["utcconversiontimezonecode"] = value;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("versionnumber")]
	public long? VersionNumber
	{
		get => TryGetAttributeValue("versionnumber", out long value) ? value : null;
	}
	public Test() : base(EntityLogicalName) { }
    public Test(string keyName, object keyValue) : base(EntityLogicalName, keyName, keyValue) { }
    public Test(KeyAttributeCollection keyAttributes) : base(EntityLogicalName, keyAttributes) { }
}

[EntityLogicalName("beone_request")]
public class Request : Entity
{
	public const string EntityLogicalName = "beone_request";
	public const string EntityLogicalCollectionName = "beone_requests";
	public const string EntitySetName = "beone_requests";
	public const int EntityTypeCode = 10094;
	public const string PrimaryNameAttribute = "beone_name";
	public const string PrimaryIdAttribute = "beone_requestid";

	public partial class Fields
	{
		public const string Actgmotivelabel = "beone_actgmotivelabel";
		public const string Actiontypeid = "beone_actiontypeid";
		public static readonly ReadOnlyCollection<string> ActiontypeidTargets = new (["ag_actiontype"]);
		public const string ActiontypeidName = "beone_actiontypeidname";
		public const string Activity = "beone_activity";
		public const string Adagionumber = "beone_adagionumber";
		public const string Agolstatus = "beone_agolstatus";
		public const string AgolstatusName = "beone_agolstatusname";
		public const string Annulationdate = "beone_annulationdate";
		public const string Awmessagetype = "beone_awmessagetype";
		public const string AwmessagetypeName = "beone_awmessagetypename";
		public const string Awproducts = "beone_awproducts";
		public const string BCENumber = "beone_bcenumber";
		public const string Brokerfeedback = "beone_brokerfeedback";
		public const string BrokerfeedbackName = "beone_brokerfeedbackname";
		public const string Claimhistoryid = "beone_claimhistoryid";
		public static readonly ReadOnlyCollection<string> ClaimhistoryidTargets = new (["beone_claimhistory"]);
		public const string ClaimhistoryidName = "beone_claimhistoryidname";
		public const string Closesla = "beone_closesla";
		public const string CloseslaName = "beone_closeslaname";
		public const string Config = "beone_config";
		public static readonly ReadOnlyCollection<string> ConfigTargets = new (["beone_requesttypeconfig"]);
		public const string ConfigName = "beone_configname";
		public const string Connexity = "beone_connexity";
		public const string ConnexityName = "beone_connexityname";
		public const string Contractid = "beone_contractid";
		public static readonly ReadOnlyCollection<string> ContractidTargets = new (["beone_contract"]);
		public const string ContractidName = "beone_contractidname";
		public const string Contractlist = "beone_contractlist";
		public const string Contractnumber = "beone_contractnumber";
		public const string CustomerName = "beone_customername";
		public const string Datein = "beone_datein";
		public const string Declarationnumber1 = "beone_declarationnumber1";
		public const string Declarationnumber2 = "beone_declarationnumber2";
		public const string Declarationnumbers = "beone_declarationnumbers";
		public const string Description = "beone_description";
		public const string Domain = "beone_domain";
		public const string Domainaccident_amount = "beone_domainaccident_amount";
		public const string Domaincar_amount = "beone_domaincar_amount";
		public const string Domainfire_amount = "beone_domainfire_amount";
		public const string Domainfireengineering_amount = "beone_domainfireengineering_amount";
		public const string Domainlaw_amount = "beone_domainlaw_amount";
		public const string Domainliability_amount = "beone_domainliability_amount";
		public const string DomainName = "beone_domainname";
		public const string Dossierid = "beone_dossierid";
		public static readonly ReadOnlyCollection<string> DossieridTargets = new (["beone_dossier"]);
		public const string DossieridName = "beone_dossieridname";
		public const string Dossiernumber = "beone_dossiernumber";
		public const string Duedate = "beone_duedate";
		public const string Entityid = "beone_entityid";
		public static readonly ReadOnlyCollection<string> EntityidTargets = new (["team"]);
		public const string EntityidName = "beone_entityidname";
		public const string EntityidYomiName = "beone_entityidyominame";
		public const string Externalofferref = "beone_externalofferref";
		public const string Externalrework = "beone_externalrework";
		public const string ExternalreworkName = "beone_externalreworkname";
		public const string Firsttodounderwriterid = "beone_firsttodounderwriterid";
		public static readonly ReadOnlyCollection<string> FirsttodounderwriteridTargets = new (["beone_underwriter"]);
		public const string FirsttodounderwriteridName = "beone_firsttodounderwriteridname";
		public const string Fleetnumber = "beone_fleetnumber";
		public const string Forcedduedate = "beone_forcedduedate";
		public const string Groupofferid = "beone_groupofferid";
		public static readonly ReadOnlyCollection<string> GroupofferidTargets = new (["beone_groupoffer"]);
		public const string GroupofferidName = "beone_groupofferidname";
		public const string Importance = "beone_importance";
		public const string ImportanceName = "beone_importancename";
		public const string Internalexternal = "beone_internalexternal";
		public const string InternalexternalName = "beone_internalexternalname";
		public const string Internalrework = "beone_internalrework";
		public const string InternalreworkName = "beone_internalreworkname";
		public const string Isautoclose = "beone_isautoclose";
		public const string IsautocloseName = "beone_isautoclosename";
		public const string IsUrgent = "beone_isurgent";
		public const string IsurgentName = "beone_isurgentname";
		public const string Json = "beone_json";
		public const string Licenseplate = "beone_licenseplate";
		public const string Mailboxid = "beone_mailboxid";
		public const string Maindomain = "beone_maindomain";
		public const string Maindomaingroup = "beone_maindomaingroup";
		public const string MaindomaingroupName = "beone_maindomaingroupname";
		public const string MaindomainName = "beone_maindomainname";
		public const string Messageid = "beone_messageid";
		public const string Modifiedonbehalfby = "beone_modifiedonbehalfby";
		public static readonly ReadOnlyCollection<string> ModifiedonbehalfbyTargets = new (["systemuser"]);
		public const string ModifiedonbehalfbyName = "beone_modifiedonbehalfbyname";
		public const string ModifiedonbehalfbyYomiName = "beone_modifiedonbehalfbyyominame";
		public const string Motive = "beone_motive";
		public const string MotiveName = "beone_motivename";
		public const string Multiregardingid = "beone_multiregardingid";
		public static readonly ReadOnlyCollection<string> MultiregardingidTargets = new (["beone_multiregardingrequest"]);
		public const string MultiregardingidName = "beone_multiregardingidname";
		public const string Naceid = "beone_naceid";
		public static readonly ReadOnlyCollection<string> NaceidTargets = new (["beone_nace"]);
		public const string NaceidName = "beone_naceidname";
		public const string Name = "beone_name";
		public const string Ncmpproid = "beone_ncmpproid";
		public static readonly ReadOnlyCollection<string> NcmpproidTargets = new (["beone_ncmppro"]);
		public const string NcmpproidName = "beone_ncmpproidname";
		public const string Newslainstanceid = "beone_newslainstanceid";
		public static readonly ReadOnlyCollection<string> NewslainstanceidTargets = new (["beone_slainstance"]);
		public const string NewslainstanceidName = "beone_newslainstanceidname";
		public const string Offerpriority = "beone_offerpriority";
		public const string OfferpriorityName = "beone_offerpriorityname";
		public const string Onholdduration = "beone_onholdduration";
		public const string Onholdreason = "beone_onholdreason";
		public const string OnholdreasonName = "beone_onholdreasonname";
		public const string Origin = "beone_origin";
		public const string Originalawmessageid = "beone_originalawmessageid";
		public static readonly ReadOnlyCollection<string> OriginalawmessageidTargets = new (["beone_awmessage"]);
		public const string OriginalawmessageidName = "beone_originalawmessageidname";
		public const string Originaldocument = "beone_originaldocument";
		public const string OriginalEmailId = "beone_originalemailid";
		public static readonly ReadOnlyCollection<string> OriginalEmailIdTargets = new (["email"]);
		public const string OriginalEmailIdName = "beone_originalemailidname";
		public const string Originalletterid = "beone_originalletterid";
		public static readonly ReadOnlyCollection<string> OriginalletteridTargets = new (["letter"]);
		public const string OriginalletteridName = "beone_originalletteridname";
		public const string Originalmpbid = "beone_originalmpbid";
		public static readonly ReadOnlyCollection<string> OriginalmpbidTargets = new (["beone_mpb"]);
		public const string OriginalmpbidName = "beone_originalmpbidname";
		public const string Originalphonecallid = "beone_originalphonecallid";
		public static readonly ReadOnlyCollection<string> OriginalphonecallidTargets = new (["phonecall"]);
		public const string OriginalphonecallidName = "beone_originalphonecallidname";
		public const string Originalsenderemailaddress = "beone_originalsenderemailaddress";
		public const string OriginName = "beone_originname";
		public const string Processexpw = "beone_processexpw";
		public const string Productid = "beone_productid";
		public static readonly ReadOnlyCollection<string> ProductidTargets = new (["product"]);
		public const string ProductidName = "beone_productidname";
		public const string Productids = "beone_productids";
		public const string Pruningdecision = "beone_pruningdecision";
		public const string PruningdecisionName = "beone_pruningdecisionname";
		public const string Pruningid = "beone_pruningid";
		public static readonly ReadOnlyCollection<string> PruningidTargets = new (["beone_pruning"]);
		public const string PruningidName = "beone_pruningidname";
		public const string RequestId = "beone_requestid";
		public const string Requesttypeid = "beone_requesttypeid";
		public static readonly ReadOnlyCollection<string> RequesttypeidTargets = new (["ag_requesttype"]);
		public const string RequesttypeidName = "beone_requesttypeidname";
		public const string Signature = "beone_signature";
		public const string SignatureName = "beone_signaturename";
		public const string Sladuration = "beone_sladuration";
		public const string SladurationName = "beone_sladurationname";
		public const string Slainstanceid = "beone_slainstanceid";
		public static readonly ReadOnlyCollection<string> SlainstanceidTargets = new (["slakpiinstance"]);
		public const string SlainstanceidName = "beone_slainstanceidname";
		public const string Slakpiinstancestatus = "beone_slakpiinstancestatus";
		public const string SlakpiinstancestatusName = "beone_slakpiinstancestatusname";
		public const string Slasuccessfailuredate = "beone_slasuccessfailuredate";
		public const string Specificdate = "beone_specificdate";
		public const string Totalproducts = "beone_totalproducts";
		public const string VerifyBroker = "beone_verifybroker";
		public const string VerifybrokerName = "beone_verifybrokername";
		public const string CreatedBy = "createdby";
		public static readonly ReadOnlyCollection<string> CreatedByTargets = new (["systemuser"]);
		public const string CreatedByName = "createdbyname";
		public const string CreatedByYomiName = "createdbyyominame";
		public const string CreatedOn = "createdon";
		public const string CreatedOnBehalfBy = "createdonbehalfby";
		public static readonly ReadOnlyCollection<string> CreatedOnBehalfByTargets = new (["systemuser"]);
		public const string CreatedOnBehalfByName = "createdonbehalfbyname";
		public const string CreatedOnBehalfByYomiName = "createdonbehalfbyyominame";
		public const string ImportSequenceNumber = "importsequencenumber";
		public const string LastOnHoldTime = "lastonholdtime";
		public const string ModifiedBy = "modifiedby";
		public static readonly ReadOnlyCollection<string> ModifiedByTargets = new (["systemuser"]);
		public const string ModifiedByName = "modifiedbyname";
		public const string ModifiedByYomiName = "modifiedbyyominame";
		public const string ModifiedOn = "modifiedon";
		public const string ModifiedOnBehalfBy = "modifiedonbehalfby";
		public static readonly ReadOnlyCollection<string> ModifiedOnBehalfByTargets = new (["systemuser"]);
		public const string ModifiedOnBehalfByName = "modifiedonbehalfbyname";
		public const string ModifiedOnBehalfByYomiName = "modifiedonbehalfbyyominame";
		public const string OnHoldTime = "onholdtime";
		public const string OverriddenCreatedOn = "overriddencreatedon";
		public const string OwnerId = "ownerid";
		public const string OwnerIdName = "owneridname";
		public const string OwnerIdYomiName = "owneridyominame";
		public const string OwningBusinessUnit = "owningbusinessunit";
		public static readonly ReadOnlyCollection<string> OwningBusinessUnitTargets = new (["businessunit"]);
		public const string OwningBusinessUnitName = "owningbusinessunitname";
		public const string OwningTeam = "owningteam";
		public static readonly ReadOnlyCollection<string> OwningTeamTargets = new (["team"]);
		public const string OwningUser = "owninguser";
		public static readonly ReadOnlyCollection<string> OwningUserTargets = new (["systemuser"]);
		public const string SLAId = "slaid";
		public static readonly ReadOnlyCollection<string> SLAIdTargets = new (["sla"]);
		public const string SLAInvokedId = "slainvokedid";
		public static readonly ReadOnlyCollection<string> SLAInvokedIdTargets = new (["sla"]);
		public const string SLAInvokedIdName = "slainvokedidname";
		public const string SLAName = "slaname";
		public const string Statecode = "statecode";
		public const string StatecodeName = "statecodename";
		public const string Statuscode = "statuscode";
		public const string StatuscodeName = "statuscodename";
		public const string TimeZoneRuleVersionNumber = "timezoneruleversionnumber";
		public const string UTCConversionTimeZoneCode = "utcconversiontimezonecode";
		public const string VersionNumber = "versionnumber";
	}

	public partial class Choices
	{
		[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
		[DataContract]
		public enum AgolStatus
		{
			[EnumMember]
			NewRequestCreated = 140140000,
			[EnumMember]
			RequestOngoing = 140140001,
			[EnumMember]
			RefusalSent = 140140002,
			[EnumMember]
			OfferSent = 140140003,
			[EnumMember]
			BrokerFeedbackReceivedRealization = 140140004,
			[EnumMember]
			BrokerFeedbackReceivedRefusedByClient = 140140005,
			[EnumMember]
			BrokerFeedbackReceivedAskModification = 140140006,
			[EnumMember]
			BrokerFeedbackReceivedAskToAddProduct = 140140007,
			[EnumMember]
			NoBrokerFeedbackReceived = 140140008,
			[EnumMember]
			BrokerFeedbackReceivedAskToResendOffer = 140140009,
			[EnumMember]
			OnHoldForInspection = 140140010,
			[EnumMember]
			OnHoldForInternalMissingInfo = 140140011,
			[EnumMember]
			OnHoldForBrokerMissingInfo = 140140012,
			[EnumMember]
			RequestCanceled = 140140013,
			[EnumMember]
			RequestCompleted = 140140014,
		}
		[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
		[DataContract]
		public enum AwMessageType
		{
			[EnumMember]
			Pw = 140140000,
			[EnumMember]
			Wdc = 140140001,
			[EnumMember]
			Cc = 140140002,
			[EnumMember]
			Agol = 140140003,
			[EnumMember]
			Vf = 140140004,
		}
		[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
		[DataContract]
		public enum BrokerFeedback
		{
			[EnumMember]
			Realization = 100000000,
			[EnumMember]
			RefusedByClient = 100000001,
			[EnumMember]
			NoAnswer = 100000002,
			[EnumMember]
			ReSendOffer = 100000003,
			[EnumMember]
			Modification = 100000004,
			[EnumMember]
			AddProduct = 100000005,
		}
		/// <summary>
		/// Type of flow  /!\ -- CHANGING THIS OPTIONSET ?? ALSO CHANGE THE FLOW DOMAIN ACCORDINGLY -- /!\
		/// </summary>
		[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
		[DataContract]
		public enum FlowConnexity
		{
			[EnumMember]
			Standalone = 140140000,
			[EnumMember]
			Modulis = 140140001,
			[EnumMember]
			ModulisEasy = 140140002,
			[EnumMember]
			Familis = 140140003,
			[EnumMember]
			Fleet = 140140004,
			[EnumMember]
			AdagioOffer = 140140005,
			[EnumMember]
			FleetOffer = 140140006,
		}
		/// <summary>
		/// Domain of the offer task  /!\ -- CHANGING THIS OPTIONSET for connexity values ?? ALSO CHANGE THE FLOW Connexity ACCORDINGLY -- /!\ Rule : Domain Connexity = Flow Connexity + 10000000
		/// </summary>
		[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
		[DataContract]
		public enum FlowTaskDomain
		{
			[EnumMember]
			Auto = 140140000,
			[EnumMember]
			Fire = 140140001,
			[EnumMember]
			FireEngineering = 140140002,
			[EnumMember]
			Law = 140140003,
			[EnumMember]
			Accident = 140140004,
			[EnumMember]
			Liability = 140140005,
			[EnumMember]
			ConnexityModulis = 150140001,
			[EnumMember]
			ConnexityModuliseasy = 150140002,
			[EnumMember]
			ConnexityFamilis = 150140003,
			[EnumMember]
			ConnexityFleet = 150140004,
			[EnumMember]
			ConnexityAdagioOffer = 150140005,
		}
		[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
		[DataContract]
		public enum RequestImportance
		{
			[EnumMember]
			_1 = 1,
			[EnumMember]
			_2 = 2,
			[EnumMember]
			_3 = 3,
			[EnumMember]
			_4 = 4,
			[EnumMember]
			_5 = 5,
			[EnumMember]
			_6 = 6,
			[EnumMember]
			_7 = 7,
			[EnumMember]
			_8 = 8,
			[EnumMember]
			_9 = 9,
		}
		[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
		[DataContract]
		public enum InternalExternal
		{
			[EnumMember]
			Internal = 100000000,
			[EnumMember]
			External = 100000001,
		}
		[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
		[DataContract]
		public enum MainDomain
		{
			[EnumMember]
			Auto = 140140000,
			[EnumMember]
			Fire = 140140001,
			[EnumMember]
			FireEngineering = 140140002,
			[EnumMember]
			Law = 140140003,
			[EnumMember]
			Accident = 140140004,
			[EnumMember]
			Liability = 140140005,
		}
		[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
		[DataContract]
		public enum MainDomainGroup
		{
			[EnumMember]
			Auto = 100000000,
			[EnumMember]
			AtRc = 100000001,
			[EnumMember]
			Incendie = 100000002,
		}
		[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
		[DataContract]
		public enum Motive
		{
			[EnumMember]
			Classic = 140140000,
			[EnumMember]
			TariffIncrease = 140140001,
			[EnumMember]
			Sinister = 140140002,
		}
		/// <summary>
		/// Priority from FAST. Impacts reminder
		/// </summary>
		[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
		[DataContract]
		public enum FlowPriority
		{
			[EnumMember]
			High = 140140000,
			[EnumMember]
			Medium = 140140001,
			[EnumMember]
			Low = 140140002,
		}
		[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
		[DataContract]
		public enum OnHoldReason
		{
			[EnumMember]
			InspectionNeeded = 140140001,
			[EnumMember]
			BrokerInfoRequested = 140140002,
			[EnumMember]
			InternalInfoRequested = 140140003,
		}
		[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
		[DataContract]
		public enum RequestOrigin
		{
			[EnumMember]
			Email = 140140000,
			[EnumMember]
			Agis = 140140001,
			[EnumMember]
			PhoneCall = 140140002,
			[EnumMember]
			Post = 140140003,
			[EnumMember]
			Mpb = 140140004,
			[EnumMember]
			Letter = 140140005,
			[EnumMember]
			AwMessage = 140140006,
			[EnumMember]
			Manual = 140140007,
			[EnumMember]
			AgOnlineForm = 140140008,
		}
		[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
		[DataContract]
		public enum PruningDecision
		{
			[EnumMember]
			ContractAlreadyCanceled = 100000005,
			[EnumMember]
			MovingContract = 100000006,
			[EnumMember]
			PartialTermination = 100000007,
			[EnumMember]
			ModifyDeductible = 100000008,
			[EnumMember]
			ModifyWarranty = 100000009,
			[EnumMember]
			ModifyConditions = 100000010,
			[EnumMember]
			PremiumReview = 100000011,
			[EnumMember]
			PreventionMeasures = 100000012,
			[EnumMember]
			MeasureCombination = 100000013,
			[EnumMember]
			DecreaseParticipation = 100000014,
			[EnumMember]
			EndProcedure = 100000000,
			[EnumMember]
			ContractModification = 100000001,
			[EnumMember]
			ContractTermination = 100000002,
			[EnumMember]
			WarningLetter = 100000003,
			[EnumMember]
			Surveillance = 100000004,
		}
		[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
		[DataContract]
		public enum Boolean
		{
			[EnumMember]
			No = 0,
			[EnumMember]
			Yes = 1,
		}
		/// <summary>
		/// /!\ !!! USE 8 HOURS PER DAY IN THE OPTIONSET VALUE !!!!!!! /!\
		/// </summary>
		[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
		[DataContract]
		public enum SlaDuration
		{
			[EnumMember]
			_4BusinessHours = 4,
			[EnumMember]
			_1BusinessDay = 8,
			[EnumMember]
			_2BusinessDays = 16,
			[EnumMember]
			_3BusinessDays = 24,
			[EnumMember]
			_4BusinessDays = 32,
			[EnumMember]
			_5BusinessDays = 40,
			[EnumMember]
			_10BusinessDays = 80,
			[EnumMember]
			_15BusinessDays = 120,
			[EnumMember]
			_20BusinessDays = 160,
			[EnumMember]
			_30BusinessDays = 240,
			[EnumMember]
			_40BusinessDays = 320,
			[EnumMember]
			_50BusinessDays = 400,
			[EnumMember]
			_60BusinessDays = 480,
			[EnumMember]
			_80BusinessDays = 640,
			[EnumMember]
			_365BusinessDays = 2920,
		}
		/// <summary>
		/// !!!!!!!!KEEP IN SYNC WITH SLA KPI INSTANCE STATUS FIELD!!!!!!!
		/// </summary>
		[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
		[DataContract]
		public enum SlaKpiInstanceStatus
		{
			[EnumMember]
			InProgress = 0,
			[EnumMember]
			Noncompliant = 1,
			[EnumMember]
			NearingNoncompliance = 2,
			[EnumMember]
			Paused = 3,
			[EnumMember]
			Succeeded = 4,
			[EnumMember]
			Canceled = 5,
		}
		/// <summary>
		/// Status of the Request
		/// </summary>
		[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
		[DataContract]
		public enum Status
		{
			[EnumMember]
			Active = 0,
			[EnumMember]
			Inactive = 1,
		}
		/// <summary>
		/// Reason for the status of the Request
		/// </summary>
		[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
		[DataContract]
		public enum StatusReason
		{
			[EnumMember]
			New = 1,
			[EnumMember]
			Ongoing = 140140000,
			[EnumMember]
			OnHold = 140140001,
			[EnumMember]
			Canceled = 2,
			[EnumMember]
			Completed = 140140002,
		}
	}

	/// <summary>
	/// Max Length: 200</br>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_actgmotivelabel")]
	public string Actgmotivelabel
	{
		get => TryGetAttributeValue("beone_actgmotivelabel", out string value) ? value : null;
		set => this["beone_actgmotivelabel"] = value;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// Targets: ag_actiontype</br>
	/// </summary>
	[AttributeLogicalName("beone_actiontypeid")]
	public EntityReference Actiontypeid
	{
		get => TryGetAttributeValue("beone_actiontypeid", out EntityReference value) ? value : null;
		set
		{
			if (!Request.Fields.ActiontypeidTargets.Contains(value.LogicalName))
			{
				throw new InvalidPluginExecutionException($"{value.LogicalName}:{value.Id} is not a valid Actiontypeid. The only valid references are ag_actiontype");			
			}
			this["beone_actiontypeid"] = value;
		}
	}

	/// <summary>
	/// Attribute of: beone_actiontypeid</br>
	/// Max Length: 600</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_actiontypeidname")]
	public string ActiontypeidName
	{
		get => FormattedValues.Contains("beone_actiontypeid") ? FormattedValues["beone_actiontypeid"] : null;
	
	}
	/// <summary>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// Auto Number Format: 
	/// </summary>
	[AttributeLogicalName("beone_activity")]
	public string Activity
	{
		get => TryGetAttributeValue("beone_activity", out string value) ? value : null;
		set => this["beone_activity"] = value;
	}
	/// <summary>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_adagionumber")]
	public string Adagionumber
	{
		get => TryGetAttributeValue("beone_adagionumber", out string value) ? value : null;
		set => this["beone_adagionumber"] = value;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_agolstatus")]
	public Request.Choices.AgolStatus? Agolstatus
	{
		get => TryGetAttributeValue("beone_agolstatus", out OptionSetValue opt) && opt == null ? null : (Request.Choices.AgolStatus)opt.Value;
		set => this["beone_agolstatus"] = value == null ? null : new OptionSetValue((int)value);
	}
	/// <summary>
	/// Attribute of: beone_agolstatus</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_agolstatusname")]
	public string AgolstatusName
	{
		get => FormattedValues.Contains("beone_agolstatus") ? FormattedValues["beone_agolstatusname"] : null;
	}

	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_annulationdate")]
	public DateTime? Annulationdate
	{
		get => TryGetAttributeValue("beone_annulationdate", out DateTime value) ? value : null;
		set => this["beone_annulationdate"] = value;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_awmessagetype")]
	public Request.Choices.AwMessageType? Awmessagetype
	{
		get => TryGetAttributeValue("beone_awmessagetype", out OptionSetValue opt) && opt == null ? null : (Request.Choices.AwMessageType)opt.Value;
		set => this["beone_awmessagetype"] = value == null ? null : new OptionSetValue((int)value);
	}
	/// <summary>
	/// Attribute of: beone_awmessagetype</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_awmessagetypename")]
	public string AwmessagetypeName
	{
		get => FormattedValues.Contains("beone_awmessagetype") ? FormattedValues["beone_awmessagetypename"] : null;
	}

	/// <summary>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_awproducts")]
	public string Awproducts
	{
		get => TryGetAttributeValue("beone_awproducts", out string value) ? value : null;
		set => this["beone_awproducts"] = value;
	}
	/// <summary>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// Auto Number Format: 
	/// </summary>
	[AttributeLogicalName("beone_bcenumber")]
	public string BCENumber
	{
		get => TryGetAttributeValue("beone_bcenumber", out string value) ? value : null;
		set => this["beone_bcenumber"] = value;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_brokerfeedback")]
	public Request.Choices.BrokerFeedback? Brokerfeedback
	{
		get => TryGetAttributeValue("beone_brokerfeedback", out OptionSetValue opt) && opt == null ? null : (Request.Choices.BrokerFeedback)opt.Value;
		set => this["beone_brokerfeedback"] = value == null ? null : new OptionSetValue((int)value);
	}
	/// <summary>
	/// Attribute of: beone_brokerfeedback</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_brokerfeedbackname")]
	public string BrokerfeedbackName
	{
		get => FormattedValues.Contains("beone_brokerfeedback") ? FormattedValues["beone_brokerfeedbackname"] : null;
	}

	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// Targets: beone_claimhistory</br>
	/// </summary>
	[AttributeLogicalName("beone_claimhistoryid")]
	public EntityReference Claimhistoryid
	{
		get => TryGetAttributeValue("beone_claimhistoryid", out EntityReference value) ? value : null;
		set
		{
			if (!Request.Fields.ClaimhistoryidTargets.Contains(value.LogicalName))
			{
				throw new InvalidPluginExecutionException($"{value.LogicalName}:{value.Id} is not a valid Claimhistoryid. The only valid references are beone_claimhistory");			
			}
			this["beone_claimhistoryid"] = value;
		}
	}

	/// <summary>
	/// Attribute of: beone_claimhistoryid</br>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_claimhistoryidname")]
	public string ClaimhistoryidName
	{
		get => FormattedValues.Contains("beone_claimhistoryid") ? FormattedValues["beone_claimhistoryid"] : null;
	
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_closesla")]
	public bool? Closesla
	{
		get => TryGetAttributeValue("beone_closesla", out bool value) ? value : null;
		set => this["beone_closesla"] = value;
	}
	/// <summary>
	/// Attribute of: beone_closesla</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_closeslaname")]
	public string CloseslaName
	{
		get => FormattedValues.Contains("beone_closesla") ? FormattedValues["beone_closeslaname"] : null;
	}

	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// Targets: beone_requesttypeconfig</br>
	/// </summary>
	[AttributeLogicalName("beone_config")]
	public EntityReference Config
	{
		get => TryGetAttributeValue("beone_config", out EntityReference value) ? value : null;
		set
		{
			if (!Request.Fields.ConfigTargets.Contains(value.LogicalName))
			{
				throw new InvalidPluginExecutionException($"{value.LogicalName}:{value.Id} is not a valid Config. The only valid references are beone_requesttypeconfig");			
			}
			this["beone_config"] = value;
		}
	}

	/// <summary>
	/// Attribute of: beone_config</br>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_configname")]
	public string ConfigName
	{
		get => FormattedValues.Contains("beone_config") ? FormattedValues["beone_config"] : null;
	
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_connexity")]
	public Request.Choices.FlowConnexity? Connexity
	{
		get => TryGetAttributeValue("beone_connexity", out OptionSetValue opt) && opt == null ? null : (Request.Choices.FlowConnexity)opt.Value;
		set => this["beone_connexity"] = value == null ? null : new OptionSetValue((int)value);
	}
	/// <summary>
	/// Attribute of: beone_connexity</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_connexityname")]
	public string ConnexityName
	{
		get => FormattedValues.Contains("beone_connexity") ? FormattedValues["beone_connexityname"] : null;
	}

	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// Targets: beone_contract</br>
	/// </summary>
	[AttributeLogicalName("beone_contractid")]
	public EntityReference Contractid
	{
		get => TryGetAttributeValue("beone_contractid", out EntityReference value) ? value : null;
		set
		{
			if (!Request.Fields.ContractidTargets.Contains(value.LogicalName))
			{
				throw new InvalidPluginExecutionException($"{value.LogicalName}:{value.Id} is not a valid Contractid. The only valid references are beone_contract");			
			}
			this["beone_contractid"] = value;
		}
	}

	/// <summary>
	/// Attribute of: beone_contractid</br>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_contractidname")]
	public string ContractidName
	{
		get => FormattedValues.Contains("beone_contractid") ? FormattedValues["beone_contractid"] : null;
	
	}
	/// <summary>
	/// Max Length: 500</br>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_contractlist")]
	public string Contractlist
	{
		get => TryGetAttributeValue("beone_contractlist", out string value) ? value : null;
		set => this["beone_contractlist"] = value;
	}
	/// <summary>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_contractnumber")]
	public string Contractnumber
	{
		get => TryGetAttributeValue("beone_contractnumber", out string value) ? value : null;
		set => this["beone_contractnumber"] = value;
	}
	/// <summary>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// Auto Number Format: 
	/// </summary>
	[AttributeLogicalName("beone_customername")]
	public string CustomerName
	{
		get => TryGetAttributeValue("beone_customername", out string value) ? value : null;
		set => this["beone_customername"] = value;
	}
	/// <summary>
	/// Required Level: ApplicationRequired<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_datein")]
	public DateTime? Datein
	{
		get => TryGetAttributeValue("beone_datein", out DateTime value) ? value : null;
		set => this["beone_datein"] = value;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_declarationnumber1")]
	public int? Declarationnumber1
	{
		get => TryGetAttributeValue("beone_declarationnumber1", out int value) ? value : null;
		set => this["beone_declarationnumber1"] = value;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_declarationnumber2")]
	public int? Declarationnumber2
	{
		get => TryGetAttributeValue("beone_declarationnumber2", out int value) ? value : null;
		set => this["beone_declarationnumber2"] = value;
	}
	/// <summary>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_declarationnumbers")]
	public string Declarationnumbers
	{
		get => TryGetAttributeValue("beone_declarationnumbers", out string value) ? value : null;
		set => this["beone_declarationnumbers"] = value;
	}
	/// <summary>
	/// Max Length: 2000</br>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_description")]
	public string Description
	{
		get => TryGetAttributeValue("beone_description", out string value) ? value : null;
		set => this["beone_description"] = value;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_domain")]
	public Request.Choices.FlowTaskDomain? Domain
	{
		get => TryGetAttributeValue("beone_domain", out OptionSetValue opt) && opt == null ? null : (Request.Choices.FlowTaskDomain)opt.Value;
		set => this["beone_domain"] = value == null ? null : new OptionSetValue((int)value);
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_domainaccident_amount")]
	public int? Domainaccident_amount
	{
		get => TryGetAttributeValue("beone_domainaccident_amount", out int value) ? value : null;
		set => this["beone_domainaccident_amount"] = value;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_domaincar_amount")]
	public int? Domaincar_amount
	{
		get => TryGetAttributeValue("beone_domaincar_amount", out int value) ? value : null;
		set => this["beone_domaincar_amount"] = value;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_domainfire_amount")]
	public int? Domainfire_amount
	{
		get => TryGetAttributeValue("beone_domainfire_amount", out int value) ? value : null;
		set => this["beone_domainfire_amount"] = value;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_domainfireengineering_amount")]
	public int? Domainfireengineering_amount
	{
		get => TryGetAttributeValue("beone_domainfireengineering_amount", out int value) ? value : null;
		set => this["beone_domainfireengineering_amount"] = value;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_domainlaw_amount")]
	public int? Domainlaw_amount
	{
		get => TryGetAttributeValue("beone_domainlaw_amount", out int value) ? value : null;
		set => this["beone_domainlaw_amount"] = value;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_domainliability_amount")]
	public int? Domainliability_amount
	{
		get => TryGetAttributeValue("beone_domainliability_amount", out int value) ? value : null;
		set => this["beone_domainliability_amount"] = value;
	}
	/// <summary>
	/// Attribute of: beone_domain</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_domainname")]
	public string DomainName
	{
		get => FormattedValues.Contains("beone_domain") ? FormattedValues["beone_domainname"] : null;
	}

	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// Targets: beone_dossier</br>
	/// </summary>
	[AttributeLogicalName("beone_dossierid")]
	public EntityReference Dossierid
	{
		get => TryGetAttributeValue("beone_dossierid", out EntityReference value) ? value : null;
		set
		{
			if (!Request.Fields.DossieridTargets.Contains(value.LogicalName))
			{
				throw new InvalidPluginExecutionException($"{value.LogicalName}:{value.Id} is not a valid Dossierid. The only valid references are beone_dossier");			
			}
			this["beone_dossierid"] = value;
		}
	}

	/// <summary>
	/// Attribute of: beone_dossierid</br>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_dossieridname")]
	public string DossieridName
	{
		get => FormattedValues.Contains("beone_dossierid") ? FormattedValues["beone_dossierid"] : null;
	
	}
	/// <summary>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_dossiernumber")]
	public string Dossiernumber
	{
		get => TryGetAttributeValue("beone_dossiernumber", out string value) ? value : null;
		set => this["beone_dossiernumber"] = value;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_duedate")]
	public DateTime? Duedate
	{
		get => TryGetAttributeValue("beone_duedate", out DateTime value) ? value : null;
		set => this["beone_duedate"] = value;
	}
	/// <summary>
	/// Required Level: ApplicationRequired<br/>
	/// Valid for: Create Update Read</br>
	/// Targets: team</br>
	/// </summary>
	[AttributeLogicalName("beone_entityid")]
	public EntityReference Entityid
	{
		get => TryGetAttributeValue("beone_entityid", out EntityReference value) ? value : null;
		set
		{
			if (!Request.Fields.EntityidTargets.Contains(value.LogicalName))
			{
				throw new InvalidPluginExecutionException($"{value.LogicalName}:{value.Id} is not a valid Entityid. The only valid references are team");			
			}
			this["beone_entityid"] = value;
		}
	}

	/// <summary>
	/// Attribute of: beone_entityid</br>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_entityidname")]
	public string EntityidName
	{
		get => FormattedValues.Contains("beone_entityid") ? FormattedValues["beone_entityid"] : null;
	
	}
	/// <summary>
	/// Attribute of: beone_entityid</br>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_entityidyominame")]
	public string EntityidYomiName
	{
		get => FormattedValues.Contains("beone_entityid") ? FormattedValues["beone_entityid"] : null;
	
	}
	/// <summary>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_externalofferref")]
	public string Externalofferref
	{
		get => TryGetAttributeValue("beone_externalofferref", out string value) ? value : null;
		set => this["beone_externalofferref"] = value;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_externalrework")]
	public bool? Externalrework
	{
		get => TryGetAttributeValue("beone_externalrework", out bool value) ? value : null;
		set => this["beone_externalrework"] = value;
	}
	/// <summary>
	/// Attribute of: beone_externalrework</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_externalreworkname")]
	public string ExternalreworkName
	{
		get => FormattedValues.Contains("beone_externalrework") ? FormattedValues["beone_externalreworkname"] : null;
	}

	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// Targets: beone_underwriter</br>
	/// </summary>
	[AttributeLogicalName("beone_firsttodounderwriterid")]
	public EntityReference Firsttodounderwriterid
	{
		get => TryGetAttributeValue("beone_firsttodounderwriterid", out EntityReference value) ? value : null;
		set
		{
			if (!Request.Fields.FirsttodounderwriteridTargets.Contains(value.LogicalName))
			{
				throw new InvalidPluginExecutionException($"{value.LogicalName}:{value.Id} is not a valid Firsttodounderwriterid. The only valid references are beone_underwriter");			
			}
			this["beone_firsttodounderwriterid"] = value;
		}
	}

	/// <summary>
	/// Attribute of: beone_firsttodounderwriterid</br>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_firsttodounderwriteridname")]
	public string FirsttodounderwriteridName
	{
		get => FormattedValues.Contains("beone_firsttodounderwriterid") ? FormattedValues["beone_firsttodounderwriterid"] : null;
	
	}
	/// <summary>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_fleetnumber")]
	public string Fleetnumber
	{
		get => TryGetAttributeValue("beone_fleetnumber", out string value) ? value : null;
		set => this["beone_fleetnumber"] = value;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_forcedduedate")]
	public DateTime? Forcedduedate
	{
		get => TryGetAttributeValue("beone_forcedduedate", out DateTime value) ? value : null;
		set => this["beone_forcedduedate"] = value;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// Targets: beone_groupoffer</br>
	/// </summary>
	[AttributeLogicalName("beone_groupofferid")]
	public EntityReference Groupofferid
	{
		get => TryGetAttributeValue("beone_groupofferid", out EntityReference value) ? value : null;
		set
		{
			if (!Request.Fields.GroupofferidTargets.Contains(value.LogicalName))
			{
				throw new InvalidPluginExecutionException($"{value.LogicalName}:{value.Id} is not a valid Groupofferid. The only valid references are beone_groupoffer");			
			}
			this["beone_groupofferid"] = value;
		}
	}

	/// <summary>
	/// Attribute of: beone_groupofferid</br>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_groupofferidname")]
	public string GroupofferidName
	{
		get => FormattedValues.Contains("beone_groupofferid") ? FormattedValues["beone_groupofferid"] : null;
	
	}
	/// <summary>
	/// Required Level: ApplicationRequired<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_importance")]
	public Request.Choices.RequestImportance? Importance
	{
		get => TryGetAttributeValue("beone_importance", out OptionSetValue opt) && opt == null ? null : (Request.Choices.RequestImportance)opt.Value;
		set => this["beone_importance"] = value == null ? null : new OptionSetValue((int)value);
	}
	/// <summary>
	/// Attribute of: beone_importance</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_importancename")]
	public string ImportanceName
	{
		get => FormattedValues.Contains("beone_importance") ? FormattedValues["beone_importancename"] : null;
	}

	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_internalexternal")]
	public Request.Choices.InternalExternal? Internalexternal
	{
		get => TryGetAttributeValue("beone_internalexternal", out OptionSetValue opt) && opt == null ? null : (Request.Choices.InternalExternal)opt.Value;
		set => this["beone_internalexternal"] = value == null ? null : new OptionSetValue((int)value);
	}
	/// <summary>
	/// Attribute of: beone_internalexternal</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_internalexternalname")]
	public string InternalexternalName
	{
		get => FormattedValues.Contains("beone_internalexternal") ? FormattedValues["beone_internalexternalname"] : null;
	}

	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_internalrework")]
	public bool? Internalrework
	{
		get => TryGetAttributeValue("beone_internalrework", out bool value) ? value : null;
		set => this["beone_internalrework"] = value;
	}
	/// <summary>
	/// Attribute of: beone_internalrework</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_internalreworkname")]
	public string InternalreworkName
	{
		get => FormattedValues.Contains("beone_internalrework") ? FormattedValues["beone_internalreworkname"] : null;
	}

	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_isautoclose")]
	public bool? Isautoclose
	{
		get => TryGetAttributeValue("beone_isautoclose", out bool value) ? value : null;
		set => this["beone_isautoclose"] = value;
	}
	/// <summary>
	/// Attribute of: beone_isautoclose</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_isautoclosename")]
	public string IsautocloseName
	{
		get => FormattedValues.Contains("beone_isautoclose") ? FormattedValues["beone_isautoclosename"] : null;
	}

	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_isurgent")]
	public bool? IsUrgent
	{
		get => TryGetAttributeValue("beone_isurgent", out bool value) ? value : null;
		set => this["beone_isurgent"] = value;
	}
	/// <summary>
	/// Attribute of: beone_isurgent</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_isurgentname")]
	public string IsurgentName
	{
		get => FormattedValues.Contains("beone_isurgent") ? FormattedValues["beone_isurgentname"] : null;
	}

	/// <summary>
	/// Max Length: 10000</br>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_json")]
	public string Json
	{
		get => TryGetAttributeValue("beone_json", out string value) ? value : null;
		set => this["beone_json"] = value;
	}
	/// <summary>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_licenseplate")]
	public string Licenseplate
	{
		get => TryGetAttributeValue("beone_licenseplate", out string value) ? value : null;
		set => this["beone_licenseplate"] = value;
	}
	/// <summary>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_mailboxid")]
	public string Mailboxid
	{
		get => TryGetAttributeValue("beone_mailboxid", out string value) ? value : null;
		set => this["beone_mailboxid"] = value;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_maindomain")]
	public Request.Choices.MainDomain? Maindomain
	{
		get => TryGetAttributeValue("beone_maindomain", out OptionSetValue opt) && opt == null ? null : (Request.Choices.MainDomain)opt.Value;
		set => this["beone_maindomain"] = value == null ? null : new OptionSetValue((int)value);
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_maindomaingroup")]
	public Request.Choices.MainDomainGroup? Maindomaingroup
	{
		get => TryGetAttributeValue("beone_maindomaingroup", out OptionSetValue opt) && opt == null ? null : (Request.Choices.MainDomainGroup)opt.Value;
		set => this["beone_maindomaingroup"] = value == null ? null : new OptionSetValue((int)value);
	}
	/// <summary>
	/// Attribute of: beone_maindomaingroup</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_maindomaingroupname")]
	public string MaindomaingroupName
	{
		get => FormattedValues.Contains("beone_maindomaingroup") ? FormattedValues["beone_maindomaingroupname"] : null;
	}

	/// <summary>
	/// Attribute of: beone_maindomain</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_maindomainname")]
	public string MaindomainName
	{
		get => FormattedValues.Contains("beone_maindomain") ? FormattedValues["beone_maindomainname"] : null;
	}

	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_messageid")]
	public int? Messageid
	{
		get => TryGetAttributeValue("beone_messageid", out int value) ? value : null;
		set => this["beone_messageid"] = value;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// Targets: systemuser</br>
	/// </summary>
	[AttributeLogicalName("beone_modifiedonbehalfby")]
	public EntityReference Modifiedonbehalfby
	{
		get => TryGetAttributeValue("beone_modifiedonbehalfby", out EntityReference value) ? value : null;
		set
		{
			if (!Request.Fields.ModifiedonbehalfbyTargets.Contains(value.LogicalName))
			{
				throw new InvalidPluginExecutionException($"{value.LogicalName}:{value.Id} is not a valid Modifiedonbehalfby. The only valid references are systemuser");			
			}
			this["beone_modifiedonbehalfby"] = value;
		}
	}

	/// <summary>
	/// Attribute of: beone_modifiedonbehalfby</br>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_modifiedonbehalfbyname")]
	public string ModifiedonbehalfbyName
	{
		get => FormattedValues.Contains("beone_modifiedonbehalfby") ? FormattedValues["beone_modifiedonbehalfby"] : null;
	
	}
	/// <summary>
	/// Attribute of: beone_modifiedonbehalfby</br>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_modifiedonbehalfbyyominame")]
	public string ModifiedonbehalfbyYomiName
	{
		get => FormattedValues.Contains("beone_modifiedonbehalfby") ? FormattedValues["beone_modifiedonbehalfby"] : null;
	
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_motive")]
	public Request.Choices.Motive? Motive
	{
		get => TryGetAttributeValue("beone_motive", out OptionSetValue opt) && opt == null ? null : (Request.Choices.Motive)opt.Value;
		set => this["beone_motive"] = value == null ? null : new OptionSetValue((int)value);
	}
	/// <summary>
	/// Attribute of: beone_motive</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_motivename")]
	public string MotiveName
	{
		get => FormattedValues.Contains("beone_motive") ? FormattedValues["beone_motivename"] : null;
	}

	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// Targets: beone_multiregardingrequest</br>
	/// </summary>
	[AttributeLogicalName("beone_multiregardingid")]
	public EntityReference Multiregardingid
	{
		get => TryGetAttributeValue("beone_multiregardingid", out EntityReference value) ? value : null;
		set
		{
			if (!Request.Fields.MultiregardingidTargets.Contains(value.LogicalName))
			{
				throw new InvalidPluginExecutionException($"{value.LogicalName}:{value.Id} is not a valid Multiregardingid. The only valid references are beone_multiregardingrequest");			
			}
			this["beone_multiregardingid"] = value;
		}
	}

	/// <summary>
	/// Attribute of: beone_multiregardingid</br>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_multiregardingidname")]
	public string MultiregardingidName
	{
		get => FormattedValues.Contains("beone_multiregardingid") ? FormattedValues["beone_multiregardingid"] : null;
	
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// Targets: beone_nace</br>
	/// </summary>
	[AttributeLogicalName("beone_naceid")]
	public EntityReference Naceid
	{
		get => TryGetAttributeValue("beone_naceid", out EntityReference value) ? value : null;
		set
		{
			if (!Request.Fields.NaceidTargets.Contains(value.LogicalName))
			{
				throw new InvalidPluginExecutionException($"{value.LogicalName}:{value.Id} is not a valid Naceid. The only valid references are beone_nace");			
			}
			this["beone_naceid"] = value;
		}
	}

	/// <summary>
	/// Attribute of: beone_naceid</br>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_naceidname")]
	public string NaceidName
	{
		get => FormattedValues.Contains("beone_naceid") ? FormattedValues["beone_naceid"] : null;
	
	}
	/// <summary>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// Auto Number Format: RQ-{SEQNUM:9}
	/// </summary>
	[AttributeLogicalName("beone_name")]
	public string Name
	{
		get => TryGetAttributeValue("beone_name", out string value) ? value : null;
		set => this["beone_name"] = value;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// Targets: beone_ncmppro</br>
	/// </summary>
	[AttributeLogicalName("beone_ncmpproid")]
	public EntityReference Ncmpproid
	{
		get => TryGetAttributeValue("beone_ncmpproid", out EntityReference value) ? value : null;
		set
		{
			if (!Request.Fields.NcmpproidTargets.Contains(value.LogicalName))
			{
				throw new InvalidPluginExecutionException($"{value.LogicalName}:{value.Id} is not a valid Ncmpproid. The only valid references are beone_ncmppro");			
			}
			this["beone_ncmpproid"] = value;
		}
	}

	/// <summary>
	/// Attribute of: beone_ncmpproid</br>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_ncmpproidname")]
	public string NcmpproidName
	{
		get => FormattedValues.Contains("beone_ncmpproid") ? FormattedValues["beone_ncmpproid"] : null;
	
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// Targets: beone_slainstance</br>
	/// </summary>
	[AttributeLogicalName("beone_newslainstanceid")]
	public EntityReference Newslainstanceid
	{
		get => TryGetAttributeValue("beone_newslainstanceid", out EntityReference value) ? value : null;
		set
		{
			if (!Request.Fields.NewslainstanceidTargets.Contains(value.LogicalName))
			{
				throw new InvalidPluginExecutionException($"{value.LogicalName}:{value.Id} is not a valid Newslainstanceid. The only valid references are beone_slainstance");			
			}
			this["beone_newslainstanceid"] = value;
		}
	}

	/// <summary>
	/// Attribute of: beone_newslainstanceid</br>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_newslainstanceidname")]
	public string NewslainstanceidName
	{
		get => FormattedValues.Contains("beone_newslainstanceid") ? FormattedValues["beone_newslainstanceid"] : null;
	
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_offerpriority")]
	public Request.Choices.FlowPriority? Offerpriority
	{
		get => TryGetAttributeValue("beone_offerpriority", out OptionSetValue opt) && opt == null ? null : (Request.Choices.FlowPriority)opt.Value;
		set => this["beone_offerpriority"] = value == null ? null : new OptionSetValue((int)value);
	}
	/// <summary>
	/// Attribute of: beone_offerpriority</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_offerpriorityname")]
	public string OfferpriorityName
	{
		get => FormattedValues.Contains("beone_offerpriority") ? FormattedValues["beone_offerpriorityname"] : null;
	}

	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_onholdduration")]
	public decimal? Onholdduration
	{
		get => TryGetAttributeValue("beone_onholdduration", out decimal value) ? value : null;
		set => this["beone_onholdduration"] = value;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_onholdreason")]
	public Request.Choices.OnHoldReason? Onholdreason
	{
		get => TryGetAttributeValue("beone_onholdreason", out OptionSetValue opt) && opt == null ? null : (Request.Choices.OnHoldReason)opt.Value;
		set => this["beone_onholdreason"] = value == null ? null : new OptionSetValue((int)value);
	}
	/// <summary>
	/// Attribute of: beone_onholdreason</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_onholdreasonname")]
	public string OnholdreasonName
	{
		get => FormattedValues.Contains("beone_onholdreason") ? FormattedValues["beone_onholdreasonname"] : null;
	}

	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_origin")]
	public Request.Choices.RequestOrigin? Origin
	{
		get => TryGetAttributeValue("beone_origin", out OptionSetValue opt) && opt == null ? null : (Request.Choices.RequestOrigin)opt.Value;
		set => this["beone_origin"] = value == null ? null : new OptionSetValue((int)value);
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// Targets: beone_awmessage</br>
	/// </summary>
	[AttributeLogicalName("beone_originalawmessageid")]
	public EntityReference Originalawmessageid
	{
		get => TryGetAttributeValue("beone_originalawmessageid", out EntityReference value) ? value : null;
		set
		{
			if (!Request.Fields.OriginalawmessageidTargets.Contains(value.LogicalName))
			{
				throw new InvalidPluginExecutionException($"{value.LogicalName}:{value.Id} is not a valid Originalawmessageid. The only valid references are beone_awmessage");			
			}
			this["beone_originalawmessageid"] = value;
		}
	}

	/// <summary>
	/// Attribute of: beone_originalawmessageid</br>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_originalawmessageidname")]
	public string OriginalawmessageidName
	{
		get => FormattedValues.Contains("beone_originalawmessageid") ? FormattedValues["beone_originalawmessageid"] : null;
	
	}
	/// <summary>
	/// Max Length: 200</br>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_originaldocument")]
	public string Originaldocument
	{
		get => TryGetAttributeValue("beone_originaldocument", out string value) ? value : null;
		set => this["beone_originaldocument"] = value;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// Targets: email</br>
	/// </summary>
	[AttributeLogicalName("beone_originalemailid")]
	public EntityReference OriginalEmailId
	{
		get => TryGetAttributeValue("beone_originalemailid", out EntityReference value) ? value : null;
		set
		{
			if (!Request.Fields.OriginalEmailIdTargets.Contains(value.LogicalName))
			{
				throw new InvalidPluginExecutionException($"{value.LogicalName}:{value.Id} is not a valid OriginalEmailId. The only valid references are email");			
			}
			this["beone_originalemailid"] = value;
		}
	}

	/// <summary>
	/// Attribute of: beone_originalemailid</br>
	/// Max Length: 800</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_originalemailidname")]
	public string OriginalEmailIdName
	{
		get => FormattedValues.Contains("beone_originalemailid") ? FormattedValues["beone_originalemailid"] : null;
	
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// Targets: letter</br>
	/// </summary>
	[AttributeLogicalName("beone_originalletterid")]
	public EntityReference Originalletterid
	{
		get => TryGetAttributeValue("beone_originalletterid", out EntityReference value) ? value : null;
		set
		{
			if (!Request.Fields.OriginalletteridTargets.Contains(value.LogicalName))
			{
				throw new InvalidPluginExecutionException($"{value.LogicalName}:{value.Id} is not a valid Originalletterid. The only valid references are letter");			
			}
			this["beone_originalletterid"] = value;
		}
	}

	/// <summary>
	/// Attribute of: beone_originalletterid</br>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_originalletteridname")]
	public string OriginalletteridName
	{
		get => FormattedValues.Contains("beone_originalletterid") ? FormattedValues["beone_originalletterid"] : null;
	
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// Targets: beone_mpb</br>
	/// </summary>
	[AttributeLogicalName("beone_originalmpbid")]
	public EntityReference Originalmpbid
	{
		get => TryGetAttributeValue("beone_originalmpbid", out EntityReference value) ? value : null;
		set
		{
			if (!Request.Fields.OriginalmpbidTargets.Contains(value.LogicalName))
			{
				throw new InvalidPluginExecutionException($"{value.LogicalName}:{value.Id} is not a valid Originalmpbid. The only valid references are beone_mpb");			
			}
			this["beone_originalmpbid"] = value;
		}
	}

	/// <summary>
	/// Attribute of: beone_originalmpbid</br>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_originalmpbidname")]
	public string OriginalmpbidName
	{
		get => FormattedValues.Contains("beone_originalmpbid") ? FormattedValues["beone_originalmpbid"] : null;
	
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// Targets: phonecall</br>
	/// </summary>
	[AttributeLogicalName("beone_originalphonecallid")]
	public EntityReference Originalphonecallid
	{
		get => TryGetAttributeValue("beone_originalphonecallid", out EntityReference value) ? value : null;
		set
		{
			if (!Request.Fields.OriginalphonecallidTargets.Contains(value.LogicalName))
			{
				throw new InvalidPluginExecutionException($"{value.LogicalName}:{value.Id} is not a valid Originalphonecallid. The only valid references are phonecall");			
			}
			this["beone_originalphonecallid"] = value;
		}
	}

	/// <summary>
	/// Attribute of: beone_originalphonecallid</br>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_originalphonecallidname")]
	public string OriginalphonecallidName
	{
		get => FormattedValues.Contains("beone_originalphonecallid") ? FormattedValues["beone_originalphonecallid"] : null;
	
	}
	/// <summary>
	/// Max Length: 200</br>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_originalsenderemailaddress")]
	public string Originalsenderemailaddress
	{
		get => TryGetAttributeValue("beone_originalsenderemailaddress", out string value) ? value : null;
		set => this["beone_originalsenderemailaddress"] = value;
	}
	/// <summary>
	/// Attribute of: beone_origin</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_originname")]
	public string OriginName
	{
		get => FormattedValues.Contains("beone_origin") ? FormattedValues["beone_originname"] : null;
	}

	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_processexpw")]
	public int? Processexpw
	{
		get => TryGetAttributeValue("beone_processexpw", out int value) ? value : null;
		set => this["beone_processexpw"] = value;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// Targets: product</br>
	/// </summary>
	[AttributeLogicalName("beone_productid")]
	public EntityReference Productid
	{
		get => TryGetAttributeValue("beone_productid", out EntityReference value) ? value : null;
		set
		{
			if (!Request.Fields.ProductidTargets.Contains(value.LogicalName))
			{
				throw new InvalidPluginExecutionException($"{value.LogicalName}:{value.Id} is not a valid Productid. The only valid references are product");			
			}
			this["beone_productid"] = value;
		}
	}

	/// <summary>
	/// Attribute of: beone_productid</br>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_productidname")]
	public string ProductidName
	{
		get => FormattedValues.Contains("beone_productid") ? FormattedValues["beone_productid"] : null;
	
	}
	/// <summary>
	/// Max Length: 700</br>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_productids")]
	public string Productids
	{
		get => TryGetAttributeValue("beone_productids", out string value) ? value : null;
		set => this["beone_productids"] = value;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_pruningdecision")]
	public Request.Choices.PruningDecision? Pruningdecision
	{
		get => TryGetAttributeValue("beone_pruningdecision", out OptionSetValue opt) && opt == null ? null : (Request.Choices.PruningDecision)opt.Value;
		set => this["beone_pruningdecision"] = value == null ? null : new OptionSetValue((int)value);
	}
	/// <summary>
	/// Attribute of: beone_pruningdecision</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_pruningdecisionname")]
	public string PruningdecisionName
	{
		get => FormattedValues.Contains("beone_pruningdecision") ? FormattedValues["beone_pruningdecisionname"] : null;
	}

	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// Targets: beone_pruning</br>
	/// </summary>
	[AttributeLogicalName("beone_pruningid")]
	public EntityReference Pruningid
	{
		get => TryGetAttributeValue("beone_pruningid", out EntityReference value) ? value : null;
		set
		{
			if (!Request.Fields.PruningidTargets.Contains(value.LogicalName))
			{
				throw new InvalidPluginExecutionException($"{value.LogicalName}:{value.Id} is not a valid Pruningid. The only valid references are beone_pruning");			
			}
			this["beone_pruningid"] = value;
		}
	}

	/// <summary>
	/// Attribute of: beone_pruningid</br>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_pruningidname")]
	public string PruningidName
	{
		get => FormattedValues.Contains("beone_pruningid") ? FormattedValues["beone_pruningid"] : null;
	
	}
	/// <summary>
	/// Required Level: SystemRequired<br/>
	/// Valid for: Create Read</br>
	/// </summary>
	
	/// <summary>
	/// Required Level: ApplicationRequired<br/>
	/// Valid for: Create Update Read</br>
	/// Targets: ag_requesttype</br>
	/// </summary>
	[AttributeLogicalName("beone_requesttypeid")]
	public EntityReference Requesttypeid
	{
		get => TryGetAttributeValue("beone_requesttypeid", out EntityReference value) ? value : null;
		set
		{
			if (!Request.Fields.RequesttypeidTargets.Contains(value.LogicalName))
			{
				throw new InvalidPluginExecutionException($"{value.LogicalName}:{value.Id} is not a valid Requesttypeid. The only valid references are ag_requesttype");			
			}
			this["beone_requesttypeid"] = value;
		}
	}

	/// <summary>
	/// Attribute of: beone_requesttypeid</br>
	/// Max Length: 500</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_requesttypeidname")]
	public string RequesttypeidName
	{
		get => FormattedValues.Contains("beone_requesttypeid") ? FormattedValues["beone_requesttypeid"] : null;
	
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_signature")]
	public Request.Choices.Boolean? Signature
	{
		get => TryGetAttributeValue("beone_signature", out OptionSetValue opt) && opt == null ? null : (Request.Choices.Boolean)opt.Value;
		set => this["beone_signature"] = value == null ? null : new OptionSetValue((int)value);
	}
	/// <summary>
	/// Attribute of: beone_signature</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_signaturename")]
	public string SignatureName
	{
		get => FormattedValues.Contains("beone_signature") ? FormattedValues["beone_signaturename"] : null;
	}

	/// <summary>
	/// Required Level: ApplicationRequired<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_sladuration")]
	public Request.Choices.SlaDuration? Sladuration
	{
		get => TryGetAttributeValue("beone_sladuration", out OptionSetValue opt) && opt == null ? null : (Request.Choices.SlaDuration)opt.Value;
		set => this["beone_sladuration"] = value == null ? null : new OptionSetValue((int)value);
	}
	/// <summary>
	/// Attribute of: beone_sladuration</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_sladurationname")]
	public string SladurationName
	{
		get => FormattedValues.Contains("beone_sladuration") ? FormattedValues["beone_sladurationname"] : null;
	}

	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// Targets: slakpiinstance</br>
	/// </summary>
	[AttributeLogicalName("beone_slainstanceid")]
	public EntityReference Slainstanceid
	{
		get => TryGetAttributeValue("beone_slainstanceid", out EntityReference value) ? value : null;
		set
		{
			if (!Request.Fields.SlainstanceidTargets.Contains(value.LogicalName))
			{
				throw new InvalidPluginExecutionException($"{value.LogicalName}:{value.Id} is not a valid Slainstanceid. The only valid references are slakpiinstance");			
			}
			this["beone_slainstanceid"] = value;
		}
	}

	/// <summary>
	/// Attribute of: beone_slainstanceid</br>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_slainstanceidname")]
	public string SlainstanceidName
	{
		get => FormattedValues.Contains("beone_slainstanceid") ? FormattedValues["beone_slainstanceid"] : null;
	
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_slakpiinstancestatus")]
	public Request.Choices.SlaKpiInstanceStatus? Slakpiinstancestatus
	{
		get => TryGetAttributeValue("beone_slakpiinstancestatus", out OptionSetValue opt) && opt == null ? null : (Request.Choices.SlaKpiInstanceStatus)opt.Value;
		set => this["beone_slakpiinstancestatus"] = value == null ? null : new OptionSetValue((int)value);
	}
	/// <summary>
	/// Attribute of: beone_slakpiinstancestatus</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_slakpiinstancestatusname")]
	public string SlakpiinstancestatusName
	{
		get => FormattedValues.Contains("beone_slakpiinstancestatus") ? FormattedValues["beone_slakpiinstancestatusname"] : null;
	}

	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_slasuccessfailuredate")]
	public DateTime? Slasuccessfailuredate
	{
		get => TryGetAttributeValue("beone_slasuccessfailuredate", out DateTime value) ? value : null;
		set => this["beone_slasuccessfailuredate"] = value;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_specificdate")]
	public DateTime? Specificdate
	{
		get => TryGetAttributeValue("beone_specificdate", out DateTime value) ? value : null;
		set => this["beone_specificdate"] = value;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_totalproducts")]
	public decimal? Totalproducts
	{
		get => TryGetAttributeValue("beone_totalproducts", out decimal value) ? value : null;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("beone_verifybroker")]
	public bool? VerifyBroker
	{
		get => TryGetAttributeValue("beone_verifybroker", out bool value) ? value : null;
		set => this["beone_verifybroker"] = value;
	}
	/// <summary>
	/// Attribute of: beone_verifybroker</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("beone_verifybrokername")]
	public string VerifybrokerName
	{
		get => FormattedValues.Contains("beone_verifybroker") ? FormattedValues["beone_verifybrokername"] : null;
	}

	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// Targets: systemuser</br>
	/// </summary>
	[AttributeLogicalName("createdby")]
	public EntityReference CreatedBy
	{
		get => TryGetAttributeValue("createdby", out EntityReference value) ? value : null;
	}

	/// <summary>
	/// Attribute of: createdby</br>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("createdbyname")]
	public string CreatedByName
	{
		get => FormattedValues.Contains("createdby") ? FormattedValues["createdby"] : null;
	
	}
	/// <summary>
	/// Attribute of: createdby</br>
	/// Max Length: 100</br>
	/// Required Level: SystemRequired<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("createdbyyominame")]
	public string CreatedByYomiName
	{
		get => FormattedValues.Contains("createdby") ? FormattedValues["createdby"] : null;
	
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("createdon")]
	public DateTime? CreatedOn
	{
		get => TryGetAttributeValue("createdon", out DateTime value) ? value : null;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// Targets: systemuser</br>
	/// </summary>
	[AttributeLogicalName("createdonbehalfby")]
	public EntityReference CreatedOnBehalfBy
	{
		get => TryGetAttributeValue("createdonbehalfby", out EntityReference value) ? value : null;
	}

	/// <summary>
	/// Attribute of: createdonbehalfby</br>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("createdonbehalfbyname")]
	public string CreatedOnBehalfByName
	{
		get => FormattedValues.Contains("createdonbehalfby") ? FormattedValues["createdonbehalfby"] : null;
	
	}
	/// <summary>
	/// Attribute of: createdonbehalfby</br>
	/// Max Length: 100</br>
	/// Required Level: SystemRequired<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("createdonbehalfbyyominame")]
	public string CreatedOnBehalfByYomiName
	{
		get => FormattedValues.Contains("createdonbehalfby") ? FormattedValues["createdonbehalfby"] : null;
	
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Read</br>
	/// </summary>
	[AttributeLogicalName("importsequencenumber")]
	public int? ImportSequenceNumber
	{
		get => TryGetAttributeValue("importsequencenumber", out int value) ? value : null;
		set => this["importsequencenumber"] = value;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("lastonholdtime")]
	public DateTime? LastOnHoldTime
	{
		get => TryGetAttributeValue("lastonholdtime", out DateTime value) ? value : null;
		set => this["lastonholdtime"] = value;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// Targets: systemuser</br>
	/// </summary>
	[AttributeLogicalName("modifiedby")]
	public EntityReference ModifiedBy
	{
		get => TryGetAttributeValue("modifiedby", out EntityReference value) ? value : null;
	}

	/// <summary>
	/// Attribute of: modifiedby</br>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("modifiedbyname")]
	public string ModifiedByName
	{
		get => FormattedValues.Contains("modifiedby") ? FormattedValues["modifiedby"] : null;
	
	}
	/// <summary>
	/// Attribute of: modifiedby</br>
	/// Max Length: 100</br>
	/// Required Level: SystemRequired<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("modifiedbyyominame")]
	public string ModifiedByYomiName
	{
		get => FormattedValues.Contains("modifiedby") ? FormattedValues["modifiedby"] : null;
	
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("modifiedon")]
	public DateTime? ModifiedOn
	{
		get => TryGetAttributeValue("modifiedon", out DateTime value) ? value : null;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// Targets: systemuser</br>
	/// </summary>
	[AttributeLogicalName("modifiedonbehalfby")]
	public EntityReference ModifiedOnBehalfBy
	{
		get => TryGetAttributeValue("modifiedonbehalfby", out EntityReference value) ? value : null;
	}

	/// <summary>
	/// Attribute of: modifiedonbehalfby</br>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("modifiedonbehalfbyname")]
	public string ModifiedOnBehalfByName
	{
		get => FormattedValues.Contains("modifiedonbehalfby") ? FormattedValues["modifiedonbehalfby"] : null;
	
	}
	/// <summary>
	/// Attribute of: modifiedonbehalfby</br>
	/// Max Length: 100</br>
	/// Required Level: SystemRequired<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("modifiedonbehalfbyyominame")]
	public string ModifiedOnBehalfByYomiName
	{
		get => FormattedValues.Contains("modifiedonbehalfby") ? FormattedValues["modifiedonbehalfby"] : null;
	
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("onholdtime")]
	public int? OnHoldTime
	{
		get => TryGetAttributeValue("onholdtime", out int value) ? value : null;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Read</br>
	/// </summary>
	[AttributeLogicalName("overriddencreatedon")]
	public DateTime? OverriddenCreatedOn
	{
		get => TryGetAttributeValue("overriddencreatedon", out DateTime value) ? value : null;
		set => this["overriddencreatedon"] = value;
	}
	/// <summary>
	/// Required Level: SystemRequired<br/>
	/// Valid for: Create Update Read</br>
	/// Targets: systemuser,team</br>
	/// </summary>
	[AttributeLogicalName("ownerid")]
	public EntityReference OwnerId
	{
		get => TryGetAttributeValue("ownerid", out EntityReference value) ? value : null;
		set => this["ownerid"] = value;
	}

	/// <summary>
	/// Attribute of: ownerid</br>
	/// Max Length: 100</br>
	/// Required Level: SystemRequired<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("owneridname")]
	public string OwnerIdName
	{
		get => TryGetAttributeValue("owneridname", out string value) ? value : null;
	}
	/// <summary>
	/// Attribute of: ownerid</br>
	/// Max Length: 100</br>
	/// Required Level: SystemRequired<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("owneridyominame")]
	public string OwnerIdYomiName
	{
		get => TryGetAttributeValue("owneridyominame", out string value) ? value : null;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// Targets: businessunit</br>
	/// </summary>
	[AttributeLogicalName("owningbusinessunit")]
	public EntityReference OwningBusinessUnit
	{
		get => TryGetAttributeValue("owningbusinessunit", out EntityReference value) ? value : null;
	}

	/// <summary>
	/// Attribute of: owningbusinessunit</br>
	/// Max Length: 160</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("owningbusinessunitname")]
	public string OwningBusinessUnitName
	{
		get => FormattedValues.Contains("owningbusinessunit") ? FormattedValues["owningbusinessunit"] : null;
	
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// Targets: team</br>
	/// </summary>
	[AttributeLogicalName("owningteam")]
	public EntityReference OwningTeam
	{
		get => TryGetAttributeValue("owningteam", out EntityReference value) ? value : null;
	}

	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// Targets: systemuser</br>
	/// </summary>
	[AttributeLogicalName("owninguser")]
	public EntityReference OwningUser
	{
		get => TryGetAttributeValue("owninguser", out EntityReference value) ? value : null;
	}

	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// Targets: sla</br>
	/// </summary>
	[AttributeLogicalName("slaid")]
	public EntityReference SLAId
	{
		get => TryGetAttributeValue("slaid", out EntityReference value) ? value : null;
		set
		{
			if (!Request.Fields.SLAIdTargets.Contains(value.LogicalName))
			{
				throw new InvalidPluginExecutionException($"{value.LogicalName}:{value.Id} is not a valid SLAId. The only valid references are sla");			
			}
			this["slaid"] = value;
		}
	}

	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// Targets: sla</br>
	/// </summary>
	[AttributeLogicalName("slainvokedid")]
	public EntityReference SLAInvokedId
	{
		get => TryGetAttributeValue("slainvokedid", out EntityReference value) ? value : null;
	}

	/// <summary>
	/// Attribute of: slainvokedid</br>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("slainvokedidname")]
	public string SLAInvokedIdName
	{
		get => FormattedValues.Contains("slainvokedid") ? FormattedValues["slainvokedid"] : null;
	
	}
	/// <summary>
	/// Attribute of: slaid</br>
	/// Max Length: 100</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("slaname")]
	public string SLAName
	{
		get => FormattedValues.Contains("slaid") ? FormattedValues["slaid"] : null;
	
	}
	/// <summary>
	/// Required Level: SystemRequired<br/>
	/// Valid for: Update Read</br>
	/// </summary>
	[AttributeLogicalName("statecode")]
	public Request.Choices.Status? Statecode
	{
		get => TryGetAttributeValue("statecode", out OptionSetValue opt) && opt != null ? (Request.Choices.Status)Enum.ToObject(typeof(Request.Choices.Status), opt.Value) : null;
		set => this["statecode"] = value == null ? null : new OptionSetValue(((IConvertible)value).ToInt32((IFormatProvider)CultureInfo.InvariantCulture));
	}
	/// <summary>
	/// Attribute of: statecode</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("statecodename")]
	public string StatecodeName
	{
		get => FormattedValues.Contains("statecode") ? FormattedValues["statecodename"] : null;
	}

	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("statuscode")]
	public Request.Choices.StatusReason? Statuscode
	{
		get => TryGetAttributeValue("statuscode", out OptionSetValue opt) && opt != null ? (Request.Choices.StatusReason)Enum.ToObject(typeof(Request.Choices.StatusReason), opt.Value) : null;
		set => this["statuscode"] = value == null ? null : new OptionSetValue(((IConvertible)value).ToInt32((IFormatProvider)CultureInfo.InvariantCulture));
	}
	/// <summary>
	/// Attribute of: statuscode</br>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("statuscodename")]
	public string StatuscodeName
	{
		get => FormattedValues.Contains("statuscode") ? FormattedValues["statuscodename"] : null;
	}

	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("timezoneruleversionnumber")]
	public int? TimeZoneRuleVersionNumber
	{
		get => TryGetAttributeValue("timezoneruleversionnumber", out int value) ? value : null;
		set => this["timezoneruleversionnumber"] = value;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("utcconversiontimezonecode")]
	public int? UTCConversionTimeZoneCode
	{
		get => TryGetAttributeValue("utcconversiontimezonecode", out int value) ? value : null;
		set => this["utcconversiontimezonecode"] = value;
	}
	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Read</br>
	/// </summary>
	[AttributeLogicalName("versionnumber")]
	public long? VersionNumber
	{
		get => TryGetAttributeValue("versionnumber", out long value) ? value : null;
	}
	public Request() : base(EntityLogicalName) { }
    public Request(string keyName, object keyValue) : base(EntityLogicalName, keyName, keyValue) { }
    public Request(KeyAttributeCollection keyAttributes) : base(EntityLogicalName, keyAttributes) { }
}

[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
public partial class PostUpdate1 : PluginDefinition<PostUpdate1Manager>, IPlugin
{
	[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
	[EntityLogicalName("crb4d_test")]
	public class TargetTest : Entity
	{
		public const string EntityLogicalName = "crb4d_test";
		public const string EntityLogicalCollectionName = "crb4d_tests";
		public const string EntitySetName = "crb4d_tests";
		public const int EntityTypeCode = 11070;
		public const string PrimaryNameAttribute = "";
		public const string PrimaryIdAttribute = "crb4d_testid";
	
		public partial class Fields
		{
			public const string AutoNumber = "crb4d_autonumber";
			public const string ChoiceGlobal = "crb4d_choiceglobal";
			public const string ChoiceglobalName = "crb4d_choiceglobalname";
			public const string ChoiceLocalMulti = "crb4d_choicelocalmulti";
			public const string ChoicelocalmultiName = "crb4d_choicelocalmultiname";
			public const string Currency = "crb4d_currency";
			public const string Currency_Base = "crb4d_currency_base";
			public const string Customer = "crb4d_customer";
			public const string CustomerName = "crb4d_customername";
			public const string CustomerYomiName = "crb4d_customeryominame";
			public const string DateandTime = "crb4d_dateandtime";
			public const string DateOnly = "crb4d_dateonly";
			public const string Decimal = "crb4d_decimal";
			public const string Duration = "crb4d_duration";
			public const string File = "crb4d_file";
			public const string File_Name = "crb4d_file_name";
			public const string Float = "crb4d_float";
			public const string Formula = "crb4d_formula";
			public const string Image = "crb4d_image";
			public const string Image_Timestamp = "crb4d_image_timestamp";
			public const string Image_URL = "crb4d_image_url";
			public const string ImageId = "crb4d_imageid";
			public const string LanguageCode = "crb4d_languagecode";
			public const string Lookup = "crb4d_lookup";
			public const string LookupName = "crb4d_lookupname";
			public const string MultipleLineofTextPlain = "crb4d_multiplelineoftextplain";
			public const string MultipleLineofTextRich = "crb4d_multiplelineoftextrich";
			public const string Name = "crb4d_name";
			public const string SingleLineOfTest = "crb4d_singlelineoftest";
			public const string SingleLineOfTextArea = "crb4d_singlelineoftextarea";
			public const string SingleLineofTextEmail = "crb4d_singlelineoftextemail";
			public const string SingleLineofTextPhoneNumber = "crb4d_singlelineoftextphonenumber";
			public const string SingleLineofTextRich = "crb4d_singlelineoftextrich";
			public const string SingleLineofTextTickerSymbol = "crb4d_singlelineoftexttickersymbol";
			public const string SingleLineofTextURL = "crb4d_singlelineoftexturl";
			public const string TestId = "crb4d_testid";
			public const string TimeZone = "crb4d_timezone";
			public const string WholeNumber = "crb4d_wholenumber";
			public const string CreatedBy = "createdby";
			public const string CreatedByName = "createdbyname";
			public const string CreatedByYomiName = "createdbyyominame";
			public const string CreatedOn = "createdon";
			public const string CreatedOnBehalfBy = "createdonbehalfby";
			public const string CreatedOnBehalfByName = "createdonbehalfbyname";
			public const string CreatedOnBehalfByYomiName = "createdonbehalfbyyominame";
			public const string ExchangeRate = "exchangerate";
			public const string ImportSequenceNumber = "importsequencenumber";
			public const string ModifiedBy = "modifiedby";
			public const string ModifiedByName = "modifiedbyname";
			public const string ModifiedByYomiName = "modifiedbyyominame";
			public const string ModifiedOn = "modifiedon";
			public const string ModifiedOnBehalfBy = "modifiedonbehalfby";
			public const string ModifiedOnBehalfByName = "modifiedonbehalfbyname";
			public const string ModifiedOnBehalfByYomiName = "modifiedonbehalfbyyominame";
			public const string OverriddenCreatedOn = "overriddencreatedon";
			public const string OwnerId = "ownerid";
			public const string OwnerIdName = "owneridname";
			public const string OwnerIdYomiName = "owneridyominame";
			public const string OwningBusinessUnit = "owningbusinessunit";
			public const string OwningBusinessUnitName = "owningbusinessunitname";
			public const string OwningTeam = "owningteam";
			public const string OwningUser = "owninguser";
			public const string Statecode = "statecode";
			public const string StatecodeName = "statecodename";
			public const string Statuscode = "statuscode";
			public const string StatuscodeName = "statuscodename";
			public const string TimeZoneRuleVersionNumber = "timezoneruleversionnumber";
			public const string TransactionCurrencyId = "transactioncurrencyid";
			public const string TransactionCurrencyIdName = "transactioncurrencyidname";
			public const string UTCConversionTimeZoneCode = "utcconversiontimezonecode";
			public const string VersionNumber = "versionnumber";
		}
	
		/// <summary>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Auto Number Format: {SEQNUM:4}
		/// </summary>
		//AutoNumber
		
		[AttributeLogicalName("crb4d_autonumber")]
		public string AutoNumber
		{
			get => TryGetAttributeValue("crb4d_autonumber", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//ChoiceGlobal
		
		[AttributeLogicalName("crb4d_choiceglobal")]
		public Test.Choices.AdagioStatus? ChoiceGlobal
		{
			get => TryGetAttributeValue("crb4d_choiceglobal", out OptionSetValue opt) && opt == null ? null : (Test.Choices.AdagioStatus)opt.Value;
		}
		/// <summary>
		/// Attribute of: crb4d_choiceglobal</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//ChoiceglobalName
		
		[AttributeLogicalName("crb4d_choiceglobalname")]
		public string ChoiceglobalName
		{
			get => FormattedValues.Contains("crb4d_choiceglobal") ? FormattedValues["crb4d_choiceglobalname"] : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//ChoiceLocalMulti
		
		[AttributeLogicalName("crb4d_choicelocalmulti")]
		public IEnumerable<Test.Choices.ChoiceLocalMulti> ChoiceLocalMulti
		{
			get => TryGetAttributeValue("crb4d_choicelocalmulti", out OptionSetValueCollection opts) && opts != null ? opts.Select(opt => (Test.Choices.ChoiceLocalMulti)opt.Value) : [];
		}
	
		/// <summary>
		/// Attribute of: crb4d_choicelocalmulti</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//ChoicelocalmultiName
		
		[AttributeLogicalName("crb4d_choicelocalmultiname")]
		public string ChoicelocalmultiName
		{
			get => FormattedValues.Contains("crb4d_choicelocalmulti") ? FormattedValues["crb4d_choicelocalmultiname"] : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Currency
		
		[AttributeLogicalName("crb4d_currency")]
		public decimal? Currency
		{
			get => TryGetAttributeValue("crb4d_currency", out Money money) ? money.Value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//Currency_Base
		
		[AttributeLogicalName("crb4d_currency_base")]
		public decimal? Currency_Base
		{
			get => TryGetAttributeValue("crb4d_currency_base", out Money money) ? money.Value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Targets: account,contact</br>
		/// </summary>
		//Customer
		
		[AttributeLogicalName("crb4d_customer")]
		public EntityReference Customer
		{
			get => TryGetAttributeValue("crb4d_customer", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: crb4d_customer</br>
		/// Max Length: 4000</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//CustomerName
		
		[AttributeLogicalName("crb4d_customername")]
		public string CustomerName
		{
			get => FormattedValues.Contains("crb4d_customer") ? FormattedValues["crb4d_customer"] : null;
		
		}
		/// <summary>
		/// Attribute of: crb4d_customer</br>
		/// Max Length: 4000</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//CustomerYomiName
		
		[AttributeLogicalName("crb4d_customeryominame")]
		public string CustomerYomiName
		{
			get => FormattedValues.Contains("crb4d_customer") ? FormattedValues["crb4d_customer"] : null;
		
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//DateandTime
		
		[AttributeLogicalName("crb4d_dateandtime")]
		public DateTime? DateandTime
		{
			get => TryGetAttributeValue("crb4d_dateandtime", out DateTime value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//DateOnly
		
		[AttributeLogicalName("crb4d_dateonly")]
		public DateTime? DateOnly
		{
			get => TryGetAttributeValue("crb4d_dateonly", out DateTime value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Decimal
		
		[AttributeLogicalName("crb4d_decimal")]
		public decimal? Decimal
		{
			get => TryGetAttributeValue("crb4d_decimal", out decimal value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Duration
		
		[AttributeLogicalName("crb4d_duration")]
		public int? Duration
		{
			get => TryGetAttributeValue("crb4d_duration", out int value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//File
		
		[AttributeLogicalName("crb4d_file")]
		public Guid? File
		{
			get => TryGetAttributeValue("crb4d_file", out Guid value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: crb4d_file</br>
		/// Max Length: 200</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//File_Name
		
		[AttributeLogicalName("crb4d_file_name")]
		public string File_Name
		{
			get => TryGetAttributeValue("crb4d_file_name", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Float
		
		[AttributeLogicalName("crb4d_float")]
		public double? Float
		{
			get => TryGetAttributeValue("crb4d_float", out double value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//Formula
		
		[AttributeLogicalName("crb4d_formula")]
		public DateTime? Formula
		{
			get => TryGetAttributeValue("crb4d_formula", out DateTime value) ? value : null;
		}
		/// <summary>
		/// Attribute of: crb4d_imageid</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Image
		
		[AttributeLogicalName("crb4d_image")]
		public byte[] Image
		{
			get => TryGetAttributeValue("crb4d_image", out byte[] value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: crb4d_imageid</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//Image_Timestamp
		
		[AttributeLogicalName("crb4d_image_timestamp")]
		public long? Image_Timestamp
		{
			get => TryGetAttributeValue("crb4d_image_timestamp", out long value) ? value : null;
		}
		/// <summary>
		/// Attribute of: crb4d_imageid</br>
		/// Max Length: 200</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//Image_URL
		
		[AttributeLogicalName("crb4d_image_url")]
		public string Image_URL
		{
			get => TryGetAttributeValue("crb4d_image_url", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//ImageId
		
		[AttributeLogicalName("crb4d_imageid")]
		public Guid? ImageId
		{
			get => TryGetAttributeValue("crb4d_imageid", out Guid value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//LanguageCode
		
		[AttributeLogicalName("crb4d_languagecode")]
		public int? LanguageCode
		{
			get => TryGetAttributeValue("crb4d_languagecode", out int value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Targets: ag_acme</br>
		/// </summary>
		//Lookup
		
		[AttributeLogicalName("crb4d_lookup")]
		public EntityReference Lookup
		{
			get => TryGetAttributeValue("crb4d_lookup", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: crb4d_lookup</br>
		/// Max Length: 400</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//LookupName
		
		[AttributeLogicalName("crb4d_lookupname")]
		public string LookupName
		{
			get => FormattedValues.Contains("crb4d_lookup") ? FormattedValues["crb4d_lookup"] : null;
		
		}
		/// <summary>
		/// Max Length: 2000</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//MultipleLineofTextPlain
		
		[AttributeLogicalName("crb4d_multiplelineoftextplain")]
		public string MultipleLineofTextPlain
		{
			get => TryGetAttributeValue("crb4d_multiplelineoftextplain", out string value) ? value : null;
		}
		/// <summary>
		/// Max Length: 2000</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//MultipleLineofTextRich
		
		[AttributeLogicalName("crb4d_multiplelineoftextrich")]
		public string MultipleLineofTextRich
		{
			get => TryGetAttributeValue("crb4d_multiplelineoftextrich", out string value) ? value : null;
		}
		/// <summary>
		/// Max Length: 100</br>
		/// Required Level: ApplicationRequired</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Name
		
		[AttributeLogicalName("crb4d_name")]
		public string Name
		{
			get => TryGetAttributeValue("crb4d_name", out string value) ? value : null;
		}
		/// <summary>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Auto Number Format: 
		/// </summary>
		//SingleLineOfTest
		
		[AttributeLogicalName("crb4d_singlelineoftest")]
		public string SingleLineOfTest
		{
			get => TryGetAttributeValue("crb4d_singlelineoftest", out string value) ? value : null;
		}
		/// <summary>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Auto Number Format: 
		/// </summary>
		//SingleLineOfTextArea
		
		[AttributeLogicalName("crb4d_singlelineoftextarea")]
		public string SingleLineOfTextArea
		{
			get => TryGetAttributeValue("crb4d_singlelineoftextarea", out string value) ? value : null;
		}
		/// <summary>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Auto Number Format: 
		/// </summary>
		//SingleLineofTextEmail
		
		[AttributeLogicalName("crb4d_singlelineoftextemail")]
		public string SingleLineofTextEmail
		{
			get => TryGetAttributeValue("crb4d_singlelineoftextemail", out string value) ? value : null;
		}
		/// <summary>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Auto Number Format: 
		/// </summary>
		//SingleLineofTextPhoneNumber
		
		[AttributeLogicalName("crb4d_singlelineoftextphonenumber")]
		public string SingleLineofTextPhoneNumber
		{
			get => TryGetAttributeValue("crb4d_singlelineoftextphonenumber", out string value) ? value : null;
		}
		/// <summary>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Auto Number Format: 
		/// </summary>
		//SingleLineofTextRich
		
		[AttributeLogicalName("crb4d_singlelineoftextrich")]
		public string SingleLineofTextRich
		{
			get => TryGetAttributeValue("crb4d_singlelineoftextrich", out string value) ? value : null;
		}
		/// <summary>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Auto Number Format: 
		/// </summary>
		//SingleLineofTextTickerSymbol
		
		[AttributeLogicalName("crb4d_singlelineoftexttickersymbol")]
		public string SingleLineofTextTickerSymbol
		{
			get => TryGetAttributeValue("crb4d_singlelineoftexttickersymbol", out string value) ? value : null;
		}
		/// <summary>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Auto Number Format: 
		/// </summary>
		//SingleLineofTextURL
		
		[AttributeLogicalName("crb4d_singlelineoftexturl")]
		public string SingleLineofTextURL
		{
			get => TryGetAttributeValue("crb4d_singlelineoftexturl", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: SystemRequired</br>
		/// Valid for: Create Read</br>
		/// </summary>
		//TestId
		
		
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//TimeZone
		
		[AttributeLogicalName("crb4d_timezone")]
		public int? TimeZone
		{
			get => TryGetAttributeValue("crb4d_timezone", out int value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//WholeNumber
		
		[AttributeLogicalName("crb4d_wholenumber")]
		public int? WholeNumber
		{
			get => TryGetAttributeValue("crb4d_wholenumber", out int value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// Targets: systemuser</br>
		/// </summary>
		//CreatedBy
		
		[AttributeLogicalName("createdby")]
		public EntityReference CreatedBy
		{
			get => TryGetAttributeValue("createdby", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: createdby</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//CreatedByName
		
		[AttributeLogicalName("createdbyname")]
		public string CreatedByName
		{
			get => FormattedValues.Contains("createdby") ? FormattedValues["createdby"] : null;
		
		}
		/// <summary>
		/// Attribute of: createdby</br>
		/// Max Length: 100</br>
		/// Required Level: SystemRequired</br>
		/// Valid for: Read</br>
		/// </summary>
		//CreatedByYomiName
		
		[AttributeLogicalName("createdbyyominame")]
		public string CreatedByYomiName
		{
			get => FormattedValues.Contains("createdby") ? FormattedValues["createdby"] : null;
		
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//CreatedOn
		
		[AttributeLogicalName("createdon")]
		public DateTime? CreatedOn
		{
			get => TryGetAttributeValue("createdon", out DateTime value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// Targets: systemuser</br>
		/// </summary>
		//CreatedOnBehalfBy
		
		[AttributeLogicalName("createdonbehalfby")]
		public EntityReference CreatedOnBehalfBy
		{
			get => TryGetAttributeValue("createdonbehalfby", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: createdonbehalfby</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//CreatedOnBehalfByName
		
		[AttributeLogicalName("createdonbehalfbyname")]
		public string CreatedOnBehalfByName
		{
			get => FormattedValues.Contains("createdonbehalfby") ? FormattedValues["createdonbehalfby"] : null;
		
		}
		/// <summary>
		/// Attribute of: createdonbehalfby</br>
		/// Max Length: 100</br>
		/// Required Level: SystemRequired</br>
		/// Valid for: Read</br>
		/// </summary>
		//CreatedOnBehalfByYomiName
		
		[AttributeLogicalName("createdonbehalfbyyominame")]
		public string CreatedOnBehalfByYomiName
		{
			get => FormattedValues.Contains("createdonbehalfby") ? FormattedValues["createdonbehalfby"] : null;
		
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//ExchangeRate
		
		[AttributeLogicalName("exchangerate")]
		public decimal? ExchangeRate
		{
			get => TryGetAttributeValue("exchangerate", out decimal value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Read</br>
		/// </summary>
		//ImportSequenceNumber
		
		[AttributeLogicalName("importsequencenumber")]
		public int? ImportSequenceNumber
		{
			get => TryGetAttributeValue("importsequencenumber", out int value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// Targets: systemuser</br>
		/// </summary>
		//ModifiedBy
		
		[AttributeLogicalName("modifiedby")]
		public EntityReference ModifiedBy
		{
			get => TryGetAttributeValue("modifiedby", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: modifiedby</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//ModifiedByName
		
		[AttributeLogicalName("modifiedbyname")]
		public string ModifiedByName
		{
			get => FormattedValues.Contains("modifiedby") ? FormattedValues["modifiedby"] : null;
		
		}
		/// <summary>
		/// Attribute of: modifiedby</br>
		/// Max Length: 100</br>
		/// Required Level: SystemRequired</br>
		/// Valid for: Read</br>
		/// </summary>
		//ModifiedByYomiName
		
		[AttributeLogicalName("modifiedbyyominame")]
		public string ModifiedByYomiName
		{
			get => FormattedValues.Contains("modifiedby") ? FormattedValues["modifiedby"] : null;
		
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//ModifiedOn
		
		[AttributeLogicalName("modifiedon")]
		public DateTime? ModifiedOn
		{
			get => TryGetAttributeValue("modifiedon", out DateTime value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// Targets: systemuser</br>
		/// </summary>
		//ModifiedOnBehalfBy
		
		[AttributeLogicalName("modifiedonbehalfby")]
		public EntityReference ModifiedOnBehalfBy
		{
			get => TryGetAttributeValue("modifiedonbehalfby", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: modifiedonbehalfby</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//ModifiedOnBehalfByName
		
		[AttributeLogicalName("modifiedonbehalfbyname")]
		public string ModifiedOnBehalfByName
		{
			get => FormattedValues.Contains("modifiedonbehalfby") ? FormattedValues["modifiedonbehalfby"] : null;
		
		}
		/// <summary>
		/// Attribute of: modifiedonbehalfby</br>
		/// Max Length: 100</br>
		/// Required Level: SystemRequired</br>
		/// Valid for: Read</br>
		/// </summary>
		//ModifiedOnBehalfByYomiName
		
		[AttributeLogicalName("modifiedonbehalfbyyominame")]
		public string ModifiedOnBehalfByYomiName
		{
			get => FormattedValues.Contains("modifiedonbehalfby") ? FormattedValues["modifiedonbehalfby"] : null;
		
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Read</br>
		/// </summary>
		//OverriddenCreatedOn
		
		[AttributeLogicalName("overriddencreatedon")]
		public DateTime? OverriddenCreatedOn
		{
			get => TryGetAttributeValue("overriddencreatedon", out DateTime value) ? value : null;
		}
		/// <summary>
		/// Required Level: SystemRequired</br>
		/// Valid for: Create Update Read</br>
		/// Targets: systemuser,team</br>
		/// </summary>
		//OwnerId
		
		[AttributeLogicalName("ownerid")]
		public EntityReference OwnerId
		{
			get => TryGetAttributeValue("ownerid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: ownerid</br>
		/// Max Length: 100</br>
		/// Required Level: SystemRequired</br>
		/// Valid for: Read</br>
		/// </summary>
		//OwnerIdName
		
		[AttributeLogicalName("owneridname")]
		public string OwnerIdName
		{
			get => TryGetAttributeValue("owneridname", out string value) ? value : null;
		}
		/// <summary>
		/// Attribute of: ownerid</br>
		/// Max Length: 100</br>
		/// Required Level: SystemRequired</br>
		/// Valid for: Read</br>
		/// </summary>
		//OwnerIdYomiName
		
		[AttributeLogicalName("owneridyominame")]
		public string OwnerIdYomiName
		{
			get => TryGetAttributeValue("owneridyominame", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// Targets: businessunit</br>
		/// </summary>
		//OwningBusinessUnit
		
		[AttributeLogicalName("owningbusinessunit")]
		public EntityReference OwningBusinessUnit
		{
			get => TryGetAttributeValue("owningbusinessunit", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: owningbusinessunit</br>
		/// Max Length: 100</br>
		/// Required Level: SystemRequired</br>
		/// Valid for: Read</br>
		/// </summary>
		//OwningBusinessUnitName
		
		[AttributeLogicalName("owningbusinessunitname")]
		public string OwningBusinessUnitName
		{
			get => FormattedValues.Contains("owningbusinessunit") ? FormattedValues["owningbusinessunit"] : null;
		
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// Targets: team</br>
		/// </summary>
		//OwningTeam
		
		[AttributeLogicalName("owningteam")]
		public EntityReference OwningTeam
		{
			get => TryGetAttributeValue("owningteam", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// Targets: systemuser</br>
		/// </summary>
		//OwningUser
		
		[AttributeLogicalName("owninguser")]
		public EntityReference OwningUser
		{
			get => TryGetAttributeValue("owninguser", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Required Level: SystemRequired</br>
		/// Valid for: Update Read</br>
		/// </summary>
		//Statecode
		
		[AttributeLogicalName("statecode")]
		public Test.Choices.Status? Statecode
		{
			get => TryGetAttributeValue("statecode", out OptionSetValue opt) && opt != null ? (Test.Choices.Status)Enum.ToObject(typeof(Test.Choices.Status), opt.Value) : null;
		}
		/// <summary>
		/// Attribute of: statecode</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//StatecodeName
		
		[AttributeLogicalName("statecodename")]
		public string StatecodeName
		{
			get => FormattedValues.Contains("statecode") ? FormattedValues["statecodename"] : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Statuscode
		
		[AttributeLogicalName("statuscode")]
		public Test.Choices.StatusReason? Statuscode
		{
			get => TryGetAttributeValue("statuscode", out OptionSetValue opt) && opt != null ? (Test.Choices.StatusReason)Enum.ToObject(typeof(Test.Choices.StatusReason), opt.Value) : null;
		}
		/// <summary>
		/// Attribute of: statuscode</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//StatuscodeName
		
		[AttributeLogicalName("statuscodename")]
		public string StatuscodeName
		{
			get => FormattedValues.Contains("statuscode") ? FormattedValues["statuscodename"] : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//TimeZoneRuleVersionNumber
		
		[AttributeLogicalName("timezoneruleversionnumber")]
		public int? TimeZoneRuleVersionNumber
		{
			get => TryGetAttributeValue("timezoneruleversionnumber", out int value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Targets: transactioncurrency</br>
		/// </summary>
		//TransactionCurrencyId
		
		[AttributeLogicalName("transactioncurrencyid")]
		public EntityReference TransactionCurrencyId
		{
			get => TryGetAttributeValue("transactioncurrencyid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: transactioncurrencyid</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//TransactionCurrencyIdName
		
		[AttributeLogicalName("transactioncurrencyidname")]
		public string TransactionCurrencyIdName
		{
			get => FormattedValues.Contains("transactioncurrencyid") ? FormattedValues["transactioncurrencyid"] : null;
		
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//UTCConversionTimeZoneCode
		
		[AttributeLogicalName("utcconversiontimezonecode")]
		public int? UTCConversionTimeZoneCode
		{
			get => TryGetAttributeValue("utcconversiontimezonecode", out int value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//VersionNumber
		
		[AttributeLogicalName("versionnumber")]
		public long? VersionNumber
		{
			get => TryGetAttributeValue("versionnumber", out long value) ? value : null;
		}
	}
	

}
public partial class PostUpdate1Manager : UpdatePluginManager
{
	[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")] public PostUpdate1Manager() : base("crb4d_test", EStage.PostOperation) { }

	new PostUpdate1.TargetTest Target => Target.ToEntity<PostUpdate1.TargetTest>();
}
[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
public partial class PostCreate : PluginDefinition<PostCreateManager>, IPlugin
{
	[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
	[EntityLogicalName("beone_request")]
	public class TargetRequest : Entity
	{
		public const string EntityLogicalName = "beone_request";
		public const string EntityLogicalCollectionName = "beone_requests";
		public const string EntitySetName = "beone_requests";
		public const int EntityTypeCode = 10094;
		public const string PrimaryNameAttribute = "";
		public const string PrimaryIdAttribute = "beone_requestid";
	
		public partial class Fields
		{
			public const string Activity = "beone_activity";
			public const string Adagionumber = "beone_adagionumber";
			public const string Agolstatus = "beone_agolstatus";
			public const string Awproducts = "beone_awproducts";
		}
	
		/// <summary>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Auto Number Format: 
		/// </summary>
		//Activity
		
		[AttributeLogicalName("beone_activity")]
		public string Activity
		{
			get => TryGetAttributeValue("beone_activity", out string value) ? value : null;
		}
		/// <summary>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Adagionumber
		
		[AttributeLogicalName("beone_adagionumber")]
		public string Adagionumber
		{
			get => TryGetAttributeValue("beone_adagionumber", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Agolstatus
		
		[AttributeLogicalName("beone_agolstatus")]
		public Request.Choices.AgolStatus? Agolstatus
		{
			get => TryGetAttributeValue("beone_agolstatus", out OptionSetValue opt) && opt == null ? null : (Request.Choices.AgolStatus)opt.Value;
		}
		/// <summary>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Awproducts
		
		[AttributeLogicalName("beone_awproducts")]
		public string Awproducts
		{
			get => TryGetAttributeValue("beone_awproducts", out string value) ? value : null;
		}
	}
	

}
public partial class PostCreateManager : CreatePluginManager
{
	[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")] public PostCreateManager() : base("beone_request", EStage.PostOperation) { }

	new PostCreate.TargetRequest Target => Target.ToEntity<PostCreate.TargetRequest>();
}
[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
public partial class PostUpdate : PluginDefinition<PostUpdateManager>, IPlugin
{
	[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
	[EntityLogicalName("beone_request")]
	public class TargetRequest : Entity
	{
		public const string EntityLogicalName = "beone_request";
		public const string EntityLogicalCollectionName = "beone_requests";
		public const string EntitySetName = "beone_requests";
		public const int EntityTypeCode = 10094;
		public const string PrimaryNameAttribute = "";
		public const string PrimaryIdAttribute = "beone_requestid";
	
		public partial class Fields
		{
			public const string Actgmotivelabel = "beone_actgmotivelabel";
			public const string Actiontypeid = "beone_actiontypeid";
			public const string ActiontypeidName = "beone_actiontypeidname";
			public const string Activity = "beone_activity";
			public const string Adagionumber = "beone_adagionumber";
			public const string Agolstatus = "beone_agolstatus";
			public const string AgolstatusName = "beone_agolstatusname";
			public const string Annulationdate = "beone_annulationdate";
			public const string Awmessagetype = "beone_awmessagetype";
			public const string AwmessagetypeName = "beone_awmessagetypename";
			public const string Awproducts = "beone_awproducts";
			public const string BCENumber = "beone_bcenumber";
			public const string Brokerfeedback = "beone_brokerfeedback";
			public const string BrokerfeedbackName = "beone_brokerfeedbackname";
			public const string Claimhistoryid = "beone_claimhistoryid";
			public const string ClaimhistoryidName = "beone_claimhistoryidname";
			public const string Closesla = "beone_closesla";
			public const string CloseslaName = "beone_closeslaname";
			public const string Config = "beone_config";
			public const string ConfigName = "beone_configname";
			public const string Connexity = "beone_connexity";
			public const string ConnexityName = "beone_connexityname";
			public const string Contractid = "beone_contractid";
			public const string ContractidName = "beone_contractidname";
			public const string Contractlist = "beone_contractlist";
			public const string Contractnumber = "beone_contractnumber";
			public const string CustomerName = "beone_customername";
			public const string Datein = "beone_datein";
			public const string Declarationnumber1 = "beone_declarationnumber1";
			public const string Declarationnumber2 = "beone_declarationnumber2";
			public const string Declarationnumbers = "beone_declarationnumbers";
			public const string Description = "beone_description";
			public const string Domain = "beone_domain";
			public const string Domainaccident_amount = "beone_domainaccident_amount";
			public const string Domaincar_amount = "beone_domaincar_amount";
			public const string Domainfire_amount = "beone_domainfire_amount";
			public const string Domainfireengineering_amount = "beone_domainfireengineering_amount";
			public const string Domainlaw_amount = "beone_domainlaw_amount";
			public const string Domainliability_amount = "beone_domainliability_amount";
			public const string DomainName = "beone_domainname";
			public const string Dossierid = "beone_dossierid";
			public const string DossieridName = "beone_dossieridname";
			public const string Dossiernumber = "beone_dossiernumber";
			public const string Duedate = "beone_duedate";
			public const string Entityid = "beone_entityid";
			public const string EntityidName = "beone_entityidname";
			public const string EntityidYomiName = "beone_entityidyominame";
			public const string Externalofferref = "beone_externalofferref";
			public const string Externalrework = "beone_externalrework";
			public const string ExternalreworkName = "beone_externalreworkname";
			public const string Firsttodounderwriterid = "beone_firsttodounderwriterid";
			public const string FirsttodounderwriteridName = "beone_firsttodounderwriteridname";
			public const string Fleetnumber = "beone_fleetnumber";
			public const string Forcedduedate = "beone_forcedduedate";
			public const string Groupofferid = "beone_groupofferid";
			public const string GroupofferidName = "beone_groupofferidname";
			public const string Importance = "beone_importance";
			public const string ImportanceName = "beone_importancename";
			public const string Internalexternal = "beone_internalexternal";
			public const string InternalexternalName = "beone_internalexternalname";
			public const string Internalrework = "beone_internalrework";
			public const string InternalreworkName = "beone_internalreworkname";
			public const string Isautoclose = "beone_isautoclose";
			public const string IsautocloseName = "beone_isautoclosename";
			public const string IsUrgent = "beone_isurgent";
			public const string IsurgentName = "beone_isurgentname";
			public const string Json = "beone_json";
			public const string Licenseplate = "beone_licenseplate";
			public const string Mailboxid = "beone_mailboxid";
			public const string Maindomain = "beone_maindomain";
			public const string Maindomaingroup = "beone_maindomaingroup";
			public const string MaindomaingroupName = "beone_maindomaingroupname";
			public const string MaindomainName = "beone_maindomainname";
			public const string Messageid = "beone_messageid";
			public const string Modifiedonbehalfby = "beone_modifiedonbehalfby";
			public const string ModifiedonbehalfbyName = "beone_modifiedonbehalfbyname";
			public const string ModifiedonbehalfbyYomiName = "beone_modifiedonbehalfbyyominame";
			public const string Motive = "beone_motive";
			public const string MotiveName = "beone_motivename";
			public const string Multiregardingid = "beone_multiregardingid";
			public const string MultiregardingidName = "beone_multiregardingidname";
			public const string Naceid = "beone_naceid";
			public const string NaceidName = "beone_naceidname";
			public const string Name = "beone_name";
			public const string Ncmpproid = "beone_ncmpproid";
			public const string NcmpproidName = "beone_ncmpproidname";
			public const string Newslainstanceid = "beone_newslainstanceid";
			public const string NewslainstanceidName = "beone_newslainstanceidname";
			public const string Offerpriority = "beone_offerpriority";
			public const string OfferpriorityName = "beone_offerpriorityname";
			public const string Onholdduration = "beone_onholdduration";
			public const string Onholdreason = "beone_onholdreason";
			public const string OnholdreasonName = "beone_onholdreasonname";
			public const string Origin = "beone_origin";
			public const string Originalawmessageid = "beone_originalawmessageid";
			public const string OriginalawmessageidName = "beone_originalawmessageidname";
			public const string Originaldocument = "beone_originaldocument";
			public const string OriginalEmailId = "beone_originalemailid";
			public const string OriginalEmailIdName = "beone_originalemailidname";
			public const string Originalletterid = "beone_originalletterid";
			public const string OriginalletteridName = "beone_originalletteridname";
			public const string Originalmpbid = "beone_originalmpbid";
			public const string OriginalmpbidName = "beone_originalmpbidname";
			public const string Originalphonecallid = "beone_originalphonecallid";
			public const string OriginalphonecallidName = "beone_originalphonecallidname";
			public const string Originalsenderemailaddress = "beone_originalsenderemailaddress";
			public const string OriginName = "beone_originname";
			public const string Processexpw = "beone_processexpw";
			public const string Productid = "beone_productid";
			public const string ProductidName = "beone_productidname";
			public const string Productids = "beone_productids";
			public const string Pruningdecision = "beone_pruningdecision";
			public const string PruningdecisionName = "beone_pruningdecisionname";
			public const string Pruningid = "beone_pruningid";
			public const string PruningidName = "beone_pruningidname";
			public const string RequestId = "beone_requestid";
			public const string Requesttypeid = "beone_requesttypeid";
			public const string RequesttypeidName = "beone_requesttypeidname";
			public const string Signature = "beone_signature";
			public const string SignatureName = "beone_signaturename";
			public const string Sladuration = "beone_sladuration";
			public const string SladurationName = "beone_sladurationname";
			public const string Slainstanceid = "beone_slainstanceid";
			public const string SlainstanceidName = "beone_slainstanceidname";
			public const string Slakpiinstancestatus = "beone_slakpiinstancestatus";
			public const string SlakpiinstancestatusName = "beone_slakpiinstancestatusname";
			public const string Slasuccessfailuredate = "beone_slasuccessfailuredate";
			public const string Specificdate = "beone_specificdate";
			public const string Totalproducts = "beone_totalproducts";
			public const string VerifyBroker = "beone_verifybroker";
			public const string VerifybrokerName = "beone_verifybrokername";
			public const string CreatedBy = "createdby";
			public const string CreatedByName = "createdbyname";
			public const string CreatedByYomiName = "createdbyyominame";
			public const string CreatedOn = "createdon";
			public const string CreatedOnBehalfBy = "createdonbehalfby";
			public const string CreatedOnBehalfByName = "createdonbehalfbyname";
			public const string CreatedOnBehalfByYomiName = "createdonbehalfbyyominame";
			public const string ImportSequenceNumber = "importsequencenumber";
			public const string LastOnHoldTime = "lastonholdtime";
			public const string ModifiedBy = "modifiedby";
			public const string ModifiedByName = "modifiedbyname";
			public const string ModifiedByYomiName = "modifiedbyyominame";
			public const string ModifiedOn = "modifiedon";
			public const string ModifiedOnBehalfBy = "modifiedonbehalfby";
			public const string ModifiedOnBehalfByName = "modifiedonbehalfbyname";
			public const string ModifiedOnBehalfByYomiName = "modifiedonbehalfbyyominame";
			public const string OnHoldTime = "onholdtime";
			public const string OverriddenCreatedOn = "overriddencreatedon";
			public const string OwnerId = "ownerid";
			public const string OwnerIdName = "owneridname";
			public const string OwnerIdYomiName = "owneridyominame";
			public const string OwningBusinessUnit = "owningbusinessunit";
			public const string OwningBusinessUnitName = "owningbusinessunitname";
			public const string OwningTeam = "owningteam";
			public const string OwningUser = "owninguser";
			public const string SLAId = "slaid";
			public const string SLAInvokedId = "slainvokedid";
			public const string SLAInvokedIdName = "slainvokedidname";
			public const string SLAName = "slaname";
			public const string Statecode = "statecode";
			public const string StatecodeName = "statecodename";
			public const string Statuscode = "statuscode";
			public const string StatuscodeName = "statuscodename";
			public const string TimeZoneRuleVersionNumber = "timezoneruleversionnumber";
			public const string UTCConversionTimeZoneCode = "utcconversiontimezonecode";
			public const string VersionNumber = "versionnumber";
		}
	
		/// <summary>
		/// Max Length: 200</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Actgmotivelabel
		
		[AttributeLogicalName("beone_actgmotivelabel")]
		public string Actgmotivelabel
		{
			get => TryGetAttributeValue("beone_actgmotivelabel", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Targets: ag_actiontype</br>
		/// </summary>
		//Actiontypeid
		
		[AttributeLogicalName("beone_actiontypeid")]
		public EntityReference Actiontypeid
		{
			get => TryGetAttributeValue("beone_actiontypeid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_actiontypeid</br>
		/// Max Length: 600</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//ActiontypeidName
		
		[AttributeLogicalName("beone_actiontypeidname")]
		public string ActiontypeidName
		{
			get => FormattedValues.Contains("beone_actiontypeid") ? FormattedValues["beone_actiontypeid"] : null;
		
		}
		/// <summary>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Auto Number Format: 
		/// </summary>
		//Activity
		
		[AttributeLogicalName("beone_activity")]
		public string Activity
		{
			get => TryGetAttributeValue("beone_activity", out string value) ? value : null;
		}
		/// <summary>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Adagionumber
		
		[AttributeLogicalName("beone_adagionumber")]
		public string Adagionumber
		{
			get => TryGetAttributeValue("beone_adagionumber", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Agolstatus
		
		[AttributeLogicalName("beone_agolstatus")]
		public Request.Choices.AgolStatus? Agolstatus
		{
			get => TryGetAttributeValue("beone_agolstatus", out OptionSetValue opt) && opt == null ? null : (Request.Choices.AgolStatus)opt.Value;
		}
		/// <summary>
		/// Attribute of: beone_agolstatus</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//AgolstatusName
		
		[AttributeLogicalName("beone_agolstatusname")]
		public string AgolstatusName
		{
			get => FormattedValues.Contains("beone_agolstatus") ? FormattedValues["beone_agolstatusname"] : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Annulationdate
		
		[AttributeLogicalName("beone_annulationdate")]
		public DateTime? Annulationdate
		{
			get => TryGetAttributeValue("beone_annulationdate", out DateTime value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Awmessagetype
		
		[AttributeLogicalName("beone_awmessagetype")]
		public Request.Choices.AwMessageType? Awmessagetype
		{
			get => TryGetAttributeValue("beone_awmessagetype", out OptionSetValue opt) && opt == null ? null : (Request.Choices.AwMessageType)opt.Value;
		}
		/// <summary>
		/// Attribute of: beone_awmessagetype</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//AwmessagetypeName
		
		[AttributeLogicalName("beone_awmessagetypename")]
		public string AwmessagetypeName
		{
			get => FormattedValues.Contains("beone_awmessagetype") ? FormattedValues["beone_awmessagetypename"] : null;
		}
	
		/// <summary>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Awproducts
		
		[AttributeLogicalName("beone_awproducts")]
		public string Awproducts
		{
			get => TryGetAttributeValue("beone_awproducts", out string value) ? value : null;
		}
		/// <summary>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Auto Number Format: 
		/// </summary>
		//BCENumber
		
		[AttributeLogicalName("beone_bcenumber")]
		public string BCENumber
		{
			get => TryGetAttributeValue("beone_bcenumber", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Brokerfeedback
		
		[AttributeLogicalName("beone_brokerfeedback")]
		public Request.Choices.BrokerFeedback? Brokerfeedback
		{
			get => TryGetAttributeValue("beone_brokerfeedback", out OptionSetValue opt) && opt == null ? null : (Request.Choices.BrokerFeedback)opt.Value;
		}
		/// <summary>
		/// Attribute of: beone_brokerfeedback</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//BrokerfeedbackName
		
		[AttributeLogicalName("beone_brokerfeedbackname")]
		public string BrokerfeedbackName
		{
			get => FormattedValues.Contains("beone_brokerfeedback") ? FormattedValues["beone_brokerfeedbackname"] : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Targets: beone_claimhistory</br>
		/// </summary>
		//Claimhistoryid
		
		[AttributeLogicalName("beone_claimhistoryid")]
		public EntityReference Claimhistoryid
		{
			get => TryGetAttributeValue("beone_claimhistoryid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_claimhistoryid</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//ClaimhistoryidName
		
		[AttributeLogicalName("beone_claimhistoryidname")]
		public string ClaimhistoryidName
		{
			get => FormattedValues.Contains("beone_claimhistoryid") ? FormattedValues["beone_claimhistoryid"] : null;
		
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Closesla
		
		[AttributeLogicalName("beone_closesla")]
		public bool? Closesla
		{
			get => TryGetAttributeValue("beone_closesla", out bool value) ? value : null;
		}
		/// <summary>
		/// Attribute of: beone_closesla</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//CloseslaName
		
		[AttributeLogicalName("beone_closeslaname")]
		public string CloseslaName
		{
			get => FormattedValues.Contains("beone_closesla") ? FormattedValues["beone_closeslaname"] : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Targets: beone_requesttypeconfig</br>
		/// </summary>
		//Config
		
		[AttributeLogicalName("beone_config")]
		public EntityReference Config
		{
			get => TryGetAttributeValue("beone_config", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_config</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//ConfigName
		
		[AttributeLogicalName("beone_configname")]
		public string ConfigName
		{
			get => FormattedValues.Contains("beone_config") ? FormattedValues["beone_config"] : null;
		
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Connexity
		
		[AttributeLogicalName("beone_connexity")]
		public Request.Choices.FlowConnexity? Connexity
		{
			get => TryGetAttributeValue("beone_connexity", out OptionSetValue opt) && opt == null ? null : (Request.Choices.FlowConnexity)opt.Value;
		}
		/// <summary>
		/// Attribute of: beone_connexity</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//ConnexityName
		
		[AttributeLogicalName("beone_connexityname")]
		public string ConnexityName
		{
			get => FormattedValues.Contains("beone_connexity") ? FormattedValues["beone_connexityname"] : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Targets: beone_contract</br>
		/// </summary>
		//Contractid
		
		[AttributeLogicalName("beone_contractid")]
		public EntityReference Contractid
		{
			get => TryGetAttributeValue("beone_contractid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_contractid</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//ContractidName
		
		[AttributeLogicalName("beone_contractidname")]
		public string ContractidName
		{
			get => FormattedValues.Contains("beone_contractid") ? FormattedValues["beone_contractid"] : null;
		
		}
		/// <summary>
		/// Max Length: 500</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Contractlist
		
		[AttributeLogicalName("beone_contractlist")]
		public string Contractlist
		{
			get => TryGetAttributeValue("beone_contractlist", out string value) ? value : null;
		}
		/// <summary>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Contractnumber
		
		[AttributeLogicalName("beone_contractnumber")]
		public string Contractnumber
		{
			get => TryGetAttributeValue("beone_contractnumber", out string value) ? value : null;
		}
		/// <summary>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Auto Number Format: 
		/// </summary>
		//CustomerName
		
		[AttributeLogicalName("beone_customername")]
		public string CustomerName
		{
			get => TryGetAttributeValue("beone_customername", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: ApplicationRequired</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Datein
		
		[AttributeLogicalName("beone_datein")]
		public DateTime? Datein
		{
			get => TryGetAttributeValue("beone_datein", out DateTime value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Declarationnumber1
		
		[AttributeLogicalName("beone_declarationnumber1")]
		public int? Declarationnumber1
		{
			get => TryGetAttributeValue("beone_declarationnumber1", out int value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Declarationnumber2
		
		[AttributeLogicalName("beone_declarationnumber2")]
		public int? Declarationnumber2
		{
			get => TryGetAttributeValue("beone_declarationnumber2", out int value) ? value : null;
		}
		/// <summary>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Declarationnumbers
		
		[AttributeLogicalName("beone_declarationnumbers")]
		public string Declarationnumbers
		{
			get => TryGetAttributeValue("beone_declarationnumbers", out string value) ? value : null;
		}
		/// <summary>
		/// Max Length: 2000</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Description
		
		[AttributeLogicalName("beone_description")]
		public string Description
		{
			get => TryGetAttributeValue("beone_description", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Domain
		
		[AttributeLogicalName("beone_domain")]
		public Request.Choices.FlowTaskDomain? Domain
		{
			get => TryGetAttributeValue("beone_domain", out OptionSetValue opt) && opt == null ? null : (Request.Choices.FlowTaskDomain)opt.Value;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Domainaccident_amount
		
		[AttributeLogicalName("beone_domainaccident_amount")]
		public int? Domainaccident_amount
		{
			get => TryGetAttributeValue("beone_domainaccident_amount", out int value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Domaincar_amount
		
		[AttributeLogicalName("beone_domaincar_amount")]
		public int? Domaincar_amount
		{
			get => TryGetAttributeValue("beone_domaincar_amount", out int value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Domainfire_amount
		
		[AttributeLogicalName("beone_domainfire_amount")]
		public int? Domainfire_amount
		{
			get => TryGetAttributeValue("beone_domainfire_amount", out int value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Domainfireengineering_amount
		
		[AttributeLogicalName("beone_domainfireengineering_amount")]
		public int? Domainfireengineering_amount
		{
			get => TryGetAttributeValue("beone_domainfireengineering_amount", out int value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Domainlaw_amount
		
		[AttributeLogicalName("beone_domainlaw_amount")]
		public int? Domainlaw_amount
		{
			get => TryGetAttributeValue("beone_domainlaw_amount", out int value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Domainliability_amount
		
		[AttributeLogicalName("beone_domainliability_amount")]
		public int? Domainliability_amount
		{
			get => TryGetAttributeValue("beone_domainliability_amount", out int value) ? value : null;
		}
		/// <summary>
		/// Attribute of: beone_domain</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//DomainName
		
		[AttributeLogicalName("beone_domainname")]
		public string DomainName
		{
			get => FormattedValues.Contains("beone_domain") ? FormattedValues["beone_domainname"] : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Targets: beone_dossier</br>
		/// </summary>
		//Dossierid
		
		[AttributeLogicalName("beone_dossierid")]
		public EntityReference Dossierid
		{
			get => TryGetAttributeValue("beone_dossierid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_dossierid</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//DossieridName
		
		[AttributeLogicalName("beone_dossieridname")]
		public string DossieridName
		{
			get => FormattedValues.Contains("beone_dossierid") ? FormattedValues["beone_dossierid"] : null;
		
		}
		/// <summary>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Dossiernumber
		
		[AttributeLogicalName("beone_dossiernumber")]
		public string Dossiernumber
		{
			get => TryGetAttributeValue("beone_dossiernumber", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Duedate
		
		[AttributeLogicalName("beone_duedate")]
		public DateTime? Duedate
		{
			get => TryGetAttributeValue("beone_duedate", out DateTime value) ? value : null;
		}
		/// <summary>
		/// Required Level: ApplicationRequired</br>
		/// Valid for: Create Update Read</br>
		/// Targets: team</br>
		/// </summary>
		//Entityid
		
		[AttributeLogicalName("beone_entityid")]
		public EntityReference Entityid
		{
			get => TryGetAttributeValue("beone_entityid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_entityid</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//EntityidName
		
		[AttributeLogicalName("beone_entityidname")]
		public string EntityidName
		{
			get => FormattedValues.Contains("beone_entityid") ? FormattedValues["beone_entityid"] : null;
		
		}
		/// <summary>
		/// Attribute of: beone_entityid</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//EntityidYomiName
		
		[AttributeLogicalName("beone_entityidyominame")]
		public string EntityidYomiName
		{
			get => FormattedValues.Contains("beone_entityid") ? FormattedValues["beone_entityid"] : null;
		
		}
		/// <summary>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Externalofferref
		
		[AttributeLogicalName("beone_externalofferref")]
		public string Externalofferref
		{
			get => TryGetAttributeValue("beone_externalofferref", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Externalrework
		
		[AttributeLogicalName("beone_externalrework")]
		public bool? Externalrework
		{
			get => TryGetAttributeValue("beone_externalrework", out bool value) ? value : null;
		}
		/// <summary>
		/// Attribute of: beone_externalrework</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//ExternalreworkName
		
		[AttributeLogicalName("beone_externalreworkname")]
		public string ExternalreworkName
		{
			get => FormattedValues.Contains("beone_externalrework") ? FormattedValues["beone_externalreworkname"] : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Targets: beone_underwriter</br>
		/// </summary>
		//Firsttodounderwriterid
		
		[AttributeLogicalName("beone_firsttodounderwriterid")]
		public EntityReference Firsttodounderwriterid
		{
			get => TryGetAttributeValue("beone_firsttodounderwriterid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_firsttodounderwriterid</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//FirsttodounderwriteridName
		
		[AttributeLogicalName("beone_firsttodounderwriteridname")]
		public string FirsttodounderwriteridName
		{
			get => FormattedValues.Contains("beone_firsttodounderwriterid") ? FormattedValues["beone_firsttodounderwriterid"] : null;
		
		}
		/// <summary>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Fleetnumber
		
		[AttributeLogicalName("beone_fleetnumber")]
		public string Fleetnumber
		{
			get => TryGetAttributeValue("beone_fleetnumber", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Forcedduedate
		
		[AttributeLogicalName("beone_forcedduedate")]
		public DateTime? Forcedduedate
		{
			get => TryGetAttributeValue("beone_forcedduedate", out DateTime value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Targets: beone_groupoffer</br>
		/// </summary>
		//Groupofferid
		
		[AttributeLogicalName("beone_groupofferid")]
		public EntityReference Groupofferid
		{
			get => TryGetAttributeValue("beone_groupofferid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_groupofferid</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//GroupofferidName
		
		[AttributeLogicalName("beone_groupofferidname")]
		public string GroupofferidName
		{
			get => FormattedValues.Contains("beone_groupofferid") ? FormattedValues["beone_groupofferid"] : null;
		
		}
		/// <summary>
		/// Required Level: ApplicationRequired</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Importance
		
		[AttributeLogicalName("beone_importance")]
		public Request.Choices.RequestImportance? Importance
		{
			get => TryGetAttributeValue("beone_importance", out OptionSetValue opt) && opt == null ? null : (Request.Choices.RequestImportance)opt.Value;
		}
		/// <summary>
		/// Attribute of: beone_importance</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//ImportanceName
		
		[AttributeLogicalName("beone_importancename")]
		public string ImportanceName
		{
			get => FormattedValues.Contains("beone_importance") ? FormattedValues["beone_importancename"] : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Internalexternal
		
		[AttributeLogicalName("beone_internalexternal")]
		public Request.Choices.InternalExternal? Internalexternal
		{
			get => TryGetAttributeValue("beone_internalexternal", out OptionSetValue opt) && opt == null ? null : (Request.Choices.InternalExternal)opt.Value;
		}
		/// <summary>
		/// Attribute of: beone_internalexternal</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//InternalexternalName
		
		[AttributeLogicalName("beone_internalexternalname")]
		public string InternalexternalName
		{
			get => FormattedValues.Contains("beone_internalexternal") ? FormattedValues["beone_internalexternalname"] : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Internalrework
		
		[AttributeLogicalName("beone_internalrework")]
		public bool? Internalrework
		{
			get => TryGetAttributeValue("beone_internalrework", out bool value) ? value : null;
		}
		/// <summary>
		/// Attribute of: beone_internalrework</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//InternalreworkName
		
		[AttributeLogicalName("beone_internalreworkname")]
		public string InternalreworkName
		{
			get => FormattedValues.Contains("beone_internalrework") ? FormattedValues["beone_internalreworkname"] : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Isautoclose
		
		[AttributeLogicalName("beone_isautoclose")]
		public bool? Isautoclose
		{
			get => TryGetAttributeValue("beone_isautoclose", out bool value) ? value : null;
		}
		/// <summary>
		/// Attribute of: beone_isautoclose</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//IsautocloseName
		
		[AttributeLogicalName("beone_isautoclosename")]
		public string IsautocloseName
		{
			get => FormattedValues.Contains("beone_isautoclose") ? FormattedValues["beone_isautoclosename"] : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//IsUrgent
		
		[AttributeLogicalName("beone_isurgent")]
		public bool? IsUrgent
		{
			get => TryGetAttributeValue("beone_isurgent", out bool value) ? value : null;
		}
		/// <summary>
		/// Attribute of: beone_isurgent</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//IsurgentName
		
		[AttributeLogicalName("beone_isurgentname")]
		public string IsurgentName
		{
			get => FormattedValues.Contains("beone_isurgent") ? FormattedValues["beone_isurgentname"] : null;
		}
	
		/// <summary>
		/// Max Length: 10000</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Json
		
		[AttributeLogicalName("beone_json")]
		public string Json
		{
			get => TryGetAttributeValue("beone_json", out string value) ? value : null;
		}
		/// <summary>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Licenseplate
		
		[AttributeLogicalName("beone_licenseplate")]
		public string Licenseplate
		{
			get => TryGetAttributeValue("beone_licenseplate", out string value) ? value : null;
		}
		/// <summary>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Mailboxid
		
		[AttributeLogicalName("beone_mailboxid")]
		public string Mailboxid
		{
			get => TryGetAttributeValue("beone_mailboxid", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Maindomain
		
		[AttributeLogicalName("beone_maindomain")]
		public Request.Choices.MainDomain? Maindomain
		{
			get => TryGetAttributeValue("beone_maindomain", out OptionSetValue opt) && opt == null ? null : (Request.Choices.MainDomain)opt.Value;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Maindomaingroup
		
		[AttributeLogicalName("beone_maindomaingroup")]
		public Request.Choices.MainDomainGroup? Maindomaingroup
		{
			get => TryGetAttributeValue("beone_maindomaingroup", out OptionSetValue opt) && opt == null ? null : (Request.Choices.MainDomainGroup)opt.Value;
		}
		/// <summary>
		/// Attribute of: beone_maindomaingroup</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//MaindomaingroupName
		
		[AttributeLogicalName("beone_maindomaingroupname")]
		public string MaindomaingroupName
		{
			get => FormattedValues.Contains("beone_maindomaingroup") ? FormattedValues["beone_maindomaingroupname"] : null;
		}
	
		/// <summary>
		/// Attribute of: beone_maindomain</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//MaindomainName
		
		[AttributeLogicalName("beone_maindomainname")]
		public string MaindomainName
		{
			get => FormattedValues.Contains("beone_maindomain") ? FormattedValues["beone_maindomainname"] : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Messageid
		
		[AttributeLogicalName("beone_messageid")]
		public int? Messageid
		{
			get => TryGetAttributeValue("beone_messageid", out int value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Targets: systemuser</br>
		/// </summary>
		//Modifiedonbehalfby
		
		[AttributeLogicalName("beone_modifiedonbehalfby")]
		public EntityReference Modifiedonbehalfby
		{
			get => TryGetAttributeValue("beone_modifiedonbehalfby", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_modifiedonbehalfby</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//ModifiedonbehalfbyName
		
		[AttributeLogicalName("beone_modifiedonbehalfbyname")]
		public string ModifiedonbehalfbyName
		{
			get => FormattedValues.Contains("beone_modifiedonbehalfby") ? FormattedValues["beone_modifiedonbehalfby"] : null;
		
		}
		/// <summary>
		/// Attribute of: beone_modifiedonbehalfby</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//ModifiedonbehalfbyYomiName
		
		[AttributeLogicalName("beone_modifiedonbehalfbyyominame")]
		public string ModifiedonbehalfbyYomiName
		{
			get => FormattedValues.Contains("beone_modifiedonbehalfby") ? FormattedValues["beone_modifiedonbehalfby"] : null;
		
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Motive
		
		[AttributeLogicalName("beone_motive")]
		public Request.Choices.Motive? Motive
		{
			get => TryGetAttributeValue("beone_motive", out OptionSetValue opt) && opt == null ? null : (Request.Choices.Motive)opt.Value;
		}
		/// <summary>
		/// Attribute of: beone_motive</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//MotiveName
		
		[AttributeLogicalName("beone_motivename")]
		public string MotiveName
		{
			get => FormattedValues.Contains("beone_motive") ? FormattedValues["beone_motivename"] : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Targets: beone_multiregardingrequest</br>
		/// </summary>
		//Multiregardingid
		
		[AttributeLogicalName("beone_multiregardingid")]
		public EntityReference Multiregardingid
		{
			get => TryGetAttributeValue("beone_multiregardingid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_multiregardingid</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//MultiregardingidName
		
		[AttributeLogicalName("beone_multiregardingidname")]
		public string MultiregardingidName
		{
			get => FormattedValues.Contains("beone_multiregardingid") ? FormattedValues["beone_multiregardingid"] : null;
		
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Targets: beone_nace</br>
		/// </summary>
		//Naceid
		
		[AttributeLogicalName("beone_naceid")]
		public EntityReference Naceid
		{
			get => TryGetAttributeValue("beone_naceid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_naceid</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//NaceidName
		
		[AttributeLogicalName("beone_naceidname")]
		public string NaceidName
		{
			get => FormattedValues.Contains("beone_naceid") ? FormattedValues["beone_naceid"] : null;
		
		}
		/// <summary>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Auto Number Format: RQ-{SEQNUM:9}
		/// </summary>
		//Name
		
		[AttributeLogicalName("beone_name")]
		public string Name
		{
			get => TryGetAttributeValue("beone_name", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Targets: beone_ncmppro</br>
		/// </summary>
		//Ncmpproid
		
		[AttributeLogicalName("beone_ncmpproid")]
		public EntityReference Ncmpproid
		{
			get => TryGetAttributeValue("beone_ncmpproid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_ncmpproid</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//NcmpproidName
		
		[AttributeLogicalName("beone_ncmpproidname")]
		public string NcmpproidName
		{
			get => FormattedValues.Contains("beone_ncmpproid") ? FormattedValues["beone_ncmpproid"] : null;
		
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Targets: beone_slainstance</br>
		/// </summary>
		//Newslainstanceid
		
		[AttributeLogicalName("beone_newslainstanceid")]
		public EntityReference Newslainstanceid
		{
			get => TryGetAttributeValue("beone_newslainstanceid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_newslainstanceid</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//NewslainstanceidName
		
		[AttributeLogicalName("beone_newslainstanceidname")]
		public string NewslainstanceidName
		{
			get => FormattedValues.Contains("beone_newslainstanceid") ? FormattedValues["beone_newslainstanceid"] : null;
		
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Offerpriority
		
		[AttributeLogicalName("beone_offerpriority")]
		public Request.Choices.FlowPriority? Offerpriority
		{
			get => TryGetAttributeValue("beone_offerpriority", out OptionSetValue opt) && opt == null ? null : (Request.Choices.FlowPriority)opt.Value;
		}
		/// <summary>
		/// Attribute of: beone_offerpriority</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//OfferpriorityName
		
		[AttributeLogicalName("beone_offerpriorityname")]
		public string OfferpriorityName
		{
			get => FormattedValues.Contains("beone_offerpriority") ? FormattedValues["beone_offerpriorityname"] : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Onholdduration
		
		[AttributeLogicalName("beone_onholdduration")]
		public decimal? Onholdduration
		{
			get => TryGetAttributeValue("beone_onholdduration", out decimal value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Onholdreason
		
		[AttributeLogicalName("beone_onholdreason")]
		public Request.Choices.OnHoldReason? Onholdreason
		{
			get => TryGetAttributeValue("beone_onholdreason", out OptionSetValue opt) && opt == null ? null : (Request.Choices.OnHoldReason)opt.Value;
		}
		/// <summary>
		/// Attribute of: beone_onholdreason</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//OnholdreasonName
		
		[AttributeLogicalName("beone_onholdreasonname")]
		public string OnholdreasonName
		{
			get => FormattedValues.Contains("beone_onholdreason") ? FormattedValues["beone_onholdreasonname"] : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Origin
		
		[AttributeLogicalName("beone_origin")]
		public Request.Choices.RequestOrigin? Origin
		{
			get => TryGetAttributeValue("beone_origin", out OptionSetValue opt) && opt == null ? null : (Request.Choices.RequestOrigin)opt.Value;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Targets: beone_awmessage</br>
		/// </summary>
		//Originalawmessageid
		
		[AttributeLogicalName("beone_originalawmessageid")]
		public EntityReference Originalawmessageid
		{
			get => TryGetAttributeValue("beone_originalawmessageid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_originalawmessageid</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//OriginalawmessageidName
		
		[AttributeLogicalName("beone_originalawmessageidname")]
		public string OriginalawmessageidName
		{
			get => FormattedValues.Contains("beone_originalawmessageid") ? FormattedValues["beone_originalawmessageid"] : null;
		
		}
		/// <summary>
		/// Max Length: 200</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Originaldocument
		
		[AttributeLogicalName("beone_originaldocument")]
		public string Originaldocument
		{
			get => TryGetAttributeValue("beone_originaldocument", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Targets: email</br>
		/// </summary>
		//OriginalEmailId
		
		[AttributeLogicalName("beone_originalemailid")]
		public EntityReference OriginalEmailId
		{
			get => TryGetAttributeValue("beone_originalemailid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_originalemailid</br>
		/// Max Length: 800</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//OriginalEmailIdName
		
		[AttributeLogicalName("beone_originalemailidname")]
		public string OriginalEmailIdName
		{
			get => FormattedValues.Contains("beone_originalemailid") ? FormattedValues["beone_originalemailid"] : null;
		
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Targets: letter</br>
		/// </summary>
		//Originalletterid
		
		[AttributeLogicalName("beone_originalletterid")]
		public EntityReference Originalletterid
		{
			get => TryGetAttributeValue("beone_originalletterid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_originalletterid</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//OriginalletteridName
		
		[AttributeLogicalName("beone_originalletteridname")]
		public string OriginalletteridName
		{
			get => FormattedValues.Contains("beone_originalletterid") ? FormattedValues["beone_originalletterid"] : null;
		
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Targets: beone_mpb</br>
		/// </summary>
		//Originalmpbid
		
		[AttributeLogicalName("beone_originalmpbid")]
		public EntityReference Originalmpbid
		{
			get => TryGetAttributeValue("beone_originalmpbid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_originalmpbid</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//OriginalmpbidName
		
		[AttributeLogicalName("beone_originalmpbidname")]
		public string OriginalmpbidName
		{
			get => FormattedValues.Contains("beone_originalmpbid") ? FormattedValues["beone_originalmpbid"] : null;
		
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Targets: phonecall</br>
		/// </summary>
		//Originalphonecallid
		
		[AttributeLogicalName("beone_originalphonecallid")]
		public EntityReference Originalphonecallid
		{
			get => TryGetAttributeValue("beone_originalphonecallid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_originalphonecallid</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//OriginalphonecallidName
		
		[AttributeLogicalName("beone_originalphonecallidname")]
		public string OriginalphonecallidName
		{
			get => FormattedValues.Contains("beone_originalphonecallid") ? FormattedValues["beone_originalphonecallid"] : null;
		
		}
		/// <summary>
		/// Max Length: 200</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Originalsenderemailaddress
		
		[AttributeLogicalName("beone_originalsenderemailaddress")]
		public string Originalsenderemailaddress
		{
			get => TryGetAttributeValue("beone_originalsenderemailaddress", out string value) ? value : null;
		}
		/// <summary>
		/// Attribute of: beone_origin</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//OriginName
		
		[AttributeLogicalName("beone_originname")]
		public string OriginName
		{
			get => FormattedValues.Contains("beone_origin") ? FormattedValues["beone_originname"] : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Processexpw
		
		[AttributeLogicalName("beone_processexpw")]
		public int? Processexpw
		{
			get => TryGetAttributeValue("beone_processexpw", out int value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Targets: product</br>
		/// </summary>
		//Productid
		
		[AttributeLogicalName("beone_productid")]
		public EntityReference Productid
		{
			get => TryGetAttributeValue("beone_productid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_productid</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//ProductidName
		
		[AttributeLogicalName("beone_productidname")]
		public string ProductidName
		{
			get => FormattedValues.Contains("beone_productid") ? FormattedValues["beone_productid"] : null;
		
		}
		/// <summary>
		/// Max Length: 700</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Productids
		
		[AttributeLogicalName("beone_productids")]
		public string Productids
		{
			get => TryGetAttributeValue("beone_productids", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Pruningdecision
		
		[AttributeLogicalName("beone_pruningdecision")]
		public Request.Choices.PruningDecision? Pruningdecision
		{
			get => TryGetAttributeValue("beone_pruningdecision", out OptionSetValue opt) && opt == null ? null : (Request.Choices.PruningDecision)opt.Value;
		}
		/// <summary>
		/// Attribute of: beone_pruningdecision</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//PruningdecisionName
		
		[AttributeLogicalName("beone_pruningdecisionname")]
		public string PruningdecisionName
		{
			get => FormattedValues.Contains("beone_pruningdecision") ? FormattedValues["beone_pruningdecisionname"] : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Targets: beone_pruning</br>
		/// </summary>
		//Pruningid
		
		[AttributeLogicalName("beone_pruningid")]
		public EntityReference Pruningid
		{
			get => TryGetAttributeValue("beone_pruningid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_pruningid</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//PruningidName
		
		[AttributeLogicalName("beone_pruningidname")]
		public string PruningidName
		{
			get => FormattedValues.Contains("beone_pruningid") ? FormattedValues["beone_pruningid"] : null;
		
		}
		/// <summary>
		/// Required Level: SystemRequired</br>
		/// Valid for: Create Read</br>
		/// </summary>
		//RequestId
		
		
		/// <summary>
		/// Required Level: ApplicationRequired</br>
		/// Valid for: Create Update Read</br>
		/// Targets: ag_requesttype</br>
		/// </summary>
		//Requesttypeid
		
		[AttributeLogicalName("beone_requesttypeid")]
		public EntityReference Requesttypeid
		{
			get => TryGetAttributeValue("beone_requesttypeid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_requesttypeid</br>
		/// Max Length: 500</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//RequesttypeidName
		
		[AttributeLogicalName("beone_requesttypeidname")]
		public string RequesttypeidName
		{
			get => FormattedValues.Contains("beone_requesttypeid") ? FormattedValues["beone_requesttypeid"] : null;
		
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Signature
		
		[AttributeLogicalName("beone_signature")]
		public Request.Choices.Boolean? Signature
		{
			get => TryGetAttributeValue("beone_signature", out OptionSetValue opt) && opt == null ? null : (Request.Choices.Boolean)opt.Value;
		}
		/// <summary>
		/// Attribute of: beone_signature</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//SignatureName
		
		[AttributeLogicalName("beone_signaturename")]
		public string SignatureName
		{
			get => FormattedValues.Contains("beone_signature") ? FormattedValues["beone_signaturename"] : null;
		}
	
		/// <summary>
		/// Required Level: ApplicationRequired</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Sladuration
		
		[AttributeLogicalName("beone_sladuration")]
		public Request.Choices.SlaDuration? Sladuration
		{
			get => TryGetAttributeValue("beone_sladuration", out OptionSetValue opt) && opt == null ? null : (Request.Choices.SlaDuration)opt.Value;
		}
		/// <summary>
		/// Attribute of: beone_sladuration</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//SladurationName
		
		[AttributeLogicalName("beone_sladurationname")]
		public string SladurationName
		{
			get => FormattedValues.Contains("beone_sladuration") ? FormattedValues["beone_sladurationname"] : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Targets: slakpiinstance</br>
		/// </summary>
		//Slainstanceid
		
		[AttributeLogicalName("beone_slainstanceid")]
		public EntityReference Slainstanceid
		{
			get => TryGetAttributeValue("beone_slainstanceid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_slainstanceid</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//SlainstanceidName
		
		[AttributeLogicalName("beone_slainstanceidname")]
		public string SlainstanceidName
		{
			get => FormattedValues.Contains("beone_slainstanceid") ? FormattedValues["beone_slainstanceid"] : null;
		
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Slakpiinstancestatus
		
		[AttributeLogicalName("beone_slakpiinstancestatus")]
		public Request.Choices.SlaKpiInstanceStatus? Slakpiinstancestatus
		{
			get => TryGetAttributeValue("beone_slakpiinstancestatus", out OptionSetValue opt) && opt == null ? null : (Request.Choices.SlaKpiInstanceStatus)opt.Value;
		}
		/// <summary>
		/// Attribute of: beone_slakpiinstancestatus</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//SlakpiinstancestatusName
		
		[AttributeLogicalName("beone_slakpiinstancestatusname")]
		public string SlakpiinstancestatusName
		{
			get => FormattedValues.Contains("beone_slakpiinstancestatus") ? FormattedValues["beone_slakpiinstancestatusname"] : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Slasuccessfailuredate
		
		[AttributeLogicalName("beone_slasuccessfailuredate")]
		public DateTime? Slasuccessfailuredate
		{
			get => TryGetAttributeValue("beone_slasuccessfailuredate", out DateTime value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Specificdate
		
		[AttributeLogicalName("beone_specificdate")]
		public DateTime? Specificdate
		{
			get => TryGetAttributeValue("beone_specificdate", out DateTime value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//Totalproducts
		
		[AttributeLogicalName("beone_totalproducts")]
		public decimal? Totalproducts
		{
			get => TryGetAttributeValue("beone_totalproducts", out decimal value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//VerifyBroker
		
		[AttributeLogicalName("beone_verifybroker")]
		public bool? VerifyBroker
		{
			get => TryGetAttributeValue("beone_verifybroker", out bool value) ? value : null;
		}
		/// <summary>
		/// Attribute of: beone_verifybroker</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//VerifybrokerName
		
		[AttributeLogicalName("beone_verifybrokername")]
		public string VerifybrokerName
		{
			get => FormattedValues.Contains("beone_verifybroker") ? FormattedValues["beone_verifybrokername"] : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// Targets: systemuser</br>
		/// </summary>
		//CreatedBy
		
		[AttributeLogicalName("createdby")]
		public EntityReference CreatedBy
		{
			get => TryGetAttributeValue("createdby", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: createdby</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//CreatedByName
		
		[AttributeLogicalName("createdbyname")]
		public string CreatedByName
		{
			get => FormattedValues.Contains("createdby") ? FormattedValues["createdby"] : null;
		
		}
		/// <summary>
		/// Attribute of: createdby</br>
		/// Max Length: 100</br>
		/// Required Level: SystemRequired</br>
		/// Valid for: Read</br>
		/// </summary>
		//CreatedByYomiName
		
		[AttributeLogicalName("createdbyyominame")]
		public string CreatedByYomiName
		{
			get => FormattedValues.Contains("createdby") ? FormattedValues["createdby"] : null;
		
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//CreatedOn
		
		[AttributeLogicalName("createdon")]
		public DateTime? CreatedOn
		{
			get => TryGetAttributeValue("createdon", out DateTime value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// Targets: systemuser</br>
		/// </summary>
		//CreatedOnBehalfBy
		
		[AttributeLogicalName("createdonbehalfby")]
		public EntityReference CreatedOnBehalfBy
		{
			get => TryGetAttributeValue("createdonbehalfby", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: createdonbehalfby</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//CreatedOnBehalfByName
		
		[AttributeLogicalName("createdonbehalfbyname")]
		public string CreatedOnBehalfByName
		{
			get => FormattedValues.Contains("createdonbehalfby") ? FormattedValues["createdonbehalfby"] : null;
		
		}
		/// <summary>
		/// Attribute of: createdonbehalfby</br>
		/// Max Length: 100</br>
		/// Required Level: SystemRequired</br>
		/// Valid for: Read</br>
		/// </summary>
		//CreatedOnBehalfByYomiName
		
		[AttributeLogicalName("createdonbehalfbyyominame")]
		public string CreatedOnBehalfByYomiName
		{
			get => FormattedValues.Contains("createdonbehalfby") ? FormattedValues["createdonbehalfby"] : null;
		
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Read</br>
		/// </summary>
		//ImportSequenceNumber
		
		[AttributeLogicalName("importsequencenumber")]
		public int? ImportSequenceNumber
		{
			get => TryGetAttributeValue("importsequencenumber", out int value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//LastOnHoldTime
		
		[AttributeLogicalName("lastonholdtime")]
		public DateTime? LastOnHoldTime
		{
			get => TryGetAttributeValue("lastonholdtime", out DateTime value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// Targets: systemuser</br>
		/// </summary>
		//ModifiedBy
		
		[AttributeLogicalName("modifiedby")]
		public EntityReference ModifiedBy
		{
			get => TryGetAttributeValue("modifiedby", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: modifiedby</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//ModifiedByName
		
		[AttributeLogicalName("modifiedbyname")]
		public string ModifiedByName
		{
			get => FormattedValues.Contains("modifiedby") ? FormattedValues["modifiedby"] : null;
		
		}
		/// <summary>
		/// Attribute of: modifiedby</br>
		/// Max Length: 100</br>
		/// Required Level: SystemRequired</br>
		/// Valid for: Read</br>
		/// </summary>
		//ModifiedByYomiName
		
		[AttributeLogicalName("modifiedbyyominame")]
		public string ModifiedByYomiName
		{
			get => FormattedValues.Contains("modifiedby") ? FormattedValues["modifiedby"] : null;
		
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//ModifiedOn
		
		[AttributeLogicalName("modifiedon")]
		public DateTime? ModifiedOn
		{
			get => TryGetAttributeValue("modifiedon", out DateTime value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// Targets: systemuser</br>
		/// </summary>
		//ModifiedOnBehalfBy
		
		[AttributeLogicalName("modifiedonbehalfby")]
		public EntityReference ModifiedOnBehalfBy
		{
			get => TryGetAttributeValue("modifiedonbehalfby", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: modifiedonbehalfby</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//ModifiedOnBehalfByName
		
		[AttributeLogicalName("modifiedonbehalfbyname")]
		public string ModifiedOnBehalfByName
		{
			get => FormattedValues.Contains("modifiedonbehalfby") ? FormattedValues["modifiedonbehalfby"] : null;
		
		}
		/// <summary>
		/// Attribute of: modifiedonbehalfby</br>
		/// Max Length: 100</br>
		/// Required Level: SystemRequired</br>
		/// Valid for: Read</br>
		/// </summary>
		//ModifiedOnBehalfByYomiName
		
		[AttributeLogicalName("modifiedonbehalfbyyominame")]
		public string ModifiedOnBehalfByYomiName
		{
			get => FormattedValues.Contains("modifiedonbehalfby") ? FormattedValues["modifiedonbehalfby"] : null;
		
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//OnHoldTime
		
		[AttributeLogicalName("onholdtime")]
		public int? OnHoldTime
		{
			get => TryGetAttributeValue("onholdtime", out int value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Read</br>
		/// </summary>
		//OverriddenCreatedOn
		
		[AttributeLogicalName("overriddencreatedon")]
		public DateTime? OverriddenCreatedOn
		{
			get => TryGetAttributeValue("overriddencreatedon", out DateTime value) ? value : null;
		}
		/// <summary>
		/// Required Level: SystemRequired</br>
		/// Valid for: Create Update Read</br>
		/// Targets: systemuser,team</br>
		/// </summary>
		//OwnerId
		
		[AttributeLogicalName("ownerid")]
		public EntityReference OwnerId
		{
			get => TryGetAttributeValue("ownerid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: ownerid</br>
		/// Max Length: 100</br>
		/// Required Level: SystemRequired</br>
		/// Valid for: Read</br>
		/// </summary>
		//OwnerIdName
		
		[AttributeLogicalName("owneridname")]
		public string OwnerIdName
		{
			get => TryGetAttributeValue("owneridname", out string value) ? value : null;
		}
		/// <summary>
		/// Attribute of: ownerid</br>
		/// Max Length: 100</br>
		/// Required Level: SystemRequired</br>
		/// Valid for: Read</br>
		/// </summary>
		//OwnerIdYomiName
		
		[AttributeLogicalName("owneridyominame")]
		public string OwnerIdYomiName
		{
			get => TryGetAttributeValue("owneridyominame", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// Targets: businessunit</br>
		/// </summary>
		//OwningBusinessUnit
		
		[AttributeLogicalName("owningbusinessunit")]
		public EntityReference OwningBusinessUnit
		{
			get => TryGetAttributeValue("owningbusinessunit", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: owningbusinessunit</br>
		/// Max Length: 160</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//OwningBusinessUnitName
		
		[AttributeLogicalName("owningbusinessunitname")]
		public string OwningBusinessUnitName
		{
			get => FormattedValues.Contains("owningbusinessunit") ? FormattedValues["owningbusinessunit"] : null;
		
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// Targets: team</br>
		/// </summary>
		//OwningTeam
		
		[AttributeLogicalName("owningteam")]
		public EntityReference OwningTeam
		{
			get => TryGetAttributeValue("owningteam", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// Targets: systemuser</br>
		/// </summary>
		//OwningUser
		
		[AttributeLogicalName("owninguser")]
		public EntityReference OwningUser
		{
			get => TryGetAttributeValue("owninguser", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Targets: sla</br>
		/// </summary>
		//SLAId
		
		[AttributeLogicalName("slaid")]
		public EntityReference SLAId
		{
			get => TryGetAttributeValue("slaid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// Targets: sla</br>
		/// </summary>
		//SLAInvokedId
		
		[AttributeLogicalName("slainvokedid")]
		public EntityReference SLAInvokedId
		{
			get => TryGetAttributeValue("slainvokedid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: slainvokedid</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//SLAInvokedIdName
		
		[AttributeLogicalName("slainvokedidname")]
		public string SLAInvokedIdName
		{
			get => FormattedValues.Contains("slainvokedid") ? FormattedValues["slainvokedid"] : null;
		
		}
		/// <summary>
		/// Attribute of: slaid</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//SLAName
		
		[AttributeLogicalName("slaname")]
		public string SLAName
		{
			get => FormattedValues.Contains("slaid") ? FormattedValues["slaid"] : null;
		
		}
		/// <summary>
		/// Required Level: SystemRequired</br>
		/// Valid for: Update Read</br>
		/// </summary>
		//Statecode
		
		[AttributeLogicalName("statecode")]
		public Request.Choices.Status? Statecode
		{
			get => TryGetAttributeValue("statecode", out OptionSetValue opt) && opt != null ? (Request.Choices.Status)Enum.ToObject(typeof(Request.Choices.Status), opt.Value) : null;
		}
		/// <summary>
		/// Attribute of: statecode</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//StatecodeName
		
		[AttributeLogicalName("statecodename")]
		public string StatecodeName
		{
			get => FormattedValues.Contains("statecode") ? FormattedValues["statecodename"] : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Statuscode
		
		[AttributeLogicalName("statuscode")]
		public Request.Choices.StatusReason? Statuscode
		{
			get => TryGetAttributeValue("statuscode", out OptionSetValue opt) && opt != null ? (Request.Choices.StatusReason)Enum.ToObject(typeof(Request.Choices.StatusReason), opt.Value) : null;
		}
		/// <summary>
		/// Attribute of: statuscode</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//StatuscodeName
		
		[AttributeLogicalName("statuscodename")]
		public string StatuscodeName
		{
			get => FormattedValues.Contains("statuscode") ? FormattedValues["statuscodename"] : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//TimeZoneRuleVersionNumber
		
		[AttributeLogicalName("timezoneruleversionnumber")]
		public int? TimeZoneRuleVersionNumber
		{
			get => TryGetAttributeValue("timezoneruleversionnumber", out int value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//UTCConversionTimeZoneCode
		
		[AttributeLogicalName("utcconversiontimezonecode")]
		public int? UTCConversionTimeZoneCode
		{
			get => TryGetAttributeValue("utcconversiontimezonecode", out int value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//VersionNumber
		
		[AttributeLogicalName("versionnumber")]
		public long? VersionNumber
		{
			get => TryGetAttributeValue("versionnumber", out long value) ? value : null;
		}
	}
	[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
	[EntityLogicalName("beone_request")]
	public class PreImageRequest : Entity
	{
		public const string EntityLogicalName = "beone_request";
		public partial class Fields
		{
			public const string Claimhistoryid = "beone_claimhistoryid";
			public const string Closesla = "beone_closesla";
			public const string Contractnumber = "beone_contractnumber";
			public const string Duedate = "beone_duedate";
			public const string Entityid = "beone_entityid";
			public const string Importance = "beone_importance";
			public const string Name = "beone_name";
			public const string Newslainstanceid = "beone_newslainstanceid";
			public const string Origin = "beone_origin";
			public const string Pruningdecision = "beone_pruningdecision";
			public const string Pruningid = "beone_pruningid";
			public const string Sladuration = "beone_sladuration";
			public const string Slainstanceid = "beone_slainstanceid";
			public const string Slakpiinstancestatus = "beone_slakpiinstancestatus";
			public const string OnHoldTime = "onholdtime";
			public const string OwnerId = "ownerid";
			public const string Statecode = "statecode";
			public const string Statuscode = "statuscode";
		}
	
		/// <summary>
		/// Max Length: 200
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_actgmotivelabel")]
		public string Actgmotivelabel
		{
			get => TryGetAttributeValue("beone_actgmotivelabel", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_actiontypeid")]
		public EntityReference Actiontypeid
		{
			get => TryGetAttributeValue("beone_actiontypeid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_actiontypeid
		/// Max Length: 600
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_actiontypeidname")]
		public string ActiontypeidName
		{
			get => TryGetAttributeValue("beone_actiontypeidname", out string value) ? value : null;
		}
		/// <summary>
		/// Max Length: 100
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_activity")]
		public string Activity
		{
			get => TryGetAttributeValue("beone_activity", out string value) ? value : null;
		}
		/// <summary>
		/// Max Length: 100
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_adagionumber")]
		public string Adagionumber
		{
			get => TryGetAttributeValue("beone_adagionumber", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_agolstatus")]
		public Request.Choices.AgolStatus? Agolstatus
		{
			get => TryGetAttributeValue("beone_agolstatus", out OptionSetValue opt) && opt == null ? null : (Request.Choices.AgolStatus)opt.Value;
		}
		/// <summary>
		/// Attribute of: beone_agolstatus
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_agolstatusname")]
		public string AgolstatusName
		{
			get => FormattedValues.Contains("beone_agolstatusname") ? FormattedValues["beone_agolstatusname"] : null;
		}
	
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_annulationdate")]
		public DateTime? Annulationdate
		{
			get => TryGetAttributeValue("beone_annulationdate", out DateTime value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_awmessagetype")]
		public Request.Choices.AwMessageType? Awmessagetype
		{
			get => TryGetAttributeValue("beone_awmessagetype", out OptionSetValue opt) && opt == null ? null : (Request.Choices.AwMessageType)opt.Value;
		}
		/// <summary>
		/// Attribute of: beone_awmessagetype
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_awmessagetypename")]
		public string AwmessagetypeName
		{
			get => FormattedValues.Contains("beone_awmessagetypename") ? FormattedValues["beone_awmessagetypename"] : null;
		}
	
		/// <summary>
		/// Max Length: 100
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_awproducts")]
		public string Awproducts
		{
			get => TryGetAttributeValue("beone_awproducts", out string value) ? value : null;
		}
		/// <summary>
		/// Max Length: 100
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_bcenumber")]
		public string BCENumber
		{
			get => TryGetAttributeValue("beone_bcenumber", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_brokerfeedback")]
		public Request.Choices.BrokerFeedback? Brokerfeedback
		{
			get => TryGetAttributeValue("beone_brokerfeedback", out OptionSetValue opt) && opt == null ? null : (Request.Choices.BrokerFeedback)opt.Value;
		}
		/// <summary>
		/// Attribute of: beone_brokerfeedback
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_brokerfeedbackname")]
		public string BrokerfeedbackName
		{
			get => FormattedValues.Contains("beone_brokerfeedbackname") ? FormattedValues["beone_brokerfeedbackname"] : null;
		}
	
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_claimhistoryid")]
		public EntityReference Claimhistoryid
		{
			get => TryGetAttributeValue("beone_claimhistoryid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_claimhistoryid
		/// Max Length: 100
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_claimhistoryidname")]
		public string ClaimhistoryidName
		{
			get => TryGetAttributeValue("beone_claimhistoryidname", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_closesla")]
		public bool? Closesla
		{
			get => TryGetAttributeValue("beone_closesla", out bool value) ? value : null;
		}
		/// <summary>
		/// Attribute of: beone_closesla
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_closeslaname")]
		public string CloseslaName
		{
			get => FormattedValues.Contains("beone_closeslaname") ? FormattedValues["beone_closeslaname"] : null;
		}
	
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_config")]
		public EntityReference Config
		{
			get => TryGetAttributeValue("beone_config", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_config
		/// Max Length: 100
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_configname")]
		public string ConfigName
		{
			get => TryGetAttributeValue("beone_configname", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_connexity")]
		public Request.Choices.FlowConnexity? Connexity
		{
			get => TryGetAttributeValue("beone_connexity", out OptionSetValue opt) && opt == null ? null : (Request.Choices.FlowConnexity)opt.Value;
		}
		/// <summary>
		/// Attribute of: beone_connexity
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_connexityname")]
		public string ConnexityName
		{
			get => FormattedValues.Contains("beone_connexityname") ? FormattedValues["beone_connexityname"] : null;
		}
	
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_contractid")]
		public EntityReference Contractid
		{
			get => TryGetAttributeValue("beone_contractid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_contractid
		/// Max Length: 100
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_contractidname")]
		public string ContractidName
		{
			get => TryGetAttributeValue("beone_contractidname", out string value) ? value : null;
		}
		/// <summary>
		/// Max Length: 500
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_contractlist")]
		public string Contractlist
		{
			get => TryGetAttributeValue("beone_contractlist", out string value) ? value : null;
		}
		/// <summary>
		/// Max Length: 100
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_contractnumber")]
		public string Contractnumber
		{
			get => TryGetAttributeValue("beone_contractnumber", out string value) ? value : null;
		}
		/// <summary>
		/// Max Length: 100
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_customername")]
		public string CustomerName
		{
			get => TryGetAttributeValue("beone_customername", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: ApplicationRequired
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_datein")]
		public DateTime? Datein
		{
			get => TryGetAttributeValue("beone_datein", out DateTime value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_declarationnumber1")]
		public int? Declarationnumber1
		{
			get => TryGetAttributeValue("beone_declarationnumber1", out int value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_declarationnumber2")]
		public int? Declarationnumber2
		{
			get => TryGetAttributeValue("beone_declarationnumber2", out int value) ? value : null;
		}
		/// <summary>
		/// Max Length: 100
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_declarationnumbers")]
		public string Declarationnumbers
		{
			get => TryGetAttributeValue("beone_declarationnumbers", out string value) ? value : null;
		}
		/// <summary>
		/// Max Length: 2000
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_description")]
		public string Description
		{
			get => TryGetAttributeValue("beone_description", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_domain")]
		public Request.Choices.FlowTaskDomain? Domain
		{
			get => TryGetAttributeValue("beone_domain", out OptionSetValue opt) && opt == null ? null : (Request.Choices.FlowTaskDomain)opt.Value;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_domainaccident_amount")]
		public int? Domainaccident_amount
		{
			get => TryGetAttributeValue("beone_domainaccident_amount", out int value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_domaincar_amount")]
		public int? Domaincar_amount
		{
			get => TryGetAttributeValue("beone_domaincar_amount", out int value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_domainfire_amount")]
		public int? Domainfire_amount
		{
			get => TryGetAttributeValue("beone_domainfire_amount", out int value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_domainfireengineering_amount")]
		public int? Domainfireengineering_amount
		{
			get => TryGetAttributeValue("beone_domainfireengineering_amount", out int value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_domainlaw_amount")]
		public int? Domainlaw_amount
		{
			get => TryGetAttributeValue("beone_domainlaw_amount", out int value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_domainliability_amount")]
		public int? Domainliability_amount
		{
			get => TryGetAttributeValue("beone_domainliability_amount", out int value) ? value : null;
		}
		/// <summary>
		/// Attribute of: beone_domain
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_domainname")]
		public string DomainName
		{
			get => FormattedValues.Contains("beone_domainname") ? FormattedValues["beone_domainname"] : null;
		}
	
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_dossierid")]
		public EntityReference Dossierid
		{
			get => TryGetAttributeValue("beone_dossierid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_dossierid
		/// Max Length: 100
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_dossieridname")]
		public string DossieridName
		{
			get => TryGetAttributeValue("beone_dossieridname", out string value) ? value : null;
		}
		/// <summary>
		/// Max Length: 100
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_dossiernumber")]
		public string Dossiernumber
		{
			get => TryGetAttributeValue("beone_dossiernumber", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_duedate")]
		public DateTime? Duedate
		{
			get => TryGetAttributeValue("beone_duedate", out DateTime value) ? value : null;
		}
		/// <summary>
		/// Required Level: ApplicationRequired
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_entityid")]
		public EntityReference Entityid
		{
			get => TryGetAttributeValue("beone_entityid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_entityid
		/// Max Length: 100
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_entityidname")]
		public string EntityidName
		{
			get => TryGetAttributeValue("beone_entityidname", out string value) ? value : null;
		}
		/// <summary>
		/// Attribute of: beone_entityid
		/// Max Length: 100
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_entityidyominame")]
		public string EntityidYomiName
		{
			get => TryGetAttributeValue("beone_entityidyominame", out string value) ? value : null;
		}
		/// <summary>
		/// Max Length: 100
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_externalofferref")]
		public string Externalofferref
		{
			get => TryGetAttributeValue("beone_externalofferref", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_externalrework")]
		public bool? Externalrework
		{
			get => TryGetAttributeValue("beone_externalrework", out bool value) ? value : null;
		}
		/// <summary>
		/// Attribute of: beone_externalrework
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_externalreworkname")]
		public string ExternalreworkName
		{
			get => FormattedValues.Contains("beone_externalreworkname") ? FormattedValues["beone_externalreworkname"] : null;
		}
	
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_firsttodounderwriterid")]
		public EntityReference Firsttodounderwriterid
		{
			get => TryGetAttributeValue("beone_firsttodounderwriterid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_firsttodounderwriterid
		/// Max Length: 100
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_firsttodounderwriteridname")]
		public string FirsttodounderwriteridName
		{
			get => TryGetAttributeValue("beone_firsttodounderwriteridname", out string value) ? value : null;
		}
		/// <summary>
		/// Max Length: 100
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_fleetnumber")]
		public string Fleetnumber
		{
			get => TryGetAttributeValue("beone_fleetnumber", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_forcedduedate")]
		public DateTime? Forcedduedate
		{
			get => TryGetAttributeValue("beone_forcedduedate", out DateTime value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_groupofferid")]
		public EntityReference Groupofferid
		{
			get => TryGetAttributeValue("beone_groupofferid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_groupofferid
		/// Max Length: 100
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_groupofferidname")]
		public string GroupofferidName
		{
			get => TryGetAttributeValue("beone_groupofferidname", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: ApplicationRequired
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_importance")]
		public Request.Choices.RequestImportance? Importance
		{
			get => TryGetAttributeValue("beone_importance", out OptionSetValue opt) && opt == null ? null : (Request.Choices.RequestImportance)opt.Value;
		}
		/// <summary>
		/// Attribute of: beone_importance
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_importancename")]
		public string ImportanceName
		{
			get => FormattedValues.Contains("beone_importancename") ? FormattedValues["beone_importancename"] : null;
		}
	
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_internalexternal")]
		public Request.Choices.InternalExternal? Internalexternal
		{
			get => TryGetAttributeValue("beone_internalexternal", out OptionSetValue opt) && opt == null ? null : (Request.Choices.InternalExternal)opt.Value;
		}
		/// <summary>
		/// Attribute of: beone_internalexternal
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_internalexternalname")]
		public string InternalexternalName
		{
			get => FormattedValues.Contains("beone_internalexternalname") ? FormattedValues["beone_internalexternalname"] : null;
		}
	
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_internalrework")]
		public bool? Internalrework
		{
			get => TryGetAttributeValue("beone_internalrework", out bool value) ? value : null;
		}
		/// <summary>
		/// Attribute of: beone_internalrework
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_internalreworkname")]
		public string InternalreworkName
		{
			get => FormattedValues.Contains("beone_internalreworkname") ? FormattedValues["beone_internalreworkname"] : null;
		}
	
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_isautoclose")]
		public bool? Isautoclose
		{
			get => TryGetAttributeValue("beone_isautoclose", out bool value) ? value : null;
		}
		/// <summary>
		/// Attribute of: beone_isautoclose
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_isautoclosename")]
		public string IsautocloseName
		{
			get => FormattedValues.Contains("beone_isautoclosename") ? FormattedValues["beone_isautoclosename"] : null;
		}
	
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_isurgent")]
		public bool? IsUrgent
		{
			get => TryGetAttributeValue("beone_isurgent", out bool value) ? value : null;
		}
		/// <summary>
		/// Attribute of: beone_isurgent
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_isurgentname")]
		public string IsurgentName
		{
			get => FormattedValues.Contains("beone_isurgentname") ? FormattedValues["beone_isurgentname"] : null;
		}
	
		/// <summary>
		/// Max Length: 10000
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_json")]
		public string Json
		{
			get => TryGetAttributeValue("beone_json", out string value) ? value : null;
		}
		/// <summary>
		/// Max Length: 100
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_licenseplate")]
		public string Licenseplate
		{
			get => TryGetAttributeValue("beone_licenseplate", out string value) ? value : null;
		}
		/// <summary>
		/// Max Length: 100
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_mailboxid")]
		public string Mailboxid
		{
			get => TryGetAttributeValue("beone_mailboxid", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_maindomain")]
		public Request.Choices.MainDomain? Maindomain
		{
			get => TryGetAttributeValue("beone_maindomain", out OptionSetValue opt) && opt == null ? null : (Request.Choices.MainDomain)opt.Value;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_maindomaingroup")]
		public Request.Choices.MainDomainGroup? Maindomaingroup
		{
			get => TryGetAttributeValue("beone_maindomaingroup", out OptionSetValue opt) && opt == null ? null : (Request.Choices.MainDomainGroup)opt.Value;
		}
		/// <summary>
		/// Attribute of: beone_maindomaingroup
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_maindomaingroupname")]
		public string MaindomaingroupName
		{
			get => FormattedValues.Contains("beone_maindomaingroupname") ? FormattedValues["beone_maindomaingroupname"] : null;
		}
	
		/// <summary>
		/// Attribute of: beone_maindomain
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_maindomainname")]
		public string MaindomainName
		{
			get => FormattedValues.Contains("beone_maindomainname") ? FormattedValues["beone_maindomainname"] : null;
		}
	
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_messageid")]
		public int? Messageid
		{
			get => TryGetAttributeValue("beone_messageid", out int value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_modifiedonbehalfby")]
		public EntityReference Modifiedonbehalfby
		{
			get => TryGetAttributeValue("beone_modifiedonbehalfby", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_modifiedonbehalfby
		/// Max Length: 100
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_modifiedonbehalfbyname")]
		public string ModifiedonbehalfbyName
		{
			get => TryGetAttributeValue("beone_modifiedonbehalfbyname", out string value) ? value : null;
		}
		/// <summary>
		/// Attribute of: beone_modifiedonbehalfby
		/// Max Length: 100
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_modifiedonbehalfbyyominame")]
		public string ModifiedonbehalfbyYomiName
		{
			get => TryGetAttributeValue("beone_modifiedonbehalfbyyominame", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_motive")]
		public Request.Choices.Motive? Motive
		{
			get => TryGetAttributeValue("beone_motive", out OptionSetValue opt) && opt == null ? null : (Request.Choices.Motive)opt.Value;
		}
		/// <summary>
		/// Attribute of: beone_motive
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_motivename")]
		public string MotiveName
		{
			get => FormattedValues.Contains("beone_motivename") ? FormattedValues["beone_motivename"] : null;
		}
	
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_multiregardingid")]
		public EntityReference Multiregardingid
		{
			get => TryGetAttributeValue("beone_multiregardingid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_multiregardingid
		/// Max Length: 100
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_multiregardingidname")]
		public string MultiregardingidName
		{
			get => TryGetAttributeValue("beone_multiregardingidname", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_naceid")]
		public EntityReference Naceid
		{
			get => TryGetAttributeValue("beone_naceid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_naceid
		/// Max Length: 100
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_naceidname")]
		public string NaceidName
		{
			get => TryGetAttributeValue("beone_naceidname", out string value) ? value : null;
		}
		/// <summary>
		/// Max Length: 100
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_name")]
		public string Name
		{
			get => TryGetAttributeValue("beone_name", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_ncmpproid")]
		public EntityReference Ncmpproid
		{
			get => TryGetAttributeValue("beone_ncmpproid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_ncmpproid
		/// Max Length: 100
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_ncmpproidname")]
		public string NcmpproidName
		{
			get => TryGetAttributeValue("beone_ncmpproidname", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_newslainstanceid")]
		public EntityReference Newslainstanceid
		{
			get => TryGetAttributeValue("beone_newslainstanceid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_newslainstanceid
		/// Max Length: 100
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_newslainstanceidname")]
		public string NewslainstanceidName
		{
			get => TryGetAttributeValue("beone_newslainstanceidname", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_offerpriority")]
		public Request.Choices.FlowPriority? Offerpriority
		{
			get => TryGetAttributeValue("beone_offerpriority", out OptionSetValue opt) && opt == null ? null : (Request.Choices.FlowPriority)opt.Value;
		}
		/// <summary>
		/// Attribute of: beone_offerpriority
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_offerpriorityname")]
		public string OfferpriorityName
		{
			get => FormattedValues.Contains("beone_offerpriorityname") ? FormattedValues["beone_offerpriorityname"] : null;
		}
	
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_onholdduration")]
		public decimal? Onholdduration
		{
			get => TryGetAttributeValue("beone_onholdduration", out decimal value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_onholdreason")]
		public Request.Choices.OnHoldReason? Onholdreason
		{
			get => TryGetAttributeValue("beone_onholdreason", out OptionSetValue opt) && opt == null ? null : (Request.Choices.OnHoldReason)opt.Value;
		}
		/// <summary>
		/// Attribute of: beone_onholdreason
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_onholdreasonname")]
		public string OnholdreasonName
		{
			get => FormattedValues.Contains("beone_onholdreasonname") ? FormattedValues["beone_onholdreasonname"] : null;
		}
	
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_origin")]
		public Request.Choices.RequestOrigin? Origin
		{
			get => TryGetAttributeValue("beone_origin", out OptionSetValue opt) && opt == null ? null : (Request.Choices.RequestOrigin)opt.Value;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_originalawmessageid")]
		public EntityReference Originalawmessageid
		{
			get => TryGetAttributeValue("beone_originalawmessageid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_originalawmessageid
		/// Max Length: 100
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_originalawmessageidname")]
		public string OriginalawmessageidName
		{
			get => TryGetAttributeValue("beone_originalawmessageidname", out string value) ? value : null;
		}
		/// <summary>
		/// Max Length: 200
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_originaldocument")]
		public string Originaldocument
		{
			get => TryGetAttributeValue("beone_originaldocument", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_originalemailid")]
		public EntityReference OriginalEmailId
		{
			get => TryGetAttributeValue("beone_originalemailid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_originalemailid
		/// Max Length: 800
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_originalemailidname")]
		public string OriginalEmailIdName
		{
			get => TryGetAttributeValue("beone_originalemailidname", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_originalletterid")]
		public EntityReference Originalletterid
		{
			get => TryGetAttributeValue("beone_originalletterid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_originalletterid
		/// Max Length: 100
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_originalletteridname")]
		public string OriginalletteridName
		{
			get => TryGetAttributeValue("beone_originalletteridname", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_originalmpbid")]
		public EntityReference Originalmpbid
		{
			get => TryGetAttributeValue("beone_originalmpbid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_originalmpbid
		/// Max Length: 100
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_originalmpbidname")]
		public string OriginalmpbidName
		{
			get => TryGetAttributeValue("beone_originalmpbidname", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_originalphonecallid")]
		public EntityReference Originalphonecallid
		{
			get => TryGetAttributeValue("beone_originalphonecallid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_originalphonecallid
		/// Max Length: 100
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_originalphonecallidname")]
		public string OriginalphonecallidName
		{
			get => TryGetAttributeValue("beone_originalphonecallidname", out string value) ? value : null;
		}
		/// <summary>
		/// Max Length: 200
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_originalsenderemailaddress")]
		public string Originalsenderemailaddress
		{
			get => TryGetAttributeValue("beone_originalsenderemailaddress", out string value) ? value : null;
		}
		/// <summary>
		/// Attribute of: beone_origin
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_originname")]
		public string OriginName
		{
			get => FormattedValues.Contains("beone_originname") ? FormattedValues["beone_originname"] : null;
		}
	
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_processexpw")]
		public int? Processexpw
		{
			get => TryGetAttributeValue("beone_processexpw", out int value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_productid")]
		public EntityReference Productid
		{
			get => TryGetAttributeValue("beone_productid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_productid
		/// Max Length: 100
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_productidname")]
		public string ProductidName
		{
			get => TryGetAttributeValue("beone_productidname", out string value) ? value : null;
		}
		/// <summary>
		/// Max Length: 700
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_productids")]
		public string Productids
		{
			get => TryGetAttributeValue("beone_productids", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_pruningdecision")]
		public Request.Choices.PruningDecision? Pruningdecision
		{
			get => TryGetAttributeValue("beone_pruningdecision", out OptionSetValue opt) && opt == null ? null : (Request.Choices.PruningDecision)opt.Value;
		}
		/// <summary>
		/// Attribute of: beone_pruningdecision
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_pruningdecisionname")]
		public string PruningdecisionName
		{
			get => FormattedValues.Contains("beone_pruningdecisionname") ? FormattedValues["beone_pruningdecisionname"] : null;
		}
	
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_pruningid")]
		public EntityReference Pruningid
		{
			get => TryGetAttributeValue("beone_pruningid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_pruningid
		/// Max Length: 100
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_pruningidname")]
		public string PruningidName
		{
			get => TryGetAttributeValue("beone_pruningidname", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: SystemRequired
		/// Valid for: Create Read
		/// </summary>
		
		
		/// <summary>
		/// Required Level: ApplicationRequired
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_requesttypeid")]
		public EntityReference Requesttypeid
		{
			get => TryGetAttributeValue("beone_requesttypeid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_requesttypeid
		/// Max Length: 500
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_requesttypeidname")]
		public string RequesttypeidName
		{
			get => TryGetAttributeValue("beone_requesttypeidname", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_signature")]
		public Request.Choices.Boolean? Signature
		{
			get => TryGetAttributeValue("beone_signature", out OptionSetValue opt) && opt == null ? null : (Request.Choices.Boolean)opt.Value;
		}
		/// <summary>
		/// Attribute of: beone_signature
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_signaturename")]
		public string SignatureName
		{
			get => FormattedValues.Contains("beone_signaturename") ? FormattedValues["beone_signaturename"] : null;
		}
	
		/// <summary>
		/// Required Level: ApplicationRequired
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_sladuration")]
		public Request.Choices.SlaDuration? Sladuration
		{
			get => TryGetAttributeValue("beone_sladuration", out OptionSetValue opt) && opt == null ? null : (Request.Choices.SlaDuration)opt.Value;
		}
		/// <summary>
		/// Attribute of: beone_sladuration
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_sladurationname")]
		public string SladurationName
		{
			get => FormattedValues.Contains("beone_sladurationname") ? FormattedValues["beone_sladurationname"] : null;
		}
	
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_slainstanceid")]
		public EntityReference Slainstanceid
		{
			get => TryGetAttributeValue("beone_slainstanceid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_slainstanceid
		/// Max Length: 100
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_slainstanceidname")]
		public string SlainstanceidName
		{
			get => TryGetAttributeValue("beone_slainstanceidname", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_slakpiinstancestatus")]
		public Request.Choices.SlaKpiInstanceStatus? Slakpiinstancestatus
		{
			get => TryGetAttributeValue("beone_slakpiinstancestatus", out OptionSetValue opt) && opt == null ? null : (Request.Choices.SlaKpiInstanceStatus)opt.Value;
		}
		/// <summary>
		/// Attribute of: beone_slakpiinstancestatus
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_slakpiinstancestatusname")]
		public string SlakpiinstancestatusName
		{
			get => FormattedValues.Contains("beone_slakpiinstancestatusname") ? FormattedValues["beone_slakpiinstancestatusname"] : null;
		}
	
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_slasuccessfailuredate")]
		public DateTime? Slasuccessfailuredate
		{
			get => TryGetAttributeValue("beone_slasuccessfailuredate", out DateTime value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_specificdate")]
		public DateTime? Specificdate
		{
			get => TryGetAttributeValue("beone_specificdate", out DateTime value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_totalproducts")]
		public decimal? Totalproducts
		{
			get => TryGetAttributeValue("beone_totalproducts", out decimal value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("beone_verifybroker")]
		public bool? VerifyBroker
		{
			get => TryGetAttributeValue("beone_verifybroker", out bool value) ? value : null;
		}
		/// <summary>
		/// Attribute of: beone_verifybroker
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("beone_verifybrokername")]
		public string VerifybrokerName
		{
			get => FormattedValues.Contains("beone_verifybrokername") ? FormattedValues["beone_verifybrokername"] : null;
		}
	
		/// <summary>
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("createdby")]
		public EntityReference CreatedBy
		{
			get => TryGetAttributeValue("createdby", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: createdby
		/// Max Length: 100
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("createdbyname")]
		public string CreatedByName
		{
			get => TryGetAttributeValue("createdbyname", out string value) ? value : null;
		}
		/// <summary>
		/// Attribute of: createdby
		/// Max Length: 100
		/// Required Level: SystemRequired
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("createdbyyominame")]
		public string CreatedByYomiName
		{
			get => TryGetAttributeValue("createdbyyominame", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("createdon")]
		public DateTime? CreatedOn
		{
			get => TryGetAttributeValue("createdon", out DateTime value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("createdonbehalfby")]
		public EntityReference CreatedOnBehalfBy
		{
			get => TryGetAttributeValue("createdonbehalfby", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: createdonbehalfby
		/// Max Length: 100
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("createdonbehalfbyname")]
		public string CreatedOnBehalfByName
		{
			get => TryGetAttributeValue("createdonbehalfbyname", out string value) ? value : null;
		}
		/// <summary>
		/// Attribute of: createdonbehalfby
		/// Max Length: 100
		/// Required Level: SystemRequired
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("createdonbehalfbyyominame")]
		public string CreatedOnBehalfByYomiName
		{
			get => TryGetAttributeValue("createdonbehalfbyyominame", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Read
		/// </summary>
		
		[AttributeLogicalName("importsequencenumber")]
		public int? ImportSequenceNumber
		{
			get => TryGetAttributeValue("importsequencenumber", out int value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("lastonholdtime")]
		public DateTime? LastOnHoldTime
		{
			get => TryGetAttributeValue("lastonholdtime", out DateTime value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("modifiedby")]
		public EntityReference ModifiedBy
		{
			get => TryGetAttributeValue("modifiedby", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: modifiedby
		/// Max Length: 100
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("modifiedbyname")]
		public string ModifiedByName
		{
			get => TryGetAttributeValue("modifiedbyname", out string value) ? value : null;
		}
		/// <summary>
		/// Attribute of: modifiedby
		/// Max Length: 100
		/// Required Level: SystemRequired
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("modifiedbyyominame")]
		public string ModifiedByYomiName
		{
			get => TryGetAttributeValue("modifiedbyyominame", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("modifiedon")]
		public DateTime? ModifiedOn
		{
			get => TryGetAttributeValue("modifiedon", out DateTime value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("modifiedonbehalfby")]
		public EntityReference ModifiedOnBehalfBy
		{
			get => TryGetAttributeValue("modifiedonbehalfby", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: modifiedonbehalfby
		/// Max Length: 100
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("modifiedonbehalfbyname")]
		public string ModifiedOnBehalfByName
		{
			get => TryGetAttributeValue("modifiedonbehalfbyname", out string value) ? value : null;
		}
		/// <summary>
		/// Attribute of: modifiedonbehalfby
		/// Max Length: 100
		/// Required Level: SystemRequired
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("modifiedonbehalfbyyominame")]
		public string ModifiedOnBehalfByYomiName
		{
			get => TryGetAttributeValue("modifiedonbehalfbyyominame", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("onholdtime")]
		public int? OnHoldTime
		{
			get => TryGetAttributeValue("onholdtime", out int value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Read
		/// </summary>
		
		[AttributeLogicalName("overriddencreatedon")]
		public DateTime? OverriddenCreatedOn
		{
			get => TryGetAttributeValue("overriddencreatedon", out DateTime value) ? value : null;
		}
		/// <summary>
		/// Required Level: SystemRequired
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("ownerid")]
		public EntityReference OwnerId
		{
			get => TryGetAttributeValue("ownerid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: ownerid
		/// Max Length: 100
		/// Required Level: SystemRequired
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("owneridname")]
		public string OwnerIdName
		{
			get => TryGetAttributeValue("owneridname", out string value) ? value : null;
		}
		/// <summary>
		/// Attribute of: ownerid
		/// Max Length: 100
		/// Required Level: SystemRequired
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("owneridyominame")]
		public string OwnerIdYomiName
		{
			get => TryGetAttributeValue("owneridyominame", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("owningbusinessunit")]
		public EntityReference OwningBusinessUnit
		{
			get => TryGetAttributeValue("owningbusinessunit", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: owningbusinessunit
		/// Max Length: 160
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("owningbusinessunitname")]
		public string OwningBusinessUnitName
		{
			get => TryGetAttributeValue("owningbusinessunitname", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("owningteam")]
		public EntityReference OwningTeam
		{
			get => TryGetAttributeValue("owningteam", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("owninguser")]
		public EntityReference OwningUser
		{
			get => TryGetAttributeValue("owninguser", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("slaid")]
		public EntityReference SLAId
		{
			get => TryGetAttributeValue("slaid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("slainvokedid")]
		public EntityReference SLAInvokedId
		{
			get => TryGetAttributeValue("slainvokedid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: slainvokedid
		/// Max Length: 100
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("slainvokedidname")]
		public string SLAInvokedIdName
		{
			get => TryGetAttributeValue("slainvokedidname", out string value) ? value : null;
		}
		/// <summary>
		/// Attribute of: slaid
		/// Max Length: 100
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("slaname")]
		public string SLAName
		{
			get => TryGetAttributeValue("slaname", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: SystemRequired
		/// Valid for: Update Read
		/// </summary>
		
		[AttributeLogicalName("statecode")]
		public Request.Choices.Status? Statecode
		{
			get => TryGetAttributeValue("statecode", out OptionSetValue opt) && opt != null ? (Request.Choices.Status)Enum.ToObject(typeof(Request.Choices.Status), opt.Value) : null;
		}
		/// <summary>
		/// Attribute of: statecode
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("statecodename")]
		public string StatecodeName
		{
			get => FormattedValues.Contains("statecodename") ? FormattedValues["statecodename"] : null;
		}
	
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("statuscode")]
		public Request.Choices.StatusReason? Statuscode
		{
			get => TryGetAttributeValue("statuscode", out OptionSetValue opt) && opt != null ? (Request.Choices.StatusReason)Enum.ToObject(typeof(Request.Choices.StatusReason), opt.Value) : null;
		}
		/// <summary>
		/// Attribute of: statuscode
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("statuscodename")]
		public string StatuscodeName
		{
			get => FormattedValues.Contains("statuscodename") ? FormattedValues["statuscodename"] : null;
		}
	
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("timezoneruleversionnumber")]
		public int? TimeZoneRuleVersionNumber
		{
			get => TryGetAttributeValue("timezoneruleversionnumber", out int value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		
		[AttributeLogicalName("utcconversiontimezonecode")]
		public int? UTCConversionTimeZoneCode
		{
			get => TryGetAttributeValue("utcconversiontimezonecode", out int value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		
		[AttributeLogicalName("versionnumber")]
		public long? VersionNumber
		{
			get => TryGetAttributeValue("versionnumber", out long value) ? value : null;
		}
	}


}
public partial class PostUpdateManager : UpdatePluginManager
{
	[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")] public PostUpdateManager() : base("beone_request", EStage.PostOperation) { }

	new PostUpdate.TargetRequest Target => Target.ToEntity<PostUpdate.TargetRequest>();
	new PostUpdate.PreImageRequest PreImage => PreImage.ToEntity<PostUpdate.PreImageRequest>();
}
[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
public partial class PreCreate : PluginDefinition<PreCreateManager>, IPlugin
{
	[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
	[EntityLogicalName("beone_request")]
	public class TargetRequest : Entity
	{
		public const string EntityLogicalName = "beone_request";
		public const string EntityLogicalCollectionName = "beone_requests";
		public const string EntitySetName = "beone_requests";
		public const int EntityTypeCode = 10094;
		public const string PrimaryNameAttribute = "";
		public const string PrimaryIdAttribute = "beone_requestid";
	
		public partial class Fields
		{
			public const string Actgmotivelabel = "beone_actgmotivelabel";
			public const string Actiontypeid = "beone_actiontypeid";
			public const string ActiontypeidName = "beone_actiontypeidname";
			public const string Activity = "beone_activity";
			public const string Adagionumber = "beone_adagionumber";
			public const string Agolstatus = "beone_agolstatus";
			public const string AgolstatusName = "beone_agolstatusname";
			public const string Annulationdate = "beone_annulationdate";
			public const string Awmessagetype = "beone_awmessagetype";
			public const string AwmessagetypeName = "beone_awmessagetypename";
			public const string Awproducts = "beone_awproducts";
			public const string BCENumber = "beone_bcenumber";
			public const string Brokerfeedback = "beone_brokerfeedback";
			public const string BrokerfeedbackName = "beone_brokerfeedbackname";
			public const string Claimhistoryid = "beone_claimhistoryid";
			public const string ClaimhistoryidName = "beone_claimhistoryidname";
			public const string Closesla = "beone_closesla";
			public const string CloseslaName = "beone_closeslaname";
			public const string Config = "beone_config";
			public const string ConfigName = "beone_configname";
			public const string Connexity = "beone_connexity";
			public const string ConnexityName = "beone_connexityname";
			public const string Contractid = "beone_contractid";
			public const string ContractidName = "beone_contractidname";
			public const string Contractlist = "beone_contractlist";
			public const string Contractnumber = "beone_contractnumber";
			public const string CustomerName = "beone_customername";
			public const string Datein = "beone_datein";
			public const string Declarationnumber1 = "beone_declarationnumber1";
			public const string Declarationnumber2 = "beone_declarationnumber2";
			public const string Declarationnumbers = "beone_declarationnumbers";
			public const string Description = "beone_description";
			public const string Domain = "beone_domain";
			public const string Domainaccident_amount = "beone_domainaccident_amount";
			public const string Domaincar_amount = "beone_domaincar_amount";
			public const string Domainfire_amount = "beone_domainfire_amount";
			public const string Domainfireengineering_amount = "beone_domainfireengineering_amount";
			public const string Domainlaw_amount = "beone_domainlaw_amount";
			public const string Domainliability_amount = "beone_domainliability_amount";
			public const string DomainName = "beone_domainname";
			public const string Dossierid = "beone_dossierid";
			public const string DossieridName = "beone_dossieridname";
			public const string Dossiernumber = "beone_dossiernumber";
			public const string Duedate = "beone_duedate";
			public const string Entityid = "beone_entityid";
			public const string EntityidName = "beone_entityidname";
			public const string EntityidYomiName = "beone_entityidyominame";
			public const string Externalofferref = "beone_externalofferref";
			public const string Externalrework = "beone_externalrework";
			public const string ExternalreworkName = "beone_externalreworkname";
			public const string Firsttodounderwriterid = "beone_firsttodounderwriterid";
			public const string FirsttodounderwriteridName = "beone_firsttodounderwriteridname";
			public const string Fleetnumber = "beone_fleetnumber";
			public const string Forcedduedate = "beone_forcedduedate";
			public const string Groupofferid = "beone_groupofferid";
			public const string GroupofferidName = "beone_groupofferidname";
			public const string Importance = "beone_importance";
			public const string ImportanceName = "beone_importancename";
			public const string Internalexternal = "beone_internalexternal";
			public const string InternalexternalName = "beone_internalexternalname";
			public const string Internalrework = "beone_internalrework";
			public const string InternalreworkName = "beone_internalreworkname";
			public const string Isautoclose = "beone_isautoclose";
			public const string IsautocloseName = "beone_isautoclosename";
			public const string IsUrgent = "beone_isurgent";
			public const string IsurgentName = "beone_isurgentname";
			public const string Json = "beone_json";
			public const string Licenseplate = "beone_licenseplate";
			public const string Mailboxid = "beone_mailboxid";
			public const string Maindomain = "beone_maindomain";
			public const string Maindomaingroup = "beone_maindomaingroup";
			public const string MaindomaingroupName = "beone_maindomaingroupname";
			public const string MaindomainName = "beone_maindomainname";
			public const string Messageid = "beone_messageid";
			public const string Modifiedonbehalfby = "beone_modifiedonbehalfby";
			public const string ModifiedonbehalfbyName = "beone_modifiedonbehalfbyname";
			public const string ModifiedonbehalfbyYomiName = "beone_modifiedonbehalfbyyominame";
			public const string Motive = "beone_motive";
			public const string MotiveName = "beone_motivename";
			public const string Multiregardingid = "beone_multiregardingid";
			public const string MultiregardingidName = "beone_multiregardingidname";
			public const string Naceid = "beone_naceid";
			public const string NaceidName = "beone_naceidname";
			public const string Name = "beone_name";
			public const string Ncmpproid = "beone_ncmpproid";
			public const string NcmpproidName = "beone_ncmpproidname";
			public const string Newslainstanceid = "beone_newslainstanceid";
			public const string NewslainstanceidName = "beone_newslainstanceidname";
			public const string Offerpriority = "beone_offerpriority";
			public const string OfferpriorityName = "beone_offerpriorityname";
			public const string Onholdduration = "beone_onholdduration";
			public const string Onholdreason = "beone_onholdreason";
			public const string OnholdreasonName = "beone_onholdreasonname";
			public const string Origin = "beone_origin";
			public const string Originalawmessageid = "beone_originalawmessageid";
			public const string OriginalawmessageidName = "beone_originalawmessageidname";
			public const string Originaldocument = "beone_originaldocument";
			public const string OriginalEmailId = "beone_originalemailid";
			public const string OriginalEmailIdName = "beone_originalemailidname";
			public const string Originalletterid = "beone_originalletterid";
			public const string OriginalletteridName = "beone_originalletteridname";
			public const string Originalmpbid = "beone_originalmpbid";
			public const string OriginalmpbidName = "beone_originalmpbidname";
			public const string Originalphonecallid = "beone_originalphonecallid";
			public const string OriginalphonecallidName = "beone_originalphonecallidname";
			public const string Originalsenderemailaddress = "beone_originalsenderemailaddress";
			public const string OriginName = "beone_originname";
			public const string Processexpw = "beone_processexpw";
			public const string Productid = "beone_productid";
			public const string ProductidName = "beone_productidname";
			public const string Productids = "beone_productids";
			public const string Pruningdecision = "beone_pruningdecision";
			public const string PruningdecisionName = "beone_pruningdecisionname";
			public const string Pruningid = "beone_pruningid";
			public const string PruningidName = "beone_pruningidname";
			public const string RequestId = "beone_requestid";
			public const string Requesttypeid = "beone_requesttypeid";
			public const string RequesttypeidName = "beone_requesttypeidname";
			public const string Signature = "beone_signature";
			public const string SignatureName = "beone_signaturename";
			public const string Sladuration = "beone_sladuration";
			public const string SladurationName = "beone_sladurationname";
			public const string Slainstanceid = "beone_slainstanceid";
			public const string SlainstanceidName = "beone_slainstanceidname";
			public const string Slakpiinstancestatus = "beone_slakpiinstancestatus";
			public const string SlakpiinstancestatusName = "beone_slakpiinstancestatusname";
			public const string Slasuccessfailuredate = "beone_slasuccessfailuredate";
			public const string Specificdate = "beone_specificdate";
			public const string Totalproducts = "beone_totalproducts";
			public const string VerifyBroker = "beone_verifybroker";
			public const string VerifybrokerName = "beone_verifybrokername";
			public const string CreatedBy = "createdby";
			public const string CreatedByName = "createdbyname";
			public const string CreatedByYomiName = "createdbyyominame";
			public const string CreatedOn = "createdon";
			public const string CreatedOnBehalfBy = "createdonbehalfby";
			public const string CreatedOnBehalfByName = "createdonbehalfbyname";
			public const string CreatedOnBehalfByYomiName = "createdonbehalfbyyominame";
			public const string ImportSequenceNumber = "importsequencenumber";
			public const string LastOnHoldTime = "lastonholdtime";
			public const string ModifiedBy = "modifiedby";
			public const string ModifiedByName = "modifiedbyname";
			public const string ModifiedByYomiName = "modifiedbyyominame";
			public const string ModifiedOn = "modifiedon";
			public const string ModifiedOnBehalfBy = "modifiedonbehalfby";
			public const string ModifiedOnBehalfByName = "modifiedonbehalfbyname";
			public const string ModifiedOnBehalfByYomiName = "modifiedonbehalfbyyominame";
			public const string OnHoldTime = "onholdtime";
			public const string OverriddenCreatedOn = "overriddencreatedon";
			public const string OwnerId = "ownerid";
			public const string OwnerIdName = "owneridname";
			public const string OwnerIdYomiName = "owneridyominame";
			public const string OwningBusinessUnit = "owningbusinessunit";
			public const string OwningBusinessUnitName = "owningbusinessunitname";
			public const string OwningTeam = "owningteam";
			public const string OwningUser = "owninguser";
			public const string SLAId = "slaid";
			public const string SLAInvokedId = "slainvokedid";
			public const string SLAInvokedIdName = "slainvokedidname";
			public const string SLAName = "slaname";
			public const string Statecode = "statecode";
			public const string StatecodeName = "statecodename";
			public const string Statuscode = "statuscode";
			public const string StatuscodeName = "statuscodename";
			public const string TimeZoneRuleVersionNumber = "timezoneruleversionnumber";
			public const string UTCConversionTimeZoneCode = "utcconversiontimezonecode";
			public const string VersionNumber = "versionnumber";
		}
	
		/// <summary>
		/// Max Length: 200</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Actgmotivelabel
		
		[AttributeLogicalName("beone_actgmotivelabel")]
		public string Actgmotivelabel
		{
			get => TryGetAttributeValue("beone_actgmotivelabel", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Targets: ag_actiontype</br>
		/// </summary>
		//Actiontypeid
		
		[AttributeLogicalName("beone_actiontypeid")]
		public EntityReference Actiontypeid
		{
			get => TryGetAttributeValue("beone_actiontypeid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_actiontypeid</br>
		/// Max Length: 600</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//ActiontypeidName
		
		[AttributeLogicalName("beone_actiontypeidname")]
		public string ActiontypeidName
		{
			get => FormattedValues.Contains("beone_actiontypeid") ? FormattedValues["beone_actiontypeid"] : null;
		
		}
		/// <summary>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Auto Number Format: 
		/// </summary>
		//Activity
		
		[AttributeLogicalName("beone_activity")]
		public string Activity
		{
			get => TryGetAttributeValue("beone_activity", out string value) ? value : null;
		}
		/// <summary>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Adagionumber
		
		[AttributeLogicalName("beone_adagionumber")]
		public string Adagionumber
		{
			get => TryGetAttributeValue("beone_adagionumber", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Agolstatus
		
		[AttributeLogicalName("beone_agolstatus")]
		public Request.Choices.AgolStatus? Agolstatus
		{
			get => TryGetAttributeValue("beone_agolstatus", out OptionSetValue opt) && opt == null ? null : (Request.Choices.AgolStatus)opt.Value;
		}
		/// <summary>
		/// Attribute of: beone_agolstatus</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//AgolstatusName
		
		[AttributeLogicalName("beone_agolstatusname")]
		public string AgolstatusName
		{
			get => FormattedValues.Contains("beone_agolstatus") ? FormattedValues["beone_agolstatusname"] : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Annulationdate
		
		[AttributeLogicalName("beone_annulationdate")]
		public DateTime? Annulationdate
		{
			get => TryGetAttributeValue("beone_annulationdate", out DateTime value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Awmessagetype
		
		[AttributeLogicalName("beone_awmessagetype")]
		public Request.Choices.AwMessageType? Awmessagetype
		{
			get => TryGetAttributeValue("beone_awmessagetype", out OptionSetValue opt) && opt == null ? null : (Request.Choices.AwMessageType)opt.Value;
		}
		/// <summary>
		/// Attribute of: beone_awmessagetype</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//AwmessagetypeName
		
		[AttributeLogicalName("beone_awmessagetypename")]
		public string AwmessagetypeName
		{
			get => FormattedValues.Contains("beone_awmessagetype") ? FormattedValues["beone_awmessagetypename"] : null;
		}
	
		/// <summary>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Awproducts
		
		[AttributeLogicalName("beone_awproducts")]
		public string Awproducts
		{
			get => TryGetAttributeValue("beone_awproducts", out string value) ? value : null;
		}
		/// <summary>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Auto Number Format: 
		/// </summary>
		//BCENumber
		
		[AttributeLogicalName("beone_bcenumber")]
		public string BCENumber
		{
			get => TryGetAttributeValue("beone_bcenumber", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Brokerfeedback
		
		[AttributeLogicalName("beone_brokerfeedback")]
		public Request.Choices.BrokerFeedback? Brokerfeedback
		{
			get => TryGetAttributeValue("beone_brokerfeedback", out OptionSetValue opt) && opt == null ? null : (Request.Choices.BrokerFeedback)opt.Value;
		}
		/// <summary>
		/// Attribute of: beone_brokerfeedback</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//BrokerfeedbackName
		
		[AttributeLogicalName("beone_brokerfeedbackname")]
		public string BrokerfeedbackName
		{
			get => FormattedValues.Contains("beone_brokerfeedback") ? FormattedValues["beone_brokerfeedbackname"] : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Targets: beone_claimhistory</br>
		/// </summary>
		//Claimhistoryid
		
		[AttributeLogicalName("beone_claimhistoryid")]
		public EntityReference Claimhistoryid
		{
			get => TryGetAttributeValue("beone_claimhistoryid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_claimhistoryid</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//ClaimhistoryidName
		
		[AttributeLogicalName("beone_claimhistoryidname")]
		public string ClaimhistoryidName
		{
			get => FormattedValues.Contains("beone_claimhistoryid") ? FormattedValues["beone_claimhistoryid"] : null;
		
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Closesla
		
		[AttributeLogicalName("beone_closesla")]
		public bool? Closesla
		{
			get => TryGetAttributeValue("beone_closesla", out bool value) ? value : null;
		}
		/// <summary>
		/// Attribute of: beone_closesla</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//CloseslaName
		
		[AttributeLogicalName("beone_closeslaname")]
		public string CloseslaName
		{
			get => FormattedValues.Contains("beone_closesla") ? FormattedValues["beone_closeslaname"] : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Targets: beone_requesttypeconfig</br>
		/// </summary>
		//Config
		
		[AttributeLogicalName("beone_config")]
		public EntityReference Config
		{
			get => TryGetAttributeValue("beone_config", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_config</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//ConfigName
		
		[AttributeLogicalName("beone_configname")]
		public string ConfigName
		{
			get => FormattedValues.Contains("beone_config") ? FormattedValues["beone_config"] : null;
		
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Connexity
		
		[AttributeLogicalName("beone_connexity")]
		public Request.Choices.FlowConnexity? Connexity
		{
			get => TryGetAttributeValue("beone_connexity", out OptionSetValue opt) && opt == null ? null : (Request.Choices.FlowConnexity)opt.Value;
		}
		/// <summary>
		/// Attribute of: beone_connexity</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//ConnexityName
		
		[AttributeLogicalName("beone_connexityname")]
		public string ConnexityName
		{
			get => FormattedValues.Contains("beone_connexity") ? FormattedValues["beone_connexityname"] : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Targets: beone_contract</br>
		/// </summary>
		//Contractid
		
		[AttributeLogicalName("beone_contractid")]
		public EntityReference Contractid
		{
			get => TryGetAttributeValue("beone_contractid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_contractid</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//ContractidName
		
		[AttributeLogicalName("beone_contractidname")]
		public string ContractidName
		{
			get => FormattedValues.Contains("beone_contractid") ? FormattedValues["beone_contractid"] : null;
		
		}
		/// <summary>
		/// Max Length: 500</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Contractlist
		
		[AttributeLogicalName("beone_contractlist")]
		public string Contractlist
		{
			get => TryGetAttributeValue("beone_contractlist", out string value) ? value : null;
		}
		/// <summary>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Contractnumber
		
		[AttributeLogicalName("beone_contractnumber")]
		public string Contractnumber
		{
			get => TryGetAttributeValue("beone_contractnumber", out string value) ? value : null;
		}
		/// <summary>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Auto Number Format: 
		/// </summary>
		//CustomerName
		
		[AttributeLogicalName("beone_customername")]
		public string CustomerName
		{
			get => TryGetAttributeValue("beone_customername", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: ApplicationRequired</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Datein
		
		[AttributeLogicalName("beone_datein")]
		public DateTime? Datein
		{
			get => TryGetAttributeValue("beone_datein", out DateTime value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Declarationnumber1
		
		[AttributeLogicalName("beone_declarationnumber1")]
		public int? Declarationnumber1
		{
			get => TryGetAttributeValue("beone_declarationnumber1", out int value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Declarationnumber2
		
		[AttributeLogicalName("beone_declarationnumber2")]
		public int? Declarationnumber2
		{
			get => TryGetAttributeValue("beone_declarationnumber2", out int value) ? value : null;
		}
		/// <summary>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Declarationnumbers
		
		[AttributeLogicalName("beone_declarationnumbers")]
		public string Declarationnumbers
		{
			get => TryGetAttributeValue("beone_declarationnumbers", out string value) ? value : null;
		}
		/// <summary>
		/// Max Length: 2000</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Description
		
		[AttributeLogicalName("beone_description")]
		public string Description
		{
			get => TryGetAttributeValue("beone_description", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Domain
		
		[AttributeLogicalName("beone_domain")]
		public Request.Choices.FlowTaskDomain? Domain
		{
			get => TryGetAttributeValue("beone_domain", out OptionSetValue opt) && opt == null ? null : (Request.Choices.FlowTaskDomain)opt.Value;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Domainaccident_amount
		
		[AttributeLogicalName("beone_domainaccident_amount")]
		public int? Domainaccident_amount
		{
			get => TryGetAttributeValue("beone_domainaccident_amount", out int value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Domaincar_amount
		
		[AttributeLogicalName("beone_domaincar_amount")]
		public int? Domaincar_amount
		{
			get => TryGetAttributeValue("beone_domaincar_amount", out int value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Domainfire_amount
		
		[AttributeLogicalName("beone_domainfire_amount")]
		public int? Domainfire_amount
		{
			get => TryGetAttributeValue("beone_domainfire_amount", out int value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Domainfireengineering_amount
		
		[AttributeLogicalName("beone_domainfireengineering_amount")]
		public int? Domainfireengineering_amount
		{
			get => TryGetAttributeValue("beone_domainfireengineering_amount", out int value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Domainlaw_amount
		
		[AttributeLogicalName("beone_domainlaw_amount")]
		public int? Domainlaw_amount
		{
			get => TryGetAttributeValue("beone_domainlaw_amount", out int value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Domainliability_amount
		
		[AttributeLogicalName("beone_domainliability_amount")]
		public int? Domainliability_amount
		{
			get => TryGetAttributeValue("beone_domainliability_amount", out int value) ? value : null;
		}
		/// <summary>
		/// Attribute of: beone_domain</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//DomainName
		
		[AttributeLogicalName("beone_domainname")]
		public string DomainName
		{
			get => FormattedValues.Contains("beone_domain") ? FormattedValues["beone_domainname"] : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Targets: beone_dossier</br>
		/// </summary>
		//Dossierid
		
		[AttributeLogicalName("beone_dossierid")]
		public EntityReference Dossierid
		{
			get => TryGetAttributeValue("beone_dossierid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_dossierid</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//DossieridName
		
		[AttributeLogicalName("beone_dossieridname")]
		public string DossieridName
		{
			get => FormattedValues.Contains("beone_dossierid") ? FormattedValues["beone_dossierid"] : null;
		
		}
		/// <summary>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Dossiernumber
		
		[AttributeLogicalName("beone_dossiernumber")]
		public string Dossiernumber
		{
			get => TryGetAttributeValue("beone_dossiernumber", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Duedate
		
		[AttributeLogicalName("beone_duedate")]
		public DateTime? Duedate
		{
			get => TryGetAttributeValue("beone_duedate", out DateTime value) ? value : null;
		}
		/// <summary>
		/// Required Level: ApplicationRequired</br>
		/// Valid for: Create Update Read</br>
		/// Targets: team</br>
		/// </summary>
		//Entityid
		
		[AttributeLogicalName("beone_entityid")]
		public EntityReference Entityid
		{
			get => TryGetAttributeValue("beone_entityid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_entityid</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//EntityidName
		
		[AttributeLogicalName("beone_entityidname")]
		public string EntityidName
		{
			get => FormattedValues.Contains("beone_entityid") ? FormattedValues["beone_entityid"] : null;
		
		}
		/// <summary>
		/// Attribute of: beone_entityid</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//EntityidYomiName
		
		[AttributeLogicalName("beone_entityidyominame")]
		public string EntityidYomiName
		{
			get => FormattedValues.Contains("beone_entityid") ? FormattedValues["beone_entityid"] : null;
		
		}
		/// <summary>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Externalofferref
		
		[AttributeLogicalName("beone_externalofferref")]
		public string Externalofferref
		{
			get => TryGetAttributeValue("beone_externalofferref", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Externalrework
		
		[AttributeLogicalName("beone_externalrework")]
		public bool? Externalrework
		{
			get => TryGetAttributeValue("beone_externalrework", out bool value) ? value : null;
		}
		/// <summary>
		/// Attribute of: beone_externalrework</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//ExternalreworkName
		
		[AttributeLogicalName("beone_externalreworkname")]
		public string ExternalreworkName
		{
			get => FormattedValues.Contains("beone_externalrework") ? FormattedValues["beone_externalreworkname"] : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Targets: beone_underwriter</br>
		/// </summary>
		//Firsttodounderwriterid
		
		[AttributeLogicalName("beone_firsttodounderwriterid")]
		public EntityReference Firsttodounderwriterid
		{
			get => TryGetAttributeValue("beone_firsttodounderwriterid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_firsttodounderwriterid</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//FirsttodounderwriteridName
		
		[AttributeLogicalName("beone_firsttodounderwriteridname")]
		public string FirsttodounderwriteridName
		{
			get => FormattedValues.Contains("beone_firsttodounderwriterid") ? FormattedValues["beone_firsttodounderwriterid"] : null;
		
		}
		/// <summary>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Fleetnumber
		
		[AttributeLogicalName("beone_fleetnumber")]
		public string Fleetnumber
		{
			get => TryGetAttributeValue("beone_fleetnumber", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Forcedduedate
		
		[AttributeLogicalName("beone_forcedduedate")]
		public DateTime? Forcedduedate
		{
			get => TryGetAttributeValue("beone_forcedduedate", out DateTime value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Targets: beone_groupoffer</br>
		/// </summary>
		//Groupofferid
		
		[AttributeLogicalName("beone_groupofferid")]
		public EntityReference Groupofferid
		{
			get => TryGetAttributeValue("beone_groupofferid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_groupofferid</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//GroupofferidName
		
		[AttributeLogicalName("beone_groupofferidname")]
		public string GroupofferidName
		{
			get => FormattedValues.Contains("beone_groupofferid") ? FormattedValues["beone_groupofferid"] : null;
		
		}
		/// <summary>
		/// Required Level: ApplicationRequired</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Importance
		
		[AttributeLogicalName("beone_importance")]
		public Request.Choices.RequestImportance? Importance
		{
			get => TryGetAttributeValue("beone_importance", out OptionSetValue opt) && opt == null ? null : (Request.Choices.RequestImportance)opt.Value;
		}
		/// <summary>
		/// Attribute of: beone_importance</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//ImportanceName
		
		[AttributeLogicalName("beone_importancename")]
		public string ImportanceName
		{
			get => FormattedValues.Contains("beone_importance") ? FormattedValues["beone_importancename"] : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Internalexternal
		
		[AttributeLogicalName("beone_internalexternal")]
		public Request.Choices.InternalExternal? Internalexternal
		{
			get => TryGetAttributeValue("beone_internalexternal", out OptionSetValue opt) && opt == null ? null : (Request.Choices.InternalExternal)opt.Value;
		}
		/// <summary>
		/// Attribute of: beone_internalexternal</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//InternalexternalName
		
		[AttributeLogicalName("beone_internalexternalname")]
		public string InternalexternalName
		{
			get => FormattedValues.Contains("beone_internalexternal") ? FormattedValues["beone_internalexternalname"] : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Internalrework
		
		[AttributeLogicalName("beone_internalrework")]
		public bool? Internalrework
		{
			get => TryGetAttributeValue("beone_internalrework", out bool value) ? value : null;
		}
		/// <summary>
		/// Attribute of: beone_internalrework</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//InternalreworkName
		
		[AttributeLogicalName("beone_internalreworkname")]
		public string InternalreworkName
		{
			get => FormattedValues.Contains("beone_internalrework") ? FormattedValues["beone_internalreworkname"] : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Isautoclose
		
		[AttributeLogicalName("beone_isautoclose")]
		public bool? Isautoclose
		{
			get => TryGetAttributeValue("beone_isautoclose", out bool value) ? value : null;
		}
		/// <summary>
		/// Attribute of: beone_isautoclose</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//IsautocloseName
		
		[AttributeLogicalName("beone_isautoclosename")]
		public string IsautocloseName
		{
			get => FormattedValues.Contains("beone_isautoclose") ? FormattedValues["beone_isautoclosename"] : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//IsUrgent
		
		[AttributeLogicalName("beone_isurgent")]
		public bool? IsUrgent
		{
			get => TryGetAttributeValue("beone_isurgent", out bool value) ? value : null;
		}
		/// <summary>
		/// Attribute of: beone_isurgent</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//IsurgentName
		
		[AttributeLogicalName("beone_isurgentname")]
		public string IsurgentName
		{
			get => FormattedValues.Contains("beone_isurgent") ? FormattedValues["beone_isurgentname"] : null;
		}
	
		/// <summary>
		/// Max Length: 10000</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Json
		
		[AttributeLogicalName("beone_json")]
		public string Json
		{
			get => TryGetAttributeValue("beone_json", out string value) ? value : null;
		}
		/// <summary>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Licenseplate
		
		[AttributeLogicalName("beone_licenseplate")]
		public string Licenseplate
		{
			get => TryGetAttributeValue("beone_licenseplate", out string value) ? value : null;
		}
		/// <summary>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Mailboxid
		
		[AttributeLogicalName("beone_mailboxid")]
		public string Mailboxid
		{
			get => TryGetAttributeValue("beone_mailboxid", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Maindomain
		
		[AttributeLogicalName("beone_maindomain")]
		public Request.Choices.MainDomain? Maindomain
		{
			get => TryGetAttributeValue("beone_maindomain", out OptionSetValue opt) && opt == null ? null : (Request.Choices.MainDomain)opt.Value;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Maindomaingroup
		
		[AttributeLogicalName("beone_maindomaingroup")]
		public Request.Choices.MainDomainGroup? Maindomaingroup
		{
			get => TryGetAttributeValue("beone_maindomaingroup", out OptionSetValue opt) && opt == null ? null : (Request.Choices.MainDomainGroup)opt.Value;
		}
		/// <summary>
		/// Attribute of: beone_maindomaingroup</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//MaindomaingroupName
		
		[AttributeLogicalName("beone_maindomaingroupname")]
		public string MaindomaingroupName
		{
			get => FormattedValues.Contains("beone_maindomaingroup") ? FormattedValues["beone_maindomaingroupname"] : null;
		}
	
		/// <summary>
		/// Attribute of: beone_maindomain</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//MaindomainName
		
		[AttributeLogicalName("beone_maindomainname")]
		public string MaindomainName
		{
			get => FormattedValues.Contains("beone_maindomain") ? FormattedValues["beone_maindomainname"] : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Messageid
		
		[AttributeLogicalName("beone_messageid")]
		public int? Messageid
		{
			get => TryGetAttributeValue("beone_messageid", out int value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Targets: systemuser</br>
		/// </summary>
		//Modifiedonbehalfby
		
		[AttributeLogicalName("beone_modifiedonbehalfby")]
		public EntityReference Modifiedonbehalfby
		{
			get => TryGetAttributeValue("beone_modifiedonbehalfby", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_modifiedonbehalfby</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//ModifiedonbehalfbyName
		
		[AttributeLogicalName("beone_modifiedonbehalfbyname")]
		public string ModifiedonbehalfbyName
		{
			get => FormattedValues.Contains("beone_modifiedonbehalfby") ? FormattedValues["beone_modifiedonbehalfby"] : null;
		
		}
		/// <summary>
		/// Attribute of: beone_modifiedonbehalfby</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//ModifiedonbehalfbyYomiName
		
		[AttributeLogicalName("beone_modifiedonbehalfbyyominame")]
		public string ModifiedonbehalfbyYomiName
		{
			get => FormattedValues.Contains("beone_modifiedonbehalfby") ? FormattedValues["beone_modifiedonbehalfby"] : null;
		
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Motive
		
		[AttributeLogicalName("beone_motive")]
		public Request.Choices.Motive? Motive
		{
			get => TryGetAttributeValue("beone_motive", out OptionSetValue opt) && opt == null ? null : (Request.Choices.Motive)opt.Value;
		}
		/// <summary>
		/// Attribute of: beone_motive</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//MotiveName
		
		[AttributeLogicalName("beone_motivename")]
		public string MotiveName
		{
			get => FormattedValues.Contains("beone_motive") ? FormattedValues["beone_motivename"] : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Targets: beone_multiregardingrequest</br>
		/// </summary>
		//Multiregardingid
		
		[AttributeLogicalName("beone_multiregardingid")]
		public EntityReference Multiregardingid
		{
			get => TryGetAttributeValue("beone_multiregardingid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_multiregardingid</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//MultiregardingidName
		
		[AttributeLogicalName("beone_multiregardingidname")]
		public string MultiregardingidName
		{
			get => FormattedValues.Contains("beone_multiregardingid") ? FormattedValues["beone_multiregardingid"] : null;
		
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Targets: beone_nace</br>
		/// </summary>
		//Naceid
		
		[AttributeLogicalName("beone_naceid")]
		public EntityReference Naceid
		{
			get => TryGetAttributeValue("beone_naceid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_naceid</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//NaceidName
		
		[AttributeLogicalName("beone_naceidname")]
		public string NaceidName
		{
			get => FormattedValues.Contains("beone_naceid") ? FormattedValues["beone_naceid"] : null;
		
		}
		/// <summary>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Auto Number Format: RQ-{SEQNUM:9}
		/// </summary>
		//Name
		
		[AttributeLogicalName("beone_name")]
		public string Name
		{
			get => TryGetAttributeValue("beone_name", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Targets: beone_ncmppro</br>
		/// </summary>
		//Ncmpproid
		
		[AttributeLogicalName("beone_ncmpproid")]
		public EntityReference Ncmpproid
		{
			get => TryGetAttributeValue("beone_ncmpproid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_ncmpproid</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//NcmpproidName
		
		[AttributeLogicalName("beone_ncmpproidname")]
		public string NcmpproidName
		{
			get => FormattedValues.Contains("beone_ncmpproid") ? FormattedValues["beone_ncmpproid"] : null;
		
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Targets: beone_slainstance</br>
		/// </summary>
		//Newslainstanceid
		
		[AttributeLogicalName("beone_newslainstanceid")]
		public EntityReference Newslainstanceid
		{
			get => TryGetAttributeValue("beone_newslainstanceid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_newslainstanceid</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//NewslainstanceidName
		
		[AttributeLogicalName("beone_newslainstanceidname")]
		public string NewslainstanceidName
		{
			get => FormattedValues.Contains("beone_newslainstanceid") ? FormattedValues["beone_newslainstanceid"] : null;
		
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Offerpriority
		
		[AttributeLogicalName("beone_offerpriority")]
		public Request.Choices.FlowPriority? Offerpriority
		{
			get => TryGetAttributeValue("beone_offerpriority", out OptionSetValue opt) && opt == null ? null : (Request.Choices.FlowPriority)opt.Value;
		}
		/// <summary>
		/// Attribute of: beone_offerpriority</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//OfferpriorityName
		
		[AttributeLogicalName("beone_offerpriorityname")]
		public string OfferpriorityName
		{
			get => FormattedValues.Contains("beone_offerpriority") ? FormattedValues["beone_offerpriorityname"] : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Onholdduration
		
		[AttributeLogicalName("beone_onholdduration")]
		public decimal? Onholdduration
		{
			get => TryGetAttributeValue("beone_onholdduration", out decimal value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Onholdreason
		
		[AttributeLogicalName("beone_onholdreason")]
		public Request.Choices.OnHoldReason? Onholdreason
		{
			get => TryGetAttributeValue("beone_onholdreason", out OptionSetValue opt) && opt == null ? null : (Request.Choices.OnHoldReason)opt.Value;
		}
		/// <summary>
		/// Attribute of: beone_onholdreason</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//OnholdreasonName
		
		[AttributeLogicalName("beone_onholdreasonname")]
		public string OnholdreasonName
		{
			get => FormattedValues.Contains("beone_onholdreason") ? FormattedValues["beone_onholdreasonname"] : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Origin
		
		[AttributeLogicalName("beone_origin")]
		public Request.Choices.RequestOrigin? Origin
		{
			get => TryGetAttributeValue("beone_origin", out OptionSetValue opt) && opt == null ? null : (Request.Choices.RequestOrigin)opt.Value;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Targets: beone_awmessage</br>
		/// </summary>
		//Originalawmessageid
		
		[AttributeLogicalName("beone_originalawmessageid")]
		public EntityReference Originalawmessageid
		{
			get => TryGetAttributeValue("beone_originalawmessageid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_originalawmessageid</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//OriginalawmessageidName
		
		[AttributeLogicalName("beone_originalawmessageidname")]
		public string OriginalawmessageidName
		{
			get => FormattedValues.Contains("beone_originalawmessageid") ? FormattedValues["beone_originalawmessageid"] : null;
		
		}
		/// <summary>
		/// Max Length: 200</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Originaldocument
		
		[AttributeLogicalName("beone_originaldocument")]
		public string Originaldocument
		{
			get => TryGetAttributeValue("beone_originaldocument", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Targets: email</br>
		/// </summary>
		//OriginalEmailId
		
		[AttributeLogicalName("beone_originalemailid")]
		public EntityReference OriginalEmailId
		{
			get => TryGetAttributeValue("beone_originalemailid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_originalemailid</br>
		/// Max Length: 800</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//OriginalEmailIdName
		
		[AttributeLogicalName("beone_originalemailidname")]
		public string OriginalEmailIdName
		{
			get => FormattedValues.Contains("beone_originalemailid") ? FormattedValues["beone_originalemailid"] : null;
		
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Targets: letter</br>
		/// </summary>
		//Originalletterid
		
		[AttributeLogicalName("beone_originalletterid")]
		public EntityReference Originalletterid
		{
			get => TryGetAttributeValue("beone_originalletterid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_originalletterid</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//OriginalletteridName
		
		[AttributeLogicalName("beone_originalletteridname")]
		public string OriginalletteridName
		{
			get => FormattedValues.Contains("beone_originalletterid") ? FormattedValues["beone_originalletterid"] : null;
		
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Targets: beone_mpb</br>
		/// </summary>
		//Originalmpbid
		
		[AttributeLogicalName("beone_originalmpbid")]
		public EntityReference Originalmpbid
		{
			get => TryGetAttributeValue("beone_originalmpbid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_originalmpbid</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//OriginalmpbidName
		
		[AttributeLogicalName("beone_originalmpbidname")]
		public string OriginalmpbidName
		{
			get => FormattedValues.Contains("beone_originalmpbid") ? FormattedValues["beone_originalmpbid"] : null;
		
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Targets: phonecall</br>
		/// </summary>
		//Originalphonecallid
		
		[AttributeLogicalName("beone_originalphonecallid")]
		public EntityReference Originalphonecallid
		{
			get => TryGetAttributeValue("beone_originalphonecallid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_originalphonecallid</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//OriginalphonecallidName
		
		[AttributeLogicalName("beone_originalphonecallidname")]
		public string OriginalphonecallidName
		{
			get => FormattedValues.Contains("beone_originalphonecallid") ? FormattedValues["beone_originalphonecallid"] : null;
		
		}
		/// <summary>
		/// Max Length: 200</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Originalsenderemailaddress
		
		[AttributeLogicalName("beone_originalsenderemailaddress")]
		public string Originalsenderemailaddress
		{
			get => TryGetAttributeValue("beone_originalsenderemailaddress", out string value) ? value : null;
		}
		/// <summary>
		/// Attribute of: beone_origin</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//OriginName
		
		[AttributeLogicalName("beone_originname")]
		public string OriginName
		{
			get => FormattedValues.Contains("beone_origin") ? FormattedValues["beone_originname"] : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Processexpw
		
		[AttributeLogicalName("beone_processexpw")]
		public int? Processexpw
		{
			get => TryGetAttributeValue("beone_processexpw", out int value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Targets: product</br>
		/// </summary>
		//Productid
		
		[AttributeLogicalName("beone_productid")]
		public EntityReference Productid
		{
			get => TryGetAttributeValue("beone_productid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_productid</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//ProductidName
		
		[AttributeLogicalName("beone_productidname")]
		public string ProductidName
		{
			get => FormattedValues.Contains("beone_productid") ? FormattedValues["beone_productid"] : null;
		
		}
		/// <summary>
		/// Max Length: 700</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Productids
		
		[AttributeLogicalName("beone_productids")]
		public string Productids
		{
			get => TryGetAttributeValue("beone_productids", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Pruningdecision
		
		[AttributeLogicalName("beone_pruningdecision")]
		public Request.Choices.PruningDecision? Pruningdecision
		{
			get => TryGetAttributeValue("beone_pruningdecision", out OptionSetValue opt) && opt == null ? null : (Request.Choices.PruningDecision)opt.Value;
		}
		/// <summary>
		/// Attribute of: beone_pruningdecision</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//PruningdecisionName
		
		[AttributeLogicalName("beone_pruningdecisionname")]
		public string PruningdecisionName
		{
			get => FormattedValues.Contains("beone_pruningdecision") ? FormattedValues["beone_pruningdecisionname"] : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Targets: beone_pruning</br>
		/// </summary>
		//Pruningid
		
		[AttributeLogicalName("beone_pruningid")]
		public EntityReference Pruningid
		{
			get => TryGetAttributeValue("beone_pruningid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_pruningid</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//PruningidName
		
		[AttributeLogicalName("beone_pruningidname")]
		public string PruningidName
		{
			get => FormattedValues.Contains("beone_pruningid") ? FormattedValues["beone_pruningid"] : null;
		
		}
		/// <summary>
		/// Required Level: SystemRequired</br>
		/// Valid for: Create Read</br>
		/// </summary>
		//RequestId
		
		
		/// <summary>
		/// Required Level: ApplicationRequired</br>
		/// Valid for: Create Update Read</br>
		/// Targets: ag_requesttype</br>
		/// </summary>
		//Requesttypeid
		
		[AttributeLogicalName("beone_requesttypeid")]
		public EntityReference Requesttypeid
		{
			get => TryGetAttributeValue("beone_requesttypeid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_requesttypeid</br>
		/// Max Length: 500</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//RequesttypeidName
		
		[AttributeLogicalName("beone_requesttypeidname")]
		public string RequesttypeidName
		{
			get => FormattedValues.Contains("beone_requesttypeid") ? FormattedValues["beone_requesttypeid"] : null;
		
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Signature
		
		[AttributeLogicalName("beone_signature")]
		public Request.Choices.Boolean? Signature
		{
			get => TryGetAttributeValue("beone_signature", out OptionSetValue opt) && opt == null ? null : (Request.Choices.Boolean)opt.Value;
		}
		/// <summary>
		/// Attribute of: beone_signature</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//SignatureName
		
		[AttributeLogicalName("beone_signaturename")]
		public string SignatureName
		{
			get => FormattedValues.Contains("beone_signature") ? FormattedValues["beone_signaturename"] : null;
		}
	
		/// <summary>
		/// Required Level: ApplicationRequired</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Sladuration
		
		[AttributeLogicalName("beone_sladuration")]
		public Request.Choices.SlaDuration? Sladuration
		{
			get => TryGetAttributeValue("beone_sladuration", out OptionSetValue opt) && opt == null ? null : (Request.Choices.SlaDuration)opt.Value;
		}
		/// <summary>
		/// Attribute of: beone_sladuration</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//SladurationName
		
		[AttributeLogicalName("beone_sladurationname")]
		public string SladurationName
		{
			get => FormattedValues.Contains("beone_sladuration") ? FormattedValues["beone_sladurationname"] : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Targets: slakpiinstance</br>
		/// </summary>
		//Slainstanceid
		
		[AttributeLogicalName("beone_slainstanceid")]
		public EntityReference Slainstanceid
		{
			get => TryGetAttributeValue("beone_slainstanceid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: beone_slainstanceid</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//SlainstanceidName
		
		[AttributeLogicalName("beone_slainstanceidname")]
		public string SlainstanceidName
		{
			get => FormattedValues.Contains("beone_slainstanceid") ? FormattedValues["beone_slainstanceid"] : null;
		
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Slakpiinstancestatus
		
		[AttributeLogicalName("beone_slakpiinstancestatus")]
		public Request.Choices.SlaKpiInstanceStatus? Slakpiinstancestatus
		{
			get => TryGetAttributeValue("beone_slakpiinstancestatus", out OptionSetValue opt) && opt == null ? null : (Request.Choices.SlaKpiInstanceStatus)opt.Value;
		}
		/// <summary>
		/// Attribute of: beone_slakpiinstancestatus</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//SlakpiinstancestatusName
		
		[AttributeLogicalName("beone_slakpiinstancestatusname")]
		public string SlakpiinstancestatusName
		{
			get => FormattedValues.Contains("beone_slakpiinstancestatus") ? FormattedValues["beone_slakpiinstancestatusname"] : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Slasuccessfailuredate
		
		[AttributeLogicalName("beone_slasuccessfailuredate")]
		public DateTime? Slasuccessfailuredate
		{
			get => TryGetAttributeValue("beone_slasuccessfailuredate", out DateTime value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Specificdate
		
		[AttributeLogicalName("beone_specificdate")]
		public DateTime? Specificdate
		{
			get => TryGetAttributeValue("beone_specificdate", out DateTime value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//Totalproducts
		
		[AttributeLogicalName("beone_totalproducts")]
		public decimal? Totalproducts
		{
			get => TryGetAttributeValue("beone_totalproducts", out decimal value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//VerifyBroker
		
		[AttributeLogicalName("beone_verifybroker")]
		public bool? VerifyBroker
		{
			get => TryGetAttributeValue("beone_verifybroker", out bool value) ? value : null;
		}
		/// <summary>
		/// Attribute of: beone_verifybroker</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//VerifybrokerName
		
		[AttributeLogicalName("beone_verifybrokername")]
		public string VerifybrokerName
		{
			get => FormattedValues.Contains("beone_verifybroker") ? FormattedValues["beone_verifybrokername"] : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// Targets: systemuser</br>
		/// </summary>
		//CreatedBy
		
		[AttributeLogicalName("createdby")]
		public EntityReference CreatedBy
		{
			get => TryGetAttributeValue("createdby", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: createdby</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//CreatedByName
		
		[AttributeLogicalName("createdbyname")]
		public string CreatedByName
		{
			get => FormattedValues.Contains("createdby") ? FormattedValues["createdby"] : null;
		
		}
		/// <summary>
		/// Attribute of: createdby</br>
		/// Max Length: 100</br>
		/// Required Level: SystemRequired</br>
		/// Valid for: Read</br>
		/// </summary>
		//CreatedByYomiName
		
		[AttributeLogicalName("createdbyyominame")]
		public string CreatedByYomiName
		{
			get => FormattedValues.Contains("createdby") ? FormattedValues["createdby"] : null;
		
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//CreatedOn
		
		[AttributeLogicalName("createdon")]
		public DateTime? CreatedOn
		{
			get => TryGetAttributeValue("createdon", out DateTime value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// Targets: systemuser</br>
		/// </summary>
		//CreatedOnBehalfBy
		
		[AttributeLogicalName("createdonbehalfby")]
		public EntityReference CreatedOnBehalfBy
		{
			get => TryGetAttributeValue("createdonbehalfby", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: createdonbehalfby</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//CreatedOnBehalfByName
		
		[AttributeLogicalName("createdonbehalfbyname")]
		public string CreatedOnBehalfByName
		{
			get => FormattedValues.Contains("createdonbehalfby") ? FormattedValues["createdonbehalfby"] : null;
		
		}
		/// <summary>
		/// Attribute of: createdonbehalfby</br>
		/// Max Length: 100</br>
		/// Required Level: SystemRequired</br>
		/// Valid for: Read</br>
		/// </summary>
		//CreatedOnBehalfByYomiName
		
		[AttributeLogicalName("createdonbehalfbyyominame")]
		public string CreatedOnBehalfByYomiName
		{
			get => FormattedValues.Contains("createdonbehalfby") ? FormattedValues["createdonbehalfby"] : null;
		
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Read</br>
		/// </summary>
		//ImportSequenceNumber
		
		[AttributeLogicalName("importsequencenumber")]
		public int? ImportSequenceNumber
		{
			get => TryGetAttributeValue("importsequencenumber", out int value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//LastOnHoldTime
		
		[AttributeLogicalName("lastonholdtime")]
		public DateTime? LastOnHoldTime
		{
			get => TryGetAttributeValue("lastonholdtime", out DateTime value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// Targets: systemuser</br>
		/// </summary>
		//ModifiedBy
		
		[AttributeLogicalName("modifiedby")]
		public EntityReference ModifiedBy
		{
			get => TryGetAttributeValue("modifiedby", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: modifiedby</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//ModifiedByName
		
		[AttributeLogicalName("modifiedbyname")]
		public string ModifiedByName
		{
			get => FormattedValues.Contains("modifiedby") ? FormattedValues["modifiedby"] : null;
		
		}
		/// <summary>
		/// Attribute of: modifiedby</br>
		/// Max Length: 100</br>
		/// Required Level: SystemRequired</br>
		/// Valid for: Read</br>
		/// </summary>
		//ModifiedByYomiName
		
		[AttributeLogicalName("modifiedbyyominame")]
		public string ModifiedByYomiName
		{
			get => FormattedValues.Contains("modifiedby") ? FormattedValues["modifiedby"] : null;
		
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//ModifiedOn
		
		[AttributeLogicalName("modifiedon")]
		public DateTime? ModifiedOn
		{
			get => TryGetAttributeValue("modifiedon", out DateTime value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// Targets: systemuser</br>
		/// </summary>
		//ModifiedOnBehalfBy
		
		[AttributeLogicalName("modifiedonbehalfby")]
		public EntityReference ModifiedOnBehalfBy
		{
			get => TryGetAttributeValue("modifiedonbehalfby", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: modifiedonbehalfby</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//ModifiedOnBehalfByName
		
		[AttributeLogicalName("modifiedonbehalfbyname")]
		public string ModifiedOnBehalfByName
		{
			get => FormattedValues.Contains("modifiedonbehalfby") ? FormattedValues["modifiedonbehalfby"] : null;
		
		}
		/// <summary>
		/// Attribute of: modifiedonbehalfby</br>
		/// Max Length: 100</br>
		/// Required Level: SystemRequired</br>
		/// Valid for: Read</br>
		/// </summary>
		//ModifiedOnBehalfByYomiName
		
		[AttributeLogicalName("modifiedonbehalfbyyominame")]
		public string ModifiedOnBehalfByYomiName
		{
			get => FormattedValues.Contains("modifiedonbehalfby") ? FormattedValues["modifiedonbehalfby"] : null;
		
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//OnHoldTime
		
		[AttributeLogicalName("onholdtime")]
		public int? OnHoldTime
		{
			get => TryGetAttributeValue("onholdtime", out int value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Read</br>
		/// </summary>
		//OverriddenCreatedOn
		
		[AttributeLogicalName("overriddencreatedon")]
		public DateTime? OverriddenCreatedOn
		{
			get => TryGetAttributeValue("overriddencreatedon", out DateTime value) ? value : null;
		}
		/// <summary>
		/// Required Level: SystemRequired</br>
		/// Valid for: Create Update Read</br>
		/// Targets: systemuser,team</br>
		/// </summary>
		//OwnerId
		
		[AttributeLogicalName("ownerid")]
		public EntityReference OwnerId
		{
			get => TryGetAttributeValue("ownerid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: ownerid</br>
		/// Max Length: 100</br>
		/// Required Level: SystemRequired</br>
		/// Valid for: Read</br>
		/// </summary>
		//OwnerIdName
		
		[AttributeLogicalName("owneridname")]
		public string OwnerIdName
		{
			get => TryGetAttributeValue("owneridname", out string value) ? value : null;
		}
		/// <summary>
		/// Attribute of: ownerid</br>
		/// Max Length: 100</br>
		/// Required Level: SystemRequired</br>
		/// Valid for: Read</br>
		/// </summary>
		//OwnerIdYomiName
		
		[AttributeLogicalName("owneridyominame")]
		public string OwnerIdYomiName
		{
			get => TryGetAttributeValue("owneridyominame", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// Targets: businessunit</br>
		/// </summary>
		//OwningBusinessUnit
		
		[AttributeLogicalName("owningbusinessunit")]
		public EntityReference OwningBusinessUnit
		{
			get => TryGetAttributeValue("owningbusinessunit", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: owningbusinessunit</br>
		/// Max Length: 160</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//OwningBusinessUnitName
		
		[AttributeLogicalName("owningbusinessunitname")]
		public string OwningBusinessUnitName
		{
			get => FormattedValues.Contains("owningbusinessunit") ? FormattedValues["owningbusinessunit"] : null;
		
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// Targets: team</br>
		/// </summary>
		//OwningTeam
		
		[AttributeLogicalName("owningteam")]
		public EntityReference OwningTeam
		{
			get => TryGetAttributeValue("owningteam", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// Targets: systemuser</br>
		/// </summary>
		//OwningUser
		
		[AttributeLogicalName("owninguser")]
		public EntityReference OwningUser
		{
			get => TryGetAttributeValue("owninguser", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// Targets: sla</br>
		/// </summary>
		//SLAId
		
		[AttributeLogicalName("slaid")]
		public EntityReference SLAId
		{
			get => TryGetAttributeValue("slaid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// Targets: sla</br>
		/// </summary>
		//SLAInvokedId
		
		[AttributeLogicalName("slainvokedid")]
		public EntityReference SLAInvokedId
		{
			get => TryGetAttributeValue("slainvokedid", out EntityReference value) ? value : null;
		}
	
		/// <summary>
		/// Attribute of: slainvokedid</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//SLAInvokedIdName
		
		[AttributeLogicalName("slainvokedidname")]
		public string SLAInvokedIdName
		{
			get => FormattedValues.Contains("slainvokedid") ? FormattedValues["slainvokedid"] : null;
		
		}
		/// <summary>
		/// Attribute of: slaid</br>
		/// Max Length: 100</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//SLAName
		
		[AttributeLogicalName("slaname")]
		public string SLAName
		{
			get => FormattedValues.Contains("slaid") ? FormattedValues["slaid"] : null;
		
		}
		/// <summary>
		/// Required Level: SystemRequired</br>
		/// Valid for: Update Read</br>
		/// </summary>
		//Statecode
		
		[AttributeLogicalName("statecode")]
		public Request.Choices.Status? Statecode
		{
			get => TryGetAttributeValue("statecode", out OptionSetValue opt) && opt != null ? (Request.Choices.Status)Enum.ToObject(typeof(Request.Choices.Status), opt.Value) : null;
		}
		/// <summary>
		/// Attribute of: statecode</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//StatecodeName
		
		[AttributeLogicalName("statecodename")]
		public string StatecodeName
		{
			get => FormattedValues.Contains("statecode") ? FormattedValues["statecodename"] : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//Statuscode
		
		[AttributeLogicalName("statuscode")]
		public Request.Choices.StatusReason? Statuscode
		{
			get => TryGetAttributeValue("statuscode", out OptionSetValue opt) && opt != null ? (Request.Choices.StatusReason)Enum.ToObject(typeof(Request.Choices.StatusReason), opt.Value) : null;
		}
		/// <summary>
		/// Attribute of: statuscode</br>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//StatuscodeName
		
		[AttributeLogicalName("statuscodename")]
		public string StatuscodeName
		{
			get => FormattedValues.Contains("statuscode") ? FormattedValues["statuscodename"] : null;
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//TimeZoneRuleVersionNumber
		
		[AttributeLogicalName("timezoneruleversionnumber")]
		public int? TimeZoneRuleVersionNumber
		{
			get => TryGetAttributeValue("timezoneruleversionnumber", out int value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		//UTCConversionTimeZoneCode
		
		[AttributeLogicalName("utcconversiontimezonecode")]
		public int? UTCConversionTimeZoneCode
		{
			get => TryGetAttributeValue("utcconversiontimezonecode", out int value) ? value : null;
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		//VersionNumber
		
		[AttributeLogicalName("versionnumber")]
		public long? VersionNumber
		{
			get => TryGetAttributeValue("versionnumber", out long value) ? value : null;
		}
	}
	

}
public partial class PreCreateManager : CreatePluginManager
{
	[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")] public PreCreateManager() : base("beone_request", EStage.PreOperation) { }

	new PreCreate.TargetRequest Target => Target.ToEntity<PreCreate.TargetRequest>();
}
