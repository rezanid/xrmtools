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

namespace XrmGenTest
{
    [GeneratedCode("TemplatedCodeGenerator", "1.3.3.0")]
    public partial class ContactCreatePlugin
    {
        protected void InjectDependencies(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;
            this.ServiceFactory = new XrmGenTest.SomeService(serviceProvider);
            this.ContactPersister = new XrmGenTest.ContactPersister(this.OrganizationService);
        }
	    [GeneratedCode("TemplatedCodeGenerator", "1.3.3.0")]
	    [EntityLogicalName("contact")]
	    public class CreateTargetContact : Entity
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
	    		}
	    
	    		public partial class Choices
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
	    	public CreateTargetContact.Meta.Choices.Role? AccountRoleCode
	    	{
	    		get => TryGetAttributeValue("accountrolecode", out OptionSetValue opt) && opt != null ? (CreateTargetContact.Meta.Choices.Role?)opt.Value : null;
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
	    
	    public CreateTargetContact CreateTarget { get; set; }

	    [GeneratedCode("TemplatedCodeGenerator", "1.3.3.0")]
	    [EntityLogicalName("contact")]
	    public class UpdateTargetContact : Entity
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
	    		}
	    
	    		public partial class Choices
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
	    	public UpdateTargetContact.Meta.Choices.Role? AccountRoleCode
	    	{
	    		get => TryGetAttributeValue("accountrolecode", out OptionSetValue opt) && opt != null ? (UpdateTargetContact.Meta.Choices.Role?)opt.Value : null;
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
	    [GeneratedCode("TemplatedCodeGenerator", "1.3.3.0")]
	    [EntityLogicalName("contact")]
	    public class UpdatecontactPreImageContact : Entity
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
	    		}
	    
	    		public partial class Choices
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
	    	public UpdatecontactPreImageContact.Meta.Choices.Role? AccountRoleCode
	    	{
	    		get => TryGetAttributeValue("accountrolecode", out OptionSetValue opt) && opt != null ? (UpdatecontactPreImageContact.Meta.Choices.Role?)opt.Value : null;
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
	    
	    public UpdateTargetContact UpdateTarget { get; set; }

	    public UpdatecontactPreImageContact contactPreImage { get; set; }

	    /// <summary>
	    /// This method should be called on every <see cref="XrmGenTest.ContactCreatePlugin.Execute(IServiceProvider)"/> execution.
	    /// </summary>
	    /// <param name="serviceProvider"></param>
	    /// <exception cref="InvalidPluginExecutionException"></exception>
        internal void Initialize(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new InvalidPluginExecutionException(nameof(serviceProvider) + " argument is null.");
            }
            var executionContext = serviceProvider.Get<IPluginExecutionContext7>();
            CreateTarget = EntityOrDefault<CreateTargetContact>(executionContext.InputParameters, "Target");
            UpdateTarget = EntityOrDefault<UpdateTargetContact>(executionContext.InputParameters, "Target");
            contactPreImage = EntityOrDefault<UpdatecontactPreImageContact>(executionContext.PreEntityImages, "contactPreImage");
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
