#nullable enable
using Microsoft.Xrm.Sdk;
using System;
using XrmTools.Meta.Attributes;

namespace XrmGenTest.Client;

internal abstract class CustomRequest<TRequest, TResponse>(string requestName)
    where TRequest : CustomRequest<TRequest, TResponse>, new()
    where TResponse : CustomResponse<TResponse>, new()
{
    public string RequestName { get; } = requestName;

    public abstract OrganizationRequest ToOrganizationRequest();
}


[CustomApiRequest("test_Echo")]
internal class EchoRequest1 : ITypedOrganizationRequest
{
    public string RequestName => "test_Echo";

    public required decimal DecimalParameter { get; set; }
    public required int IntegerParameter { get; set; }
    public string? StringParameter { get; set; }
    public EntityCollection? EntityCollectionParameter { get; set; }
    public required OptionSetValue EnumParameter { get; set; }
    public required Entity EntityParameter { get; set; }
    public required OptionSetValue PicklistParameter { get; set; }
    public required EntityReference EntityReferenceParameter { get; set; }
    public required DateTime DateTimeParameter { get; set; }
    public bool? BooleanParameter { get; set; }
    public required float FloatParameter { get; set; }
    public required string[] StringArrayParameter { get; set; }
    public required Guid GuidParameter { get; set; }
    public required Money MoneyParameter { get; set; }

    public OrganizationRequest ToOrganizationRequest()
    {
        var  request = new OrganizationRequest(RequestName);
        request.Parameters.AddOrUpdateIfNotNull("RequestName", RequestName);
        request.Parameters.AddOrUpdateIfNotNull("DecimalParameter", DecimalParameter);
        request.Parameters.AddOrUpdateIfNotNull("IntegerParameter", IntegerParameter);
        request.Parameters.AddOrUpdateIfNotNull("StringParameter", StringParameter);
        request.Parameters.AddOrUpdateIfNotNull("EntityCollectionParameter", EntityCollectionParameter);
        request.Parameters.AddOrUpdateIfNotNull("EnumParameter", EnumParameter);
        request.Parameters.AddOrUpdateIfNotNull("EntityParameter", EntityParameter);
        request.Parameters.AddOrUpdateIfNotNull("PicklistParameter", PicklistParameter);
        request.Parameters.AddOrUpdateIfNotNull("EntityReferenceParameter", EntityReferenceParameter);
        request.Parameters.AddOrUpdateIfNotNull("DateTimeParameter", DateTimeParameter);
        request.Parameters.AddOrUpdateIfNotNull("BooleanParameter", BooleanParameter);
        request.Parameters.AddOrUpdateIfNotNull("FloatParameter", FloatParameter);
        request.Parameters.AddOrUpdateIfNotNull("StringArrayParameter", StringArrayParameter);
        request.Parameters.AddOrUpdateIfNotNull("GuidParameter", GuidParameter);
        request.Parameters.AddOrUpdateIfNotNull("MoneyParameter", MoneyParameter);
        return request;
    }
}

[CustomApiResponse("test_Echo")]
internal class EchoResponse1 : ITypedOrganizationResponse
{
    public bool? BooleanParameter { get; set; }
    public DateTime? DateTimeParameter { get; set; }
    public decimal? DecimalParameter { get; set; }
    public Entity? EntityParameter { get; set; }
    public EntityCollection? EntityCollectionParameter { get; set; }
    public EntityReference? EntityReferenceParameter { get; set; }
    public float? FloatParameter { get; set; }
    public int? IntegerParameter { get; set; }
    public Money? MoneyParameter { get; set; }
    public OptionSetValue? PicklistParameter { get; set; }
    public OptionSetValue? EnumParameter { get; set; }
    public string? StringParameter { get; set; }
    public string[]? StringArrayParameter { get; set; }
    public Guid? GuidParameter { get; set; }

    public EchoResponse1() : base() { }

    public void LoadFromOrganizationResponse(OrganizationResponse response)
    {
        BooleanParameter = response.Results.TryGetValue("BooleanParameter", out bool? booleanParameter) ? booleanParameter : default;
        DateTimeParameter = response.Results.TryGetValue("DateTimeParameter", out DateTime? dateTimeParameter) ? dateTimeParameter : default;
        DecimalParameter = response.Results.TryGetValue("DecimalParameter", out decimal? decimalParameter) ? decimalParameter : default;
        EntityParameter = response.Results.TryGetValue("EntityParameter", out Entity? entityParameter) ? entityParameter : default;
        EntityCollectionParameter = response.Results.TryGetValue("EntityCollectionParameter", out EntityCollection? entityCollectionParameter) ? entityCollectionParameter : default;
        EntityReferenceParameter = response.Results.TryGetValue("EntityReferenceParameter", out EntityReference? entityReferenceParameter) ? entityReferenceParameter : default;
        FloatParameter = response.Results.TryGetValue("FloatParameter", out float? floatParameter) ? floatParameter : default;
        IntegerParameter = response.Results.TryGetValue("IntegerParameter", out int? integerParameter) ? integerParameter : default;
        MoneyParameter = response.Results.TryGetValue("MoneyParameter", out Money? moneyParameter) ? moneyParameter : default;
        PicklistParameter = response.Results.TryGetValue("PicklistParameter", out OptionSetValue? picklistParameter) ? picklistParameter : default;
        EnumParameter = response.Results.TryGetValue("EnumParameter", out OptionSetValue? enumParameter) ? enumParameter : default;
        StringParameter = response.Results.TryGetValue("StringParameter", out string? stringParameter) ? stringParameter : default;
        StringArrayParameter = response.Results.TryGetValue("StringArrayParameter", out string[]? stringArrayParameter) ? stringArrayParameter : default;
        GuidParameter = response.Results.TryGetValue("GuidParameter", out Guid? guidParameter) ? guidParameter : default;
    }
}
#nullable restore