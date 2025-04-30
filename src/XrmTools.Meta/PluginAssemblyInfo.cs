namespace XrmTools.Meta;
using System;
using System.Collections.Generic;
using XrmTools.Meta.Attributes;

public class PluginAssemblyInfo(string name, PluginAssemblyAttribute attribute)
{
    public string AssemblyName { get; set; } = name;
    public PluginAssemblyAttribute AssemblyAttribute { get; set; } = attribute;
    public List<PluginInfo> Plugins { get; set; } = [];

    public void AddPlugin(PluginInfo plugin)
    {
        if (plugin == null) throw new ArgumentNullException(nameof(plugin));
        Plugins.Add(plugin);
    }
}

public class PluginInfo(string name, PluginAttribute attribute)
{
    public string ClassName { get; set; } = name;
    public PluginAttribute PluginAttribute { get; set; } = attribute;
    public List<StepInfo> StepInfos { get; set; } = [];

    public void AddStep(StepInfo step)
    {
        if (step == null) throw new ArgumentNullException(nameof(step));
        StepInfos.Add(step);
    }
}

public class StepInfo(StepAttribute attribute)
{
    public StepAttribute StepAttribute { get; set; } = attribute;
    public List<ImageInfo> ImageInfos { get; set; } = [];

    public void AddImage(ImageInfo image)
    {
        if (image == null) throw new ArgumentNullException(nameof(image));
        ImageInfos.Add(image);
    }
}

public class ImageInfo(ImageAttribute attribute)
{
    public ImageAttribute ImageAttribute { get; set; } = attribute;
}
