#nullable enable
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System;
using System.Runtime.Serialization;

namespace XrmGenTest.TypedSdkMessages;

// This is to test the refactoring action that should generate the
// rest of the code by just having a class with the RequestProxyAttribute.

[RequestProxy("test_Echo")]
[DataContract(Namespace = "http://schemas.microsoft.com/xrm/2011/new/")]
internal class EchoRequest : OrganizationRequest
{
    public bool? BooleanParameter { get => Parameters.TryGetValue("BooleanParameter", out bool? value_booleanparameter) ? value_booleanparameter : default; set => Parameters["BooleanParameter"] = value; }
    public DateTime DateTimeParameter { get => Parameters.TryGetValue("DateTimeParameter", out DateTime value_datetimeparameter) ? value_datetimeparameter : default; set => Parameters["DateTimeParameter"] = value; }
    public decimal DecimalParameter { get => Parameters.TryGetValue("DecimalParameter", out decimal value_decimalparameter) ? value_decimalparameter : default; set => Parameters["DecimalParameter"] = value; }
    public required Entity EntityParameter { get => Parameters.TryGetValue("EntityParameter", out Entity value_entityparameter) ? value_entityparameter : default!; set => Parameters["EntityParameter"] = value; }
    public EntityCollection? EntityCollectionParameter { get => Parameters.TryGetValue("EntityCollectionParameter", out EntityCollection? value_entitycollectionparameter) ? value_entitycollectionparameter : default; set => Parameters["EntityCollectionParameter"] = value; }
    public required EntityReference EntityReferenceParameter { get => Parameters.TryGetValue("EntityReferenceParameter", out EntityReference value_entityreferenceparameter) ? value_entityreferenceparameter : default!; set => Parameters["EntityReferenceParameter"] = value; }
    public double FloatParameter { get => Parameters.TryGetValue("FloatParameter", out double value_floatparameter) ? value_floatparameter : default; set => Parameters["FloatParameter"] = value; }
    public int IntegerParameter { get => Parameters.TryGetValue("IntegerParameter", out int value_integerparameter) ? value_integerparameter : default; set => Parameters["IntegerParameter"] = value; }
    public required Money MoneyParameter { get => Parameters.TryGetValue("MoneyParameter", out Money value_moneyparameter) ? value_moneyparameter : default!; set => Parameters["MoneyParameter"] = value; }
    public required OptionSetValue PicklistParameter { get => Parameters.TryGetValue("PicklistParameter", out OptionSetValue value_picklistparameter) ? value_picklistparameter : default!; set => Parameters["PicklistParameter"] = value; }
    public required OptionSetValue EnumParameter { get => Parameters.TryGetValue("EnumParameter", out OptionSetValue value_enumparameter) ? value_enumparameter : default!; set => Parameters["EnumParameter"] = value; }
    public string? StringParameter { get => Parameters.TryGetValue("StringParameter", out string? value_stringparameter) ? value_stringparameter : default; set => Parameters["StringParameter"] = value; }
    public Guid GuidParameter { get => Parameters.TryGetValue("GuidParameter", out Guid value_guidparameter) ? value_guidparameter : default; set => Parameters["GuidParameter"] = value; }
}