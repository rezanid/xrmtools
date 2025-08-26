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

namespace XrmGenTestClassic
{
    [GeneratedCode("TemplatedCodeGenerator", "1.4.0.0")]
    public partial class AccountCreatePlugin
    {
        /// <summary>
        /// This method should be called in <see cref="XrmGenTestClassic.AccountCreatePlugin.Execute(IServiceProvider)"/> before
        /// any target, image or other dependencies are used.
        /// </summary>
        protected IDisposable CreateScope(IServiceProvider serviceProvider)
        {
            var scope = new DependencyScope<AccountCreatePlugin>();
            scope.Set<IServiceProvider>(serviceProvider);
            scope.Set<IPluginExecutionContext>(serviceProvider.GetService(typeof(IPluginExecutionContext)) as IPluginExecutionContext);
        
        
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
	    			public const string AccountNumber = "accountnumber";
	    			public const string Description = "description";
	    			public const string Name = "name";
	    		
	    			public static bool TryGet(string logicalName, out string attribute)
	    			{
	    				switch (logicalName)
	    				{
	    					case nameof(AccountNumber): attribute = AccountNumber; return true;
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
	    		}
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
            get => EntityOrDefault<TargetAccount>(DependencyScope<AccountCreatePlugin>.Current.Require<IPluginExecutionContext>().InputParameters, "Target");
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
