namespace XrmTools.Shell.Styles;

using XrmTools.Shell.Controls;
using System.Collections.Generic;

internal static class DefaultStyles
{
    public static DeferredStyleDictionary Instance { get; } = DeferredStyleDictionary.Create(new Dictionary<object, string>()
    {
        //{
        //  (object) typeof (Button),
        //  "/XrmTools;component/Shell/Styles/ButtonStyle.xaml"
        //},
        //{
        //  (object) typeof (Callout),
        //  "/XrmTools;component/Shell/Styles/CalloutStyle.xaml"
        //},
        //{
        //  (object) typeof (CalloutSurface),
        //  "/XrmTools;component/Shell/Styles/CalloutSurfaceStyle.xaml"
        //},
        //{
        //  (object) typeof (CheckableTreeViewItem),
        //  "/XrmTools;component/Shell/Styles/CheckableTreeViewItemStyle.xaml"
        //},
        //{
        //  (object) typeof (CheckBox),
        //  "/XrmTools;component/Shell/Styles/CheckBoxStyle.xaml"
        //},
        //{
        //  (object) typeof (CodeBlock),
        //  "/XrmTools;component/Shell/Styles/MarkdownViewer/CodeBlockStyle.xaml"
        //},
        //{
        //  (object) typeof (CodeInline),
        //  "/XrmTools;component/Shell/Styles/MarkdownViewer/CodeInlineStyle.xaml"
        //},
        //{
        //  (object) typeof (ComboBox),
        //  "/XrmTools;component/Shell/Styles/ComboBoxStyle.xaml"
        //},
        //{
        //  (object) typeof (ComboBoxItem),
        //  "/XrmTools;component/Shell/Styles/ComboBoxItemStyle.xaml"
        //},
        //{
        //  (object) typeof (ContextMenu),
        //  "/XrmTools;component/Shell/Styles/ContextMenuStyle.xaml"
        //},
        //{
        //  (object) typeof (DataGrid),
        //  "/XrmTools;component/Shell/Styles/DataGrid/DataGridStyle.xaml"
        //},
        //{
        //  (object) typeof (DataGridCell),
        //  "/XrmTools;component/Shell/Styles/DataGrid/DataGridCellStyle.xaml"
        //},
        //{
        //  (object) typeof (DataGridCellsPresenter),
        //  "/XrmTools;component/Shell/Styles/DataGrid/DataGridCellsPresenterStyle.xaml"
        //},
        //{
        //  (object) typeof (DataGridColumnHeader),
        //  "/XrmTools;component/Shell/Styles/DataGrid/DataGridColumnHeaderStyle.xaml"
        //},
        //{
        //  (object) typeof (DataGridColumnHeadersPresenter),
        //  "/XrmTools;component/Shell/Styles/DataGrid/DataGridColumnHeadersPresenterStyle.xaml"
        //},
        //{
        //  (object) typeof (DataGridRow),
        //  "/XrmTools;component/Shell/Styles/DataGrid/DataGridRowStyle.xaml"
        //},
        //{
        //  (object) typeof (DataGridThumb),
        //  "/XrmTools;component/Shell/Styles/DataGrid/DataGridThumbStyle.xaml"
        //},
        //{
        //  (object) typeof (DialogButton),
        //  "/XrmTools;component/Shell/Styles/Window/DialogButtonStyle.xaml"
        //},
        //{
        //  (object) typeof (DialogContent),
        //  "/XrmTools;component/Shell/Styles/Window/DialogContentStyle.xaml"
        //},
        //{
        //  (object) typeof (DialogWindow),
        //  "/XrmTools;component/Shell/Styles/Window/DialogWindowStyle.xaml"
        //},
        //{
        //  (object) typeof (DropDownButton),
        //  "/XrmTools;component/Shell/Styles/DropDownButtonStyle.xaml"
        //},
        //{
        //  (object) typeof (EmphasisInline),
        //  "/XrmTools;component/Shell/Styles/MarkdownViewer/EmphasisInlineStyle.xaml"
        //},
        {
          (object) typeof (Expander),
          "/XrmTools;component/Shell/Styles/ExpanderStyle.xaml"
        },
        //{
        //  (object) typeof (FeedbackPanel),
        //  "/XrmTools;component/Shell/Styles/FeedbackPanelStyle.xaml"
        //},
        //{
        //  (object) typeof (FlyoutSurface),
        //  "/XrmTools;component/Shell/Styles/FlyoutSurfaceStyle.xaml"
        //},
        //{
        //  (object) typeof (Heading),
        //  "/XrmTools;component/Shell/Styles/MarkdownViewer/HeadingStyle.xaml"
        //},
        //{
        //  (object) typeof (Hyperlink),
        //  "/XrmTools;component/Shell/Styles/HyperlinkStyle.xaml"
        //},
        //{
        //  (object) typeof (HyperlinkButton),
        //  "/XrmTools;component/Shell/Styles/HyperlinkButtonStyle.xaml"
        //},
        //{
        //  (object) typeof (ImageButton),
        //  "/XrmTools;component/Shell/Styles/ImageButtonStyle.xaml"
        //},
        {
          (object) typeof (Indicator),
          "/XrmTools;component/Shell/Styles/IndicatorStyle.xaml"
        },
        //{
        //  (object) typeof (Label),
        //  "/XrmTools;component/Shell/Styles/LabelStyle.xaml"
        //},
        //{
        //  (object) typeof (List),
        //  "/XrmTools;component/Shell/Styles/MarkdownViewer/ListStyle.xaml"
        //},
        //{
        //  (object) typeof (ListBox),
        //  "/XrmTools;component/Shell/Styles/ListBoxStyle.xaml"
        //},
        {
          (object) typeof (ListBoxItem),
          "/XrmTools;component/Shell/Styles/ListBoxItemStyle.xaml"
        },
        //{
        //  (object) typeof (ListItem),
        //  "/XrmTools;component/Shell/Styles/MarkdownViewer/ListItemStyle.xaml"
        //},
        //{
        //  (object) typeof (MarkdownViewer),
        //  "/XrmTools;component/Shell/Styles/MarkdownViewer/MarkdownViewerStyle.xaml"
        //},
        //{
        //  (object) typeof (MediaHost),
        //  "/XrmTools;component/Shell/Styles/MarkdownViewer/MediaHostStyle.xaml"
        //},
        //{
        //  (object) typeof (MenuItem),
        //  "/XrmTools;component/Shell/Styles/MenuItemStyle.xaml"
        //},
        //{
        //  (object) typeof (MenuScrollViewer),
        //  "/XrmTools;component/Shell/Styles/MenuScrollViewerStyle.xaml"
        //},
        //{
        //  (object) typeof (MessageContent),
        //  "/XrmTools;component/Shell/Styles/Window/MessageContentStyle.xaml"
        //},
        //{
        //  (object) typeof (MessageFooter),
        //  "/XrmTools;component/Shell/Styles/Window/MessageFooterStyle.xaml"
        //},
        //{
        //  (object) typeof (MessageWindow),
        //  "/XrmTools;component/Shell/Styles/Window/MessageWindowStyle.xaml"
        //},
        //{
        //  (object) typeof (NonClientButton),
        //  "/XrmTools;component/Shell/Styles/Window/NonClientButtonStyle.xaml"
        //},
        //{
        //  (object) typeof (Paragraph),
        //  "/XrmTools;component/Shell/Styles/MarkdownViewer/ParagraphStyle.xaml"
        //},
        //{
        //  (object) typeof (PasswordBox),
        //  "/XrmTools;component/Shell/Styles/PasswordBoxStyle.xaml"
        //},
        //{
        //  (object) typeof (PathButton),
        //  "/XrmTools;component/Shell/Styles/PathButtonStyle.xaml"
        //},
        //{
        //  (object) typeof (Popup),
        //  "/XrmTools;component/Shell/Styles/PopupStyle.xaml"
        //},
        //{
        //  (object) typeof (ProgressControl),
        //  "/XrmTools;component/Shell/Styles/ProgressControlStyle.xaml"
        //},
        //{
        //  (object) typeof (RadioButton),
        //  "/XrmTools;component/Shell/Styles/RadioButtonStyle.xaml"
        //},
        {
          (object) typeof (ScrollBar),
          "/XrmTools;component/Shell/Styles/ScrollBarStyle.xaml"
        },
        {
          (object) typeof (ScrollViewer),
          "/XrmTools;component/Shell/Styles/ScrollViewerStyle.xaml"
        },
        //{
        //  (object) typeof (Section),
        //  "/XrmTools;component/Shell/Styles/MarkdownViewer/SectionStyle.xaml"
        //},
        //{
        //  (object) typeof (Separator),
        //  "/XrmTools;component/Shell/Styles/SeparatorStyle.xaml"
        //},
        //{
        //  (object) typeof (SplitButton),
        //  "/XrmTools;component/Shell/Styles/SplitButtonStyle.xaml"
        //},
        //{
        //  (object) typeof (TextBlock),
        //  "/XrmTools;component/Shell/Styles/TextBlockStyle.xaml"
        //},
        //{
        //  (object) typeof (TextBox),
        //  "/XrmTools;component/Shell/Styles/TextBoxStyle.xaml"
        //},
        {
          (object) typeof (ToggleButton),
          "/XrmTools;component/Shell/Styles/ToggleButtonStyle.xaml"
        },
        //{
        //  (object) typeof (ToolTip),
        //  "/XrmTools;component/Shell/Styles/ToolTipStyle.xaml"
        //},
        {
          (object) typeof (TreeView),
          "/XrmTools;component/Shell/Styles/TreeViewStyle.xaml"
        },
        {
          (object) typeof (TreeViewItem),
          "/XrmTools;component/Shell/Styles/TreeViewItemStyle.xaml"
        },
        //{
        //  (object) typeof (Window),
        //  "/XrmTools;component/Shell/Styles/Window/WindowStyle.xaml"
        //}
    });
}