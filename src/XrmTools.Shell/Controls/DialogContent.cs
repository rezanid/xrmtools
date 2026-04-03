namespace XrmTools.Shell.Controls;

using Microsoft.VisualStudio.Imaging.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using XrmTools.Shell.Helpers;

internal class DialogContent : ContentControl
{
    public static readonly DependencyProperty MonikerProperty = Property.RegisterFull<DialogContent, ImageMoniker>(nameof(Moniker));
    public static readonly DependencyProperty TitleProperty = Property.RegisterFull<DialogContent, string>(nameof(Title));

    static DialogContent()
    {
        FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(DialogContent), (PropertyMetadata)new FrameworkPropertyMetadata((object)typeof(DialogContent)));
    }

    public ImageMoniker Moniker
    {
        get => (ImageMoniker)this.GetValue(DialogContent.MonikerProperty);
        set => this.SetValue(DialogContent.MonikerProperty, (object)value);
    }

    public string Title
    {
        get => (string)this.GetValue(DialogContent.TitleProperty);
        set => this.SetValue(DialogContent.TitleProperty, (object)value);
    }
}