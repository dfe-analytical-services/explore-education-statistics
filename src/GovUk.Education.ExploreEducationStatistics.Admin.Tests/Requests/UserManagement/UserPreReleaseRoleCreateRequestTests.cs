#nullable enable
using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests.UserManagement;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Requests.UserManagement;

public class UserPreReleaseRoleCreateRequestTests
{
    [Fact]
    public void WhenObjectIsValid_ValidationPasses()
    {
        var validator = new UserPreReleaseRoleCreateRequest.Validator();

        var request = new UserPreReleaseRoleCreateRequest { ReleaseId = Guid.NewGuid() };

        var result = validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.ReleaseId);
    }

    [Fact]
    public void WhenReleaseIdIsDefault_ValidationFails()
    {
        var validator = new UserPreReleaseRoleCreateRequest.Validator();

        var request = new UserPreReleaseRoleCreateRequest { ReleaseId = Guid.Empty };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.ReleaseId).WithErrorCode(FluentValidationKeys.NotEmptyValidator);
    }
}
