using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace XrmGenTestClassic
{
    internal static partial class FetchQueries
    {
        public const string Test = @"
<fetch  >
	<entity name=""contact"">
		<attribute name=""fullname"" />
		<attribute name=""telephone1""/>
		<attribute name=""lastname""/>
		<attribute name=""firstname""/>
		<attribute name=""telephone1""/>
		<attribute name=""bpg_address1_fulladdress""/>
		<filter>
			<condition attribute=""fullname"" operator=""not-begin-with"" value=""Logic""/>
			<condition attribute=""fullname"" operator=""neq"" value="". .""/>
			<condition attribute=""telephone1"" operator=""begins-with"" value=""+32""/>
		</filter>
	</entity>
</fetch>";
        public static EntityCollection QueryTest(this IOrganizationService service) => service.RetrieveMultiple(new FetchExpression(Test));
    }
}