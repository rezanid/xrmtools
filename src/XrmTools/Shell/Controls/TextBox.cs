namespace XrmTools.Shell.Controls;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using XrmTools.Shell.Styles;
using XrmTools.Shell.Helpers;

public class TextBox : System.Windows.Controls.TextBox
{
    public static readonly double ToolTipOffset = 5.0;
    public static readonly double TypingIndicatorHeightRest = 1.0;
    public static readonly double TypingIndicatorHeightTyping = 2.0;
    public static readonly DependencyProperty CornerRadiusProperty = Property.Register<TextBox, CornerRadius>(nameof(CornerRadius));
    public static readonly DependencyProperty HintTextProperty = Property.Register<TextBox, string>(nameof(HintText));

    static TextBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TextBox), new FrameworkPropertyMetadata(typeof(TextBox)));
    }

    public TextBox()
    {
        SetResourceReference(SelectionBrushProperty, ShellColors.AccentFillSelectedTextBackgroundBrushKey);
    }

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, Boxes.Box(value));
    }

    public string HintText
    {
        get => (string)GetValue(HintTextProperty);
        set => SetValue(HintTextProperty, value);
    }

    protected override void OnContextMenuOpening(ContextMenuEventArgs e)
    {
        if (ContextMenu != null)
            return;
        ContextMenu = CreateContextMenu();
    }

    private void AddMenuItem(ContextMenu contextMenu, ICommand command, string header)
    {
        ItemCollection items = contextMenu.Items;
        MenuItem newItem = new MenuItem
        {
            Command = command,
            CommandTarget = this,
            Header = header
        };
        items.Add(newItem);
    }

    private ContextMenu CreateContextMenu()
    {
        ContextMenu contextMenu = new ContextMenu();
        AddMenuItem(contextMenu, ApplicationCommands.Cut, "Cut");
        AddMenuItem(contextMenu, ApplicationCommands.Copy, "Copy");
        AddMenuItem(contextMenu, ApplicationCommands.Paste, "Paste");
        return contextMenu;
    }
}