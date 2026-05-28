#nullable enable
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
    public partial class EchoApi
    {
        /// <summary>
        /// This method should be called before accessing any target, image or any of your dependencies.
        /// </summary>
        protected IDisposable CreateScope(IServiceProvider serviceProvider)
        {
            var scope = new DependencyScope<EchoApi>();
            scope.Set<IServiceProvider>(serviceProvider);
        
            scope.Set<IPluginExecutionContext>((IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext)));
            return scope;
        }

        private static EchoApi.Request GetRequest(IExecutionContext context)
        {
            var request = new EchoApi.Request();
            request.DecimalParameter = context.InputParameters.TryGetValue("DecimalParameter", out decimal decimalparameter) ? decimalparameter : default;
            request.IntegerParameter = context.InputParameters.TryGetValue("IntegerParameter", out int integerparameter) ? integerparameter : default;
            request.StringParameter = context.InputParameters.TryGetValue("StringParameter", out string? stringparameter) ? stringparameter : string.Empty;
            if (context.InputParameters.TryGetValue("EntityCollectionParameter", out EntityCollection entitycollectionparameter)
                && entitycollectionparameter != null)
            {
                request.EntityCollectionParameter = entitycollectionparameter;
            }
            if (context.InputParameters.TryGetValue("CustomEntitiesParameter", out EntityCollection customentitiesparameter)
                && customentitiesparameter != null)
            {
                request.CustomEntitiesParameter = customentitiesparameter;
            }
            request.EnumParameter = context.InputParameters.TryGetValue("EnumParameter", out OptionSetValue enumparameter) ? enumparameter : default;
            if (context.InputParameters.TryGetValue("EntityParameter", out Entity entityparameter)
                && entityparameter != null)
            {
                request.EntityParameter = entityparameter;
            }
            request.PicklistParameter = context.InputParameters.TryGetValue("PicklistParameter", out OptionSetValue picklistparameter) ? picklistparameter : default;
            request.EntityReferenceParameter = context.InputParameters.TryGetValue("EntityReferenceParameter", out Microsoft.Xrm.Sdk.EntityReference entityreferenceparameter) ? entityreferenceparameter : default;
            request.DateTimeParameter = context.InputParameters.TryGetValue("DateTimeParameter", out System.DateTime datetimeparameter) ? datetimeparameter : default;
            request.BooleanParameter = context.InputParameters.TryGetValue("BooleanParameter", out bool? booleanparameter) ? booleanparameter : default;
            request.FloatParameter = context.InputParameters.TryGetValue("FloatParameter", out double floatparameter) ? floatparameter : default;
            request.StringArrayParameter = context.InputParameters.TryGetValue("StringArrayParameter", out string[] stringarrayparameter) ? stringarrayparameter : default;
            request.GuidParameter = context.InputParameters.TryGetValue("GuidParameter", out System.Guid guidparameter) ? guidparameter : default;
            if (context.InputParameters.TryGetValue("CustomEntityParameter", out Entity customentityparameter)
                && customentityparameter != null)
            {
                request.CustomEntityParameter = customentityparameter;
            }
            request.MoneyParameter = context.InputParameters.TryGetValue("MoneyParameter", out Microsoft.Xrm.Sdk.Money moneyparameter) ? moneyparameter : default;
            return request;
        }

        private static void SetResponse(IExecutionContext context, EchoApi.Response response)
        {
            if (response.PicklistParameter is Microsoft.Xrm.Sdk.OptionSetValue? picklistparameterValue) context.OutputParameters["PicklistParameter"] = new OptionSetValue((int)picklistparameterValue);
            if (response.FloatParameter is double floatparameterValue) context.OutputParameters["FloatParameter"] = floatparameterValue;
            if (response.StringParameter is string stringparameterValue) context.OutputParameters["StringParameter"] = stringparameterValue;
            if (response.MoneyParameter is Money moneyparameterValue) context.OutputParameters["MoneyParameter"] = moneyparameterValue;
            if (response.EntityParameter is Entity entityparameterValue)
            {
                context.OutputParameters["EntityParameter"] = entityparameterValue;
            }
            if (response.StringArrayParameter is string[] stringarrayparameterValue) context.OutputParameters["StringArrayParameter"] = stringarrayparameterValue;
            if (response.BooleanParameter is bool booleanparameterValue) context.OutputParameters["BooleanParameter"] = booleanparameterValue;
            if (response.DateTimeParameter is DateTime datetimeparameterValue) context.OutputParameters["DateTimeParameter"] = datetimeparameterValue;
            if (response.CustomEntityParameter is Entity customentityparameterValue)
            {
                context.OutputParameters["CustomEntityParameter"] = customentityparameterValue;
            }
            if (response.EnumParameter is Microsoft.Xrm.Sdk.OptionSetValue? enumparameterValue) context.OutputParameters["EnumParameter"] = new OptionSetValue((int)enumparameterValue);
            if (response.GuidParameter is Guid guidparameterValue) context.OutputParameters["GuidParameter"] = guidparameterValue;
            if (response.CustomEntitiesParameter is EntityCollection customentitiesparameterValue)
            {
                context.OutputParameters["CustomEntitiesParameter"] = customentitiesparameterValue;
            }
            if (response.EntityCollectionParameter is EntityCollection entitycollectionparameterValue)
            {
                context.OutputParameters["EntityCollectionParameter"] = entitycollectionparameterValue;
            }
            if (response.EntityReferenceParameter is EntityReference entityreferenceparameterValue) context.OutputParameters["EntityReferenceParameter"] = entityreferenceparameterValue;
            if (response.IntegerParameter is int integerparameterValue) context.OutputParameters["IntegerParameter"] = integerparameterValue;
            if (response.DecimalParameter is decimal decimalparameterValue) context.OutputParameters["DecimalParameter"] = decimalparameterValue;
        }

	    private static T? EntityOrDefault<T>(DataCollection<string, object> keyValues, string key) where T : Entity
        {
            if (keyValues is null) return default;
            return keyValues.TryGetValue(key, out var obj) ? obj is Entity entity ? entity.ToEntity<T>() : default : default;
        }
        private static T? EntityOrDefault<T>(DataCollection<string, Entity> keyValues, string key) where T : Entity
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

        private static T Require<T>() => DependencyScope<EchoApi>.Current.Require<T>();
        private static T Require<T>(string name) => DependencyScope<EchoApi>.Current.Require<T>(name);

        private static bool TryGet<T>(out T instance) => DependencyScope<EchoApi>.Current.TryGet(out instance);
        private static bool TryGet<T>(string name, out T instance) => DependencyScope<EchoApi>.Current.TryGet(name, out instance);

        private static T Set<T>(T instance) => DependencyScope<EchoApi>.Current.Set(instance);
        private static T Set<T>(string name, T instance) => DependencyScope<EchoApi>.Current.Set(name, instance);
        private static T SetAndTrack<T>(T instance) where T : IDisposable => DependencyScope<EchoApi>.Current.SetAndTrack(instance);
        private static T SetAndTrack<T>(string name, T instance) where T : IDisposable => DependencyScope<EchoApi>.Current.SetAndTrack(name, instance);
    }
}
#nullable restore