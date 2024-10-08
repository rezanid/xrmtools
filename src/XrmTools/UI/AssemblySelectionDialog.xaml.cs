﻿#nullable enable
namespace XrmGen.UI;

using Microsoft.VisualStudio.PlatformUI;
using System.Windows;
using XrmGen.Xrm;
using System.Reflection;
using System.Linq;
using System;

public partial class AssemblySelectionDialog : DialogWindow
{
    public AssemblySelectionDialog(IXrmSchemaProvider schemaProvider)
    {
        EnsureReferencedAssembliesInMarkupAreLoaded();
        InitializeComponent();
        DataContext = new AssemblySelectionViewModel(schemaProvider);
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;
        ((AssemblySelectionViewModel)DataContext).LoadAssembliesCommand.ExecuteAsync(null);
    }

    private void OnSelectClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    // This is just to work around the following exception:
    /* System.Windows.Markup.XamlParseException
        HResult = 0x80131501
        Message = Could not load file or assembly 'Microsoft.Xaml.Behaviors, PublicKeyToken=b03f5f7f11d50a3a' or one of its dependencies.The system cannot find the file specified.
    */
    private void EnsureReferencedAssembliesInMarkupAreLoaded()
    {
        var requiredAssemblyNames = new[] { "Microsoft.Xaml.Behaviors" };
        var loadedAssemblyNames = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetName().Name);
        var notLoadedAssemblyNames = requiredAssemblyNames.Except(loadedAssemblyNames).ToList();
        notLoadedAssemblyNames.ForEach(a => Assembly.Load(a));
    }
}
#nullable restore