using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Extensions;
using System;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace XrmGenTest;

[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
public partial class ContactCreatePlugin
{
	[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
	[EntityLogicalName("contact")]
	public class TargetContact : Entity
	{
		public static class Meta
		{
			public const string EntityLogicalName = "contact";
			public const string EntityLogicalCollectionName = "contacts";
			public const string EntitySetName = "contacts";
			public const string PrimaryNameAttribute = "";
			public const string PrimaryIdAttribute = "contactid";
	
			public partial class Fields
			{
				public const string FirstName = "firstname";
				public const string LastName = "lastname";
			}
	
			public partial class Choices
			{
			}
		}
	
		/// <summary>
		/// Max Length: 50</br>
		/// Required Level: Recommended</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		[AttributeLogicalName("firstname")]
		public string FirstName
		{
			get => TryGetAttributeValue("firstname", out string value) ? value : null;
			set => this["firstname"] = value;
		}
		/// <summary>
		/// Max Length: 50</br>
		/// Required Level: ApplicationRequired</br>
		/// Valid for: Create Update Read</br>
		/// </summary>
		[AttributeLogicalName("lastname")]
		public string LastName
		{
			get => TryGetAttributeValue("lastname", out string value) ? value : null;
			set => this["lastname"] = value;
		}
	}
	[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
	[EntityLogicalName("contact")]
	public class PostImageContact : Entity
	{
		public static class Meta
		{
			public const string EntityLogicalName = "contact";
			public const string EntityLogicalCollectionName = "contacts";
			public const string EntitySetName = "contacts";
			public const string PrimaryNameAttribute = "";
			public const string PrimaryIdAttribute = "contactid";
	
			public partial class Fields
			{
				public const string AccountId = "accountid";
				public static readonly ReadOnlyCollection<string> AccountIdTargets = new (["account"]);
				public const string AccountIdName = "accountidname";
				public const string AccountIdYomiName = "accountidyominame";
				public const string AccountRoleCode = "accountrolecode";
				public const string AccountRoleCodeName = "accountrolecodename";
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
				public const string Address3_AddressId = "address3_addressid";
				public const string Address3_AddressTypeCode = "address3_addresstypecode";
				public const string Address3_AddressTypeCodeName = "address3_addresstypecodename";
				public const string Address3_City = "address3_city";
				public const string Address3_Composite = "address3_composite";
				public const string Address3_Country = "address3_country";
				public const string Address3_County = "address3_county";
				public const string Address3_Fax = "address3_fax";
				public const string Address3_FreightTermsCode = "address3_freighttermscode";
				public const string Address3_FreightTermsCodeName = "address3_freighttermscodename";
				public const string Address3_Latitude = "address3_latitude";
				public const string Address3_Line1 = "address3_line1";
				public const string Address3_Line2 = "address3_line2";
				public const string Address3_Line3 = "address3_line3";
				public const string Address3_Longitude = "address3_longitude";
				public const string Address3_Name = "address3_name";
				public const string Address3_PostalCode = "address3_postalcode";
				public const string Address3_PostOfficeBox = "address3_postofficebox";
				public const string Address3_PrimaryContactName = "address3_primarycontactname";
				public const string Address3_ShippingMethodCode = "address3_shippingmethodcode";
				public const string Address3_ShippingMethodCodeName = "address3_shippingmethodcodename";
				public const string Address3_StateOrProvince = "address3_stateorprovince";
				public const string Address3_Telephone1 = "address3_telephone1";
				public const string Address3_Telephone2 = "address3_telephone2";
				public const string Address3_Telephone3 = "address3_telephone3";
				public const string Address3_UPSZone = "address3_upszone";
				public const string Address3_UTCOffset = "address3_utcoffset";
				public const string Adx_ConfirmRemovePassword = "adx_confirmremovepassword";
				public const string Adx_confirmremovepasswordName = "adx_confirmremovepasswordname";
				public const string Adx_CreatedByIPAddress = "adx_createdbyipaddress";
				public const string Adx_CreatedByUsername = "adx_createdbyusername";
				public const string Adx_identity_accessfailedcount = "adx_identity_accessfailedcount";
				public const string Adx_identity_emailaddress1confirmed = "adx_identity_emailaddress1confirmed";
				public const string Adx_identity_emailaddress1confirmedName = "adx_identity_emailaddress1confirmedname";
				public const string Adx_identity_lastsuccessfullogin = "adx_identity_lastsuccessfullogin";
				public const string Adx_identity_locallogindisabled = "adx_identity_locallogindisabled";
				public const string Adx_identity_locallogindisabledName = "adx_identity_locallogindisabledname";
				public const string Adx_identity_lockoutenabled = "adx_identity_lockoutenabled";
				public const string Adx_identity_lockoutenabledName = "adx_identity_lockoutenabledname";
				public const string Adx_identity_lockoutenddate = "adx_identity_lockoutenddate";
				public const string Adx_identity_logonenabled = "adx_identity_logonenabled";
				public const string Adx_identity_logonenabledName = "adx_identity_logonenabledname";
				public const string Adx_identity_mobilephoneconfirmed = "adx_identity_mobilephoneconfirmed";
				public const string Adx_identity_mobilephoneconfirmedName = "adx_identity_mobilephoneconfirmedname";
				public const string Adx_identity_newpassword = "adx_identity_newpassword";
				public const string Adx_identity_passwordhash = "adx_identity_passwordhash";
				public const string Adx_identity_securitystamp = "adx_identity_securitystamp";
				public const string Adx_identity_twofactorenabled = "adx_identity_twofactorenabled";
				public const string Adx_identity_twofactorenabledName = "adx_identity_twofactorenabledname";
				public const string Adx_identity_username = "adx_identity_username";
				public const string Adx_ModifiedByIPAddress = "adx_modifiedbyipaddress";
				public const string Adx_ModifiedByUsername = "adx_modifiedbyusername";
				public const string Adx_OrganizationName = "adx_organizationname";
				public const string Adx_preferredlcid = "adx_preferredlcid";
				public const string Adx_profilealert = "adx_profilealert";
				public const string Adx_profilealertdate = "adx_profilealertdate";
				public const string Adx_profilealertinstructions = "adx_profilealertinstructions";
				public const string Adx_profilealertName = "adx_profilealertname";
				public const string Adx_ProfileIsAnonymous = "adx_profileisanonymous";
				public const string Adx_profileisanonymousName = "adx_profileisanonymousname";
				public const string Adx_ProfileLastActivity = "adx_profilelastactivity";
				public const string Adx_profilemodifiedon = "adx_profilemodifiedon";
				public const string Adx_PublicProfileCopy = "adx_publicprofilecopy";
				public const string Adx_TimeZone = "adx_timezone";
				public const string Aging30 = "aging30";
				public const string Aging30_Base = "aging30_base";
				public const string Aging60 = "aging60";
				public const string Aging60_Base = "aging60_base";
				public const string Aging90 = "aging90";
				public const string Aging90_Base = "aging90_base";
				public const string Anniversary = "anniversary";
				public const string AnnualIncome = "annualincome";
				public const string AnnualIncome_Base = "annualincome_base";
				public const string AssistantName = "assistantname";
				public const string AssistantPhone = "assistantphone";
				public const string BirthDate = "birthdate";
				public const string Business2 = "business2";
				public const string Callback = "callback";
				public const string ChildrensNames = "childrensnames";
				public const string Company = "company";
				public const string ContactId = "contactid";
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
				public const string Department = "department";
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
				public const string EducationCode = "educationcode";
				public const string EducationCodeName = "educationcodename";
				public const string EMailAddress1 = "emailaddress1";
				public const string EMailAddress2 = "emailaddress2";
				public const string EMailAddress3 = "emailaddress3";
				public const string EmployeeId = "employeeid";
				public const string EntityImage = "entityimage";
				public const string EntityImage_Timestamp = "entityimage_timestamp";
				public const string EntityImage_URL = "entityimage_url";
				public const string EntityImageId = "entityimageid";
				public const string ExchangeRate = "exchangerate";
				public const string ExternalUserIdentifier = "externaluseridentifier";
				public const string FamilyStatusCode = "familystatuscode";
				public const string FamilyStatusCodeName = "familystatuscodename";
				public const string Fax = "fax";
				public const string FirstName = "firstname";
				public const string FollowEmail = "followemail";
				public const string FollowEmailName = "followemailname";
				public const string FtpSiteUrl = "ftpsiteurl";
				public const string FullName = "fullname";
				public const string GenderCode = "gendercode";
				public const string GenderCodeName = "gendercodename";
				public const string GovernmentId = "governmentid";
				public const string HasChildrenCode = "haschildrencode";
				public const string HasChildrenCodeName = "haschildrencodename";
				public const string Home2 = "home2";
				public const string ImportSequenceNumber = "importsequencenumber";
				public const string IsAutoCreate = "isautocreate";
				public const string IsBackofficeCustomer = "isbackofficecustomer";
				public const string IsBackofficeCustomerName = "isbackofficecustomername";
				public const string IsPrivate = "isprivate";
				public const string IsPrivateName = "isprivatename";
				public const string JobTitle = "jobtitle";
				public const string LastName = "lastname";
				public const string LastOnHoldTime = "lastonholdtime";
				public const string LastUsedInCampaign = "lastusedincampaign";
				public const string LeadSourceCode = "leadsourcecode";
				public const string LeadSourceCodeName = "leadsourcecodename";
				public const string ManagerName = "managername";
				public const string ManagerPhone = "managerphone";
				public const string MarketingOnly = "marketingonly";
				public const string MarketingOnlyName = "marketingonlyname";
				public const string MasterContactIdName = "mastercontactidname";
				public const string MasterContactIdYomiName = "mastercontactidyominame";
				public const string MasterId = "masterid";
				public static readonly ReadOnlyCollection<string> MasterIdTargets = new (["contact"]);
				public const string Merged = "merged";
				public const string MergedName = "mergedname";
				public const string MiddleName = "middlename";
				public const string MobilePhone = "mobilephone";
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
				public const string Msdyn_disablewebtracking = "msdyn_disablewebtracking";
				public const string Msdyn_disablewebtrackingName = "msdyn_disablewebtrackingname";
				public const string Msdyn_isminor = "msdyn_isminor";
				public const string Msdyn_isminorName = "msdyn_isminorname";
				public const string Msdyn_isminorwithparentalconsent = "msdyn_isminorwithparentalconsent";
				public const string Msdyn_isminorwithparentalconsentName = "msdyn_isminorwithparentalconsentname";
				public const string Msdyn_portaltermsagreementdate = "msdyn_portaltermsagreementdate";
				public const string Mspp_userpreferredlcid = "mspp_userpreferredlcid";
				public const string Mspp_userpreferredlcidName = "mspp_userpreferredlcidname";
				public const string NickName = "nickname";
				public const string NumberOfChildren = "numberofchildren";
				public const string OnHoldTime = "onholdtime";
				public const string OverriddenCreatedOn = "overriddencreatedon";
				public const string OwnerId = "ownerid";
				public const string OwnerIdName = "owneridname";
				public const string OwnerIdType = "owneridtype";
				public const string OwnerIdYomiName = "owneridyominame";
				public const string OwningBusinessUnit = "owningbusinessunit";
				public static readonly ReadOnlyCollection<string> OwningBusinessUnitTargets = new (["businessunit"]);
				public const string OwningBusinessUnitName = "owningbusinessunitname";
				public const string OwningTeam = "owningteam";
				public static readonly ReadOnlyCollection<string> OwningTeamTargets = new (["team"]);
				public const string OwningUser = "owninguser";
				public static readonly ReadOnlyCollection<string> OwningUserTargets = new (["systemuser"]);
				public const string Pager = "pager";
				public const string ParentContactId = "parentcontactid";
				public static readonly ReadOnlyCollection<string> ParentContactIdTargets = new (["contact"]);
				public const string ParentContactIdName = "parentcontactidname";
				public const string ParentContactIdYomiName = "parentcontactidyominame";
				public const string ParentCustomerId = "parentcustomerid";
				public static readonly ReadOnlyCollection<string> ParentCustomerIdTargets = new (["account","contact"]);
				public const string ParentCustomerIdName = "parentcustomeridname";
				public const string ParentCustomerIdType = "parentcustomeridtype";
				public const string ParentCustomerIdYomiName = "parentcustomeridyominame";
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
				public const string ProcessId = "processid";
				public const string Salutation = "salutation";
				public const string ShippingMethodCode = "shippingmethodcode";
				public const string ShippingMethodCodeName = "shippingmethodcodename";
				public const string SLAId = "slaid";
				public static readonly ReadOnlyCollection<string> SLAIdTargets = new (["sla"]);
				public const string SLAInvokedId = "slainvokedid";
				public static readonly ReadOnlyCollection<string> SLAInvokedIdTargets = new (["sla"]);
				public const string SLAInvokedIdName = "slainvokedidname";
				public const string SLAName = "slaname";
				public const string SpousesName = "spousesname";
				public const string StageId = "stageid";
				public const string StateCode = "statecode";
				public const string StateCodeName = "statecodename";
				public const string StatusCode = "statuscode";
				public const string StatusCodeName = "statuscodename";
				public const string SubscriptionId = "subscriptionid";
				public const string Suffix = "suffix";
				public const string Telephone1 = "telephone1";
				public const string Telephone2 = "telephone2";
				public const string Telephone3 = "telephone3";
				public const string TerritoryCode = "territorycode";
				public const string TerritoryCodeName = "territorycodename";
				public const string TimeSpentByMeOnEmailAndMeetings = "timespentbymeonemailandmeetings";
				public const string TimeZoneRuleVersionNumber = "timezoneruleversionnumber";
				public const string TransactionCurrencyId = "transactioncurrencyid";
				public static readonly ReadOnlyCollection<string> TransactionCurrencyIdTargets = new (["transactioncurrency"]);
				public const string TransactionCurrencyIdName = "transactioncurrencyidname";
				public const string TraversedPath = "traversedpath";
				public const string UTCConversionTimeZoneCode = "utcconversiontimezonecode";
				public const string VersionNumber = "versionnumber";
				public const string WebSiteUrl = "websiteurl";
				public const string YomiFirstName = "yomifirstname";
				public const string YomiFullName = "yomifullname";
				public const string YomiLastName = "yomilastname";
				public const string YomiMiddleName = "yomimiddlename";
			}
	
			public partial class Choices
			{
				/// <summary>
				/// Account role of the contact.
				/// </summary>
				[DataContract]
				public enum Role
				{
					[EnumMember]
					DecisionMaker = 1,
					[EnumMember]
					Employee = 2,
					[EnumMember]
					Influencer = 3,
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
				/// Type of address for address 3, such as billing, shipping, or primary address.
				/// </summary>
				[DataContract]
				public enum Address3AddressType
				{
					[EnumMember]
					DefaultValue = 1,
				}
				/// <summary>
				/// Freight terms for address 3.
				/// </summary>
				[DataContract]
				public enum Address3FreightTerms
				{
					[EnumMember]
					DefaultValue = 1,
				}
				/// <summary>
				/// Method of shipment for address 3.
				/// </summary>
				[DataContract]
				public enum Address3ShippingMethod
				{
					[EnumMember]
					DefaultValue = 1,
				}
				/// <summary>
				/// Size of the contact's business.
				/// </summary>
				[DataContract]
				public enum CustomerSize
				{
					[EnumMember]
					DefaultValue = 1,
				}
				/// <summary>
				/// Type of business associated with the contact.
				/// </summary>
				[DataContract]
				public enum RelationshipType
				{
					[EnumMember]
					DefaultValue = 1,
				}
				/// <summary>
				/// Formal education level that the contact has attained.
				/// </summary>
				[DataContract]
				public enum Education
				{
					[EnumMember]
					DefaultValue = 1,
				}
				/// <summary>
				/// Marital status of the contact.
				/// </summary>
				[DataContract]
				public enum MaritalStatus
				{
					[EnumMember]
					Single = 1,
					[EnumMember]
					Married = 2,
					[EnumMember]
					Divorced = 3,
					[EnumMember]
					Widowed = 4,
				}
				/// <summary>
				/// Gender of the contact.
				/// </summary>
				[DataContract]
				public enum Gender
				{
					[EnumMember]
					Male = 1,
					[EnumMember]
					Female = 2,
				}
				/// <summary>
				/// Information about whether the contact has children.
				/// </summary>
				[DataContract]
				public enum HasChildren
				{
					[EnumMember]
					DefaultValue = 1,
				}
				/// <summary>
				/// Source of the lead of the contact.
				/// </summary>
				[DataContract]
				public enum LeadSource
				{
					[EnumMember]
					DefaultValue = 1,
				}
				/// <summary>
				/// Power Pages Languages
				/// </summary>
				[DataContract]
				public enum PowerPagesLanguages
				{
					[EnumMember]
					Arabic = 1025,
					[EnumMember]
					BasqueBasque = 1069,
					[EnumMember]
					BulgarianBulgaria = 1026,
					[EnumMember]
					CatalanCatalan = 1027,
					[EnumMember]
					ChineseChina = 2052,
					[EnumMember]
					ChineseHongKongSar = 3076,
					[EnumMember]
					ChineseTraditional = 1028,
					[EnumMember]
					CroatianCroatia = 1050,
					[EnumMember]
					CzechCzechRepublic = 1029,
					[EnumMember]
					DanishDenmark = 1030,
					[EnumMember]
					DutchNetherlands = 1043,
					[EnumMember]
					English = 1033,
					[EnumMember]
					EstonianEstonia = 1061,
					[EnumMember]
					FinnishFinland = 1035,
					[EnumMember]
					FrenchFrance = 1036,
					[EnumMember]
					GalicianSpain = 1110,
					[EnumMember]
					GermanGermany = 1031,
					[EnumMember]
					GreekGreece = 1032,
					[EnumMember]
					Hebrew = 1037,
					[EnumMember]
					HindiIndia = 1081,
					[EnumMember]
					HungarianHungary = 1038,
					[EnumMember]
					IndonesianIndonesia = 1057,
					[EnumMember]
					ItalianItaly = 1040,
					[EnumMember]
					JapaneseJapan = 1041,
					[EnumMember]
					KazakhKazakhstan = 1087,
					[EnumMember]
					KoreanKorea = 1042,
					[EnumMember]
					LatvianLatvia = 1062,
					[EnumMember]
					LithuanianLithuania = 1063,
					[EnumMember]
					MalayMalaysia = 1086,
					[EnumMember]
					NorwegianBokmalNorway = 1044,
					[EnumMember]
					PolishPoland = 1045,
					[EnumMember]
					PortugueseBrazil = 1046,
					[EnumMember]
					PortuguesePortugal = 2070,
					[EnumMember]
					RomanianRomania = 1048,
					[EnumMember]
					RussianRussia = 1049,
					[EnumMember]
					SerbianCyrillicSerbia = 3098,
					[EnumMember]
					SerbianLatinSerbia = 2074,
					[EnumMember]
					SlovakSlovakia = 1051,
					[EnumMember]
					SlovenianSlovenia = 1060,
					[EnumMember]
					SpanishTraditionalSortSpain = 3082,
					[EnumMember]
					SwedishSweden = 1053,
					[EnumMember]
					ThaiThailand = 1054,
					[EnumMember]
					TurkishTurkiye = 1055,
					[EnumMember]
					UkrainianUkraine = 1058,
					[EnumMember]
					VietnameseVietnam = 1066,
				}
				/// <summary>
				/// Payment terms for the contact.
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
				/// Day of the week that the contact prefers for scheduling service activities.
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
				/// Time of day that the contact prefers for scheduling service activities.
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
				/// Preferred contact method for the contact.
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
				/// Method of shipping for the contact.
				/// </summary>
				[DataContract]
				public enum ShippingMethod
				{
					[EnumMember]
					DefaultValue = 1,
				}
				/// <summary>
				/// Status of the contact.
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
				/// Reason for the status of the contact.
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
		/// Max Length: 50
		/// Required Level: Recommended
		/// Valid for: Create Update Read
		/// </summary>
		[AttributeLogicalName("firstname")]
		public string FirstName
		{
			get => TryGetAttributeValue("firstname", out string value) ? value : null;
		}
		/// <summary>
		/// Max Length: 50
		/// Required Level: ApplicationRequired
		/// Valid for: Create Update Read
		/// </summary>
		[AttributeLogicalName("lastname")]
		public string LastName
		{
			get => TryGetAttributeValue("lastname", out string value) ? value : null;
		}
	}

	public TargetContact Target { get; set; }
	public PostImageContact PostImage { get; set; }

	/// <summary>
	/// This method should be called on every <see cref="XrmGenTest.ContactCreatePlugin.Execute(IServiceProvider)"/> execution.
	/// </summary>
	/// <param name="serviceProvider"></param>
	/// <exception cref="InvalidPluginExecutionException"></exception>
	internal void Initialize(IServiceProvider serviceProvider)
    {
        if (serviceProvider == null)
        {
            throw new InvalidPluginExecutionException(nameof(serviceProvider) + " argument is null.");
        }
        var executionContext = serviceProvider.Get<IPluginExecutionContext7>();
        Target = EntityOrDefault<TargetContact>(executionContext.InputParameters, "Target");
        PostImage = EntityOrDefault<PostImageContact>(executionContext.PreEntityImages, "PostImage");
    }

	private static T EntityOrDefault<T>(DataCollection<string, object> keyValues, string key) where T : Entity
    {
        if (keyValues is null) return default;
        return keyValues.TryGetValue(key, out var obj) ? obj is Entity entity ? entity.ToEntity<T>() : default : default;
    }

    private static T EntityOrDefault<T>(DataCollection<string, Entity> keyValues, string key) where T : Entity
    {
        if (keyValues is null) return default;
        return keyValues.TryGetValue(key, out var entity) ? entity?.ToEntity<T>() : default;
    }
}
