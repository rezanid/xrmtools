#nullable enable
namespace XrmTools.OData;

using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;

internal enum ODataEnvironmentFileUpdateKind
{
    Updated,
    AlreadyPresent,
    Conflict,
    Failed
}

internal readonly record struct ODataEnvironmentFileUpdate(ODataEnvironmentFileUpdateKind Kind, string? Path, string? Error);

internal static class ODataEnvironmentFile
{
    private const string FileName = "odata-client.env.json";
    private static readonly JsonSerializerOptions Formatting = new() { WriteIndented = true };

    public static string? FindNearest(string documentPath)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(documentPath));
        while (!string.IsNullOrEmpty(directory))
        {
            string candidate = Path.Combine(directory, FileName);
            if (File.Exists(candidate)) return candidate;
            directory = Directory.GetParent(directory)?.FullName;
        }
        return null;
    }

    public static string[] GetSearchDirectories(string documentPath)
    {
        var directories = new System.Collections.Generic.List<string>();
        var directory = Path.GetDirectoryName(Path.GetFullPath(documentPath));
        while (!string.IsNullOrEmpty(directory))
        {
            directories.Add(directory);
            directory = Directory.GetParent(directory)?.FullName;
        }
        return directories.ToArray();
    }

    public static ODataEnvironmentFileUpdate MoveVariable(string documentPath, string profileName, string variableName,
        string value, CancellationToken cancellationToken = default)
    {
        string? temporary = null;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            string? existingPath = FindNearest(documentPath);
            string targetPath = existingPath ?? Path.Combine(Path.GetDirectoryName(Path.GetFullPath(documentPath))!, FileName);
            bool existed = existingPath != null;
            string original = existed ? File.ReadAllText(targetPath) : "{}";
            var parsed = ODataEnvironmentVariables.Parse(original, profileName);
            if (parsed.Error != null) return new(ODataEnvironmentFileUpdateKind.Failed, targetPath, parsed.Error);

            var root = JsonNode.Parse(original) as JsonObject;
            if (root == null) return new(ODataEnvironmentFileUpdateKind.Failed, targetPath, "The configuration root must be an object.");
            var profileKey = root.Select(pair => pair.Key)
                .SingleOrDefault(key => string.Equals(key, profileName, StringComparison.OrdinalIgnoreCase));
            JsonObject profile;
            if (profileKey == null)
            {
                profileKey = profileName;
                profile = new JsonObject();
                root.Add(profileKey, profile);
            }
            else if (root[profileKey] is JsonObject existingProfile)
            {
                profile = existingProfile;
            }
            else
            {
                return new(ODataEnvironmentFileUpdateKind.Failed, targetPath, "The selected environment profile must be an object.");
            }

            if (profile.TryGetPropertyValue(variableName, out var currentNode))
            {
                if (currentNode?.GetValue<string>() != value)
                    return new(ODataEnvironmentFileUpdateKind.Conflict, targetPath, "The selected profile already defines this variable with a different value.");
                return new(ODataEnvironmentFileUpdateKind.AlreadyPresent, targetPath, null);
            }

            profile.Add(variableName, value);
            string updated = root.ToJsonString(Formatting);
            cancellationToken.ThrowIfCancellationRequested();
            var parent = Path.GetDirectoryName(targetPath)!;
            Directory.CreateDirectory(parent);
            temporary = Path.Combine(parent, ".odata-client.env." + Guid.NewGuid().ToString("N") + ".tmp");
            File.WriteAllText(temporary, updated, new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            cancellationToken.ThrowIfCancellationRequested();

            if (!string.Equals(FindNearest(documentPath), existed ? targetPath : null, StringComparison.OrdinalIgnoreCase))
                return new(ODataEnvironmentFileUpdateKind.Failed, targetPath, "The nearest environment file changed concurrently. Review it and try again.");

            if (existed)
            {
                if (!File.Exists(targetPath) || !string.Equals(File.ReadAllText(targetPath), original, StringComparison.Ordinal))
                    return new(ODataEnvironmentFileUpdateKind.Failed, targetPath, "The environment file changed concurrently. Review it and try again.");
                File.Replace(temporary, targetPath, null);
            }
            else
            {
                if (File.Exists(targetPath))
                    return new(ODataEnvironmentFileUpdateKind.Failed, targetPath, "An environment file appeared concurrently. Review it and try again.");
                File.Move(temporary, targetPath);
            }
            temporary = null;
            return new(ODataEnvironmentFileUpdateKind.Updated, targetPath, null);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Security.SecurityException or JsonException or InvalidOperationException or ArgumentException)
        {
            return new(ODataEnvironmentFileUpdateKind.Failed, null, "Could not update the environment file. " + ex.Message);
        }
        finally
        {
            if (temporary != null)
            {
                try { File.Delete(temporary); }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
        }
    }
}
