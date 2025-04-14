using System.IO;
using FluentAssertions;
using RamlToOpenApiConverter;
using RamlToOpenApiConverterTest.Extensions;
using Xunit;

namespace RamlToOpenApiConverterTest.QueryParameters;

public class QueryParametersTests
{
    private readonly RamlConverter _sut;

    public QueryParametersTests()
    {
        _sut = new RamlConverter();
    }

    [Theory]
    [InlineData("QueryParameterEnumInline")]
    [InlineData("QueryParameterRequired")]
    public void CanConvertParameter(string path)
    {
        // Arrange
        var expected = File.ReadAllText(Path.Combine("QueryParameters", $"{path}.json"));

        // Act
        var result = _sut.Convert(Path.Combine("QueryParameters", $"{path}.raml"));

        // Assert
        result.NormalizeNewLines().Should().BeEquivalentTo(expected.NormalizeNewLines());
    }

    [Fact(Skip = "nullable is not supported?")]
    public void CanConvertParameterNil()
    {
        // Arrange
        var path = "QueryParameterNil";
        var expected = File.ReadAllText(Path.Combine("QueryParameters", $"{path}.json"));

        // Act
        var result = _sut.Convert(Path.Combine("QueryParameters", $"{path}.raml"));

        // Assert
        result.NormalizeNewLines().Should().BeEquivalentTo(expected.NormalizeNewLines());
    }
}