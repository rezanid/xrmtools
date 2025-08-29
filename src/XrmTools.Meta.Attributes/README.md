XrmTools.Meta.Attributes brings source only attributes to decorate your Dataverse plugins, custom APIs, and typed entities. There are also contains interfaces like `ITypedOrganizationRequest`, `ITypedOrganizationResponse` and `TResponse ExecuteTyped<TRequest, TResponse>(this IOrganizationService service)` to make it very easy to call Custom API and actions. This package is part of the [Xrm Tools](https://marketplace.visualstudio.com/items?itemName=rezanid.XrmTools) extension for Visual Studio, which provides a set of tools to enhance your development experience with Microsoft Dataverse (formerly known as Common Data Service or Dynamics 365). Although the extension is not required to use this package, it provides a convenient way to manage your Dataverse projects and generate code based on your schema.

If you aren't already using [Xrm Tools](https://marketplace.visualstudio.com/items?itemName=rezanid.XrmTools) for Power Platform development, I suggest checking out [Xrm Tools Wiki](https://github.com/rezanid/xrmtools/wiki) to learn how modern way of Power Platform development can enhance your experience. In short, the extension provides the code generation capabilities for your plugins and custom APIs. By using these attributes, you can easily register your plugins and custom APIs without writing boilerplate code, and you can also generate typed entities, typed requests and responses for your APIs and actions based on your Dataverse schema and keep them up-to-date without ever leaving Visual Studio.

## Attributes

By installing this nuget packge, you will be able to use attributes to decorate your Dataverse plugins with metadata to enable code generation and automatic registration. Supported attributes are:
- `PluginRegistrationAttribute`: Used to decorate a class that implements `IPlugin` to specify the plugin registration details. This attribute should be the first registration attribute on the class.
- `PluginStepAttribute`: Used to decorate a class that implements `IPlugin` to specify the step registration details. This attribute comes after the `PluginRegistrationAttribute` and can be used multiple times to register multiple steps for the same plugin.
- `PluginImageAttribute`: Used to decorate a class that implements `IPlugin` to specify the image registration details. This attribute comes after the `PluginStepAttribute` and can be used multiple times to register multiple images for the same step.
- `CustomApiAttribute`: Used to decorate a clas that implements `IPlugin` to specify the custom API registration details. This attribute comes after the `PluginRegistrattionAttribute` and can be used only once to register a custom API for the same plugin.
- `CustomApiRequestAttribute`: Used to decorate a class, **nested** inside the calss that implements `IPlugin` to specify the custom API request parameters. By applying this attribute, all properties of the class will become request parameters for your custom API. This attribute can only be applied to a single class within the plugin class.
- `CustomApiResponseAttribute`: Used to decorate a class, **nested** inside the calss that implements `IPlugin` to specify the custom API response properties. By applying this attribute, all properties of the class will become response properties for your custom API. This attribute can only be applied to a single class within the plugin class.
- `CustomApiRequestParameter`: Used to decorate a property of the class that is marked with `CustomApiRequestAttribute` to specify the custom API request parameter details. This attribute can be used to set the parameter name, description, and other details.
- `CustomApiResponseProperty`: Used to decorate a property of the class that is marked with `CustomApiResponseAttribute` to specify the custom API response property details. This attribute can be used to set the parameter name, description, and other details.
- `DependencyAttribute`: Decorate a property as a dependency so that code generator can generate code to inject this dependency.
- `DependencyConstructor`: Decorate a constructor method as depdency so that code generator can generate code to inject this dependency.
- `EntityAttribute`: Assembly scoped attribute that instructs the code generator to generate a typed entity with the given attributes.
- `CodeGenReplacePrefixesAttribute`: Assembly scoped attribute that instructs the code generator to replace the prefixes of the entity names with the given values. This is useful when you want to use a different prefix for your entities in the generated code.
- `CodeGenGlobalOptionSetAttribute`: Assembly scoped attribute that lets the developer decide if global option sets should be generated in the GlobalOptionSets.cs file or as enums in the typed entity files. By default, global option sets are generated locally.

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
        // Marking the type as nullable will make the parameter optional in the Custom API.
        public decimal? DecimalParameter { get; set; }
        public Entity EntityParameter { get; set; }
        public Contact ContactParameter { get; set; } // This is a typed entity, you can use any entity that is registered in your Dataverse environment.
        public IEnumerable<Contact> ContactsParameter { get; set; } // This will be converted to an EntityCollection in the Custom API request.
        public EntityCollection EntityCollectionParameter { get; set; }
        public EntityReference EntityReferenceParameter { get; set; }
        // Null annotation makes the parameter optional.
        public float? FloatParameter { get; set; }
        public int IntegerParameter { get; set; }
        // You can manually set the parameter metadata like name, description.
        [CustomApiRequestParameter(UniqueName = "money_parameter", IsOptional = true, DisplayName = "Money Parameter", Description = "Money parameter description")]]
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

### Example 4: Two typed entities.
```csharp
// Entities.cs

[assembly:Entity("contact", "firstname,lastname")]
[assembly:Entity("account", "name") ]

```

To learn more about Xrm Tools extension for Visual Studio, please refer to:
* [GitHub repository](https://github.com/rezanid/xrmtools)
* [Official documentation](https://rezanid.github.io/xrmtools/).