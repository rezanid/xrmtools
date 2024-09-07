#nullable enable
using Community.VisualStudio.Toolkit;
using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using XrmGen._Core;
using XrmGen.Extensions;
using XrmGen.UI;
using XrmGen.Xrm;
using XrmGen.Xrm.Model;
using Task = System.Threading.Tasks.Task;

namespace XrmGen.Commands;

/// <summary>
/// Command handler to generate the registration file for the selected assembly.
/// </summary>
internal sealed class GenerateRegistrationFileCommand
{
    private static readonly HashSet<char> _invalidFileNameChars = new(Path.GetInvalidFileNameChars());
    private const string _solutionItemsProjectName = "Solution Items";
    private static readonly Regex _reservedFileNamePattern = new ($@"(?i)^(PRN|AUX|NUL|CON|COM\d|LPT\d)(\.|$)");

    private readonly AsyncPackage package;
    private readonly DTE2 dte;
    private IXrmSchemaProviderFactory? _schemaProviderFactory;

    [Import]
    IXrmSchemaProviderFactory? SchemaProviderFactory
    {
        // MEF does not work here, so this is our only option.
        get => _schemaProviderFactory ??= ServiceProvider.GetServiceAsync<IXrmSchemaProviderFactory, IXrmSchemaProviderFactory>().Result;
        set => _schemaProviderFactory = value;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="GenerateRegistrationFileCommand"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    /// <param name="commandService">Command service to add command to, not null.</param>
    private GenerateRegistrationFileCommand(AsyncPackage package, IMenuCommandService commandService, DTE2 dte)
    {
        this.package = package ?? throw new ArgumentNullException(nameof(package));
        this.dte = dte ?? throw new ArgumentNullException(nameof(dte));
        commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

        var cmdId = new CommandID(PackageGuids.guidXrmCodeGenPackageCmdSet, PackageIds.GeneratePluginConfigFileCommandId);
        var menuItem = new OleMenuCommand(OnExecute, cmdId, queryStatusSupported: false);
        commandService.AddCommand(menuItem);
    }

    /// <summary>
    /// Gets the instance of the command.
    /// </summary>
    public static GenerateRegistrationFileCommand Instance { get; private set; }

    /// <summary>
    /// Gets the service provider from the owner package.
    /// </summary>
    private IAsyncServiceProvider ServiceProvider { get => this.package; }

    /// <summary>
    /// Initializes the singleton instance of the command.
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    public static async Task InitializeAsync(AsyncPackage package)
    {
        // Switch to the main thread - the call to AddCommand in ToolWindow1Command's constructor requires
        // the UI thread.
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

        var commandService = await package.GetServiceAsync<IMenuCommandService, IMenuCommandService>();

        // Unless there is a better / easier  way to do it instead of using DTE, we will. For now, it's OK.
        // Note! DTE2 is the new DTE, but the service is registered in DTE, so we get it and cast it like following.
        var dte = await package.GetServiceAsync<DTE, DTE2>();
        Assumes.Present(dte);

        Instance = new GenerateRegistrationFileCommand(package, commandService, dte);
    }

    private async void OnExecute(object sender, EventArgs e)
    {
        var target = NewItemTarget.Create(dte);

        if (target == null)
        {
            VS.MessageBox.Show(
                    Vsix.Name,
                    "Could not determine where to create the new file. Select a file or folder in Solution Explorer and try again.");
            return;
        }

        //item.Properties.Item("CustomTool").Value = PluginCodeGenerator.Name;
        var (selectedAssembly, filename) = ChooseAssembly();
        if (selectedAssembly is not null && filename is not null)
        {
            Logger.Log("Assembly selected: " + selectedAssembly.Name);
            var content = StringHelpers.SerializeJson(selectedAssembly);
            await AddItemAsync(filename, target, content ?? string.Empty);
        }
        else
        {
            Logger.Log("No assembly selected.");
        }
    }

    private async Task AddItemAsync(string name, NewItemTarget target, string content)
    {
        // The naming rules that apply to files created on disk also apply to virtual solution folders,
        // so regardless of what type of item we are creating, we need to validate the name.
        ValidatePath(name);

        if (name.EndsWith("\\", StringComparison.Ordinal))
        {
            if (target.IsSolutionOrSolutionFolder)
            {
                GetOrAddSolutionFolder(name, target);
            }
            else
            {
                AddProjectFolder(name, target);
            }
        }
        else
        {
            await AddFileAsync(name, target, content);
        }
    }

    private void ValidatePath(string path)
    {
        do
        {
            string name = Path.GetFileName(path);

            if (_reservedFileNamePattern.IsMatch(name))
            {
                throw new InvalidOperationException($"The name '{name}' is a system reserved name.");
            }

            if (name.Any(c => _invalidFileNameChars.Contains(c)))
            {
                throw new InvalidOperationException($"The name '{name}' contains invalid characters.");
            }
            //TODO: Error: The path is not of a legal form.
            path = Path.GetDirectoryName(path);
        } while (!string.IsNullOrEmpty(path));
    }

    private async Task AddFileAsync(string name, NewItemTarget target, String content)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        FileInfo file;

        // If the file is being added to a solution folder, but that
        // solution folder doesn't have a corresponding directory on
        // disk, then write the file to the root of the solution instead.
        if (target.IsSolutionFolder && !Directory.Exists(target.Directory))
        {
            file = new FileInfo(Path.Combine(Path.GetDirectoryName(dte.Solution.FullName), Path.GetFileName(name)));
        }
        else if (name.StartsWith("sln\\"))
        {
            file = new FileInfo(Path.Combine(Path.GetDirectoryName(dte.Solution.FullName), Path.GetFileName(name.Substring(4))));
        }
        else if (name.StartsWith("prj\\") && target.Project != null)
        {
            file = new FileInfo(Path.Combine(Path.GetDirectoryName(target.Project.FileName), Path.GetFileName(name.Substring(4))));
        }
        else
        {
            file = new FileInfo(Path.Combine(target.Directory, name));
        }

        // Make sure the directory exists before we create the file. Don't use
        // `PackageUtilities.EnsureOutputPath()` because it can silently fail.
        Directory.CreateDirectory(file.DirectoryName);

        if (!file.Exists)
        {
            EnvDTE.Project project;

            if (target.IsSolutionOrSolutionFolder)
            {
                project = GetOrAddSolutionFolder(Path.GetDirectoryName(name), target);
            }
            else
            {
                project = target.Project;
            }

            await WriteToDiskAsync(file.FullName, content);
            if (target.ProjectItem != null && target.ProjectItem.IsKind(EnvDTE.Constants.vsProjectItemKindVirtualFolder))
            {
                target.ProjectItem.ProjectItems.AddFromFile(file.FullName);
            }
            else
            {
                project.AddFileToProject(file);
            }

            VsShellUtilities.OpenDocument(package, file.FullName);

            dte.ExecuteCommandIfAvailable("SolutionExplorer.SyncWithActiveDocument");
            dte.ActiveDocument.Activate();
        }
        else
        {
            await VS.MessageBox.ShowAsync(Vsix.Name, $"The file '{file}' already exists.");
        }
    }

