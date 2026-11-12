#nullable enable
namespace XrmTools.DataverseSolutions;

using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Build;
using System;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

[Export(typeof(IProjectGlobalPropertiesProvider))]
[AppliesTo(CpsProjectCapabilities.NonDotNetProject + " & !.NET")]
internal sealed class CdsProjectGlobalPropertiesProvider : StaticGlobalPropertiesProviderBase
{
    [ImportingConstructor]
    public CdsProjectGlobalPropertiesProvider(UnconfiguredProject project)
        : base(project.Services)
    {
    }

    public override Task<IImmutableDictionary<string, string>> GetGlobalPropertiesAsync(CancellationToken cancellationToken)
        => Task.FromResult(CreateGlobalProperties(
            Path.GetDirectoryName(typeof(Microsoft.Build.Evaluation.Project).Assembly.Location)
                ?? throw new InvalidOperationException("The MSBuild installation directory could not be determined."),
            CultureInfo.CurrentUICulture));

    internal static IImmutableDictionary<string, string> CreateGlobalProperties(string msbuildDirectory, CultureInfo culture)
    {
        // Some MSBuild architecture folders contain targets but not the common CPS schemas.
        var directories = new[] { msbuildDirectory, Path.GetDirectoryName(msbuildDirectory) };
        foreach (var language in new[] { culture.Name, "en-US" })
        {
            if (string.IsNullOrEmpty(language)) continue;
            foreach (var directory in directories)
            {
                if (directory is null) continue;
                var resources = Path.Combine(directory, language);
                if (!File.Exists(Path.Combine(resources, "ProjectItemsSchema.xaml"))) continue;

                return Empty.PropertiesMap
                    .Add("DefineCommonItemSchemas", "true")
                    .Add("DefineCommonCapabilities", "true")
                    .Add("CommonXamlResourcesDirectory", resources + Path.DirectorySeparatorChar);
            }
        }

        throw new FileNotFoundException("The MSBuild installation does not contain the common CPS project schemas.",
            Path.Combine(msbuildDirectory, "en-US", "ProjectItemsSchema.xaml"));
    }
}
#nullable restore
