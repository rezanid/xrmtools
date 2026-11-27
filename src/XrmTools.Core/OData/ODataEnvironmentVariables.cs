#nullable enable
namespace XrmTools.OData;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;

internal sealed record ODataEnvironmentProfile(IReadOnlyDictionary<string, string> Variables, string? Error)
{
    public bool ProfileMissing { get; init; }
}

internal static class ODataEnvironmentVariables
{
    private static readonly Regex VariableName = new(@"^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static ODataEnvironmentProfile Parse(string json, string? selectedEnvironment)
    {
        var profiles = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
                return Invalid("The root must be an object of environment profiles.");

            foreach (var profile in document.RootElement.EnumerateObject())
            {
                if (string.IsNullOrWhiteSpace(profile.Name) || profile.Value.ValueKind != JsonValueKind.Object)
                    return Invalid("Each environment profile must be an object of string variables.");
                if (profiles.ContainsKey(profile.Name))
                    return Invalid("The configuration contains ambiguous environment profile names.");

                var profileVariables = new Dictionary<string, string>(StringComparer.Ordinal);
                foreach (var variable in profile.Value.EnumerateObject())
                {
                    if (!VariableName.IsMatch(variable.Name) || profileVariables.ContainsKey(variable.Name))
                        return Invalid("The configuration contains an invalid or duplicate variable name.");
                    if (variable.Value.ValueKind != JsonValueKind.String)
                        return Invalid("Environment variable values must be JSON strings.");
                    profileVariables.Add(variable.Name, variable.Value.GetString() ?? "");
                }
                profiles.Add(profile.Name, profileVariables);
            }
        }
        catch (JsonException)
        {
            return Invalid("The file contains malformed JSON.");
        }

        var selectedVariables = new Dictionary<string, string>(StringComparer.Ordinal);
        bool found = selectedEnvironment != null && profiles.ContainsKey(selectedEnvironment);
        if (found)
            foreach (var variable in profiles[selectedEnvironment!]) selectedVariables.Add(variable.Key, variable.Value);
        return new ODataEnvironmentProfile(selectedVariables, null) { ProfileMissing = selectedEnvironment != null && !found };
    }

    public static ODataEnvironmentProfile Missing()
        => new(new Dictionary<string, string>(StringComparer.Ordinal), null);

    public static ODataEnvironmentProfile Invalid(string error)
        => new(new Dictionary<string, string>(StringComparer.Ordinal), error);
}
