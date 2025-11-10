XrmTools.Meta.Attributes brings source only attributes to decorate your Dataverse plugins, custom APIs, and typed entities. There are also contains interfaces like `ITypedOrganizationRequest`, `ITypedOrganizationResponse` and `TResponse ExecuteTyped<TRequest, TResponse>(this IOrganizationService service)` to make it very easy to call Custom API and actions. This package is part of the [Xrm Tools](https://marketplace.visualstudio.com/items?itemName=rezanid.XrmTools) extension for Visual Studio, which provides a set of tools to enhance your development experience with Microsoft Dataverse (formerly known as Common Data Service or Dynamics 365). Although the extension is not required to use this package, it provides a convenient way to manage your Dataverse projects and generate code based on your schema.

If you aren't already using [Xrm Tools](https://marketplace.visualstudio.com/items?itemName=rezanid.XrmTools) for Power Platform development, I suggest checking out [Xrm Tools Wiki](https://github.com/rezanid/xrmtools/wiki) to learn how modern way of Power Platform development can enhance your experience. In short, the extension provides the code generation capabilities for your plugins and custom APIs. By using these attributes, you can easily register your plugins and custom APIs without writing boilerplate code, and you can also generate typed entities, typed requests and responses for your APIs and actions based on your Dataverse schema and keep them up-to-date without ever leaving Visual Studio.

## Attributes

By installing this nuget packge, you will be able to use attributes to decorate your Dataverse plugins with metadata to enable code generation and automatic registration. Supported attributes are:
- `PluginAttribute`: Used to decorate a class that implements `IPlugin` to specify the plugin registration details. This attribute should be **the first registration attribute** on the class.
- `StepAttribute`: Used to decorate a class that implements `IPlugin` to specify the step registration details. This attribute comes after the `PluginRegistrationAttribute` and can be used multiple times to register multiple steps for the same plugin.
- `ImageAttribute`: Used to decorate a class that implements `IPlugin` to specify the image registration details. This attribute comes after the `PluginStepAttribute` and can be used multiple times to register multiple images for the same step.

### Custom API
A plugin that provides a Custom API in Power Platform can benefit from the following attributes. Use the following attributes to make your API more readable and write a lot less code. Read more in [Xrm Tools Wiki - Writing a Custom API](https://github.com/rezanid/xrmtools/wiki/Writing-a-Custom-API)
- `CustomApiAttribute`: Used to decorate a class that implements `IPlugin` to specify the custom API registration details. This attribute comes after the `PluginRegistrattionAttribute` and can be used only once on a class to register a custom API for the plugin.
- `CustomApiRequestAttribute`: Used to decorate a class, **nested** inside the calss that implements `IPlugin` to specify the custom API request parameters. By applying this attribute, all properties of the class will become request parameters for your custom API. This attribute can only be applied to a single class within the plugin class.
- `CustomApiResponseAttribute`: Used to decorate a class, **nested** inside the calss that implements `IPlugin` to specify the custom API response properties. By applying this attribute, all properties of the class will become response properties for your custom API. This attribute can only be applied to a single class within the plugin class.
- `CustomApiRequestParameter`: Used to optionally decorate a property of the class that is marked with `CustomApiRequestAttribute` to specify the custom API request parameter details. This attribute can be used to set the parameter name, description, and other details.
- `CustomApiResponseProperty`: Used to optionally decorate a property of the class that is marked with `CustomApiResponseAttribute` to specify the custom API response property details. This attribute can be used to set the parameter name, description, and other details.

### Dependency Injection
By adding the following attributes to properties of your class you can opt into a simple compile-time dependency injection. Read more in [Xrm Tools Wiki - Dependency Injection](https://github.com/rezanid/xrmtools/wiki/Dependency-Injection)
- `DependencyAttribute`: Decorate a property as a dependency so that code generator can generate code to inject this dependency.
- `DependencyConstructor`: Decorate a constructor method as the constructor to be used by the dependency injection. It's useful when you have more than one constructor.
- `DependencyProvider`: Used for when you need your own custom logic to initialize a depdency. Just add the attribute to a property in your plugin write set your depdency like the following example.

```csharp
    [DependencyProvider("User")]
    protected IOrganizationService UserOrgService
    {
        get => TryGet<IOrganizationService>("User", out var service)
            ? service
            : Set("User", OrgServiceFactory.CreateOrganizationService(Context.UserId));
    }
```

### Early-bound (typed) Entities
It's a lot easier to add early-bound (typed) entities to your project. Just add the following attribute to your `AssemblyInfo.cs` or any global `.cs` file and save. Read more in [Xrm Tools Wiki - Typed Entities](https://github.com/rezanid/xrmtools/wiki/Typed-Entities)
- `EntityAttribute`: Assembly scoped attribute that instructs the code generator to generate a typed entity with the given attributes.


### Code Generation Settings
You can fine-tune code generation in Xrm Tools by adding the following global attributes to your `Assemblyinfo.cs` or any global `.cs` file.
- `CodeGenReplacePrefixesAttribute`: Assembly scoped attribute that instructs the code generator to replace or remove the prefixes of the entity names with the given values. This is useful when you want to use a different prefix for your entities or remove prefixes in the generated code.
- `CodeGenGlobalOptionSetAttribute`: Assembly scoped attribute that lets the developer decide if global option sets should be generated in the GlobalOptionSets.cs file or as enums in the typed entity files. By default, global option sets are generated locally.

### Other Global Attributes
- `PluginAssemblyAttribute`: You can optionally customize the assembly level registration by adding this attribute to your `AssemblyInfor.cs` or any other global `.cs` file.
- `SolutionAttribute`: You can optionally include all your plugins in your designated Power Platform solution. Your solution need to already exist.

An example `AssemblyInfo.cs` file with these optional attributes.

```csharp
using Microsoft.Xrm.Sdk.Client;
using System.Reflection;
using System.Runtime.InteropServices;
using XrmTools.Meta.Attributes;

[assembly: AssemblyTitle("XrmGenTest")]
// Other assembly attributes removed for brevity.

// The following is useful for when you have early-bound (typed) entities. It let's 
// Dataverse recognize your custom entity types.
[assembly: ProxyTypesAssembly]
// The following attribute is just using the default values.
[assembly: PluginAssembly]
// The following attribute is declaring which Power Platform solution will contain your plugins.
[assembly: Solution("Test")]
// The following attribute is telling Xrm Tools to generate global optionsets in a dedicated "GlobalOptionSets.cs" file.
[assembly: CodeGenGlobalOptionSet(GlobalOptionSetGenerationMode.GlobalOptionSetFile)]
```

Let's look at some  more examples:

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
        // ... rest of the code.
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

    internal abstract void ExecuteInternal(IServiceProvider serviceProvider) { }
}

[Plugin]
[Step("Create", "account", "accountnumber,accountcategorycode,accountclassificationcode", Stages.PostOperation, ExecutionMode.Synchronous)]
[Image(ImageTypes.PostImage, "accountnumber")]
public partial class AccountCreatePlugin : PluginBase, IPlugin
{
    internal override void ExecuteLocal(IServiceProvider serviceProvider)
    {
        //... rest of the logic
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