#nullable enable
namespace XrmTools.Validation;

using System;
using System.ComponentModel.Composition;
using System.ComponentModel.DataAnnotations;
using XrmTools.Meta.Model.Configuration;

[Export(typeof(IValidator))]
[Validator(Category = Categories.WebApi)]
[method:ImportingConstructor]
internal class WebApiPluginAssemblyValidator(
    Logging.Compatibility.ILogger<WebApiPluginAssemblyValidator> logger) : ValidatorBase<PluginAssemblyConfig>
{
    private readonly Logging.Compatibility.ILogger<WebApiPluginAssemblyValidator> Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    
    public override ValidationResult Validate(PluginAssemblyConfig pluginAssembly)
    {
        if (pluginAssembly == null)
        {
            return new ("Invalid plugin assembly, or could not parse the file. Make sure the file is a valid C# file with at least a plugin in correct structure.");
        }
        if (string.IsNullOrWhiteSpace(pluginAssembly.Name))
        {
            return new ("Plugin assembly does not have a name.");
        }

        if (pluginAssembly.Package is WebApi.Entities.PluginPackage package)
        {
            if (string.IsNullOrWhiteSpace(package.Name))
            {
                return new("Plugin package does not have a name.");
            }
        }

        foreach (var plugin in pluginAssembly.PluginTypes)
        {
            if (plugin is null)
            {
                return new("Could not parse plugin type.");
            }
            if (string.IsNullOrWhiteSpace(plugin.TypeName))
            {
                return new ("Invalid plugin type.");
            }

            foreach (var step in plugin.Steps)
            {
                if (step is null)
                {
                    return new("Invalid plugin step.");
                }
                if (string.IsNullOrWhiteSpace(step.Name))
                {
                    return new("Could not parse plugin step name.");
                }
                foreach (var image in step.Images)
                {
                    if (image is null)
                    {
                        return new("Invalid plugin image.");
                    }
                    if (string.IsNullOrWhiteSpace(image.Name))
                    {
                        return new("Invalid plugin image.");
                    }
                    if (image.ImageType == null)
                    {
                        return new($"Image processing step {image.Name} has not ImageType.");
                    }
                }
            }

            if (plugin.CustomApi != null)
            {
                if (string.IsNullOrWhiteSpace(plugin.CustomApi.UniqueName))
                {
                    return new("Invalid custom API. The Custom API is missing UniqueName.");
                }

                if (string.IsNullOrWhiteSpace(plugin.CustomApi.Name))
                {
                    plugin.CustomApi.Name = plugin.CustomApi.UniqueName;
                }

                if (plugin.CustomApi.Description is null)
                {
                    plugin.CustomApi.Description = plugin.CustomApi.DisplayName ?? plugin.CustomApi.Name;
                }

                foreach (var parameter in plugin.CustomApi.RequestParameters)
                {
                    if (string.IsNullOrWhiteSpace(parameter.Name))
                    {
                        return new("Custom API found with no name!");
                    }
                }

                foreach (var parameter in plugin.CustomApi.ResponseProperties)
                {
                    if (string.IsNullOrWhiteSpace(parameter.Name))
                    {
                        return new("Custom API found with no name!");
                    }
                }
            }
        }
        return ValidationResult.Success;
    }
}
#nullable restore