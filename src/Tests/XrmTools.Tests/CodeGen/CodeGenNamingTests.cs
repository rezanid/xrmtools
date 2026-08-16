namespace XrmTools.Tests.CodeGen;

using System.Collections.Generic;
using FluentAssertions;
using XrmTools;
using XrmTools.WebApi.Entities;
using XrmTools.WebApi.Types;
using Xunit;

public class CodeGenNamingTests
{
    private static AttributeMetadata Attribute(string schemaName, string logicalName) => new()
    {
        SchemaName = schemaName,
        LogicalName = logicalName,
        AttributeType = AttributeTypeCode.String
    };

    private static EntityMetadata Entity(string schemaName, params AttributeMetadata[] attributes) =>
        new("xxx_postalcode", "xxx_postalcodes")
        {
            SchemaName = schemaName,
            Attributes = attributes
        };

    [Fact]
    public void ResolveEnclosingTypeNameCollisions_RenamesAttributeMatchingEnclosingType()
    {
        var colliding = Attribute("PostalCode", "xxx_postalcode");
        var entity = Entity("PostalCode", colliding, Attribute("Name", "xxx_name"));

        CodeGenNaming.ResolveEnclosingTypeNameCollisions(entity, entity.Attributes);

        colliding.SchemaName.Should().Be("PostalCodeValue");
        entity.SchemaName.Should().Be("PostalCode");
    }

    [Fact]
    public void ResolveEnclosingTypeNameCollisions_LeavesNonCollidingAttributesUntouched()
    {
        var name = Attribute("Name", "xxx_name");
        var code = Attribute("Code", "xxx_code");
        var entity = Entity("PostalCode", name, code);

        CodeGenNaming.ResolveEnclosingTypeNameCollisions(entity, entity.Attributes);

        name.SchemaName.Should().Be("Name");
        code.SchemaName.Should().Be("Code");
    }

    [Fact]
    public void ResolveEnclosingTypeNameCollisions_DisambiguatesWhenSuffixedNameAlreadyExists()
    {
        var colliding = Attribute("PostalCode", "xxx_postalcode");
        var existing = Attribute("PostalCodeValue", "xxx_postalcodevalue");
        var entity = Entity("PostalCode", colliding, existing);

        CodeGenNaming.ResolveEnclosingTypeNameCollisions(entity, entity.Attributes);

        colliding.SchemaName.Should().Be("PostalCodeValue1");
        existing.SchemaName.Should().Be("PostalCodeValue");
    }

    [Fact]
    public void ResolveEnclosingTypeNameCollisions_UsesConfiguredSuffix()
    {
        var colliding = Attribute("PostalCode", "xxx_postalcode");
        var entity = Entity("PostalCode", colliding, Attribute("Name", "xxx_name"));

        CodeGenNaming.ResolveEnclosingTypeNameCollisions(entity, entity.Attributes, "Attribute");

        colliding.SchemaName.Should().Be("PostalCodeAttribute");
    }

    [Fact]
    public void ResolveEnclosingTypeNameCollisions_FallsBackToDefaultSuffix_WhenConfiguredSuffixIsBlank()
    {
        var colliding = Attribute("PostalCode", "xxx_postalcode");
        var entity = Entity("PostalCode", colliding);

        CodeGenNaming.ResolveEnclosingTypeNameCollisions(entity, entity.Attributes, "   ");

        colliding.SchemaName.Should().Be("PostalCodeValue");
    }

    [Fact]
    public void ResolveEnclosingTypeNameCollisions_ComparisonIsCaseSensitive()
    {
        // "postalcode" (lower) is a distinct C# identifier from the type "PostalCode",
        // so it must not be renamed.
        var different = Attribute("postalcode", "xxx_postalcode2");
        var entity = Entity("PostalCode", different);

        CodeGenNaming.ResolveEnclosingTypeNameCollisions(entity, entity.Attributes);

        different.SchemaName.Should().Be("postalcode");
    }

    [Fact]
    public void ResolveEnclosingTypeNameCollisions_HandlesNullArguments()
    {
        var act = () =>
        {
            CodeGenNaming.ResolveEnclosingTypeNameCollisions(null, new List<AttributeMetadata>());
            CodeGenNaming.ResolveEnclosingTypeNameCollisions(Entity("PostalCode"), null);
        };

        act.Should().NotThrow();
    }
}
