#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class ReplacementBatchServiceTests
{
    private readonly DataFixture _fixture = new();

    [Fact]
    public async Task Replace_Success()
    {
        var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

        var originalFileValid1Id = Guid.NewGuid();
        var originalFileValid2Id = Guid.NewGuid();

        var replacementService = new Mock<IReplacementService>(Strict);

        replacementService
            .Setup(mock => mock.Replace(releaseVersion.Id, originalFileValid1Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Instance);

        replacementService
            .Setup(mock => mock.Replace(releaseVersion.Id, originalFileValid2Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Instance);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var replacementBatchService = BuildReplacementBatchService(contentDbContext, replacementService.Object);

            var result = await replacementBatchService.Replace(
                releaseVersionId: releaseVersion.Id,
                originalFileIds: [originalFileValid1Id, originalFileValid2Id]
            );

            result.AssertRight();

            VerifyAllMocks(replacementService);
        }
    }

    [Fact]
    public async Task Replace_ReleaseVersionNotFound()
    {
        var replacementService = new Mock<IReplacementService>(Strict);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);
        var replacementBatchService = BuildReplacementBatchService(contentDbContext, replacementService.Object);

        var result = await replacementBatchService.Replace(
            releaseVersionId: Guid.NewGuid(), // releaseVersion does not exist
            originalFileIds: [Guid.NewGuid()]
        );

        result.AssertNotFound();

        VerifyAllMocks(replacementService);
    }

    [Fact]
    public async Task Replace_OneValidThreeErrors()
    {
        var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

        var originalFileValidId = Guid.NewGuid();
        var originalFileNotFoundId = Guid.NewGuid();
        var originalFileInvalidId = Guid.NewGuid();
        var originalFileImportNotCompleteId = Guid.NewGuid();

        var replacementService = new Mock<IReplacementService>(Strict);

        replacementService
            .Setup(mock => mock.Replace(releaseVersion.Id, originalFileValidId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Instance);

        replacementService
            .Setup(mock => mock.Replace(releaseVersion.Id, originalFileNotFoundId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotFoundResult());

        replacementService
            .Setup(mock => mock.Replace(releaseVersion.Id, originalFileInvalidId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidationUtils.ValidationActionResult(ReplacementMustBeValid));

        replacementService
            .Setup(mock =>
                mock.Replace(releaseVersion.Id, originalFileImportNotCompleteId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(ValidationUtils.ValidationActionResult(ReplacementImportMustBeComplete));

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var replacementBatchService = BuildReplacementBatchService(contentDbContext, replacementService.Object);

            var result = await replacementBatchService.Replace(
                releaseVersionId: releaseVersion.Id,
                originalFileIds:
                [
                    originalFileValidId,
                    originalFileInvalidId,
                    originalFileNotFoundId,
                    originalFileImportNotCompleteId,
                ]
            );

            // The valid replacement completes, but we still return errors for the invalid and not found
            // replacements as in normal usage the user should never see one of these errors.
            var validationProblem = result.AssertBadRequestWithValidationProblem();
            validationProblem.AssertHasErrors(
                [
                    ValidationMessages.GenerateErrorReplacementNotFound(originalFileNotFoundId),
                    ValidationMessages.GenerateErrorReplacementMustBeValid(originalFileInvalidId),
                    ValidationMessages.GenerateErrorReplacementImportMustBeComplete(originalFileImportNotCompleteId),
                ]
            );

            VerifyAllMocks(replacementService);
        }
    }

    private static ReplacementBatchService BuildReplacementBatchService(
        ContentDbContext contentDbContext,
        IReplacementService? replacementService = null
    )
    {
        return new ReplacementBatchService(
            contentDbContext,
            AlwaysTrueUserService().Object,
            replacementService ?? Mock.Of<IReplacementService>(Strict)
        );
    }
}
