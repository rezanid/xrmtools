using Microsoft.Xrm.Sdk;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Extensions;
using Microsoft.Xrm.Sdk.PluginTelemetry;
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
    [GeneratedCode("TemplatedCodeGenerator", "1.6.0.0")]
    public partial class AccountGreetingPlugin
    {
        /// <summary>
        /// This method should be called before accessing any target, image or any of your dependencies.
        /// </summary>
        protected IDisposable CreateScope(IServiceProvider serviceProvider)
        {
            var scope = new DependencyScope<AccountGreetingPlugin>();
            scope.Set<IServiceProvider>(serviceProvider);
        
            scope.Set<IPluginExecutionContext>((IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext)));
            scope.Set<IOrganizationService>((IOrganizationService)serviceProvider.GetService(typeof(IOrganizationService)));
            return scope;
        }
    
        public static CreateAccountEntity Target
        {
            get => EntityOrDefault<CreateAccountEntity>(Require<IPluginExecutionContext>().InputParameters, "Target");
        }
            
        [EntityLogicalName("account")]
        public class CreateAccountEntity : Entity
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

        private static T[] EntityCollectionOrDefault<T>(DataCollection<string, object> keyValues, string key) where T : Entity
        {
            if (keyValues is null) return Array.Empty<T>();
            return keyValues.TryGetValue(key, out var obj) ? obj is EntityCollection entityCollection
                ? entityCollection.Entities.Select(e => e.ToEntity<T>()).ToArray() : Array.Empty<T>() : Array.Empty<T>();
        }

        private static T Require<T>() => DependencyScope<AccountGreetingPlugin>.Current.Require<T>();
        private static T Require<T>(string name) => DependencyScope<AccountGreetingPlugin>.Current.Require<T>(name);

        private static bool TryGet<T>(out T instance) => DependencyScope<AccountGreetingPlugin>.Current.TryGet(out instance);
        private static bool TryGet<T>(string name, out T instance) => DependencyScope<AccountGreetingPlugin>.Current.TryGet(name, out instance);

        private static T Set<T>(T instance) => DependencyScope<AccountGreetingPlugin>.Current.Set(instance);
        private static T Set<T>(string name, T instance) => DependencyScope<AccountGreetingPlugin>.Current.Set(name, instance);
        private static T SetAndTrack<T>(T instance) where T : IDisposable => DependencyScope<AccountGreetingPlugin>.Current.SetAndTrack(instance);
        private static T SetAndTrack<T>(string name, T instance) where T : IDisposable => DependencyScope<AccountGreetingPlugin>.Current.SetAndTrack(name, instance);
    }
}
