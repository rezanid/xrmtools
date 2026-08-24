#nullable enable
namespace XrmTools.Helpers;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.ServiceBroker;
using NuGet.VisualStudio.Contracts;
using System.IO;
using System;
using System.Linq;
using System.Threading.Tasks;

public static class ProjectExtensions
{
    public static class BuildProperties
    {
        public const string IsXrmToolsPlugin = "IsXrmToolsPlugin";
        public const string GeneratePackageOnBuild = "GeneratePackageOnBuild";
        public const string PackageOutputPath = "PackageOutputPath";
    }

    private const string XrmToolsMetaAttributesPackageId = "XrmTools.Meta.Attributes";

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

#pragma warning disable VSTHRD109 // Visual Studio hierarchy access requires an explicit main-thread switch.
    public static async Task<bool> IsXrmToolsPluginProjectAsync(this Project project)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        if (bool.TryParse(project.GetBuildProperty(BuildProperties.IsXrmToolsPlugin), out var isXrmToolsPlugin)
            && isXrmToolsPlugin)
        {
            return true;
        }

        project.GetItemInfo(out var hierarchy, out _, out _);
        var solution = Package.GetGlobalService(typeof(SVsSolution)) as IVsSolution;
        var brokered = Package.GetGlobalService(typeof(SVsBrokeredServiceContainer)) as IBrokeredServiceContainer;
        var serviceBroker = brokered?.GetFullAccessServiceBroker();
        if (solution is null || serviceBroker is null)
            return false;

        if (ErrorHandler.Failed(solution.GetGuidOfProject(hierarchy, out var projectGuid)))
            return false;

        var proxy = await serviceBroker.GetProxyAsync<INuGetProjectService>(NuGetServices.NuGetProjectServiceV1).ConfigureAwait(false);
        using (proxy as IDisposable)
        {
            var installed = proxy is null
                ? null
                : await proxy.GetInstalledPackagesAsync(projectGuid, default).ConfigureAwait(false);
            var package = installed?.Packages?.FirstOrDefault(p =>
                string.Equals(p.Id, XrmToolsMetaAttributesPackageId, StringComparison.OrdinalIgnoreCase));

            return package is not null;
        }
    }
#pragma warning restore VSTHRD109

    public static string? FindOutputPackagePath(string projectFilePath, string? packageOutputPath)
    {
        var projectDirectory = Path.GetDirectoryName(projectFilePath);
        if (projectDirectory is null || string.IsNullOrWhiteSpace(packageOutputPath)) return null;

        var outputDirectory = Path.GetFullPath(Path.Combine(projectDirectory, packageOutputPath));
        if (!Directory.Exists(outputDirectory)) return null;

        return Directory.EnumerateFiles(outputDirectory, "*.nupkg", SearchOption.TopDirectoryOnly)
            .Where(path => !path.EndsWith(".symbols.nupkg", StringComparison.OrdinalIgnoreCase))
            .Select(path => new FileInfo(path))
            .OrderByDescending(file => file.LastWriteTimeUtc)
            .FirstOrDefault()?.FullName;
    }

    private static string? GetBuildProperty(IVsBuildPropertyStorage storage, string name)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        return storage?.GetPropertyValue(name, null, (uint)_PersistStorageType.PST_PROJECT_FILE, out var value) == VSConstants.S_OK ? value : null;
    }
}
