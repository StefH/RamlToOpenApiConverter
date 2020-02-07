using System.IO;
using FluentAssertions;
using RamlToOpenApiConverter;
using Xunit;

namespace RamlToOpenApiConverterTest.TypesAndSchemas
{
    public class EnumTests
    {
        private readonly RamlConverter _sut;

        public EnumTests()
        {
            _sut = new RamlConverter();
        }

        [Fact]
        public void CanConvertTypes_Enum()
        {
            // Arrange
            string expected = File.ReadAllText(Path.Combine("TypesAndSchemas", "TypesEnum.json"));

            // Act
            string result = _sut.Convert(Path.Combine("TypesAndSchemas", "TypesEnum.raml"));

            // Assert
            result.Should().Be(expected);
        }
    }
}