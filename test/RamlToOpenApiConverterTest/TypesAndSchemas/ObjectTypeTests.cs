using System.IO;
using FluentAssertions;
using RamlToOpenApiConverter;
using Xunit;

namespace RamlToOpenApiConverterTest.TypesAndSchemas
{
    public class ObjectTypeTests
    {
        private readonly RamlConverter _sut;

        public ObjectTypeTests()
        {
            _sut = new RamlConverter();
        }

        [Theory]
        [InlineData("TypesObjectInline")]
        [InlineData("TypesObjectInclude")]
        public void CanConvertTypes_InlineAndInclude(string path)
        {
            // Arrange
            string expected = File.ReadAllText(Path.Combine("TypesAndSchemas", $"{path}.json"));

            // Act
            string result = _sut.Convert(Path.Combine("TypesAndSchemas", $"{path}.raml"));

            // Assert
            result.Should().Be(expected);
        }
    }
}