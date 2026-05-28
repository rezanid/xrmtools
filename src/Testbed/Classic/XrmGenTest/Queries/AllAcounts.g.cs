using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace XrmGenTest.Queries
{
    internal static partial class FetchQueries
    {
        public const string AllAcounts = @"
<fetch >
  <entity name=""account"">
	  <attribute name=""name"" />
	  <attribute name=""description"" />
  </entity>
</fetch>";
        public static EntityCollection QueryAllAcounts(this IOrganizationService service) => service.RetrieveMultiple(new FetchExpression(AllAcounts));
    }
}