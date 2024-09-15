#nullable enable
using Microsoft.Xrm.Sdk.Metadata;
using System.Collections.Generic;

namespace XrmGen;

public class OptionSetMetadataNameComparer : IEqualityComparer<OptionSetMetadata>
{
    public bool Equals(OptionSetMetadata x, OptionSetMetadata y) => x.Name == y.Name;
    public int GetHashCode(OptionSetMetadata obj) => obj.Name.GetHashCode();
}
#nullable restore