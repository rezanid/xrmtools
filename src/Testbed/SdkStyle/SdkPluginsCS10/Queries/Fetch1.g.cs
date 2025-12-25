using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace SdkPluginsCS10.Queries
{
    internal static partial class FetchQueries
    {
        public static EntityCollection QueryFetch1(
            this IOrganizationService service,
            string name = @"name",
            string entity = @"account")
        {
            var fetchXml = $@"
<fetch >
  <entity name=""{entity}"">
	  <attribute name=""{name}""/>
	  <attribute name=""accountcategorycode""/>
  </entity>
</fetch>";
            return service.RetrieveMultiple(new FetchExpression(fetchXml));
        }
    }
}