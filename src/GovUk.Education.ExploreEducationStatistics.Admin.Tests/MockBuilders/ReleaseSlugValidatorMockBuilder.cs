using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;

public class ReleaseSlugValidatorMockBuilder
{
    private readonly Mock<IReleaseSlugValidator> _mock = new(MockBehavior.Strict);

    private static readonly Expression<Func<IReleaseSlugValidator, Task<Either<ActionResult, Unit>>>> ValidateNewSlug =
        m => m.ValidateNewSlug(
            It.IsAny<string>(),
            It.IsAny<Guid>(),
            It.IsAny<Guid?>(),
            It.IsAny<CancellationToken>());

    public IReleaseSlugValidator Build() => _mock.Object;

    public ReleaseSlugValidatorMockBuilder()
    {
        _mock
            .Setup(ValidateNewSlug)
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

    public class Asserter(Mock<IReleaseSlugValidator> mock)
    {
        public void ValidateNewSlugWasCalled(
            string expectedNewReleaseSlug = null,
            Guid? expectedPublicationId = null,
            Guid? expectedReleaseId = null) =>
            mock.Verify(m => m.ValidateNewSlug(
                It.Is<string>(newReleaseSlug => expectedNewReleaseSlug == null || newReleaseSlug == expectedNewReleaseSlug),
                It.Is<Guid>(publicationId => expectedPublicationId == null || publicationId == expectedPublicationId),
                It.Is<Guid?>(releaseId => expectedReleaseId == null || releaseId == expectedReleaseId),
                It.IsAny<CancellationToken>()),
                Times.Once);

        public void ValidateNewSlugWasNotCalled() =>
            mock.Verify(ValidateNewSlug, Times.Never);
    }
    public Asserter Assert => new(_mock);
}
