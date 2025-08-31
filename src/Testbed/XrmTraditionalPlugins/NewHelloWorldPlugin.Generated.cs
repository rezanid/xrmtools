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

namespace XrmTraditionalPlugins
{
    [GeneratedCode("TemplatedCodeGenerator", "1.4.2.0")]
    public partial class CleanAccountPostOperation
    {
        public string Config => DependencyScope<CleanAccountPostOperation>.Current.Require<string>("Config");
        public string SecureConfig => DependencyScope<CleanAccountPostOperation>.Current.Require<string>("SecureConfig");

        
        /// <summary>
        /// This method should be called before accessing any target, image or any of your dependencies.
        /// </summary>
        protected IDisposable CreateScope(IServiceProvider serviceProvider)
        {
            var scope = new DependencyScope<CleanAccountPostOperation>();
            scope.Set<IServiceProvider>(serviceProvider);
            scope.Set<IPluginExecutionContext>((IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext)));
        
            // Store unsecure plugin configuration (named)
            scope.Set<string>("Config", this.Config);
            // Store secure plugin configuration (named)
            scope.Set<string>("SecureConfig", this.SecureConfig);
        
            scope.Set<Microsoft.Xrm.Sdk.IOrganizationServiceFactory>((Microsoft.Xrm.Sdk.IOrganizationServiceFactory)serviceProvider.GetService(typeof(Microsoft.Xrm.Sdk.IOrganizationServiceFactory)));
        
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
	    			public const string Description = "description";
	    			public const string Name = "name";
	    		
	    			public static bool TryGet(string logicalName, out string attribute)
	    			{
	    				switch (logicalName)
	    				{
	    					case nameof(AccountCategoryCode): attribute = AccountCategoryCode; return true;
	    					case nameof(Description): attribute = Description; return true;
	    					case nameof(Name): attribute = Name; return true;
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
	    	/// Max Length: 2000</br>
	    	/// Required Level: None</br>
	    	/// Valid for: Create Update Read</br>
	    	/// </summary>
	    	[AttributeLogicalName("description")]
	    	public string Description
	    	{
	    		get => TryGetAttributeValue("description", out string value) ? value : null;
	    		set => this["description"] = value;
	    	}
	    	/// <summary>
	    	/// Max Length: 160</br>
	    	/// Required Level: ApplicationRequired</br>
	    	/// Valid for: Create Update Read</br>
	    	/// </summary>
	    	[AttributeLogicalName("name")]
	    	public string Name
	    	{
	    		get => TryGetAttributeValue("name", out string value) ? value : null;
	    		set => this["name"] = value;
	    	}
	    }
	    
	    public TargetAccount Target
        {
            get => EntityOrDefault<TargetAccount>(DependencyScope<CleanAccountPostOperation>.Current.Require<IPluginExecutionContext>().InputParameters, "Target");
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
}
