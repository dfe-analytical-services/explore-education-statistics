#nullable enable
using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests.UserManagement;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Requests.UserManagement;

public class UserEditRequestTests
{
    [Theory]
    [InlineData("abc")]
    [InlineData("123")]
    public void WhenObjectIsValid_ValidationPasses(string roleId)
    {
        var validator = new UserGlobalRoleUpdateRequest.Validator();

        var request = new UserGlobalRoleUpdateRequest { RoleId = roleId };

        var result = validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.RoleId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void WhenRoleIdIsEmpty_ValidationFails(string roleId)
    {
        var validator = new UserGlobalRoleUpdateRequest.Validator();

        var request = new UserGlobalRoleUpdateRequest { RoleId = roleId };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.RoleId).WithErrorCode(FluentValidationKeys.NotEmptyValidator);
    }
}
