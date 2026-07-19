namespace XrmTools.Shell.Controls;

using XrmTools.Shell.Helpers;
using XrmTools.Shell.Styles;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Media;
using System.Windows.Threading;

public class TextBlock : System.Windows.Controls.TextBlock
{
    private bool hasDeferredLiveRegionChange;
    public static readonly DependencyProperty KindProperty = Property.Register<TextBlock, TextKind>(nameof(Kind));
    public static readonly DependencyProperty PreferredTextFormattingModeProperty = Property.RegisterAttachedInherited<TextBlock, TextFormattingMode?>(nameof(PreferredTextFormattingMode));

    static TextBlock()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TextBlock), new FrameworkPropertyMetadata(typeof(TextBlock)));
        TextProperty.OverrideMetadata(typeof(TextBlock), new FrameworkPropertyMetadata(new PropertyChangedCallback(TextChanged)));
    }

    public TextKind Kind
    {
        get => (TextKind)this.GetValue(KindProperty);
        set => SetValue(KindProperty, Boxes.Box(value));
    }

    public TextFormattingMode? PreferredTextFormattingMode
    {
        get => GetPreferredTextFormattingMode(this);
    }

    public static TextFormattingMode? GetPreferredTextFormattingMode(DependencyObject dependencyObject)
    {
        return (TextFormattingMode?)(dependencyObject?.GetValue(PreferredTextFormattingModeProperty) ?? (object)null);
    }

    public static void SetPreferredTextFormattingMode(
      DependencyObject dependencyObject,
      TextFormattingMode? textFormattingMode)
    {
        dependencyObject?.SetValue(PreferredTextFormattingModeProperty, Boxes.Box(textFormattingMode));
    }

    private static void TextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        TextBlock textBlock = d as TextBlock;
        if (textBlock == null)
            return;
        if (!textBlock.IsLoaded && !textBlock.hasDeferredLiveRegionChange)
        {
            textBlock.hasDeferredLiveRegionChange = true;
            textBlock.Dispatcher.BeginInvoke((() =>
            {
                TextChanged(textBlock, e);
                textBlock.hasDeferredLiveRegionChange = false;
            }), DispatcherPriority.Input);
        }
        else
        {
            if (AutomationProperties.GetLiveSetting(textBlock) == AutomationLiveSetting.Off || !AutomationPeer.ListenerExists(AutomationEvents.LiveRegionChanged) || string.IsNullOrWhiteSpace(e.NewValue?.ToString()))
                return;
            (UIElementAutomationPeer.FromElement(textBlock) ?? UIElementAutomationPeer.CreatePeerForElement(textBlock))?.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);
        }
    }
}

