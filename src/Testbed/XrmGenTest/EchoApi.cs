#nullable enable
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using XrmTools.Meta.Attributes;

namespace XrmGenTest;

[Plugin]
[CustomApi("test_Echo", DisplayName = "Echo API", Description = "Echos all input parameters to output parameters", BindingType = BindingTypes.Global)]
public partial class EchoApi : IPlugin
{
    public void Execute(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetService(typeof(IPluginExecutionContext)) as IPluginExecutionContext;
        var tracing = serviceProvider.GetService(typeof(ITracingService)) as ITracingService;

        var request = GetRequest(context);
        tracing.Trace("Request has been extracted.");

        SetResponse(context, new Response
        {
            BooleanParameter = request.BooleanParameter.Value,
            DateTimeParameter = request.DateTimeParameter,
            DecimalParameter = request.DecimalParameter,
            EntityParameter = request.EntityParameter,
            CustomEntityParameter = request.CustomEntityParameter,
            EntityCollectionParameter = request.EntityCollectionParameter,
            CustomEntitiesParameter = request.CustomEntitiesParameter,
            EntityReferenceParameter = request.EntityReferenceParameter,
            FloatParameter = request.FloatParameter,
            IntegerParameter = request.IntegerParameter,
            MoneyParameter = request.MoneyParameter,
            PicklistParameter = request.PicklistParameter,
            EnumParameter = request.EnumParameter,
            StringParameter = request.StringParameter,
            StringArrayParameter = request.StringArrayParameter,
            GuidParameter = request.GuidParameter,
        });
        tracing.Trace("Response has been set.");
    }

    [CustomApiRequest]
    public class Request
    {
        public bool? BooleanParameter { get; set; }
        public DateTime DateTimeParameter { get; set; }
        public decimal DecimalParameter { get; set; }
        public Entity EntityParameter { get; set; }
        public CustomEntity CustomEntityParameter { get; set; }
        public EntityCollection? EntityCollectionParameter { get; set; }
        public IEnumerable<CustomEntity> CustomEntitiesParameter { get; set; }
        public EntityReference EntityReferenceParameter { get; set; }
        public double FloatParameter { get; set; }
        public int IntegerParameter { get; set; }
        public Money MoneyParameter { get; set; }
        public OptionSetValue PicklistParameter { get; set; }
        public TestEnum EnumParameter { get; set; }
        [CustomApiRequestParameter(IsOptional = true)]
        public string StringParameter { get; set; }
        public string[] StringArrayParameter { get; set; }
        public Guid GuidParameter { get; set; }
    }

    [CustomApiResponse]
    public class Response
    {
        public required bool BooleanParameter { get; set; }
        public required DateTime DateTimeParameter { get; set; }
        public required decimal DecimalParameter { get; set; }
        public required Entity EntityParameter { get; set; }
        public required CustomEntity CustomEntityParameter { get; set; }
        public required EntityCollection EntityCollectionParameter { get; set; }
        public required IEnumerable<CustomEntity> CustomEntitiesParameter { get; set; }
        public required EntityReference EntityReferenceParameter { get; set; }
        public required double FloatParameter { get; set; }
        public required int IntegerParameter { get; set; }
        public required Money MoneyParameter { get; set; }
        public required OptionSetValue PicklistParameter { get; set; }
        public required TestEnum EnumParameter { get; set; }
        public required string StringParameter { get; set; }
        public required string[] StringArrayParameter { get; set; }
        public required Guid GuidParameter { get; set; }
    }
}

public class CustomEntity : Entity
{
    public CustomEntity() : base("custom_entity") { }
    public CustomEntity(Guid id) : base("custom_entity", id) { }
    public string CustomField
    {
        get => GetAttributeValue<string>("custom_field");
        set => SetAttributeValue("custom_field", value);
    }
}

public enum TestEnum
{
    Option1 = 1,
    Option2 = 2,
    Option3 = 3,
    Option4 = 4,
    Option5 = 5,
    Option6 = 6,
    Option7 = 7,
    Option8 = 8,
    Option9 = 9,
    Option10 = 10
}