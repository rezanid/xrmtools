#nullable enable
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using XrmTools.Meta.Attributes;

namespace XrmGenTest;

[Plugin]
[CustomApi("test_Echo", Name = "EchoAPI", DisplayName = "Echo API", Description = "Echos all input parameters to output parameters")]
public partial class EchoApi : IPlugin
{
    public void Execute(IServiceProvider serviceProvider)
    {
        var context = (serviceProvider.GetService(typeof(IPluginExecutionContext)) as IPluginExecutionContext)!;
        var tracing = (serviceProvider.GetService(typeof(ITracingService)) as ITracingService)!;

        var request = GetRequest(context);
        tracing.Trace("Request has been extracted.");

        SetResponse(context, new Response
        {
            BooleanParameter = request.BooleanParameter ?? false,
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
        public decimal DecimalParameter { get; set; }
        public int IntegerParameter { get; set; }
        public string? StringParameter { get; set; }
        public EntityCollection? EntityCollectionParameter { get; set; }
        public EntityCollection CustomEntitiesParameter { get; set; }
        public OptionSetValue EnumParameter { get; set; }
        public Entity EntityParameter { get; set; }
        public OptionSetValue PicklistParameter { get; set; }
        public EntityReference EntityReferenceParameter { get; set; }
        public DateTime DateTimeParameter { get; set; }
        public bool? BooleanParameter { get; set; }
        public double FloatParameter { get; set; }
        public string[] StringArrayParameter { get; set; }
        public Guid GuidParameter { get; set; }
        public Entity CustomEntityParameter { get; set; }
        public Money MoneyParameter { get; set; }
    }

    [CustomApiResponse]
    public class Response
    {
        public OptionSetValue? PicklistParameter { get; set; }
        public double? FloatParameter { get; set; }
        public string? StringParameter { get; set; }
        public Money? MoneyParameter { get; set; }
        public Entity? EntityParameter { get; set; }
        public string[]? StringArrayParameter { get; set; }
        public bool? BooleanParameter { get; set; }
        public DateTime? DateTimeParameter { get; set; }
        public Entity? CustomEntityParameter { get; set; }
        public OptionSetValue? EnumParameter { get; set; }
        public Guid? GuidParameter { get; set; }
        public EntityCollection? CustomEntitiesParameter { get; set; }
        public EntityCollection? EntityCollectionParameter { get; set; }
        public EntityReference? EntityReferenceParameter { get; set; }
        public int? IntegerParameter { get; set; }
        public decimal? DecimalParameter { get; set; }
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