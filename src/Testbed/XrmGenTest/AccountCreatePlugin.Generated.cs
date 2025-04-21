using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Extensions;
using System;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace XrmGenTest;

[GeneratedCode("TemplatedCodeGenerator", "1.0.0.0")]
public partial class AccountCreatePlugin : PluginBase
{
	[GeneratedCode("TemplatedCodeGenerator", "1.0.0.0")]
	[EntityLogicalName("account")]
	public class TargetAccount : Entity
	{
		public static class Meta
		{
			public const string EntityLogicalName = "account";
			public const string EntityLogicalCollectionName = "accounts";
			public const string EntitySetName = "accounts";
			public const string PrimaryNameAttribute = "";
			public const string PrimaryIdAttribute = "accountid";
	
			public partial class Fields
			{
				public const string AccountCategoryCode = "accountcategorycode";
				public const string AccountClassificationCode = "accountclassificationcode";
				public const string AccountNumber = "accountnumber";
			}
	
			public partial class Choices
			{
				/// <summary>
				/// Drop-down list for selecting the category of the account.
				/// </summary>
				[DataContract]
				public enum Category
				{
					[EnumMember]
					PreferredCustomer = 1,
					[EnumMember]
					Standard = 2,
				}
				/// <summary>
				/// Drop-down list for classifying an account.
				/// </summary>
				[DataContract]
				public enum Classification
				{
					[EnumMember]
					DefaultValue = 1,
				}
			}
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		[AttributeLogicalName("accountcategorycode")]
		public TargetAccount.Meta.Choices.Category? AccountCategoryCode
		{
			get => TryGetAttributeValue("accountcategorycode", out OptionSetValue opt) && opt == null ? null : (TargetAccount.Meta.Choices.Category)opt.Value;
			set => this["accountcategorycode"] = value == null ? null : new OptionSetValue((int)value);
		}
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		[AttributeLogicalName("accountclassificationcode")]
		public TargetAccount.Meta.Choices.Classification? AccountClassificationCode
		{
			get => TryGetAttributeValue("accountclassificationcode", out OptionSetValue opt) && opt == null ? null : (TargetAccount.Meta.Choices.Classification)opt.Value;
			set => this["accountclassificationcode"] = value == null ? null : new OptionSetValue((int)value);
		}
		/// <summary>
		/// Max Length: 20</br>
		/// Required Level: None</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		[AttributeLogicalName("accountnumber")]
		public string AccountNumber
		{
			get => TryGetAttributeValue("accountnumber", out string value) ? value : null;
			set => this["accountnumber"] = value;
		}
	}
	[GeneratedCode("TemplatedCodeGenerator", "1.0.0.0")]
	[EntityLogicalName("account")]
	public class PostImageAccount : Entity
	{
		public static class Meta
		{
			public const string EntityLogicalName = "account";
			public const string EntityLogicalCollectionName = "accounts";
			public const string EntitySetName = "accounts";
			public const string PrimaryNameAttribute = "";
			public const string PrimaryIdAttribute = "accountid";
	
