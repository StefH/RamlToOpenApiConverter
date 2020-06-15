using System.IO;
using FluentAssertions;
using RamlToOpenApiConverter;
using RamlToOpenApiConverterTest.Extensions;
using Xunit;

namespace RamlToOpenApiConverterTest.QueryParameters
{
    public class QueryParametersTests
    {
        private readonly RamlConverter _sut;

        public QueryParametersTests()
        {
            _sut = new RamlConverter();
        }

        [Theory]
        [InlineData("QueryParameterEnumInline")]
        [InlineData("QueryParameterNil")]
        [InlineData("QueryParameterRequired")]
        public void CanConvertParameter(string path)
        {
            // Arrange
            string expected = File.ReadAllText(Path.Combine("QueryParameters", $"{path}.json"));

            // Act
            string result = _sut.Convert(Path.Combine("QueryParameters", $"{path}.raml"));

            // Assert
            result.NormalizeNewLines().Should().BeEquivalentTo(expected.NormalizeNewLines());
        }
    }
}