using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.Build.Framework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Events;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TaskStatusCenter;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using Microsoft.VisualStudio.Threading;
using System;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using XrmGen.Xrm;
using Task = System.Threading.Tasks.Task;

namespace XrmGen;

/// <summary>
/// This is the class that implements the package exposed by this assembly.
/// </summary>
/// <remarks>
/// <para>
/// The minimum requirement for a class to be considered a valid package for Visual Studio
/// is to implement the IVsPackage interface and register itself with the shell.
/// This package uses the helper classes defined inside the Managed Package Framework (MPF)
/// to do it: it derives from the Package class that provides the implementation of the
/// IVsPackage interface and uses the registration attributes defined in the framework to
/// register itself and its components with the shell. These attributes tell the pkgdef creation
/// utility what data to put into .pkgdef file.
/// </para>
/// <para>
/// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
/// </para>
/// </remarks>
[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
[Guid(PackageGuids.guidXrmCodeGenPackageString)]
[ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
[ProvideCodeGenerator(typeof(EntityGenerator), EntityGenerator.Name, EntityGenerator.Description, true, ProjectSystem = ProvideCodeGeneratorAttribute.CSharpProjectGuid, RegisterCodeBase = true)]
[ProvideCodeGeneratorExtension(EntityGenerator.Name, ".yaml")]
[ProvideCodeGenerator(typeof(PluginGenerator), PluginGenerator.Name, PluginGenerator.Description, true, ProjectSystem = ProvideCodeGeneratorAttribute.CSharpProjectGuid, RegisterCodeBase = true)]
[ProvideCodeGeneratorExtension(EntityGenerator.Name, ".def.json")]
[ProvideMenuResource("Menus.ctmenu", 1)]
// Decide the visibility of our command when the command is NOT yet loaded.
[ProvideUIContextRule(PackageGuids.guidXrmCodeGenUIRuleString,
    name: "UI Context",
	expression: "(Yaml | Proj) & CSharp & (SingleProj | MultiProj)",
	termNames: ["Yaml", "Proj", "CSharp", "SingleProj", "MultiProj"],
	termValues: ["HierSingleSelectionName:.yaml$|.yml$", "HierSingleSelectionItemType:ProjectItem", "ActiveProjectCapability:CSharp", VSConstants.UICONTEXT.SolutionHasSingleProject_string, VSConstants.UICONTEXT.SolutionHasMultipleProjects_string])]
public sealed class XrmGenPackage : AsyncPackage
{
    private DTE2 _dte;

    /// <summary>
    /// XrmGenPackage GUID string.
    /// </summary>
    //public const string PackageGuidString = "9d0b1940-11e7-41cc-a95a-ad5a6ed3c73b";

    /// <summary>
    /// Initialization of the package; this method is called right after the package is sited, so this is the place
    /// where you can put all the initialization code that rely on services provided by VisualStudio.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
    /// <param name="progress">A provider for progress updates.</param>
    /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
    protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
    {
        await base.InitializeAsync(cancellationToken, progress);

        // When initialized asynchronously, the current thread may be a background thread at this point.
        // Do any initialization that requires the UI thread after switching to the UI thread.
        await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
        await ApplyEntityGeneratorCommand.InitializeAsync(this);

        _dte = await GetServiceAsync(typeof(DTE)) as DTE2;
        Assumes.Present(_dte);

        InitializeXrmSchemaForAllProjects();

        // Subscribe to solution events
        _dte.Events.SolutionEvents.Opened += delegate
        {
            ThreadHelper.JoinableTaskFactory.WithPriority(VsTaskRunContext.UIThreadIdlePriority).Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(alwaysYield: true);
                StartBackgroundOperations();
            });
        };

    }

    private void StartBackgroundOperations()
    {
        var taskCenter = GetService(typeof(SVsTaskStatusCenterService)) as IVsTaskStatusCenterService;
        Assumes.Present(taskCenter);

        var options = default(TaskHandlerOptions);
        options.Title = "Loading XRM Metadata";
        options.ActionsAfterCompletion = CompletionActions.None;

        var data = default(TaskProgressData);
        data.CanBeCanceled = false;

        var handler = taskCenter.PreRegister(options, data);
        handler.RegisterTask(RefreshMetadataAsync(data, handler));
    }

    private async Task RefreshMetadataAsync(TaskProgressData data, ITaskHandler handler)
    {
        await XrmSchemaProvider.Instance.RefreshEntityNamesCacheAsync();

        data.ProgressText = "Metadata refresh complete";
        data.PercentComplete = 100;
        handler.Progress.Report(data);
    }

    private void InitializeXrmSchemaForAllProjects()
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        var solution = (IVsSolution)GetService(typeof(SVsSolution));
        var guid = Guid.Empty;
        solution.GetProjectEnum((uint)__VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION, ref guid, out var enumHierarchies);

        IVsHierarchy[] hierarchies = new IVsHierarchy[1];
        while (enumHierarchies.Next(1, hierarchies, out var fetched) == VSConstants.S_OK && fetched == 1)
        {
            var environmentUrl = GetPropertyForProject(hierarchies[0], "EnvironmentUrl");
            var applicationId = GetPropertyForProject(hierarchies[0], "ApplicationId");
            XrmSchemaProvider.Initialize(environmentUrl, applicationId);
        }
    }

    private string GetPropertyForProject(IVsHierarchy hierarchy, string propertyName)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        if (hierarchy is IVsBuildPropertyStorage propertyStorage)
        {
            propertyStorage.GetPropertyValue(propertyName, null, (uint)_PersistStorageType.PST_PROJECT_FILE, out var value);
            return value;
        }
        return string.Empty;
    }

    private (string environmentUrl, string applicationId) GetProjectPropertiesOld()
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        var project = _dte.Solution.Projects.Cast<Project>().FirstOrDefault() ?? throw new InvalidOperationException("No project found in the solution.");
        var environmentUrl = project.Properties.Item("EnvironmentUrl").Value.ToString();
        var applicationId = project.Properties.Item("ApplicationId").Value.ToString();

        return (environmentUrl, applicationId);
    }
}
