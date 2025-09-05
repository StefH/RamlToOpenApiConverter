using System.IO;
using AwesomeAssertions;
using RamlToOpenApiConverter;
using RamlToOpenApiConverterTest.Extensions;
using Xunit;

namespace RamlToOpenApiConverterTest.TypesAndSchemas;

public class EnumTests
{
    private readonly RamlConverter _sut = new();

    [Fact]
    public void CanConvertTypes_Enum()
    {
        // Arrange
        var expected = File.ReadAllText(Path.Combine("TypesAndSchemas", "TypesEnum.json"));

        // Act
        var result = _sut.Convert(Path.Combine("TypesAndSchemas", "TypesEnum.raml"));

        // Assert
        result.NormalizeNewLines().Should().BeEquivalentTo(expected.NormalizeNewLines());
    }
}