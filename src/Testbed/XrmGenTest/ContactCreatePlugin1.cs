using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Extensions;
using System;
using System.CodeDom.Compiler;

namespace XrmGenTest;

[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
public partial class ContactCreatePlugin
{
	[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
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
				public const string FirstName = "firstname";
				public const string LastName = "lastname";
			}
	
			public partial class Choices
			{
			}
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
	[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
	[EntityLogicalName("contact")]
	public class PostImageContact : Entity
	{
		public const string EntityLogicalName = "contact";
		public partial class Fields
		{
			public const string FirstName = "firstname";
			public const string LastName = "lastname";
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

	ContactCreatePlugin.TargetContact Target { get; set; }
	ContactCreatePlugin.PostImageContact PostImage { get; set; }

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
        PostImage = EntityOrDefault<PostImageContact>(executionContext.PreEntityImages, "PostImage");
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
