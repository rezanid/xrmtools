using Microsoft.Xrm.Sdk;
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

namespace SdkPluginsCS10
{
    [GeneratedCode("TemplatedCodeGenerator", "1.5.0.3")]
    public partial class Plugin1
    {
        /// <summary>
        /// This method should be called before accessing any target, image or any of your dependencies.
        /// </summary>
        protected IDisposable CreateScope(IServiceProvider serviceProvider)
        {
            var scope = new DependencyScope<Plugin1>();
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
	    			public const string Name = "name";
	    		
	    			public static bool TryGet(string logicalName, out string attribute)
	    			{
	    				switch (logicalName)
	    				{
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
            get => EntityOrDefault<TargetAccount>(DependencyScope<Plugin1>.Current.Require<IPluginExecutionContext>().InputParameters, "Target");
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

        private static T Require<T>() => DependencyScope<Plugin1>.Current.Require<T>();
        private static T Require<T>(string name) => DependencyScope<Plugin1>.Current.Require<T>(name);

        private static bool TryGet<T>(out T instance) => DependencyScope<Plugin1>.Current.TryGet(out instance);
        private static bool TryGet<T>(string name, out T instance) => DependencyScope<Plugin1>.Current.TryGet(name, out instance);

        private static T Set<T>(T instance) => DependencyScope<Plugin1>.Current.Set(instance);
        private static T Set<T>(string name, T instance) => DependencyScope<Plugin1>.Current.Set(name, instance);
        private static T SetAndTrack<T>(T instance) where T : IDisposable => DependencyScope<Plugin1>.Current.SetAndTrack(instance);
        private static T SetAndTrack<T>(string name, T instance) where T : IDisposable => DependencyScope<Plugin1>.Current.SetAndTrack(name, instance);
    }
}
