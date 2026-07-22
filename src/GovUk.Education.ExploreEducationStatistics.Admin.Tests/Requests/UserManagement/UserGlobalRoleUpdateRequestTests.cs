#nullable enable
using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests.UserManagement;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Requests.UserManagement;

public class UserGlobalRoleUpdateRequestTests
{
    [Theory]
    [InlineData(GlobalRoles.Role.StandardUser)]
    [InlineData(GlobalRoles.Role.BauUser)]
    public void WhenObjectIsValid_ValidationPasses(GlobalRoles.Role targetGlobalRole)
    {
        var validator = new UserGlobalRoleUpdateRequest.Validator();

        var request = new UserGlobalRoleUpdateRequest { TargetGlobalRole = targetGlobalRole };
        var result = validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.TargetGlobalRole);
    }

    [Fact]
    public void WhenTargetGlobalRoleIsEmpty_ValidationFails()
    {
        var validator = new UserGlobalRoleUpdateRequest.Validator();

        var request = new UserGlobalRoleUpdateRequest { TargetGlobalRole = null };

        var result = validator.TestValidate(request);

        result
            .ShouldHaveValidationErrorFor(x => x.TargetGlobalRole)
            .WithErrorCode(FluentValidationKeys.NotNullValidator);
    }
}
