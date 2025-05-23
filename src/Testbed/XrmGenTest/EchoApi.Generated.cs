using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Extensions;
using System;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
namespace XrmGenTest;

[GeneratedCode("TemplatedCodeGenerator", "1.0.5.0")]
public partial class EchoApi
{
    protected void InjectDependencies(IServiceProvider serviceProvider)
    {
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
        => new()
        {
            BooleanParameter = context.InputParameters.TryGetValue("BooleanParameter", out bool? BooleanParameter) ? BooleanParameter : default,
            DateTimeParameter = context.InputParameters.TryGetValue("DateTimeParameter", out System.DateTime DateTimeParameter) ? DateTimeParameter : default,
            DecimalParameter = context.InputParameters.TryGetValue("DecimalParameter", out decimal DecimalParameter) ? DecimalParameter : default,
            EntityParameter = context.InputParameters.TryGetValue("EntityParameter", out Microsoft.Xrm.Sdk.Entity EntityParameter) ? EntityParameter : default,
            EntityCollectionParameter = context.InputParameters.TryGetValue("EntityCollectionParameter", out Microsoft.Xrm.Sdk.EntityCollection? EntityCollectionParameter) ? EntityCollectionParameter : default,
            EntityReferenceParameter = context.InputParameters.TryGetValue("EntityReferenceParameter", out Microsoft.Xrm.Sdk.EntityReference EntityReferenceParameter) ? EntityReferenceParameter : default,
            FloatParameter = context.InputParameters.TryGetValue("FloatParameter", out float FloatParameter) ? FloatParameter : default,
            IntegerParameter = context.InputParameters.TryGetValue("IntegerParameter", out int IntegerParameter) ? IntegerParameter : default,
            MoneyParameter = context.InputParameters.TryGetValue("MoneyParameter", out Microsoft.Xrm.Sdk.Money MoneyParameter) ? MoneyParameter : default,
            PicklistParameter = context.InputParameters.TryGetValue("PicklistParameter", out OptionSetValue PicklistParameter) ? PicklistParameter : default,
            EnumParameter = context.InputParameters.TryGetValue("EnumParameter", out OptionSetValue EnumParameter) ? (XrmGenTest.TestEnum)EnumParameter.Value : default,
            StringParameter = context.InputParameters.TryGetValue("StringParameter", out string StringParameter) ? StringParameter : default,
            StringArrayParameter = context.InputParameters.TryGetValue("StringArrayParameter", out string[] StringArrayParameter) ? StringArrayParameter : default,
            GuidParameter = context.InputParameters.TryGetValue("GuidParameter", out System.Guid GuidParameter) ? GuidParameter : default,
        };
    
    protected static void SetResponse(IExecutionContext context, EchoApi.Response response)
    {
        if (response.BooleanParameter is Boolean booleanValue) context.OutputParameters["BooleanParameter"] = booleanValue;
        if (response.DateTimeParameter is DateTime datetimeValue) context.OutputParameters["DateTimeParameter"] = datetimeValue;
        if (response.DecimalParameter is Decimal decimalValue) context.OutputParameters["DecimalParameter"] = decimalValue;
        if (response.EntityParameter is Entity entityValue) context.OutputParameters["EntityParameter"] = entityValue;
        if (response.EntityCollectionParameter is EntityCollection entitycollectionValue) context.OutputParameters["EntityCollectionParameter"] = entitycollectionValue;
        if (response.EntityReferenceParameter is EntityReference entityreferenceValue) context.OutputParameters["EntityReferenceParameter"] = entityreferenceValue;
        if (response.FloatParameter is Single singleValue) context.OutputParameters["FloatParameter"] = singleValue;
        if (response.IntegerParameter is Int32 int32Value) context.OutputParameters["IntegerParameter"] = int32Value;
        if (response.MoneyParameter is Money moneyValue) context.OutputParameters["MoneyParameter"] = moneyValue;
        if (response.PicklistParameter is Microsoft.Xrm.Sdk.OptionSetValue optionsetvalueValue) context.OutputParameters["PicklistParameter"] = optionsetvalueValue;
        if (response.EnumParameter is XrmGenTest.TestEnum testenumValue) context.OutputParameters["EnumParameter"] = new OptionSetValue((int)testenumValue);
        if (response.StringParameter is String stringValue) context.OutputParameters["StringParameter"] = stringValue;
        if (response.StringArrayParameter is string[] stringArrayValue) context.OutputParameters["StringArrayParameter"] = stringArrayValue;
        if (response.GuidParameter is Guid guidValue) context.OutputParameters["GuidParameter"] = guidValue;
    }
}
