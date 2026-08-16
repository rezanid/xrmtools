namespace XrmTools.Meta.Xsd.Tests;

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using Xunit;

public class FetchSchemaValidationTests
{
    private static readonly Lazy<XmlSchemaSet> SchemaSet = new(CreateSchemaSet);

    public static TheoryData<string, string> ValidDocuments => new()
    {
        {
            "simple fetch with entity and attributes",
            """
            <fetch>
              <entity name="account">
                <attribute name="name" />
                <attribute name="accountnumber" />
              </entity>
            </fetch>
            """
        },
        {
            "fetch with paging and output attributes",
            """
            <fetch version="1.0" count="50" page="2" paging-cookie="cookie" utc-offset="60" distinct="true" top="10" mapping="logical" min-active-row-version="true" output-format="xml-platform" returntotalrecordcount="true" no-lock="true">
              <entity name="contact">
                <attribute name="fullname" />
                <order attribute="fullname" descending="true" />
              </entity>
            </fetch>
            """
        },
        {
            "reports view style fetch with root order",
            """
            <fetch distinct="false">
              <order attribute="createdon" descending="true" />
            </fetch>
            """
        },
        {
            "entity filter with single value operator",
            """
            <fetch>
              <entity name="account">
                <attribute name="name" />
                <filter type="and">
                  <condition attribute="name" operator="eq" value="Contoso" />
                </filter>
              </entity>
            </fetch>
            """
        },
        {
            "condition using multiple values",
            """
            <fetch>
              <entity name="contact">
                <attribute name="fullname" />
                <filter>
                  <condition attribute="statuscode" operator="in">
                    <value>1</value>
                    <value uiname="Active" uitype="status">2</value>
                  </condition>
                </filter>
              </entity>
            </fetch>
            """
        },
        {
            "condition with operator that requires no explicit value",
            """
            <fetch>
              <entity name="lead">
                <filter>
                  <condition attribute="fax" operator="null" />
                </filter>
              </entity>
            </fetch>
            """
        },
        {
            "nested filters with quick find flag",
            """
            <fetch>
              <entity name="account">
                <filter type="and" isquickfindfields="true">
                  <condition attribute="name" operator="like" value="Contoso%" />
                  <filter type="or">
                    <condition attribute="telephone1" operator="not-null" />
                    <condition attribute="address1_city" operator="eq" value="Redmond" />
                  </filter>
                </filter>
              </entity>
            </fetch>
            """
        },
        {
            "link entity with nested content",
            """
            <fetch>
              <entity name="account" enableprefiltering="true" prefilterparametername="CRM_FilteredAccount">
                <attribute name="name" />
                <link-entity name="contact" from="parentcustomerid" to="accountid" alias="primarycontact" link-type="outer" visible="true" intersect="false" enableprefiltering="false" prefilterparametername="ContactFilter">
                  <attribute name="fullname" />
                  <order attribute="fullname" descending="false" />
                  <filter type="and">
                    <condition attribute="statecode" operator="eq" value="0" />
                  </filter>
                </link-entity>
              </entity>
            </fetch>
            """
        },
        {
            "aggregate attribute configuration",
            """
            <fetch aggregate="true">
              <entity name="opportunity">
                <attribute name="estimatedvalue" alias="totalvalue" aggregate="sum" />
                <attribute name="createdon" alias="createdmonth" groupby="true" dategrouping="month" usertimezone="false" distinct="false" />
              </entity>
            </fetch>
            """
        },
        {
            "all attributes with param placeholder",
            """
            <fetch>
              <entity name="account">
                <all-attributes>
                  <param name="allAttributesDefault" />
                </all-attributes>
              </entity>
            </fetch>
            """
        },
        {
            "entity param with default fragment",
            """
            <fetch>
              <entity name="account">
                <param name="entityDefaults">
                  <attribute name="name" />
                  <order attribute="name" descending="false" />
                  <filter type="and">
                    <condition attribute="statecode" operator="eq" value="0" />
                  </filter>
                  <link-entity name="contact" from="parentcustomerid" to="accountid">
                    <attribute name="fullname" />
                  </link-entity>
                  <all-attributes />
                </param>
              </entity>
            </fetch>
            """
        },
        {
            "filter param with condition and nested filter",
            """
            <fetch>
              <entity name="account">
                <filter>
                  <param name="criteria">
                    <condition attribute="name" operator="eq" value="Contoso" />
                    <filter type="or">
                      <condition attribute="accountnumber" operator="like" value="A%" />
                    </filter>
                  </param>
                </filter>
              </entity>
            </fetch>
            """
        },
        {
            "condition param with default values",
            """
            <fetch>
              <entity name="account">
                <filter>
                  <condition attribute="accountclassificationcode" operator="in">
                    <param name="classificationValues">
                      <value>1</value>
                      <value uiname="Preferred" uitype="accountclassificationcode">2</value>
                    </param>
                  </condition>
                </filter>
              </entity>
            </fetch>
            """
        },
        {
            "fetch param with default entity fragment",
            """
            <fetch>
              <param name="queryBody">
                <entity name="incident">
                  <attribute name="title" />
                  <filter>
                    <condition attribute="statecode" operator="eq" value="0" />
                  </filter>
                </entity>
                <order attribute="createdon" descending="true" />
              </param>
            </fetch>
            """
        },
        {
            "root param element containing multiple fragments",
            """
            <param name="rootDefaults">
              <fetch>
                <entity name="task">
                  <attribute name="subject" />
                </entity>
              </fetch>
              <condition attribute="statecode" operator="eq" value="0" />
              <value>fallback</value>
            </param>
            """
        },
        {
            "deeply nested recursive params",
            """
            <fetch>
              <entity name="account">
                <param name="outerEntityDefaults">
                  <filter>
                    <param name="nestedFilterDefaults">
                      <condition attribute="accountcategorycode" operator="in">
                        <param name="nestedConditionDefaults">
                          <value>1</value>
                          <value>2</value>
                        </param>
                      </condition>
                    </param>
                  </filter>
                </param>
              </entity>
            </fetch>
            """
        },
        {
            "condition using column aggregate and ui attributes",
            """
            <fetch aggregate="true">
              <entity name="account">
                <filter>
                  <condition column="accountid" entityname="account" operator="ne" aggregate="count" alias="accountCount" uiname="Accounts" uitype="account" uihidden="1" />
                </filter>
              </entity>
            </fetch>
            """
        },
        {
            "attribute with legacy build metadata",
            """
            <fetch>
              <entity name="account">
                <attribute name="name" build="1.504021" addedby="system" alias="account_name" />
              </entity>
            </fetch>
            """
        },
        {
            "savedquery with embedded fetch and layout",
            """
            <savedquery>
              <name>Active Accounts</name>
              <savedqueryid>{01234567-89AB-CDEF-0123-456789ABCDEF}</savedqueryid>
              <returnedtypecode>1</returnedtypecode>
              <querytype>0</querytype>
              <fetchxml>
                <fetch>
                  <entity name="account">
                    <attribute name="name" />
                  </entity>
                </fetch>
              </fetchxml>
              <columnsetxml>
                <columnset version="1.0" distinct="false">
                  <column build="1.504021" addedby="system">name</column>
                  <ascend />
                  <filter column="name" operator="eq" value="Contoso" type="and">
                    <condition column="name" operator="eq" value="Contoso" />
                  </filter>
                </columnset>
              </columnsetxml>
              <layoutxml>
                <grid name="resultset" select="true" preview="false" icon="false" object="1">
                  <row name="result" id="accountid">
                    <cell name="name" width="150" cellType="string" imageproviderwebresource="" imageproviderfunctionname="" />
                  </row>
                </grid>
              </layoutxml>
            </savedquery>
            """
        },
        {
            "savedquery with optional serialized values",
            """
            <savedquery donotuseinLCID="1033" useinLCID="1031">
              <name>Quick Find Accounts</name>
              <savedqueryid>01234567-89ab-cdef-0123-456789abcdef</savedqueryid>
              <returnedtypecode formattedvalue="Account">1</returnedtypecode>
              <description>Accounts quick find</description>
              <querytype formattedvalue="Main">64</querytype>
              <IsCustomizable name="CanCustomize">1</IsCustomizable>
              <CanBeDeleted name="CanDelete">0</CanBeDeleted>
              <IntroducedVersion>1.0.0.0</IntroducedVersion>
              <isquickfindquery name="QuickFind">1</isquickfindquery>
              <isuserdefined name="UserDefined">0</isuserdefined>
              <isdefault name="DefaultView">1</isdefault>
              <isprivate>false</isprivate>
              <queryapi>FetchXml</queryapi>
            </savedquery>
            """
        }
    };

