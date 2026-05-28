using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace XrmGenTest.Queries
{
    internal static partial class FetchQueries
    {
        public const string AllAccounts = @"
<fetch >
  <entity name=""account"">
	  <attribute name=""name"" />
	  <attribute name=""description"" />
  </entity>
</fetch>";
        public static EntityCollection QueryAllAccounts(this IOrganizationService service) => service.RetrieveMultiple(new FetchExpression(AllAccounts));
    }
}