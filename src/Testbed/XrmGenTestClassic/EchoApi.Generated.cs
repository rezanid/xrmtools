using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Extensions;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;

namespace XrmGenTest
{
    [GeneratedCode("TemplatedCodeGenerator", "1.3.3.0")]
    public partial class EchoApi
    {
	    /// <summary>
	    /// This method should be called on every <see cref="XrmGenTest.EchoApi.Execute(IServiceProvider)"/> execution.
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

        protected static EchoApi.Request GetRequest(IExecutionContext context)
        {
            var request = new EchoApi.Request();
            request.BooleanParameter = context.InputParameters.TryGetValue("BooleanParameter", out bool? booleanparameter) ? booleanparameter : default;
            request.DateTimeParameter = context.InputParameters.TryGetValue("DateTimeParameter", out System.DateTime datetimeparameter) ? datetimeparameter : default;
            request.DecimalParameter = context.InputParameters.TryGetValue("DecimalParameter", out decimal decimalparameter) ? decimalparameter : default;
            if (context.InputParameters.TryGetValue("EntityParameter", out Entity entityparameter)
                && entityparameter != null)
            {
                request.EntityParameter = entityparameter;
            }
            if (context.InputParameters.TryGetValue("ContactParameter", out Entity contactparameter)
                && contactparameter != null)
            {
                request.ContactParameter = new XrmGenTest.Contact(contactparameter.Id)
                { 
                    Attributes = contactparameter.Attributes
                };
            }
            if (context.InputParameters.TryGetValue("TypedExpandoParameter", out Entity typedexpandoparameter)
                && typedexpandoparameter != null)
            {
                request.TypedExpandoParameter = new XrmGenTest.TypedExpando(typedexpandoparameter.Id)
                { 
                    Attributes = typedexpandoparameter.Attributes
                };
            }
            if (context.InputParameters.TryGetValue("EntityCollectionParameter", out EntityCollection entitycollectionparameter)
                && entitycollectionparameter != null)
            {
                request.EntityCollectionParameter = entitycollectionparameter;
            }
            if (context.InputParameters.TryGetValue("CustomEntitiesParameter", out EntityCollection customentitiesparameter)
                && customentitiesparameter != null)
            {
                request.CustomEntitiesParameter = customentitiesparameter.Entities
                    .Select(e => new XrmGenTest.Contact(e.Id)
                    {
                        Attributes = e.Attributes
                    }).ToList();
            }
            request.EntityReferenceParameter = context.InputParameters.TryGetValue("EntityReferenceParameter", out Microsoft.Xrm.Sdk.EntityReference entityreferenceparameter) ? entityreferenceparameter : default;
            request.FloatParameter = context.InputParameters.TryGetValue("FloatParameter", out double floatparameter) ? floatparameter : default;
            request.IntegerParameter = context.InputParameters.TryGetValue("IntegerParameter", out int integerparameter) ? integerparameter : default;
            request.MoneyParameter = context.InputParameters.TryGetValue("MoneyParameter", out Microsoft.Xrm.Sdk.Money moneyparameter) ? moneyparameter : default;
            request.PicklistParameter = context.InputParameters.TryGetValue("PicklistParameter", out OptionSetValue picklistparameter) ? picklistparameter : default;
            request.EnumParameter = context.InputParameters.TryGetValue("EnumParameter", out OptionSetValue enumparameter) ? (XrmGenTest.TestEnum)enumparameter.Value : default;
            request.StringParameter = context.InputParameters.TryGetValue("StringParameter", out string stringparameter) ? stringparameter : string.Empty;
            request.StringArrayParameter = context.InputParameters.TryGetValue("StringArrayParameter", out string[] stringarrayparameter) ? stringarrayparameter : default;
            request.GuidParameter = context.InputParameters.TryGetValue("GuidParameter", out System.Guid guidparameter) ? guidparameter : default;
            return request;
        }

        protected static void SetResponse(IExecutionContext context, EchoApi.Response response)
        {
            if (response.BooleanParameter is bool booleanparameterValue) context.OutputParameters["BooleanParameter"] = booleanparameterValue;
            if (response.DateTimeParameter is DateTime datetimeparameterValue) context.OutputParameters["DateTimeParameter"] = datetimeparameterValue;
            if (response.DecimalParameter is decimal decimalparameterValue) context.OutputParameters["DecimalParameter"] = decimalparameterValue;
            if (response.EntityParameter is Entity entityparameterValue)
            {
                context.OutputParameters["EntityParameter"] = entityparameterValue;
            }
            if (response.ContactParameter is XrmGenTest.Contact contactparameterValue)
            {
                context.OutputParameters["ContactParameter"] = new Entity(contactparameterValue.LogicalName, contactparameterValue.Id)
                {
                    Attributes = contactparameterValue.Attributes
                };
            }
            if (response.TypedExpandoParameter is XrmGenTest.TypedExpando typedexpandoparameterValue)
            {
                context.OutputParameters["TypedExpandoParameter"] = new Entity(typedexpandoparameterValue.LogicalName, typedexpandoparameterValue.Id)
                {
                    Attributes = typedexpandoparameterValue.Attributes
                };
            }
            if (response.EntityCollectionParameter is EntityCollection entitycollectionparameterValue)
            {
                context.OutputParameters["EntityCollectionParameter"] = entitycollectionparameterValue;
            }
            if (response.CustomEntitiesParameter is System.Collections.Generic.IEnumerable<XrmGenTest.Contact> customentitiesparameterValue)
            {
                context.OutputParameters["CustomEntitiesParameter"] =
                    new EntityCollection(customentitiesparameterValue.Select(e => e.ToEntity<Entity>()).ToList());
            }
            if (response.EntityReferenceParameter is EntityReference entityreferenceparameterValue) context.OutputParameters["EntityReferenceParameter"] = entityreferenceparameterValue;
            if (response.FloatParameter is double floatparameterValue) context.OutputParameters["FloatParameter"] = floatparameterValue;
            if (response.IntegerParameter is int integerparameterValue) context.OutputParameters["IntegerParameter"] = integerparameterValue;
            if (response.MoneyParameter is Money moneyparameterValue) context.OutputParameters["MoneyParameter"] = moneyparameterValue;
            if (response.PicklistParameter is Microsoft.Xrm.Sdk.OptionSetValue picklistparameterValue) context.OutputParameters["PicklistParameter"] = picklistparameterValue;
            if (response.EnumParameter is XrmGenTest.TestEnum enumparameterValue) context.OutputParameters["EnumParameter"] = new OptionSetValue((int)enumparameterValue);
            if (response.StringParameter is string stringparameterValue) context.OutputParameters["StringParameter"] = stringparameterValue;
            if (response.StringArrayParameter is string[] stringarrayparameterValue) context.OutputParameters["StringArrayParameter"] = stringarrayparameterValue;
            if (response.GuidParameter is Guid guidparameterValue) context.OutputParameters["GuidParameter"] = guidparameterValue;
            if (response.Logs is string[] logsValue) context.OutputParameters["Logs"] = logsValue;
        }
    }
}
