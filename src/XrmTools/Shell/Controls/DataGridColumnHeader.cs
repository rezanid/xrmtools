namespace XrmTools.Shell.Controls;

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using XrmTools.Shell.Helpers;

[TemplatePart(Name = HeaderOptionsPart, Type = typeof(DropDownButton))]
[TemplatePart(Name = RightHeaderGripperPart, Type = typeof(DataGridThumb))]
internal class DataGridColumnHeader : System.Windows.Controls.Primitives.DataGridColumnHeader
{
    private const string HeaderOptionsPart = "PART_HeaderOptions";
    private const string RightHeaderGripperPart = "PART_RightHeaderGripper";
    private DropDownButton? dropDownButton;
    private DataGridThumb? thumb;

    public static readonly DependencyProperty CanUserReorderProperty = Property.Register<DataGridColumnHeader, bool>(nameof(CanUserReorder));
    public static readonly DependencyProperty CanUserResizeProperty = Property.Register<DataGridColumnHeader, bool>(nameof(CanUserResize));
    public static readonly DependencyProperty ColumnsCountProperty = Property.Register<DataGridColumnHeader, int>(nameof(ColumnsCount));

    static DataGridColumnHeader()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DataGridColumnHeader), new FrameworkPropertyMetadata(typeof(DataGridColumnHeader)));
        MoveLeftCommand = new HeaderCommand(header => header.CanUserReorder && header.DisplayIndex > 0, header => header.Move(-1));
        MoveRightCommand = new HeaderCommand(header => header.CanUserReorder && header.DisplayIndex < header.ColumnsCount - 1, header => header.Move(1));
        ResizeCommand = new HeaderCommand(header => header.CanUserResize, header => header.Resize());
        SortAscendingCommand = new HeaderCommand(header => header.CanUserSort, header => header.Sort(ListSortDirection.Ascending));
        SortDescendingCommand = new HeaderCommand(header => header.CanUserSort, header => header.Sort(ListSortDirection.Descending));
    }

    public static ICommand MoveLeftCommand { get; }
    public static ICommand MoveRightCommand { get; }
    public static ICommand ResizeCommand { get; }
    public static ICommand SortAscendingCommand { get; }
    public static ICommand SortDescendingCommand { get; }

    public bool CanUserReorder
    {
        get => (bool)GetValue(CanUserReorderProperty);
        set => SetValue(CanUserReorderProperty, Boxes.Box(value));
    }

    public bool CanUserResize
    {
        get => (bool)GetValue(CanUserResizeProperty);
        set => SetValue(CanUserResizeProperty, Boxes.Box(value));
    }

    public int ColumnsCount
    {
        get => (int)GetValue(ColumnsCountProperty);
        set => SetValue(ColumnsCountProperty, Boxes.Box(value));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        dropDownButton = GetTemplateChild(HeaderOptionsPart) as DropDownButton;
        thumb = GetTemplateChild(RightHeaderGripperPart) as DataGridThumb;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        if (thumb?.IsSingleActionDragging == true)
        {
            if (key is Key.Return or Key.Escape)
            {
                e.Handled = true;
                thumb.EndSingleActionDrag(key == Key.Escape);
            }
            return;
        }

        if ((CanUserReorder || CanUserResize || CanUserSort) && key == Key.Down && e.KeyboardDevice.Modifiers == ModifierKeys.Alt)
        {
            dropDownButton?.ShowDropDown();
            e.Handled = true;
            return;
        }

        base.OnKeyDown(e);
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        switch (e.Property.Name)
        {
            case nameof(CanUserReorder):
            case nameof(ColumnsCount):
            case nameof(DisplayIndex):
                ((HeaderCommand)MoveLeftCommand).RaiseCanExecuteChanged();
                ((HeaderCommand)MoveRightCommand).RaiseCanExecuteChanged();
                break;
            case nameof(CanUserResize):
                ((HeaderCommand)ResizeCommand).RaiseCanExecuteChanged();
                break;
            case nameof(CanUserSort):
                ((HeaderCommand)SortAscendingCommand).RaiseCanExecuteChanged();
                ((HeaderCommand)SortDescendingCommand).RaiseCanExecuteChanged();
                break;
        }
    }

    private void Move(int direction) => Column.DisplayIndex += direction;

    private void Resize()
    {
        Focus();
        thumb?.StartSingleActionDrag();
    }

    private void Sort(ListSortDirection direction)
    {
        Column.SortDirection = direction == ListSortDirection.Ascending
            ? ListSortDirection.Descending
            : ListSortDirection.Ascending;
        OnClick();
    }

    private sealed class HeaderCommand(Func<DataGridColumnHeader, bool> canExecute, Action<DataGridColumnHeader> execute) : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object parameter) => parameter is DataGridColumnHeader header && canExecute(header);

        public void Execute(object parameter)
        {
            if (parameter is DataGridColumnHeader header) execute(header);
        }

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
