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

namespace XrmGenTestClassic
{
    [GeneratedCode("TemplatedCodeGenerator", "1.5.0.3")]
    public partial class ContactGreetingPlugin
    {
        /// <summary>
        /// This method should be called before accessing any target, image or any of your dependencies.
        /// </summary>
        protected override IDisposable CreateScope(IServiceProvider serviceProvider)
        {
            var scope = new DependencyScope<ContactGreetingPlugin>();
            scope.Set<IServiceProvider>(serviceProvider);
        
            var iTracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            var iOrganizationServiceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var contactPersister = new XrmGenTestClassic.ContactPersister((IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory)), (ITracingService)serviceProvider.GetService(typeof(ITracingService)));
        
            scope.Set<ITracingService>((ITracingService)serviceProvider.GetService(typeof(ITracingService)));
            scope.Set<XrmGenTestClassic.ILoggingService>(scope.Set(new XrmGenTestClassic.LoggingService(serviceProvider)));
            scope.Set<IOrganizationServiceFactory>((IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory)));
            scope.Set<IPluginExecutionContext>((IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext)));
            scope.Set<XrmGenTestClassic.IContactPersister>(contactPersister);
            scope.Set<XrmGenTestClassic.IContactOrchestrator>(scope.SetAndTrack(new XrmGenTestClassic.ContactOrchestrator(contactPersister, (ITracingService)serviceProvider.GetService(typeof(ITracingService)))));
            scope.Set<XrmGenTestClassic.IValidationService>(scope.Set(new XrmGenTestClassic.ValidationService()));
            return scope;
        }
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
	    			public const string AccountRoleCode = "accountrolecode";
	    			public const string Description = "description";
	    			public const string FirstName = "firstname";
	    			public const string LastName = "lastname";
	    		
	    			public static bool TryGet(string logicalName, out string attribute)
	    			{
	    				switch (logicalName)
	    				{
	    					case nameof(AccountRoleCode): attribute = AccountRoleCode; return true;
	    					case nameof(Description): attribute = Description; return true;
	    					case nameof(FirstName): attribute = FirstName; return true;
	    					case nameof(LastName): attribute = LastName; return true;
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
	    		}
	    	}
	    
	    	/// <summary>
	    	/// Required Level: None</br>
	    	/// Valid for: Create Update Read</br>
	    	/// </summary>
	    	[AttributeLogicalName("accountrolecode")]
	    	public TargetContact.Meta.OptionSets.Role? AccountRoleCode
	    	{
	    		get => TryGetAttributeValue("accountrolecode", out OptionSetValue opt) && opt != null ? (TargetContact.Meta.OptionSets.Role?)opt.Value : null;
	    		set => this["accountrolecode"] = value == null ? null : new OptionSetValue((int)value);
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
	    
	    public TargetContact Target
        {
            get => EntityOrDefault<TargetContact>(DependencyScope<ContactGreetingPlugin>.Current.Require<IPluginExecutionContext>().InputParameters, "Target");
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
