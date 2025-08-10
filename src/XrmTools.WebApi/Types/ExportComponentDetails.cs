#nullable enable
namespace XrmTools.WebApi.Types;

using System;

public class ExportComponentDetails
{
    public Guid ComponentId { get; set; }   

    public int ComponentType { get; set; }  

    public bool AddRequiredComponents { get; set; }
}
#nullable restore