using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Extensions;
using System;
using System.CodeDom.Compiler;

namespace XrmGenTest;

[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
public partial class AccountCreatePlugin : PluginBase
{
	[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
	[EntityLogicalName("account")]
	public class TargetPartner : Entity
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
				public const string CreatedOn = "createdon";
				public const string Name = "name";
			}
	
			public partial class Choices
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
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// </summary>
		[AttributeLogicalName("createdon")]
		public DateTime? CreatedOn
		{
			get => TryGetAttributeValue("createdon", out DateTime value) ? value : null;
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
	[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
	[EntityLogicalName("account")]
	public class PostImagePartner : Entity
	{
		public const string EntityLogicalName = "account";
		public partial class Fields
		{
			public const string AccountNumber = "accountnumber";
			public const string CreatedOn = "createdon";
			public const string Name = "name";
		}
	
		/// <summary>
		/// Max Length: 20
		/// Required Level: None
		/// Valid for: Create Update Read
		/// </summary>
		[AttributeLogicalName("accountnumber")]
		public string AccountNumber
		{
			get => TryGetAttributeValue("accountnumber", out string value) ? value : null;
		}
		/// <summary>
		/// Required Level: None
		/// Valid for: Read
		/// </summary>
		[AttributeLogicalName("createdon")]
		public DateTime? CreatedOn
		{
			get => TryGetAttributeValue("createdon", out DateTime value) ? value : null;
		}
		/// <summary>
		/// Max Length: 160
		/// Required Level: ApplicationRequired
		/// Valid for: Create Update Read
		/// </summary>
		[AttributeLogicalName("name")]
		public string Name
		{
			get => TryGetAttributeValue("name", out string value) ? value : null;
		}
	}

	AccountCreatePlugin.TargetPartner Target { get; set; }
	AccountCreatePlugin.PostImagePartner PostImage { get; set; }

	/// <summary>
	/// This method should be called on every <see cref="PluginBase.Execute(IServiceProvider)"/> execution.
	/// </summary>
	/// <param name="serviceProvider"></param>
	/// <exception cref="InvalidPluginExecutionException"></exception>
	internal override void Initialize(IServiceProvider serviceProvider)
    {
        if (serviceProvider == null)
        {
            throw new InvalidPluginExecutionException(nameof(serviceProvider) + " argument is null.");
        }
        var executionContext = serviceProvider.Get<IPluginExecutionContext7>();
        Target = EntityOrDefault<TargetPartner>(executionContext.InputParameters, "Target");
        PostImage = EntityOrDefault<PostImagePartner>(executionContext.PreEntityImages, "PostImage");
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
