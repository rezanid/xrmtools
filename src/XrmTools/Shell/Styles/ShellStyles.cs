namespace XrmTools.Shell.Styles;

using System;
using System.Collections.Generic;
using System.IO;
using System.Resources;
using System.Windows;

public static class ShellStyles
{
    internal const string UriBase = "/XrmTools;component/Shell/Styles";

    public static Style ButtonBase { get; } = LoadStyle("ButtonBaseStyle.xaml");

    public static Style FocusVisual { get; } = LoadStyle("FocusVisualStyle.xaml");

    public static Style ListBoxItem { get; } = LoadStyle("ListBoxItemBaseStyle.xaml");

    internal static Style LoadStyle(string xamlFile)
    {
        string str = Application.LoadComponent(new Uri($"{UriBase}/" + xamlFile, UriKind.Relative)) is ResourceDictionary resourceDictionary ? Path.GetFileNameWithoutExtension(xamlFile) : throw new MissingManifestResourceException(xamlFile);
        return resourceDictionary.Contains(str) ? (Style)resourceDictionary[str] : throw new KeyNotFoundException(str);
    }
}