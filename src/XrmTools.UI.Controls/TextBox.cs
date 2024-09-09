namespace XrmTools.UI.Controls
{
    using Microsoft.VisualStudio.PlatformUI;
    using Microsoft.VisualStudio.Shell;
    using System;
    using System.Drawing;
    using System.Windows;
    using System.Windows.Media;

    public class TextBox : System.Windows.Controls.TextBox
    {
        #region PlaceHolder
        public static readonly DependencyProperty PlaceHolderProperty =
            DependencyProperty.Register(
                "PlaceHolder",
                typeof(string),
                typeof(TextBox),
                new PropertyMetadata(default(string)));

        public string PlaceHolder
        {
            get => (string)GetValue(PlaceHolderProperty);
            set => SetValue(PlaceHolderProperty, value);
        }
        #endregion

        #region Is Empty
        private static readonly DependencyPropertyKey IsEmptyPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "IsEmpty",
                typeof(bool),
                typeof(TextBox),
                new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty IsEmptyProperty = IsEmptyPropertyKey.DependencyProperty;

        public bool IsEmpty
        {
            get => (bool)GetValue(IsEmptyProperty);
            private set => SetValue(IsEmptyPropertyKey, value);
        }
        #endregion

        #region PlaceHolderTemplate
        public static readonly DependencyProperty PlaceHolderTemplateProperty =
            DependencyProperty.Register(
                "PlaceHolderTemplate",
                typeof(DataTemplate),
                typeof(TextBox),
                new PropertyMetadata(default(DataTemplate)));

        public DataTemplate PlaceHolderTemplate
        {
            get => (DataTemplate)GetValue(PlaceHolderTemplateProperty);
            set => SetValue(PlaceHolderTemplateProperty, value);
        }
        #endregion

        static TextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TextBox), new FrameworkPropertyMetadata(typeof(TextBox)));
        }

        protected override void OnTextChanged(System.Windows.Controls.TextChangedEventArgs e)
        {
            IsEmpty = string.IsNullOrEmpty(Text);
            base.OnTextChanged(e);
        }

        protected override void OnInitialized(EventArgs e)
        {
            IsEmpty = string.IsNullOrEmpty(Text);
            base.OnInitialized(e);
        }
    }
}