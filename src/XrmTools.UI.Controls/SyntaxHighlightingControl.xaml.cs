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

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        [Import]
        internal ITextEditorFactoryService TextEditorFactoryService { get; set; }

        [Import]
        internal ITextBufferFactoryService TextBufferFactoryService { get; set; }

        [Import]
        internal IClassificationFormatMapService ClassificationFormatMapService { get; set; }

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
                EditorHost.Content = "Design view is not supported because MEF services not available in design mode.";
                return;
            }
            componentModel.DefaultCompositionService.SatisfyImportsOnce(this);

            var contentType = ContentTypeRegistryService.GetContentType("JSON");
            var textBuffer = TextBufferFactoryService.CreateTextBuffer(contentType);
            var textViewRoleSet = TextEditorFactoryService.CreateTextViewRoleSet("Interactive");
            var textView = TextEditorFactoryService.CreateTextView(textBuffer, textViewRoleSet);
            TextViewHost = TextEditorFactoryService.CreateTextViewHost(textView, false);

            // Add the editor to the UI
            EditorHost.Content = TextViewHost.HostControl;
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SyntaxHighlightingControl control && e.NewValue is string newText)
            {
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
    }
}
