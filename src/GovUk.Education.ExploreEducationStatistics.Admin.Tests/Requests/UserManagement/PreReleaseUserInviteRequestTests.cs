#nullable enable
using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests.UserManagement;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Requests.UserManagement;

public class PreReleaseUserInviteRequestTests
{
    [Fact]
    public void WhenEmailsIsEmpty_ValidationFails()
    {
        var validator = new PreReleaseUserInviteRequest.Validator();

        var request = new PreReleaseUserInviteRequest { Emails = [] };

        var result = validator.TestValidate(request);

        result
            .ShouldHaveValidationErrorFor(x => x.Emails)
            .WithErrorCode(FluentValidationKeys.NotEmptyValidator)
            .WithErrorMessage("Must have at least one email.");
    }

    [Fact]
    public void WhenEmailsSuppliedContainAnInvalidEmail_ValidationFails()
    {
        var validator = new PreReleaseUserInviteRequest.Validator();
        var request = new PreReleaseUserInviteRequest
        {
            Emails = ["validEmail@test.com", "invalidEmail", "invalidEmail2"],
        };

        var result = validator.TestValidate(request);

        result
            .ShouldHaveValidationErrorFor("Emails[1]")
            .WithErrorCode(FluentValidationKeys.EmailValidator)
            .WithErrorMessage("'invalidemail' is not a valid email address.");

        result
            .ShouldHaveValidationErrorFor("Emails[2]")
            .WithErrorCode(FluentValidationKeys.EmailValidator)
            .WithErrorMessage("'invalidemail2' is not a valid email address.");
    }

    [Fact]
    public void WhenEmailsAreAllValid_ValidationPasses()
    {
        var validator = new PreReleaseUserInviteRequest.Validator();
        var request = new PreReleaseUserInviteRequest { Emails = ["validEmail@test.com", "validEmail2@test.co.uk"] };

        var result = validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(r => r.Emails);
    }

    [Fact]
    public void WhenValidEmailsHaveWhiteSpaceAndCapitalisation_ValidationTrimsWhiteSpaceAndConvertsToLowercaseAndPasses()
    {
        var validator = new PreReleaseUserInviteRequest.Validator();

        var request = new PreReleaseUserInviteRequest
        {
            Emails = ["  VALIDEMAIL@TEST.COM  ", "  VALIDEMAIL2@TEST.COM  "],
        };

        var result = validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Emails);

        // Assert mutation happened
        Assert.Equal("validemail@test.com", request.Emails[0]);
        Assert.Equal("validemail2@test.com", request.Emails[1]);
    }
}
