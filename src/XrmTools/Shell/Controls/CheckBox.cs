namespace XrmTools.Shell.Controls;
using System.Windows;

public class CheckBox : System.Windows.Controls.CheckBox
{
    static CheckBox() => DefaultStyleKeyProperty.OverrideMetadata(typeof(CheckBox), new FrameworkPropertyMetadata(typeof(CheckBox)));
}
