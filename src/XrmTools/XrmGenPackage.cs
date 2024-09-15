#nullable enable
namespace XrmGen;

using Community.VisualStudio.Toolkit;
using Community.VisualStudio.Toolkit.DependencyInjection.Microsoft;
using EnvDTE;
using EnvDTE80;
using Humanizer.Localisation;
using Microsoft;
using Microsoft.Build.Framework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.RpcContracts.Utilities;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Events;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TaskStatusCenter;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using VSLangProj;
using XrmGen.Commands;
using XrmGen.Helpers;
using XrmGen.Logging;
using XrmGen.Options;
using XrmGen.Xrm;
using XrmGen.Xrm.Generators;
using Task = System.Threading.Tasks.Task;

internal record ProjectDataverseSettings(
    string EnvironmentUrl, 
    string? ConnectionString, 
    string? PluginCodeGenTemplatePath, 
    string? EntityCodeGenTemplatePath);


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
[ProvideCodeGenerator(typeof(EntityCodeGenerator), EntityCodeGenerator.Name, EntityCodeGenerator.Description, true, ProjectSystem = ProvideCodeGeneratorAttribute.CSharpProjectGuid, RegisterCodeBase = true)]
[ProvideCodeGeneratorExtension(EntityCodeGenerator.Name, ".yaml")]
[ProvideCodeGenerator(typeof(PluginCodeGenerator), PluginCodeGenerator.Name, PluginCodeGenerator.Description, true, ProjectSystem = ProvideCodeGeneratorAttribute.CSharpProjectGuid, RegisterCodeBase = true)]
[ProvideCodeGeneratorExtension(PluginCodeGenerator.Name, ".def.json")]
[ProvideMenuResource("Menus.ctmenu", 1)]
// Decide the visibility of our command when the command is NOT yet loaded.
[ProvideUIContextRule(PackageGuids.guidXrmCodeGenUIRuleString,
    name: "UI Context",
    expression: "(Yaml | Proj) & CSharp & (SingleProj | MultiProj)",
    termNames: ["Yaml", "Proj", "CSharp", "SingleProj", "MultiProj"],
    termValues: ["HierSingleSelectionName:.yaml$|.yml$", "HierSingleSelectionItemType:ProjectItem", "ActiveProjectCapability:CSharp", VSConstants.UICONTEXT.SolutionHasSingleProject_string, VSConstants.UICONTEXT.SolutionHasMultipleProjects_string])]
[ProvideUIContextRule(PackageGuids.guidXrmCodeGenSetPluginGeneratorCommandUIRuleString,
    name: "UI Context 2",
    expression: "(Json | Proj) & CSharp & (SingleProj | MultiProj)",
    termNames: ["Json", "Proj", "CSharp", "SingleProj", "MultiProj"],
    termValues: [
        "HierSingleSelectionName:.def.json$", 
        "HierSingleSelectionItemType:ProjectItem", 
        "ActiveProjectCapability:CSharp", 
        VSConstants.UICONTEXT.SolutionHasSingleProject_string, 
        VSConstants.UICONTEXT.SolutionHasMultipleProjects_string])]
[ProvideUIContextRule(PackageGuids.guidGeneratePluginConfigFileCommandUIRuleString,
    name: "UI Context NewPluginConfigCommand",
    expression: "Proj & CSharp & (SingleProj | MultiProj)",
    termNames: ["Proj", "CSharp", "SingleProj", "MultiProj"],
    termValues: ["HierSingleSelectionItemType:ProjectItem",
        "ActiveProjectCapability:CSharp",
        VSConstants.UICONTEXT.SolutionHasSingleProject_string,
        VSConstants.UICONTEXT.SolutionHasMultipleProjects_string])]
