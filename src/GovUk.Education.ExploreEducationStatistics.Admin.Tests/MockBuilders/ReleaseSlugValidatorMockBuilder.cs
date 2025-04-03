using System;
using System.Threading;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;

public class ReleaseSlugValidatorMockBuilder
{
    private readonly Mock<IReleaseSlugValidator> _mock = new(MockBehavior.Strict);
    public IReleaseSlugValidator Build() => _mock.Object;

    public ReleaseSlugValidatorMockBuilder()
    {
        _mock
            .Setup(m => m.ValidateNewSlug(
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Instance);
    }

    public ReleaseSlugValidatorMockBuilder SetValidationToFail(
        ValidationErrorMessages validationErrorMessage,
        string releaseSlug = null,
        Guid? publicationId = null,
        Guid? releaseId = null)
    {
        _mock
            .Setup(m => m.ValidateNewSlug(
                It.Is<string>(slug => releaseSlug == null || slug == releaseSlug),
                It.Is<Guid>(id => publicationId == null || id == publicationId),
                It.Is<Guid?>(id => releaseId == null || id == releaseId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidationUtils.ValidationActionResult(validationErrorMessage));
        
        return this;
    }
}
