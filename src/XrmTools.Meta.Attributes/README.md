# Power Platform Tools - Attributes

By installing this nuget packge, you will be able to use attributes to decorate your Dataverse plugins with metadata to enable code generation and automatic registration. Supported attributes are:
- `PluginRegistrationAttribute`: Used to decorate a class that implements `IPlugin` to specify the plugin registration details. This attribute should be the first registration attribute on the class.
- `PluginStepAttribute`: Used to decorate a class that implements `IPlugin` to specify the step registration details. This attribute comes after the `PluginRegistrationAttribute` and can be used multiple times to register multiple steps for the same plugin.
- `PluginImageAttribute`: Used to decorate a class that implements `IPlugin` to specify the image registration details. This attribute comes after the `PluginStepAttribute` and can be used multiple times to register multiple images for the same step.
- `CustomApiAttribute`: Used to decorate a clas that implements `IPlugin` to specify the custom API registration details. This attribute comes after the `PluginRegistrattionAttribute` and can be used only once to register a custom API for the same plugin.
- `CustomApiRequestAttribute`: Used to decorate a class, INSIDE the calss that implements `IPlugin` to specify the custom API request parameters. By applying this attribute, all properties of the class will become request parameters for your custom API. This attribute can only be applied to a single class within the plugin class.
- `CustomApiResponseAttribute`: Used to decorate a class, INSIDE the calss that implements `IPlugin` to specify the custom API response properties. By applying this attribute, all properties of the class will become response properties for your custom API. This attribute can only be applied to a single class within the plugin class.

Let's look at some examples:

## Example 1: Plugin Registration
```csharp
using Microsoft.Xrm.Sdk;
using System;
using XrmTools.Meta.Attributes;
using XrmTools.Meta.Model;

namespace XrmGenTest;

[Plugin]
[Step("Create", "contact", "firstname,lastname", Stages.PostOperation, ExecutionMode.Synchronous)]
[Image(ImageTypes.PostImage, "firstname,lastname")]
public partial class ContactCreatePlugin : IPlugin
{
    public void Execute(IServiceProvider serviceProvider)
    {
        if (serviceProvider == null)
        {
            throw new InvalidPluginExecutionException(nameof(serviceProvider));
        }
        Initialize(serviceProvider);
    }
}
```

## Example 2: Plugin Registration with a PluginBase class
```csharp
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using System;
using XrmTools.Meta.Attributes;
using XrmTools.Meta.Model;

namespace XrmGenTest;

public abstract class PluginBase : IPlugin
{
    public void Execute(IServiceProvider serviceProvider)
    {
        if (serviceProvider == null)
        {
            throw new InvalidPluginExecutionException(nameof(serviceProvider));
        }
        var executionContext = serviceProvider.Get<IPluginExecutionContext7>();
        var organizationService = serviceProvider.GetOrganizationService(executionContext.UserId);
        var tracing = serviceProvider.Get<ITracingService>();
        Initialize(serviceProvider);
    }

    internal virtual void Initialize(IServiceProvider serviceProvider) { }
}

[Plugin]
[Step("Create", "account", "accountnumber,accountcategorycode,accountclassificationcode", Stages.PostOperation, ExecutionMode.Synchronous)]
[Image(ImageTypes.PostImage, "accountnumber")]
public partial class AccountCreatePlugin : PluginBase, IPlugin
{
    public void ExecuteLocal(IServiceProvider serviceProvider)
    {
        // It's just business logic!
    }
}
```

## Example 3: Custom API Registration
```csharp
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XrmTools.Meta.Attributes;

namespace XrmGenTest;

[Plugin]
[CustomApi("test_MyCustomApi", "My Custom API", "MyCustomApi")]
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
```

To learn more about Xrm Tools extension for Visual Studio, please refer to:
* [GitHub repository](https://github.com/rezanid/xrmtools)
* [Official documentation](https://rezanid.github.io/xrmtools/).