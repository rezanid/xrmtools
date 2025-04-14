#nullable enable
namespace XrmTools.WebApi.Types;

using System.Collections.Generic;

public class ExportComponentsParams
{
    public List<ExportComponentDetails> ExportComponentsList { get; set; }
}
#nullable restore