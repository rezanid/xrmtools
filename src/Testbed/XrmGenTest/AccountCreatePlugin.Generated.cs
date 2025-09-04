using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Extensions;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using XrmTools;

namespace XrmGenTest
{
    [GeneratedCode("TemplatedCodeGenerator", "1.4.3.0")]
    public partial class AccountCreatePlugin
    {
        /// <summary>
        /// This method should be called before accessing any target, image or any of your dependencies.
        /// </summary>
        protected IDisposable CreateScope(IServiceProvider serviceProvider)
        {
            var scope = new DependencyScope<AccountCreatePlugin>();
            scope.Set<IServiceProvider>(serviceProvider);
            scope.Set<IPluginExecutionContext>((IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext)));
        
            return scope;
        }
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
	    		
	    			public static bool TryGet(string logicalName, out string attribute)
	    			{
	    				switch (logicalName)
	    				{
	    					case nameof(AccountCategoryCode): attribute = AccountCategoryCode; return true;
	    					case nameof(AccountClassificationCode): attribute = AccountClassificationCode; return true;
	    					case nameof(AccountNumber): attribute = AccountNumber; return true;
	    					default: attribute = null; return false;
	    				}
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
	    	public TargetAccount.Meta.OptionSets.Category? AccountCategoryCode
	    	{
	    		get => TryGetAttributeValue("accountcategorycode", out OptionSetValue opt) && opt != null ? (TargetAccount.Meta.OptionSets.Category?)opt.Value : null;
	    		set => this["accountcategorycode"] = value == null ? null : new OptionSetValue((int)value);
	    	}
	    	/// <summary>
	    	/// Required Level: None</br>
	    	/// Valid for: Create Update Read</br>
	    	/// </summary>
	    	[AttributeLogicalName("accountclassificationcode")]
	    	public TargetAccount.Meta.OptionSets.Classification? AccountClassificationCode
	    	{
	    		get => TryGetAttributeValue("accountclassificationcode", out OptionSetValue opt) && opt != null ? (TargetAccount.Meta.OptionSets.Classification?)opt.Value : null;
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
	    		
	    			public static bool TryGet(string logicalName, out string attribute)
	    			{
	    				switch (logicalName)
	    				{
	    					case nameof(AccountCategoryCode): attribute = AccountCategoryCode; return true;
	    					case nameof(AccountNumber): attribute = AccountNumber; return true;
	    					case nameof(Address1_AddressTypeCode): attribute = Address1_AddressTypeCode; return true;
	    					case nameof(Address1_FreightTermsCode): attribute = Address1_FreightTermsCode; return true;
	    					default: attribute = null; return false;
	    				}
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
	    	public PostImageAccount.Meta.OptionSets.Category? AccountCategoryCode
	    	{
	    		get => TryGetAttributeValue("accountcategorycode", out OptionSetValue opt) && opt != null ? (PostImageAccount.Meta.OptionSets.Category?)opt.Value : null;
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
	    	/// <summary>
	    	/// Required Level: None
	    	/// Valid for: Create Update Read
	    	/// </summary>
	    	[AttributeLogicalName("address1_addresstypecode")]
	    	public PostImageAccount.Meta.OptionSets.Address1AddressType? Address1_AddressTypeCode
	    	{
	    		get => TryGetAttributeValue("address1_addresstypecode", out OptionSetValue opt) && opt != null ? (PostImageAccount.Meta.OptionSets.Address1AddressType?)opt.Value : null;
	    	}
	    	/// <summary>
	    	/// Required Level: None
	    	/// Valid for: Create Update Read
	    	/// </summary>
	    	[AttributeLogicalName("address1_freighttermscode")]
	    	public PostImageAccount.Meta.OptionSets.Address1FreightTerms? Address1_FreightTermsCode
	    	{
	    		get => TryGetAttributeValue("address1_freighttermscode", out OptionSetValue opt) && opt != null ? (PostImageAccount.Meta.OptionSets.Address1FreightTerms?)opt.Value : null;
	    	}
	    }
	    
	    public TargetAccount Target
        {
            get => EntityOrDefault<TargetAccount>(DependencyScope<AccountCreatePlugin>.Current.Require<IPluginExecutionContext>().InputParameters, "Target");
        }

	    public PostImageAccount AccountPostImage { get => EntityOrDefault<PostImageAccount>(DependencyScope<AccountCreatePlugin>.Current.Require<IPluginExecutionContext>().PostEntityImages, "AccountPostImage"); }

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
}