			public partial class Fields
			{
				public const string AccountCategoryCode = "accountcategorycode";
				public const string AccountCategoryCodeName = "accountcategorycodename";
				public const string AccountClassificationCode = "accountclassificationcode";
				public const string AccountClassificationCodeName = "accountclassificationcodename";
				public const string AccountId = "accountid";
				public const string AccountNumber = "accountnumber";
				public const string AccountRatingCode = "accountratingcode";
				public const string AccountRatingCodeName = "accountratingcodename";
				public const string Address1_AddressId = "address1_addressid";
				public const string Address1_AddressTypeCode = "address1_addresstypecode";
				public const string Address1_AddressTypeCodeName = "address1_addresstypecodename";
				public const string Address1_City = "address1_city";
				public const string Address1_Composite = "address1_composite";
				public const string Address1_Country = "address1_country";
				public const string Address1_County = "address1_county";
				public const string Address1_Fax = "address1_fax";
				public const string Address1_FreightTermsCode = "address1_freighttermscode";
				public const string Address1_FreightTermsCodeName = "address1_freighttermscodename";
				public const string Address1_Latitude = "address1_latitude";
				public const string Address1_Line1 = "address1_line1";
				public const string Address1_Line2 = "address1_line2";
				public const string Address1_Line3 = "address1_line3";
				public const string Address1_Longitude = "address1_longitude";
				public const string Address1_Name = "address1_name";
				public const string Address1_PostalCode = "address1_postalcode";
				public const string Address1_PostOfficeBox = "address1_postofficebox";
				public const string Address1_PrimaryContactName = "address1_primarycontactname";
				public const string Address1_ShippingMethodCode = "address1_shippingmethodcode";
				public const string Address1_ShippingMethodCodeName = "address1_shippingmethodcodename";
				public const string Address1_StateOrProvince = "address1_stateorprovince";
				public const string Address1_Telephone1 = "address1_telephone1";
				public const string Address1_Telephone2 = "address1_telephone2";
				public const string Address1_Telephone3 = "address1_telephone3";
				public const string Address1_UPSZone = "address1_upszone";
				public const string Address1_UTCOffset = "address1_utcoffset";
				public const string Address2_AddressId = "address2_addressid";
				public const string Address2_AddressTypeCode = "address2_addresstypecode";
				public const string Address2_AddressTypeCodeName = "address2_addresstypecodename";
				public const string Address2_City = "address2_city";
				public const string Address2_Composite = "address2_composite";
				public const string Address2_Country = "address2_country";
				public const string Address2_County = "address2_county";
				public const string Address2_Fax = "address2_fax";
				public const string Address2_FreightTermsCode = "address2_freighttermscode";
				public const string Address2_FreightTermsCodeName = "address2_freighttermscodename";
				public const string Address2_Latitude = "address2_latitude";
				public const string Address2_Line1 = "address2_line1";
				public const string Address2_Line2 = "address2_line2";
				public const string Address2_Line3 = "address2_line3";
				public const string Address2_Longitude = "address2_longitude";
				public const string Address2_Name = "address2_name";
				public const string Address2_PostalCode = "address2_postalcode";
				public const string Address2_PostOfficeBox = "address2_postofficebox";
				public const string Address2_PrimaryContactName = "address2_primarycontactname";
				public const string Address2_ShippingMethodCode = "address2_shippingmethodcode";
				public const string Address2_ShippingMethodCodeName = "address2_shippingmethodcodename";
				public const string Address2_StateOrProvince = "address2_stateorprovince";
				public const string Address2_Telephone1 = "address2_telephone1";
				public const string Address2_Telephone2 = "address2_telephone2";
				public const string Address2_Telephone3 = "address2_telephone3";
				public const string Address2_UPSZone = "address2_upszone";
				public const string Address2_UTCOffset = "address2_utcoffset";
				public const string Adx_CreatedByIPAddress = "adx_createdbyipaddress";
				public const string Adx_CreatedByUsername = "adx_createdbyusername";
				public const string Adx_ModifiedByIPAddress = "adx_modifiedbyipaddress";
				public const string Adx_ModifiedByUsername = "adx_modifiedbyusername";
				public const string Aging30 = "aging30";
				public const string Aging30_Base = "aging30_base";
				public const string Aging60 = "aging60";
				public const string Aging60_Base = "aging60_base";
				public const string Aging90 = "aging90";
				public const string Aging90_Base = "aging90_base";
				public const string BusinessTypeCode = "businesstypecode";
				public const string BusinessTypeCodeName = "businesstypecodename";
				public const string CreatedBy = "createdby";
				public static readonly ReadOnlyCollection<string> CreatedByTargets = new (["systemuser"]);
				public const string CreatedByExternalParty = "createdbyexternalparty";
				public static readonly ReadOnlyCollection<string> CreatedByExternalPartyTargets = new (["externalparty"]);
				public const string CreatedByExternalPartyName = "createdbyexternalpartyname";
				public const string CreatedByExternalPartyYomiName = "createdbyexternalpartyyominame";
				public const string CreatedByName = "createdbyname";
				public const string CreatedByYomiName = "createdbyyominame";
				public const string CreatedOn = "createdon";
				public const string CreatedOnBehalfBy = "createdonbehalfby";
				public static readonly ReadOnlyCollection<string> CreatedOnBehalfByTargets = new (["systemuser"]);
				public const string CreatedOnBehalfByName = "createdonbehalfbyname";
				public const string CreatedOnBehalfByYomiName = "createdonbehalfbyyominame";
				public const string CreditLimit = "creditlimit";
				public const string CreditLimit_Base = "creditlimit_base";
				public const string CreditOnHold = "creditonhold";
				public const string CreditOnHoldName = "creditonholdname";
				public const string CustomerSizeCode = "customersizecode";
				public const string CustomerSizeCodeName = "customersizecodename";
				public const string CustomerTypeCode = "customertypecode";
				public const string CustomerTypeCodeName = "customertypecodename";
				public const string Description = "description";
				public const string DoNotBulkEMail = "donotbulkemail";
				public const string DoNotBulkEMailName = "donotbulkemailname";
				public const string DoNotBulkPostalMail = "donotbulkpostalmail";
				public const string DoNotBulkPostalMailName = "donotbulkpostalmailname";
				public const string DoNotEMail = "donotemail";
				public const string DoNotEMailName = "donotemailname";
				public const string DoNotFax = "donotfax";
				public const string DoNotFaxName = "donotfaxname";
				public const string DoNotPhone = "donotphone";
				public const string DoNotPhoneName = "donotphonename";
				public const string DoNotPostalMail = "donotpostalmail";
				public const string DoNotPostalMailName = "donotpostalmailname";
				public const string DoNotSendMarketingMaterialName = "donotsendmarketingmaterialname";
				public const string DoNotSendMM = "donotsendmm";
				public const string EMailAddress1 = "emailaddress1";
				public const string EMailAddress2 = "emailaddress2";
				public const string EMailAddress3 = "emailaddress3";
				public const string EntityImage = "entityimage";
				public const string EntityImage_Timestamp = "entityimage_timestamp";
				public const string EntityImage_URL = "entityimage_url";
				public const string EntityImageId = "entityimageid";
				public const string ExchangeRate = "exchangerate";
				public const string Fax = "fax";
				public const string FollowEmail = "followemail";
				public const string FollowEmailName = "followemailname";
				public const string FtpSiteURL = "ftpsiteurl";
				public const string ImportSequenceNumber = "importsequencenumber";
				public const string IndustryCode = "industrycode";
				public const string IndustryCodeName = "industrycodename";
				public const string IsPrivate = "isprivate";
				public const string IsPrivateName = "isprivatename";
				public const string LastOnHoldTime = "lastonholdtime";
				public const string LastUsedInCampaign = "lastusedincampaign";
				public const string MarketCap = "marketcap";
				public const string MarketCap_Base = "marketcap_base";
				public const string MarketingOnly = "marketingonly";
				public const string MarketingOnlyName = "marketingonlyname";
				public const string MasterAccountIdName = "masteraccountidname";
				public const string MasterAccountIdYomiName = "masteraccountidyominame";
				public const string MasterId = "masterid";
				public static readonly ReadOnlyCollection<string> MasterIdTargets = new (["account"]);
				public const string Merged = "merged";
				public const string MergedName = "mergedname";
				public const string ModifiedBy = "modifiedby";
				public static readonly ReadOnlyCollection<string> ModifiedByTargets = new (["systemuser"]);
				public const string ModifiedByExternalParty = "modifiedbyexternalparty";
				public static readonly ReadOnlyCollection<string> ModifiedByExternalPartyTargets = new (["externalparty"]);
				public const string ModifiedByExternalPartyName = "modifiedbyexternalpartyname";
				public const string ModifiedByExternalPartyYomiName = "modifiedbyexternalpartyyominame";
				public const string ModifiedByName = "modifiedbyname";
				public const string ModifiedByYomiName = "modifiedbyyominame";
				public const string ModifiedOn = "modifiedon";
				public const string ModifiedOnBehalfBy = "modifiedonbehalfby";
				public static readonly ReadOnlyCollection<string> ModifiedOnBehalfByTargets = new (["systemuser"]);
				public const string ModifiedOnBehalfByName = "modifiedonbehalfbyname";
				public const string ModifiedOnBehalfByYomiName = "modifiedonbehalfbyyominame";
				public const string Msa_managingpartnerid = "msa_managingpartnerid";
				public static readonly ReadOnlyCollection<string> Msa_managingpartneridTargets = new (["account"]);
				public const string Msa_managingpartneridName = "msa_managingpartneridname";
				public const string Msa_managingpartneridYomiName = "msa_managingpartneridyominame";
				public const string Name = "name";
				public const string NumberOfEmployees = "numberofemployees";
				public const string OnHoldTime = "onholdtime";
				public const string OverriddenCreatedOn = "overriddencreatedon";
				public const string OwnerId = "ownerid";
				public const string OwnerIdName = "owneridname";
				public const string OwnerIdType = "owneridtype";
				public const string OwnerIdYomiName = "owneridyominame";
				public const string OwnershipCode = "ownershipcode";
				public const string OwnershipCodeName = "ownershipcodename";
				public const string OwningBusinessUnit = "owningbusinessunit";
				public static readonly ReadOnlyCollection<string> OwningBusinessUnitTargets = new (["businessunit"]);
				public const string OwningBusinessUnitName = "owningbusinessunitname";
				public const string OwningTeam = "owningteam";
				public static readonly ReadOnlyCollection<string> OwningTeamTargets = new (["team"]);
				public const string OwningUser = "owninguser";
				public static readonly ReadOnlyCollection<string> OwningUserTargets = new (["systemuser"]);
				public const string ParentAccountId = "parentaccountid";
				public static readonly ReadOnlyCollection<string> ParentAccountIdTargets = new (["account"]);
				public const string ParentAccountIdName = "parentaccountidname";
				public const string ParentAccountIdYomiName = "parentaccountidyominame";
				public const string ParticipatesInWorkflow = "participatesinworkflow";
				public const string ParticipatesInWorkflowName = "participatesinworkflowname";
				public const string PaymentTermsCode = "paymenttermscode";
				public const string PaymentTermsCodeName = "paymenttermscodename";
				public const string PreferredAppointmentDayCode = "preferredappointmentdaycode";
				public const string PreferredAppointmentDayCodeName = "preferredappointmentdaycodename";
				public const string PreferredAppointmentTimeCode = "preferredappointmenttimecode";
				public const string PreferredAppointmentTimeCodeName = "preferredappointmenttimecodename";
				public const string PreferredContactMethodCode = "preferredcontactmethodcode";
				public const string PreferredContactMethodCodeName = "preferredcontactmethodcodename";
				public const string PreferredSystemUserId = "preferredsystemuserid";
				public static readonly ReadOnlyCollection<string> PreferredSystemUserIdTargets = new (["systemuser"]);
				public const string PreferredSystemUserIdName = "preferredsystemuseridname";
				public const string PreferredSystemUserIdYomiName = "preferredsystemuseridyominame";
				public const string PrimaryContactId = "primarycontactid";
				public static readonly ReadOnlyCollection<string> PrimaryContactIdTargets = new (["contact"]);
				public const string PrimaryContactIdName = "primarycontactidname";
				public const string PrimaryContactIdYomiName = "primarycontactidyominame";
				public const string PrimarySatoriId = "primarysatoriid";
				public const string PrimaryTwitterId = "primarytwitterid";
				public const string ProcessId = "processid";
				public const string Revenue = "revenue";
				public const string Revenue_Base = "revenue_base";
				public const string SharesOutstanding = "sharesoutstanding";
				public const string ShippingMethodCode = "shippingmethodcode";
				public const string ShippingMethodCodeName = "shippingmethodcodename";
				public const string SIC = "sic";
				public const string SLAId = "slaid";
				public static readonly ReadOnlyCollection<string> SLAIdTargets = new (["sla"]);
				public const string SLAInvokedId = "slainvokedid";
				public static readonly ReadOnlyCollection<string> SLAInvokedIdTargets = new (["sla"]);
				public const string SLAInvokedIdName = "slainvokedidname";
				public const string SLAName = "slaname";
				public const string StageId = "stageid";
				public const string StateCode = "statecode";
				public const string StateCodeName = "statecodename";
				public const string StatusCode = "statuscode";
				public const string StatusCodeName = "statuscodename";
				public const string StockExchange = "stockexchange";
				public const string Telephone1 = "telephone1";
				public const string Telephone2 = "telephone2";
				public const string Telephone3 = "telephone3";
				public const string TerritoryCode = "territorycode";
				public const string TerritoryCodeName = "territorycodename";
				public const string TickerSymbol = "tickersymbol";
				public const string TimeSpentByMeOnEmailAndMeetings = "timespentbymeonemailandmeetings";
				public const string TimeZoneRuleVersionNumber = "timezoneruleversionnumber";
				public const string TransactionCurrencyId = "transactioncurrencyid";
				public static readonly ReadOnlyCollection<string> TransactionCurrencyIdTargets = new (["transactioncurrency"]);
				public const string TransactionCurrencyIdName = "transactioncurrencyidname";
				public const string TraversedPath = "traversedpath";
				public const string UTCConversionTimeZoneCode = "utcconversiontimezonecode";
				public const string VersionNumber = "versionnumber";
				public const string WebSiteURL = "websiteurl";
				public const string YomiName = "yominame";
			}
	
