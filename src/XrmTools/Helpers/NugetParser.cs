namespace XrmTools.Helpers;
using System;
using System.IO;
using System.IO.Compression;
using System.Xml.Linq;
using XrmTools.WebApi.Entities;

public static class NugetParser
{
    public static PluginPackage LoadFromNugetFile(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("NuGet package not found.", path);

        byte[] fileBytes = File.ReadAllBytes(path);
        using var memoryStream = new MemoryStream(fileBytes);
        using var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read, leaveOpen: true);

        var nuspecEntry = FindNuspecEntry(archive) ?? throw new InvalidOperationException(
            "The .nuspec file was not found in the NuGet package.");
        using var nuspecStream = nuspecEntry.Open();
        var xml = XDocument.Load(nuspecStream);
        XNamespace ns = xml.Root.GetDefaultNamespace();

        var metadata = xml.Root.Element(ns + "metadata");
        if (metadata == null)
            throw new InvalidOperationException("Missing <metadata> element in .nuspec.");

        var id = metadata.Element(ns + "id")?.Value;
        var version = metadata.Element(ns + "version")?.Value;

        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(version))
            throw new InvalidOperationException("Missing <id> or <version> in .nuspec.");

        return new PluginPackage
        {
            Name = id,
            Version = version,
            Content = Convert.ToBase64String(fileBytes)
        };
    }

    public static bool HasPackagePrefix(string? packageName, string? prefix)
    {
        if (string.IsNullOrWhiteSpace(packageName) || string.IsNullOrWhiteSpace(prefix))
        {
            return false;
        }

        var normalizedPackageName = packageName.Trim();
        var normalizedPrefix = prefix.Trim().TrimEnd('_');

        if (normalizedPrefix.Length == 0)
        {
            return false;
        }

        var expectedPrefix = normalizedPrefix + "_";
        return normalizedPackageName.Length > expectedPrefix.Length
            && normalizedPackageName.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase);
    }

    public static string EnsurePackagePrefix(string packageName, string prefix)
    {
        if (string.IsNullOrWhiteSpace(packageName))
            throw new ArgumentException("Package name is required.", nameof(packageName));

        if (string.IsNullOrWhiteSpace(prefix))
            throw new ArgumentException("Package prefix is required.", nameof(prefix));

        var normalizedPackageName = packageName.Trim();
        var normalizedPrefix = prefix.Trim().TrimEnd('_');

        return HasPackagePrefix(normalizedPackageName, normalizedPrefix)
            ? normalizedPackageName
            : $"{normalizedPrefix}_{normalizedPackageName}";
    }

    private static ZipArchiveEntry FindNuspecEntry(ZipArchive archive)
    {
        foreach (var entry in archive.Entries)
        {
            if (entry.FullName.EndsWith(".nuspec", StringComparison.OrdinalIgnoreCase))
                return entry;
        }

        return null;
    }
}
