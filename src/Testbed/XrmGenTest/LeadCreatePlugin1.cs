using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Extensions;
using System;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace XrmGenTest;

[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
public partial class LeadCreatePlugin
{
	[GeneratedCode("TemplatedPluginCodeGenerator", "1.0.0.0")]
	[EntityLogicalName("annotation")]
	public class TargetNote : Entity
	{
		public static class Meta
		{
			public const string EntityLogicalName = "annotation";
			public const string EntityLogicalCollectionName = "annotations";
			public const string EntitySetName = "annotations";
			public const string PrimaryNameAttribute = "";
			public const string PrimaryIdAttribute = "annotationid";
	
			public partial class Fields
			{
				public const string OwningTeam = "owningteam";
				public static readonly ReadOnlyCollection<string> OwningTeamTargets = new (["team"]);
			}
	
			public partial class Choices
			{
			}
		}
	
		/// <summary>
		/// Required Level: None</br>
		/// Valid for: Read</br>
		/// Targets: team</br>
		/// </summary>
		[AttributeLogicalName("owningteam")]
		public EntityReference OwningTeam
		{
			get => TryGetAttributeValue("owningteam", out EntityReference value) ? value : null;
		}
	}
	
	public TargetNote Target { get; set; }

	/// <summary>
	/// This method should be called on every <see cref="XrmGenTest.LeadCreatePlugin.Execute(IServiceProvider)"/> execution.
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
        Target = EntityOrDefault<TargetNote>(executionContext.InputParameters, "Target");
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
