using GovUk.Education.ExploreEducationStatistics.Admin.Validators;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Validators;

public class FileNameValidatorsTests
{
    [Theory]
    [InlineData("short.csv")]
    [InlineData("file-with-no-extension")]
    [InlineData("meeeeeeeeeee_eeeeeeeeeeeeeeeeeeeeeeeeee-eeeeeeeeeeeeeeeeeeeeedium.csv")] // 65 chars without extension
    [InlineData("looooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooong.csv")] // 150 chars without extension
    [InlineData("loooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooong.with.periods.csv")] // 150 chars without extension
    public void MeetsLengthRequirements_Valid_ReturnsTrue(string fileName)
    {
        // Arrange
        // Act
        var result = FileNameValidators.MeetsLengthRequirements(fileName);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void MeetsLengthRequirements_EmptyString_ReturnsFalse()
    {
        // Act
        var result = FileNameValidators.MeetsLengthRequirements("");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void MeetsLengthRequirements_TooLong_ReturnsFalse()
    {
        // Arrange
        var fileName = new string('a', FileNameValidators.MaxFileNameSize + 1);

        // Act
        var result = FileNameValidators.MeetsLengthRequirements($"{fileName}.csv");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void MeetsLengthRequirements_TooLongWithNoExtension_ReturnsFalse()
    {
        // Arrange
        var fileName = new string('a', FileNameValidators.MaxFileNameSize + 1);

        // Act
        var result = FileNameValidators.MeetsLengthRequirements(fileName);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void MeetsLengthRequirements_TooLongWithAdditionalPeriods_ReturnsFalse()
    {
        // Arrange
        var fileName = new string('a', 127);

        // Act
        var result = FileNameValidators.MeetsLengthRequirements($"{fileName}.with.additional.periods.csv"); // 151 chars without extension

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("file.csv")]
    [InlineData("fi¶le.csv")]
    [InlineData("fi&nbsp;le.csv")]
    [InlineData("fi\tle.csv")]
    [InlineData("fi\nle.csv")]
    [InlineData("fi\\sle.csv")]
    public void DoesNotContainSpaces_Valid_ReturnsTrue(string fileName)
    {
        // Act
        var result = FileNameValidators.DoesNotContainSpaces(fileName);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("fi le.csv")]
    public void DoesNotContainSpaces_ContainsASpace_ReturnsFalse(string fileName)
    {
        // Act
        var result = FileNameValidators.DoesNotContainSpaces(fileName);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("file-name.csv")]
    public void DoesNotContainSpecialChars_Valid_ReturnsTrue(string fileName)
    {
        // Act
        var result = FileNameValidators.DoesNotContainSpecialChars(fileName);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("file&name.csv")]
    [InlineData("file\\name.csv")]
    [InlineData("file/name.csv")]
    [InlineData("file|name.csv")]
    [InlineData("file:name.csv")]
    [InlineData("file?name.csv")]
    [InlineData("file>name.csv")]
    [InlineData("file\"name.csv")]
    public void DoesNotContainSpecialChars_ContainsInvalidChars_ReturnsFalse(string fileName)
    {
        // Act
        var result = FileNameValidators.DoesNotContainSpecialChars(fileName);

        // Assert
        Assert.False(result);
    }
}
