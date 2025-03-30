
using XrmGenTest;
using XrmTools.Meta.Attributes;

[assembly: Entity("notification", AttributeNames = "createdon,eventid,notificationnumber,notificationid")]
[assembly: Entity("account", AttributeNames = "name,accountnumber,telephone1,statecode,statuscode,primarycontactid,address1_city")]
[assembly: Entity("contact", AttributeNames = "name,parentcontactid,annualincome,territorycode,accountid,accountidyominame")]

public class Test
{
    public void Test1()
    {
        var s = "";
    }
}