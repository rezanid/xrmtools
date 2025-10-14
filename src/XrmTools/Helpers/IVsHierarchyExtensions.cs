#nullable enable
namespace XrmTools.Helpers;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

internal static class IVsHierarchyExtensions
{
    public static string? GetProjectProperty(this IVsHierarchy hierarchy, string propertyName)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        if (hierarchy is IVsBuildPropertyStorage propertyStorage)
        {
            propertyStorage.GetPropertyValue(propertyName, null, (uint)_PersistStorageType.PST_PROJECT_FILE, out var value);
            return value;
        }
        return null;
    }

    public static bool TryGetHierarchyAndItemID(this IVsRunningDocumentTable runningDocumentTable, string filePath, out IVsHierarchy hierarchy, out uint itemID)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        hierarchy = null;
        itemID = VSConstants.VSITEMID_NIL;

        IntPtr docData = IntPtr.Zero;
        try
        {
            runningDocumentTable.FindAndLockDocument((uint)_VSRDTFLAGS.RDT_NoLock, filePath, out hierarchy, out itemID, out docData, out var docCookie);
            return hierarchy != null;
        }
        finally
        {
            if (docData != IntPtr.Zero)
            {
                Marshal.Release(docData);
            }
        }
    }

    /// <summary>
    /// Selects the given hierarchy/itemId in Solution Explorer.
    /// Pass the item-specific hierarchy if you have both (hierarchy & itemHierarchy).
    /// </summary>
    public static async Task SelectInSolutionExplorerAsync(this IVsHierarchy hierarchy, uint itemId)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        if (hierarchy == null)
            return;

        // Ensure Solution Explorer is created/shown
        var uiShellObj = await AsyncServiceProvider.GlobalProvider.GetServiceAsync(typeof(SVsUIShell));
        var seGuid = VSConstants.StandardToolWindows.SolutionExplorer;
        IVsWindowFrame? frame = null;
        if (uiShellObj is IVsUIShell uiShell)
        {
            uiShell.FindToolWindow((uint)__VSFINDTOOLWIN.FTW_fForceCreate, ref seGuid, out frame);
        }

        if (frame == null)
            return;

        frame.Show();

        // Get the IVsUIHierarchyWindow behind Solution Explorer
        frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out object docView);
        if (docView is not IVsUIHierarchyWindow hierarchyWindow)
            return;

        // Select (optionally ensure visible / expand)
        if (hierarchy is IVsUIHierarchy uiHierarchy)
        {
            hierarchyWindow.ExpandItem(uiHierarchy, itemId, EXPANDFLAGS.EXPF_SelectItem);
        }
    }
}
#nullable restore