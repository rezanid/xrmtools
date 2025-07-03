using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Extensions;
using System;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
namespace XrmGenTest;

[GeneratedCode("TemplatedCodeGenerator", "1.1.0.0")]
public partial class AccountCreatePlugin
{
    protected void InjectDependencies(IServiceProvider serviceProvider)
    {
    }
	[GeneratedCode("TemplatedCodeGenerator", "1.1.0.0")]
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
	[GeneratedCode("TemplatedCodeGenerator", "1.1.0.0")]
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
				public const string AccountId = "accountid";
				public const string AccountNumber = "accountnumber";
				public const string Address1_AddressTypeCode = "address1_addresstypecode";
				public const string Address1_FreightTermsCode = "address1_freighttermscode";
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
		/// Required Level: SystemRequired
		/// Valid for: Create Read
		/// </summary>
		
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
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		[AttributeLogicalName("address1_addresstypecode")]
		public PostImageAccount.Meta.Choices.Address1AddressType? Address1_AddressTypeCode
		{
			get => TryGetAttributeValue("address1_addresstypecode", out OptionSetValue opt) && opt == null ? null : (PostImageAccount.Meta.Choices.Address1AddressType)opt.Value;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		[AttributeLogicalName("address1_freighttermscode")]
		public PostImageAccount.Meta.Choices.Address1FreightTerms? Address1_FreightTermsCode
		{
			get => TryGetAttributeValue("address1_freighttermscode", out OptionSetValue opt) && opt == null ? null : (PostImageAccount.Meta.Choices.Address1FreightTerms)opt.Value;
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
