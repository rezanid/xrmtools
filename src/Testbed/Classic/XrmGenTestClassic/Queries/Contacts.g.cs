using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace XrmGenTestClassic.Queries
{
    internal static partial class FetchQueries
    {
        public const string Contacts = @"
<fetch >
	<entity name=""contact"">
		<attribute name=""fullname"" />
		<attribute name=""telephone1""/>
		<attribute name=""lastname""/>
		<attribute name=""firstname""/>
		<attribute name=""telephone1""/>
	</entity>
</fetch>";
        public static EntityCollection QueryContacts(this IOrganizationService service) => service.RetrieveMultiple(new FetchExpression(Contacts));
    }
}