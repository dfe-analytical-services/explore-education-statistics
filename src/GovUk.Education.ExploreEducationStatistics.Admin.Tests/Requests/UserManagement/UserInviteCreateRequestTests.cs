#nullable enable
using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests.UserManagement;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Requests.UserManagement;

public class UserInviteCreateRequestTests
{
    [Fact]
    public void WhenObjectIsValid_ValidationPasses()
    {
        var validator = new UserInviteCreateRequest.Validator();

        var request = new UserInviteCreateRequest { Email = "validEmail@test.com", RoleId = "valid-role-id" };

        var result = validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void WhenValidEmailHasWhiteSpaceAndCapitalisation_ValidationTrimsWhiteSpaceAndConvertsToLowercaseAndPasses()
    {
        var validator = new UserInviteCreateRequest.Validator();

        var request = new UserInviteCreateRequest { Email = "  VALIDEMAIL@TEST.COM  ", RoleId = "valid-role-id" };

        var result = validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Email);

        // Assert mutation happened
        Assert.Equal("validemail@test.com", request.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void WhenEmailsIsEmpty_ValidationFails(string email)
    {
        var validator = new UserInviteCreateRequest.Validator();

        var request = new UserInviteCreateRequest { Email = email, RoleId = "valid-role-id" };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorCode(FluentValidationKeys.NotEmptyValidator);
    }

    [Fact]
    public void WhenEmailIsInvalid_ValidationFails()
    {
        var validator = new UserInviteCreateRequest.Validator();

        var request = new UserInviteCreateRequest { Email = "invalidEmail", RoleId = "valid-role-id" };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorCode(FluentValidationKeys.EmailValidator);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void WhenRoleIdIsEmpty_ValidationFails(string roleId)
    {
        var validator = new UserInviteCreateRequest.Validator();

        var request = new UserInviteCreateRequest { Email = "validEmail@test.com", RoleId = roleId };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.RoleId).WithErrorCode(FluentValidationKeys.NotEmptyValidator);
    }
}