			public partial class Choices
			{
				/// <summary>
				/// Drop-down list for selecting the category of the account.
				/// </summary>
				[DataContract]
				public enum Category
				{
					[EnumMember]
					PreferredCustomer = 1,
					[EnumMember]
					Standard = 2,
				}
				/// <summary>
				/// Drop-down list for classifying an account.
				/// </summary>
				[DataContract]
				public enum Classification
				{
					[EnumMember]
					DefaultValue = 1,
				}
				/// <summary>
				/// Drop-down list for selecting account ratings.
				/// </summary>
				[DataContract]
				public enum AccountRating
				{
					[EnumMember]
					DefaultValue = 1,
				}
				/// <summary>
				/// Type of address for address 1, such as billing, shipping, or primary address.
				/// </summary>
				[DataContract]
				public enum Address1AddressType
				{
					[EnumMember]
					BillTo = 1,
					[EnumMember]
					ShipTo = 2,
					[EnumMember]
					Primary = 3,
					[EnumMember]
					Other = 4,
				}
				/// <summary>
				/// Freight terms for address 1.
				/// </summary>
				[DataContract]
				public enum Address1FreightTerms
				{
					[EnumMember]
					Fob = 1,
					[EnumMember]
					NoCharge = 2,
				}
				/// <summary>
				/// Method of shipment for address 1.
				/// </summary>
				[DataContract]
				public enum Address1ShippingMethod
				{
					[EnumMember]
					Airborne = 1,
					[EnumMember]
					Dhl = 2,
					[EnumMember]
					Fedex = 3,
					[EnumMember]
					Ups = 4,
					[EnumMember]
					PostalMail = 5,
					[EnumMember]
					FullLoad = 6,
					[EnumMember]
					WillCall = 7,
				}
				/// <summary>
				/// Type of address for address 2, such as billing, shipping, or primary address.
				/// </summary>
				[DataContract]
				public enum Address2AddressType
				{
					[EnumMember]
					DefaultValue = 1,
				}
				/// <summary>
				/// Freight terms for address 2.
				/// </summary>
				[DataContract]
				public enum Address2FreightTerms
				{
					[EnumMember]
					DefaultValue = 1,
				}
				/// <summary>
				/// Method of shipment for address 2.
				/// </summary>
				[DataContract]
				public enum Address2ShippingMethod
				{
					[EnumMember]
					DefaultValue = 1,
				}
				/// <summary>
				/// Type of business associated with the account.
				/// </summary>
				[DataContract]
				public enum BusinessType
				{
					[EnumMember]
					DefaultValue = 1,
				}
				/// <summary>
				/// Size of the account.
				/// </summary>
				[DataContract]
				public enum CustomerSize
				{
					[EnumMember]
					DefaultValue = 1,
				}
				/// <summary>
				/// Type of the account.
				/// </summary>
				[DataContract]
				public enum RelationshipType
				{
					[EnumMember]
					Competitor = 1,
					[EnumMember]
					Consultant = 2,
					[EnumMember]
					Customer = 3,
					[EnumMember]
					Investor = 4,
					[EnumMember]
					Partner = 5,
					[EnumMember]
					Influencer = 6,
					[EnumMember]
					Press = 7,
					[EnumMember]
					Prospect = 8,
					[EnumMember]
					Reseller = 9,
					[EnumMember]
					Supplier = 10,
					[EnumMember]
					Vendor = 11,
					[EnumMember]
					Other = 12,
				}
				/// <summary>
				/// Type of industry with which the account is associated.
				/// </summary>
				[DataContract]
				public enum Industry
				{
					[EnumMember]
					Accounting = 1,
					[EnumMember]
					AgricultureAndNonPetrolNaturalResourceExtraction = 2,
					[EnumMember]
					BroadcastingPrintingAndPublishing = 3,
					[EnumMember]
					Brokers = 4,
					[EnumMember]
					BuildingSupplyRetail = 5,
					[EnumMember]
					BusinessServices = 6,
					[EnumMember]
					Consulting = 7,
					[EnumMember]
					ConsumerServices = 8,
					[EnumMember]
					DesignDirectionAndCreativeManagement = 9,
					[EnumMember]
					DistributorsDispatchersAndProcessors = 10,
					[EnumMember]
					DoctorSOfficesAndClinics = 11,
					[EnumMember]
					DurableManufacturing = 12,
					[EnumMember]
					EatingAndDrinkingPlaces = 13,
					[EnumMember]
					EntertainmentRetail = 14,
					[EnumMember]
					EquipmentRentalAndLeasing = 15,
					[EnumMember]
					Financial = 16,
					[EnumMember]
					FoodAndTobaccoProcessing = 17,
					[EnumMember]
					InboundCapitalIntensiveProcessing = 18,
					[EnumMember]
					InboundRepairAndServices = 19,
					[EnumMember]
					Insurance = 20,
					[EnumMember]
					LegalServices = 21,
					[EnumMember]
					NonDurableMerchandiseRetail = 22,
					[EnumMember]
					OutboundConsumerService = 23,
					[EnumMember]
					PetrochemicalExtractionAndDistribution = 24,
					[EnumMember]
					ServiceRetail = 25,
					[EnumMember]
					SigAffiliations = 26,
					[EnumMember]
					SocialServices = 27,
					[EnumMember]
					SpecialOutboundTradeContractors = 28,
					[EnumMember]
					SpecialtyRealty = 29,
					[EnumMember]
					Transportation = 30,
					[EnumMember]
					UtilityCreationAndDistribution = 31,
					[EnumMember]
					VehicleRetail = 32,
					[EnumMember]
					Wholesale = 33,
				}
				/// <summary>
				/// Type of company ownership, such as public or private.
				/// </summary>
				[DataContract]
				public enum Ownership
				{
					[EnumMember]
					Public = 1,
					[EnumMember]
					Private = 2,
					[EnumMember]
					Subsidiary = 3,
					[EnumMember]
					Other = 4,
				}
				/// <summary>
				/// Payment terms for the account.
				/// </summary>
				[DataContract]
				public enum PaymentTerms
				{
					[EnumMember]
					Net30 = 1,
					[EnumMember]
					_210Net30 = 2,
					[EnumMember]
					Net45 = 3,
					[EnumMember]
					Net60 = 4,
				}
				/// <summary>
				/// Day of the week that the account prefers for scheduling service activities.
				/// </summary>
				[DataContract]
				public enum PreferredDay
				{
					[EnumMember]
					Sunday = 0,
					[EnumMember]
					Monday = 1,
					[EnumMember]
					Tuesday = 2,
					[EnumMember]
					Wednesday = 3,
					[EnumMember]
					Thursday = 4,
					[EnumMember]
					Friday = 5,
					[EnumMember]
					Saturday = 6,
				}
				/// <summary>
				/// Time of day that the account prefers for scheduling service activities.
				/// </summary>
				[DataContract]
				public enum PreferredTime
				{
					[EnumMember]
					Morning = 1,
					[EnumMember]
					Afternoon = 2,
					[EnumMember]
					Evening = 3,
				}
				/// <summary>
				/// Preferred contact method for the account.
				/// </summary>
				[DataContract]
				public enum PreferredMethodOfContact
				{
					[EnumMember]
					Any = 1,
					[EnumMember]
					Email = 2,
					[EnumMember]
					Phone = 3,
					[EnumMember]
					Fax = 4,
					[EnumMember]
					Mail = 5,
				}
				/// <summary>
				/// Method of shipment for the account.
				/// </summary>
				[DataContract]
				public enum ShippingMethod
				{
					[EnumMember]
					DefaultValue = 1,
				}
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
				/// <summary>
				/// Territory to which the account belongs.
				/// </summary>
				[DataContract]
				public enum TerritoryCode
				{
					[EnumMember]
					DefaultValue = 1,
				}
			}
		}
	
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		[AttributeLogicalName("accountcategorycode")]
		public PostImageAccount.Meta.Choices.Category? AccountCategoryCode
		{
			get => TryGetAttributeValue("accountcategorycode", out OptionSetValue opt) && opt == null ? null : (PostImageAccount.Meta.Choices.Category)opt.Value;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		[AttributeLogicalName("accountclassificationcode")]
		public PostImageAccount.Meta.Choices.Classification? AccountClassificationCode
		{
			get => TryGetAttributeValue("accountclassificationcode", out OptionSetValue opt) && opt == null ? null : (PostImageAccount.Meta.Choices.Classification)opt.Value;
		}
		/// <summary>
		/// Max Length: 20
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		[AttributeLogicalName("accountnumber")]
		public string AccountNumber
		{
			get => TryGetAttributeValue("accountnumber", out string value) ? value : null;
		}
	}

