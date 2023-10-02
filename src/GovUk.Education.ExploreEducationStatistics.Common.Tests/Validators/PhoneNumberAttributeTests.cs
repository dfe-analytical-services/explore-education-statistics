#nullable enable
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Validators;

public class PhoneNumberAttributeTests
{
    [Theory]
    [InlineData("01234567")]
    [InlineData("0   123 4567")]
    [InlineData("0 1 2 3   4 5 6 7      ")]
    [InlineData("")]
    [InlineData("    ")]
    public void AllowedValues_Valid(string value)
    {
        var attribute = new PhoneNumberAttribute();
        var result = attribute.GetValidationResult(value, new ValidationContext(value));

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void AllowedValues_Null()
    {
        var attribute = new PhoneNumberAttribute();
        string? value = null;
        var result = attribute.GetValidationResult(value, new ValidationContext("test")); // Have to give ValidationContext something that isn't null

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void DisallowedValues_NotString()
    {
        var attribute = new PhoneNumberAttribute();
        var value = 42;
        var result = attribute.GetValidationResult(value, new ValidationContext(value));

        Assert.Equal("A phone number must be a string", result?.ErrorMessage);
    }

    [Fact]
    public void DisallowedValues_TooShort()
    {
        var attribute = new PhoneNumberAttribute();
        var value = "0123456";
        var result = attribute.GetValidationResult(value, new ValidationContext(value));

        Assert.Equal("The value must have a minimum length of 8.", result?.ErrorMessage);
    }

    [Theory]
    [InlineData("012  34567a")]
    [InlineData(@"0!£$%^&*'`()_+=")]
    [InlineData("1234567890")]
    [InlineData("a12345678")]
    [InlineData("0  12345678 !")]
    public void DisallowedValues_NotNumericOrNotStartWithZero(string value)
    {
        var attribute = new PhoneNumberAttribute();
        var result = attribute.GetValidationResult(value, new ValidationContext(value));

        Assert.Equal("The value must start with a '0' and  contain only numbers or spaces.", result?.ErrorMessage);
    }
}
