#nullable enable
namespace XrmTools.Helpers;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using System.IO;
using System;
using System.Linq;

public static class ProjectExtensions
{
    public static class BuildProperties
    {
        public const string OutputPath = "OutputPath";
        public const string AssemblyName = "AssemblyName";
        public const string OutputType = "OutputType";
        public const string GeneratePackageOnBuild = "GeneratePackageOnBuild";
        public const string PackageId = "PackageId";
        public const string PackageVersion = "PackageVersion";
        public const string PackageOutputPath = "PackageOutputPath";
        public const string VersionPrefix = "VersionPrefix";
    }

    public static bool IsSdkStyle(this Project project)
        => project.IsCapabilityMatch("OpenProjectFile");
    // Other capabilities that are also true for SDK-Style projects: ProjectReferences, PackageReferences

    //public static bool IsSdkStyle(this Project project)
    //{

    //    project.GetItemInfo(out var hierarchy, out _, out _);
    //    return hierarchy.IsSdkStyleProject();
    //}

    public static IVsBuildPropertyStorage? ToBuildPropertyStorage(this Project project)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        project.GetItemInfo(out var hierarchy, out _, out _);
        return hierarchy is IVsBuildPropertyStorage buildPropertyStorage ? buildPropertyStorage : null;
    }

    public static string? GetBuildProperty(this Project project, string name)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        var buildPropertyStorage = project.ToBuildPropertyStorage();
        if (buildPropertyStorage is null) return null;
        if (buildPropertyStorage.GetPropertyValue(name, null, (uint)_PersistStorageType.PST_PROJECT_FILE, out var value) == VSConstants.S_OK)
        {
            return value;
        }
        return null;
    }

    public static T? GetBuildProperty<T>(this Project project, string name) where T : IConvertible
        => GetBuildProperty(project, name) is string value ? (T)Convert.ChangeType(value, typeof(T)) : default;

    public static string? GetOutputAssemblyPath(this Project project)
    {
        var storage = project.ToBuildPropertyStorage();
        if (storage is null) return null;
        var outputType = GetBuildProperty(storage, BuildProperties.OutputType);
        var outputPath = GetBuildProperty(storage, BuildProperties.OutputPath);
        var assemblyName = GetBuildProperty(storage, BuildProperties.AssemblyName);
        return Path.Combine(Path.Combine(Path.GetDirectoryName(project.FullPath), outputPath), assemblyName + (outputType == "Library" ? ".dll" : ".exe"));
    }

    public static string? GetOutputPackagePath(this Project project)
    {
        var storage = project.ToBuildPropertyStorage();
        if (storage is null) return null;
        var packageOutputDir = GetBuildProperty(storage, BuildProperties.PackageOutputPath);
        var packageName = GetBuildProperty(storage, BuildProperties.PackageId);
        var packageVersion = GetBuildProperty(storage, BuildProperties.PackageVersion).TrimSuffix(".0");
        return Path.Combine(Path.GetDirectoryName(project.FullPath), packageOutputDir, $"{packageName}.{packageVersion}.nupkg");
    }

    private static string? GetBuildProperty(IVsBuildPropertyStorage storage, string name)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        return storage?.GetPropertyValue(name, null, (uint)_PersistStorageType.PST_PROJECT_FILE, out var value) == VSConstants.S_OK ? value : null;
    }
}
