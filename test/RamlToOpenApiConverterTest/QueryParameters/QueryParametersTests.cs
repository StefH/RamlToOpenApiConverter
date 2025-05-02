using System.IO;
using FluentAssertions;
using RamlToOpenApiConverter;
using RamlToOpenApiConverterTest.Extensions;
using Xunit;

namespace RamlToOpenApiConverterTest.QueryParameters;

public class QueryParametersTests
{
    private readonly RamlConverter _sut = new();

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

    [Fact]
    public void CanConvertParameterNil_OpenApi2_0()
    {
        // Arrange
        var path = "QueryParameterNil";
        var expected = File.ReadAllText(Path.Combine("QueryParameters", $"{path}2.json"));

        // Act
        var result = _sut.Convert(Path.Combine("QueryParameters", $"{path}.raml"), OpenApiSpecificationVersion.OpenApi2_0);

        // Assert
        result.NormalizeNewLines().Should().BeEquivalentTo(expected.NormalizeNewLines());
    }

    [Fact]
    public void CanConvertParameterNil_OpenApi3_0()
    {
        // Arrange
        var path = "QueryParameterNil";
        var expected = File.ReadAllText(Path.Combine("QueryParameters", $"{path}3.json"));

        // Act
        var result = _sut.Convert(Path.Combine("QueryParameters", $"{path}.raml"), OpenApiSpecificationVersion.OpenApi3_0);

        // Assert
        result.NormalizeNewLines().Should().BeEquivalentTo(expected.NormalizeNewLines());
    }

    [Fact]
    public void CanConvertParameterNil_OpenApi3_1()
    {
        // Arrange
        var path = "QueryParameterNil";
        var expected = File.ReadAllText(Path.Combine("QueryParameters", $"{path}31.json"));

        // Act
        var result = _sut.Convert(Path.Combine("QueryParameters", $"{path}.raml"), OpenApiSpecificationVersion.OpenApi3_1);

        // Assert
        result.NormalizeNewLines().Should().BeEquivalentTo(expected.NormalizeNewLines());
    }
}