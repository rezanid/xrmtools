using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;

namespace SdkPluginsCS10
{
	/// <summary>
	/// Display Name: Account
	/// </summary>
	[GeneratedCode("TemplatedCodeGenerator", "1.5.0.3")]
	[EntityLogicalName("account")]
	[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public partial class Account : Microsoft.Xrm.Sdk.Entity
	{
		public partial class Meta 
		{
			public const string EntityLogicalName = "account";
			public const string EntityLogicalCollectionName = "accounts";
			public const string EntitySetName = "accounts";
			public const string PrimaryNameAttribute = "name";
			public const string PrimaryIdAttribute = "accountid";

			public partial class Fields
			{
				public const string AccountCategoryCode = "accountcategorycode";
				public const string AccountClassificationCode = "accountclassificationcode";
				public const string AccountNumber = "accountnumber";
				public const string AccountRatingCode = "accountratingcode";
				public const string Address1_AddressTypeCode = "address1_addresstypecode";
				public const string Address1_City = "address1_city";
				public const string Address1_Composite = "address1_composite";
				public const string Address1_Country = "address1_country";
				public const string Address1_County = "address1_county";
				public const string Address1_Fax = "address1_fax";
				public const string Address1_FreightTermsCode = "address1_freighttermscode";
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
				public const string Address1_StateOrProvince = "address1_stateorprovince";
				public const string Address1_Telephone1 = "address1_telephone1";
				public const string Address1_Telephone2 = "address1_telephone2";
				public const string Address1_Telephone3 = "address1_telephone3";
				public const string Address1_UPSZone = "address1_upszone";
				public const string Address1_UTCOffset = "address1_utcoffset";
				public const string Address2_AddressTypeCode = "address2_addresstypecode";
				public const string Address2_City = "address2_city";
				public const string Address2_Composite = "address2_composite";
				public const string Address2_Country = "address2_country";
				public const string Address2_County = "address2_county";
				public const string Address2_Fax = "address2_fax";
				public const string Address2_FreightTermsCode = "address2_freighttermscode";
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
				public const string Cr22a_SupportedLanguages = "cr22a_supportedlanguages";
				public const string CreatedBy = "createdby";
				public static readonly ReadOnlyCollection<string> CreatedByTargets = new ReadOnlyCollection<string>(new string[] { "systemuser" });
				public const string CreatedByExternalParty = "createdbyexternalparty";
				public static readonly ReadOnlyCollection<string> CreatedByExternalPartyTargets = new ReadOnlyCollection<string>(new string[] { "externalparty" });
				public const string CreatedOn = "createdon";
				public const string CreatedOnBehalfBy = "createdonbehalfby";
				public static readonly ReadOnlyCollection<string> CreatedOnBehalfByTargets = new ReadOnlyCollection<string>(new string[] { "systemuser" });
				public const string CreditLimit = "creditlimit";
				public const string CreditLimit_Base = "creditlimit_base";
				public const string CreditOnHold = "creditonhold";
				public const string CustomerSizeCode = "customersizecode";
				public const string CustomerTypeCode = "customertypecode";
				public const string Description = "description";
				public const string DoNotBulkEMail = "donotbulkemail";
				public const string DoNotBulkPostalMail = "donotbulkpostalmail";
				public const string DoNotEMail = "donotemail";
				public const string DoNotFax = "donotfax";
				public const string DoNotPhone = "donotphone";
				public const string DoNotPostalMail = "donotpostalmail";
				public const string DoNotSendMM = "donotsendmm";
				public const string EMailAddress1 = "emailaddress1";
				public const string EMailAddress2 = "emailaddress2";
				public const string EMailAddress3 = "emailaddress3";
				public const string EntityImageId = "entityimageid";
				public const string ExchangeRate = "exchangerate";
				public const string Fax = "fax";
				public const string FollowEmail = "followemail";
				public const string FtpSiteURL = "ftpsiteurl";
				public const string ImportSequenceNumber = "importsequencenumber";
				public const string IndustryCode = "industrycode";
				public const string LastOnHoldTime = "lastonholdtime";
				public const string LastUsedInCampaign = "lastusedincampaign";
				public const string MarketCap = "marketcap";
				public const string MarketCap_Base = "marketcap_base";
				public const string MarketingOnly = "marketingonly";
				public const string MasterId = "masterid";
				public static readonly ReadOnlyCollection<string> MasterIdTargets = new ReadOnlyCollection<string>(new string[] { "account" });
				public const string Merged = "merged";
				public const string ModifiedBy = "modifiedby";
				public static readonly ReadOnlyCollection<string> ModifiedByTargets = new ReadOnlyCollection<string>(new string[] { "systemuser" });
				public const string ModifiedByExternalParty = "modifiedbyexternalparty";
				public static readonly ReadOnlyCollection<string> ModifiedByExternalPartyTargets = new ReadOnlyCollection<string>(new string[] { "externalparty" });
				public const string ModifiedOn = "modifiedon";
				public const string ModifiedOnBehalfBy = "modifiedonbehalfby";
				public static readonly ReadOnlyCollection<string> ModifiedOnBehalfByTargets = new ReadOnlyCollection<string>(new string[] { "systemuser" });
				public const string Msa_managingpartnerid = "msa_managingpartnerid";
				public static readonly ReadOnlyCollection<string> Msa_managingpartneridTargets = new ReadOnlyCollection<string>(new string[] { "account" });
				public const string Msft_DataState = "msft_datastate";
				public const string Name = "name";
				public const string NumberOfEmployees = "numberofemployees";
				public const string OnHoldTime = "onholdtime";
				public const string OverriddenCreatedOn = "overriddencreatedon";
				public const string OwnerId = "ownerid";
				public const string OwnershipCode = "ownershipcode";
				public const string OwningBusinessUnit = "owningbusinessunit";
				public static readonly ReadOnlyCollection<string> OwningBusinessUnitTargets = new ReadOnlyCollection<string>(new string[] { "businessunit" });
				public const string OwningTeam = "owningteam";
				public static readonly ReadOnlyCollection<string> OwningTeamTargets = new ReadOnlyCollection<string>(new string[] { "team" });
				public const string OwningUser = "owninguser";
				public static readonly ReadOnlyCollection<string> OwningUserTargets = new ReadOnlyCollection<string>(new string[] { "systemuser" });
				public const string ParentAccountId = "parentaccountid";
				public static readonly ReadOnlyCollection<string> ParentAccountIdTargets = new ReadOnlyCollection<string>(new string[] { "account" });
				public const string ParticipatesInWorkflow = "participatesinworkflow";
				public const string PaymentTermsCode = "paymenttermscode";
				public const string PreferredAppointmentDayCode = "preferredappointmentdaycode";
				public const string PreferredAppointmentTimeCode = "preferredappointmenttimecode";
				public const string PreferredContactMethodCode = "preferredcontactmethodcode";
				public const string PreferredSystemUserId = "preferredsystemuserid";
				public static readonly ReadOnlyCollection<string> PreferredSystemUserIdTargets = new ReadOnlyCollection<string>(new string[] { "systemuser" });
				public const string PrimaryContactId = "primarycontactid";
				public static readonly ReadOnlyCollection<string> PrimaryContactIdTargets = new ReadOnlyCollection<string>(new string[] { "contact" });
				public const string PrimarySatoriId = "primarysatoriid";
				public const string PrimaryTwitterId = "primarytwitterid";
				public const string ProcessId = "processid";
				public const string Revenue = "revenue";
				public const string Revenue_Base = "revenue_base";
				public const string Rn_SuggestedFollowup = "rn_suggestedfollowup";
				public const string SharesOutstanding = "sharesoutstanding";
				public const string ShippingMethodCode = "shippingmethodcode";
				public const string SIC = "sic";
				public const string SLAId = "slaid";
				public static readonly ReadOnlyCollection<string> SLAIdTargets = new ReadOnlyCollection<string>(new string[] { "sla" });
				public const string SLAInvokedId = "slainvokedid";
				public static readonly ReadOnlyCollection<string> SLAInvokedIdTargets = new ReadOnlyCollection<string>(new string[] { "sla" });
				public const string StageId = "stageid";
				public const string StateCode = "statecode";
				public const string StatusCode = "statuscode";
				public const string StockExchange = "stockexchange";
				public const string Telephone1 = "telephone1";
				public const string Telephone2 = "telephone2";
				public const string Telephone3 = "telephone3";
				public const string TerritoryCode = "territorycode";
				public const string TickerSymbol = "tickersymbol";
				public const string TimeSpentByMeOnEmailAndMeetings = "timespentbymeonemailandmeetings";
				public const string TimeZoneRuleVersionNumber = "timezoneruleversionnumber";
				public const string TransactionCurrencyId = "transactioncurrencyid";
				public static readonly ReadOnlyCollection<string> TransactionCurrencyIdTargets = new ReadOnlyCollection<string>(new string[] { "transactioncurrency" });
				public const string TraversedPath = "traversedpath";
				public const string UTCConversionTimeZoneCode = "utcconversiontimezonecode";
				public const string VersionNumber = "versionnumber";
				public const string WebSiteURL = "websiteurl";
				public const string YomiName = "yominame";

				private static readonly Dictionary<string, string> _fieldMap = new Dictionary<string, string>
				{
					[nameof(AccountCategoryCode)] = "accountcategorycode",
					[nameof(AccountClassificationCode)] = "accountclassificationcode",
					[nameof(AccountNumber)] = "accountnumber",
					[nameof(AccountRatingCode)] = "accountratingcode",
					[nameof(Address1_AddressTypeCode)] = "address1_addresstypecode",
					[nameof(Address1_City)] = "address1_city",
					[nameof(Address1_Composite)] = "address1_composite",
					[nameof(Address1_Country)] = "address1_country",
					[nameof(Address1_County)] = "address1_county",
					[nameof(Address1_Fax)] = "address1_fax",
					[nameof(Address1_FreightTermsCode)] = "address1_freighttermscode",
					[nameof(Address1_Latitude)] = "address1_latitude",
					[nameof(Address1_Line1)] = "address1_line1",
					[nameof(Address1_Line2)] = "address1_line2",
					[nameof(Address1_Line3)] = "address1_line3",
					[nameof(Address1_Longitude)] = "address1_longitude",
					[nameof(Address1_Name)] = "address1_name",
					[nameof(Address1_PostalCode)] = "address1_postalcode",
					[nameof(Address1_PostOfficeBox)] = "address1_postofficebox",
					[nameof(Address1_PrimaryContactName)] = "address1_primarycontactname",
					[nameof(Address1_ShippingMethodCode)] = "address1_shippingmethodcode",
					[nameof(Address1_StateOrProvince)] = "address1_stateorprovince",
					[nameof(Address1_Telephone1)] = "address1_telephone1",
					[nameof(Address1_Telephone2)] = "address1_telephone2",
					[nameof(Address1_Telephone3)] = "address1_telephone3",
					[nameof(Address1_UPSZone)] = "address1_upszone",
					[nameof(Address1_UTCOffset)] = "address1_utcoffset",
					[nameof(Address2_AddressTypeCode)] = "address2_addresstypecode",
					[nameof(Address2_City)] = "address2_city",
					[nameof(Address2_Composite)] = "address2_composite",
					[nameof(Address2_Country)] = "address2_country",
					[nameof(Address2_County)] = "address2_county",
					[nameof(Address2_Fax)] = "address2_fax",
					[nameof(Address2_FreightTermsCode)] = "address2_freighttermscode",
					[nameof(Address2_Latitude)] = "address2_latitude",
					[nameof(Address2_Line1)] = "address2_line1",
					[nameof(Address2_Line2)] = "address2_line2",
					[nameof(Address2_Line3)] = "address2_line3",
					[nameof(Address2_Longitude)] = "address2_longitude",
					[nameof(Address2_Name)] = "address2_name",
					[nameof(Address2_PostalCode)] = "address2_postalcode",
					[nameof(Address2_PostOfficeBox)] = "address2_postofficebox",
					[nameof(Address2_PrimaryContactName)] = "address2_primarycontactname",
					[nameof(Address2_ShippingMethodCode)] = "address2_shippingmethodcode",
					[nameof(Address2_StateOrProvince)] = "address2_stateorprovince",
					[nameof(Address2_Telephone1)] = "address2_telephone1",
					[nameof(Address2_Telephone2)] = "address2_telephone2",
					[nameof(Address2_Telephone3)] = "address2_telephone3",
					[nameof(Address2_UPSZone)] = "address2_upszone",
					[nameof(Address2_UTCOffset)] = "address2_utcoffset",
					[nameof(Adx_CreatedByIPAddress)] = "adx_createdbyipaddress",
					[nameof(Adx_CreatedByUsername)] = "adx_createdbyusername",
					[nameof(Adx_ModifiedByIPAddress)] = "adx_modifiedbyipaddress",
					[nameof(Adx_ModifiedByUsername)] = "adx_modifiedbyusername",
					[nameof(Aging30)] = "aging30",
					[nameof(Aging30_Base)] = "aging30_base",
					[nameof(Aging60)] = "aging60",
					[nameof(Aging60_Base)] = "aging60_base",
					[nameof(Aging90)] = "aging90",
					[nameof(Aging90_Base)] = "aging90_base",
					[nameof(BusinessTypeCode)] = "businesstypecode",
					[nameof(Cr22a_SupportedLanguages)] = "cr22a_supportedlanguages",
					[nameof(CreatedBy)] = "createdby",
					[nameof(CreatedByExternalParty)] = "createdbyexternalparty",
					[nameof(CreatedOn)] = "createdon",
					[nameof(CreatedOnBehalfBy)] = "createdonbehalfby",
					[nameof(CreditLimit)] = "creditlimit",
					[nameof(CreditLimit_Base)] = "creditlimit_base",
					[nameof(CreditOnHold)] = "creditonhold",
					[nameof(CustomerSizeCode)] = "customersizecode",
					[nameof(CustomerTypeCode)] = "customertypecode",
					[nameof(Description)] = "description",
					[nameof(DoNotBulkEMail)] = "donotbulkemail",
					[nameof(DoNotBulkPostalMail)] = "donotbulkpostalmail",
					[nameof(DoNotEMail)] = "donotemail",
					[nameof(DoNotFax)] = "donotfax",
					[nameof(DoNotPhone)] = "donotphone",
					[nameof(DoNotPostalMail)] = "donotpostalmail",
					[nameof(DoNotSendMM)] = "donotsendmm",
					[nameof(EMailAddress1)] = "emailaddress1",
					[nameof(EMailAddress2)] = "emailaddress2",
					[nameof(EMailAddress3)] = "emailaddress3",
					[nameof(EntityImageId)] = "entityimageid",
					[nameof(ExchangeRate)] = "exchangerate",
					[nameof(Fax)] = "fax",
					[nameof(FollowEmail)] = "followemail",
					[nameof(FtpSiteURL)] = "ftpsiteurl",
					[nameof(ImportSequenceNumber)] = "importsequencenumber",
					[nameof(IndustryCode)] = "industrycode",
					[nameof(LastOnHoldTime)] = "lastonholdtime",
					[nameof(LastUsedInCampaign)] = "lastusedincampaign",
					[nameof(MarketCap)] = "marketcap",
					[nameof(MarketCap_Base)] = "marketcap_base",
					[nameof(MarketingOnly)] = "marketingonly",
					[nameof(MasterId)] = "masterid",
					[nameof(Merged)] = "merged",
					[nameof(ModifiedBy)] = "modifiedby",
					[nameof(ModifiedByExternalParty)] = "modifiedbyexternalparty",
					[nameof(ModifiedOn)] = "modifiedon",
					[nameof(ModifiedOnBehalfBy)] = "modifiedonbehalfby",
					[nameof(Msa_managingpartnerid)] = "msa_managingpartnerid",
					[nameof(Msft_DataState)] = "msft_datastate",
					[nameof(Name)] = "name",
					[nameof(NumberOfEmployees)] = "numberofemployees",
					[nameof(OnHoldTime)] = "onholdtime",
					[nameof(OverriddenCreatedOn)] = "overriddencreatedon",
					[nameof(OwnerId)] = "ownerid",
					[nameof(OwnershipCode)] = "ownershipcode",
					[nameof(OwningBusinessUnit)] = "owningbusinessunit",
					[nameof(OwningTeam)] = "owningteam",
					[nameof(OwningUser)] = "owninguser",
					[nameof(ParentAccountId)] = "parentaccountid",
					[nameof(ParticipatesInWorkflow)] = "participatesinworkflow",
					[nameof(PaymentTermsCode)] = "paymenttermscode",
					[nameof(PreferredAppointmentDayCode)] = "preferredappointmentdaycode",
					[nameof(PreferredAppointmentTimeCode)] = "preferredappointmenttimecode",
					[nameof(PreferredContactMethodCode)] = "preferredcontactmethodcode",
					[nameof(PreferredSystemUserId)] = "preferredsystemuserid",
					[nameof(PrimaryContactId)] = "primarycontactid",
					[nameof(PrimarySatoriId)] = "primarysatoriid",
					[nameof(PrimaryTwitterId)] = "primarytwitterid",
					[nameof(ProcessId)] = "processid",
					[nameof(Revenue)] = "revenue",
					[nameof(Revenue_Base)] = "revenue_base",
					[nameof(Rn_SuggestedFollowup)] = "rn_suggestedfollowup",
					[nameof(SharesOutstanding)] = "sharesoutstanding",
					[nameof(ShippingMethodCode)] = "shippingmethodcode",
					[nameof(SIC)] = "sic",
					[nameof(SLAId)] = "slaid",
					[nameof(SLAInvokedId)] = "slainvokedid",
					[nameof(StageId)] = "stageid",
					[nameof(StateCode)] = "statecode",
					[nameof(StatusCode)] = "statuscode",
					[nameof(StockExchange)] = "stockexchange",
					[nameof(Telephone1)] = "telephone1",
					[nameof(Telephone2)] = "telephone2",
					[nameof(Telephone3)] = "telephone3",
					[nameof(TerritoryCode)] = "territorycode",
					[nameof(TickerSymbol)] = "tickersymbol",
					[nameof(TimeSpentByMeOnEmailAndMeetings)] = "timespentbymeonemailandmeetings",
					[nameof(TimeZoneRuleVersionNumber)] = "timezoneruleversionnumber",
					[nameof(TransactionCurrencyId)] = "transactioncurrencyid",
					[nameof(TraversedPath)] = "traversedpath",
					[nameof(UTCConversionTimeZoneCode)] = "utcconversiontimezonecode",
					[nameof(VersionNumber)] = "versionnumber",
					[nameof(WebSiteURL)] = "websiteurl",
					[nameof(YomiName)] = "yominame",
				};

				public static bool TryGet(string memberName, out string logicalName)
					=> _fieldMap.TryGetValue(memberName, out logicalName);

				public static IEnumerable<string> GetAll() => _fieldMap.Values;

				public static string Get(string memberName)
					=> TryGet(memberName, out var logicalName)
						? logicalName
						: throw new ArgumentException("Invalid attribute logical name.", nameof(memberName));
			}

			public partial class OptionSets
			{
				/// <summary>
				/// Drop-down list for selecting the category of the account.
				/// </summary>
				[DataContract]
				public enum Category
				{
					[EnumMember] PreferredCustomer = 1,
					[EnumMember] Standard = 2,
				}
				/// <summary>
				/// Drop-down list for classifying an account.
				/// </summary>
				[DataContract]
				public enum Classification
				{
					[EnumMember] DefaultValue = 1,
				}
				/// <summary>
				/// Drop-down list for selecting account ratings.
				/// </summary>
				[DataContract]
				public enum AccountRating
				{
					[EnumMember] DefaultValue = 1,
				}
				/// <summary>
				/// Type of address for address 1, such as billing, shipping, or primary address.
				/// </summary>
				[DataContract]
				public enum Address1AddressType
				{
					[EnumMember] BillTo = 1,
					[EnumMember] ShipTo = 2,
					[EnumMember] Primary = 3,
					[EnumMember] Other = 4,
				}
				/// <summary>
				/// Freight terms for address 1.
				/// </summary>
				[DataContract]
				public enum Address1FreightTerms
				{
					[EnumMember] Fob = 1,
					[EnumMember] NoCharge = 2,
				}
				/// <summary>
				/// Method of shipment for address 1.
				/// </summary>
				[DataContract]
				public enum Address1ShippingMethod
				{
					[EnumMember] Airborne = 1,
					[EnumMember] Dhl = 2,
					[EnumMember] Fedex = 3,
					[EnumMember] Ups = 4,
					[EnumMember] PostalMail = 5,
					[EnumMember] FullLoad = 6,
					[EnumMember] WillCall = 7,
				}
				/// <summary>
				/// Type of address for address 2, such as billing, shipping, or primary address.
				/// </summary>
				[DataContract]
				public enum Address2AddressType
				{
					[EnumMember] DefaultValue = 1,
				}
				/// <summary>
				/// Freight terms for address 2.
				/// </summary>
				[DataContract]
				public enum Address2FreightTerms
				{
					[EnumMember] DefaultValue = 1,
				}
				/// <summary>
				/// Method of shipment for address 2.
				/// </summary>
				[DataContract]
				public enum Address2ShippingMethod
				{
					[EnumMember] DefaultValue = 1,
				}
				/// <summary>
				/// Type of business associated with the account.
				/// </summary>
				[DataContract]
				public enum BusinessType
				{
					[EnumMember] DefaultValue = 1,
				}
				[DataContract]
				public enum Supportedlanguages
				{
					[EnumMember] Fr = 125410000,
					[EnumMember] Nl = 125410001,
					[EnumMember] En = 125410002,
					[EnumMember] Es = 125410003,
					[EnumMember] De = 125410004,
				}
				/// <summary>
				/// Size of the account.
				/// </summary>
				[DataContract]
				public enum CustomerSize
				{
					[EnumMember] DefaultValue = 1,
				}
				/// <summary>
				/// Type of the account.
				/// </summary>
				[DataContract]
				public enum RelationshipType
				{
					[EnumMember] Competitor = 1,
					[EnumMember] Consultant = 2,
					[EnumMember] Customer = 3,
					[EnumMember] Investor = 4,
					[EnumMember] Partner = 5,
					[EnumMember] Influencer = 6,
					[EnumMember] Press = 7,
					[EnumMember] Prospect = 8,
					[EnumMember] Reseller = 9,
					[EnumMember] Supplier = 10,
					[EnumMember] Vendor = 11,
					[EnumMember] Other = 12,
				}
				/// <summary>
				/// Type of industry with which the account is associated.
				/// </summary>
				[DataContract]
				public enum Industry
				{
					[EnumMember] Accounting = 1,
					[EnumMember] AgricultureAndNonPetrolNaturalResourceExtraction = 2,
					[EnumMember] BroadcastingPrintingAndPublishing = 3,
					[EnumMember] Brokers = 4,
					[EnumMember] BuildingSupplyRetail = 5,
					[EnumMember] BusinessServices = 6,
					[EnumMember] Consulting = 7,
					[EnumMember] ConsumerServices = 8,
					[EnumMember] DesignDirectionAndCreativeManagement = 9,
					[EnumMember] DistributorsDispatchersAndProcessors = 10,
					[EnumMember] DoctorSOfficesAndClinics = 11,
					[EnumMember] DurableManufacturing = 12,
					[EnumMember] EatingAndDrinkingPlaces = 13,
					[EnumMember] EntertainmentRetail = 14,
					[EnumMember] EquipmentRentalAndLeasing = 15,
					[EnumMember] Financial = 16,
					[EnumMember] FoodAndTobaccoProcessing = 17,
					[EnumMember] InboundCapitalIntensiveProcessing = 18,
					[EnumMember] InboundRepairAndServices = 19,
					[EnumMember] Insurance = 20,
					[EnumMember] LegalServices = 21,
					[EnumMember] NonDurableMerchandiseRetail = 22,
					[EnumMember] OutboundConsumerService = 23,
					[EnumMember] PetrochemicalExtractionAndDistribution = 24,
					[EnumMember] ServiceRetail = 25,
					[EnumMember] SigAffiliations = 26,
					[EnumMember] SocialServices = 27,
					[EnumMember] SpecialOutboundTradeContractors = 28,
					[EnumMember] SpecialtyRealty = 29,
					[EnumMember] Transportation = 30,
					[EnumMember] UtilityCreationAndDistribution = 31,
					[EnumMember] VehicleRetail = 32,
					[EnumMember] Wholesale = 33,
				}
				[DataContract]
				public enum Datastate
				{
					[EnumMember] Default = 0,
					[EnumMember] Retain = 1,
				}
				/// <summary>
				/// Type of company ownership, such as public or private.
				/// </summary>
				[DataContract]
				public enum Ownership
				{
					[EnumMember] Public = 1,
					[EnumMember] Private = 2,
					[EnumMember] Subsidiary = 3,
					[EnumMember] Other = 4,
				}
				/// <summary>
				/// Payment terms for the account.
				/// </summary>
				[DataContract]
				public enum PaymentTerms
				{
					[EnumMember] Net30 = 1,
					[EnumMember] _210Net30 = 2,
					[EnumMember] Net45 = 3,
					[EnumMember] Net60 = 4,
				}
				/// <summary>
				/// Day of the week that the account prefers for scheduling service activities.
				/// </summary>
				[DataContract]
				public enum PreferredDay
				{
					[EnumMember] Sunday = 0,
					[EnumMember] Monday = 1,
					[EnumMember] Tuesday = 2,
					[EnumMember] Wednesday = 3,
					[EnumMember] Thursday = 4,
					[EnumMember] Friday = 5,
					[EnumMember] Saturday = 6,
				}
				/// <summary>
				/// Time of day that the account prefers for scheduling service activities.
				/// </summary>
				[DataContract]
				public enum PreferredTime
				{
					[EnumMember] Morning = 1,
					[EnumMember] Afternoon = 2,
					[EnumMember] Evening = 3,
				}
				/// <summary>
				/// Preferred contact method for the account.
				/// </summary>
				[DataContract]
				public enum PreferredMethodOfContact
				{
					[EnumMember] Any = 1,
					[EnumMember] Email = 2,
					[EnumMember] Phone = 3,
					[EnumMember] Fax = 4,
					[EnumMember] Mail = 5,
				}
				/// <summary>
				/// Method of shipment for the account.
				/// </summary>
				[DataContract]
				public enum ShippingMethod
				{
					[EnumMember] DefaultValue = 1,
				}
				/// <summary>
				/// Status of the account.
				/// </summary>
				[DataContract]
				public enum Status
				{
					[EnumMember] Active = 0,
					[EnumMember] Inactive = 1,
				}
				/// <summary>
				/// Reason for the status of the account.
				/// </summary>
				[DataContract]
				public enum StatusReason
				{
					[EnumMember] Active = 1,
					[EnumMember] Inactive = 2,
				}
				/// <summary>
				/// Territory to which the account belongs.
				/// </summary>
				[DataContract]
				public enum TerritoryCode
				{
					[EnumMember] DefaultValue = 1,
				}
			}
		}

		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("accountcategorycode")]
		public Account.Meta.OptionSets.Category? AccountCategoryCode
		{
			get => TryGetAttributeValue("accountcategorycode", out OptionSetValue opt) && opt != null ? (Account.Meta.OptionSets.Category?)opt.Value : null;
			set => this["accountcategorycode"] = value == null ? null : new OptionSetValue((int)value);
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("accountclassificationcode")]
		public Account.Meta.OptionSets.Classification? AccountClassificationCode
		{
			get => TryGetAttributeValue("accountclassificationcode", out OptionSetValue opt) && opt != null ? (Account.Meta.OptionSets.Classification?)opt.Value : null;
			set => this["accountclassificationcode"] = value == null ? null : new OptionSetValue((int)value);
		}
		/// <summary>
		/// Max Length: 20<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("accountnumber")]
		public string AccountNumber
		{
			get => TryGetAttributeValue("accountnumber", out string value) ? value : null;
			set => this["accountnumber"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("accountratingcode")]
		public Account.Meta.OptionSets.AccountRating? AccountRatingCode
		{
			get => TryGetAttributeValue("accountratingcode", out OptionSetValue opt) && opt != null ? (Account.Meta.OptionSets.AccountRating?)opt.Value : null;
			set => this["accountratingcode"] = value == null ? null : new OptionSetValue((int)value);
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address1_addresstypecode")]
		public Account.Meta.OptionSets.Address1AddressType? Address1_AddressTypeCode
		{
			get => TryGetAttributeValue("address1_addresstypecode", out OptionSetValue opt) && opt != null ? (Account.Meta.OptionSets.Address1AddressType?)opt.Value : null;
			set => this["address1_addresstypecode"] = value == null ? null : new OptionSetValue((int)value);
		}
		/// <summary>
		/// Max Length: 80<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address1_city")]
		public string Address1_City
		{
			get => TryGetAttributeValue("address1_city", out string value) ? value : null;
			set => this["address1_city"] = value;
		}
		/// <summary>
		/// Max Length: 1000<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("address1_composite")]
		public string Address1_Composite
		{
			get => TryGetAttributeValue("address1_composite", out string value) ? value : null;
		}
		/// <summary>
		/// Max Length: 80<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address1_country")]
		public string Address1_Country
		{
			get => TryGetAttributeValue("address1_country", out string value) ? value : null;
			set => this["address1_country"] = value;
		}
		/// <summary>
		/// Max Length: 50<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address1_county")]
		public string Address1_County
		{
			get => TryGetAttributeValue("address1_county", out string value) ? value : null;
			set => this["address1_county"] = value;
		}
		/// <summary>
		/// Max Length: 50<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address1_fax")]
		public string Address1_Fax
		{
			get => TryGetAttributeValue("address1_fax", out string value) ? value : null;
			set => this["address1_fax"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address1_freighttermscode")]
		public Account.Meta.OptionSets.Address1FreightTerms? Address1_FreightTermsCode
		{
			get => TryGetAttributeValue("address1_freighttermscode", out OptionSetValue opt) && opt != null ? (Account.Meta.OptionSets.Address1FreightTerms?)opt.Value : null;
			set => this["address1_freighttermscode"] = value == null ? null : new OptionSetValue((int)value);
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address1_latitude")]
		public double? Address1_Latitude
		{
			get => TryGetAttributeValue("address1_latitude", out double? value) ? value : null;
			set => this["address1_latitude"] = value;
		}
		/// <summary>
		/// Max Length: 250<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address1_line1")]
		public string Address1_Line1
		{
			get => TryGetAttributeValue("address1_line1", out string value) ? value : null;
			set => this["address1_line1"] = value;
		}
		/// <summary>
		/// Max Length: 250<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address1_line2")]
		public string Address1_Line2
		{
			get => TryGetAttributeValue("address1_line2", out string value) ? value : null;
			set => this["address1_line2"] = value;
		}
		/// <summary>
		/// Max Length: 250<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address1_line3")]
		public string Address1_Line3
		{
			get => TryGetAttributeValue("address1_line3", out string value) ? value : null;
			set => this["address1_line3"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address1_longitude")]
		public double? Address1_Longitude
		{
			get => TryGetAttributeValue("address1_longitude", out double? value) ? value : null;
			set => this["address1_longitude"] = value;
		}
		/// <summary>
		/// Max Length: 200<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address1_name")]
		public string Address1_Name
		{
			get => TryGetAttributeValue("address1_name", out string value) ? value : null;
			set => this["address1_name"] = value;
		}
		/// <summary>
		/// Max Length: 20<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address1_postalcode")]
		public string Address1_PostalCode
		{
			get => TryGetAttributeValue("address1_postalcode", out string value) ? value : null;
			set => this["address1_postalcode"] = value;
		}
		/// <summary>
		/// Max Length: 20<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address1_postofficebox")]
		public string Address1_PostOfficeBox
		{
			get => TryGetAttributeValue("address1_postofficebox", out string value) ? value : null;
			set => this["address1_postofficebox"] = value;
		}
		/// <summary>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address1_primarycontactname")]
		public string Address1_PrimaryContactName
		{
			get => TryGetAttributeValue("address1_primarycontactname", out string value) ? value : null;
			set => this["address1_primarycontactname"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address1_shippingmethodcode")]
		public Account.Meta.OptionSets.Address1ShippingMethod? Address1_ShippingMethodCode
		{
			get => TryGetAttributeValue("address1_shippingmethodcode", out OptionSetValue opt) && opt != null ? (Account.Meta.OptionSets.Address1ShippingMethod?)opt.Value : null;
			set => this["address1_shippingmethodcode"] = value == null ? null : new OptionSetValue((int)value);
		}
		/// <summary>
		/// Max Length: 50<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address1_stateorprovince")]
		public string Address1_StateOrProvince
		{
			get => TryGetAttributeValue("address1_stateorprovince", out string value) ? value : null;
			set => this["address1_stateorprovince"] = value;
		}
		/// <summary>
		/// Max Length: 50<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address1_telephone1")]
		public string Address1_Telephone1
		{
			get => TryGetAttributeValue("address1_telephone1", out string value) ? value : null;
			set => this["address1_telephone1"] = value;
		}
		/// <summary>
		/// Max Length: 50<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address1_telephone2")]
		public string Address1_Telephone2
		{
			get => TryGetAttributeValue("address1_telephone2", out string value) ? value : null;
			set => this["address1_telephone2"] = value;
		}
		/// <summary>
		/// Max Length: 50<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address1_telephone3")]
		public string Address1_Telephone3
		{
			get => TryGetAttributeValue("address1_telephone3", out string value) ? value : null;
			set => this["address1_telephone3"] = value;
		}
		/// <summary>
		/// Max Length: 4<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address1_upszone")]
		public string Address1_UPSZone
		{
			get => TryGetAttributeValue("address1_upszone", out string value) ? value : null;
			set => this["address1_upszone"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address1_utcoffset")]
		public int? Address1_UTCOffset
		{
			get => TryGetAttributeValue("address1_utcoffset", out int? value) ? value : null;
			set => this["address1_utcoffset"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address2_addresstypecode")]
		public Account.Meta.OptionSets.Address2AddressType? Address2_AddressTypeCode
		{
			get => TryGetAttributeValue("address2_addresstypecode", out OptionSetValue opt) && opt != null ? (Account.Meta.OptionSets.Address2AddressType?)opt.Value : null;
			set => this["address2_addresstypecode"] = value == null ? null : new OptionSetValue((int)value);
		}
		/// <summary>
		/// Max Length: 80<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address2_city")]
		public string Address2_City
		{
			get => TryGetAttributeValue("address2_city", out string value) ? value : null;
			set => this["address2_city"] = value;
		}
		/// <summary>
		/// Max Length: 1000<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("address2_composite")]
		public string Address2_Composite
		{
			get => TryGetAttributeValue("address2_composite", out string value) ? value : null;
		}
		/// <summary>
		/// Max Length: 80<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address2_country")]
		public string Address2_Country
		{
			get => TryGetAttributeValue("address2_country", out string value) ? value : null;
			set => this["address2_country"] = value;
		}
		/// <summary>
		/// Max Length: 50<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address2_county")]
		public string Address2_County
		{
			get => TryGetAttributeValue("address2_county", out string value) ? value : null;
			set => this["address2_county"] = value;
		}
		/// <summary>
		/// Max Length: 50<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address2_fax")]
		public string Address2_Fax
		{
			get => TryGetAttributeValue("address2_fax", out string value) ? value : null;
			set => this["address2_fax"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address2_freighttermscode")]
		public Account.Meta.OptionSets.Address2FreightTerms? Address2_FreightTermsCode
		{
			get => TryGetAttributeValue("address2_freighttermscode", out OptionSetValue opt) && opt != null ? (Account.Meta.OptionSets.Address2FreightTerms?)opt.Value : null;
			set => this["address2_freighttermscode"] = value == null ? null : new OptionSetValue((int)value);
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address2_latitude")]
		public double? Address2_Latitude
		{
			get => TryGetAttributeValue("address2_latitude", out double? value) ? value : null;
			set => this["address2_latitude"] = value;
		}
		/// <summary>
		/// Max Length: 250<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address2_line1")]
		public string Address2_Line1
		{
			get => TryGetAttributeValue("address2_line1", out string value) ? value : null;
			set => this["address2_line1"] = value;
		}
		/// <summary>
		/// Max Length: 250<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address2_line2")]
		public string Address2_Line2
		{
			get => TryGetAttributeValue("address2_line2", out string value) ? value : null;
			set => this["address2_line2"] = value;
		}
		/// <summary>
		/// Max Length: 250<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address2_line3")]
		public string Address2_Line3
		{
			get => TryGetAttributeValue("address2_line3", out string value) ? value : null;
			set => this["address2_line3"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address2_longitude")]
		public double? Address2_Longitude
		{
			get => TryGetAttributeValue("address2_longitude", out double? value) ? value : null;
			set => this["address2_longitude"] = value;
		}
		/// <summary>
		/// Max Length: 200<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address2_name")]
		public string Address2_Name
		{
			get => TryGetAttributeValue("address2_name", out string value) ? value : null;
			set => this["address2_name"] = value;
		}
		/// <summary>
		/// Max Length: 20<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address2_postalcode")]
		public string Address2_PostalCode
		{
			get => TryGetAttributeValue("address2_postalcode", out string value) ? value : null;
			set => this["address2_postalcode"] = value;
		}
		/// <summary>
		/// Max Length: 20<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address2_postofficebox")]
		public string Address2_PostOfficeBox
		{
			get => TryGetAttributeValue("address2_postofficebox", out string value) ? value : null;
			set => this["address2_postofficebox"] = value;
		}
		/// <summary>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address2_primarycontactname")]
		public string Address2_PrimaryContactName
		{
			get => TryGetAttributeValue("address2_primarycontactname", out string value) ? value : null;
			set => this["address2_primarycontactname"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address2_shippingmethodcode")]
		public Account.Meta.OptionSets.Address2ShippingMethod? Address2_ShippingMethodCode
		{
			get => TryGetAttributeValue("address2_shippingmethodcode", out OptionSetValue opt) && opt != null ? (Account.Meta.OptionSets.Address2ShippingMethod?)opt.Value : null;
			set => this["address2_shippingmethodcode"] = value == null ? null : new OptionSetValue((int)value);
		}
		/// <summary>
		/// Max Length: 50<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address2_stateorprovince")]
		public string Address2_StateOrProvince
		{
			get => TryGetAttributeValue("address2_stateorprovince", out string value) ? value : null;
			set => this["address2_stateorprovince"] = value;
		}
		/// <summary>
		/// Max Length: 50<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address2_telephone1")]
		public string Address2_Telephone1
		{
			get => TryGetAttributeValue("address2_telephone1", out string value) ? value : null;
			set => this["address2_telephone1"] = value;
		}
		/// <summary>
		/// Max Length: 50<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address2_telephone2")]
		public string Address2_Telephone2
		{
			get => TryGetAttributeValue("address2_telephone2", out string value) ? value : null;
			set => this["address2_telephone2"] = value;
		}
		/// <summary>
		/// Max Length: 50<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address2_telephone3")]
		public string Address2_Telephone3
		{
			get => TryGetAttributeValue("address2_telephone3", out string value) ? value : null;
			set => this["address2_telephone3"] = value;
		}
		/// <summary>
		/// Max Length: 4<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address2_upszone")]
		public string Address2_UPSZone
		{
			get => TryGetAttributeValue("address2_upszone", out string value) ? value : null;
			set => this["address2_upszone"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address2_utcoffset")]
		public int? Address2_UTCOffset
		{
			get => TryGetAttributeValue("address2_utcoffset", out int? value) ? value : null;
			set => this["address2_utcoffset"] = value;
		}
		/// <summary>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("adx_createdbyipaddress")]
		public string Adx_CreatedByIPAddress
		{
			get => TryGetAttributeValue("adx_createdbyipaddress", out string value) ? value : null;
			set => this["adx_createdbyipaddress"] = value;
		}
		/// <summary>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("adx_createdbyusername")]
		public string Adx_CreatedByUsername
		{
			get => TryGetAttributeValue("adx_createdbyusername", out string value) ? value : null;
			set => this["adx_createdbyusername"] = value;
		}
		/// <summary>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("adx_modifiedbyipaddress")]
		public string Adx_ModifiedByIPAddress
		{
			get => TryGetAttributeValue("adx_modifiedbyipaddress", out string value) ? value : null;
			set => this["adx_modifiedbyipaddress"] = value;
		}
		/// <summary>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("adx_modifiedbyusername")]
		public string Adx_ModifiedByUsername
		{
			get => TryGetAttributeValue("adx_modifiedbyusername", out string value) ? value : null;
			set => this["adx_modifiedbyusername"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("aging30")]
		public decimal? Aging30
		{
			get => TryGetAttributeValue("aging30", out Money money) ? (decimal?)money.Value : null;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("aging30_base")]
		public decimal? Aging30_Base
		{
			get => TryGetAttributeValue("aging30_base", out Money money) ? (decimal?)money.Value : null;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("aging60")]
		public decimal? Aging60
		{
			get => TryGetAttributeValue("aging60", out Money money) ? (decimal?)money.Value : null;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("aging60_base")]
		public decimal? Aging60_Base
		{
			get => TryGetAttributeValue("aging60_base", out Money money) ? (decimal?)money.Value : null;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("aging90")]
		public decimal? Aging90
		{
			get => TryGetAttributeValue("aging90", out Money money) ? (decimal?)money.Value : null;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("aging90_base")]
		public decimal? Aging90_Base
		{
			get => TryGetAttributeValue("aging90_base", out Money money) ? (decimal?)money.Value : null;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("businesstypecode")]
		public Account.Meta.OptionSets.BusinessType? BusinessTypeCode
		{
			get => TryGetAttributeValue("businesstypecode", out OptionSetValue opt) && opt != null ? (Account.Meta.OptionSets.BusinessType?)opt.Value : null;
			set => this["businesstypecode"] = value == null ? null : new OptionSetValue((int)value);
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("cr22a_supportedlanguages")]
		public IEnumerable<Account.Meta.OptionSets.Supportedlanguages> Cr22a_SupportedLanguages
		{
			get => TryGetAttributeValue("cr22a_supportedlanguages", out OptionSetValueCollection opts) && opts != null ? opts.Select(opt => (Account.Meta.OptionSets.Supportedlanguages)opt.Value) : Array.Empty<Account.Meta.OptionSets.Supportedlanguages>();
			set => this["cr22a_supportedlanguages"] = value == null || !value.Any() ? null : new OptionSetValueCollection(value.Select(each => new OptionSetValue((int)each)).ToList());
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
		/// Targets: externalparty<br/>
		/// </summary>
		[AttributeLogicalName("createdbyexternalparty")]
		public EntityReference CreatedByExternalParty
		{
			get => TryGetAttributeValue("createdbyexternalparty", out EntityReference value) ? value : null;
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
		[AttributeLogicalName("creditlimit")]
		public decimal? CreditLimit
		{
			get => TryGetAttributeValue("creditlimit", out Money money) ? (decimal?)money.Value : null;
			set => this["creditlimit"] = value.HasValue ? new Money(value.Value) : null;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("creditlimit_base")]
		public decimal? CreditLimit_Base
		{
			get => TryGetAttributeValue("creditlimit_base", out Money money) ? (decimal?)money.Value : null;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("creditonhold")]
		public bool? CreditOnHold
		{
			get => TryGetAttributeValue("creditonhold", out bool? value) ? value : null;
			set => this["creditonhold"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("customersizecode")]
		public Account.Meta.OptionSets.CustomerSize? CustomerSizeCode
		{
			get => TryGetAttributeValue("customersizecode", out OptionSetValue opt) && opt != null ? (Account.Meta.OptionSets.CustomerSize?)opt.Value : null;
			set => this["customersizecode"] = value == null ? null : new OptionSetValue((int)value);
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("customertypecode")]
		public Account.Meta.OptionSets.RelationshipType? CustomerTypeCode
		{
			get => TryGetAttributeValue("customertypecode", out OptionSetValue opt) && opt != null ? (Account.Meta.OptionSets.RelationshipType?)opt.Value : null;
			set => this["customertypecode"] = value == null ? null : new OptionSetValue((int)value);
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
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("donotbulkemail")]
		public bool? DoNotBulkEMail
		{
			get => TryGetAttributeValue("donotbulkemail", out bool? value) ? value : null;
			set => this["donotbulkemail"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("donotbulkpostalmail")]
		public bool? DoNotBulkPostalMail
		{
			get => TryGetAttributeValue("donotbulkpostalmail", out bool? value) ? value : null;
			set => this["donotbulkpostalmail"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("donotemail")]
		public bool? DoNotEMail
		{
			get => TryGetAttributeValue("donotemail", out bool? value) ? value : null;
			set => this["donotemail"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("donotfax")]
		public bool? DoNotFax
		{
			get => TryGetAttributeValue("donotfax", out bool? value) ? value : null;
			set => this["donotfax"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("donotphone")]
		public bool? DoNotPhone
		{
			get => TryGetAttributeValue("donotphone", out bool? value) ? value : null;
			set => this["donotphone"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("donotpostalmail")]
		public bool? DoNotPostalMail
		{
			get => TryGetAttributeValue("donotpostalmail", out bool? value) ? value : null;
			set => this["donotpostalmail"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("donotsendmm")]
		public bool? DoNotSendMM
		{
			get => TryGetAttributeValue("donotsendmm", out bool? value) ? value : null;
			set => this["donotsendmm"] = value;
		}
		/// <summary>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("emailaddress1")]
		public string EMailAddress1
		{
			get => TryGetAttributeValue("emailaddress1", out string value) ? value : null;
			set => this["emailaddress1"] = value;
		}
		/// <summary>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("emailaddress2")]
		public string EMailAddress2
		{
			get => TryGetAttributeValue("emailaddress2", out string value) ? value : null;
			set => this["emailaddress2"] = value;
		}
		/// <summary>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("emailaddress3")]
		public string EMailAddress3
		{
			get => TryGetAttributeValue("emailaddress3", out string value) ? value : null;
			set => this["emailaddress3"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("entityimageid")]
		public Guid? EntityImageId
		{
			get => TryGetAttributeValue("entityimageid", out Guid? value) ? value : null;
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
		/// Max Length: 50<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("fax")]
		public string Fax
		{
			get => TryGetAttributeValue("fax", out string value) ? value : null;
			set => this["fax"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("followemail")]
		public bool? FollowEmail
		{
			get => TryGetAttributeValue("followemail", out bool? value) ? value : null;
			set => this["followemail"] = value;
		}
		/// <summary>
		/// Max Length: 200<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("ftpsiteurl")]
		public string FtpSiteURL
		{
			get => TryGetAttributeValue("ftpsiteurl", out string value) ? value : null;
			set => this["ftpsiteurl"] = value;
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
		[AttributeLogicalName("industrycode")]
		public Account.Meta.OptionSets.Industry? IndustryCode
		{
			get => TryGetAttributeValue("industrycode", out OptionSetValue opt) && opt != null ? (Account.Meta.OptionSets.Industry?)opt.Value : null;
			set => this["industrycode"] = value == null ? null : new OptionSetValue((int)value);
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
		/// Valid for: Update Read<br/>
		/// </summary>
		[AttributeLogicalName("lastusedincampaign")]
		public DateTime? LastUsedInCampaign
		{
			get => TryGetAttributeValue("lastusedincampaign", out DateTime? value) ? value : null;
			set => this["lastusedincampaign"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("marketcap")]
		public decimal? MarketCap
		{
			get => TryGetAttributeValue("marketcap", out Money money) ? (decimal?)money.Value : null;
			set => this["marketcap"] = value.HasValue ? new Money(value.Value) : null;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("marketcap_base")]
		public decimal? MarketCap_Base
		{
			get => TryGetAttributeValue("marketcap_base", out Money money) ? (decimal?)money.Value : null;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("marketingonly")]
		public bool? MarketingOnly
		{
			get => TryGetAttributeValue("marketingonly", out bool? value) ? value : null;
			set => this["marketingonly"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// Targets: account<br/>
		/// </summary>
		[AttributeLogicalName("masterid")]
		public EntityReference MasterId
		{
			get => TryGetAttributeValue("masterid", out EntityReference value) ? value : null;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("merged")]
		public bool? Merged
		{
			get => TryGetAttributeValue("merged", out bool? value) ? value : null;
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
		/// Targets: externalparty<br/>
		/// </summary>
		[AttributeLogicalName("modifiedbyexternalparty")]
		public EntityReference ModifiedByExternalParty
		{
			get => TryGetAttributeValue("modifiedbyexternalparty", out EntityReference value) ? value : null;
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
		/// Valid for: Create Update Read<br/>
		/// Targets: account<br/>
		/// </summary>
		[AttributeLogicalName("msa_managingpartnerid")]
		public EntityReference Msa_managingpartnerid
		{
			get => TryGetAttributeValue("msa_managingpartnerid", out EntityReference value) ? value : null;
			set
			{
				if (!Account.Meta.Fields.Msa_managingpartneridTargets.Contains(value.LogicalName))
				{
					throw new InvalidPluginExecutionException($"{value.LogicalName}:{value.Id} is not a valid Msa_managingpartnerid. The only valid references are account");			
				}
				this["msa_managingpartnerid"] = value;
			}
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("msft_datastate")]
		public Account.Meta.OptionSets.Datastate? Msft_DataState
		{
			get => TryGetAttributeValue("msft_datastate", out OptionSetValue opt) && opt != null ? (Account.Meta.OptionSets.Datastate?)opt.Value : null;
		}
		/// <summary>
		/// Max Length: 160<br/>
		/// Required Level: ApplicationRequired<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("name")]
		public string Name
		{
			get => TryGetAttributeValue("name", out string value) ? value : null;
			set => this["name"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("numberofemployees")]
		public int? NumberOfEmployees
		{
			get => TryGetAttributeValue("numberofemployees", out int? value) ? value : null;
			set => this["numberofemployees"] = value;
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
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("ownershipcode")]
		public Account.Meta.OptionSets.Ownership? OwnershipCode
		{
			get => TryGetAttributeValue("ownershipcode", out OptionSetValue opt) && opt != null ? (Account.Meta.OptionSets.Ownership?)opt.Value : null;
			set => this["ownershipcode"] = value == null ? null : new OptionSetValue((int)value);
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
		/// Targets: account<br/>
		/// </summary>
		[AttributeLogicalName("parentaccountid")]
		public EntityReference ParentAccountId
		{
			get => TryGetAttributeValue("parentaccountid", out EntityReference value) ? value : null;
			set
			{
				if (!Account.Meta.Fields.ParentAccountIdTargets.Contains(value.LogicalName))
				{
					throw new InvalidPluginExecutionException($"{value.LogicalName}:{value.Id} is not a valid ParentAccountId. The only valid references are account");			
				}
				this["parentaccountid"] = value;
			}
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("participatesinworkflow")]
		public bool? ParticipatesInWorkflow
		{
			get => TryGetAttributeValue("participatesinworkflow", out bool? value) ? value : null;
			set => this["participatesinworkflow"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("paymenttermscode")]
		public Account.Meta.OptionSets.PaymentTerms? PaymentTermsCode
		{
			get => TryGetAttributeValue("paymenttermscode", out OptionSetValue opt) && opt != null ? (Account.Meta.OptionSets.PaymentTerms?)opt.Value : null;
			set => this["paymenttermscode"] = value == null ? null : new OptionSetValue((int)value);
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("preferredappointmentdaycode")]
		public Account.Meta.OptionSets.PreferredDay? PreferredAppointmentDayCode
		{
			get => TryGetAttributeValue("preferredappointmentdaycode", out OptionSetValue opt) && opt != null ? (Account.Meta.OptionSets.PreferredDay?)opt.Value : null;
			set => this["preferredappointmentdaycode"] = value == null ? null : new OptionSetValue((int)value);
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("preferredappointmenttimecode")]
		public Account.Meta.OptionSets.PreferredTime? PreferredAppointmentTimeCode
		{
			get => TryGetAttributeValue("preferredappointmenttimecode", out OptionSetValue opt) && opt != null ? (Account.Meta.OptionSets.PreferredTime?)opt.Value : null;
			set => this["preferredappointmenttimecode"] = value == null ? null : new OptionSetValue((int)value);
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("preferredcontactmethodcode")]
		public Account.Meta.OptionSets.PreferredMethodOfContact? PreferredContactMethodCode
		{
			get => TryGetAttributeValue("preferredcontactmethodcode", out OptionSetValue opt) && opt != null ? (Account.Meta.OptionSets.PreferredMethodOfContact?)opt.Value : null;
			set => this["preferredcontactmethodcode"] = value == null ? null : new OptionSetValue((int)value);
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// Targets: systemuser<br/>
		/// </summary>
		[AttributeLogicalName("preferredsystemuserid")]
		public EntityReference PreferredSystemUserId
		{
			get => TryGetAttributeValue("preferredsystemuserid", out EntityReference value) ? value : null;
			set
			{
				if (!Account.Meta.Fields.PreferredSystemUserIdTargets.Contains(value.LogicalName))
				{
					throw new InvalidPluginExecutionException($"{value.LogicalName}:{value.Id} is not a valid PreferredSystemUserId. The only valid references are systemuser");			
				}
				this["preferredsystemuserid"] = value;
			}
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// Targets: contact<br/>
		/// </summary>
		[AttributeLogicalName("primarycontactid")]
		public EntityReference PrimaryContactId
		{
			get => TryGetAttributeValue("primarycontactid", out EntityReference value) ? value : null;
			set
			{
				if (!Account.Meta.Fields.PrimaryContactIdTargets.Contains(value.LogicalName))
				{
					throw new InvalidPluginExecutionException($"{value.LogicalName}:{value.Id} is not a valid PrimaryContactId. The only valid references are contact");			
				}
				this["primarycontactid"] = value;
			}
		}
		/// <summary>
		/// Max Length: 200<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("primarysatoriid")]
		public string PrimarySatoriId
		{
			get => TryGetAttributeValue("primarysatoriid", out string value) ? value : null;
			set => this["primarysatoriid"] = value;
		}
		/// <summary>
		/// Max Length: 128<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("primarytwitterid")]
		public string PrimaryTwitterId
		{
			get => TryGetAttributeValue("primarytwitterid", out string value) ? value : null;
			set => this["primarytwitterid"] = value;
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
		/// </summary>
		[AttributeLogicalName("revenue")]
		public decimal? Revenue
		{
			get => TryGetAttributeValue("revenue", out Money money) ? (decimal?)money.Value : null;
			set => this["revenue"] = value.HasValue ? new Money(value.Value) : null;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("revenue_base")]
		public decimal? Revenue_Base
		{
			get => TryGetAttributeValue("revenue_base", out Money money) ? (decimal?)money.Value : null;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("rn_suggestedfollowup")]
		public DateTime? Rn_SuggestedFollowup
		{
			get => TryGetAttributeValue("rn_suggestedfollowup", out DateTime? value) ? value : null;
			set => this["rn_suggestedfollowup"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("sharesoutstanding")]
		public int? SharesOutstanding
		{
			get => TryGetAttributeValue("sharesoutstanding", out int? value) ? value : null;
			set => this["sharesoutstanding"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("shippingmethodcode")]
		public Account.Meta.OptionSets.ShippingMethod? ShippingMethodCode
		{
			get => TryGetAttributeValue("shippingmethodcode", out OptionSetValue opt) && opt != null ? (Account.Meta.OptionSets.ShippingMethod?)opt.Value : null;
			set => this["shippingmethodcode"] = value == null ? null : new OptionSetValue((int)value);
		}
		/// <summary>
		/// Max Length: 20<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("sic")]
		public string SIC
		{
			get => TryGetAttributeValue("sic", out string value) ? value : null;
			set => this["sic"] = value;
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
				if (!Account.Meta.Fields.SLAIdTargets.Contains(value.LogicalName))
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
		public Account.Meta.OptionSets.Status? StateCode
		{
			get => TryGetAttributeValue("statecode", out OptionSetValue opt) && opt != null ? (Account.Meta.OptionSets.Status?)Enum.ToObject(typeof(Account.Meta.OptionSets.Status), opt.Value) : null;
			set => this["statecode"] = value == null ? null : new OptionSetValue(((IConvertible)value).ToInt32((IFormatProvider)CultureInfo.InvariantCulture));
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("statuscode")]
		public Account.Meta.OptionSets.StatusReason? StatusCode
		{
			get => TryGetAttributeValue("statuscode", out OptionSetValue opt) && opt != null ? (Account.Meta.OptionSets.StatusReason?)Enum.ToObject(typeof(Account.Meta.OptionSets.StatusReason), opt.Value) : null;
			set => this["statuscode"] = value == null ? null : new OptionSetValue(((IConvertible)value).ToInt32((IFormatProvider)CultureInfo.InvariantCulture));
		}
		/// <summary>
		/// Max Length: 20<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("stockexchange")]
		public string StockExchange
		{
			get => TryGetAttributeValue("stockexchange", out string value) ? value : null;
			set => this["stockexchange"] = value;
		}
		/// <summary>
		/// Max Length: 50<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("telephone1")]
		public string Telephone1
		{
			get => TryGetAttributeValue("telephone1", out string value) ? value : null;
			set => this["telephone1"] = value;
		}
		/// <summary>
		/// Max Length: 50<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("telephone2")]
		public string Telephone2
		{
			get => TryGetAttributeValue("telephone2", out string value) ? value : null;
			set => this["telephone2"] = value;
		}
		/// <summary>
		/// Max Length: 50<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("telephone3")]
		public string Telephone3
		{
			get => TryGetAttributeValue("telephone3", out string value) ? value : null;
			set => this["telephone3"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("territorycode")]
		public Account.Meta.OptionSets.TerritoryCode? TerritoryCode
		{
			get => TryGetAttributeValue("territorycode", out OptionSetValue opt) && opt != null ? (Account.Meta.OptionSets.TerritoryCode?)opt.Value : null;
			set => this["territorycode"] = value == null ? null : new OptionSetValue((int)value);
		}
		/// <summary>
		/// Max Length: 10<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("tickersymbol")]
		public string TickerSymbol
		{
			get => TryGetAttributeValue("tickersymbol", out string value) ? value : null;
			set => this["tickersymbol"] = value;
		}
		/// <summary>
		/// Max Length: 1250<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("timespentbymeonemailandmeetings")]
		public string TimeSpentByMeOnEmailAndMeetings
		{
			get => TryGetAttributeValue("timespentbymeonemailandmeetings", out string value) ? value : null;
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
				if (!Account.Meta.Fields.TransactionCurrencyIdTargets.Contains(value.LogicalName))
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
		/// <summary>
		/// Max Length: 200<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("websiteurl")]
		public string WebSiteURL
		{
			get => TryGetAttributeValue("websiteurl", out string value) ? value : null;
			set => this["websiteurl"] = value;
		}
		/// <summary>
		/// Max Length: 160<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("yominame")]
		public string YomiName
		{
			get => TryGetAttributeValue("yominame", out string value) ? value : null;
			set => this["yominame"] = value;
		}
		public Account() : base(Meta.EntityLogicalName) { }
		public Account(Guid id) : base(Meta.EntityLogicalName, id) { }
		public Account(string keyName, object keyValue) : base(Meta.EntityLogicalName, keyName, keyValue) { }
		public Account(KeyAttributeCollection keyAttributes) : base(Meta.EntityLogicalName, keyAttributes) { }
	}
}