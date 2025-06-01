#nullable enable
namespace XrmTools.Validation;

using System;
using System.ComponentModel.Composition;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using XrmTools.Xrm.Model;

[Export(typeof(IValidator))]
[Validator(Category = Categories.CodeGeneration)]
[method: ImportingConstructor]
internal class CodeGenPluginAssemblyValidator(Logging.Compatibility.ILogger<CodeGenPluginAssemblyValidator> logger) : ValidatorBase<PluginAssemblyConfig>
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