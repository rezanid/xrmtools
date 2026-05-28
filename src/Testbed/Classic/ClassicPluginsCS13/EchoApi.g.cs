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
            request.DecimalParameter = context.InputParameters.TryGetValue("DecimalParameter", out decimal decimalparameter) ? decimalparameter : throw new InvalidPluginExecutionException("Required input parameter 'DecimalParameter' was not provided.");
            request.OptionalDecimalParameter = context.InputParameters.TryGetValue("OptionalDecimalParameter", out decimal? optionaldecimalparameter) ? optionaldecimalparameter : default;
            request.IntegerParameter = context.InputParameters.TryGetValue("IntegerParameter", out int integerparameter) ? integerparameter : throw new InvalidPluginExecutionException("Required input parameter 'IntegerParameter' was not provided.");
            request.OptionalStringParameter = context.InputParameters.TryGetValue("OptionalStringParameter", out string? optionalstringparameter) ? optionalstringparameter : default;
            request.StringParameter = context.InputParameters.TryGetValue("StringParameter", out string stringparameter) ? stringparameter : throw new InvalidPluginExecutionException("Required input parameter 'StringParameter' was not provided.");
            if (context.InputParameters.TryGetValue("EntityCollectionParameter", out EntityCollection entitycollectionparameter)
                && entitycollectionparameter != null)
            {
                request.EntityCollectionParameter = entitycollectionparameter;
            }
            else
            {
                throw new InvalidPluginExecutionException("Required input parameter 'EntityCollectionParameter' was not provided.");
            }
            if (context.InputParameters.TryGetValue("OptionalEntityCollectionParameter", out EntityCollection optionalentitycollectionparameter)
                && optionalentitycollectionparameter != null)
            {
                request.OptionalEntityCollectionParameter = optionalentitycollectionparameter;
            }
            if (context.InputParameters.TryGetValue("CustomEntitiesParameter", out EntityCollection customentitiesparameter)
                && customentitiesparameter != null)
            {
                request.CustomEntitiesParameter = customentitiesparameter.Entities
                    .Select(e => new XrmGenTest.CustomEntity
                    {
                        Id = e.Id,
                        Attributes = e.Attributes
                    }).ToList();
            }
            else
            {
                throw new InvalidPluginExecutionException("Required input parameter 'CustomEntitiesParameter' was not provided.");
            }
            if (context.InputParameters.TryGetValue("OptionalCustomEntitiesParameter", out EntityCollection optionalcustomentitiesparameter)
                && optionalcustomentitiesparameter != null)
            {
                request.OptionalCustomEntitiesParameter = optionalcustomentitiesparameter.Entities
                    .Select(e => new XrmGenTest.CustomEntity
                    {
                        Id = e.Id,
                        Attributes = e.Attributes
                    }).ToList();
            }
            request.EnumParameter = context.InputParameters.TryGetValue("EnumParameter", out OptionSetValue enumparameter) ? (XrmGenTest.TestEnum)enumparameter.Value : throw new InvalidPluginExecutionException("Required input parameter 'EnumParameter' was not provided.");
            request.OptionalEnumParameter = context.InputParameters.TryGetValue("OptionalEnumParameter", out OptionSetValue optionalenumparameter) ? (XrmGenTest.TestEnum)optionalenumparameter.Value : default;
            if (context.InputParameters.TryGetValue("EntityParameter", out Entity entityparameter)
                && entityparameter != null)
            {
                request.EntityParameter = entityparameter;
            }
            else
            {
                throw new InvalidPluginExecutionException("Required input parameter 'EntityParameter' was not provided.");
            }
            if (context.InputParameters.TryGetValue("OptionalEntityParameter", out Entity optionalentityparameter)
                && optionalentityparameter != null)
            {
                request.OptionalEntityParameter = optionalentityparameter;
            }
            request.PicklistParameter = context.InputParameters.TryGetValue("PicklistParameter", out OptionSetValue picklistparameter) ? picklistparameter : throw new InvalidPluginExecutionException("Required input parameter 'PicklistParameter' was not provided.");
            request.OptionalPicklistParameter = context.InputParameters.TryGetValue("OptionalPicklistParameter", out OptionSetValue optionalpicklistparameter) ? optionalpicklistparameter : default;
            request.EntityReferenceParameter = context.InputParameters.TryGetValue("EntityReferenceParameter", out Microsoft.Xrm.Sdk.EntityReference entityreferenceparameter) ? entityreferenceparameter : throw new InvalidPluginExecutionException("Required input parameter 'EntityReferenceParameter' was not provided.");
            request.OptionalEntityReferenceParameter = context.InputParameters.TryGetValue("OptionalEntityReferenceParameter", out Microsoft.Xrm.Sdk.EntityReference? optionalentityreferenceparameter) ? optionalentityreferenceparameter : default;
            request.DateTimeParameter = context.InputParameters.TryGetValue("DateTimeParameter", out System.DateTime datetimeparameter) ? datetimeparameter : throw new InvalidPluginExecutionException("Required input parameter 'DateTimeParameter' was not provided.");
            request.OptionalDateTimeParameter = context.InputParameters.TryGetValue("OptionalDateTimeParameter", out System.DateTime? optionaldatetimeparameter) ? optionaldatetimeparameter : default;
            request.BooleanParameter = context.InputParameters.TryGetValue("BooleanParameter", out bool booleanparameter) ? booleanparameter : throw new InvalidPluginExecutionException("Required input parameter 'BooleanParameter' was not provided.");
            request.OptionalBooleanParameter = context.InputParameters.TryGetValue("OptionalBooleanParameter", out bool? optionalbooleanparameter) ? optionalbooleanparameter : default;
            request.FloatParameter = context.InputParameters.TryGetValue("FloatParameter", out double floatparameter) ? floatparameter : throw new InvalidPluginExecutionException("Required input parameter 'FloatParameter' was not provided.");
            request.OptionalFloatParameter = context.InputParameters.TryGetValue("OptionalFloatParameter", out double? optionalfloatparameter) ? optionalfloatparameter : default;
            request.StringArrayParameter = context.InputParameters.TryGetValue("StringArrayParameter", out string[] stringarrayparameter) ? stringarrayparameter : throw new InvalidPluginExecutionException("Required input parameter 'StringArrayParameter' was not provided.");
            request.OptionalStringArrayParameter = context.InputParameters.TryGetValue("OptionalStringArrayParameter", out string[] optionalstringarrayparameter) ? optionalstringarrayparameter : default;
            request.GuidParameter = context.InputParameters.TryGetValue("GuidParameter", out System.Guid guidparameter) ? guidparameter : throw new InvalidPluginExecutionException("Required input parameter 'GuidParameter' was not provided.");
            request.OptionalGuidParameter = context.InputParameters.TryGetValue("OptionalGuidParameter", out System.Guid? optionalguidparameter) ? optionalguidparameter : default;
            if (context.InputParameters.TryGetValue("CustomEntityParameter", out Entity customentityparameter)
                && customentityparameter != null)
            {
                request.CustomEntityParameter = new XrmGenTest.CustomEntity
                {
                    Id = customentityparameter.Id,
                    Attributes = customentityparameter.Attributes
                };
            }
            else
            {
                throw new InvalidPluginExecutionException("Required input parameter 'CustomEntityParameter' was not provided.");
            }
            if (context.InputParameters.TryGetValue("OptionalCustomEntityParameter", out Entity optionalcustomentityparameter)
                && optionalcustomentityparameter != null)
            {
                request.OptionalCustomEntityParameter = new XrmGenTest.CustomEntity
                {
                    Id = optionalcustomentityparameter.Id,
                    Attributes = optionalcustomentityparameter.Attributes
                };
            }
            request.MoneyParameter = context.InputParameters.TryGetValue("MoneyParameter", out Microsoft.Xrm.Sdk.Money moneyparameter) ? moneyparameter : throw new InvalidPluginExecutionException("Required input parameter 'MoneyParameter' was not provided.");
            request.OptionalMoneyParameter = context.InputParameters.TryGetValue("OptionalMoneyParameter", out Microsoft.Xrm.Sdk.Money? optionalmoneyparameter) ? optionalmoneyparameter : default;
            return request;
        }

        private static void SetResponse(IExecutionContext context, EchoApi.Response response)
        {
            if (response.PicklistParameter is Microsoft.Xrm.Sdk.OptionSetValue picklistparameterValue) context.OutputParameters["PicklistParameter"] = picklistparameterValue;
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
            if (response.EnumParameter is TestEnum enumparameterValue) context.OutputParameters["EnumParameter"] = enumparameterValue;
            if (response.GuidParameter is Guid guidparameterValue) context.OutputParameters["GuidParameter"] = guidparameterValue;
            if (response.CustomEntitiesParameter is System.Collections.Generic.IEnumerable<XrmGenTest.CustomEntity> customentitiesparameterValue)
            {
                context.OutputParameters["CustomEntitiesParameter"] =
                    new EntityCollection(customentitiesparameterValue.Select(e => e.ToEntity<Entity>()).ToList());
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