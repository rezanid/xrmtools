using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;

namespace XrmGenTest
{
	/// <summary>
	/// Display Name: Contact
	/// </summary>
	[GeneratedCode("TemplatedCodeGenerator", "1.3.3.0")]
	[EntityLogicalName("contact")]
	[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public partial class Contact : Entity
	{
		public partial class Meta 
		{
			public const string EntityLogicalName = "contact";
			public const string EntityLogicalCollectionName = "contacts";
			public const string EntitySetName = "contacts";
			public const string PrimaryNameAttribute = "fullname";
			public const string PrimaryIdAttribute = "contactid";

			public partial class Fields
			{
				public const string AccountId = "accountid";
				public static readonly ReadOnlyCollection<string> AccountIdTargets = new ReadOnlyCollection<string>(new string[] { "account" });
				public const string AccountIdName = "accountidname";
				public const string AccountIdYomiName = "accountidyominame";
				public const string AccountRoleCode = "accountrolecode";
				public const string AccountRoleCodeName = "accountrolecodename";
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
				public const string CreatedBy = "createdby";
				public static readonly ReadOnlyCollection<string> CreatedByTargets = new ReadOnlyCollection<string>(new string[] { "systemuser" });
				public const string CreatedByExternalParty = "createdbyexternalparty";
				public static readonly ReadOnlyCollection<string> CreatedByExternalPartyTargets = new ReadOnlyCollection<string>(new string[] { "externalparty" });
				public const string CreatedByExternalPartyName = "createdbyexternalpartyname";
				public const string CreatedByExternalPartyYomiName = "createdbyexternalpartyyominame";
				public const string CreatedByName = "createdbyname";
				public const string CreatedByYomiName = "createdbyyominame";
				public const string CreatedOn = "createdon";
				public const string CreatedOnBehalfBy = "createdonbehalfby";
				public static readonly ReadOnlyCollection<string> CreatedOnBehalfByTargets = new ReadOnlyCollection<string>(new string[] { "systemuser" });
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
				public const string IsBackofficeCustomer = "isbackofficecustomer";
				public const string IsBackofficeCustomerName = "isbackofficecustomername";
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
				public static readonly ReadOnlyCollection<string> MasterIdTargets = new ReadOnlyCollection<string>(new string[] { "contact" });
				public const string Merged = "merged";
				public const string MergedName = "mergedname";
				public const string MiddleName = "middlename";
				public const string MobilePhone = "mobilephone";
				public const string ModifiedBy = "modifiedby";
				public static readonly ReadOnlyCollection<string> ModifiedByTargets = new ReadOnlyCollection<string>(new string[] { "systemuser" });
				public const string ModifiedByExternalParty = "modifiedbyexternalparty";
				public static readonly ReadOnlyCollection<string> ModifiedByExternalPartyTargets = new ReadOnlyCollection<string>(new string[] { "externalparty" });
				public const string ModifiedByExternalPartyName = "modifiedbyexternalpartyname";
				public const string ModifiedByExternalPartyYomiName = "modifiedbyexternalpartyyominame";
				public const string ModifiedByName = "modifiedbyname";
				public const string ModifiedByYomiName = "modifiedbyyominame";
				public const string ModifiedOn = "modifiedon";
				public const string ModifiedOnBehalfBy = "modifiedonbehalfby";
				public static readonly ReadOnlyCollection<string> ModifiedOnBehalfByTargets = new ReadOnlyCollection<string>(new string[] { "systemuser" });
				public const string ModifiedOnBehalfByName = "modifiedonbehalfbyname";
				public const string ModifiedOnBehalfByYomiName = "modifiedonbehalfbyyominame";
				public const string Msa_managingpartnerid = "msa_managingpartnerid";
				public static readonly ReadOnlyCollection<string> Msa_managingpartneridTargets = new ReadOnlyCollection<string>(new string[] { "account" });
				public const string Msa_managingpartneridName = "msa_managingpartneridname";
				public const string Msa_managingpartneridYomiName = "msa_managingpartneridyominame";
				public const string Msdyn_disablewebtracking = "msdyn_disablewebtracking";
				public const string Msdyn_disablewebtrackingName = "msdyn_disablewebtrackingname";
				public const string Msdyn_isminor = "msdyn_isminor";
				public const string Msdyn_isminorName = "msdyn_isminorname";
				public const string Msdyn_isminorwithparentalconsent = "msdyn_isminorwithparentalconsent";
				public const string Msdyn_isminorwithparentalconsentName = "msdyn_isminorwithparentalconsentname";
				public const string Msdyn_portaltermsagreementdate = "msdyn_portaltermsagreementdate";
				public const string Msft_DataState = "msft_datastate";
				public const string Msft_datastateName = "msft_datastatename";
				public const string Mspp_userpreferredlcid = "mspp_userpreferredlcid";
				public const string Mspp_userpreferredlcidName = "mspp_userpreferredlcidname";
				public const string NickName = "nickname";
				public const string NumberOfChildren = "numberofchildren";
				public const string OnHoldTime = "onholdtime";
				public const string OverriddenCreatedOn = "overriddencreatedon";
				public const string OwnerId = "ownerid";
				public const string OwnerIdName = "owneridname";
				public const string OwnerIdYomiName = "owneridyominame";
				public const string OwningBusinessUnit = "owningbusinessunit";
				public static readonly ReadOnlyCollection<string> OwningBusinessUnitTargets = new ReadOnlyCollection<string>(new string[] { "businessunit" });
				public const string OwningBusinessUnitName = "owningbusinessunitname";
				public const string OwningTeam = "owningteam";
				public static readonly ReadOnlyCollection<string> OwningTeamTargets = new ReadOnlyCollection<string>(new string[] { "team" });
				public const string OwningUser = "owninguser";
				public static readonly ReadOnlyCollection<string> OwningUserTargets = new ReadOnlyCollection<string>(new string[] { "systemuser" });
				public const string Pager = "pager";
				public const string ParentContactId = "parentcontactid";
				public static readonly ReadOnlyCollection<string> ParentContactIdTargets = new ReadOnlyCollection<string>(new string[] { "contact" });
				public const string ParentContactIdName = "parentcontactidname";
				public const string ParentContactIdYomiName = "parentcontactidyominame";
				public const string ParentCustomerId = "parentcustomerid";
				public static readonly ReadOnlyCollection<string> ParentCustomerIdTargets = new ReadOnlyCollection<string>(new string[] { "account","contact" });
				public const string ParentCustomerIdName = "parentcustomeridname";
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
				public static readonly ReadOnlyCollection<string> PreferredSystemUserIdTargets = new ReadOnlyCollection<string>(new string[] { "systemuser" });
				public const string PreferredSystemUserIdName = "preferredsystemuseridname";
				public const string PreferredSystemUserIdYomiName = "preferredsystemuseridyominame";
				public const string ProcessId = "processid";
				public const string Salutation = "salutation";
				public const string ShippingMethodCode = "shippingmethodcode";
				public const string ShippingMethodCodeName = "shippingmethodcodename";
				public const string SLAId = "slaid";
				public static readonly ReadOnlyCollection<string> SLAIdTargets = new ReadOnlyCollection<string>(new string[] { "sla" });
				public const string SLAInvokedId = "slainvokedid";
				public static readonly ReadOnlyCollection<string> SLAInvokedIdTargets = new ReadOnlyCollection<string>(new string[] { "sla" });
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
				public static readonly ReadOnlyCollection<string> TransactionCurrencyIdTargets = new ReadOnlyCollection<string>(new string[] { "transactioncurrency" });
				public const string TransactionCurrencyIdName = "transactioncurrencyidname";
				public const string TraversedPath = "traversedpath";
				public const string UTCConversionTimeZoneCode = "utcconversiontimezonecode";
				public const string VersionNumber = "versionnumber";
				public const string WebSiteUrl = "websiteurl";
				public const string YomiFirstName = "yomifirstname";
				public const string YomiFullName = "yomifullname";
				public const string YomiLastName = "yomilastname";
				public const string YomiMiddleName = "yomimiddlename";

				private static readonly Dictionary<string, string> _fieldMap = new Dictionary<string, string>
				{
					[nameof(AccountId)] = AccountId,
					[nameof(AccountIdName)] = AccountIdName,
					[nameof(AccountIdYomiName)] = AccountIdYomiName,
					[nameof(AccountRoleCode)] = AccountRoleCode,
					[nameof(AccountRoleCodeName)] = AccountRoleCodeName,
					[nameof(Address1_AddressTypeCode)] = Address1_AddressTypeCode,
					[nameof(Address1_AddressTypeCodeName)] = Address1_AddressTypeCodeName,
					[nameof(Address1_City)] = Address1_City,
					[nameof(Address1_Composite)] = Address1_Composite,
					[nameof(Address1_Country)] = Address1_Country,
					[nameof(Address1_County)] = Address1_County,
					[nameof(Address1_Fax)] = Address1_Fax,
					[nameof(Address1_FreightTermsCode)] = Address1_FreightTermsCode,
					[nameof(Address1_FreightTermsCodeName)] = Address1_FreightTermsCodeName,
					[nameof(Address1_Latitude)] = Address1_Latitude,
					[nameof(Address1_Line1)] = Address1_Line1,
					[nameof(Address1_Line2)] = Address1_Line2,
					[nameof(Address1_Line3)] = Address1_Line3,
					[nameof(Address1_Longitude)] = Address1_Longitude,
					[nameof(Address1_Name)] = Address1_Name,
					[nameof(Address1_PostalCode)] = Address1_PostalCode,
					[nameof(Address1_PostOfficeBox)] = Address1_PostOfficeBox,
					[nameof(Address1_PrimaryContactName)] = Address1_PrimaryContactName,
					[nameof(Address1_ShippingMethodCode)] = Address1_ShippingMethodCode,
					[nameof(Address1_ShippingMethodCodeName)] = Address1_ShippingMethodCodeName,
					[nameof(Address1_StateOrProvince)] = Address1_StateOrProvince,
					[nameof(Address1_Telephone1)] = Address1_Telephone1,
					[nameof(Address1_Telephone2)] = Address1_Telephone2,
					[nameof(Address1_Telephone3)] = Address1_Telephone3,
					[nameof(Address1_UPSZone)] = Address1_UPSZone,
					[nameof(Address1_UTCOffset)] = Address1_UTCOffset,
					[nameof(Address2_AddressTypeCode)] = Address2_AddressTypeCode,
					[nameof(Address2_AddressTypeCodeName)] = Address2_AddressTypeCodeName,
					[nameof(Address2_City)] = Address2_City,
					[nameof(Address2_Composite)] = Address2_Composite,
					[nameof(Address2_Country)] = Address2_Country,
					[nameof(Address2_County)] = Address2_County,
					[nameof(Address2_Fax)] = Address2_Fax,
					[nameof(Address2_FreightTermsCode)] = Address2_FreightTermsCode,
					[nameof(Address2_FreightTermsCodeName)] = Address2_FreightTermsCodeName,
					[nameof(Address2_Latitude)] = Address2_Latitude,
					[nameof(Address2_Line1)] = Address2_Line1,
					[nameof(Address2_Line2)] = Address2_Line2,
					[nameof(Address2_Line3)] = Address2_Line3,
					[nameof(Address2_Longitude)] = Address2_Longitude,
					[nameof(Address2_Name)] = Address2_Name,
					[nameof(Address2_PostalCode)] = Address2_PostalCode,
					[nameof(Address2_PostOfficeBox)] = Address2_PostOfficeBox,
					[nameof(Address2_PrimaryContactName)] = Address2_PrimaryContactName,
					[nameof(Address2_ShippingMethodCode)] = Address2_ShippingMethodCode,
					[nameof(Address2_ShippingMethodCodeName)] = Address2_ShippingMethodCodeName,
					[nameof(Address2_StateOrProvince)] = Address2_StateOrProvince,
					[nameof(Address2_Telephone1)] = Address2_Telephone1,
					[nameof(Address2_Telephone2)] = Address2_Telephone2,
					[nameof(Address2_Telephone3)] = Address2_Telephone3,
					[nameof(Address2_UPSZone)] = Address2_UPSZone,
					[nameof(Address2_UTCOffset)] = Address2_UTCOffset,
					[nameof(Address3_AddressTypeCode)] = Address3_AddressTypeCode,
					[nameof(Address3_AddressTypeCodeName)] = Address3_AddressTypeCodeName,
					[nameof(Address3_City)] = Address3_City,
					[nameof(Address3_Composite)] = Address3_Composite,
					[nameof(Address3_Country)] = Address3_Country,
					[nameof(Address3_County)] = Address3_County,
					[nameof(Address3_Fax)] = Address3_Fax,
					[nameof(Address3_FreightTermsCode)] = Address3_FreightTermsCode,
					[nameof(Address3_FreightTermsCodeName)] = Address3_FreightTermsCodeName,
					[nameof(Address3_Latitude)] = Address3_Latitude,
					[nameof(Address3_Line1)] = Address3_Line1,
					[nameof(Address3_Line2)] = Address3_Line2,
					[nameof(Address3_Line3)] = Address3_Line3,
					[nameof(Address3_Longitude)] = Address3_Longitude,
					[nameof(Address3_Name)] = Address3_Name,
					[nameof(Address3_PostalCode)] = Address3_PostalCode,
					[nameof(Address3_PostOfficeBox)] = Address3_PostOfficeBox,
					[nameof(Address3_PrimaryContactName)] = Address3_PrimaryContactName,
					[nameof(Address3_ShippingMethodCode)] = Address3_ShippingMethodCode,
					[nameof(Address3_ShippingMethodCodeName)] = Address3_ShippingMethodCodeName,
					[nameof(Address3_StateOrProvince)] = Address3_StateOrProvince,
					[nameof(Address3_Telephone1)] = Address3_Telephone1,
					[nameof(Address3_Telephone2)] = Address3_Telephone2,
					[nameof(Address3_Telephone3)] = Address3_Telephone3,
					[nameof(Address3_UPSZone)] = Address3_UPSZone,
					[nameof(Address3_UTCOffset)] = Address3_UTCOffset,
					[nameof(Adx_ConfirmRemovePassword)] = Adx_ConfirmRemovePassword,
					[nameof(Adx_confirmremovepasswordName)] = Adx_confirmremovepasswordName,
					[nameof(Adx_CreatedByIPAddress)] = Adx_CreatedByIPAddress,
					[nameof(Adx_CreatedByUsername)] = Adx_CreatedByUsername,
					[nameof(Adx_identity_accessfailedcount)] = Adx_identity_accessfailedcount,
					[nameof(Adx_identity_emailaddress1confirmed)] = Adx_identity_emailaddress1confirmed,
					[nameof(Adx_identity_emailaddress1confirmedName)] = Adx_identity_emailaddress1confirmedName,
					[nameof(Adx_identity_lastsuccessfullogin)] = Adx_identity_lastsuccessfullogin,
					[nameof(Adx_identity_locallogindisabled)] = Adx_identity_locallogindisabled,
					[nameof(Adx_identity_locallogindisabledName)] = Adx_identity_locallogindisabledName,
					[nameof(Adx_identity_lockoutenabled)] = Adx_identity_lockoutenabled,
					[nameof(Adx_identity_lockoutenabledName)] = Adx_identity_lockoutenabledName,
					[nameof(Adx_identity_lockoutenddate)] = Adx_identity_lockoutenddate,
					[nameof(Adx_identity_logonenabled)] = Adx_identity_logonenabled,
					[nameof(Adx_identity_logonenabledName)] = Adx_identity_logonenabledName,
					[nameof(Adx_identity_mobilephoneconfirmed)] = Adx_identity_mobilephoneconfirmed,
					[nameof(Adx_identity_mobilephoneconfirmedName)] = Adx_identity_mobilephoneconfirmedName,
					[nameof(Adx_identity_newpassword)] = Adx_identity_newpassword,
					[nameof(Adx_identity_passwordhash)] = Adx_identity_passwordhash,
					[nameof(Adx_identity_securitystamp)] = Adx_identity_securitystamp,
					[nameof(Adx_identity_twofactorenabled)] = Adx_identity_twofactorenabled,
					[nameof(Adx_identity_twofactorenabledName)] = Adx_identity_twofactorenabledName,
					[nameof(Adx_identity_username)] = Adx_identity_username,
					[nameof(Adx_ModifiedByIPAddress)] = Adx_ModifiedByIPAddress,
					[nameof(Adx_ModifiedByUsername)] = Adx_ModifiedByUsername,
					[nameof(Adx_OrganizationName)] = Adx_OrganizationName,
					[nameof(Adx_preferredlcid)] = Adx_preferredlcid,
					[nameof(Adx_profilealert)] = Adx_profilealert,
					[nameof(Adx_profilealertdate)] = Adx_profilealertdate,
					[nameof(Adx_profilealertinstructions)] = Adx_profilealertinstructions,
					[nameof(Adx_profilealertName)] = Adx_profilealertName,
					[nameof(Adx_ProfileIsAnonymous)] = Adx_ProfileIsAnonymous,
					[nameof(Adx_profileisanonymousName)] = Adx_profileisanonymousName,
					[nameof(Adx_ProfileLastActivity)] = Adx_ProfileLastActivity,
					[nameof(Adx_profilemodifiedon)] = Adx_profilemodifiedon,
					[nameof(Adx_PublicProfileCopy)] = Adx_PublicProfileCopy,
					[nameof(Adx_TimeZone)] = Adx_TimeZone,
					[nameof(Aging30)] = Aging30,
					[nameof(Aging30_Base)] = Aging30_Base,
					[nameof(Aging60)] = Aging60,
					[nameof(Aging60_Base)] = Aging60_Base,
					[nameof(Aging90)] = Aging90,
					[nameof(Aging90_Base)] = Aging90_Base,
					[nameof(Anniversary)] = Anniversary,
					[nameof(AnnualIncome)] = AnnualIncome,
					[nameof(AnnualIncome_Base)] = AnnualIncome_Base,
					[nameof(AssistantName)] = AssistantName,
					[nameof(AssistantPhone)] = AssistantPhone,
					[nameof(BirthDate)] = BirthDate,
					[nameof(Business2)] = Business2,
					[nameof(Callback)] = Callback,
					[nameof(ChildrensNames)] = ChildrensNames,
					[nameof(Company)] = Company,
					[nameof(CreatedBy)] = CreatedBy,
					[nameof(CreatedByExternalParty)] = CreatedByExternalParty,
					[nameof(CreatedByExternalPartyName)] = CreatedByExternalPartyName,
					[nameof(CreatedByExternalPartyYomiName)] = CreatedByExternalPartyYomiName,
					[nameof(CreatedByName)] = CreatedByName,
					[nameof(CreatedByYomiName)] = CreatedByYomiName,
					[nameof(CreatedOn)] = CreatedOn,
					[nameof(CreatedOnBehalfBy)] = CreatedOnBehalfBy,
					[nameof(CreatedOnBehalfByName)] = CreatedOnBehalfByName,
					[nameof(CreatedOnBehalfByYomiName)] = CreatedOnBehalfByYomiName,
					[nameof(CreditLimit)] = CreditLimit,
					[nameof(CreditLimit_Base)] = CreditLimit_Base,
					[nameof(CreditOnHold)] = CreditOnHold,
					[nameof(CreditOnHoldName)] = CreditOnHoldName,
					[nameof(CustomerSizeCode)] = CustomerSizeCode,
					[nameof(CustomerSizeCodeName)] = CustomerSizeCodeName,
					[nameof(CustomerTypeCode)] = CustomerTypeCode,
					[nameof(CustomerTypeCodeName)] = CustomerTypeCodeName,
					[nameof(Department)] = Department,
					[nameof(Description)] = Description,
					[nameof(DoNotBulkEMail)] = DoNotBulkEMail,
					[nameof(DoNotBulkEMailName)] = DoNotBulkEMailName,
					[nameof(DoNotBulkPostalMail)] = DoNotBulkPostalMail,
					[nameof(DoNotBulkPostalMailName)] = DoNotBulkPostalMailName,
					[nameof(DoNotEMail)] = DoNotEMail,
					[nameof(DoNotEMailName)] = DoNotEMailName,
					[nameof(DoNotFax)] = DoNotFax,
					[nameof(DoNotFaxName)] = DoNotFaxName,
					[nameof(DoNotPhone)] = DoNotPhone,
					[nameof(DoNotPhoneName)] = DoNotPhoneName,
					[nameof(DoNotPostalMail)] = DoNotPostalMail,
					[nameof(DoNotPostalMailName)] = DoNotPostalMailName,
					[nameof(DoNotSendMarketingMaterialName)] = DoNotSendMarketingMaterialName,
					[nameof(DoNotSendMM)] = DoNotSendMM,
					[nameof(EducationCode)] = EducationCode,
					[nameof(EducationCodeName)] = EducationCodeName,
					[nameof(EMailAddress1)] = EMailAddress1,
					[nameof(EMailAddress2)] = EMailAddress2,
					[nameof(EMailAddress3)] = EMailAddress3,
					[nameof(EmployeeId)] = EmployeeId,
					[nameof(EntityImage)] = EntityImage,
					[nameof(EntityImage_Timestamp)] = EntityImage_Timestamp,
					[nameof(EntityImage_URL)] = EntityImage_URL,
					[nameof(EntityImageId)] = EntityImageId,
					[nameof(ExchangeRate)] = ExchangeRate,
					[nameof(ExternalUserIdentifier)] = ExternalUserIdentifier,
					[nameof(FamilyStatusCode)] = FamilyStatusCode,
					[nameof(FamilyStatusCodeName)] = FamilyStatusCodeName,
					[nameof(Fax)] = Fax,
					[nameof(FirstName)] = FirstName,
					[nameof(FollowEmail)] = FollowEmail,
					[nameof(FollowEmailName)] = FollowEmailName,
					[nameof(FtpSiteUrl)] = FtpSiteUrl,
					[nameof(FullName)] = FullName,
					[nameof(GenderCode)] = GenderCode,
					[nameof(GenderCodeName)] = GenderCodeName,
					[nameof(GovernmentId)] = GovernmentId,
					[nameof(HasChildrenCode)] = HasChildrenCode,
					[nameof(HasChildrenCodeName)] = HasChildrenCodeName,
					[nameof(Home2)] = Home2,
					[nameof(ImportSequenceNumber)] = ImportSequenceNumber,
					[nameof(IsBackofficeCustomer)] = IsBackofficeCustomer,
					[nameof(IsBackofficeCustomerName)] = IsBackofficeCustomerName,
					[nameof(IsPrivateName)] = IsPrivateName,
					[nameof(JobTitle)] = JobTitle,
					[nameof(LastName)] = LastName,
					[nameof(LastOnHoldTime)] = LastOnHoldTime,
					[nameof(LastUsedInCampaign)] = LastUsedInCampaign,
					[nameof(LeadSourceCode)] = LeadSourceCode,
					[nameof(LeadSourceCodeName)] = LeadSourceCodeName,
					[nameof(ManagerName)] = ManagerName,
					[nameof(ManagerPhone)] = ManagerPhone,
					[nameof(MarketingOnly)] = MarketingOnly,
					[nameof(MarketingOnlyName)] = MarketingOnlyName,
					[nameof(MasterContactIdName)] = MasterContactIdName,
					[nameof(MasterContactIdYomiName)] = MasterContactIdYomiName,
					[nameof(MasterId)] = MasterId,
					[nameof(Merged)] = Merged,
					[nameof(MergedName)] = MergedName,
					[nameof(MiddleName)] = MiddleName,
					[nameof(MobilePhone)] = MobilePhone,
					[nameof(ModifiedBy)] = ModifiedBy,
					[nameof(ModifiedByExternalParty)] = ModifiedByExternalParty,
					[nameof(ModifiedByExternalPartyName)] = ModifiedByExternalPartyName,
					[nameof(ModifiedByExternalPartyYomiName)] = ModifiedByExternalPartyYomiName,
					[nameof(ModifiedByName)] = ModifiedByName,
					[nameof(ModifiedByYomiName)] = ModifiedByYomiName,
					[nameof(ModifiedOn)] = ModifiedOn,
					[nameof(ModifiedOnBehalfBy)] = ModifiedOnBehalfBy,
					[nameof(ModifiedOnBehalfByName)] = ModifiedOnBehalfByName,
					[nameof(ModifiedOnBehalfByYomiName)] = ModifiedOnBehalfByYomiName,
					[nameof(Msa_managingpartnerid)] = Msa_managingpartnerid,
					[nameof(Msa_managingpartneridName)] = Msa_managingpartneridName,
					[nameof(Msa_managingpartneridYomiName)] = Msa_managingpartneridYomiName,
					[nameof(Msdyn_disablewebtracking)] = Msdyn_disablewebtracking,
					[nameof(Msdyn_disablewebtrackingName)] = Msdyn_disablewebtrackingName,
					[nameof(Msdyn_isminor)] = Msdyn_isminor,
					[nameof(Msdyn_isminorName)] = Msdyn_isminorName,
					[nameof(Msdyn_isminorwithparentalconsent)] = Msdyn_isminorwithparentalconsent,
					[nameof(Msdyn_isminorwithparentalconsentName)] = Msdyn_isminorwithparentalconsentName,
					[nameof(Msdyn_portaltermsagreementdate)] = Msdyn_portaltermsagreementdate,
					[nameof(Msft_DataState)] = Msft_DataState,
					[nameof(Msft_datastateName)] = Msft_datastateName,
					[nameof(Mspp_userpreferredlcid)] = Mspp_userpreferredlcid,
					[nameof(Mspp_userpreferredlcidName)] = Mspp_userpreferredlcidName,
					[nameof(NickName)] = NickName,
					[nameof(NumberOfChildren)] = NumberOfChildren,
					[nameof(OnHoldTime)] = OnHoldTime,
					[nameof(OverriddenCreatedOn)] = OverriddenCreatedOn,
					[nameof(OwnerId)] = OwnerId,
					[nameof(OwnerIdName)] = OwnerIdName,
					[nameof(OwnerIdYomiName)] = OwnerIdYomiName,
					[nameof(OwningBusinessUnit)] = OwningBusinessUnit,
					[nameof(OwningBusinessUnitName)] = OwningBusinessUnitName,
					[nameof(OwningTeam)] = OwningTeam,
					[nameof(OwningUser)] = OwningUser,
					[nameof(Pager)] = Pager,
					[nameof(ParentContactId)] = ParentContactId,
					[nameof(ParentContactIdName)] = ParentContactIdName,
					[nameof(ParentContactIdYomiName)] = ParentContactIdYomiName,
					[nameof(ParentCustomerId)] = ParentCustomerId,
					[nameof(ParentCustomerIdName)] = ParentCustomerIdName,
					[nameof(ParentCustomerIdYomiName)] = ParentCustomerIdYomiName,
					[nameof(ParticipatesInWorkflow)] = ParticipatesInWorkflow,
					[nameof(ParticipatesInWorkflowName)] = ParticipatesInWorkflowName,
					[nameof(PaymentTermsCode)] = PaymentTermsCode,
					[nameof(PaymentTermsCodeName)] = PaymentTermsCodeName,
					[nameof(PreferredAppointmentDayCode)] = PreferredAppointmentDayCode,
					[nameof(PreferredAppointmentDayCodeName)] = PreferredAppointmentDayCodeName,
					[nameof(PreferredAppointmentTimeCode)] = PreferredAppointmentTimeCode,
					[nameof(PreferredAppointmentTimeCodeName)] = PreferredAppointmentTimeCodeName,
					[nameof(PreferredContactMethodCode)] = PreferredContactMethodCode,
					[nameof(PreferredContactMethodCodeName)] = PreferredContactMethodCodeName,
					[nameof(PreferredSystemUserId)] = PreferredSystemUserId,
					[nameof(PreferredSystemUserIdName)] = PreferredSystemUserIdName,
					[nameof(PreferredSystemUserIdYomiName)] = PreferredSystemUserIdYomiName,
					[nameof(ProcessId)] = ProcessId,
					[nameof(Salutation)] = Salutation,
					[nameof(ShippingMethodCode)] = ShippingMethodCode,
					[nameof(ShippingMethodCodeName)] = ShippingMethodCodeName,
					[nameof(SLAId)] = SLAId,
					[nameof(SLAInvokedId)] = SLAInvokedId,
					[nameof(SLAInvokedIdName)] = SLAInvokedIdName,
					[nameof(SLAName)] = SLAName,
					[nameof(SpousesName)] = SpousesName,
					[nameof(StageId)] = StageId,
					[nameof(StateCode)] = StateCode,
					[nameof(StateCodeName)] = StateCodeName,
					[nameof(StatusCode)] = StatusCode,
					[nameof(StatusCodeName)] = StatusCodeName,
					[nameof(SubscriptionId)] = SubscriptionId,
					[nameof(Suffix)] = Suffix,
					[nameof(Telephone1)] = Telephone1,
					[nameof(Telephone2)] = Telephone2,
					[nameof(Telephone3)] = Telephone3,
					[nameof(TerritoryCode)] = TerritoryCode,
					[nameof(TerritoryCodeName)] = TerritoryCodeName,
					[nameof(TimeSpentByMeOnEmailAndMeetings)] = TimeSpentByMeOnEmailAndMeetings,
					[nameof(TimeZoneRuleVersionNumber)] = TimeZoneRuleVersionNumber,
					[nameof(TransactionCurrencyId)] = TransactionCurrencyId,
					[nameof(TransactionCurrencyIdName)] = TransactionCurrencyIdName,
					[nameof(TraversedPath)] = TraversedPath,
					[nameof(UTCConversionTimeZoneCode)] = UTCConversionTimeZoneCode,
					[nameof(VersionNumber)] = VersionNumber,
					[nameof(WebSiteUrl)] = WebSiteUrl,
					[nameof(YomiFirstName)] = YomiFirstName,
					[nameof(YomiFullName)] = YomiFullName,
					[nameof(YomiLastName)] = YomiLastName,
					[nameof(YomiMiddleName)] = YomiMiddleName,
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
				/// Account role of the contact.
				/// </summary>
				[DataContract]
				public enum Role
				{
					[EnumMember] DecisionMaker = 1,
					[EnumMember] Employee = 2,
					[EnumMember] Influencer = 3,
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
				/// Type of address for address 3, such as billing, shipping, or primary address.
				/// </summary>
				[DataContract]
				public enum Address3AddressType
				{
					[EnumMember] DefaultValue = 1,
				}
				/// <summary>
				/// Freight terms for address 3.
				/// </summary>
				[DataContract]
				public enum Address3FreightTerms
				{
					[EnumMember] DefaultValue = 1,
				}
				/// <summary>
				/// Method of shipment for address 3.
				/// </summary>
				[DataContract]
				public enum Address3ShippingMethod
				{
					[EnumMember] DefaultValue = 1,
				}
				/// <summary>
				/// Size of the contact's business.
				/// </summary>
				[DataContract]
				public enum CustomerSize
				{
					[EnumMember] DefaultValue = 1,
				}
				/// <summary>
				/// Type of business associated with the contact.
				/// </summary>
				[DataContract]
				public enum RelationshipType
				{
					[EnumMember] DefaultValue = 1,
				}
				/// <summary>
				/// Formal education level that the contact has attained.
				/// </summary>
				[DataContract]
				public enum Education
				{
					[EnumMember] DefaultValue = 1,
				}
				/// <summary>
				/// Marital status of the contact.
				/// </summary>
				[DataContract]
				public enum MaritalStatus
				{
					[EnumMember] Single = 1,
					[EnumMember] Married = 2,
					[EnumMember] Divorced = 3,
					[EnumMember] Widowed = 4,
				}
				/// <summary>
				/// Gender of the contact.
				/// </summary>
				[DataContract]
				public enum Gender
				{
					[EnumMember] Male = 1,
					[EnumMember] Female = 2,
				}
				/// <summary>
				/// Information about whether the contact has children.
				/// </summary>
				[DataContract]
				public enum HasChildren
				{
					[EnumMember] DefaultValue = 1,
				}
				/// <summary>
				/// Source of the lead of the contact.
				/// </summary>
				[DataContract]
				public enum LeadSource
				{
					[EnumMember] DefaultValue = 1,
				}
				/// <summary>
				/// Payment terms for the contact.
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
				/// Day of the week that the contact prefers for scheduling service activities.
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
				/// Time of day that the contact prefers for scheduling service activities.
				/// </summary>
				[DataContract]
				public enum PreferredTime
				{
					[EnumMember] Morning = 1,
					[EnumMember] Afternoon = 2,
					[EnumMember] Evening = 3,
				}
				/// <summary>
				/// Preferred contact method for the contact.
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
				/// Method of shipping for the contact.
				/// </summary>
				[DataContract]
				public enum ShippingMethod
				{
					[EnumMember] DefaultValue = 1,
				}
				/// <summary>
				/// Status of the contact.
				/// </summary>
				[DataContract]
				public enum Status
				{
					[EnumMember] Active = 0,
					[EnumMember] Inactive = 1,
				}
				/// <summary>
				/// Reason for the status of the contact.
				/// </summary>
				[DataContract]
				public enum StatusReason
				{
					[EnumMember] Active = 1,
					[EnumMember] Inactive = 2,
				}
				/// <summary>
				/// Unique identifier of the territory to which the contact is assigned.
				/// </summary>
				[DataContract]
				public enum Territory
				{
					[EnumMember] DefaultValue = 1,
				}
			}
		}

		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// Targets: account<br/>
		/// </summary>
		[AttributeLogicalName("accountid")]
		public EntityReference AccountId
		{
			get => TryGetAttributeValue("accountid", out EntityReference value) ? value : null;
		}
		/// <summary>
		/// Attribute of: accountid<br/>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("accountidname")]
		public string AccountIdName
		{
			get => FormattedValues.Contains("accountid") ? FormattedValues["accountid"] : null;
		
		}
		/// <summary>
		/// Attribute of: accountid<br/>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("accountidyominame")]
		public string AccountIdYomiName
		{
			get => FormattedValues.Contains("accountid") ? FormattedValues["accountid"] : null;
		
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("accountrolecode")]
		public Contact.Meta.OptionSets.Role? AccountRoleCode
		{
			get => TryGetAttributeValue("accountrolecode", out OptionSetValue opt) && opt != null ? (Contact.Meta.OptionSets.Role?)opt.Value : null;
			set => this["accountrolecode"] = value == null ? null : new OptionSetValue((int)value);
		}
		/// <summary>
		/// Attribute of: accountrolecode<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("accountrolecodename")]
		public string AccountRoleCodeName
		{
			get => FormattedValues.Contains("accountrolecode") ? FormattedValues["accountrolecodename"] : null;
		}

		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address1_addresstypecode")]
		public Contact.Meta.OptionSets.Address1AddressType? Address1_AddressTypeCode
		{
			get => TryGetAttributeValue("address1_addresstypecode", out OptionSetValue opt) && opt != null ? (Contact.Meta.OptionSets.Address1AddressType?)opt.Value : null;
			set => this["address1_addresstypecode"] = value == null ? null : new OptionSetValue((int)value);
		}
		/// <summary>
		/// Attribute of: address1_addresstypecode<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("address1_addresstypecodename")]
		public string Address1_AddressTypeCodeName
		{
			get => FormattedValues.Contains("address1_addresstypecode") ? FormattedValues["address1_addresstypecodename"] : null;
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
		public Contact.Meta.OptionSets.Address1FreightTerms? Address1_FreightTermsCode
		{
			get => TryGetAttributeValue("address1_freighttermscode", out OptionSetValue opt) && opt != null ? (Contact.Meta.OptionSets.Address1FreightTerms?)opt.Value : null;
			set => this["address1_freighttermscode"] = value == null ? null : new OptionSetValue((int)value);
		}
		/// <summary>
		/// Attribute of: address1_freighttermscode<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("address1_freighttermscodename")]
		public string Address1_FreightTermsCodeName
		{
			get => FormattedValues.Contains("address1_freighttermscode") ? FormattedValues["address1_freighttermscodename"] : null;
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
		public Contact.Meta.OptionSets.Address1ShippingMethod? Address1_ShippingMethodCode
		{
			get => TryGetAttributeValue("address1_shippingmethodcode", out OptionSetValue opt) && opt != null ? (Contact.Meta.OptionSets.Address1ShippingMethod?)opt.Value : null;
			set => this["address1_shippingmethodcode"] = value == null ? null : new OptionSetValue((int)value);
		}
		/// <summary>
		/// Attribute of: address1_shippingmethodcode<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("address1_shippingmethodcodename")]
		public string Address1_ShippingMethodCodeName
		{
			get => FormattedValues.Contains("address1_shippingmethodcode") ? FormattedValues["address1_shippingmethodcodename"] : null;
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
		public Contact.Meta.OptionSets.Address2AddressType? Address2_AddressTypeCode
		{
			get => TryGetAttributeValue("address2_addresstypecode", out OptionSetValue opt) && opt != null ? (Contact.Meta.OptionSets.Address2AddressType?)opt.Value : null;
			set => this["address2_addresstypecode"] = value == null ? null : new OptionSetValue((int)value);
		}
		/// <summary>
		/// Attribute of: address2_addresstypecode<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("address2_addresstypecodename")]
		public string Address2_AddressTypeCodeName
		{
			get => FormattedValues.Contains("address2_addresstypecode") ? FormattedValues["address2_addresstypecodename"] : null;
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
		public Contact.Meta.OptionSets.Address2FreightTerms? Address2_FreightTermsCode
		{
			get => TryGetAttributeValue("address2_freighttermscode", out OptionSetValue opt) && opt != null ? (Contact.Meta.OptionSets.Address2FreightTerms?)opt.Value : null;
			set => this["address2_freighttermscode"] = value == null ? null : new OptionSetValue((int)value);
		}
		/// <summary>
		/// Attribute of: address2_freighttermscode<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("address2_freighttermscodename")]
		public string Address2_FreightTermsCodeName
		{
			get => FormattedValues.Contains("address2_freighttermscode") ? FormattedValues["address2_freighttermscodename"] : null;
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
		/// Max Length: 100<br/>
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
		public Contact.Meta.OptionSets.Address2ShippingMethod? Address2_ShippingMethodCode
		{
			get => TryGetAttributeValue("address2_shippingmethodcode", out OptionSetValue opt) && opt != null ? (Contact.Meta.OptionSets.Address2ShippingMethod?)opt.Value : null;
			set => this["address2_shippingmethodcode"] = value == null ? null : new OptionSetValue((int)value);
		}
		/// <summary>
		/// Attribute of: address2_shippingmethodcode<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("address2_shippingmethodcodename")]
		public string Address2_ShippingMethodCodeName
		{
			get => FormattedValues.Contains("address2_shippingmethodcode") ? FormattedValues["address2_shippingmethodcodename"] : null;
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
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address3_addresstypecode")]
		public Contact.Meta.OptionSets.Address3AddressType? Address3_AddressTypeCode
		{
			get => TryGetAttributeValue("address3_addresstypecode", out OptionSetValue opt) && opt != null ? (Contact.Meta.OptionSets.Address3AddressType?)opt.Value : null;
			set => this["address3_addresstypecode"] = value == null ? null : new OptionSetValue((int)value);
		}
		/// <summary>
		/// Attribute of: address3_addresstypecode<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("address3_addresstypecodename")]
		public string Address3_AddressTypeCodeName
		{
			get => FormattedValues.Contains("address3_addresstypecode") ? FormattedValues["address3_addresstypecodename"] : null;
		}

		/// <summary>
		/// Max Length: 80<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address3_city")]
		public string Address3_City
		{
			get => TryGetAttributeValue("address3_city", out string value) ? value : null;
			set => this["address3_city"] = value;
		}
		/// <summary>
		/// Max Length: 1000<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("address3_composite")]
		public string Address3_Composite
		{
			get => TryGetAttributeValue("address3_composite", out string value) ? value : null;
		}
		/// <summary>
		/// Max Length: 80<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address3_country")]
		public string Address3_Country
		{
			get => TryGetAttributeValue("address3_country", out string value) ? value : null;
			set => this["address3_country"] = value;
		}
		/// <summary>
		/// Max Length: 50<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address3_county")]
		public string Address3_County
		{
			get => TryGetAttributeValue("address3_county", out string value) ? value : null;
			set => this["address3_county"] = value;
		}
		/// <summary>
		/// Max Length: 50<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address3_fax")]
		public string Address3_Fax
		{
			get => TryGetAttributeValue("address3_fax", out string value) ? value : null;
			set => this["address3_fax"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address3_freighttermscode")]
		public Contact.Meta.OptionSets.Address3FreightTerms? Address3_FreightTermsCode
		{
			get => TryGetAttributeValue("address3_freighttermscode", out OptionSetValue opt) && opt != null ? (Contact.Meta.OptionSets.Address3FreightTerms?)opt.Value : null;
			set => this["address3_freighttermscode"] = value == null ? null : new OptionSetValue((int)value);
		}
		/// <summary>
		/// Attribute of: address3_freighttermscode<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("address3_freighttermscodename")]
		public string Address3_FreightTermsCodeName
		{
			get => FormattedValues.Contains("address3_freighttermscode") ? FormattedValues["address3_freighttermscodename"] : null;
		}

		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address3_latitude")]
		public double? Address3_Latitude
		{
			get => TryGetAttributeValue("address3_latitude", out double? value) ? value : null;
			set => this["address3_latitude"] = value;
		}
		/// <summary>
		/// Max Length: 250<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address3_line1")]
		public string Address3_Line1
		{
			get => TryGetAttributeValue("address3_line1", out string value) ? value : null;
			set => this["address3_line1"] = value;
		}
		/// <summary>
		/// Max Length: 250<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address3_line2")]
		public string Address3_Line2
		{
			get => TryGetAttributeValue("address3_line2", out string value) ? value : null;
			set => this["address3_line2"] = value;
		}
		/// <summary>
		/// Max Length: 250<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address3_line3")]
		public string Address3_Line3
		{
			get => TryGetAttributeValue("address3_line3", out string value) ? value : null;
			set => this["address3_line3"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address3_longitude")]
		public double? Address3_Longitude
		{
			get => TryGetAttributeValue("address3_longitude", out double? value) ? value : null;
			set => this["address3_longitude"] = value;
		}
		/// <summary>
		/// Max Length: 200<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address3_name")]
		public string Address3_Name
		{
			get => TryGetAttributeValue("address3_name", out string value) ? value : null;
			set => this["address3_name"] = value;
		}
		/// <summary>
		/// Max Length: 20<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address3_postalcode")]
		public string Address3_PostalCode
		{
			get => TryGetAttributeValue("address3_postalcode", out string value) ? value : null;
			set => this["address3_postalcode"] = value;
		}
		/// <summary>
		/// Max Length: 20<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address3_postofficebox")]
		public string Address3_PostOfficeBox
		{
			get => TryGetAttributeValue("address3_postofficebox", out string value) ? value : null;
			set => this["address3_postofficebox"] = value;
		}
		/// <summary>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address3_primarycontactname")]
		public string Address3_PrimaryContactName
		{
			get => TryGetAttributeValue("address3_primarycontactname", out string value) ? value : null;
			set => this["address3_primarycontactname"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address3_shippingmethodcode")]
		public Contact.Meta.OptionSets.Address3ShippingMethod? Address3_ShippingMethodCode
		{
			get => TryGetAttributeValue("address3_shippingmethodcode", out OptionSetValue opt) && opt != null ? (Contact.Meta.OptionSets.Address3ShippingMethod?)opt.Value : null;
			set => this["address3_shippingmethodcode"] = value == null ? null : new OptionSetValue((int)value);
		}
		/// <summary>
		/// Attribute of: address3_shippingmethodcode<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("address3_shippingmethodcodename")]
		public string Address3_ShippingMethodCodeName
		{
			get => FormattedValues.Contains("address3_shippingmethodcode") ? FormattedValues["address3_shippingmethodcodename"] : null;
		}

		/// <summary>
		/// Max Length: 50<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address3_stateorprovince")]
		public string Address3_StateOrProvince
		{
			get => TryGetAttributeValue("address3_stateorprovince", out string value) ? value : null;
			set => this["address3_stateorprovince"] = value;
		}
		/// <summary>
		/// Max Length: 50<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address3_telephone1")]
		public string Address3_Telephone1
		{
			get => TryGetAttributeValue("address3_telephone1", out string value) ? value : null;
			set => this["address3_telephone1"] = value;
		}
		/// <summary>
		/// Max Length: 50<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address3_telephone2")]
		public string Address3_Telephone2
		{
			get => TryGetAttributeValue("address3_telephone2", out string value) ? value : null;
			set => this["address3_telephone2"] = value;
		}
		/// <summary>
		/// Max Length: 50<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address3_telephone3")]
		public string Address3_Telephone3
		{
			get => TryGetAttributeValue("address3_telephone3", out string value) ? value : null;
			set => this["address3_telephone3"] = value;
		}
		/// <summary>
		/// Max Length: 4<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address3_upszone")]
		public string Address3_UPSZone
		{
			get => TryGetAttributeValue("address3_upszone", out string value) ? value : null;
			set => this["address3_upszone"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("address3_utcoffset")]
		public int? Address3_UTCOffset
		{
			get => TryGetAttributeValue("address3_utcoffset", out int? value) ? value : null;
			set => this["address3_utcoffset"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("adx_confirmremovepassword")]
		public bool? Adx_ConfirmRemovePassword
		{
			get => TryGetAttributeValue("adx_confirmremovepassword", out bool? value) ? value : null;
			set => this["adx_confirmremovepassword"] = value;
		}
		/// <summary>
		/// Attribute of: adx_confirmremovepassword<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("adx_confirmremovepasswordname")]
		public string Adx_confirmremovepasswordName
		{
			get => FormattedValues.Contains("adx_confirmremovepassword") ? FormattedValues["adx_confirmremovepasswordname"] : null;
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
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("adx_identity_accessfailedcount")]
		public int? Adx_identity_accessfailedcount
		{
			get => TryGetAttributeValue("adx_identity_accessfailedcount", out int? value) ? value : null;
			set => this["adx_identity_accessfailedcount"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("adx_identity_emailaddress1confirmed")]
		public bool? Adx_identity_emailaddress1confirmed
		{
			get => TryGetAttributeValue("adx_identity_emailaddress1confirmed", out bool? value) ? value : null;
			set => this["adx_identity_emailaddress1confirmed"] = value;
		}
		/// <summary>
		/// Attribute of: adx_identity_emailaddress1confirmed<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("adx_identity_emailaddress1confirmedname")]
		public string Adx_identity_emailaddress1confirmedName
		{
			get => FormattedValues.Contains("adx_identity_emailaddress1confirmed") ? FormattedValues["adx_identity_emailaddress1confirmedname"] : null;
		}

		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("adx_identity_lastsuccessfullogin")]
		public DateTime? Adx_identity_lastsuccessfullogin
		{
			get => TryGetAttributeValue("adx_identity_lastsuccessfullogin", out DateTime? value) ? value : null;
			set => this["adx_identity_lastsuccessfullogin"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("adx_identity_locallogindisabled")]
		public bool? Adx_identity_locallogindisabled
		{
			get => TryGetAttributeValue("adx_identity_locallogindisabled", out bool? value) ? value : null;
			set => this["adx_identity_locallogindisabled"] = value;
		}
		/// <summary>
		/// Attribute of: adx_identity_locallogindisabled<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("adx_identity_locallogindisabledname")]
		public string Adx_identity_locallogindisabledName
		{
			get => FormattedValues.Contains("adx_identity_locallogindisabled") ? FormattedValues["adx_identity_locallogindisabledname"] : null;
		}

		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("adx_identity_lockoutenabled")]
		public bool? Adx_identity_lockoutenabled
		{
			get => TryGetAttributeValue("adx_identity_lockoutenabled", out bool? value) ? value : null;
			set => this["adx_identity_lockoutenabled"] = value;
		}
		/// <summary>
		/// Attribute of: adx_identity_lockoutenabled<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("adx_identity_lockoutenabledname")]
		public string Adx_identity_lockoutenabledName
		{
			get => FormattedValues.Contains("adx_identity_lockoutenabled") ? FormattedValues["adx_identity_lockoutenabledname"] : null;
		}

		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("adx_identity_lockoutenddate")]
		public DateTime? Adx_identity_lockoutenddate
		{
			get => TryGetAttributeValue("adx_identity_lockoutenddate", out DateTime? value) ? value : null;
			set => this["adx_identity_lockoutenddate"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("adx_identity_logonenabled")]
		public bool? Adx_identity_logonenabled
		{
			get => TryGetAttributeValue("adx_identity_logonenabled", out bool? value) ? value : null;
			set => this["adx_identity_logonenabled"] = value;
		}
		/// <summary>
		/// Attribute of: adx_identity_logonenabled<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("adx_identity_logonenabledname")]
		public string Adx_identity_logonenabledName
		{
			get => FormattedValues.Contains("adx_identity_logonenabled") ? FormattedValues["adx_identity_logonenabledname"] : null;
		}

		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("adx_identity_mobilephoneconfirmed")]
		public bool? Adx_identity_mobilephoneconfirmed
		{
			get => TryGetAttributeValue("adx_identity_mobilephoneconfirmed", out bool? value) ? value : null;
			set => this["adx_identity_mobilephoneconfirmed"] = value;
		}
		/// <summary>
		/// Attribute of: adx_identity_mobilephoneconfirmed<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("adx_identity_mobilephoneconfirmedname")]
		public string Adx_identity_mobilephoneconfirmedName
		{
			get => FormattedValues.Contains("adx_identity_mobilephoneconfirmed") ? FormattedValues["adx_identity_mobilephoneconfirmedname"] : null;
		}

		/// <summary>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("adx_identity_newpassword")]
		public string Adx_identity_newpassword
		{
			get => TryGetAttributeValue("adx_identity_newpassword", out string value) ? value : null;
			set => this["adx_identity_newpassword"] = value;
		}
		/// <summary>
		/// Max Length: 128<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("adx_identity_passwordhash")]
		public string Adx_identity_passwordhash
		{
			get => TryGetAttributeValue("adx_identity_passwordhash", out string value) ? value : null;
			set => this["adx_identity_passwordhash"] = value;
		}
		/// <summary>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("adx_identity_securitystamp")]
		public string Adx_identity_securitystamp
		{
			get => TryGetAttributeValue("adx_identity_securitystamp", out string value) ? value : null;
			set => this["adx_identity_securitystamp"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("adx_identity_twofactorenabled")]
		public bool? Adx_identity_twofactorenabled
		{
			get => TryGetAttributeValue("adx_identity_twofactorenabled", out bool? value) ? value : null;
			set => this["adx_identity_twofactorenabled"] = value;
		}
		/// <summary>
		/// Attribute of: adx_identity_twofactorenabled<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("adx_identity_twofactorenabledname")]
		public string Adx_identity_twofactorenabledName
		{
			get => FormattedValues.Contains("adx_identity_twofactorenabled") ? FormattedValues["adx_identity_twofactorenabledname"] : null;
		}

		/// <summary>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("adx_identity_username")]
		public string Adx_identity_username
		{
			get => TryGetAttributeValue("adx_identity_username", out string value) ? value : null;
			set => this["adx_identity_username"] = value;
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
		/// Max Length: 250<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("adx_organizationname")]
		public string Adx_OrganizationName
		{
			get => TryGetAttributeValue("adx_organizationname", out string value) ? value : null;
			set => this["adx_organizationname"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("adx_preferredlcid")]
		public int? Adx_preferredlcid
		{
			get => TryGetAttributeValue("adx_preferredlcid", out int? value) ? value : null;
			set => this["adx_preferredlcid"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("adx_profilealert")]
		public bool? Adx_profilealert
		{
			get => TryGetAttributeValue("adx_profilealert", out bool? value) ? value : null;
			set => this["adx_profilealert"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("adx_profilealertdate")]
		public DateTime? Adx_profilealertdate
		{
			get => TryGetAttributeValue("adx_profilealertdate", out DateTime? value) ? value : null;
			set => this["adx_profilealertdate"] = value;
		}
		/// <summary>
		/// Max Length: 4096<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("adx_profilealertinstructions")]
		public string Adx_profilealertinstructions
		{
			get => TryGetAttributeValue("adx_profilealertinstructions", out string value) ? value : null;
			set => this["adx_profilealertinstructions"] = value;
		}
		/// <summary>
		/// Attribute of: adx_profilealert<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("adx_profilealertname")]
		public string Adx_profilealertName
		{
			get => FormattedValues.Contains("adx_profilealert") ? FormattedValues["adx_profilealertname"] : null;
		}

		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("adx_profileisanonymous")]
		public bool? Adx_ProfileIsAnonymous
		{
			get => TryGetAttributeValue("adx_profileisanonymous", out bool? value) ? value : null;
			set => this["adx_profileisanonymous"] = value;
		}
		/// <summary>
		/// Attribute of: adx_profileisanonymous<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("adx_profileisanonymousname")]
		public string Adx_profileisanonymousName
		{
			get => FormattedValues.Contains("adx_profileisanonymous") ? FormattedValues["adx_profileisanonymousname"] : null;
		}

		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("adx_profilelastactivity")]
		public DateTime? Adx_ProfileLastActivity
		{
			get => TryGetAttributeValue("adx_profilelastactivity", out DateTime? value) ? value : null;
			set => this["adx_profilelastactivity"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("adx_profilemodifiedon")]
		public DateTime? Adx_profilemodifiedon
		{
			get => TryGetAttributeValue("adx_profilemodifiedon", out DateTime? value) ? value : null;
			set => this["adx_profilemodifiedon"] = value;
		}
		/// <summary>
		/// Max Length: 64000<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("adx_publicprofilecopy")]
		public string Adx_PublicProfileCopy
		{
			get => TryGetAttributeValue("adx_publicprofilecopy", out string value) ? value : null;
			set => this["adx_publicprofilecopy"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("adx_timezone")]
		public int? Adx_TimeZone
		{
			get => TryGetAttributeValue("adx_timezone", out int? value) ? value : null;
			set => this["adx_timezone"] = value;
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
		[AttributeLogicalName("anniversary")]
		public DateTime? Anniversary
		{
			get => TryGetAttributeValue("anniversary", out DateTime? value) ? value : null;
			set => this["anniversary"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("annualincome")]
		public decimal? AnnualIncome
		{
			get => TryGetAttributeValue("annualincome", out Money money) ? (decimal?)money.Value : null;
			set => this["annualincome"] = value.HasValue ? new Money(value.Value) : null;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("annualincome_base")]
		public decimal? AnnualIncome_Base
		{
			get => TryGetAttributeValue("annualincome_base", out Money money) ? (decimal?)money.Value : null;
		}
		/// <summary>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("assistantname")]
		public string AssistantName
		{
			get => TryGetAttributeValue("assistantname", out string value) ? value : null;
			set => this["assistantname"] = value;
		}
		/// <summary>
		/// Max Length: 50<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("assistantphone")]
		public string AssistantPhone
		{
			get => TryGetAttributeValue("assistantphone", out string value) ? value : null;
			set => this["assistantphone"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("birthdate")]
		public DateTime? BirthDate
		{
			get => TryGetAttributeValue("birthdate", out DateTime? value) ? value : null;
			set => this["birthdate"] = value;
		}
		/// <summary>
		/// Max Length: 50<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("business2")]
		public string Business2
		{
			get => TryGetAttributeValue("business2", out string value) ? value : null;
			set => this["business2"] = value;
		}
		/// <summary>
		/// Max Length: 50<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("callback")]
		public string Callback
		{
			get => TryGetAttributeValue("callback", out string value) ? value : null;
			set => this["callback"] = value;
		}
		/// <summary>
		/// Max Length: 255<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("childrensnames")]
		public string ChildrensNames
		{
			get => TryGetAttributeValue("childrensnames", out string value) ? value : null;
			set => this["childrensnames"] = value;
		}
		/// <summary>
		/// Max Length: 50<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("company")]
		public string Company
		{
			get => TryGetAttributeValue("company", out string value) ? value : null;
			set => this["company"] = value;
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
		/// Attribute of: createdbyexternalparty<br/>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("createdbyexternalpartyname")]
		public string CreatedByExternalPartyName
		{
			get => FormattedValues.Contains("createdbyexternalparty") ? FormattedValues["createdbyexternalparty"] : null;
		
		}
		/// <summary>
		/// Attribute of: createdbyexternalparty<br/>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("createdbyexternalpartyyominame")]
		public string CreatedByExternalPartyYomiName
		{
			get => FormattedValues.Contains("createdbyexternalparty") ? FormattedValues["createdbyexternalparty"] : null;
		
		}
		/// <summary>
		/// Attribute of: createdby<br/>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("createdbyname")]
		public string CreatedByName
		{
			get => FormattedValues.Contains("createdby") ? FormattedValues["createdby"] : null;
		
		}
		/// <summary>
		/// Attribute of: createdby<br/>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("createdbyyominame")]
		public string CreatedByYomiName
		{
			get => FormattedValues.Contains("createdby") ? FormattedValues["createdby"] : null;
		
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
		/// Attribute of: createdonbehalfby<br/>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("createdonbehalfbyname")]
		public string CreatedOnBehalfByName
		{
			get => FormattedValues.Contains("createdonbehalfby") ? FormattedValues["createdonbehalfby"] : null;
		
		}
		/// <summary>
		/// Attribute of: createdonbehalfby<br/>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("createdonbehalfbyyominame")]
		public string CreatedOnBehalfByYomiName
		{
			get => FormattedValues.Contains("createdonbehalfby") ? FormattedValues["createdonbehalfby"] : null;
		
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
		/// Attribute of: creditonhold<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("creditonholdname")]
		public string CreditOnHoldName
		{
			get => FormattedValues.Contains("creditonhold") ? FormattedValues["creditonholdname"] : null;
		}

		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("customersizecode")]
		public Contact.Meta.OptionSets.CustomerSize? CustomerSizeCode
		{
			get => TryGetAttributeValue("customersizecode", out OptionSetValue opt) && opt != null ? (Contact.Meta.OptionSets.CustomerSize?)opt.Value : null;
			set => this["customersizecode"] = value == null ? null : new OptionSetValue((int)value);
		}
		/// <summary>
		/// Attribute of: customersizecode<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("customersizecodename")]
		public string CustomerSizeCodeName
		{
			get => FormattedValues.Contains("customersizecode") ? FormattedValues["customersizecodename"] : null;
		}

		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("customertypecode")]
		public Contact.Meta.OptionSets.RelationshipType? CustomerTypeCode
		{
			get => TryGetAttributeValue("customertypecode", out OptionSetValue opt) && opt != null ? (Contact.Meta.OptionSets.RelationshipType?)opt.Value : null;
			set => this["customertypecode"] = value == null ? null : new OptionSetValue((int)value);
		}
		/// <summary>
		/// Attribute of: customertypecode<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("customertypecodename")]
		public string CustomerTypeCodeName
		{
			get => FormattedValues.Contains("customertypecode") ? FormattedValues["customertypecodename"] : null;
		}

		/// <summary>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("department")]
		public string Department
		{
			get => TryGetAttributeValue("department", out string value) ? value : null;
			set => this["department"] = value;
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
		/// Attribute of: donotbulkemail<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("donotbulkemailname")]
		public string DoNotBulkEMailName
		{
			get => FormattedValues.Contains("donotbulkemail") ? FormattedValues["donotbulkemailname"] : null;
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
		/// Attribute of: donotbulkpostalmail<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("donotbulkpostalmailname")]
		public string DoNotBulkPostalMailName
		{
			get => FormattedValues.Contains("donotbulkpostalmail") ? FormattedValues["donotbulkpostalmailname"] : null;
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
		/// Attribute of: donotemail<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("donotemailname")]
		public string DoNotEMailName
		{
			get => FormattedValues.Contains("donotemail") ? FormattedValues["donotemailname"] : null;
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
		/// Attribute of: donotfax<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("donotfaxname")]
		public string DoNotFaxName
		{
			get => FormattedValues.Contains("donotfax") ? FormattedValues["donotfaxname"] : null;
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
		/// Attribute of: donotphone<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("donotphonename")]
		public string DoNotPhoneName
		{
			get => FormattedValues.Contains("donotphone") ? FormattedValues["donotphonename"] : null;
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
		/// Attribute of: donotpostalmail<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("donotpostalmailname")]
		public string DoNotPostalMailName
		{
			get => FormattedValues.Contains("donotpostalmail") ? FormattedValues["donotpostalmailname"] : null;
		}

		/// <summary>
		/// Attribute of: donotsendmm<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("donotsendmarketingmaterialname")]
		public string DoNotSendMarketingMaterialName
		{
			get => FormattedValues.Contains("donotsendmm") ? FormattedValues["donotsendmarketingmaterialname"] : null;
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
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("educationcode")]
		public Contact.Meta.OptionSets.Education? EducationCode
		{
			get => TryGetAttributeValue("educationcode", out OptionSetValue opt) && opt != null ? (Contact.Meta.OptionSets.Education?)opt.Value : null;
			set => this["educationcode"] = value == null ? null : new OptionSetValue((int)value);
		}
		/// <summary>
		/// Attribute of: educationcode<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("educationcodename")]
		public string EducationCodeName
		{
			get => FormattedValues.Contains("educationcode") ? FormattedValues["educationcodename"] : null;
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
		/// Max Length: 50<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("employeeid")]
		public string EmployeeId
		{
			get => TryGetAttributeValue("employeeid", out string value) ? value : null;
			set => this["employeeid"] = value;
		}
		/// <summary>
		/// Attribute of: entityimageid<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("entityimage")]
		public byte[] EntityImage
		{
			get => TryGetAttributeValue("entityimage", out byte[] value) ? value : null;
			set => this["entityimage"] = value;
		}

		/// <summary>
		/// Attribute of: entityimageid<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("entityimage_timestamp")]
		public long? EntityImage_Timestamp
		{
			get => TryGetAttributeValue("entityimage_timestamp", out long value) ? (long?)value : null;
		}
		/// <summary>
		/// Attribute of: entityimageid<br/>
		/// Max Length: 200<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("entityimage_url")]
		public string EntityImage_URL
		{
			get => TryGetAttributeValue("entityimage_url", out string value) ? value : null;
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
		[AttributeLogicalName("externaluseridentifier")]
		public string ExternalUserIdentifier
		{
			get => TryGetAttributeValue("externaluseridentifier", out string value) ? value : null;
			set => this["externaluseridentifier"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("familystatuscode")]
		public Contact.Meta.OptionSets.MaritalStatus? FamilyStatusCode
		{
			get => TryGetAttributeValue("familystatuscode", out OptionSetValue opt) && opt != null ? (Contact.Meta.OptionSets.MaritalStatus?)opt.Value : null;
			set => this["familystatuscode"] = value == null ? null : new OptionSetValue((int)value);
		}
		/// <summary>
		/// Attribute of: familystatuscode<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("familystatuscodename")]
		public string FamilyStatusCodeName
		{
			get => FormattedValues.Contains("familystatuscode") ? FormattedValues["familystatuscodename"] : null;
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
		/// Max Length: 50<br/>
		/// Required Level: Recommended<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("firstname")]
		public string FirstName
		{
			get => TryGetAttributeValue("firstname", out string value) ? value : null;
			set => this["firstname"] = value;
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
		/// Attribute of: followemail<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("followemailname")]
		public string FollowEmailName
		{
			get => FormattedValues.Contains("followemail") ? FormattedValues["followemailname"] : null;
		}

		/// <summary>
		/// Max Length: 200<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("ftpsiteurl")]
		public string FtpSiteUrl
		{
			get => TryGetAttributeValue("ftpsiteurl", out string value) ? value : null;
			set => this["ftpsiteurl"] = value;
		}
		/// <summary>
		/// Max Length: 160<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("fullname")]
		public string FullName
		{
			get => TryGetAttributeValue("fullname", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("gendercode")]
		public Contact.Meta.OptionSets.Gender? GenderCode
		{
			get => TryGetAttributeValue("gendercode", out OptionSetValue opt) && opt != null ? (Contact.Meta.OptionSets.Gender?)opt.Value : null;
			set => this["gendercode"] = value == null ? null : new OptionSetValue((int)value);
		}
		/// <summary>
		/// Attribute of: gendercode<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("gendercodename")]
		public string GenderCodeName
		{
			get => FormattedValues.Contains("gendercode") ? FormattedValues["gendercodename"] : null;
		}

		/// <summary>
		/// Max Length: 50<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("governmentid")]
		public string GovernmentId
		{
			get => TryGetAttributeValue("governmentid", out string value) ? value : null;
			set => this["governmentid"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("haschildrencode")]
		public Contact.Meta.OptionSets.HasChildren? HasChildrenCode
		{
			get => TryGetAttributeValue("haschildrencode", out OptionSetValue opt) && opt != null ? (Contact.Meta.OptionSets.HasChildren?)opt.Value : null;
			set => this["haschildrencode"] = value == null ? null : new OptionSetValue((int)value);
		}
		/// <summary>
		/// Attribute of: haschildrencode<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("haschildrencodename")]
		public string HasChildrenCodeName
		{
			get => FormattedValues.Contains("haschildrencode") ? FormattedValues["haschildrencodename"] : null;
		}

		/// <summary>
		/// Max Length: 50<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("home2")]
		public string Home2
		{
			get => TryGetAttributeValue("home2", out string value) ? value : null;
			set => this["home2"] = value;
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
		[AttributeLogicalName("isbackofficecustomer")]
		public bool? IsBackofficeCustomer
		{
			get => TryGetAttributeValue("isbackofficecustomer", out bool? value) ? value : null;
			set => this["isbackofficecustomer"] = value;
		}
		/// <summary>
		/// Attribute of: isbackofficecustomer<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("isbackofficecustomername")]
		public string IsBackofficeCustomerName
		{
			get => FormattedValues.Contains("isbackofficecustomer") ? FormattedValues["isbackofficecustomername"] : null;
		}

		/// <summary>
		/// Attribute of: isprivate<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("isprivatename")]
		public string IsPrivateName
		{
			get => FormattedValues.Contains("isprivate") ? FormattedValues["isprivatename"] : null;
		}

		/// <summary>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("jobtitle")]
		public string JobTitle
		{
			get => TryGetAttributeValue("jobtitle", out string value) ? value : null;
			set => this["jobtitle"] = value;
		}
		/// <summary>
		/// Max Length: 50<br/>
		/// Required Level: ApplicationRequired<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("lastname")]
		public string LastName
		{
			get => TryGetAttributeValue("lastname", out string value) ? value : null;
			set => this["lastname"] = value;
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
		[AttributeLogicalName("leadsourcecode")]
		public Contact.Meta.OptionSets.LeadSource? LeadSourceCode
		{
			get => TryGetAttributeValue("leadsourcecode", out OptionSetValue opt) && opt != null ? (Contact.Meta.OptionSets.LeadSource?)opt.Value : null;
			set => this["leadsourcecode"] = value == null ? null : new OptionSetValue((int)value);
		}
		/// <summary>
		/// Attribute of: leadsourcecode<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("leadsourcecodename")]
		public string LeadSourceCodeName
		{
			get => FormattedValues.Contains("leadsourcecode") ? FormattedValues["leadsourcecodename"] : null;
		}

		/// <summary>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("managername")]
		public string ManagerName
		{
			get => TryGetAttributeValue("managername", out string value) ? value : null;
			set => this["managername"] = value;
		}
		/// <summary>
		/// Max Length: 50<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("managerphone")]
		public string ManagerPhone
		{
			get => TryGetAttributeValue("managerphone", out string value) ? value : null;
			set => this["managerphone"] = value;
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
		/// Attribute of: marketingonly<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("marketingonlyname")]
		public string MarketingOnlyName
		{
			get => FormattedValues.Contains("marketingonly") ? FormattedValues["marketingonlyname"] : null;
		}

		/// <summary>
		/// Attribute of: masterid<br/>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("mastercontactidname")]
		public string MasterContactIdName
		{
			get => FormattedValues.Contains("masterid") ? FormattedValues["masterid"] : null;
		
		}
		/// <summary>
		/// Attribute of: masterid<br/>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("mastercontactidyominame")]
		public string MasterContactIdYomiName
		{
			get => FormattedValues.Contains("masterid") ? FormattedValues["masterid"] : null;
		
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// Targets: contact<br/>
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
		/// Attribute of: merged<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("mergedname")]
		public string MergedName
		{
			get => FormattedValues.Contains("merged") ? FormattedValues["mergedname"] : null;
		}

		/// <summary>
		/// Max Length: 50<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("middlename")]
		public string MiddleName
		{
			get => TryGetAttributeValue("middlename", out string value) ? value : null;
			set => this["middlename"] = value;
		}
		/// <summary>
		/// Max Length: 50<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("mobilephone")]
		public string MobilePhone
		{
			get => TryGetAttributeValue("mobilephone", out string value) ? value : null;
			set => this["mobilephone"] = value;
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
		/// Attribute of: modifiedbyexternalparty<br/>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("modifiedbyexternalpartyname")]
		public string ModifiedByExternalPartyName
		{
			get => FormattedValues.Contains("modifiedbyexternalparty") ? FormattedValues["modifiedbyexternalparty"] : null;
		
		}
		/// <summary>
		/// Attribute of: modifiedbyexternalparty<br/>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("modifiedbyexternalpartyyominame")]
		public string ModifiedByExternalPartyYomiName
		{
			get => FormattedValues.Contains("modifiedbyexternalparty") ? FormattedValues["modifiedbyexternalparty"] : null;
		
		}
		/// <summary>
		/// Attribute of: modifiedby<br/>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("modifiedbyname")]
		public string ModifiedByName
		{
			get => FormattedValues.Contains("modifiedby") ? FormattedValues["modifiedby"] : null;
		
		}
		/// <summary>
		/// Attribute of: modifiedby<br/>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("modifiedbyyominame")]
		public string ModifiedByYomiName
		{
			get => FormattedValues.Contains("modifiedby") ? FormattedValues["modifiedby"] : null;
		
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
		/// Attribute of: modifiedonbehalfby<br/>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("modifiedonbehalfbyname")]
		public string ModifiedOnBehalfByName
		{
			get => FormattedValues.Contains("modifiedonbehalfby") ? FormattedValues["modifiedonbehalfby"] : null;
		
		}
		/// <summary>
		/// Attribute of: modifiedonbehalfby<br/>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("modifiedonbehalfbyyominame")]
		public string ModifiedOnBehalfByYomiName
		{
			get => FormattedValues.Contains("modifiedonbehalfby") ? FormattedValues["modifiedonbehalfby"] : null;
		
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
				if (!Contact.Meta.Fields.Msa_managingpartneridTargets.Contains(value.LogicalName))
				{
					throw new InvalidPluginExecutionException($"{value.LogicalName}:{value.Id} is not a valid Msa_managingpartnerid. The only valid references are account");			
				}
				this["msa_managingpartnerid"] = value;
			}
		}
		/// <summary>
		/// Attribute of: msa_managingpartnerid<br/>
		/// Max Length: 160<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("msa_managingpartneridname")]
		public string Msa_managingpartneridName
		{
			get => FormattedValues.Contains("msa_managingpartnerid") ? FormattedValues["msa_managingpartnerid"] : null;
		
		}
		/// <summary>
		/// Attribute of: msa_managingpartnerid<br/>
		/// Max Length: 160<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("msa_managingpartneridyominame")]
		public string Msa_managingpartneridYomiName
		{
			get => FormattedValues.Contains("msa_managingpartnerid") ? FormattedValues["msa_managingpartnerid"] : null;
		
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("msdyn_disablewebtracking")]
		public bool? Msdyn_disablewebtracking
		{
			get => TryGetAttributeValue("msdyn_disablewebtracking", out bool? value) ? value : null;
			set => this["msdyn_disablewebtracking"] = value;
		}
		/// <summary>
		/// Attribute of: msdyn_disablewebtracking<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("msdyn_disablewebtrackingname")]
		public string Msdyn_disablewebtrackingName
		{
			get => FormattedValues.Contains("msdyn_disablewebtracking") ? FormattedValues["msdyn_disablewebtrackingname"] : null;
		}

		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("msdyn_isminor")]
		public bool? Msdyn_isminor
		{
			get => TryGetAttributeValue("msdyn_isminor", out bool? value) ? value : null;
			set => this["msdyn_isminor"] = value;
		}
		/// <summary>
		/// Attribute of: msdyn_isminor<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("msdyn_isminorname")]
		public string Msdyn_isminorName
		{
			get => FormattedValues.Contains("msdyn_isminor") ? FormattedValues["msdyn_isminorname"] : null;
		}

		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("msdyn_isminorwithparentalconsent")]
		public bool? Msdyn_isminorwithparentalconsent
		{
			get => TryGetAttributeValue("msdyn_isminorwithparentalconsent", out bool? value) ? value : null;
			set => this["msdyn_isminorwithparentalconsent"] = value;
		}
		/// <summary>
		/// Attribute of: msdyn_isminorwithparentalconsent<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("msdyn_isminorwithparentalconsentname")]
		public string Msdyn_isminorwithparentalconsentName
		{
			get => FormattedValues.Contains("msdyn_isminorwithparentalconsent") ? FormattedValues["msdyn_isminorwithparentalconsentname"] : null;
		}

		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("msdyn_portaltermsagreementdate")]
		public DateTime? Msdyn_portaltermsagreementdate
		{
			get => TryGetAttributeValue("msdyn_portaltermsagreementdate", out DateTime? value) ? value : null;
			set => this["msdyn_portaltermsagreementdate"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("msft_datastate")]
		public Contact.Meta.OptionSets.Datastate? Msft_DataState
		{
			get => TryGetAttributeValue("msft_datastate", out OptionSetValue opt) && opt != null ? (Contact.Meta.OptionSets.Datastate?)opt.Value : null;
		}
		/// <summary>
		/// Attribute of: msft_datastate<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("msft_datastatename")]
		public string Msft_datastateName
		{
			get => FormattedValues.Contains("msft_datastate") ? FormattedValues["msft_datastatename"] : null;
		}

		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("mspp_userpreferredlcid")]
		public Contact.Meta.OptionSets.PowerPagesLanguages? Mspp_userpreferredlcid
		{
			get => TryGetAttributeValue("mspp_userpreferredlcid", out OptionSetValue opt) && opt != null ? (Contact.Meta.OptionSets.PowerPagesLanguages?)opt.Value : null;
			set => this["mspp_userpreferredlcid"] = value == null ? null : new OptionSetValue((int)value);
		}
		/// <summary>
		/// Attribute of: mspp_userpreferredlcid<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("mspp_userpreferredlcidname")]
		public string Mspp_userpreferredlcidName
		{
			get => FormattedValues.Contains("mspp_userpreferredlcid") ? FormattedValues["mspp_userpreferredlcidname"] : null;
		}

		/// <summary>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("nickname")]
		public string NickName
		{
			get => TryGetAttributeValue("nickname", out string value) ? value : null;
			set => this["nickname"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("numberofchildren")]
		public int? NumberOfChildren
		{
			get => TryGetAttributeValue("numberofchildren", out int? value) ? value : null;
			set => this["numberofchildren"] = value;
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
		/// Attribute of: ownerid<br/>
		/// Max Length: 100<br/>
		/// Required Level: SystemRequired<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("owneridname")]
		public string OwnerIdName
		{
			get => TryGetAttributeValue("owneridname", out string value) ? value : null;
		}
		/// <summary>
		/// Attribute of: ownerid<br/>
		/// Max Length: 100<br/>
		/// Required Level: SystemRequired<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("owneridyominame")]
		public string OwnerIdYomiName
		{
			get => TryGetAttributeValue("owneridyominame", out string value) ? value : null;
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
		/// Attribute of: owningbusinessunit<br/>
		/// Max Length: 160<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("owningbusinessunitname")]
		public string OwningBusinessUnitName
		{
			get => FormattedValues.Contains("owningbusinessunit") ? FormattedValues["owningbusinessunit"] : null;
		
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
		/// Max Length: 50<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("pager")]
		public string Pager
		{
			get => TryGetAttributeValue("pager", out string value) ? value : null;
			set => this["pager"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// Targets: contact<br/>
		/// </summary>
		[AttributeLogicalName("parentcontactid")]
		public EntityReference ParentContactId
		{
			get => TryGetAttributeValue("parentcontactid", out EntityReference value) ? value : null;
		}
		/// <summary>
		/// Attribute of: parentcontactid<br/>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("parentcontactidname")]
		public string ParentContactIdName
		{
			get => FormattedValues.Contains("parentcontactid") ? FormattedValues["parentcontactid"] : null;
		
		}
		/// <summary>
		/// Attribute of: parentcontactid<br/>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("parentcontactidyominame")]
		public string ParentContactIdYomiName
		{
			get => FormattedValues.Contains("parentcontactid") ? FormattedValues["parentcontactid"] : null;
		
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// Targets: account,contact<br/>
		/// </summary>
		[AttributeLogicalName("parentcustomerid")]
		public EntityReference ParentCustomerId
		{
			get => TryGetAttributeValue("parentcustomerid", out EntityReference value) ? value : null;
			set
			{
				if (!Contact.Meta.Fields.ParentCustomerIdTargets.Contains(value.LogicalName))
				{
					throw new InvalidPluginExecutionException($"{value.LogicalName}:{value.Id} is not a valid ParentCustomerId. The only valid references are account, contact");			
				}
				this["parentcustomerid"] = value;
			}
		}
		/// <summary>
		/// Attribute of: parentcustomerid<br/>
		/// Max Length: 160<br/>
		/// Required Level: ApplicationRequired<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("parentcustomeridname")]
		public string ParentCustomerIdName
		{
			get => FormattedValues.Contains("parentcustomerid") ? FormattedValues["parentcustomerid"] : null;
		
		}
		/// <summary>
		/// Attribute of: parentcustomerid<br/>
		/// Max Length: 450<br/>
		/// Required Level: ApplicationRequired<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("parentcustomeridyominame")]
		public string ParentCustomerIdYomiName
		{
			get => FormattedValues.Contains("parentcustomerid") ? FormattedValues["parentcustomerid"] : null;
		
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
		/// Attribute of: participatesinworkflow<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("participatesinworkflowname")]
		public string ParticipatesInWorkflowName
		{
			get => FormattedValues.Contains("participatesinworkflow") ? FormattedValues["participatesinworkflowname"] : null;
		}

		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("paymenttermscode")]
		public Contact.Meta.OptionSets.PaymentTerms? PaymentTermsCode
		{
			get => TryGetAttributeValue("paymenttermscode", out OptionSetValue opt) && opt != null ? (Contact.Meta.OptionSets.PaymentTerms?)opt.Value : null;
			set => this["paymenttermscode"] = value == null ? null : new OptionSetValue((int)value);
		}
		/// <summary>
		/// Attribute of: paymenttermscode<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("paymenttermscodename")]
		public string PaymentTermsCodeName
		{
			get => FormattedValues.Contains("paymenttermscode") ? FormattedValues["paymenttermscodename"] : null;
		}

		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("preferredappointmentdaycode")]
		public Contact.Meta.OptionSets.PreferredDay? PreferredAppointmentDayCode
		{
			get => TryGetAttributeValue("preferredappointmentdaycode", out OptionSetValue opt) && opt != null ? (Contact.Meta.OptionSets.PreferredDay?)opt.Value : null;
			set => this["preferredappointmentdaycode"] = value == null ? null : new OptionSetValue((int)value);
		}
		/// <summary>
		/// Attribute of: preferredappointmentdaycode<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("preferredappointmentdaycodename")]
		public string PreferredAppointmentDayCodeName
		{
			get => FormattedValues.Contains("preferredappointmentdaycode") ? FormattedValues["preferredappointmentdaycodename"] : null;
		}

		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("preferredappointmenttimecode")]
		public Contact.Meta.OptionSets.PreferredTime? PreferredAppointmentTimeCode
		{
			get => TryGetAttributeValue("preferredappointmenttimecode", out OptionSetValue opt) && opt != null ? (Contact.Meta.OptionSets.PreferredTime?)opt.Value : null;
			set => this["preferredappointmenttimecode"] = value == null ? null : new OptionSetValue((int)value);
		}
		/// <summary>
		/// Attribute of: preferredappointmenttimecode<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("preferredappointmenttimecodename")]
		public string PreferredAppointmentTimeCodeName
		{
			get => FormattedValues.Contains("preferredappointmenttimecode") ? FormattedValues["preferredappointmenttimecodename"] : null;
		}

		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("preferredcontactmethodcode")]
		public Contact.Meta.OptionSets.PreferredMethodOfContact? PreferredContactMethodCode
		{
			get => TryGetAttributeValue("preferredcontactmethodcode", out OptionSetValue opt) && opt != null ? (Contact.Meta.OptionSets.PreferredMethodOfContact?)opt.Value : null;
			set => this["preferredcontactmethodcode"] = value == null ? null : new OptionSetValue((int)value);
		}
		/// <summary>
		/// Attribute of: preferredcontactmethodcode<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("preferredcontactmethodcodename")]
		public string PreferredContactMethodCodeName
		{
			get => FormattedValues.Contains("preferredcontactmethodcode") ? FormattedValues["preferredcontactmethodcodename"] : null;
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
				if (!Contact.Meta.Fields.PreferredSystemUserIdTargets.Contains(value.LogicalName))
				{
					throw new InvalidPluginExecutionException($"{value.LogicalName}:{value.Id} is not a valid PreferredSystemUserId. The only valid references are systemuser");			
				}
				this["preferredsystemuserid"] = value;
			}
		}
		/// <summary>
		/// Attribute of: preferredsystemuserid<br/>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("preferredsystemuseridname")]
		public string PreferredSystemUserIdName
		{
			get => FormattedValues.Contains("preferredsystemuserid") ? FormattedValues["preferredsystemuserid"] : null;
		
		}
		/// <summary>
		/// Attribute of: preferredsystemuserid<br/>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("preferredsystemuseridyominame")]
		public string PreferredSystemUserIdYomiName
		{
			get => FormattedValues.Contains("preferredsystemuserid") ? FormattedValues["preferredsystemuserid"] : null;
		
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
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("salutation")]
		public string Salutation
		{
			get => TryGetAttributeValue("salutation", out string value) ? value : null;
			set => this["salutation"] = value;
		}
		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("shippingmethodcode")]
		public Contact.Meta.OptionSets.ShippingMethod? ShippingMethodCode
		{
			get => TryGetAttributeValue("shippingmethodcode", out OptionSetValue opt) && opt != null ? (Contact.Meta.OptionSets.ShippingMethod?)opt.Value : null;
			set => this["shippingmethodcode"] = value == null ? null : new OptionSetValue((int)value);
		}
		/// <summary>
		/// Attribute of: shippingmethodcode<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("shippingmethodcodename")]
		public string ShippingMethodCodeName
		{
			get => FormattedValues.Contains("shippingmethodcode") ? FormattedValues["shippingmethodcodename"] : null;
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
				if (!Contact.Meta.Fields.SLAIdTargets.Contains(value.LogicalName))
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
		/// Attribute of: slainvokedid<br/>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("slainvokedidname")]
		public string SLAInvokedIdName
		{
			get => FormattedValues.Contains("slainvokedid") ? FormattedValues["slainvokedid"] : null;
		
		}
		/// <summary>
		/// Attribute of: slaid<br/>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("slaname")]
		public string SLAName
		{
			get => FormattedValues.Contains("slaid") ? FormattedValues["slaid"] : null;
		
		}
		/// <summary>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("spousesname")]
		public string SpousesName
		{
			get => TryGetAttributeValue("spousesname", out string value) ? value : null;
			set => this["spousesname"] = value;
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
		public Contact.Meta.OptionSets.Status? StateCode
		{
			get => TryGetAttributeValue("statecode", out OptionSetValue opt) && opt != null ? (Contact.Meta.OptionSets.Status?)Enum.ToObject(typeof(Contact.Meta.OptionSets.Status), opt.Value) : null;
			set => this["statecode"] = value == null ? null : new OptionSetValue(((IConvertible)value).ToInt32((IFormatProvider)CultureInfo.InvariantCulture));
		}
		/// <summary>
		/// Attribute of: statecode<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("statecodename")]
		public string StateCodeName
		{
			get => FormattedValues.Contains("statecode") ? FormattedValues["statecodename"] : null;
		}

		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("statuscode")]
		public Contact.Meta.OptionSets.StatusReason? StatusCode
		{
			get => TryGetAttributeValue("statuscode", out OptionSetValue opt) && opt != null ? (Contact.Meta.OptionSets.StatusReason?)Enum.ToObject(typeof(Contact.Meta.OptionSets.StatusReason), opt.Value) : null;
			set => this["statuscode"] = value == null ? null : new OptionSetValue(((IConvertible)value).ToInt32((IFormatProvider)CultureInfo.InvariantCulture));
		}
		/// <summary>
		/// Attribute of: statuscode<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("statuscodename")]
		public string StatusCodeName
		{
			get => FormattedValues.Contains("statuscode") ? FormattedValues["statuscodename"] : null;
		}

		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create<br/>
		/// </summary>
		[AttributeLogicalName("subscriptionid")]
		public Guid? SubscriptionId
		{
			set => this["subscriptionid"] = value;
		}
		/// <summary>
		/// Max Length: 10<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("suffix")]
		public string Suffix
		{
			get => TryGetAttributeValue("suffix", out string value) ? value : null;
			set => this["suffix"] = value;
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
		public Contact.Meta.OptionSets.Territory? TerritoryCode
		{
			get => TryGetAttributeValue("territorycode", out OptionSetValue opt) && opt != null ? (Contact.Meta.OptionSets.Territory?)opt.Value : null;
			set => this["territorycode"] = value == null ? null : new OptionSetValue((int)value);
		}
		/// <summary>
		/// Attribute of: territorycode<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("territorycodename")]
		public string TerritoryCodeName
		{
			get => FormattedValues.Contains("territorycode") ? FormattedValues["territorycodename"] : null;
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
				if (!Contact.Meta.Fields.TransactionCurrencyIdTargets.Contains(value.LogicalName))
				{
					throw new InvalidPluginExecutionException($"{value.LogicalName}:{value.Id} is not a valid TransactionCurrencyId. The only valid references are transactioncurrency");			
				}
				this["transactioncurrencyid"] = value;
			}
		}
		/// <summary>
		/// Attribute of: transactioncurrencyid<br/>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("transactioncurrencyidname")]
		public string TransactionCurrencyIdName
		{
			get => FormattedValues.Contains("transactioncurrencyid") ? FormattedValues["transactioncurrencyid"] : null;
		
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
		public string WebSiteUrl
		{
			get => TryGetAttributeValue("websiteurl", out string value) ? value : null;
			set => this["websiteurl"] = value;
		}
		/// <summary>
		/// Max Length: 150<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("yomifirstname")]
		public string YomiFirstName
		{
			get => TryGetAttributeValue("yomifirstname", out string value) ? value : null;
			set => this["yomifirstname"] = value;
		}
		/// <summary>
		/// Max Length: 450<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("yomifullname")]
		public string YomiFullName
		{
			get => TryGetAttributeValue("yomifullname", out string value) ? value : null;
		}
		/// <summary>
		/// Max Length: 150<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("yomilastname")]
		public string YomiLastName
		{
			get => TryGetAttributeValue("yomilastname", out string value) ? value : null;
			set => this["yomilastname"] = value;
		}
		/// <summary>
		/// Max Length: 150<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("yomimiddlename")]
		public string YomiMiddleName
		{
			get => TryGetAttributeValue("yomimiddlename", out string value) ? value : null;
			set => this["yomimiddlename"] = value;
		}
		public Contact() : base(Meta.EntityLogicalName) { }
		public Contact(Guid id) : base(Meta.EntityLogicalName, id) { }
		public Contact(string keyName, object keyValue) : base(Meta.EntityLogicalName, keyName, keyValue) { }
		public Contact(KeyAttributeCollection keyAttributes) : base(Meta.EntityLogicalName, keyAttributes) { }
	}
	/// <summary>
	/// Display Name: Partner
	/// </summary>
	[GeneratedCode("TemplatedCodeGenerator", "1.3.3.0")]
	[EntityLogicalName("account")]
	[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public partial class Account : Entity
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
				public const string AccountCategoryCodeName = "accountcategorycodename";
				public const string AccountClassificationCode = "accountclassificationcode";
				public const string AccountClassificationCodeName = "accountclassificationcodename";
				public const string AccountNumber = "accountnumber";
				public const string AccountRatingCode = "accountratingcode";
				public const string AccountRatingCodeName = "accountratingcodename";
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
				public const string Cr22a_SupportedLanguages = "cr22a_supportedlanguages";
				public const string Cr22a_supportedlanguagesName = "cr22a_supportedlanguagesname";
				public const string CreatedBy = "createdby";
				public static readonly ReadOnlyCollection<string> CreatedByTargets = new ReadOnlyCollection<string>(new string[] { "systemuser" });
				public const string CreatedByExternalParty = "createdbyexternalparty";
				public static readonly ReadOnlyCollection<string> CreatedByExternalPartyTargets = new ReadOnlyCollection<string>(new string[] { "externalparty" });
				public const string CreatedByExternalPartyName = "createdbyexternalpartyname";
				public const string CreatedByExternalPartyYomiName = "createdbyexternalpartyyominame";
				public const string CreatedByName = "createdbyname";
				public const string CreatedByYomiName = "createdbyyominame";
				public const string CreatedOn = "createdon";
				public const string CreatedOnBehalfBy = "createdonbehalfby";
				public static readonly ReadOnlyCollection<string> CreatedOnBehalfByTargets = new ReadOnlyCollection<string>(new string[] { "systemuser" });
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
				public static readonly ReadOnlyCollection<string> MasterIdTargets = new ReadOnlyCollection<string>(new string[] { "account" });
				public const string Merged = "merged";
				public const string MergedName = "mergedname";
				public const string ModifiedBy = "modifiedby";
				public static readonly ReadOnlyCollection<string> ModifiedByTargets = new ReadOnlyCollection<string>(new string[] { "systemuser" });
				public const string ModifiedByExternalParty = "modifiedbyexternalparty";
				public static readonly ReadOnlyCollection<string> ModifiedByExternalPartyTargets = new ReadOnlyCollection<string>(new string[] { "externalparty" });
				public const string ModifiedByExternalPartyName = "modifiedbyexternalpartyname";
				public const string ModifiedByExternalPartyYomiName = "modifiedbyexternalpartyyominame";
				public const string ModifiedByName = "modifiedbyname";
				public const string ModifiedByYomiName = "modifiedbyyominame";
				public const string ModifiedOn = "modifiedon";
				public const string ModifiedOnBehalfBy = "modifiedonbehalfby";
				public static readonly ReadOnlyCollection<string> ModifiedOnBehalfByTargets = new ReadOnlyCollection<string>(new string[] { "systemuser" });
				public const string ModifiedOnBehalfByName = "modifiedonbehalfbyname";
				public const string ModifiedOnBehalfByYomiName = "modifiedonbehalfbyyominame";
				public const string Msa_managingpartnerid = "msa_managingpartnerid";
				public static readonly ReadOnlyCollection<string> Msa_managingpartneridTargets = new ReadOnlyCollection<string>(new string[] { "account" });
				public const string Msa_managingpartneridName = "msa_managingpartneridname";
				public const string Msa_managingpartneridYomiName = "msa_managingpartneridyominame";
				public const string Msft_DataState = "msft_datastate";
				public const string Msft_datastateName = "msft_datastatename";
				public const string Name = "name";
				public const string NumberOfEmployees = "numberofemployees";
				public const string OnHoldTime = "onholdtime";
				public const string OverriddenCreatedOn = "overriddencreatedon";
				public const string OwnerId = "ownerid";
				public const string OwnerIdName = "owneridname";
				public const string OwnerIdYomiName = "owneridyominame";
				public const string OwnershipCode = "ownershipcode";
				public const string OwnershipCodeName = "ownershipcodename";
				public const string OwningBusinessUnit = "owningbusinessunit";
				public static readonly ReadOnlyCollection<string> OwningBusinessUnitTargets = new ReadOnlyCollection<string>(new string[] { "businessunit" });
				public const string OwningBusinessUnitName = "owningbusinessunitname";
				public const string OwningTeam = "owningteam";
				public static readonly ReadOnlyCollection<string> OwningTeamTargets = new ReadOnlyCollection<string>(new string[] { "team" });
				public const string OwningUser = "owninguser";
				public static readonly ReadOnlyCollection<string> OwningUserTargets = new ReadOnlyCollection<string>(new string[] { "systemuser" });
				public const string ParentAccountId = "parentaccountid";
				public static readonly ReadOnlyCollection<string> ParentAccountIdTargets = new ReadOnlyCollection<string>(new string[] { "account" });
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
				public static readonly ReadOnlyCollection<string> PreferredSystemUserIdTargets = new ReadOnlyCollection<string>(new string[] { "systemuser" });
				public const string PreferredSystemUserIdName = "preferredsystemuseridname";
				public const string PreferredSystemUserIdYomiName = "preferredsystemuseridyominame";
				public const string PrimaryContactId = "primarycontactid";
				public static readonly ReadOnlyCollection<string> PrimaryContactIdTargets = new ReadOnlyCollection<string>(new string[] { "contact" });
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
				public static readonly ReadOnlyCollection<string> SLAIdTargets = new ReadOnlyCollection<string>(new string[] { "sla" });
				public const string SLAInvokedId = "slainvokedid";
				public static readonly ReadOnlyCollection<string> SLAInvokedIdTargets = new ReadOnlyCollection<string>(new string[] { "sla" });
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
				public static readonly ReadOnlyCollection<string> TransactionCurrencyIdTargets = new ReadOnlyCollection<string>(new string[] { "transactioncurrency" });
				public const string TransactionCurrencyIdName = "transactioncurrencyidname";
				public const string TraversedPath = "traversedpath";
				public const string UTCConversionTimeZoneCode = "utcconversiontimezonecode";
				public const string VersionNumber = "versionnumber";
				public const string WebSiteURL = "websiteurl";
				public const string YomiName = "yominame";

				private static readonly Dictionary<string, string> _fieldMap = new Dictionary<string, string>
				{
					[nameof(AccountCategoryCode)] = AccountCategoryCode,
					[nameof(AccountCategoryCodeName)] = AccountCategoryCodeName,
					[nameof(AccountClassificationCode)] = AccountClassificationCode,
					[nameof(AccountClassificationCodeName)] = AccountClassificationCodeName,
					[nameof(AccountNumber)] = AccountNumber,
					[nameof(AccountRatingCode)] = AccountRatingCode,
					[nameof(AccountRatingCodeName)] = AccountRatingCodeName,
					[nameof(Address1_AddressTypeCode)] = Address1_AddressTypeCode,
					[nameof(Address1_AddressTypeCodeName)] = Address1_AddressTypeCodeName,
					[nameof(Address1_City)] = Address1_City,
					[nameof(Address1_Composite)] = Address1_Composite,
					[nameof(Address1_Country)] = Address1_Country,
					[nameof(Address1_County)] = Address1_County,
					[nameof(Address1_Fax)] = Address1_Fax,
					[nameof(Address1_FreightTermsCode)] = Address1_FreightTermsCode,
					[nameof(Address1_FreightTermsCodeName)] = Address1_FreightTermsCodeName,
					[nameof(Address1_Latitude)] = Address1_Latitude,
					[nameof(Address1_Line1)] = Address1_Line1,
					[nameof(Address1_Line2)] = Address1_Line2,
					[nameof(Address1_Line3)] = Address1_Line3,
					[nameof(Address1_Longitude)] = Address1_Longitude,
					[nameof(Address1_Name)] = Address1_Name,
					[nameof(Address1_PostalCode)] = Address1_PostalCode,
					[nameof(Address1_PostOfficeBox)] = Address1_PostOfficeBox,
					[nameof(Address1_PrimaryContactName)] = Address1_PrimaryContactName,
					[nameof(Address1_ShippingMethodCode)] = Address1_ShippingMethodCode,
					[nameof(Address1_ShippingMethodCodeName)] = Address1_ShippingMethodCodeName,
					[nameof(Address1_StateOrProvince)] = Address1_StateOrProvince,
					[nameof(Address1_Telephone1)] = Address1_Telephone1,
					[nameof(Address1_Telephone2)] = Address1_Telephone2,
					[nameof(Address1_Telephone3)] = Address1_Telephone3,
					[nameof(Address1_UPSZone)] = Address1_UPSZone,
					[nameof(Address1_UTCOffset)] = Address1_UTCOffset,
					[nameof(Address2_AddressTypeCode)] = Address2_AddressTypeCode,
					[nameof(Address2_AddressTypeCodeName)] = Address2_AddressTypeCodeName,
					[nameof(Address2_City)] = Address2_City,
					[nameof(Address2_Composite)] = Address2_Composite,
					[nameof(Address2_Country)] = Address2_Country,
					[nameof(Address2_County)] = Address2_County,
					[nameof(Address2_Fax)] = Address2_Fax,
					[nameof(Address2_FreightTermsCode)] = Address2_FreightTermsCode,
					[nameof(Address2_FreightTermsCodeName)] = Address2_FreightTermsCodeName,
					[nameof(Address2_Latitude)] = Address2_Latitude,
					[nameof(Address2_Line1)] = Address2_Line1,
					[nameof(Address2_Line2)] = Address2_Line2,
					[nameof(Address2_Line3)] = Address2_Line3,
					[nameof(Address2_Longitude)] = Address2_Longitude,
					[nameof(Address2_Name)] = Address2_Name,
					[nameof(Address2_PostalCode)] = Address2_PostalCode,
					[nameof(Address2_PostOfficeBox)] = Address2_PostOfficeBox,
					[nameof(Address2_PrimaryContactName)] = Address2_PrimaryContactName,
					[nameof(Address2_ShippingMethodCode)] = Address2_ShippingMethodCode,
					[nameof(Address2_ShippingMethodCodeName)] = Address2_ShippingMethodCodeName,
					[nameof(Address2_StateOrProvince)] = Address2_StateOrProvince,
					[nameof(Address2_Telephone1)] = Address2_Telephone1,
					[nameof(Address2_Telephone2)] = Address2_Telephone2,
					[nameof(Address2_Telephone3)] = Address2_Telephone3,
					[nameof(Address2_UPSZone)] = Address2_UPSZone,
					[nameof(Address2_UTCOffset)] = Address2_UTCOffset,
					[nameof(Adx_CreatedByIPAddress)] = Adx_CreatedByIPAddress,
					[nameof(Adx_CreatedByUsername)] = Adx_CreatedByUsername,
					[nameof(Adx_ModifiedByIPAddress)] = Adx_ModifiedByIPAddress,
					[nameof(Adx_ModifiedByUsername)] = Adx_ModifiedByUsername,
					[nameof(Aging30)] = Aging30,
					[nameof(Aging30_Base)] = Aging30_Base,
					[nameof(Aging60)] = Aging60,
					[nameof(Aging60_Base)] = Aging60_Base,
					[nameof(Aging90)] = Aging90,
					[nameof(Aging90_Base)] = Aging90_Base,
					[nameof(BusinessTypeCode)] = BusinessTypeCode,
					[nameof(BusinessTypeCodeName)] = BusinessTypeCodeName,
					[nameof(Cr22a_SupportedLanguages)] = Cr22a_SupportedLanguages,
					[nameof(Cr22a_supportedlanguagesName)] = Cr22a_supportedlanguagesName,
					[nameof(CreatedBy)] = CreatedBy,
					[nameof(CreatedByExternalParty)] = CreatedByExternalParty,
					[nameof(CreatedByExternalPartyName)] = CreatedByExternalPartyName,
					[nameof(CreatedByExternalPartyYomiName)] = CreatedByExternalPartyYomiName,
					[nameof(CreatedByName)] = CreatedByName,
					[nameof(CreatedByYomiName)] = CreatedByYomiName,
					[nameof(CreatedOn)] = CreatedOn,
					[nameof(CreatedOnBehalfBy)] = CreatedOnBehalfBy,
					[nameof(CreatedOnBehalfByName)] = CreatedOnBehalfByName,
					[nameof(CreatedOnBehalfByYomiName)] = CreatedOnBehalfByYomiName,
					[nameof(CreditLimit)] = CreditLimit,
					[nameof(CreditLimit_Base)] = CreditLimit_Base,
					[nameof(CreditOnHold)] = CreditOnHold,
					[nameof(CreditOnHoldName)] = CreditOnHoldName,
					[nameof(CustomerSizeCode)] = CustomerSizeCode,
					[nameof(CustomerSizeCodeName)] = CustomerSizeCodeName,
					[nameof(CustomerTypeCode)] = CustomerTypeCode,
					[nameof(CustomerTypeCodeName)] = CustomerTypeCodeName,
					[nameof(Description)] = Description,
					[nameof(DoNotBulkEMail)] = DoNotBulkEMail,
					[nameof(DoNotBulkEMailName)] = DoNotBulkEMailName,
					[nameof(DoNotBulkPostalMail)] = DoNotBulkPostalMail,
					[nameof(DoNotBulkPostalMailName)] = DoNotBulkPostalMailName,
					[nameof(DoNotEMail)] = DoNotEMail,
					[nameof(DoNotEMailName)] = DoNotEMailName,
					[nameof(DoNotFax)] = DoNotFax,
					[nameof(DoNotFaxName)] = DoNotFaxName,
					[nameof(DoNotPhone)] = DoNotPhone,
					[nameof(DoNotPhoneName)] = DoNotPhoneName,
					[nameof(DoNotPostalMail)] = DoNotPostalMail,
					[nameof(DoNotPostalMailName)] = DoNotPostalMailName,
					[nameof(DoNotSendMarketingMaterialName)] = DoNotSendMarketingMaterialName,
					[nameof(DoNotSendMM)] = DoNotSendMM,
					[nameof(EMailAddress1)] = EMailAddress1,
					[nameof(EMailAddress2)] = EMailAddress2,
					[nameof(EMailAddress3)] = EMailAddress3,
					[nameof(EntityImage)] = EntityImage,
					[nameof(EntityImage_Timestamp)] = EntityImage_Timestamp,
					[nameof(EntityImage_URL)] = EntityImage_URL,
					[nameof(EntityImageId)] = EntityImageId,
					[nameof(ExchangeRate)] = ExchangeRate,
					[nameof(Fax)] = Fax,
					[nameof(FollowEmail)] = FollowEmail,
					[nameof(FollowEmailName)] = FollowEmailName,
					[nameof(FtpSiteURL)] = FtpSiteURL,
					[nameof(ImportSequenceNumber)] = ImportSequenceNumber,
					[nameof(IndustryCode)] = IndustryCode,
					[nameof(IndustryCodeName)] = IndustryCodeName,
					[nameof(IsPrivateName)] = IsPrivateName,
					[nameof(LastOnHoldTime)] = LastOnHoldTime,
					[nameof(LastUsedInCampaign)] = LastUsedInCampaign,
					[nameof(MarketCap)] = MarketCap,
					[nameof(MarketCap_Base)] = MarketCap_Base,
					[nameof(MarketingOnly)] = MarketingOnly,
					[nameof(MarketingOnlyName)] = MarketingOnlyName,
					[nameof(MasterAccountIdName)] = MasterAccountIdName,
					[nameof(MasterAccountIdYomiName)] = MasterAccountIdYomiName,
					[nameof(MasterId)] = MasterId,
					[nameof(Merged)] = Merged,
					[nameof(MergedName)] = MergedName,
					[nameof(ModifiedBy)] = ModifiedBy,
					[nameof(ModifiedByExternalParty)] = ModifiedByExternalParty,
					[nameof(ModifiedByExternalPartyName)] = ModifiedByExternalPartyName,
					[nameof(ModifiedByExternalPartyYomiName)] = ModifiedByExternalPartyYomiName,
					[nameof(ModifiedByName)] = ModifiedByName,
					[nameof(ModifiedByYomiName)] = ModifiedByYomiName,
					[nameof(ModifiedOn)] = ModifiedOn,
					[nameof(ModifiedOnBehalfBy)] = ModifiedOnBehalfBy,
					[nameof(ModifiedOnBehalfByName)] = ModifiedOnBehalfByName,
					[nameof(ModifiedOnBehalfByYomiName)] = ModifiedOnBehalfByYomiName,
					[nameof(Msa_managingpartnerid)] = Msa_managingpartnerid,
					[nameof(Msa_managingpartneridName)] = Msa_managingpartneridName,
					[nameof(Msa_managingpartneridYomiName)] = Msa_managingpartneridYomiName,
					[nameof(Msft_DataState)] = Msft_DataState,
					[nameof(Msft_datastateName)] = Msft_datastateName,
					[nameof(Name)] = Name,
					[nameof(NumberOfEmployees)] = NumberOfEmployees,
					[nameof(OnHoldTime)] = OnHoldTime,
					[nameof(OverriddenCreatedOn)] = OverriddenCreatedOn,
					[nameof(OwnerId)] = OwnerId,
					[nameof(OwnerIdName)] = OwnerIdName,
					[nameof(OwnerIdYomiName)] = OwnerIdYomiName,
					[nameof(OwnershipCode)] = OwnershipCode,
					[nameof(OwnershipCodeName)] = OwnershipCodeName,
					[nameof(OwningBusinessUnit)] = OwningBusinessUnit,
					[nameof(OwningBusinessUnitName)] = OwningBusinessUnitName,
					[nameof(OwningTeam)] = OwningTeam,
					[nameof(OwningUser)] = OwningUser,
					[nameof(ParentAccountId)] = ParentAccountId,
					[nameof(ParentAccountIdName)] = ParentAccountIdName,
					[nameof(ParentAccountIdYomiName)] = ParentAccountIdYomiName,
					[nameof(ParticipatesInWorkflow)] = ParticipatesInWorkflow,
					[nameof(ParticipatesInWorkflowName)] = ParticipatesInWorkflowName,
					[nameof(PaymentTermsCode)] = PaymentTermsCode,
					[nameof(PaymentTermsCodeName)] = PaymentTermsCodeName,
					[nameof(PreferredAppointmentDayCode)] = PreferredAppointmentDayCode,
					[nameof(PreferredAppointmentDayCodeName)] = PreferredAppointmentDayCodeName,
					[nameof(PreferredAppointmentTimeCode)] = PreferredAppointmentTimeCode,
					[nameof(PreferredAppointmentTimeCodeName)] = PreferredAppointmentTimeCodeName,
					[nameof(PreferredContactMethodCode)] = PreferredContactMethodCode,
					[nameof(PreferredContactMethodCodeName)] = PreferredContactMethodCodeName,
					[nameof(PreferredSystemUserId)] = PreferredSystemUserId,
					[nameof(PreferredSystemUserIdName)] = PreferredSystemUserIdName,
					[nameof(PreferredSystemUserIdYomiName)] = PreferredSystemUserIdYomiName,
					[nameof(PrimaryContactId)] = PrimaryContactId,
					[nameof(PrimaryContactIdName)] = PrimaryContactIdName,
					[nameof(PrimaryContactIdYomiName)] = PrimaryContactIdYomiName,
					[nameof(PrimarySatoriId)] = PrimarySatoriId,
					[nameof(PrimaryTwitterId)] = PrimaryTwitterId,
					[nameof(ProcessId)] = ProcessId,
					[nameof(Revenue)] = Revenue,
					[nameof(Revenue_Base)] = Revenue_Base,
					[nameof(SharesOutstanding)] = SharesOutstanding,
					[nameof(ShippingMethodCode)] = ShippingMethodCode,
					[nameof(ShippingMethodCodeName)] = ShippingMethodCodeName,
					[nameof(SIC)] = SIC,
					[nameof(SLAId)] = SLAId,
					[nameof(SLAInvokedId)] = SLAInvokedId,
					[nameof(SLAInvokedIdName)] = SLAInvokedIdName,
					[nameof(SLAName)] = SLAName,
					[nameof(StageId)] = StageId,
					[nameof(StateCode)] = StateCode,
					[nameof(StateCodeName)] = StateCodeName,
					[nameof(StatusCode)] = StatusCode,
					[nameof(StatusCodeName)] = StatusCodeName,
					[nameof(StockExchange)] = StockExchange,
					[nameof(Telephone1)] = Telephone1,
					[nameof(Telephone2)] = Telephone2,
					[nameof(Telephone3)] = Telephone3,
					[nameof(TerritoryCode)] = TerritoryCode,
					[nameof(TerritoryCodeName)] = TerritoryCodeName,
					[nameof(TickerSymbol)] = TickerSymbol,
					[nameof(TimeSpentByMeOnEmailAndMeetings)] = TimeSpentByMeOnEmailAndMeetings,
					[nameof(TimeZoneRuleVersionNumber)] = TimeZoneRuleVersionNumber,
					[nameof(TransactionCurrencyId)] = TransactionCurrencyId,
					[nameof(TransactionCurrencyIdName)] = TransactionCurrencyIdName,
					[nameof(TraversedPath)] = TraversedPath,
					[nameof(UTCConversionTimeZoneCode)] = UTCConversionTimeZoneCode,
					[nameof(VersionNumber)] = VersionNumber,
					[nameof(WebSiteURL)] = WebSiteURL,
					[nameof(YomiName)] = YomiName,
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
		/// Attribute of: accountcategorycode<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("accountcategorycodename")]
		public string AccountCategoryCodeName
		{
			get => FormattedValues.Contains("accountcategorycode") ? FormattedValues["accountcategorycodename"] : null;
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
		/// Attribute of: accountclassificationcode<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("accountclassificationcodename")]
		public string AccountClassificationCodeName
		{
			get => FormattedValues.Contains("accountclassificationcode") ? FormattedValues["accountclassificationcodename"] : null;
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
		/// Attribute of: accountratingcode<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("accountratingcodename")]
		public string AccountRatingCodeName
		{
			get => FormattedValues.Contains("accountratingcode") ? FormattedValues["accountratingcodename"] : null;
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
		/// Attribute of: address1_addresstypecode<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("address1_addresstypecodename")]
		public string Address1_AddressTypeCodeName
		{
			get => FormattedValues.Contains("address1_addresstypecode") ? FormattedValues["address1_addresstypecodename"] : null;
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
		/// Attribute of: address1_freighttermscode<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("address1_freighttermscodename")]
		public string Address1_FreightTermsCodeName
		{
			get => FormattedValues.Contains("address1_freighttermscode") ? FormattedValues["address1_freighttermscodename"] : null;
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
		/// Attribute of: address1_shippingmethodcode<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("address1_shippingmethodcodename")]
		public string Address1_ShippingMethodCodeName
		{
			get => FormattedValues.Contains("address1_shippingmethodcode") ? FormattedValues["address1_shippingmethodcodename"] : null;
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
		/// Attribute of: address2_addresstypecode<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("address2_addresstypecodename")]
		public string Address2_AddressTypeCodeName
		{
			get => FormattedValues.Contains("address2_addresstypecode") ? FormattedValues["address2_addresstypecodename"] : null;
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
		/// Attribute of: address2_freighttermscode<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("address2_freighttermscodename")]
		public string Address2_FreightTermsCodeName
		{
			get => FormattedValues.Contains("address2_freighttermscode") ? FormattedValues["address2_freighttermscodename"] : null;
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
		/// Attribute of: address2_shippingmethodcode<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("address2_shippingmethodcodename")]
		public string Address2_ShippingMethodCodeName
		{
			get => FormattedValues.Contains("address2_shippingmethodcode") ? FormattedValues["address2_shippingmethodcodename"] : null;
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
		/// Attribute of: businesstypecode<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("businesstypecodename")]
		public string BusinessTypeCodeName
		{
			get => FormattedValues.Contains("businesstypecode") ? FormattedValues["businesstypecodename"] : null;
		}

		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("cr22a_supportedlanguages")]
		public IEnumerable<Account.OptionSets.Supportedlanguages> Cr22a_SupportedLanguages
		{
			get => TryGetAttributeValue("cr22a_supportedlanguages", out OptionSetValueCollection opts) && opts != null ? opts.Select(opt => (Account.OptionSets.Supportedlanguages)opt.Value) : [];
			set => this["cr22a_supportedlanguages"] = value == null || !value.Any() ? null : new OptionSetValueCollection(value.Select(each => new OptionSetValue((int)each)).ToList());
		}

		/// <summary>
		/// Attribute of: cr22a_supportedlanguages<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("cr22a_supportedlanguagesname")]
		public string Cr22a_supportedlanguagesName
		{
			get => FormattedValues.Contains("cr22a_supportedlanguages") ? FormattedValues["cr22a_supportedlanguagesname"] : null;
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
		/// Attribute of: createdbyexternalparty<br/>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("createdbyexternalpartyname")]
		public string CreatedByExternalPartyName
		{
			get => FormattedValues.Contains("createdbyexternalparty") ? FormattedValues["createdbyexternalparty"] : null;
		
		}
		/// <summary>
		/// Attribute of: createdbyexternalparty<br/>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("createdbyexternalpartyyominame")]
		public string CreatedByExternalPartyYomiName
		{
			get => FormattedValues.Contains("createdbyexternalparty") ? FormattedValues["createdbyexternalparty"] : null;
		
		}
		/// <summary>
		/// Attribute of: createdby<br/>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("createdbyname")]
		public string CreatedByName
		{
			get => FormattedValues.Contains("createdby") ? FormattedValues["createdby"] : null;
		
		}
		/// <summary>
		/// Attribute of: createdby<br/>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("createdbyyominame")]
		public string CreatedByYomiName
		{
			get => FormattedValues.Contains("createdby") ? FormattedValues["createdby"] : null;
		
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
		/// Attribute of: createdonbehalfby<br/>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("createdonbehalfbyname")]
		public string CreatedOnBehalfByName
		{
			get => FormattedValues.Contains("createdonbehalfby") ? FormattedValues["createdonbehalfby"] : null;
		
		}
		/// <summary>
		/// Attribute of: createdonbehalfby<br/>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("createdonbehalfbyyominame")]
		public string CreatedOnBehalfByYomiName
		{
			get => FormattedValues.Contains("createdonbehalfby") ? FormattedValues["createdonbehalfby"] : null;
		
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
		/// Attribute of: creditonhold<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("creditonholdname")]
		public string CreditOnHoldName
		{
			get => FormattedValues.Contains("creditonhold") ? FormattedValues["creditonholdname"] : null;
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
		/// Attribute of: customersizecode<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("customersizecodename")]
		public string CustomerSizeCodeName
		{
			get => FormattedValues.Contains("customersizecode") ? FormattedValues["customersizecodename"] : null;
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
		/// Attribute of: customertypecode<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("customertypecodename")]
		public string CustomerTypeCodeName
		{
			get => FormattedValues.Contains("customertypecode") ? FormattedValues["customertypecodename"] : null;
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
		/// Attribute of: donotbulkemail<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("donotbulkemailname")]
		public string DoNotBulkEMailName
		{
			get => FormattedValues.Contains("donotbulkemail") ? FormattedValues["donotbulkemailname"] : null;
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
		/// Attribute of: donotbulkpostalmail<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("donotbulkpostalmailname")]
		public string DoNotBulkPostalMailName
		{
			get => FormattedValues.Contains("donotbulkpostalmail") ? FormattedValues["donotbulkpostalmailname"] : null;
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
		/// Attribute of: donotemail<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("donotemailname")]
		public string DoNotEMailName
		{
			get => FormattedValues.Contains("donotemail") ? FormattedValues["donotemailname"] : null;
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
		/// Attribute of: donotfax<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("donotfaxname")]
		public string DoNotFaxName
		{
			get => FormattedValues.Contains("donotfax") ? FormattedValues["donotfaxname"] : null;
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
		/// Attribute of: donotphone<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("donotphonename")]
		public string DoNotPhoneName
		{
			get => FormattedValues.Contains("donotphone") ? FormattedValues["donotphonename"] : null;
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
		/// Attribute of: donotpostalmail<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("donotpostalmailname")]
		public string DoNotPostalMailName
		{
			get => FormattedValues.Contains("donotpostalmail") ? FormattedValues["donotpostalmailname"] : null;
		}

		/// <summary>
		/// Attribute of: donotsendmm<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("donotsendmarketingmaterialname")]
		public string DoNotSendMarketingMaterialName
		{
			get => FormattedValues.Contains("donotsendmm") ? FormattedValues["donotsendmarketingmaterialname"] : null;
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
		/// Attribute of: entityimageid<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("entityimage")]
		public byte[] EntityImage
		{
			get => TryGetAttributeValue("entityimage", out byte[] value) ? value : null;
			set => this["entityimage"] = value;
		}

		/// <summary>
		/// Attribute of: entityimageid<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("entityimage_timestamp")]
		public long? EntityImage_Timestamp
		{
			get => TryGetAttributeValue("entityimage_timestamp", out long value) ? (long?)value : null;
		}
		/// <summary>
		/// Attribute of: entityimageid<br/>
		/// Max Length: 200<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("entityimage_url")]
		public string EntityImage_URL
		{
			get => TryGetAttributeValue("entityimage_url", out string value) ? value : null;
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
		/// Attribute of: followemail<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("followemailname")]
		public string FollowEmailName
		{
			get => FormattedValues.Contains("followemail") ? FormattedValues["followemailname"] : null;
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
		/// Attribute of: industrycode<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("industrycodename")]
		public string IndustryCodeName
		{
			get => FormattedValues.Contains("industrycode") ? FormattedValues["industrycodename"] : null;
		}

		/// <summary>
		/// Attribute of: isprivate<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("isprivatename")]
		public string IsPrivateName
		{
			get => FormattedValues.Contains("isprivate") ? FormattedValues["isprivatename"] : null;
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
		/// Attribute of: marketingonly<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("marketingonlyname")]
		public string MarketingOnlyName
		{
			get => FormattedValues.Contains("marketingonly") ? FormattedValues["marketingonlyname"] : null;
		}

		/// <summary>
		/// Attribute of: masterid<br/>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("masteraccountidname")]
		public string MasterAccountIdName
		{
			get => FormattedValues.Contains("masterid") ? FormattedValues["masterid"] : null;
		
		}
		/// <summary>
		/// Attribute of: masterid<br/>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("masteraccountidyominame")]
		public string MasterAccountIdYomiName
		{
			get => FormattedValues.Contains("masterid") ? FormattedValues["masterid"] : null;
		
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
		/// Attribute of: merged<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("mergedname")]
		public string MergedName
		{
			get => FormattedValues.Contains("merged") ? FormattedValues["mergedname"] : null;
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
		/// Attribute of: modifiedbyexternalparty<br/>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("modifiedbyexternalpartyname")]
		public string ModifiedByExternalPartyName
		{
			get => FormattedValues.Contains("modifiedbyexternalparty") ? FormattedValues["modifiedbyexternalparty"] : null;
		
		}
		/// <summary>
		/// Attribute of: modifiedbyexternalparty<br/>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("modifiedbyexternalpartyyominame")]
		public string ModifiedByExternalPartyYomiName
		{
			get => FormattedValues.Contains("modifiedbyexternalparty") ? FormattedValues["modifiedbyexternalparty"] : null;
		
		}
		/// <summary>
		/// Attribute of: modifiedby<br/>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("modifiedbyname")]
		public string ModifiedByName
		{
			get => FormattedValues.Contains("modifiedby") ? FormattedValues["modifiedby"] : null;
		
		}
		/// <summary>
		/// Attribute of: modifiedby<br/>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("modifiedbyyominame")]
		public string ModifiedByYomiName
		{
			get => FormattedValues.Contains("modifiedby") ? FormattedValues["modifiedby"] : null;
		
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
		/// Attribute of: modifiedonbehalfby<br/>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("modifiedonbehalfbyname")]
		public string ModifiedOnBehalfByName
		{
			get => FormattedValues.Contains("modifiedonbehalfby") ? FormattedValues["modifiedonbehalfby"] : null;
		
		}
		/// <summary>
		/// Attribute of: modifiedonbehalfby<br/>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("modifiedonbehalfbyyominame")]
		public string ModifiedOnBehalfByYomiName
		{
			get => FormattedValues.Contains("modifiedonbehalfby") ? FormattedValues["modifiedonbehalfby"] : null;
		
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
		/// Attribute of: msa_managingpartnerid<br/>
		/// Max Length: 160<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("msa_managingpartneridname")]
		public string Msa_managingpartneridName
		{
			get => FormattedValues.Contains("msa_managingpartnerid") ? FormattedValues["msa_managingpartnerid"] : null;
		
		}
		/// <summary>
		/// Attribute of: msa_managingpartnerid<br/>
		/// Max Length: 160<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("msa_managingpartneridyominame")]
		public string Msa_managingpartneridYomiName
		{
			get => FormattedValues.Contains("msa_managingpartnerid") ? FormattedValues["msa_managingpartnerid"] : null;
		
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
		/// Attribute of: msft_datastate<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("msft_datastatename")]
		public string Msft_datastateName
		{
			get => FormattedValues.Contains("msft_datastate") ? FormattedValues["msft_datastatename"] : null;
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
		/// Attribute of: ownerid<br/>
		/// Max Length: 100<br/>
		/// Required Level: SystemRequired<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("owneridname")]
		public string OwnerIdName
		{
			get => TryGetAttributeValue("owneridname", out string value) ? value : null;
		}
		/// <summary>
		/// Attribute of: ownerid<br/>
		/// Max Length: 100<br/>
		/// Required Level: SystemRequired<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("owneridyominame")]
		public string OwnerIdYomiName
		{
			get => TryGetAttributeValue("owneridyominame", out string value) ? value : null;
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
		/// Attribute of: ownershipcode<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("ownershipcodename")]
		public string OwnershipCodeName
		{
			get => FormattedValues.Contains("ownershipcode") ? FormattedValues["ownershipcodename"] : null;
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
		/// Attribute of: owningbusinessunit<br/>
		/// Max Length: 160<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("owningbusinessunitname")]
		public string OwningBusinessUnitName
		{
			get => FormattedValues.Contains("owningbusinessunit") ? FormattedValues["owningbusinessunit"] : null;
		
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
		/// Attribute of: parentaccountid<br/>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("parentaccountidname")]
		public string ParentAccountIdName
		{
			get => FormattedValues.Contains("parentaccountid") ? FormattedValues["parentaccountid"] : null;
		
		}
		/// <summary>
		/// Attribute of: parentaccountid<br/>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("parentaccountidyominame")]
		public string ParentAccountIdYomiName
		{
			get => FormattedValues.Contains("parentaccountid") ? FormattedValues["parentaccountid"] : null;
		
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
		/// Attribute of: participatesinworkflow<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("participatesinworkflowname")]
		public string ParticipatesInWorkflowName
		{
			get => FormattedValues.Contains("participatesinworkflow") ? FormattedValues["participatesinworkflowname"] : null;
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
		/// Attribute of: paymenttermscode<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("paymenttermscodename")]
		public string PaymentTermsCodeName
		{
			get => FormattedValues.Contains("paymenttermscode") ? FormattedValues["paymenttermscodename"] : null;
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
		/// Attribute of: preferredappointmentdaycode<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("preferredappointmentdaycodename")]
		public string PreferredAppointmentDayCodeName
		{
			get => FormattedValues.Contains("preferredappointmentdaycode") ? FormattedValues["preferredappointmentdaycodename"] : null;
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
		/// Attribute of: preferredappointmenttimecode<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("preferredappointmenttimecodename")]
		public string PreferredAppointmentTimeCodeName
		{
			get => FormattedValues.Contains("preferredappointmenttimecode") ? FormattedValues["preferredappointmenttimecodename"] : null;
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
		/// Attribute of: preferredcontactmethodcode<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("preferredcontactmethodcodename")]
		public string PreferredContactMethodCodeName
		{
			get => FormattedValues.Contains("preferredcontactmethodcode") ? FormattedValues["preferredcontactmethodcodename"] : null;
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
		/// Attribute of: preferredsystemuserid<br/>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("preferredsystemuseridname")]
		public string PreferredSystemUserIdName
		{
			get => FormattedValues.Contains("preferredsystemuserid") ? FormattedValues["preferredsystemuserid"] : null;
		
		}
		/// <summary>
		/// Attribute of: preferredsystemuserid<br/>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("preferredsystemuseridyominame")]
		public string PreferredSystemUserIdYomiName
		{
			get => FormattedValues.Contains("preferredsystemuserid") ? FormattedValues["preferredsystemuserid"] : null;
		
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
		/// Attribute of: primarycontactid<br/>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("primarycontactidname")]
		public string PrimaryContactIdName
		{
			get => FormattedValues.Contains("primarycontactid") ? FormattedValues["primarycontactid"] : null;
		
		}
		/// <summary>
		/// Attribute of: primarycontactid<br/>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("primarycontactidyominame")]
		public string PrimaryContactIdYomiName
		{
			get => FormattedValues.Contains("primarycontactid") ? FormattedValues["primarycontactid"] : null;
		
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
		/// Attribute of: shippingmethodcode<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("shippingmethodcodename")]
		public string ShippingMethodCodeName
		{
			get => FormattedValues.Contains("shippingmethodcode") ? FormattedValues["shippingmethodcodename"] : null;
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
		/// Attribute of: slainvokedid<br/>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("slainvokedidname")]
		public string SLAInvokedIdName
		{
			get => FormattedValues.Contains("slainvokedid") ? FormattedValues["slainvokedid"] : null;
		
		}
		/// <summary>
		/// Attribute of: slaid<br/>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("slaname")]
		public string SLAName
		{
			get => FormattedValues.Contains("slaid") ? FormattedValues["slaid"] : null;
		
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
		/// Attribute of: statecode<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("statecodename")]
		public string StateCodeName
		{
			get => FormattedValues.Contains("statecode") ? FormattedValues["statecodename"] : null;
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
		/// Attribute of: statuscode<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("statuscodename")]
		public string StatusCodeName
		{
			get => FormattedValues.Contains("statuscode") ? FormattedValues["statuscodename"] : null;
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
		/// Attribute of: territorycode<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("territorycodename")]
		public string TerritoryCodeName
		{
			get => FormattedValues.Contains("territorycode") ? FormattedValues["territorycodename"] : null;
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
		/// Attribute of: transactioncurrencyid<br/>
		/// Max Length: 100<br/>
		/// Required Level: None<br/>
		/// Valid for: Read<br/>
		/// </summary>
		[AttributeLogicalName("transactioncurrencyidname")]
		public string TransactionCurrencyIdName
		{
			get => FormattedValues.Contains("transactioncurrencyid") ? FormattedValues["transactioncurrencyid"] : null;
		
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