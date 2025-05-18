using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XrmTools.Meta.Attributes;

namespace XrmGenTest;

[Plugin]
[CustomApi("test_MyCustomApi", "My Custom API", )]
public partial class MyCustomApiPlugin : IPlugin
{
    public void Execute(IServiceProvider serviceProvider)
    {
        throw new InvalidPluginExecutionException("Test", PluginHttpStatusCode.ExpectationFailed);
    }

    [CustomApiRequest]
    public class Request
    {
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
        public XrmTools.Meta.Model.BindingTypes EnumParameter { get; set; }
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
        public EntityCollection EntityCollectionParameter { get; set; }
        public EntityReference EntityReferenceParameter { get; set; }
        public float FloatParameter { get; set; }
        public int IntegerParameter { get; set; }
        public Money MoneyParameter { get; set; }
        public OptionSetValue PicklistParameter { get; set; }
        public XrmTools.Meta.Model.BindingTypes EnumParameter { get; set; }
        public string StringParameter { get; set; }
        public string[] StringArrayParameter { get; set; }
        public Guid GuidParameter { get; set; }
    }
}
