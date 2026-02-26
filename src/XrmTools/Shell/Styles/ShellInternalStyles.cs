namespace XrmTools.Shell.Styles;

using XrmTools.Shell.Controls;
using System.Windows;

public static class ShellInternalStyles
{
    //public static Style ButtonStyle { get; } = (Style)DefaultStyles.Instance[(object)typeof(Button)];

    public static Style HeaderFocusVisual { get; } = ShellStyles.LoadStyle("HeaderFocusVisualStyle.xaml");

    //public static Style PathButtonStyle { get; } = (Style)DefaultStyles.Instance[(object)typeof(PathButton)];

    //public static Style TextBoxStyle { get; } = (Style)DefaultStyles.Instance[(object)typeof(TextBox)];

    //public static Style WindowStyle { get; } = (Style)DefaultStyles.Instance[(object)typeof(Microsoft.VisualStudio.Shell.Controls.Window)];
}
