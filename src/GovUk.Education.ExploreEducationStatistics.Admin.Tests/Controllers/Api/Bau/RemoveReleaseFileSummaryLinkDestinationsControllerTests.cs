#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau;
using GovUk.Education.ExploreEducationStatistics.Admin.Repositories;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Bau;

public abstract class RemoveReleaseFileSummaryLinkDestinationsControllerTests
{
    private const string Html =
        "<p>Read <a href=\"https://example.com/a(b)\">report</a>; keep (https://intentional.example).</p>";
    private const string Converted = "Read report (https://example.com/a(b)); keep (https://intentional.example).";
    private const string Cleaned = "Read report; keep (https://intentional.example).";
    private readonly DataFixture _fixture = new();

    private ReleaseFile File(string? summary)
    {
        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion();
        return _fixture.DefaultReleaseFile().WithReleaseVersion(releaseVersion).WithSummary(summary);
    }

    private static Mock<IReleaseFileSummaryLinkCleanupRepository> Repository(
        bool exists,
        params (Guid Id, string? Html)[] sources
    )
    {
        var repository = new Mock<IReleaseFileSummaryLinkCleanupRepository>(MockBehavior.Strict);
        repository
            .Setup(repo => repo.GetOriginalSummaries(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new SummaryHtmlBackup(exists, sources.ToDictionary(source => source.Id, source => source.Html))
            );
        return repository;
    }

    public class DryRunTests : RemoveReleaseFileSummaryLinkDestinationsControllerTests
    {
        [Fact]
        public async Task ReconstructsFromHtmlAndDoesNotWrite()
        {
            await using var context = InMemoryContentDbContext();
            var file = File(Converted);
            context.ReleaseFiles.Add(file);
            await context.SaveChangesAsync();
            var repository = Repository(true, (file.Id, Html));
            var controller = new RemoveReleaseFileSummaryLinkDestinationsController(context, repository.Object);

            var report = (await controller.DryRun()).AssertOkResult();

            Assert.True(report.SourceBackupExists);
            Assert.Equal(1, report.TotalSummaryCount);
            Assert.Equal(1, report.InspectedCount);
            Assert.Null(report.NextOffset);
            Assert.Equal(0, report.AppliedCount);
            Assert.Equal(
                new SummaryLinkCleanupChangeViewModel(file.Id, Converted, Cleaned),
                Assert.Single(report.Changes)
            );
            Assert.Empty(report.Skipped);
            Assert.Equal(Converted, context.ReleaseFiles.Single().Summary);
            repository.Verify(
                repo =>
                    repo.TryApply(
                        It.IsAny<Guid>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Never
            );
        }

        [Theory]
        [InlineData(false, Html, Converted, "MissingSourceBackup")]
        [InlineData(true, null, Converted, "MissingSourceHtml")]
        [InlineData(
            true,
            Html,
            "Edited report (https://example.com/a(b)).",
            "CurrentSummaryDoesNotMatchSourceConversion"
        )]
        [InlineData(true, Html, Converted + " ", "CurrentSummaryDoesNotMatchSourceConversion")]
        [InlineData(
            true,
            Html,
            "read report (https://example.com/a(b)); keep (https://intentional.example).",
            "CurrentSummaryDoesNotMatchSourceConversion"
        )]
        [InlineData(true, Html, Html, "CurrentSummaryDoesNotMatchSourceConversion")]
        [InlineData(true, Html, Cleaned, "AlreadyCleaned")]
        [InlineData(
            true,
            "<p>Keep (https://intentional.example).</p>",
            "Keep (https://intentional.example).",
            "NoLinkDestinations"
        )]
        public async Task ReportsSkipReasonAndNeverApplies(bool exists, string? html, string current, string reason)
        {
            await using var context = InMemoryContentDbContext();
            var file = File(current);
            context.ReleaseFiles.Add(file);
            await context.SaveChangesAsync();
            var repository = Repository(exists, (file.Id, html));
            var controller = new RemoveReleaseFileSummaryLinkDestinationsController(context, repository.Object);

            var report = (await controller.Apply()).AssertOkResult();

            Assert.Empty(report.Changes);
            Assert.Equal(new SummaryLinkCleanupSkipViewModel(file.Id, reason), Assert.Single(report.Skipped));
            Assert.Equal(1, report.SkippedCount);
            Assert.Equal(0, report.AppliedCount);
            Assert.Equal(current, context.ReleaseFiles.Single().Summary);
            repository.Verify(
                repo =>
                    repo.TryApply(
                        It.IsAny<Guid>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Never
            );
        }

        [Fact]
        public async Task MissingBackupRowIsSkipped()
        {
            await using var context = InMemoryContentDbContext();
            context.ReleaseFiles.Add(File(Converted));
            await context.SaveChangesAsync();
            var report = (
                await new RemoveReleaseFileSummaryLinkDestinationsController(context, Repository(true).Object).DryRun()
            ).AssertOkResult();
            Assert.Equal("MissingSourceHtml", Assert.Single(report.Skipped).Reason);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(501, 0)]
        [InlineData(1, -1)]
        public async Task InvalidBoundsAreRejectedBeforeAnyRead(int limit, int offset)
        {
            await using var context = InMemoryContentDbContext();
            var repository = new Mock<IReleaseFileSummaryLinkCleanupRepository>(MockBehavior.Strict);
            var controller = new RemoveReleaseFileSummaryLinkDestinationsController(context, repository.Object);
            Assert.IsType<BadRequestObjectResult>((await controller.DryRun(limit, offset)).Result);
            Assert.IsType<BadRequestObjectResult>((await controller.Apply(limit, offset)).Result);
            repository.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task PagesPastSkippedRowsAndExcludesNullSummaries()
        {
            await using var context = InMemoryContentDbContext();
            var files = new[] { File("Unrelated text"), File(Converted), File(Converted), File(null) };
            context.ReleaseFiles.AddRange(files);
            await context.SaveChangesAsync();
            var repository = Repository(true, files.Select(file => (file.Id, (string?)Html)).ToArray());
            var controller = new RemoveReleaseFileSummaryLinkDestinationsController(context, repository.Object);

            var first = (await controller.DryRun(limit: 1)).AssertOkResult();
            var second = (await controller.DryRun(limit: 1, offset: first.NextOffset!.Value)).AssertOkResult();
            var third = (await controller.DryRun(limit: 1, offset: second.NextOffset!.Value)).AssertOkResult();

            Assert.Equal(3, first.TotalSummaryCount);
            Assert.Equal(1, first.InspectedCount);
            Assert.Equal(2, second.NextOffset);
            Assert.Null(third.NextOffset);
            var inspected = new[] { first, second, third }
                .SelectMany(report =>
                    report
                        .Changes.Select(change => change.ReleaseFileId)
                        .Concat(report.Skipped.Select(skip => skip.ReleaseFileId))
                )
                .ToList();
            Assert.Equal(3, inspected.Distinct().Count());
            Assert.DoesNotContain(files[3].Id, inspected);
        }
    }

    public class ApplyTests : RemoveReleaseFileSummaryLinkDestinationsControllerTests
    {
        [Fact]
        public async Task AppliesExactCurrentValueAndRepeatedRequestIsSafe()
        {
            var contextId = Guid.NewGuid().ToString();
            await using var context = InMemoryContentDbContext(contextId);
            var file = File(Converted);
            context.ReleaseFiles.Add(file);
            await context.SaveChangesAsync();
            var repository = Repository(true, (file.Id, Html));
            repository
                .Setup(repo => repo.TryApply(file.Id, Converted, Cleaned, It.IsAny<CancellationToken>()))
                .Returns(async () =>
                {
                    await using var writeContext = InMemoryContentDbContext(contextId);
                    writeContext.ReleaseFiles.Single().Summary = Cleaned;
                    await writeContext.SaveChangesAsync();
                    return true;
                });
            var controller = new RemoveReleaseFileSummaryLinkDestinationsController(context, repository.Object);

            var first = (await controller.Apply()).AssertOkResult();
            var second = (await controller.Apply()).AssertOkResult();

            Assert.Equal(1, first.AppliedCount);
            Assert.Equal(Cleaned, Assert.Single(first.Changes).PlainText);
            Assert.Empty(second.Changes);
            Assert.Equal("AlreadyCleaned", Assert.Single(second.Skipped).Reason);
            repository.Verify(
                repo => repo.TryApply(file.Id, Converted, Cleaned, It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Fact]
        public async Task ChangedValueDuringWriteIsReportedAndNotClaimedAsApplied()
        {
            await using var context = InMemoryContentDbContext();
            var file = File(Converted);
            context.ReleaseFiles.Add(file);
            await context.SaveChangesAsync();
            var repository = Repository(true, (file.Id, Html));
            repository
                .Setup(repo => repo.TryApply(file.Id, Converted, Cleaned, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var report = (
                await new RemoveReleaseFileSummaryLinkDestinationsController(context, repository.Object).Apply()
            ).AssertOkResult();

            Assert.Equal(0, report.AppliedCount);
            Assert.Empty(report.Changes);
            Assert.Equal("ConcurrentChangeOrExistingCleanupBackup", Assert.Single(report.Skipped).Reason);
            Assert.Equal(Converted, context.ReleaseFiles.Single().Summary);
        }
    }
}
