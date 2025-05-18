using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Extensions;
using System;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
namespace XrmGenTest;

[GeneratedCode("TemplatedCodeGenerator", "1.0.4.0")]
public partial class ContactCreatePlugin
{
    protected void InjectDependencies(IServiceProvider serviceProvider)
    {
    }
	[GeneratedCode("TemplatedCodeGenerator", "1.0.4.0")]
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
				public const string Description = "description";
				public const string FirstName = "firstname";
				public const string LastName = "lastname";
			}
	
			public partial class Choices
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
	
	public TargetContact Target { get; set; }

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
        Target = EntityOrDefault<TargetContact>(executionContext.InputParameters, "Target");
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
