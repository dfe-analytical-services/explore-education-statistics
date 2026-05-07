#nullable enable
using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests.UserManagement;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Requests.UserManagement;

public class PreReleaseUserRemoveRequestTests
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void WhenEmailIsEmpty_ValidationFails(string email)
    {
        var validator = new PreReleaseUserRemoveRequest.Validator();

        var request = new PreReleaseUserRemoveRequest { Email = email };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorCode(FluentValidationKeys.NotEmptyValidator);
    }

    [Fact]
    public void WhenValidEmailHasWhiteSpaceAndCapitalisation_ValidationTrimsWhiteSpaceAndConvertsToLowercaseAndPasses()
    {
        var validator = new PreReleaseUserRemoveRequest.Validator();

        var request = new PreReleaseUserRemoveRequest { Email = "  VALIDEMAIL@TEST.COM  " };

        var result = validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Email);

        // Assert mutation happened
        Assert.Equal("validemail@test.com", request.Email);
    }

    [Fact]
    public void WhenEmailIsInvalid_ValidationFails()
    {
        var validator = new PreReleaseUserRemoveRequest.Validator();

        var request = new PreReleaseUserRemoveRequest { Email = "invalidEmail" };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorCode(FluentValidationKeys.EmailValidator);
    }

    [Fact]
    public void WhenObjectIsValid_ValidationPasses()
    {
        var validator = new PreReleaseUserRemoveRequest.Validator();

        var request = new PreReleaseUserRemoveRequest { Email = "validEmail@test.com" };

        var result = validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }
}
