using System.IO;
using AwesomeAssertions;
using RamlToOpenApiConverter;
using RamlToOpenApiConverterTest.Extensions;
using Xunit;

namespace RamlToOpenApiConverterTest.Examples;

public class ExamplesTests
{
    private readonly RamlConverter _sut = new();

    [Fact]
    public void CanConvertTypes_Examples()
    {
        // Arrange
        var expected = File.ReadAllText(Path.Combine("Examples", "Examples.json"));

        // Act
        var result = _sut.Convert(Path.Combine("Examples", "Examples.raml"));

        // Assert
        result.NormalizeNewLines().Should().BeEquivalentTo(expected.NormalizeNewLines());
    }
}