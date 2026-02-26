namespace XrmTools.Shell.Controls;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using XrmTools.Shell.Helpers;

public class ScrollBar : System.Windows.Controls.Primitives.ScrollBar
{
    public static readonly double ScrollThumbSize = 2.0;
    public static readonly double ScrollThumbExpandedSize = 8.0;
    public static readonly double ScrollButtonSize = 16.0;
    public static readonly DependencyProperty AlwaysExpandedProperty = Property.Register<ScrollBar, bool>(nameof(AlwaysExpanded));

    static ScrollBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ScrollBar), new FrameworkPropertyMetadata(typeof(ScrollBar)));
    }

    public bool AlwaysExpanded
    {
        get => (bool)GetValue(AlwaysExpandedProperty);
        set => this.SetValue(AlwaysExpandedProperty, Boxes.Box<bool>(value));
    }

    protected override void OnContextMenuOpening(ContextMenuEventArgs e)
    {
        if (ContextMenu != null)
            return;
        ContextMenu contextMenu;
        switch (Orientation)
        {
            case Orientation.Horizontal:
                contextMenu = CreateHorizontalContextMenu();
                break;
            case Orientation.Vertical:
                contextMenu = CreateVerticalContextMenu();
                break;
            default:
                throw new NotSupportedException(Orientation.ToString());
        }
        ContextMenu = contextMenu;
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

    private ContextMenu CreateHorizontalContextMenu()
    {
        ContextMenu contextMenu = new ContextMenu();
        AddMenuItem(contextMenu, ScrollHereCommand, "Scroll Here");
        contextMenu.Items.Add(new Separator());
        AddMenuItem(contextMenu, ScrollToLeftEndCommand, "Left edge");
        AddMenuItem(contextMenu, ScrollToRightEndCommand, "Right edge");
        contextMenu.Items.Add(new Separator());
        AddMenuItem(contextMenu, PageLeftCommand, "Page left");
        AddMenuItem(contextMenu, PageRightCommand, "Page right");
        contextMenu.Items.Add(new Separator());
        AddMenuItem(contextMenu, LineLeftCommand, "Scroll left");
        AddMenuItem(contextMenu, LineRightCommand, "Scroll right");
        return contextMenu;
    }

    private ContextMenu CreateVerticalContextMenu()
    {
        ContextMenu contextMenu = new ContextMenu();
        AddMenuItem(contextMenu, ScrollHereCommand, "Scroll Here");
        contextMenu.Items.Add(new Separator());
        AddMenuItem(contextMenu, ScrollToTopCommand, "Top");
        AddMenuItem(contextMenu, ScrollToBottomCommand, "Bottom");
        contextMenu.Items.Add(new Separator());
        AddMenuItem(contextMenu, PageUpCommand, "Page Up");
        AddMenuItem(contextMenu, PageDownCommand, "Page Down");
        contextMenu.Items.Add(new Separator());
        AddMenuItem(contextMenu, LineUpCommand, "Scroll Up");
        AddMenuItem(contextMenu, LineDownCommand, "Scroll Down");
        return contextMenu;
    }
}

