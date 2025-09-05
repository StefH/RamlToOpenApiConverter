using System.IO;
using AwesomeAssertions;
using RamlToOpenApiConverter;
using RamlToOpenApiConverterTest.Extensions;
using Xunit;

namespace RamlToOpenApiConverterTest.TypesAndSchemas;

public class ObjectTypeTests
{
    private readonly RamlConverter _sut = new();

    [Theory]
    [InlineData("TypesObjectInline")]
    [InlineData("TypesObjectInclude")]
    public void CanConvertTypes_InlineAndInclude(string path)
    {
        // Arrange
        var expected = File.ReadAllText(Path.Combine("TypesAndSchemas", $"{path}.json"));

        // Act
        var result = _sut.Convert(Path.Combine("TypesAndSchemas", $"{path}.raml"));

        // Assert
        result.NormalizeNewLines().Should().BeEquivalentTo(expected.NormalizeNewLines());
    }
}