    public static TheoryData<string, string> InvalidDocuments => new()
    {
        {
            "param missing required name attribute",
            """
            <fetch>
              <entity name="account">
                <param>
                  <attribute name="name" />
                </param>
              </entity>
            </fetch>
            """
        },
        {
            "condition with invalid operator value",
            """
            <fetch>
              <entity name="account">
                <filter>
                  <condition attribute="name" operator="equals" value="Contoso" />
                </filter>
              </entity>
            </fetch>
            """
        },
        {
            "condition param contains disallowed filter fragment",
            """
            <fetch>
              <entity name="account">
                <filter>
                  <condition attribute="name" operator="in">
                    <param name="invalidConditionDefaults">
                      <filter>
                        <condition attribute="name" operator="eq" value="Contoso" />
                      </filter>
                    </param>
                  </condition>
                </filter>
              </entity>
            </fetch>
            """
        },
        {
            "filter param contains disallowed attribute fragment",
            """
            <fetch>
              <entity name="account">
                <filter>
                  <param name="invalidFilterDefaults">
                    <attribute name="name" />
                  </param>
                </filter>
              </entity>
            </fetch>
            """
        },
        {
            "entity param contains disallowed condition fragment",
            """
            <fetch>
              <entity name="account">
                <param name="invalidEntityDefaults">
                  <condition attribute="name" operator="eq" value="Contoso" />
                </param>
              </entity>
            </fetch>
            """
        },
        {
            "fetch param contains disallowed filter fragment",
            """
            <fetch>
              <param name="invalidFetchDefaults">
                <filter>
                  <condition attribute="name" operator="eq" value="Contoso" />
                </filter>
              </param>
            </fetch>
            """
        },
        {
            "all attributes param cannot contain child elements",
            """
            <fetch>
              <entity name="account">
                <all-attributes>
                  <param name="invalidAllAttributesDefaults">
                    <value>unexpected</value>
                  </param>
                </all-attributes>
              </entity>
            </fetch>
            """
        },
        {
            "link entity missing required name attribute",
            """
            <fetch>
              <entity name="account">
                <link-entity from="parentcustomerid" to="accountid">
                  <attribute name="fullname" />
                </link-entity>
              </entity>
            </fetch>
            """
        },
        {
            "fetch has invalid mapping enumeration",
            """
            <fetch mapping="physical">
              <entity name="account" />
            </fetch>
            """
        },
        {
            "attribute has invalid aggregate enumeration",
            """
            <fetch aggregate="true">
              <entity name="account">
                <attribute name="accountid" aggregate="median" />
              </entity>
            </fetch>
            """
        },
        {
            "condition has invalid uihidden value",
            """
            <fetch>
              <entity name="account">
                <filter>
                  <condition attribute="name" operator="eq" value="Contoso" uihidden="true" />
                </filter>
              </entity>
            </fetch>
            """
        },
        {
            "root param contains unexpected child element",
            """
            <param name="invalidRootDefaults">
              <foo />
            </param>
            """
        },
        {
            "savedquery contains invalid guid",
            """
            <savedquery>
              <name>Invalid View</name>
              <savedqueryid>not-a-guid</savedqueryid>
              <returnedtypecode>1</returnedtypecode>
              <querytype>0</querytype>
            </savedquery>
            """
        }
    };