    private static async Task WriteToDiskAsync(string file, string content)
    {
        using StreamWriter writer = new StreamWriter(file, false, Encoding.UTF8);
        await writer.WriteAsync(content);
    }

    private EnvDTE.Project GetOrAddSolutionFolder(string name, NewItemTarget target)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        if (target.IsSolution && string.IsNullOrEmpty(name))
        {
            // An empty solution folder name means we are not creating any solution
            // folders for that item, and the file we are adding is intended to be
            // added to the solution. Files cannot be added directly to the solution,
            // so there is a "Solution Items" folder that they are added to.
            return dte.Solution.FindSolutionFolder(_solutionItemsProjectName)
                    ?? ((Solution2)dte.Solution).AddSolutionFolder(_solutionItemsProjectName);
        }

        // Even though solution folders are always virtual, if the target directory exists,
        // then we will also create the new directory on disk. This ensures that any files
        // that are added to this folder will end up in the corresponding physical directory.
        if (Directory.Exists(target.Directory))
        {
            // Don't use `PackageUtilities.EnsureOutputPath()` because it can silently fail.
            Directory.CreateDirectory(Path.Combine(target.Directory, name));
        }

        var parent = target.Project;

        foreach (var segment in SplitPath(name))
        {
            // If we don't have a parent project yet,
            // then this folder is added to the solution.
            if (parent == null)
            {
                parent = dte.Solution.FindSolutionFolder(segment) ?? ((Solution2)dte.Solution).AddSolutionFolder(segment);
            }
            else
            {
                parent = parent.FindSolutionFolder(segment) ?? ((EnvDTE80.SolutionFolder)parent.Object).AddSolutionFolder(segment);
            }
        }

        return parent;
    }

    private static string[] SplitPath(string path)
        => path.Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries);

    private void AddProjectFolder(string name, NewItemTarget target)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        // Make sure the directory exists before we add it to the project. Don't
        // use `PackageUtilities.EnsureOutputPath()` because it can silently fail.
        string targetFolder = Path.Combine(target.Directory, name);
        Directory.CreateDirectory(targetFolder);
        ProjectHelpers.AddFolders(target.Project, targetFolder);
    }

    private string? GetProjectProperty(string propertyName)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        IVsSolution solution = (IVsSolution)ServiceProvider.GetServiceAsync(typeof(SVsSolution)).Result;
        Assumes.Present(solution);
        solution.GetProjectOfUniqueName(dte.SelectedItems.Item(1).ProjectItem.ContainingProject.UniqueName, out var hierarchy);

        return hierarchy.GetProjectProperty(propertyName);
    }

    private (PluginAssemblyConfig? config, string? filename) ChooseAssembly()
    {
        var url = GetProjectProperty("EnvironmentUrl");
        if (string.IsNullOrWhiteSpace(url)) { return (null, null); }
        var schemaProvider = SchemaProviderFactory?.Get(url!);
        if (schemaProvider == null) 
        {
            Logger.Log(url + " used in your EnvironmentUrl build property is not a valid environment URL.");
            return (null, null); 
        }
        var dialog = new AssemblySelectionDialog(schemaProvider);
        if (dialog.ShowDialog() == true)
        {
            var viewmodel = (AssemblySelectionViewModel)dialog.DataContext;
            return (viewmodel.SelectedAssembly, viewmodel.FileName);
        }
        return (null, null);
    }

}
#nullable restore