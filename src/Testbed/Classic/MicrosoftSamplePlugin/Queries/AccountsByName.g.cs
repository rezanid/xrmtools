using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace XrmTraditionalPlugins.Queries
{
    internal static partial class FetchQueries
    {
        public const string AccountsByName = @"
<fetch >
  <entity name=""account"">
	  <attribute name=""name""/>
  </entity>
</fetch>";
        public static EntityCollection QueryAccountsByName(this IOrganizationService service) => service.RetrieveMultiple(new FetchExpression(AccountsByName));
    }
}