[ProvideService(typeof(IXrmSchemaProviderFactory), IsAsyncQueryable = true, IsCacheable = true, IsFreeThreaded = true)]
[ProvideService(typeof(IXrmPluginCodeGenerator), IsAsyncQueryable = true, IsCacheable = true, IsFreeThreaded = true)]
[ProvideService(typeof(IXrmEntityCodeGenerator), IsAsyncQueryable = true, IsCacheable = true, IsFreeThreaded = true)]
[ProvideOptionPage(typeof(OptionsProvider.GeneralOptions), Vsix.Name, "General", 0, 0, true, SupportsProfiles = true)]
public sealed class XrmGenPackage : MicrosoftDIToolkitPackage<XrmGenPackage>
{
    public DTE2? Dte;

    /// <summary>
    /// The initial configuration of the extension. The value can change after initialization if 
    /// the user changes the settings.
    /// </summary>
    private GeneralOptions? InitialConfig;

    // Warning!
    // MEF Doesn't work here. Use the ServiceProvider to get the requried service.
    private IXrmSchemaProviderFactory? XrmSchemaProviderFactory;

    static XrmGenPackage()
    {
        var t = VS.Shell.GetType();
    }

    protected override void InitializeServices(IServiceCollection services)
    {
        services
            .AddMemoryCache()
            .AddLogging(builder =>
            {
                builder.SetMinimumLevel(GeneralOptions.Instance.LogLevel);
                builder.AddOutputLogger();
            })
            //.AddHostedService<IXrmSchemaProviderFactory>()
            .RegisterCommands(ServiceLifetime.Singleton);
        // Register your services here
        // services.AddSingleton<IYourService, YourService>();

        // Register any commands.They can be registered as a 'Singleton' or 'Scoped'.
        // 'Transient' will work but in practice it will behave the same as 'Scoped'.
        // services.AddSingleton<YourCommand>();

        //Alternatively, you can use the 'RegisterCommands' extension method to automatically register all commands in an assembly.
        //services.RegisterCommands(ServiceLifetime.Singleton);
        //...

    }

