#nullable enable
namespace XrmTools.Shell.Controls;

using XrmTools.Shell.Helpers;
using System.Windows;
using System.Windows.Controls;


[TemplatePart(Name = PART_Border, Type = typeof(Border))]
public class TreeViewItem : System.Windows.Controls.TreeViewItem
{
    private const string PART_Border = "PART_Border";
    private Border? border;
    public static readonly DependencyProperty BringHeaderIntoViewOnSelectionProperty = Property.Register<TreeViewItem, bool>(nameof(BringHeaderIntoViewOnSelection));
    private static readonly DependencyPropertyKey HeaderHeightPropertyKey = Property.RegisterReadOnly<TreeViewItem, double>(nameof(HeaderHeight));
    public static readonly DependencyProperty HeaderHeightProperty = HeaderHeightPropertyKey.DependencyProperty;
    private static readonly DependencyPropertyKey LevelPropertyKey = Property.RegisterReadOnlyFull<TreeViewItem, int>(nameof(Level));
    public static readonly DependencyProperty LevelProperty = LevelPropertyKey.DependencyProperty;

    static TreeViewItem() => DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeViewItem), new FrameworkPropertyMetadata(typeof(TreeViewItem)));

    public TreeViewItem() => RequestBringIntoView += new RequestBringIntoViewEventHandler(OnRequestBringIntoView);

    public bool BringHeaderIntoViewOnSelection
    {
        get => (bool)GetValue(BringHeaderIntoViewOnSelectionProperty);
        set
        {
            SetValue(BringHeaderIntoViewOnSelectionProperty, Boxes.Box<bool>(value));
        }
    }

    public double HeaderHeight
    {
        get => (double)GetValue(HeaderHeightProperty);
        private set => SetValue(HeaderHeightPropertyKey, Boxes.Box<double>(value));
    }

    public int Level
    {
        get => (int)GetValue(LevelProperty);
        private set => SetValue(LevelPropertyKey, Boxes.Box<int>(value));
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new TreeViewItem();
    }

    protected override bool IsItemItsOwnContainerOverride(object item) => item is TreeViewItem;

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        border = (Border)GetTemplateChild(PART_Border);
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        if (border?.ActualHeight is not null) HeaderHeight = border.ActualHeight;
    }

    protected override void OnVisualParentChanged(DependencyObject oldParent)
    {
        base.OnVisualParentChanged(oldParent);
        int num;
        switch (ItemsControlFromItemContainer(this))
        {
            case TreeView _:
                num = 1;
                break;
            case TreeViewItem treeViewItem:
                num = treeViewItem.Level + 1;
                break;
            default:
                num = 0;
                break;
        }
        Level = num;
    }

    private void OnRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
    {
        if (BringHeaderIntoViewOnSelection || e.TargetRect != Rect.Empty || !(e.TargetObject is FrameworkElement targetObject))
            return;
        Rect targetRectangle = new Rect(TranslatePoint(new Point(0.0, 0.0), targetObject).X, 0.0, targetObject.ActualWidth, targetObject.ActualHeight);
        targetObject.BringIntoView(targetRectangle);
        e.Handled = true;
    }
}
