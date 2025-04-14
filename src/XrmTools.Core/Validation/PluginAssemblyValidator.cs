#nullable enable
namespace XrmTools.Validation;

using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.Composition;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using XrmTools.Helpers;
using XrmTools.Logging.Compatibility;
using XrmTools.Xrm;
using XrmTools.Xrm.Model;

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
            return new ("Plugin assembly does not have any name.");
        }

        if (!pluginAssembly.PluginAssemblyId.HasValue || pluginAssembly.PluginAssemblyId.Value == Guid.Empty)
        {
            pluginAssembly.PluginAssemblyId = GuidFactory.DeterministicGuid(GuidFactory.Namespace.PluginAssembly, pluginAssembly.Name!);
            Logger.LogTrace($"A deterministic GUID has been asseigned to assembly {pluginAssembly.Name} based on its name.");
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

            if (!plugin.PluginTypeId.HasValue || plugin.PluginTypeId.Value == Guid.Empty)
            {
                plugin.PluginTypeId = GuidFactory.DeterministicGuid(GuidFactory.Namespace.PluginType, plugin.TypeName!);
                Logger.LogTrace($"A deterministic GUID has been asseigned to plugin type {plugin.TypeName} based on its name.");
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
                if (!step.PluginStepId.HasValue || step.PluginStepId.Value == Guid.Empty)
                {
                    step.PluginStepId = GuidFactory.DeterministicGuid(GuidFactory.Namespace.Step, plugin.TypeName + step.Name);
                    Logger.LogTrace($"A deterministic GUID has been asseigned to sdkmessageprocessingstep based on its plugin type and its name.");
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

                    if (!image.PluginStepImageId.HasValue || image.PluginStepImageId.Value == Guid.Empty)
                    {
                        image.PluginStepImageId = GuidFactory.DeterministicGuid(GuidFactory.Namespace.Image, plugin.TypeName + step.Name + image.Name);
                        Logger.LogTrace($"A deterministic GUID has been asseigned to sdkmessageprocessingstepimage based on its plugin type, step and its name.");
                    }
                }
            }

            if (plugin.CustomApi != null)
            {
                if (string.IsNullOrWhiteSpace(plugin.CustomApi.Name))
                {
                    return new("Invalid custom API.");
                }

                if (plugin.CustomApi.Id is null || plugin.CustomApi.Id.Value == Guid.Empty)
                {
                    plugin.CustomApi.Id = GuidFactory.DeterministicGuid(GuidFactory.Namespace.CustomApi, plugin.CustomApi.Name);
                    Logger.LogTrace($"A deterministic GUID has been asseigned to custom API based on its name.");
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
                    if (parameter.Id is null || parameter.Id == Guid.Empty)
                    {
                        parameter.Id = GuidFactory.DeterministicGuid(GuidFactory.Namespace.CustomApiInput, plugin.CustomApi.Name + parameter.Name);
                        Logger.LogTrace($"A deterministic GUID has been asseigned to customapirequestparameter based on its customapi name and  its name.");
                    }
                }

                foreach (var parameter in plugin.CustomApi.ResponseProperties)
                {
                    if (string.IsNullOrWhiteSpace(parameter.Name))
                    {
                        return new("Custom API found with no name!");
                    }
                    if (parameter.Id is null || parameter.Id == Guid.Empty)
                    {
                        parameter.Id = GuidFactory.DeterministicGuid(GuidFactory.Namespace.CustomApiOutput, plugin.CustomApi.Name + parameter.Name);
                        Logger.LogTrace($"A deterministic GUID has been asseigned to customapiresponseproperty based on its customapi name and  its name.");
                    }
                }
            }
        }
        return ValidationResult.Success;
    }
}

[Export(typeof(IValidator))]
[Validator(Category = Categories.CodeGeneration)]
[method: ImportingConstructor]
public class CodeGenPluginAssemblyValidator(Logging.Compatibility.ILogger<CodeGenPluginAssemblyValidator> logger) : ValidatorBase<PluginAssemblyConfig>
{
    private readonly Logging.Compatibility.ILogger<CodeGenPluginAssemblyValidator> Logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public override ValidationResult Validate(PluginAssemblyConfig pluginAssembly)
    {
        if (pluginAssembly == null)
        {
            return new("Invalid plugin assembly, or could not parse the file. Make sure the file is a valid C# file with at least a plugin in correct structure.");
        }

        if ((pluginAssembly.PluginTypes is null || !pluginAssembly.PluginTypes.Any()) && (pluginAssembly.Entities is null || !pluginAssembly.Entities.Any()))
        {
            return new (Core.Resources.Strings.CodeGen_Plugin_NoPluginTypesOrEntities);
        }

        return ValidationResult.Success;
    }
}
#nullable restore