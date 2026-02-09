namespace XrmTools.WebApi.Tests;

using Scriban;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XrmTools.WebApi.Entities;
using Xunit;

public class EntityMetadataTests
{
    [Fact]
    public void EntityMetadataAttributes_Scriban_Safe()
    {
        var entity = new EntityMetadata("contact", "contacts")
        {
            Attributes = [
                new () {
                    LogicalName = "firstname",
                    AttributeType = Types.AttributeTypeCode.String
                },
                new () {
                    LogicalName = "lastname",
                    AttributeType = Types.AttributeTypeCode.String
                }
                ]
        };

        // Prepare Scriban
        var templateContent = "{{~for attribute in attributes~}}TEST{{~end~}}";
        var template = Template.Parse(templateContent);
        var result = template.Render(entity);
    }
}
