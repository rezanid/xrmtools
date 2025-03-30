#nullable enable
namespace XrmTools.WebApi.Types;

using Newtonsoft.Json.Linq;

public class SolutionComponentOption
{
    public ImportDecision ImportDecision { get; set; }

    public JObject Component { get; set; }
}
#nullable restore