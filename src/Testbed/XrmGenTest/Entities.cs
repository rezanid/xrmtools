using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;

namespace XrmGenTest;
[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
[EntityLogicalName("account")]
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public partial class Partner : Entity
{
	public static class Meta 
	{
		public const string EntityLogicalName = "account";
		public const string EntityLogicalCollectionName = "accounts";
		public const string EntitySetName = "accounts";
		public const string PrimaryNameAttribute = "name";
		public const string PrimaryIdAttribute = "accountid";

		public partial class Fields
		{
			public const string Name = "name";
		}

		public partial class Choices
		{
		}
	}

	/// <summary>
	/// Max Length: 160</br>
	/// Required Level: ApplicationRequired<br/>
	/// Valid for: Create Update Read</br>
	/// </summary>
	[AttributeLogicalName("name")]
	public string Name
	{
		get => TryGetAttributeValue("name", out string value) ? value : null;
		set => this["name"] = value;
	}
	public Partner() : base(Meta.EntityLogicalName) { }
    public Partner(string keyName, object keyValue) : base(Meta.EntityLogicalName, keyName, keyValue) { }
    public Partner(KeyAttributeCollection keyAttributes) : base(Meta.EntityLogicalName, keyAttributes) { }
}
