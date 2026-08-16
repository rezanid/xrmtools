using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using XrmTools.Meta.Attributes;

namespace $rootnamespace$
{
    /// <summary>
    /// This is a sample Custom API with registration attributes (i.e. [Plugin], [CustomApi]) and code generation. Learn
    /// more about writing [Power Platform Custom APIs using Xrm Tools](https://github.com/rezanid/xrmtools/wiki/Writing-a-Custom-API)
    /// If you see errors in the code, just save this file (Ctrl+S) to trigger code generation - all the errors will disappear.
    /// </summary>
    [Plugin]
    [CustomApi("test_Echo", DisplayName = "Echo API", Description = "Echos all input parameters to output parameters", BindingType = BindingTypes.Global)]
    public partial class $safeitemname$ : IPlugin
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

        /// <summary>
        /// All the properties of this class will become input parameters of the Custom API. The parameter types can be
        /// primitive types, enums, or any of the supported Dataverse types (e.g. Entity, EntityReference, Money, OptionSetValue).
        /// </summary>
        [CustomApiRequest]
        public class Request
        {
            public bool? BooleanParameter { get; set; }
            public DateTime DateTimeParameter { get; set; }
            public decimal DecimalParameter { get; set; }
            public Entity EntityParameter { get; set; }
            public CustomEntity CustomEntityParameter { get; set; }
            // If you enabled Nullable Reference Types for the project, you can make parameters
            // nullable (e.g. Entity?) and they will become optional parameters in the Custom API.
            // If not, you can use the [CustomApiRequestParameter(IsOptional = true)] attribute to
            // make parameters optional.
            [CustomApiRequestParameter(IsOptional = true)]
            public EntityCollection EntityCollectionParameter { get; set; }
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

        /// <summary>
        /// All the properties of this class will become output properties of the Custom API. The parameter types can
        /// be primitive types, enums, or any of the supported Dataverse types (e.g. Entity, EntityReference, Money, OptionSetValue).
        /// </summary>
        [CustomApiResponse]
        public class Response
        {
            public bool BooleanParameter { get; set; }
            public DateTime DateTimeParameter { get; set; }
            public decimal DecimalParameter { get; set; }
            public Entity EntityParameter { get; set; }
            public CustomEntity CustomEntityParameter { get; set; }
            public EntityCollection EntityCollectionParameter { get; set; }
            public IEnumerable<CustomEntity> CustomEntitiesParameter { get; set; }
            public EntityReference EntityReferenceParameter { get; set; }
            public double FloatParameter { get; set; }
            public int IntegerParameter { get; set; }
            public Money MoneyParameter { get; set; }
            public OptionSetValue PicklistParameter { get; set; }
            public TestEnum EnumParameter { get; set; }
            public string StringParameter { get; set; }
            public string[] StringArrayParameter { get; set; }
            public Guid GuidParameter { get; set; }
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
}