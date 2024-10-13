#nullable enable
namespace XrmTools;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio;
using System;
using System.Runtime.InteropServices;
using XrmTools.Options;
using XrmTools.Helpers;
using System.IO;
using System.Linq;

[ProvideSolutionProperties(SolutionPersistanceKey)]
public sealed partial class XrmToolsPackage : IVsPersistSolutionProps
{
    #region Solution User Options
    protected override void OnLoadOptions(string key, Stream stream)
    {
        if (SettingsProvider.SolutionUserSettings.Keys.Contains(key, StringComparer.InvariantCultureIgnoreCase))
        {
            SettingsProvider.SolutionUserSettings.Set(key, new StreamReader(stream).ReadToEnd());
        }
    }
    protected override void OnSaveOptions(string key, Stream stream)
    {
        if (SettingsProvider.SolutionUserSettings.Keys.Contains(key, StringComparer.InvariantCultureIgnoreCase))
        {
            var writer = new StreamWriter(stream);
            writer.Write(SettingsProvider.SolutionUserSettings.Get(key));
            writer.Flush();
        }
    }
    #endregion
    #region Solution User Options (Already implemented in Package)
    public int SaveUserOptions(IVsSolutionPersistence pPersistence)
        => GeneralOptions.Instance.CurrentEnvironmentStorage != SettingsStorageTypes.SolutionUser ? VSConstants.S_OK : implicitInvoker.Invoke<int>(this, nameof(SaveUserOptions), pPersistence);
    public int LoadUserOptions(IVsSolutionPersistence pPersistence, uint grfLoadOpts)
        => GeneralOptions.Instance.CurrentEnvironmentStorage != SettingsStorageTypes.SolutionUser ? VSConstants.S_OK : implicitInvoker.Invoke<int>(this, nameof(LoadUserOptions), pPersistence, grfLoadOpts);
    public int WriteUserOptions(IStream pOptionsStream, string pszKey)
        => GeneralOptions.Instance.CurrentEnvironmentStorage != SettingsStorageTypes.SolutionUser ? VSConstants.S_OK : implicitInvoker.Invoke<int>(this, nameof(WriteUserOptions), pOptionsStream, pszKey);
    public int ReadUserOptions(IStream pOptionsStream, string pszKey)
        => GeneralOptions.Instance.CurrentEnvironmentStorage != SettingsStorageTypes.SolutionUser ? VSConstants.S_OK : implicitInvoker.Invoke<int>(this, nameof(ReadUserOptions), pOptionsStream, pszKey);
    #endregion
    #region Solution Properties
    public int QuerySaveSolutionProps(IVsHierarchy pHierarchy, VSQUERYSAVESLNPROPS[] pqsspSave)
    {
        // This function is called by the IDE to determine if something needs to be saved in the solution.
        // If the package returns that it has dirty properties, the shell will callback on SaveSolutionProps.
        if (pHierarchy == null)
        {
            pqsspSave[0] = !SettingsProvider.SolutionSettings.IsDirty ? VSQUERYSAVESLNPROPS.QSP_HasNoDirtyProps : VSQUERYSAVESLNPROPS.QSP_HasDirtyProps;
        }
        return VSConstants.S_OK;
    }
    public int SaveSolutionProps(IVsHierarchy pHierarchy, IVsSolutionPersistence pPersistence)
    {
        // This function gets called by the shell after QuerySaveSolutionProps returned QSP_HasDirtyProps.

        // The package will pass in the key under which it wants to save its properties, 
        // and the IDE will call back on WriteSolutionProps.

        // The properties will be saved in the Pre-Load section. When the solution will be reopened,
        // the IDE will call our package to load them back before the projects in the solution are actually open.
        if (GeneralOptions.Instance.CurrentEnvironmentStorage != SettingsStorageTypes.Solution)
        {
            return VSConstants.S_OK;
        }
        // Register to save our section in the solution
        //return pPersistence.SavePackageSolutionProps(1 /* =True */, pHierarchy, this, SolutionPersistanceKey);
        return pPersistence.SavePackageSolutionProps(1 /* =True */, null, this, SolutionPersistanceKey);
    }
    public int WriteSolutionProps(IVsHierarchy pHierarchy, string pszKey, IPropertyBag pPropBag)
    {
        if (pHierarchy != null)
        {
            // Not send by our code!
            return VSConstants.S_OK;
        }
        if (pPropBag == null)
        {
            return VSConstants.E_POINTER;
        }
        if (GeneralOptions.Instance.CurrentEnvironmentStorage != SettingsStorageTypes.Solution)
        {
            return VSConstants.S_OK;
        }

        ThreadHelper.ThrowIfNotOnUIThread();
        if (pszKey == SolutionPersistanceKey)
        {
            try
            {
                foreach (var settingKey in SettingsProvider.SolutionSettings.Keys)
                {
                    pPropBag.Write(settingKey, SettingsProvider.SolutionSettings.Get(settingKey));
                }
                SettingsProvider.SolutionSettings.IsDirty = false;
            }
            catch (Exception ex)
            {
                return Marshal.GetHRForException(ex);
            }
        }
        return VSConstants.S_OK;
    }
    public int ReadSolutionProps(IVsHierarchy pHierarchy, string pszProjectName, string pszProjectMk, string pszKey, int fPreLoad, IPropertyBag pPropBag)
    {
        if (pHierarchy != null)
        {
            return VSConstants.S_OK;
        }
        if (GeneralOptions.Instance.CurrentEnvironmentStorage != SettingsStorageTypes.Solution)
        {
            return VSConstants.S_OK;
        }
        ThreadHelper.ThrowIfNotOnUIThread();
        if (pszKey == SolutionPersistanceKey)
        {
            try
            {
                // Create a placeholder for the property value
                // VARTYPE for a string (VT_BSTR in COM) is 8, and we don't need an error log or unknown object
                //pPropBag.Read(pszKey, out var propValue, null, 8, null);
                foreach (var settingKey in SettingsProvider.SolutionSettings.Keys)
                {
                    pPropBag.Read(settingKey, out var propValue, null, 8, null);
                    // Store the value
                    SettingsProvider.SolutionSettings.Set(settingKey, propValue as string);
                }
                SettingsProvider.ProjectSettings.IsDirty = false;
            }
            catch (Exception ex)
            {
                return Marshal.GetHRForException(ex);
            }
        }
        return VSConstants.S_OK;
    }
    public int OnProjectLoadFailure(IVsHierarchy pStubHierarchy, string pszProjectName, string pszProjectMk, string pszKey)
        => VSConstants.S_OK;
    #endregion
}
#nullable restore