    [Fact]
    public void FetchSchema_CompilesSuccessfully()
    {
        Assert.NotNull(SchemaSet.Value);
        Assert.NotEmpty(SchemaSet.Value.Schemas());
    }

    [Theory]
    [MemberData(nameof(ValidDocuments))]
    public void FetchSchema_Accepts_ValidDocuments(string scenario, string xml)
    {
        var result = Validate(xml);

        Assert.True(result.IsValid, $"Scenario '{scenario}' should be valid. Errors: {string.Join(" | ", result.Errors)}");
    }

    [Theory]
    [MemberData(nameof(InvalidDocuments))]
    public void FetchSchema_Rejects_InvalidDocuments(string scenario, string xml)
    {
        var result = Validate(xml);

        Assert.False(result.IsValid, $"Scenario '{scenario}' should be invalid.");
        Assert.NotEmpty(result.Errors);
    }

    private static SchemaValidationResult Validate(string xml)
    {
        var errors = new List<string>();
        var settings = new XmlReaderSettings
        {
            ValidationType = ValidationType.Schema,
            Schemas = SchemaSet.Value,
            DtdProcessing = DtdProcessing.Prohibit,
        };

        settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
        settings.ValidationEventHandler += (_, eventArgs) => errors.Add(eventArgs.Message);

        using var stringReader = new StringReader(xml);
        using var xmlReader = XmlReader.Create(stringReader, settings);

        while (xmlReader.Read())
        {
        }

        return new SchemaValidationResult(errors.Count == 0, errors);
    }

    private static XmlSchemaSet CreateSchemaSet()
    {
        var schemaPath = Path.Combine(AppContext.BaseDirectory, "schemas", "Fetch.xsd");
        var schemas = new XmlSchemaSet();

        using var fileStream = File.OpenRead(schemaPath);
        using var schemaReader = XmlReader.Create(fileStream);

        schemas.Add(null, schemaReader);
        schemas.Compile();

        return schemas;
    }

    private sealed record SchemaValidationResult(bool IsValid, IReadOnlyList<string> Errors);
}
