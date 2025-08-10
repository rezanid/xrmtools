#nullable enable
using System.Collections.Generic;
using XrmTools.WebApi.Entities;

namespace XrmTools;

public class OptionSetMetadataNameComparer : IEqualityComparer<OptionSetMetadata>
{
    public bool Equals(OptionSetMetadata x, OptionSetMetadata y) => x.Name == y.Name;
    public int GetHashCode(OptionSetMetadata obj) => obj.Name.GetHashCode();
}
#nullable restore