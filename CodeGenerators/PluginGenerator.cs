using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using System.Text;
using System.Text.Json;
using XrmGen.Xrm.Generators;
using XrmGen.Xrm.Model;

namespace XrmGen;

public class PluginGenerator : BaseCodeGeneratorWithSite
{
    public const string Name = "XrmGen Plugin Generator";
    public const string Description = "Generates plugin classes from .dej.json file.";

    public override string GetDefaultExtension() => ".cs";

    protected override byte[] GenerateCode(string inputFileName, string inputFileContent)
    {
        var config = JsonSerializer.Deserialize<PluginAssembly>(inputFileContent);
        if (config?.PluginTypes is null) { return null; }
        var generator = new TemplatedPluginGenerator();
        var (isValid, validationMessage) = generator.IsValid(config);
        if (!isValid)
        {
            return Encoding.UTF8.GetBytes(validationMessage);
        }
        return Encoding.UTF8.GetBytes(generator.GenerateCode(config, GetDefaultNamespace()).ToString());
    }

    private string GetDefaultNamespace()
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        if (GetService(typeof(IVsHierarchy)) is IVsHierarchy hierarchy)
        {
            // Get the current item ID
            hierarchy.ParseCanonicalName(InputFilePath, out var itemId);

            if (hierarchy.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_DefaultNamespace, out object defaultNamespace) == VSConstants.S_OK)
            {
                return defaultNamespace as string;
            }
        }

        return null;
    }
}