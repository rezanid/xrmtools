namespace XrmTools.Shell.Styles;

using XrmTools.Shell.Controls;
using System.Collections.Generic;

internal static class DefaultStyles
{
    public static DeferredStyleDictionary Instance { get; } = DeferredStyleDictionary.Create(new Dictionary<object, string>()
    {
        {
          typeof (Button),
          "/XrmTools;component/Shell/Styles/ButtonStyle.xaml"
        },
        //{
        //  typeof (Callout),
        //  "/XrmTools;component/Shell/Styles/CalloutStyle.xaml"
        //},
        //{
        //  typeof (CalloutSurface),
        //  "/XrmTools;component/Shell/Styles/CalloutSurfaceStyle.xaml"
        //},
        //{
        //  typeof (CheckableTreeViewItem),
        //  "/XrmTools;component/Shell/Styles/CheckableTreeViewItemStyle.xaml"
        //},
        //{
        //  typeof (CheckBox),
        //  "/XrmTools;component/Shell/Styles/CheckBoxStyle.xaml"
        //},
        //{
        //  typeof (CodeBlock),
        //  "/XrmTools;component/Shell/Styles/MarkdownViewer/CodeBlockStyle.xaml"
        //},
        //{
        //  typeof (CodeInline),
        //  "/XrmTools;component/Shell/Styles/MarkdownViewer/CodeInlineStyle.xaml"
        //},
        //{
        //  typeof (ComboBox),
        //  "/XrmTools;component/Shell/Styles/ComboBoxStyle.xaml"
        //},
        //{
        //  typeof (ComboBoxItem),
        //  "/XrmTools;component/Shell/Styles/ComboBoxItemStyle.xaml"
        //},
        {
          typeof (ContextMenu),
          "/XrmTools;component/Shell/Styles/ContextMenuStyle.xaml"
        },
        //{
        //  typeof (DataGrid),
        //  "/XrmTools;component/Shell/Styles/DataGrid/DataGridStyle.xaml"
        //},
        //{
        //  typeof (DataGridCell),
        //  "/XrmTools;component/Shell/Styles/DataGrid/DataGridCellStyle.xaml"
        //},
        //{
        //  typeof (DataGridCellsPresenter),
        //  "/XrmTools;component/Shell/Styles/DataGrid/DataGridCellsPresenterStyle.xaml"
        //},
        //{
        //  typeof (DataGridColumnHeader),
        //  "/XrmTools;component/Shell/Styles/DataGrid/DataGridColumnHeaderStyle.xaml"
        //},
        //{
        //  typeof (DataGridColumnHeadersPresenter),
        //  "/XrmTools;component/Shell/Styles/DataGrid/DataGridColumnHeadersPresenterStyle.xaml"
        //},
        //{
        //  typeof (DataGridRow),
        //  "/XrmTools;component/Shell/Styles/DataGrid/DataGridRowStyle.xaml"
        //},
        //{
        //  typeof (DataGridThumb),
        //  "/XrmTools;component/Shell/Styles/DataGrid/DataGridThumbStyle.xaml"
        //},
        //{
        //  typeof (DialogButton),
        //  "/XrmTools;component/Shell/Styles/Window/DialogButtonStyle.xaml"
        //},
        //{
        //  typeof (DialogContent),
        //  "/XrmTools;component/Shell/Styles/Window/DialogContentStyle.xaml"
        //},
        //{
        //  typeof (DialogWindow),
        //  "/XrmTools;component/Shell/Styles/Window/DialogWindowStyle.xaml"
        //},
        //{
        //  typeof (DropDownButton),
        //  "/XrmTools;component/Shell/Styles/DropDownButtonStyle.xaml"
        //},
        //{
        //  typeof (EmphasisInline),
        //  "/XrmTools;component/Shell/Styles/MarkdownViewer/EmphasisInlineStyle.xaml"
        //},
        {
          typeof (Expander),
          "/XrmTools;component/Shell/Styles/ExpanderStyle.xaml"
        },
        //{
        //  typeof (FeedbackPanel),
        //  "/XrmTools;component/Shell/Styles/FeedbackPanelStyle.xaml"
        //},
        {
          typeof (FlyoutSurface),
          "/XrmTools;component/Shell/Styles/FlyoutSurfaceStyle.xaml"
        },
        //{
        //  typeof (Heading),
        //  "/XrmTools;component/Shell/Styles/MarkdownViewer/HeadingStyle.xaml"
        //},
        //{
        //  typeof (Hyperlink),
        //  "/XrmTools;component/Shell/Styles/HyperlinkStyle.xaml"
        //},
        //{
        //  typeof (HyperlinkButton),
        //  "/XrmTools;component/Shell/Styles/HyperlinkButtonStyle.xaml"
        //},
        //{
        //  typeof (ImageButton),
        //  "/XrmTools;component/Shell/Styles/ImageButtonStyle.xaml"
        //},
        {
          typeof (Indicator),
          "/XrmTools;component/Shell/Styles/IndicatorStyle.xaml"
        },
        //{
        //  typeof (Label),
        //  "/XrmTools;component/Shell/Styles/LabelStyle.xaml"
        //},
        //{
        //  typeof (List),
        //  "/XrmTools;component/Shell/Styles/MarkdownViewer/ListStyle.xaml"
        //},
        {
          typeof (ListBox),
          "/XrmTools;component/Shell/Styles/ListBoxStyle.xaml"
        },
        {
          typeof (ListBoxItem),
          "/XrmTools;component/Shell/Styles/ListBoxItemStyle.xaml"
        },
        //{
        //  typeof (ListItem),
        //  "/XrmTools;component/Shell/Styles/MarkdownViewer/ListItemStyle.xaml"
        //},
        //{
        //  typeof (MarkdownViewer),
        //  "/XrmTools;component/Shell/Styles/MarkdownViewer/MarkdownViewerStyle.xaml"
        //},
        //{
        //  typeof (MediaHost),
        //  "/XrmTools;component/Shell/Styles/MarkdownViewer/MediaHostStyle.xaml"
        //},
        {
          typeof (MenuItem),
          "/XrmTools;component/Shell/Styles/MenuItemStyle.xaml"
        },
        {
          typeof (MenuScrollViewer),
          "/XrmTools;component/Shell/Styles/MenuScrollViewerStyle.xaml"
        },
        //{
        //  typeof (MessageContent),
        //  "/XrmTools;component/Shell/Styles/Window/MessageContentStyle.xaml"
        //},
        //{
        //  typeof (MessageFooter),
        //  "/XrmTools;component/Shell/Styles/Window/MessageFooterStyle.xaml"
        //},
        //{
        //  typeof (MessageWindow),
        //  "/XrmTools;component/Shell/Styles/Window/MessageWindowStyle.xaml"
        //},
        //{
        //  typeof (NonClientButton),
        //  "/XrmTools;component/Shell/Styles/Window/NonClientButtonStyle.xaml"
        //},
        //{
        //  typeof (Paragraph),
        //  "/XrmTools;component/Shell/Styles/MarkdownViewer/ParagraphStyle.xaml"
        //},
        //{
        //  typeof (PasswordBox),
        //  "/XrmTools;component/Shell/Styles/PasswordBoxStyle.xaml"
        //},
        //{
        //  typeof (PathButton),
        //  "/XrmTools;component/Shell/Styles/PathButtonStyle.xaml"
        //},
        {
          typeof (Popup),
          "/XrmTools;component/Shell/Styles/PopupStyle.xaml"
        },
        //{
        //  typeof (ProgressControl),
        //  "/XrmTools;component/Shell/Styles/ProgressControlStyle.xaml"
        //},
        {
          typeof (RadioButton),
          "/XrmTools;component/Shell/Styles/RadioButtonStyle.xaml"
        },
        {
          typeof (ScrollBar),
          "/XrmTools;component/Shell/Styles/ScrollBarStyle.xaml"
        },
        {
          typeof (ScrollViewer),
          "/XrmTools;component/Shell/Styles/ScrollViewerStyle.xaml"
        },
        //{
        //  typeof (Section),
        //  "/XrmTools;component/Shell/Styles/MarkdownViewer/SectionStyle.xaml"
        //},
        {
          typeof (Separator),
          "/XrmTools;component/Shell/Styles/SeparatorStyle.xaml"
        },
        //{
        //  typeof (SplitButton),
        //  "/XrmTools;component/Shell/Styles/SplitButtonStyle.xaml"
        //},
        {
          typeof (TextBlock),
          "/XrmTools;component/Shell/Styles/TextBlockStyle.xaml"
        },
        {
          typeof (TextBox),
          "/XrmTools;component/Shell/Styles/TextBoxStyle.xaml"
        },
        {
          typeof (ToggleButton),
          "/XrmTools;component/Shell/Styles/ToggleButtonStyle.xaml"
        },
        //{
        //  typeof (ToolTip),
        //  "/XrmTools;component/Shell/Styles/ToolTipStyle.xaml"
        //},
        {
          typeof (TreeView),
          "/XrmTools;component/Shell/Styles/TreeViewStyle.xaml"
        },
        {
          typeof (TreeViewItem),
          "/XrmTools;component/Shell/Styles/TreeViewItemStyle.xaml"
        },
        //{
        //  typeof (Window),
        //  "/XrmTools;component/Shell/Styles/Window/WindowStyle.xaml"
        //}
    });
}