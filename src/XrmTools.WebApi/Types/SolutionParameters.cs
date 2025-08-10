#nullable enable
namespace XrmTools.WebApi.Types;

using System;
using System.Collections.Generic;

public class SolutionParameters
{
    public Guid StageSolutionUploadId { get; set; }
    public List<SolutionComponentOption> SolutionComponentOptions { get; set; }
}
#nullable restore