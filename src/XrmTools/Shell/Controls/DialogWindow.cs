#nullable enable
namespace XrmTools.Shell.Controls;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Windows;
using System.Windows.Interop;

/// <summary>A Shell window that enters VS modal state and uses the current VS dialog owner.</summary>
public class DialogWindow : Window
{
    static DialogWindow() => DefaultStyleKeyProperty.OverrideMetadata(typeof(DialogWindow), new FrameworkPropertyMetadata(typeof(DialogWindow)));

    public bool? ShowModal()
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        var shell = ServiceProvider.GlobalProvider.GetService(typeof(SVsUIShell)) as IVsUIShell;
        var helper = new WindowInteropHelper(this);
        if (helper.Owner == IntPtr.Zero && shell != null)
        {
            ErrorHandler.ThrowOnFailure(shell.GetDialogOwnerHwnd(out var owner));
            helper.Owner = owner;
        }
        WindowStartupLocation = helper.Owner == IntPtr.Zero ? WindowStartupLocation.CenterScreen : WindowStartupLocation.CenterOwner;
        bool disabledModeless = false;
        try
        {
            if (shell != null)
            {
                ErrorHandler.ThrowOnFailure(shell.EnableModeless(0));
                disabledModeless = true;
            }
            return base.ShowDialog();
        }
        finally
        {
            if (disabledModeless)
                shell!.EnableModeless(1);
        }
    }
}
