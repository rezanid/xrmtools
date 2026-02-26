namespace XrmTools.Shell.Styles;

using Microsoft;
using System;
using System.Collections.Generic;
using System.Resources;
using System.Windows;

public class DeferredStyleDictionary : ResourceDictionary
{
    private DeferredStyleDictionary(Dictionary<object, string> styles)
    {
        Requires.NotNull(styles, nameof(styles));
        Styles = styles;
        RealizedStyles = new Dictionary<object, Style>(styles.Count);
        foreach (KeyValuePair<object, string> style in Styles)
            Add(style.Key, style.Key);
    }

    public Dictionary<object, Style> RealizedStyles { get; }

    public Dictionary<object, string> Styles { get; }

    public static DeferredStyleDictionary Create(Dictionary<object, string> styles)
    {
        return new DeferredStyleDictionary(styles);
    }

    protected override void OnGettingValue(object key, ref object value, out bool canCache)
    {
        canCache = false;
        if (RealizedStyles.TryGetValue(key, out var style1))
        {
            value = style1;
        }
        else
        {
            string style2 = Styles[key];
            if (!(Application.LoadComponent(new Uri(style2, UriKind.Relative)) is ResourceDictionary resourceDictionary))
                throw new MissingManifestResourceException(style2);
            if (!resourceDictionary.Contains(key))
                throw new KeyNotFoundException(key.ToString());
            value = RealizedStyles[key] = (Style)resourceDictionary[key];
        }
    }
}