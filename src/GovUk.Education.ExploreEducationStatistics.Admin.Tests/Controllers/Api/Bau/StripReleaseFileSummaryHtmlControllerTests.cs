#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Bau;

public abstract class StripReleaseFileSummaryHtmlControllerTests
{
    private readonly DataFixture _dataFixture = new();

    private const string HtmlSummary = "<p>Test paragraph with <strong>bold text</strong></p>";
    private const string HtmlSummaryAsPlainText = "Test paragraph with bold text";

    public class DryRunStripSummaryHtmlTests : StripReleaseFileSummaryHtmlControllerTests
    {
        [Fact]
        public async Task WhenSummaryIsHtml_ReportsChangeWithoutApplyingIt()
        {
            // Arrange
            ReleaseFile releaseFile = DefaultReleaseFile().WithSummary(HtmlSummary);

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.ReleaseFiles.Add(releaseFile);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var sqlExecutor = new Mock<IRawSqlExecutor>(Strict);
                var controller = BuildController(context, sqlExecutor.Object);

                // Act
                var result = await controller.DryRunStripSummaryHtml();

                // Assert
                var report = result.AssertOkResult();

                Assert.Equal(1, report.CandidateCount);
                var change = Assert.Single(report.Changes);
                Assert.Equal(releaseFile.Id, change.ReleaseFileId);
                Assert.Equal(HtmlSummary, change.Current);
                Assert.Equal(HtmlSummaryAsPlainText, change.PlainText);

                // The summary itself is left untouched, and no backup is taken.
                VerifyAllMocks(sqlExecutor);
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                Assert.Equal(HtmlSummary, context.ReleaseFiles.Single().Summary);
            }
        }

        [Fact]
        public async Task WhenSummaryIsPlainTextContainingAmpersand_ReportsNoChange()
        {
            // Arrange
            // This matches the query looking for character entities, but converting it changes nothing.
            ReleaseFile releaseFile = DefaultReleaseFile().WithSummary("Fees & charges; see notes");

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.ReleaseFiles.Add(releaseFile);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var controller = BuildController(context);

                // Act
                var result = await controller.DryRunStripSummaryHtml();

                // Assert
                var report = result.AssertOkResult();

                Assert.Equal(1, report.CandidateCount);
                Assert.Empty(report.Changes);
            }
        }

        [Fact]
        public async Task WhenSummaryIsPlainText_IsNotACandidate()
        {
            // Arrange
            ReleaseFile releaseFile = DefaultReleaseFile().WithSummary("A plain text summary");

            ReleaseFile releaseFileWithoutSummary = DefaultReleaseFile().WithSummary(null);

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.ReleaseFiles.AddRange(releaseFile, releaseFileWithoutSummary);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var controller = BuildController(context);

                // Act
                var result = await controller.DryRunStripSummaryHtml();

                // Assert
                var report = result.AssertOkResult();

                Assert.Equal(0, report.CandidateCount);
                Assert.Empty(report.Changes);
            }
        }
    }

    public class StripSummaryHtmlTests : StripReleaseFileSummaryHtmlControllerTests
    {
        [Fact]
        public async Task WhenSummaryIsHtml_ConvertsItToPlainTextAfterBackingItUp()
        {
            // Arrange
            ReleaseFile releaseFile = DefaultReleaseFile().WithSummary(HtmlSummary);

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.ReleaseFiles.Add(releaseFile);
                await context.SaveChangesAsync();
            }

            var capturedSql = new List<string>();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var sqlExecutor = new Mock<IRawSqlExecutor>(Strict);

                sqlExecutor
                    .Setup(s =>
                        s.ExecuteSqlRaw(
                            It.IsAny<ContentDbContext>(),
                            Capture.In(capturedSql),
                            It.IsAny<CancellationToken>()
                        )
                    )
                    .Returns(Task.CompletedTask);

                var controller = BuildController(context, sqlExecutor.Object);

                // Act
                var result = await controller.StripSummaryHtml();

                // Assert
                var report = result.AssertOkResult();

                Assert.Equal(1, report.ChangeCount);

                VerifyAllMocks(sqlExecutor);
            }

            var backupSql = Assert.Single(capturedSql);
            Assert.Contains("ReleaseFiles_Ees7367_SummaryBackup", backupSql, StringComparison.Ordinal);

            await using (var context = InMemoryContentDbContext(contextId))
            {
                Assert.Equal(HtmlSummaryAsPlainText, context.ReleaseFiles.Single().Summary);
            }
        }

        [Fact]
        public async Task WhenSummaryIsPlainText_LeavesItUnchanged()
        {
            // Arrange
            const string summary = "Fees & charges; see notes";

            ReleaseFile releaseFile = DefaultReleaseFile().WithSummary(summary);

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.ReleaseFiles.Add(releaseFile);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var controller = BuildController(context);

                // Act
                var result = await controller.StripSummaryHtml();

                // Assert
                var report = result.AssertOkResult();

                Assert.Equal(0, report.ChangeCount);
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                Assert.Equal(summary, context.ReleaseFiles.Single().Summary);
            }
        }

        [Fact]
        public async Task WhenMoreCandidatesThanLimit_ConvertsUpToLimitAndReportsTheRest()
        {
            // Arrange
            var releaseFiles = DefaultReleaseFile().WithSummary(HtmlSummary).GenerateList(3);

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.ReleaseFiles.AddRange(releaseFiles);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var controller = BuildController(context);

                // Act
                var result = await controller.StripSummaryHtml(limit: 2);

                // Assert
                var report = result.AssertOkResult();

                Assert.Equal(3, report.CandidateCount);
                Assert.Equal(2, report.ChangeCount);
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                // Only the remaining summary still contains HTML, so a further request would convert it.
                var summaries = context.ReleaseFiles.Select(rf => rf.Summary).ToList();

                Assert.Equal(2, summaries.Count(summary => summary == HtmlSummaryAsPlainText));
                Assert.Single(summaries, summary => summary == HtmlSummary);
            }
        }
    }

    /// <summary>
    /// Release files are filtered by whether their release version is soft deleted, so each one needs a
    /// release version to be visible to the controller's queries at all.
    /// </summary>
    private Generator<ReleaseFile> DefaultReleaseFile()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion();

        return _dataFixture.DefaultReleaseFile().WithReleaseVersion(releaseVersion);
    }

    private static StripReleaseFileSummaryHtmlController BuildController(
        ContentDbContext contentDbContext,
        IRawSqlExecutor? sqlExecutor = null
    ) => new(contentDbContext, sqlExecutor ?? Mock.Of<IRawSqlExecutor>());
}
