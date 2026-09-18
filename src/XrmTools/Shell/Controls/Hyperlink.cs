namespace XrmTools.Shell.Controls;

using System.Windows;
using System.Windows.Documents;

/// <summary>A theme-aware action hyperlink, using the Shell's foreground and interaction states.</summary>
public class Hyperlink : System.Windows.Documents.Hyperlink
{
    static Hyperlink() => DefaultStyleKeyProperty.OverrideMetadata(typeof(Hyperlink), new FrameworkPropertyMetadata(typeof(Hyperlink)));
    public Hyperlink() { }
    public Hyperlink(Inline child) : base(child) { }
}
