namespace XrmGenTestClassic
{
    using Microsoft.Xrm.Sdk;
    using System;
    using System.Collections.Generic;
    using XrmTools.Meta.Attributes;

    [Plugin]
    [CustomApi("test_EchoClassic", DisplayName = "Echo API", Description = "Echos all input parameters to output parameters", BindingType = BindingTypes.Global)]
    public partial class EchoApi : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var logs = new List<string>();
            var context = serviceProvider.GetService(typeof(IPluginExecutionContext)) as IPluginExecutionContext;
            var tracing = serviceProvider.GetService(typeof(ITracingService)) as ITracingService;

            var request = GetRequest(context);
            tracing.Trace("Request has been extracted.");

            string typeOfFloatParameter = context.InputParameters.TryGetValue("FloatParameter", out var floatParameter) ? floatParameter.GetType().Name : "not set";
            logs.Add("FloatParameter type: " + typeOfFloatParameter);
            
            SetResponse(context, new Response
            {
                BooleanParameter = request.BooleanParameter.Value,
                DateTimeParameter = request.DateTimeParameter,
                DecimalParameter = request.DecimalParameter,
                EntityParameter = request.EntityParameter,
                TypedExpandoParameter = request.TypedExpandoParameter,
                ContactParameter = request.ContactParameter,
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
                Logs = logs.ToArray()
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
            public ApiContact ContactParameter { get; set; }
            public TypedExpando TypedExpandoParameter { get; set; }
            public EntityCollection EntityCollectionParameter { get; set; }
            public IEnumerable<ApiContact> CustomEntitiesParameter { get; set; }
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
            public bool BooleanParameter { get; set; }
            public DateTime DateTimeParameter { get; set; }
            public decimal DecimalParameter { get; set; }
            public Entity EntityParameter { get; set; }
            public ApiContact ContactParameter { get; set; }
            public TypedExpando TypedExpandoParameter { get; set; }
            public EntityCollection EntityCollectionParameter { get; set; }
            public IEnumerable<ApiContact> CustomEntitiesParameter { get; set; }
            public EntityReference EntityReferenceParameter { get; set; }
            public double FloatParameter { get; set; }
            public int IntegerParameter { get; set; }
            public Money MoneyParameter { get; set; }
            public OptionSetValue PicklistParameter { get; set; }
            public TestEnum EnumParameter { get; set; }
            public string StringParameter { get; set; }
            public string[] StringArrayParameter { get; set; }
            public Guid GuidParameter { get; set; }
            public string[] Logs { get; set; } = Array.Empty<string>();
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
    
    public class ApiContact : Entity
    {
        public ApiContact() : base("contact") { }
        public ApiContact(Guid id) : base("contact", id) { }
        public string FirstName 
        { 
            get => GetAttributeValue<string>("firstname");
            set => SetAttributeValue("firstname", value);
        }
        public string LastName
        {
            get => GetAttributeValue<string>("lastname");
            set => SetAttributeValue("lastname", value);
        }
    }
    
    public class TypedExpando : Entity
    {
        public TypedExpando() : base() { }
        public TypedExpando(Guid id) : base(null, id) { }
        public string Name 
        { 
            get => GetAttributeValue<string>("name");
            set => SetAttributeValue("name", value);
        }
        public string Description 
        { 
            get => GetAttributeValue<string>("description");
            set => SetAttributeValue("description", value);
        }
    }
}