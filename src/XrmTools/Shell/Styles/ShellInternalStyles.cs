namespace XrmTools.Shell.Styles;

using XrmTools.Shell.Controls;
using System.Windows;

public static class ShellInternalStyles
{
    public static Style ComboBoxToggleButton { get; } = ShellStyles.LoadStyle("ComboBoxToggleButtonStyle.xaml");
    public static Style Button { get; } = (Style)DefaultStyles.Instance[typeof(Button)];

    public static Style HeaderFocusVisual { get; } = ShellStyles.LoadStyle("HeaderFocusVisualStyle.xaml");

    //public static Style ImageButton { get; } = (Style)DefaultStyles.Instance[typeof(ImageButton)];

    public static Style PathButton => (Style)DefaultStyles.Instance[typeof(PathButton)];

    public static Style PillFocusVisual { get; } = ShellStyles.LoadStyle("PillFocusVisualStyle.xaml");
    public static Style TextBox { get; } = (Style)DefaultStyles.Instance[typeof(TextBox)];

    public static Style Window => (Style)DefaultStyles.Instance[typeof(Controls.Window)];
}
