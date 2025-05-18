#nullable enable
namespace XrmTools;

using Community.VisualStudio.Toolkit;
using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TaskStatusCenter;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using Microsoft.VisualStudio.Threading;
using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using XrmTools.Helpers;
using XrmTools.Logging;
using XrmTools.Options;
using XrmTools.Settings;
using XrmTools.Xrm.Generators;
using XrmTools.Tokens;
using Task = System.Threading.Tasks.Task;
using XrmTools.Commands;
using XrmTools.Environments;
using System.Reflection;
using System.IO;

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
[Guid(PackageGuids.XrmToolsPackageIdString)]
[ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
[ProvideCodeGenerator(typeof(EntityCodeGenerator), EntityCodeGenerator.Name, EntityCodeGenerator.Description, true, ProjectSystem = ProvideCodeGeneratorAttribute.CSharpProjectGuid, RegisterCodeBase = true)]
[ProvideCodeGeneratorExtension(EntityCodeGenerator.Name, ".yaml")]
[ProvideCodeGenerator(typeof(PluginCodeGenerator), PluginCodeGenerator.Name, PluginCodeGenerator.Description, true, ProjectSystem = ProvideCodeGeneratorAttribute.CSharpProjectGuid, RegisterCodeBase = true)]
[ProvideCodeGeneratorExtension(PluginCodeGenerator.Name, ".def.json")]
[ProvideMenuResource("Menus.ctmenu", 1)]
// Decide the visibility of our commands when the commands are NOT yet loaded.
[ProvideUIContextRule(PackageGuids.SetCustomToolEntitiesCmdUIRuleString,
    name: "UI Context Entity Definition",
    expression: "(Yaml | CSEntity) & CSharp & (SingleProj | MultiProj)",
    termNames: ["Yaml", "CSEntity", "CSharp", "SingleProj", "MultiProj"],
    termValues: [
        "HierSingleSelectionName:\\.yaml$|\\.yml$",
        "HierSingleSelectionName:\\.cs$",
        "ActiveProjectCapability:CSharp",
        VSConstants.UICONTEXT.SolutionHasSingleProject_string,
        VSConstants.UICONTEXT.SolutionHasMultipleProjects_string])]
[ProvideUIContextRule(PackageGuids.SetCustomToolPluginDefitionCmdUIRuleString,
    name: "UI Context Plugin Definition",
    expression: "(Json | CSPlugin) & CSharp & (SingleProj | MultiProj)",
    termNames: ["Json", "CSPlugin", "CSharp", "SingleProj", "MultiProj"],
    termValues: [
        "HierSingleSelectionName:\\.def\\.json$",
        "HierSingleSelectionName:\\.cs$",
        "ActiveProjectCapability:CSharp",
        VSConstants.UICONTEXT.SolutionHasSingleProject_string,
        VSConstants.UICONTEXT.SolutionHasMultipleProjects_string])]
[ProvideUIContextRule(PackageGuids.SetPluginGeneratorTemplateCmdUIRuleString,
    name: "UI Context Plugin Generator Template",
    expression: "Sbn & CSharp & (SingleProj | MultiProj)",
    termNames: ["Sbn", "CSharp", "SingleProj", "MultiProj"],
    termValues: [
        "HierSingleSelectionName:.sbncs$", 
        "ActiveProjectCapability:CSharp", 
        VSConstants.UICONTEXT.SolutionHasSingleProject_string, 
        VSConstants.UICONTEXT.SolutionHasMultipleProjects_string])]
[ProvideUIContextRule(PackageGuids.NewPluginDefinitionCmdUIRuleString,
    name: "UI Context NewPluginConfigCommand",
    expression: "CSharp & (SingleProj | MultiProj)",
    termNames: ["CSharp", "SingleProj", "MultiProj"],
    termValues: [
        "ActiveProjectCapability:CSharp",
        VSConstants.UICONTEXT.SolutionHasSingleProject_string,
        VSConstants.UICONTEXT.SolutionHasMultipleProjects_string])]
[ProvideService(typeof(IXrmCodeGenerator), IsAsyncQueryable = true, IsCacheable = true, IsFreeThreaded = true)]
[ProvideService(typeof(IEnvironmentProvider), IsAsyncQueryable = true, IsCacheable = true, IsFreeThreaded = true)]
[ProvideService(typeof(ISettingsProvider), IsAsyncQueryable = true, IsCacheable = true, IsFreeThreaded = true)]
[ProvideOptionPage(typeof(OptionsProvider.GeneralOptions), Vsix.Name, "General", 0, 0, true, SupportsProfiles = true)]
public sealed partial class XrmToolsPackage : ToolkitPackage
{
    private static readonly object _lock = new();

    // It has been observed that VS can call the constructor of the package twice! causing multiple instances of the singleton
    // services to be created. To avoid side effects, we store them in static fields.
    private readonly static IOutputLoggerService _loggerService;
    private readonly static IEnvironmentProvider _environmentProvider;
    private readonly static ISettingsProvider _settingsProvider;
    private readonly static ITokenExpanderService _tokenExpanderService;

    public const string SolutionPersistanceKey = "XrmToolsProperies";
    private static readonly ExplicitInterfaceInvoker<Package> implicitInvoker = new();
    public DTE2? Dte;

    /// <summary>
    /// The initial configuration of the extension. The value can change after initialization if 
    /// the user changes the settings.
    /// </summary>
    private GeneralOptions? Options;

    [Export(typeof(TimeProvider))] internal TimeProvider TimeProvider { get => TimeProvider.System; }
    [Export(typeof(IOutputLoggerService))] internal IOutputLoggerService OutputLoggerService { get => _loggerService; }
    [Export(typeof(IEnvironmentProvider))] internal IEnvironmentProvider EnvironmentProvider { get => _environmentProvider; }
    [Export(typeof(ISettingsProvider))] internal ISettingsProvider SettingsProvider { get => _settingsProvider; }
    [Export(typeof(ITokenExpanderService))] internal ITokenExpanderService TokenExpanderService { get => _tokenExpanderService; }

    static XrmToolsPackage()
    {
        _loggerService = new OutputLoggerService();
        _settingsProvider = new SettingsProvider();
        _environmentProvider = new DataverseEnvironmentProvider(_settingsProvider);
        _tokenExpanderService = new TokenExpanderService([
            new CredentialTokenExpander(new CredentialManager()),
            new EnvironmentTokenExpander()]);
        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
    }

    private static Assembly? CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    {
        //System.Security.Cryptography.ProtectedData, Version=4.0.3.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
        if (args.Name.StartsWith("System.Security.Cryptography.ProtectedData"))
        {
            return Assembly.LoadFile(Path.Combine(Assembly.GetExecutingAssembly().Location, "System.Security.Cryptography.ProtectedData.dll"));
        }
        return null;
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
        foreach (var key in SettingsProvider.SolutionUserSettings.Keys)
        {
            AddOptionKey(key);
        }

        await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

        Dte = await GetServiceAsync(typeof(DTE)) as DTE2
            ?? throw new InvalidOperationException(
                string.Format(Resources.Strings.Package_InitializationErroMissingDte, nameof(XrmToolsPackage)));
        Assumes.Present(Dte);

        Options = await GeneralOptions.GetLiveInstanceAsync();

        await InitializeMefServicesAsync();

        await NewPluginDefinitionFileCommand.InitializeAsync(this);
        await SetPluginGeneratorTemplateInProjectCommand.InitializeAsync(this);
        await SetPluginGeneratorTemplateInSolutionCommand.InitializeAsync(this);
        await SetEntityGeneratorTemplateInProjectCommand.InitializeAsync(this);
        await SetEntityGeneratorTemplateInSolutionCommand.InitializeAsync(this);
        await SetCustomToolEntityGeneratorCommand.InitializeAsync(this);
        await SetCustomToolPluginGeneratorCommand.InitializeAsync(this);
        await RegisterPluginCommand.InitializeAsync(this);
        await SelectEnvironmentCommand.InitializeAsync(this);

        // When initialized asynchronously, the current thread may be a background thread at this point.
        // Do any initialization that requires the UI thread after switching to the UI thread.
        //await ApplyEntityGeneratorCommand.InitializeAsync(this);
        //await SetXrmPluginGeneratorCommand.InitializeAsync(this);
        //await GenerateRegistrationFileCommand.InitializeAsync(this);

        //ProjectHelpers.ParentPackage = this;

        // This is for when extension is loaded before the solution is opened.
        // Usually happens when user opens Visual Studio with a solution already loaded.
        Dte.Events.SolutionEvents.Opened += () =>
        {
            ThreadHelper.JoinableTaskFactory.WithPriority(VsTaskRunContext.UIThreadIdlePriority).Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(alwaysYield: true);
                StartBackgroundOperations();
            });
        };
        // This is for when extension is loaded after the solution is already open.
        // Usually happens when user opens Visual Studio and then opens a solution.

        //TODO: In one article, the following call is at the begining of the method:
        // https://learn.microsoft.com/en-us/visualstudio/extensibility/how-to-provide-an-asynchronous-visual-studio-service?view=vs-2022
        await base.InitializeAsync(cancellationToken, progress);
    }

    private async Task InitializeMefServicesAsync()
    {
        //TODO: Temporary remove to test.
        //AddService(
        //    typeof(IXrmPluginCodeGenerator),
        //    async (container, cancellationToken, type) =>
        //        await Task.FromResult(new TemplatedPluginCodeGenerator())
        //    , promote: true);
        AddService(
            typeof(ISettingsProvider),
            async (container, cancellationToken, type) =>
                await Task.FromResult(SettingsProvider)
            // Add service at global level to make it available to Single-File generators.
            , promote: true);
        AddService(
            typeof(IEnvironmentProvider),
            async (container, cancellationToken, type) =>
                await Task.FromResult(EnvironmentProvider)
                , promote: true);
        await Task.CompletedTask;
    }

    private void StartBackgroundOperations()
    {
        //var envs = GetProjectsDataverseEnvironments()
        //    .Where(s => !string.IsNullOrWhiteSpace(s.EnvironmentUrl)
        //                && Uri.IsWellFormedUriString(s.EnvironmentUrl, UriKind.Absolute))
        //    // The following grouping is to avoid having multiple environments with the same URL.
        //    .GroupBy(s => s.EnvironmentUrl)
        //    .Select(g => g.First())
        //    .ToList();
        var taskCenter = GetService(typeof(SVsTaskStatusCenterService)) as IVsTaskStatusCenterService;
        Assumes.Present(taskCenter);
        var options = default(TaskHandlerOptions);
        options.Title = "Loading XRM Metadata";
        options.ActionsAfterCompletion = CompletionActions.None;

        var data = default(TaskProgressData);
        data.CanBeCanceled = false;

        var handler = taskCenter.PreRegister(options, data);
        handler.RegisterTask(InitializeXrmMetadataAsync(data, handler));
    }

    private async Task InitializeXrmMetadataAsync(TaskProgressData data, ITaskHandler handler)
    {
        var options = await GeneralOptions.GetLiveInstanceAsync();
        var environments = options.Environments
            .Where(e => e.IsValid).ToList();
        var currentIndex = 0;
        foreach (var env in environments)
        {
            data.ProgressText = $"Refreshing metadata for {env.Url}";
            data.PercentComplete = 100 * currentIndex / environments.Count;
            handler.Progress.Report(data);
            //TODO: Write code to prefetch data if possible.
            //await XrmSchemaProviderFactory.EnsureInitializedAsync(env).ConfigureAwait(false);
        }
        data.ProgressText = "Metadata refresh complete";
        data.PercentComplete = 100;
        handler.Progress.Report(data);
    }
}
#nullable restore