#nullable enable
using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Requests;

public class ReleaseVersionPreReleaseAccessListUpdateRequestTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void WhenPreReleaseAccessListIsNullOrEmpty_ValidationFails(string? preReleaseAccessList)
    {
        // Arrange
        var validator = new ReleaseVersionPreReleaseAccessListUpdateRequest.Validator();
        var request = new ReleaseVersionPreReleaseAccessListUpdateRequest
        {
            PreReleaseAccessList = preReleaseAccessList,
        };

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result
            .ShouldHaveValidationErrorFor(r => r.PreReleaseAccessList)
            .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
    }

    [Fact]
    public void WhenPreReleaseAccessListIsNotNull_ValidationPasses()
    {
        // Arrange
        var validator = new ReleaseVersionPreReleaseAccessListUpdateRequest.Validator();
        var request = new ReleaseVersionPreReleaseAccessListUpdateRequest { PreReleaseAccessList = "access list" };

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(r => r.PreReleaseAccessList);
    }
}
