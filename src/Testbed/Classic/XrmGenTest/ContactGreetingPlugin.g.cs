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
    public partial class ContactGreetingPlugin
    {
        /// <summary>
        /// This method should be called before accessing any target, image or any of your dependencies.
        /// </summary>
        protected override IDisposable CreateScope(IServiceProvider serviceProvider)
        {
            var scope = new DependencyScope<ContactGreetingPlugin>();
            scope.Set<IServiceProvider>(serviceProvider);
        
            var contactPersister = new XrmGenTest.ContactPersister((IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory)), (ITracingService)serviceProvider.GetService(typeof(ITracingService)));
        
            scope.Set<ITracingService>((ITracingService)serviceProvider.GetService(typeof(ITracingService)));
            scope.Set<XrmGenTest.ILoggingService>(scope.Set(new XrmGenTest.LoggingService(serviceProvider)));
            scope.Set<IOrganizationServiceFactory>((IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory)));
            scope.Set<IPluginExecutionContext>((IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext)));
            scope.Set<XrmGenTest.IContactPersister>(contactPersister);
            scope.Set<XrmGenTest.IContactOrchestrator>(scope.SetAndTrack(new XrmGenTest.ContactOrchestrator(contactPersister, (ITracingService)serviceProvider.GetService(typeof(ITracingService)))));
            scope.Set<XrmGenTest.IValidationService>(scope.Set(new XrmGenTest.ValidationService()));
            scope.Set<IOrganizationService>(this.UserOrgService);
            return scope;
        }
    
        public static class UpdateInputParameters
        {
            public static UpdateContactEntity Target
            {
                get => EntityOrDefault<UpdateContactEntity>(Require<IPluginExecutionContext>().InputParameters, "Target");
            }
                
            [EntityLogicalName("contact")]
            public class UpdateContactEntity : Entity
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
            	public UpdateContactEntity.Meta.OptionSets.Role? AccountRoleCode
            	{
            		get => TryGetAttributeValue("accountrolecode", out OptionSetValue opt) && opt != null ? (UpdateContactEntity.Meta.OptionSets.Role?)opt.Value : null;
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
            }}
        [EntityLogicalName("contact")]
        public class UpdatePreImageContact : Entity
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
        	/// Required Level: None
        	/// Valid for: Create Update Read
        	/// </summary>
        	[AttributeLogicalName("accountrolecode")]
        	public UpdatePreImageContact.Meta.OptionSets.Role? AccountRoleCode
        	{
        		get => TryGetAttributeValue("accountrolecode", out OptionSetValue opt) && opt != null ? (UpdatePreImageContact.Meta.OptionSets.Role?)opt.Value : null;
        	}
        	/// <summary>
        	/// Max Length: 2000
        	/// Required Level: None
        	/// Valid for: Create Update Read
        	/// </summary>
        	[AttributeLogicalName("description")]
        	public string Description
        	{
        		get => TryGetAttributeValue("description", out string value) ? value : null;
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
        
        public UpdatePreImageContact ContactPreImage { get => EntityOrDefault<UpdatePreImageContact>(DependencyScope<ContactGreetingPlugin>.Current.Require<IPluginExecutionContext>().PreEntityImages, "ContactPreImage"); }
        public static class CreateInputParameters
        {
            public static CreateContactEntity Target
            {
                get => EntityOrDefault<CreateContactEntity>(Require<IPluginExecutionContext>().InputParameters, "Target");
            }
                
            [EntityLogicalName("contact")]
            public class CreateContactEntity : Entity
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
            	public CreateContactEntity.Meta.OptionSets.Role? AccountRoleCode
            	{
            		get => TryGetAttributeValue("accountrolecode", out OptionSetValue opt) && opt != null ? (CreateContactEntity.Meta.OptionSets.Role?)opt.Value : null;
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
            	}
            }}
        

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
    }
}
