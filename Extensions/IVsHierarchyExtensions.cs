﻿using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio;
using System.Runtime.InteropServices;
using System;

namespace XrmGen.Extensions;
internal static class IVsHierarchyExtensions
{
    public static string GetPropertyForProject(this IVsHierarchy hierarchy, string propertyName)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        if (hierarchy is IVsBuildPropertyStorage propertyStorage)
        {
            propertyStorage.GetPropertyValue(propertyName, null, (uint)_PersistStorageType.PST_PROJECT_FILE, out var value);
            return value;
        }
        return string.Empty;
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

}
