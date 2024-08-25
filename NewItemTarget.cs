﻿#nullable enable
namespace XrmGen;

using EnvDTE;
using EnvDTE80;

using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XrmGen.Extensions;

public class NewItemTarget
{
    public static NewItemTarget Create(DTE2 dte)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        NewItemTarget? item = null;

        // If a document is active, try to use the document's containing directory.
        if (dte.ActiveWindow is Window2 window && window.Type == vsWindowType.vsWindowTypeDocument)
        {
            item = CreateFromActiveDocument(dte);
        }

        // If no document was selected, or we could not get a selected item from 
        // the document, then use the selected item in the Solution Explorer window.
        item ??= CreateFromSolutionExplorerSelection(dte);

        return item;
    }

    [SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "Already done in caller's site.")]
    private static NewItemTarget? CreateFromActiveDocument(DTE2 dte)
    {
        var fileName = dte.ActiveDocument?.FullName;
        if (File.Exists(fileName))
        {
            ProjectItem docItem = dte.Solution.FindProjectItem(fileName);
            if (docItem != null)
            {
                return CreateFromProjectItem(docItem);
            }
        }

        return null;
    }

    private static NewItemTarget? CreateFromSolutionExplorerSelection(DTE2 dte)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        var items = (Array)dte.ToolWindows.SolutionExplorer.SelectedItems;

        if (items.Length == 1)
        {
            var selection = items.Cast<UIHierarchyItem>().First();

            if (selection.Object is Solution solution)
            {
                return new NewItemTarget(Path.GetDirectoryName(solution.FullName), null, null, isSolutionOrSolutionFolder: true);
            }
            else if (selection.Object is Project project)
            {
                if (project.IsKind(Constants.vsProjectKindSolutionItems))
                {
                    return new NewItemTarget(GetSolutionFolderPath(project), project, null, isSolutionOrSolutionFolder: true);
                }
                else
                {
                    return new NewItemTarget(project.GetRootFolder(), project, null, isSolutionOrSolutionFolder: false);
                }
            }
            else if (selection.Object is ProjectItem projectItem)
            {
                return CreateFromProjectItem(projectItem);
            }
        }

        return null;
    }

    private static NewItemTarget CreateFromProjectItem(ProjectItem projectItem)
    {
        if (projectItem.IsKind(Constants.vsProjectItemKindSolutionItems))
        {
            return new NewItemTarget(
                    GetSolutionFolderPath(projectItem.ContainingProject),
                    projectItem.ContainingProject,
                    null,
                    isSolutionOrSolutionFolder: true);
        }
        else
        {
            // The selected item needs a directory. This project item could be 
            // a virtual folder, so resolve it to a physical file or folder.
            var physical_projectItem = ResolveToPhysicalProjectItem(projectItem);
            string fileName = physical_projectItem?.GetFileName();

            if (string.IsNullOrEmpty(fileName))
            {
                return null;
            }

            // If the file exists, then it must be a file and we can get the
            // directory name from it. If the file does not exist, then it
            // must be a directory, and the directory name is the file name.
            string directory = File.Exists(fileName) ? Path.GetDirectoryName(fileName) : fileName;
            return new NewItemTarget(directory, projectItem.ContainingProject, projectItem, isSolutionOrSolutionFolder: false);
        }
    }

    private static ProjectItem ResolveToPhysicalProjectItem(ProjectItem projectItem)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        if (projectItem.IsKind(Constants.vsProjectItemKindVirtualFolder))
        {
            // Find the first descendant item that is not a virtual folder.
            return projectItem.ProjectItems
                    .Cast<ProjectItem>()
                    .Select(item => ResolveToPhysicalProjectItem(item))
                    .FirstOrDefault(item => item != null);
        }

        return projectItem;
    }

    private static string GetSolutionFolderPath(Project folder)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        string solutionDirectory = Path.GetDirectoryName(folder.DTE.Solution.FullName);
        List<string> segments = new List<string>();

        // Record the names of each folder up the 
        // hierarchy until we reach the solution.
        do
        {
            segments.Add(folder.Name);
            folder = folder.ParentProjectItem?.ContainingProject;
        } while (folder != null);

        // Because we walked up the hierarchy, 
        // the path segments are in reverse order.
        segments.Reverse();

        return Path.Combine(new[] { solutionDirectory }.Concat(segments).ToArray());
    }

    private NewItemTarget(string directory, Project project, ProjectItem projectItem, bool isSolutionOrSolutionFolder)
    {
        Directory = directory;
        Project = project;
        ProjectItem = projectItem;
        IsSolutionOrSolutionFolder = isSolutionOrSolutionFolder;
    }

    public string Directory { get; }

    public Project Project { get; }

    public ProjectItem ProjectItem { get; }

    public bool IsSolutionOrSolutionFolder { get; }

    public bool IsSolution => IsSolutionOrSolutionFolder && Project == null;

    public bool IsSolutionFolder => IsSolutionOrSolutionFolder && Project != null;

}
#nullable restore