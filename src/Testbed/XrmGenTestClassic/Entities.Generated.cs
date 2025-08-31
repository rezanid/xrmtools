using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;

namespace XrmGenTestClassic
{
	/// <summary>
	/// Display Name: Contact
	/// </summary>
	[GeneratedCode("TemplatedCodeGenerator", "1.4.2.0")]
	[EntityLogicalName("contact")]
	[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public partial class Contact : Microsoft.Xrm.Sdk.Entity
	{
		public partial class Meta 
		{
			public const string EntityLogicalName = "contact";
			public const string EntityLogicalCollectionName = "contacts";
			public const string EntitySetName = "contacts";
			public const string PrimaryNameAttribute = "fullname";
			public const string PrimaryIdAttribute = "contactid";

			public partial class Fields
			{
				public const string Description = "description";
				public const string FirstName = "firstname";
				public const string LastName = "lastname";

				public static bool TryGet(string logicalName, out string attribute)
				{
					switch (logicalName)
					{
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
		}

		/// <summary>
		/// Max Length: 2000<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("description")]
		public string Description
		{
			get => TryGetAttributeValue("description", out string value) ? value : null;
			set => this["description"] = value;
		}
		/// <summary>
		/// Max Length: 50<br/>
		/// Required Level: Recommended<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("firstname")]
		public string FirstName
		{
			get => TryGetAttributeValue("firstname", out string value) ? value : null;
			set => this["firstname"] = value;
		}
		/// <summary>
		/// Max Length: 50<br/>
		/// Required Level: ApplicationRequired<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("lastname")]
		public string LastName
		{
			get => TryGetAttributeValue("lastname", out string value) ? value : null;
			set => this["lastname"] = value;
		}
		public Contact() : base(Meta.EntityLogicalName) { }
		public Contact(Guid id) : base(Meta.EntityLogicalName, id) { }
		public Contact(string keyName, object keyValue) : base(Meta.EntityLogicalName, keyName, keyValue) { }
		public Contact(KeyAttributeCollection keyAttributes) : base(Meta.EntityLogicalName, keyAttributes) { }
	}
	/// <summary>
	/// Display Name: Account
	/// </summary>
	[GeneratedCode("TemplatedCodeGenerator", "1.4.2.0")]
	[EntityLogicalName("account")]
	[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public partial class Account : Microsoft.Xrm.Sdk.Entity
	{
		public partial class Meta 
		{
			public const string EntityLogicalName = "account";
			public const string EntityLogicalCollectionName = "accounts";
			public const string EntitySetName = "accounts";
			public const string PrimaryNameAttribute = "name";
			public const string PrimaryIdAttribute = "accountid";

			public partial class Fields
			{
				public const string AccountRatingCode = "accountratingcode";
				public const string Description = "description";
				public const string Name = "name";

				public static bool TryGet(string logicalName, out string attribute)
				{
					switch (logicalName)
					{
						case nameof(AccountRatingCode): attribute = AccountRatingCode; return true;
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
				/// <summary>
				/// Drop-down list for selecting account ratings.
				/// </summary>
				[DataContract]
				public enum AccountRating
				{
					[EnumMember] DefaultValue = 1,
				}
			}
		}

		/// <summary>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("accountratingcode")]
		public Account.Meta.OptionSets.AccountRating? AccountRatingCode
		{
			get => TryGetAttributeValue("accountratingcode", out OptionSetValue opt) && opt != null ? (Account.Meta.OptionSets.AccountRating?)opt.Value : null;
			set => this["accountratingcode"] = value == null ? null : new OptionSetValue((int)value);
		}
		/// <summary>
		/// Max Length: 2000<br/>
		/// Required Level: None<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("description")]
		public string Description
		{
			get => TryGetAttributeValue("description", out string value) ? value : null;
			set => this["description"] = value;
		}
		/// <summary>
		/// Max Length: 160<br/>
		/// Required Level: ApplicationRequired<br/>
		/// Valid for: Create Update Read<br/>
		/// </summary>
		[AttributeLogicalName("name")]
		public string Name
		{
			get => TryGetAttributeValue("name", out string value) ? value : null;
			set => this["name"] = value;
		}
		public Account() : base(Meta.EntityLogicalName) { }
		public Account(Guid id) : base(Meta.EntityLogicalName, id) { }
		public Account(string keyName, object keyValue) : base(Meta.EntityLogicalName, keyName, keyValue) { }
		public Account(KeyAttributeCollection keyAttributes) : base(Meta.EntityLogicalName, keyAttributes) { }
	}
}