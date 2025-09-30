using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;

public static class BoundaryDataUtilsTests
{
    public class GetCodeTests
    {
        [Fact]
        public void GetCode_NoMatchingSuffix_ThrowsArgumentException()
        {
            // Arrange
            var properties = new Dictionary<string, object>();

            // Act & Assert
            var result = Assert.Throws<ArgumentException>(() =>
                BoundaryDataUtils.GetCode(properties)
            );
            Assert.Equal("Required key not found (expects key ending 'CD')", result.Message);
        }

        [Fact]
        public void GetCode__DuplicateSuffix_ThrowsInvalidOperationException()
        {
            // Arrange
            var properties = new Dictionary<string, object>()
            {
                { "ctry17cd", "E92000001" },
                { "abc123cd", "test" },
            };

            // Act & Assert
            var result = Assert.Throws<InvalidOperationException>(() =>
                BoundaryDataUtils.GetCode(properties)
            );
            Assert.Equal("Sequence contains more than one matching element", result.Message);
        }

        [Fact]
        public void GetCode_EmptyValue_ReturnsEmptyString()
        {
            // Arrange
            var properties = new Dictionary<string, object>() { { "ctry17cd", "" } };

            // Act
            var result = BoundaryDataUtils.GetCode(properties);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void GetCode_ReturnsCode()
        {
            // Arrange
            var properties = new Dictionary<string, object>() { { "ctry17cd", "E92000001" } };

            // Act
            var result = BoundaryDataUtils.GetCode(properties);

            // Assert
            Assert.Equal("E92000001", result);
        }
    }

    public class GetNameTests
    {
        [Fact]
        public void GetName_NoMatchingSuffix_ThrowsArgumentException()
        {
            // Arrange
            var properties = new Dictionary<string, object>();

            // Act & Assert
            var result = Assert.Throws<ArgumentException>(() =>
                BoundaryDataUtils.GetName(properties)
            );
            Assert.Equal("Required key not found (expects key ending 'NM')", result.Message);
        }

        [Fact]
        public void GetName__DuplicateSuffix_ThrowsInvalidOperationException()
        {
            // Arrange
            var properties = new Dictionary<string, object>()
            {
                { "ctry17nm", "England" },
                { "abc123nm", "test" },
            };

            // Act & Assert
            var result = Assert.Throws<InvalidOperationException>(() =>
                BoundaryDataUtils.GetName(properties)
            );
            Assert.Equal("Sequence contains more than one matching element", result.Message);
        }

        [Fact]
        public void GetName_EmptyValue_ReturnsEmptyString()
        {
            // Arrange
            var properties = new Dictionary<string, object>() { { "ctry17nm", "" } };

            // Act
            var result = BoundaryDataUtils.GetName(properties);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void GetName_ReturnsName()
        {
            // Arrange
            var properties = new Dictionary<string, object>() { { "ctry17nm", "England" } };

            // Act
            var result = BoundaryDataUtils.GetName(properties);

            // Assert
            Assert.Equal("England", result);
        }
    }

    public class CombinedGetTests
    {
        [Fact]
        public void Get_UpperCaseKeys_ReturnsValue()
        {
            // Arrange
            var properties = new Dictionary<string, object>()
            {
                { "CTRY17CD", "E92000001" },
                { "CTRY17NM", "England" },
            };

            // Act
            var codeResult = BoundaryDataUtils.GetCode(properties);
            var nameResult = BoundaryDataUtils.GetName(properties);

            // Assert
            Assert.Equal("E92000001", codeResult);
            Assert.Equal("England", nameResult);
        }
    }
}
