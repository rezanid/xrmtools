using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace XrmGenTest.Queries
{
    internal static partial class FetchQueries
    {
        public const string Accounts = @"
<fetch >
  <entity name=""account"">
  </entity>
</fetch>";
        public static EntityCollection QueryAccounts(this IOrganizationService service) => service.RetrieveMultiple(new FetchExpression(Accounts));
    }
}