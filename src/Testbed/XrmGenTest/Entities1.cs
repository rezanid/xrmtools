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

[EntityLogicalName("account")]
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public class Partner : Entity
{
	public const string EntityLogicalName = "account";
	public const string EntityLogicalCollectionName = "accounts";
	public const string EntitySetName = "accounts";
	public const int EntityTypeCode = 1;
	public const string PrimaryNameAttribute = "name";
	public const string PrimaryIdAttribute = "accountid";

	public partial class Fields
	{
		public const string AccountCategoryCode = "accountcategorycode";
	}

	public partial class Choices
	{
		/// <summary>
        /// Drop-down list for selecting the category of the account.
		/// </summary>
		[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
		[DataContract]
		public enum Category
		{
			[EnumMember]
			PreferredCustomer = 1,
			[EnumMember]
			Standard = 2,
		}
	}

	/// <summary>
	/// Required Level: None<br/>
	/// Valid for: Create Update Read</br>
	/// Auto Number Format: 
	/// </summary>
	[AttributeLogicalName("accountcategorycode")]
	public Partner.Choices.Category? AccountCategoryCode
	{
		get => TryGetAttributeValue("accountcategorycode", out OptionSetValue opt) && opt == null ? null : (Partner.Choices.Category)opt.Value;
		set => this["accountcategorycode"] = value == null ? null : new OptionSetValue((int)value);
	}
	public Partner() : base(EntityLogicalName) { }
    public Partner(string keyName, object keyValue) : base(EntityLogicalName, keyName, keyValue) { }
    public Partner(KeyAttributeCollection keyAttributes) : base(EntityLogicalName, keyAttributes) { }
}
