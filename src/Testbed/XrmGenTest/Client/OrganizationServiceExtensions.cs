using Microsoft.Xrm.Sdk;
using System;
using XrmTools.Meta.Attributes;

namespace XrmGenTest.Client;

internal static class OrganizationServiceExtensions
{
    public static TResponse ExecuteTyped<TRequest, TResponse>(
        this IOrganizationService service,
        TRequest request)
        where TRequest : ITypedOrganizationRequest
        where TResponse : ITypedOrganizationResponse, new()
    {
        var orgRequest = request.ToOrganizationRequest();
        var orgResponse = service.Execute(orgRequest);
        
        var typedResponse = new TResponse();
        typedResponse.LoadFromOrganizationResponse(orgResponse);
        return typedResponse;
    }
}

internal interface ITypedOrganizationRequest
{
    public string RequestName { get; }
    OrganizationRequest ToOrganizationRequest();
}

internal interface ITypedOrganizationResponse
{
    public void LoadFromOrganizationResponse(OrganizationResponse response);
}

internal class EchoRequest : ITypedOrganizationRequest
{
    public string RequestName => "test_Echo";
    public bool BooleanParameter { get; set; }
    public DateTime DateTimeParameter { get; set; }
    public decimal DecimalParameter { get; set; }
    public Entity EntityParameter { get; set; }
    public EntityCollection EntityCollectionParameter { get; set; }
    public EntityReference EntityReferenceParameter { get; set; }
    public float FloatParameter { get; set; }
    public int IntegerParameter { get; set; }
    public Money MoneyParameter { get; set; }
    public OptionSetValue PicklistParameter { get; set; }
    public TestEnum EnumParameter { get; set; }
    [CustomApiRequestParameter(IsOptional = true)]
    public string StringParameter { get; set; }
    public string[] StringArrayParameter { get; set; }
    public Guid GuidParameter { get; set; }

    public OrganizationRequest ToOrganizationRequest()
    {
        var request = new OrganizationRequest(RequestName);
        request.Parameters.AddOrUpdateIfNotNull("BooleanParameter", BooleanParameter);
        request.Parameters.AddOrUpdateIfNotNull("DateTimeParameter", DateTimeParameter);
        request.Parameters.AddOrUpdateIfNotNull("DecimalParameter", DecimalParameter);
        request.Parameters.AddOrUpdateIfNotNull("EntityParameter", EntityParameter);
        request.Parameters.AddOrUpdateIfNotNull("EntityCollectionParameter", EntityCollectionParameter);
        request.Parameters.AddOrUpdateIfNotNull("EntityReferenceParameter", EntityReferenceParameter);
        request.Parameters.AddOrUpdateIfNotNull("FloatParameter", FloatParameter);
        request.Parameters.AddOrUpdateIfNotNull("IntegerParameter", IntegerParameter);
        request.Parameters.AddOrUpdateIfNotNull("MoneyParameter", MoneyParameter);
        request.Parameters.AddOrUpdateIfNotNull("PicklistParameter", PicklistParameter);
        request.Parameters.AddOrUpdateIfNotNull("EnumParameter", EnumParameter);
        request.Parameters.AddOrUpdateIfNotNull("StringParameter", StringParameter);
        request.Parameters.AddOrUpdateIfNotNull("StringArrayParameter", StringArrayParameter);
        request.Parameters.AddOrUpdateIfNotNull("GuidParameter", GuidParameter);
        return request;
    }
}

internal class EchoResponse : ITypedOrganizationResponse
{
    public bool BooleanParameter { get; set; }
    public DateTime DateTimeParameter { get; set; }
    public decimal DecimalParameter { get; set; }
    public Entity? EntityParameter { get; set; }
    public EntityCollection? EntityCollectionParameter { get; set; }
    public EntityReference? EntityReferenceParameter { get; set; }
    public float FloatParameter { get; set; }
    public int IntegerParameter { get; set; }
    public Money? MoneyParameter { get; set; }
    public OptionSetValue? PicklistParameter { get; set; }
    public TestEnum EnumParameter { get; set; }
    public string? StringParameter { get; set; }
    public string[] StringArrayParameter { get; set; } = [];
    public Guid GuidParameter { get; set; }


    public void LoadFromOrganizationResponse(OrganizationResponse response)
    {
        BooleanParameter = response.Results.TryGetValue("BooleanParameter", out var booleanValue) && booleanValue is bool booleanResult ? booleanResult : default;
        DateTimeParameter = response.Results.TryGetValue("DateTimeParameter", out var datetimeValue) && datetimeValue is DateTime datetimeResult ? datetimeResult : default;
        DecimalParameter = response.Results.TryGetValue("DecimalParameter", out var decimalValue) && decimalValue is decimal decimalResult ? decimalResult : default;
        EntityParameter = response.Results.TryGetValue("EntityParameter", out var entityValue) && entityValue is Entity entityResult ? entityResult : default;
        EntityCollectionParameter = response.Results.TryGetValue("EntityCollectionParameter", out var entityCollectionValue) && entityCollectionValue is EntityCollection entityCollectionResult ? entityCollectionResult : default;
        EntityReferenceParameter = response.Results.TryGetValue("EntityReferenceParameter", out var entityReferenceValue) && entityReferenceValue is EntityReference entityReferenceResult ? entityReferenceResult : default;
        FloatParameter = response.Results.TryGetValue("FloatParameter", out var floatValue) && floatValue is float floatResult ? floatResult : default;
        IntegerParameter = response.Results.TryGetValue("IntegerParameter", out var integerValue) && integerValue is int integerResult ? integerResult : default;
        MoneyParameter = response.Results.TryGetValue("MoneyParameter", out var moneyValue) && moneyValue is Money moneyResult ? moneyResult : default;
        PicklistParameter = response.Results.TryGetValue("PicklistParameter", out var picklistValue) && picklistValue is OptionSetValue picklistResult ? picklistResult : default;
        EnumParameter = response.Results.TryGetValue("EnumParameter", out var enumValue) && enumValue is OptionSetValue enumResult ? (TestEnum)enumResult.Value : default;
        StringParameter = response.Results.TryGetValue("StringParameter", out var stringValue) && stringValue is string stringResult ? stringResult : default;
        StringArrayParameter = response.Results.TryGetValue("StringArrayParameter", out var stringArrayValue) && stringArrayValue is string[] stringArrayResult ? stringArrayResult : [];
        GuidParameter = response.Results.TryGetValue("GuidParameter", out var guidValue) && guidValue is Guid guidResult ? guidResult : default;
    }
}