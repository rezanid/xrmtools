namespace XrmTools.Meta.Model;

using XrmTools.Meta.Attributes;

public class CodeGenGlobalOptionSetsConfig
{
    public GlobalOptionSetGenerationMode Mode { get; set; } = GlobalOptionSetGenerationMode.NestedInEntityClass;
}