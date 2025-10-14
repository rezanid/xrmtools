#nullable enable
namespace XrmTools.Helpers;

using Community.VisualStudio.Toolkit;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

internal static class FileHelper
{
    private static readonly HashSet<char> _invalidFileNameChars = [.. Path.GetInvalidFileNameChars()];
    private static readonly Regex _reservedFileNamePattern = new($@"(?i)^(PRN|AUX|NUL|CON|COM\d|LPT\d)(\.|$)");

    public static async Task<SolutionItem?> FindActiveItemAsync()
    {
        var activeItem = await VS.Solutions.GetActiveItemAsync();
        if (activeItem != null) return activeItem;

        var activeDocView = await VS.Documents.GetActiveDocumentViewAsync();
        if (activeDocView == null) return null;

        var filePath = activeDocView.FilePath;
        if (filePath == null) return null;
        return await PhysicalFile.FromFileAsync(filePath);
    }

    public static async Task<Document?> GetDocumentAsync(string filePath)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
        var workspace = componentModel.GetService<VisualStudioWorkspace>();
        var documentId = workspace.CurrentSolution.GetDocumentIdsWithFilePath(filePath).FirstOrDefault();
        if (documentId == null) return null;
        return workspace.CurrentSolution.GetDocument(documentId);
    }

    public static async Task<Microsoft.CodeAnalysis.Project?> GetProjectAsync(string projectFilePath)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
        var workspace = componentModel.GetService<VisualStudioWorkspace>();

        // Find the project by its file path
        return workspace.CurrentSolution.Projects.FirstOrDefault(p => p.FilePath == projectFilePath);
    }

    public static async Task AddItemAsync(string name, string content, SolutionItem activeItem, bool selectItem = false)
    {
        if (activeItem is null || string.IsNullOrWhiteSpace(name) || content is null) return;
        ValidatePath(name);

        var proj = activeItem.Type is SolutionItemType.Project or SolutionItemType.VirtualProject or SolutionItemType.MiscProject
            ? activeItem as Community.VisualStudio.Toolkit.Project
            : activeItem.FindParent(SolutionItemType.Project) as Community.VisualStudio.Toolkit.Project;

        var path = FindPath(activeItem);
        if (path == null)
        {
            await VS.MessageBox.ShowErrorAsync(Vsix.Name, "Could not determine where to create the new file.");
            return;
        }
        if (!Directory.Exists(path)) path = Path.GetDirectoryName(path);
        string filePath = Path.Combine(path, name);
        FileInfo file = new (filePath);

        if (file.Exists)
        {
            await VS.MessageBox.ShowWarningAsync(Vsix.Name, $"The file '{file}' already exists.");
            return;
        }

        if (proj != null)
        {
            await AddItemAsync(file, content, proj, selectItem);
            return;
        }

        var folder = proj != null ? null : activeItem.FindParent(SolutionItemType.PhysicalFolder) as PhysicalFolder;

        if (folder == null)
        {
            await VS.MessageBox.ShowErrorAsync(Vsix.Name, "Could not determine where to create the new file.");
            return;
        }
        await AddItemAsync(file, content, folder!, selectItem);
    }

    private static string? FindPath(SolutionItem? solutionItem)
    {
        if (solutionItem == null) return null;
        do
        {
            var path = solutionItem.FullPath;
            if (!string.IsNullOrEmpty(path)) return path;
            solutionItem = solutionItem.Parent;
        } while (solutionItem != null);
        return null;
    }

    private static void ValidatePath(string path)
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
            path = Path.GetDirectoryName(path);
        } while (!string.IsNullOrEmpty(path));
    }

    private static async Task AddItemAsync(FileInfo file, string content, Community.VisualStudio.Toolkit.Project project, bool selectItem)
    {
        try
        {
            await WriteToDiskAsync(file.FullName, content);
        }
        catch (Exception)
        {
            await VS.MessageBox.ShowErrorAsync(Vsix.Name, $"Failed to write to file '{file.FullName}'.");
            return;
        }
        var files = await project.AddExistingFilesAsync([file.FullName]);

        files.First().GetItemInfo(out var hierarchy, out var itemId, out _);
        if (selectItem) await hierarchy.SelectInSolutionExplorerAsync(itemId);
    }

    private static async Task AddItemAsync(FileInfo file, string content, PhysicalFolder folder, bool selectItem)
    {
        try
        {
            await WriteToDiskAsync(file.FullName, content);
        }
        catch (Exception)
        {
            await VS.MessageBox.ShowErrorAsync(Vsix.Name, $"Failed to write to file '{file.FullName}'.");
            return;
        }
        var files = await folder.AddExistingFilesAsync([file.FullName]);
        files.First().GetItemInfo(out var hierarchy, out var itemId, out _);
        if (selectItem) await hierarchy.SelectInSolutionExplorerAsync(itemId);
    }

    //TODO: Add another AddItemAsync(FileInfo file, string content, Solution solution)
    //      to add files to the "Solution Items" folder in solution. Inspired by AddNewItem extension.

    private static async Task WriteToDiskAsync(string file, string content)
    {
        using StreamWriter writer = new StreamWriter(file, false, Encoding.UTF8);
        await writer.WriteAsync(content);
    }
}
#nullable restore