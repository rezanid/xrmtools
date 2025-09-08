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

namespace XrmTraditionalPlugins
{
    [GeneratedCode("TemplatedCodeGenerator", "1.4.4.0")]
    public partial class CleanAccountPostOperation
    {
        // full_type_name:Microsoft.Xrm.Sdk.IOrganizationServiceFactory | dep_name:  | provided_by_property: 
            // full_type_name:Microsoft.Xrm.Sdk.ITracingService | dep_name:  | provided_by_property: 
            // full_type_name:Microsoft.Xrm.Sdk.PluginTelemetry.ILogger | dep_name:  | provided_by_property: 
            // full_type_name:Microsoft.Xrm.Sdk.IExecutionContext | dep_name:  | provided_by_property: 
            // full_type_name:XrmTraditionalPlugins.IBusinessService | dep_name:  | provided_by_property: 
            // full_type_name:string | dep_name: Config | provided_by_property: Config
            /// <summary>
        /// This method should be called before accessing any target, image or any of your dependencies.
        /// </summary>
        protected IDisposable CreateScope(IServiceProvider serviceProvider)
        {
            var scope = new DependencyScope<CleanAccountPostOperation>();
            scope.Set<IServiceProvider>(serviceProvider);
            scope.Set<IPluginExecutionContext>((IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext)));
        
            var iLogger = (ILogger)serviceProvider.GetService(typeof(ILogger));
        
            scope.Set<IOrganizationServiceFactory>((IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory)));
            scope.Set<ITracingService>((ITracingService)serviceProvider.GetService(typeof(ITracingService)));
            scope.Set<ILogger>((ILogger)serviceProvider.GetService(typeof(ILogger)));
            scope.Set<IExecutionContext>(scope.Set(new RemoteExecutionContext()));
            scope.Set<XrmTraditionalPlugins.IBusinessService>(scope.Set(new XrmTraditionalPlugins.BusinessService((ILogger)serviceProvider.GetService(typeof(ILogger)), this.Config)));
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
	    			public const string Description = "description";
	    			public const string Name = "name";
	    		
	    			public static bool TryGet(string logicalName, out string attribute)
	    			{
	    				switch (logicalName)
	    				{
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
}