	public TargetAccount Target { get; set; }
	public PostImageAccount PostImage { get; set; }

	/// <summary>
	/// This method should be called on every <see cref="PluginBase.Execute(IServiceProvider)"/> execution.
	/// </summary>
	/// <param name="serviceProvider"></param>
	/// <exception cref="InvalidPluginExecutionException"></exception>
	internal override void Initialize(IServiceProvider serviceProvider)
    {
        if (serviceProvider == null)
        {
            throw new InvalidPluginExecutionException(nameof(serviceProvider) + " argument is null.");
        }
        var executionContext = serviceProvider.Get<IPluginExecutionContext7>();
        Target = EntityOrDefault<TargetAccount>(executionContext.InputParameters, "Target");
        PostImage = EntityOrDefault<PostImageAccount>(executionContext.PreEntityImages, "PostImage");
    }

	protected static T EntityOrDefault<T>(DataCollection<string, object> keyValues, string key) where T : Entity
    {
        if (keyValues is null) return default;
        return keyValues.TryGetValue(key, out var obj) ? obj is Entity entity ? entity.ToEntity<T>() : default : default;
    }

    protected static T EntityOrDefault<T>(DataCollection<string, Entity> keyValues, string key) where T : Entity
    {
        if (keyValues is null) return default;
        return keyValues.TryGetValue(key, out var entity) ? entity?.ToEntity<T>() : default;
    }

}
