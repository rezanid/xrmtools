#nullable enable
namespace XrmTools.Helpers;

using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using System;

internal static class PackageHelper
{
    public static XrmToolsPackage? GetXrmGenPackage()
    {
        if (!ThreadHelper.JoinableTaskContext.IsOnMainThread) return null;
        if (Package.GetGlobalService(typeof(SVsShell)) is not IVsShell shell)
        {
            throw new InvalidOperationException("Cannot get the shell service.");
        }
        ErrorHandler.ThrowOnFailure(shell.IsPackageLoaded(ref PackageGuids.XrmToolsPackageId, out var package));
        if (package == null)
            ErrorHandler.ThrowOnFailure(shell.LoadPackage(ref PackageGuids.XrmToolsPackageId, out package));
        return (XrmToolsPackage)package;
    }

}
#nullable restore