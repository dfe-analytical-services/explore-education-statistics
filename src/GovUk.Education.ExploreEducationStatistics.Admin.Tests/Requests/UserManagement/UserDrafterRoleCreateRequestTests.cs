#nullable enable
using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests.UserManagement;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Requests.UserManagement;

public class UserDrafterRoleCreateRequestTests
{
    [Fact]
    public void WhenObjectIsValid_ValidationPasses()
    {
        var validator = new UserDrafterRoleCreateRequest.Validator();

        var request = new UserDrafterRoleCreateRequest
        {
            Email = "validEmail@test.com",
            PublicationId = Guid.NewGuid(),
        };

        var result = validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void WhenValidEmailHasWhiteSpaceAndCapitalisation_ValidationTrimsWhiteSpaceAndConvertsToLowercaseAndPasses()
    {
        var validator = new UserDrafterRoleCreateRequest.Validator();

        var request = new UserDrafterRoleCreateRequest
        {
            Email = "  VALIDEMAIL@TEST.COM  ",
            PublicationId = Guid.NewGuid(),
        };

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
        var validator = new UserDrafterRoleCreateRequest.Validator();

        var request = new UserDrafterRoleCreateRequest { Email = email, PublicationId = Guid.NewGuid() };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorCode(FluentValidationKeys.NotEmptyValidator);
    }

    [Fact]
    public void WhenEmailIsInvalid_ValidationFails()
    {
        var validator = new UserDrafterRoleCreateRequest.Validator();

        var request = new UserDrafterRoleCreateRequest { Email = "invalidEmail", PublicationId = Guid.NewGuid() };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorCode(FluentValidationKeys.EmailValidator);
    }

    [Fact]
    public void WhenPublicationIdIsDefault_ValidationFails()
    {
        var validator = new UserDrafterRoleCreateRequest.Validator();

        var request = new UserDrafterRoleCreateRequest { Email = "validEmail@test.com", PublicationId = Guid.Empty };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.PublicationId).WithErrorCode(FluentValidationKeys.NotEmptyValidator);
    }
}
