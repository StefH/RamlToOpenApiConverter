using System.IO;
using AwesomeAssertions;
using RamlToOpenApiConverter;
using RamlToOpenApiConverterTest.Extensions;
using Xunit;

namespace RamlToOpenApiConverterTest.Uses;

public class UsesTest
{
    private readonly RamlConverter _sut = new();

    [Theory]
    [InlineData("Test1")]
    [InlineData("Test2")]
    [InlineData("Test3")]
    [InlineData("Test4")]
    public void ValidateUsesReplace(string path)
    {
        // Arrange
        var expected = File.ReadAllText(Path.Combine("Uses/TestFilesResultExpected", $"{path}Result.json"));

        // Act
        var result = _sut.Convert(Path.Combine("Uses/TestFiles", $"{path}.raml"));

        // Assert
        result.NormalizeNewLines().Should().BeEquivalentTo(expected.NormalizeNewLines());
    }

    [Fact]
    public void Issue21()
    {
        // Arrange
        var expected = File.ReadAllText(Path.Combine("Uses/TestFilesResultExpected", "app.json"));

        // Act
        var result = _sut.Convert(Path.Combine("Uses/TestFiles", "app.raml"));

        // Assert
        result.NormalizeNewLines().Should().BeEquivalentTo(expected.NormalizeNewLines());
    }
}