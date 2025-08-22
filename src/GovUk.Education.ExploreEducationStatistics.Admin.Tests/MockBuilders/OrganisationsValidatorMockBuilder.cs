#nullable enable
using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ValidationUtils = GovUk.Education.ExploreEducationStatistics.Common.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;

public class OrganisationsValidatorMockBuilder
{
    private readonly Mock<IOrganisationsValidator> _mock = new(MockBehavior.Strict);

    private Organisation[]? _organisations;
    private ErrorViewModel[]? _validationErrors;

    private static readonly Expression<Func<IOrganisationsValidator, Task<Either<ActionResult, Organisation[]>>>>
        ValidateOrganisations =
            m => m.ValidateOrganisations(
                It.IsAny<Guid[]?>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>());

    public IOrganisationsValidator Build()
    {
        _mock.Setup(ValidateOrganisations)
            .ReturnsAsync(_validationErrors?.Length > 0
                ? ValidationUtils.ValidationResult(_validationErrors)
                : _organisations ?? []);

        return _mock.Object;
    }

    public OrganisationsValidatorMockBuilder WhereHasOrganisations(Organisation[] organisations)
    {
        _organisations = organisations;
        return this;
    }

    public OrganisationsValidatorMockBuilder WhereHasValidationErrors(ErrorViewModel[] validationErrors)
    {
        _validationErrors = validationErrors;
        return this;
    }

    public Asserter Assert => new(_mock);

    public class Asserter(Mock<IOrganisationsValidator> mock)
    {
        public void ValidateOrganisationsWasCalled(
            Guid[]? expectedOrganisationIds = null,
            string? expectedPath = null) =>
            mock.Verify(m => m.ValidateOrganisations(
                    It.Is<Guid[]?>(organisationIds =>
                        expectedOrganisationIds == null || organisationIds == expectedOrganisationIds),
                    It.Is<string?>(path => expectedPath == null || path == expectedPath),
                    It.IsAny<CancellationToken>()),
                Times.Once);

        public void ValidateOrganisationsWasNotCalled() => mock.Verify(ValidateOrganisations, Times.Never);
    }
}
