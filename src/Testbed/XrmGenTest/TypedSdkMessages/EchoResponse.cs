#nullable enable
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System;
using System.Runtime.Serialization;

namespace XrmGenTest.TypedSdkMessages;

// This is to test the refactoring action that should generate the
// rest of the code by just having a class with the RequestProxyAttribute.

[ResponseProxy("test_Echo")]
[DataContract(Namespace = "http://schemas.microsoft.com/xrm/2011/new/")]
internal class EchoResponse : OrganizationResponse
{
    public bool? BooleanParameter => Results.TryGetValue("BooleanParameter", out bool? value_booleanparameter) ? value_booleanparameter : default;

    public DateTime? DateTimeParameter => Results.TryGetValue("DateTimeParameter", out DateTime? value_datetimeparameter) ? value_datetimeparameter : default;

    public decimal? DecimalParameter => Results.TryGetValue("DecimalParameter", out decimal? value_decimalparameter) ? value_decimalparameter : default;

    public Entity? EntityParameter => Results.TryGetValue("EntityParameter", out Entity? value_entityparameter) ? value_entityparameter : default;

    public EntityCollection? EntityCollectionParameter => Results.TryGetValue("EntityCollectionParameter", out EntityCollection? value_entitycollectionparameter) ? value_entitycollectionparameter : default;

    public EntityReference? EntityReferenceParameter => Results.TryGetValue("EntityReferenceParameter", out EntityReference? value_entityreferenceparameter) ? value_entityreferenceparameter : default;

    public double? FloatParameter => Results.TryGetValue("FloatParameter", out double? value_floatparameter) ? value_floatparameter : default;

    public int? IntegerParameter => Results.TryGetValue("IntegerParameter", out int? value_integerparameter) ? value_integerparameter : default;

    public Money? MoneyParameter => Results.TryGetValue("MoneyParameter", out Money? value_moneyparameter) ? value_moneyparameter : default;

    public OptionSetValue? PicklistParameter => Results.TryGetValue("PicklistParameter", out OptionSetValue? value_picklistparameter) ? value_picklistparameter : default;

    public OptionSetValue? EnumParameter => Results.TryGetValue("EnumParameter", out OptionSetValue? value_enumparameter) ? value_enumparameter : default;

    public string? StringParameter => Results.TryGetValue("StringParameter", out string? value_stringparameter) ? value_stringparameter : default;

    public Guid? GuidParameter => Results.TryGetValue("GuidParameter", out Guid? value_guidparameter) ? value_guidparameter : default;
}