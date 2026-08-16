using System.Collections.Generic;
using XrmTools.Serialization;
using System.Text.Json;
using FluentAssertions;
using Xunit;
using System.Runtime.Serialization;

namespace XrmTools.Core.Tests;

[KnownType(typeof(Dog))]
[KnownType(typeof(Cat))]
public class Animal { public string Name { get; set; } }

public class Dog : Animal { public bool Barks { get; set; } }
public class Cat : Animal { public bool Meows { get; set; } }

public class Circus
{
    public string Name { get; set; }

    public Animal StarringAnimal { get; set; }
}

public class PolymorphicJsonConverterTests
{
    private static JsonSerializerOptions GetOptions()
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        options.Converters.Add(new PolymorphicJsonConverter<Animal>());
        return options;
    }

    [Fact]
    public void Deserialize_DogJson_ResolvesDogType()
    {
        var json = "{\"Name\":\"Fido\",\"Barks\":true}";
        var animal = JsonSerializer.Deserialize<Animal>(json, GetOptions());
        animal.Should().BeOfType<Dog>();
        animal.Name.Should().Be("Fido");
        ((Dog)animal).Barks.Should().BeTrue();
    }

    [Fact]
    public void Deserialize_CatJson_ResolvesCatType()
    {
        var json = "{\"Name\":\"Whiskers\",\"Meows\":true}";
        var animal = JsonSerializer.Deserialize<Animal>(json, GetOptions());
        animal.Should().BeOfType<Cat>();
        animal.Name.Should().Be("Whiskers");
        ((Cat)animal).Meows.Should().BeTrue();
    }

    [Fact]
    public void Serialize_Dog_ProducesDogJson()
    {
        var dog = new Dog { Name = "Fido", Barks = true };
        var json = JsonSerializer.Serialize<Animal>(dog, GetOptions());
        json.Should().Contain("Barks");
        json.Should().Contain("Fido");
    }

    [Fact]
    public void Serialize_Cat_ProducesCatJson()
    {
        var cat = new Cat { Name = "Kitty", Meows = true };
        var json = JsonSerializer.Serialize<Animal>(cat, GetOptions());
        json.Should().Contain("Meows");
        json.Should().Contain("Kitty");
    }

    [Fact]
    public void Deserialize_UnknownType_ThrowsJsonException()
    {
        var json = "{\"Name\":\"Unknown\"}";
        var act = () => JsonSerializer.Deserialize<Animal>(json, GetOptions());
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Deserialize_NestedPolymorphicJson_ResolvesCorrectType()
    {
        var json = "[{\"Name\":\"Fido\",\"Barks\":true},{\"Name\":\"Whiskers\",\"Meows\":true}]";
        var animals = JsonSerializer.Deserialize<List<Animal>>(json, GetOptions());
        animals.Should().HaveCount(2);
        animals[0].Should().BeOfType<Dog>();
        animals[1].Should().BeOfType<Cat>();
    }

    [Fact]
    public void Serialize_NestedPolymorphicObjects_ProducesCorrectJson()
    {
        var animals = new List<Animal>
        {
            new Dog { Name = "Fido", Barks = true },
            new Cat { Name = "Whiskers", Meows = true }
        };
        var json = JsonSerializer.Serialize(animals, GetOptions());
        json.Should().Contain("Fido");
        json.Should().Contain("Barks");
        json.Should().Contain("Whiskers");
        json.Should().Contain("Meows");
    }

    [Fact]
    public void Deserializa_NestedPolymorphicObjects_ResolvesCorrectType()
    {
        var json = "[{\"Name\":\"Fido\",\"Barks\":true},{\"Name\":\"Whiskers\",\"Meows\":true}]";
        var animals = JsonSerializer.Deserialize<List<Animal>>(json, GetOptions());
        animals.Should().HaveCount(2);
        animals[0].Should().BeOfType<Dog>();
        animals[1].Should().BeOfType<Cat>();
        ((Dog)animals[0]).Barks.Should().BeTrue();
        ((Cat)animals[1]).Meows.Should().BeTrue();
    }

    [Fact]
    public void Serialize_ComplexTypeWithPolymorphism_ProducesCorrectJson()
    {
        var json = JsonSerializer.Serialize(new Circus
        {
            Name = "Animal Circus",
            StarringAnimal = new Dog { Name = "Rex", Barks = true }
        }, GetOptions());
        json.Should().Contain("Animal Circus");
        json.Should().Contain("Rex");
        json.Should().Contain("Barks");
    }

    [Fact]
    public void Deserialize_ComplexTypeWithPolymorphism_ResolvesCorrectType()
    {
        var json = "{\"Name\":\"Animal Circus\",\"StarringAnimal\":{\"Name\":\"Rex\",\"Barks\":true}}";
        var circus = JsonSerializer.Deserialize<Circus>(json, GetOptions());
        circus.Should().NotBeNull();
        circus.Name.Should().Be("Animal Circus");
        circus.StarringAnimal.Should().BeOfType<Dog>();
        ((Dog)circus.StarringAnimal).Barks.Should().BeTrue();
    }
}