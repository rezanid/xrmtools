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
    public partial class LeadCreatePlugin
    {
        /// <summary>
        /// This method should be called before accessing any target, image or any of your dependencies.
        /// </summary>
        protected IDisposable CreateScope(IServiceProvider serviceProvider)
        {
            var scope = new DependencyScope<LeadCreatePlugin>();
            scope.Set<IServiceProvider>(serviceProvider);
        
            scope.Set<IPluginExecutionContext>((IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext)));
            return scope;
        }
    
        public static UpdateAnnotationEntity Target
        {
            get => EntityOrDefault<UpdateAnnotationEntity>(Require<IPluginExecutionContext>().InputParameters, "Target");
        }
            
        [EntityLogicalName("annotation")]
        public class UpdateAnnotationEntity : Entity
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
        			public static readonly ReadOnlyCollection<string> OwningTeamTargets =
        				new ReadOnlyCollection<string>(new string[] { "team" });
        		
        			public static bool TryGet(string logicalName, out string attribute)
        			{
        				switch (logicalName)
        				{
        					case nameof(OwningTeam): attribute = OwningTeam; return true;
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

        private static T Require<T>() => DependencyScope<LeadCreatePlugin>.Current.Require<T>();
        private static T Require<T>(string name) => DependencyScope<LeadCreatePlugin>.Current.Require<T>(name);

        private static bool TryGet<T>(out T instance) => DependencyScope<LeadCreatePlugin>.Current.TryGet(out instance);
        private static bool TryGet<T>(string name, out T instance) => DependencyScope<LeadCreatePlugin>.Current.TryGet(name, out instance);

        private static T Set<T>(T instance) => DependencyScope<LeadCreatePlugin>.Current.Set(instance);
        private static T Set<T>(string name, T instance) => DependencyScope<LeadCreatePlugin>.Current.Set(name, instance);
        private static T SetAndTrack<T>(T instance) where T : IDisposable => DependencyScope<LeadCreatePlugin>.Current.SetAndTrack(instance);
        private static T SetAndTrack<T>(string name, T instance) where T : IDisposable => DependencyScope<LeadCreatePlugin>.Current.SetAndTrack(name, instance);
    }
}
