#nullable enable
using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests.UserManagement;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Requests.UserManagement;

public class UserPublicationRoleCreateRequestTests
{
    [Fact]
    public void WhenObjectIsValid_ValidationPasses()
    {
        var validator = new UserPublicationRoleCreateRequest.Validator();

        var request = new UserPublicationRoleCreateRequest { PublicationId = Guid.NewGuid() };

        var result = validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.PublicationId);
    }

    [Fact]
    public void WhenPublicationIdIsDefault_ValidationFails()
    {
        var validator = new UserPublicationRoleCreateRequest.Validator();

        var request = new UserPublicationRoleCreateRequest { PublicationId = Guid.Empty };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.PublicationId).WithErrorCode(FluentValidationKeys.NotEmptyValidator);
    }
}
