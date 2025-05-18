using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;

namespace XrmTools.UI.Controls
{
    public partial class SyntaxHighlightingControl : UserControl
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(SyntaxHighlightingControl),
        new PropertyMetadata(string.Empty, OnTextChanged));

        public static readonly DependencyProperty ContentTypeProperty =
            DependencyProperty.Register(nameof(ContentType), typeof(string), typeof(SyntaxHighlightingControl),
        new PropertyMetadata(string.Empty, OnContentTypeChanged));

        public static readonly DependencyProperty TextBufferProperty =
            DependencyProperty.Register(nameof(TextBuffer), typeof(ITextBuffer), typeof(SyntaxHighlightingControl),
                new PropertyMetadata(null));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public string ContentType
        {
            get => (string)GetValue(ContentTypeProperty);
            set => SetValue(ContentTypeProperty, value);
        }

        /// <summary>
        /// TextBuffer property should be set in other to support syntax highlighting for languages like C#.
        /// </summary>
        public ITextBuffer TextBuffer
        {
            get => (ITextBuffer)GetValue(TextBufferProperty);
            set => SetValue(TextBufferProperty, value);
        }

        [Import]
        internal ITextEditorFactoryService TextEditorFactoryService { get; set; }

        [Import]
        internal ITextBufferFactoryService TextBufferFactoryService { get; set; }

        [Import]
        internal IClassificationFormatMapService ClassificationFormatMapService { get; set; }

        [Import]
        internal IClassifierAggregatorService ClassifierAggregatorService { get; set; }

        [Import]
        internal IContentTypeRegistryService ContentTypeRegistryService { get; set; }

        public IWpfTextViewHost TextViewHost { get; private set; }

        public SyntaxHighlightingControl()
        {
            InitializeComponent();
            InitializeEditor();
        }

        private void InitializeEditor()
        {
            var componentModel = (IComponentModel)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SComponentModel));
            if (componentModel == null)
            {
                EditorHost.Content = "Design view is not supported because MEF services are not available in design mode.";
                return;
            }
            componentModel.DefaultCompositionService.SatisfyImportsOnce(this);
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SyntaxHighlightingControl control && e.NewValue is string newText)
            {
                if (control.TextViewHost is null) control.ContentType = "JSON";
                control.UpdateText(newText);
            }
        }

        private void UpdateText(string text)
        {
            if (TextViewHost != null)
            {
                var textBuffer = TextViewHost.TextView.TextBuffer;
                using (var edit = textBuffer.CreateEdit())
                {
                    edit.Replace(0, textBuffer.CurrentSnapshot.Length, text ?? string.Empty);
                    edit.Apply();
                }
            }
        }

        private static void OnContentTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SyntaxHighlightingControl control && e.NewValue is string newContentType)
            {
                if (control.TextBuffer == null)
                {
                    var contentType = control.ContentTypeRegistryService.GetContentType(newContentType);
                    control.TextBuffer = control.TextBufferFactoryService.CreateTextBuffer(contentType);
                }
                var textViewRoleSet = control.TextEditorFactoryService.CreateTextViewRoleSet(
                    PredefinedTextViewRoles.Document,
                    PredefinedTextViewRoles.ChangePreview,
                    PredefinedTextViewRoles.Interactive
                );
                var textView = control.TextEditorFactoryService.CreateTextView(control.TextBuffer, textViewRoleSet);
                control.TextViewHost = control.TextEditorFactoryService.CreateTextViewHost(textView, false);
                control.EditorHost.Content = control.TextViewHost.HostControl;
                control.UpdateText(control.Text);
            }
        }
    }
}
