using System.IO;
using FluentAssertions;
using RamlToOpenApiConverter;
using RamlToOpenApiConverterTest.Extensions;
using Xunit;

namespace RamlToOpenApiConverterTest.Uses;

public class UsesTest
{
    private readonly RamlConverter _testResult;

    public UsesTest()
    {
        _testResult = new RamlConverter();
    }

    [Theory]
    [InlineData("Test1")]
    [InlineData("Test2")]
    [InlineData("Test3")]
    [InlineData("Test4")]
    private void ValidateUsesReplace(string path)
    {
        // Arrange
        string expected = File.ReadAllText(Path.Combine("Uses/TestFilesResultExpected", $"{path}Result.json"));

        // Act
        string result = _testResult.Convert(Path.Combine("Uses/TestFiles", $"{path}.raml"));

        // Assert
        result.NormalizeNewLines().Should().BeEquivalentTo(expected.NormalizeNewLines());
    }

}