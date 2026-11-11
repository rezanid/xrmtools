#nullable enable
namespace XrmTools.DataverseSolutions;

using Microsoft.VisualStudio.ProjectSystem;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

[Export(typeof(IProjectConfigurationDimensionsProvider))]
[AppliesTo(CdsProjectRegistration.UniqueCapability + " & !.NET")]
[ConfigurationDimensionDescription("Configuration", isVariantDimension: false)]
[ConfigurationDimensionDescription("Platform", isVariantDimension: false)]
internal sealed class CdsProjectConfigurationDimensionsProvider : IProjectConfigurationDimensionsProvider
{
    public Task<IEnumerable<KeyValuePair<string, IEnumerable<string>>>> GetProjectConfigurationDimensionsAsync(UnconfiguredProject project)
        => project.Services.ProjectLockService.ReadLockAsync(async access =>
        {
            var configuredProject = await project.GetSuggestedConfiguredProjectAsync();
            var evaluation = await access.GetProjectAsync(configuredProject);
            return GetMissingDimensions(evaluation.GetPropertyValue, evaluation.ConditionedProperties);
        });

    public Task<IEnumerable<KeyValuePair<string, string>>> GetDefaultValuesForDimensionsAsync(UnconfiguredProject project)
        => project.Services.ProjectLockService.ReadLockAsync(async access =>
        {
            var configuredProject = await project.GetSuggestedConfiguredProjectAsync();
            var evaluation = await access.GetProjectAsync(configuredProject);
            return GetMissingDimensions(evaluation.GetPropertyValue, evaluation.ConditionedProperties).Select(dimension =>
            {
                var current = evaluation.GetPropertyValue(dimension.Key);
                return new KeyValuePair<string, string>(dimension.Key,
                    dimension.Value.FirstOrDefault(value => string.Equals(value, current, StringComparison.OrdinalIgnoreCase))
                    ?? dimension.Value.First());
            }).ToArray().AsEnumerable();
        });

    internal static IEnumerable<KeyValuePair<string, IEnumerable<string>>> GetMissingDimensions(
        Func<string, string> getPropertyValue, IDictionary<string, List<string>> conditionedProperties)
    {
        // Managed CPS handles explicit Configurations/Platforms. PAC projects often declare neither.
        var dimensions = new List<KeyValuePair<string, IEnumerable<string>>>();
        AddMissingDimension("Configuration", "Configurations", new[] { "Debug", "Release" });
        AddMissingDimension("Platform", "Platforms", new[] { "AnyCPU" });
        return dimensions;

        void AddMissingDimension(string name, string propertyName, string[] defaults)
        {
            if (!string.IsNullOrWhiteSpace(getPropertyValue(propertyName))) return;

            var values = new List<string>();
            if (conditionedProperties.TryGetValue(name, out var conditionedValues))
                values.AddRange(conditionedValues.Where(value => !string.IsNullOrWhiteSpace(value)));

            var current = getPropertyValue(name);
            if (!string.IsNullOrWhiteSpace(current)) values.Add(current);
            if (values.Count == 0 || values.All(value => defaults.Contains(value, StringComparer.OrdinalIgnoreCase)))
                values.AddRange(defaults);

            dimensions.Add(new KeyValuePair<string, IEnumerable<string>>(name,
                values.Distinct(StringComparer.OrdinalIgnoreCase).ToArray()));
        }
    }
}
#nullable restore