    /// <summary>
    /// Initialization of the package; this method is called right after the package is sited, so this is the place
    /// where you can put all the initialization code that rely on services provided by VisualStudio.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
    /// <param name="progress">A provider for progress updates.</param>
    /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
    protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
    {
        await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

        Dte = await GetServiceAsync(typeof(DTE)) as DTE2
            ?? throw new InvalidOperationException(
                string.Format(Resources.Strings.Package_InitializationErroMissingDte, nameof(XrmGenPackage)));
        Assumes.Present(Dte);

        InitialConfig = await GeneralOptions.GetLiveInstanceAsync();

        await InitializeMefServicesAsync();
        if (XrmSchemaProviderFactory is null)
        {
            throw new InvalidOperationException("XrmSchemaProviderFactory is missing.");
        }

        // When initialized asynchronously, the current thread may be a background thread at this point.
        // Do any initialization that requires the UI thread after switching to the UI thread.
        //await ApplyEntityGeneratorCommand.InitializeAsync(this);
        //await SetXrmPluginGeneratorCommand.InitializeAsync(this);
        //await GenerateRegistrationFileCommand.InitializeAsync(this);


        ProjectHelpers.ParentPackage = this;

        // This is for when extension is loaded before the solution is opened.
        // Usually happens when user opens Visual Studio with a solution already loaded.
        Dte.Events.SolutionEvents.Opened += () =>
        {
            OutputLogger.Log("Solution Opened");
            ThreadHelper.JoinableTaskFactory.WithPriority(VsTaskRunContext.UIThreadIdlePriority).Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(alwaysYield: true);
                StartBackgroundOperations();
            });
        };
        // This is for when extension is loaded after the solution is already open.
        // Usually happens when user opens Visual Studio and then opens a solution.
        StartBackgroundOperations();
        await base.InitializeAsync(cancellationToken, progress);
    }

    [MemberNotNull(nameof(XrmSchemaProviderFactory))]
    private async Task InitializeMefServicesAsync()
    {
        XrmSchemaProviderFactory ??= new XrmSchemaProviderFactory();
        AddService(
            typeof(IXrmSchemaProviderFactory), 
            async (container, cancellationToken, type) =>
                await Task.FromResult(XrmSchemaProviderFactory)
            , promote: true);
        AddService(
            typeof(IXrmPluginCodeGenerator),
            async (container, cancellationToken, type) =>
                await Task.FromResult(new TemplatedPluginCodeGenerator())
            // Add service at global level to make it available to Single-File generators.
            , promote: true);
        AddService(
            typeof(IXrmEntityCodeGenerator),
            async (container, cancellationToken, type) =>
                await Task.FromResult(new TemplatedEntityCodeGenerator())
            // Add service at global level to make it available to Single-File generators.
            , promote: true);
        await Task.CompletedTask;
    }

    private void StartBackgroundOperations()
    {
        var envs = GetProjectsDataverseEnvironments()
            .Where(s => !string.IsNullOrWhiteSpace(s.EnvironmentUrl)
                        && Uri.IsWellFormedUriString(s.EnvironmentUrl, UriKind.Absolute))
            // The following grouping is to avoid having multiple environments with the same URL.
            .GroupBy(s => s.EnvironmentUrl)
            .Select(g => g.First())
            .ToList();
        if (!envs.Any()) { return; }
        var taskCenter = GetService(typeof(SVsTaskStatusCenterService)) as IVsTaskStatusCenterService;
        Assumes.Present(taskCenter);
        var options = default(TaskHandlerOptions);
        options.Title = "Loading XRM Metadata";
        options.ActionsAfterCompletion = CompletionActions.None;

        var data = default(TaskProgressData);
        data.CanBeCanceled = false;

        var handler = taskCenter.PreRegister(options, data);
        handler.RegisterTask(InitializeXrmMetadataAsync(envs, data, handler));
    }

    private async Task InitializeXrmMetadataAsync(IList<ProjectDataverseSettings> envs, TaskProgressData data, ITaskHandler handler)
    {
        if (XrmSchemaProviderFactory is null)
        {
            data.ProgressText = "Failed to initialize XRM metadata.";
            data.PercentComplete = 100;
            handler.Progress.Report(data);
            throw new InvalidOperationException("XrmSchemaProviderFactory is missing.");
        }
        var currentIndex = 0;
        foreach (var env in envs)
        {
            data.ProgressText = $"Refreshing metadata for {env.EnvironmentUrl}";
            data.PercentComplete = 100 * currentIndex / envs.Count;
            handler.Progress.Report(data);

            await XrmSchemaProviderFactory.EnsureInitializedAsync(env.EnvironmentUrl);
        }
        data.ProgressText = "Metadata refresh complete";
        data.PercentComplete = 100;
        handler.Progress.Report(data);
    }

    private IEnumerable<ProjectDataverseSettings> GetProjectsDataverseEnvironments()
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        var solution = (IVsSolution)GetService(typeof(SVsSolution));
        var guid = Guid.Empty;
        solution.GetProjectEnum((uint)__VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION, ref guid, out var enumHierarchies);

        var hierarchies = new IVsHierarchy[1];
        while (enumHierarchies.Next(1, hierarchies, out var fetched) == VSConstants.S_OK && fetched == 1)
        {
            yield return new(
                EnvironmentUrl: hierarchies[0].GetProjectProperty("EnvironmentUrl"),
                ConnectionString: hierarchies[0].GetProjectProperty("ConnectionString"),
                PluginCodeGenTemplatePath: hierarchies[0].GetProjectProperty("PluginCodeGenTemplatePath"),
                EntityCodeGenTemplatePath: hierarchies[0].GetProjectProperty("EntityCodeGenTemplatePath"));
        }
    }

    [MemberNotNull(nameof(Dte), nameof(XrmSchemaProviderFactory))]
    private void ThrowIfNotInitialized()
    {
        if (XrmSchemaProviderFactory is null)
        {
            throw new InvalidOperationException("XrmSchemaProviderFactory is missing. Extension package not initialized successfully.");
        }
        if (Dte is null)
        {
            throw new InvalidOperationException("DTE is missing. Extension package not initialized successfully.");
        }
    }
}
