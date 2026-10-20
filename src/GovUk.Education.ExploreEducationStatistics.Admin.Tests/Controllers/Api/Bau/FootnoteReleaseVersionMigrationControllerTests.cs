#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Bau;

public class FootnoteReleaseVersionMigrationControllerTests
{
    private readonly DataFixture _fixture = new();

    [Fact]
    public async Task SharedFootnote_ForkedForEveryReleaseVersionButFirst()
    {
        // Arrange
        var releaseVersions = _fixture.DefaultStatsReleaseVersion().GenerateList(3);

        Subject subject = _fixture
            .DefaultSubject()
            .WithFilters(_fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 1).Generate(1))
            .WithIndicatorGroups(
                _fixture.DefaultIndicatorGroup().WithIndicators(_fixture.DefaultIndicator().Generate(1)).Generate(1)
            );

        Footnote footnote = _fixture
            .DefaultFootnote()
            .WithSubjects(ListOf(subject))
            .WithFilters(subject.Filters)
            .WithFilterGroups(subject.Filters[0].FilterGroups)
            .WithFilterItems(subject.Filters[0].FilterGroups[0].FilterItems)
            .WithIndicators(subject.IndicatorGroups[0].Indicators);

        var releaseFootnotes = releaseVersions
            .Select(releaseVersion =>
                (ReleaseFootnote)
                    _fixture.DefaultReleaseFootnote().WithReleaseVersion(releaseVersion).WithFootnote(footnote)
            )
            .ToList();

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryStatisticsDbContext(contextId))
        {
            context.ReleaseFootnote.AddRange(releaseFootnotes);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryStatisticsDbContext(contextId))
        {
            var controller = BuildController(context);

            // Act
            var result = await controller.MigrateFootnoteReleaseVersions(dryRun: false);

            // Assert
            Assert.False(result.IsDryRun);
            Assert.Equal(2, result.ForkedFootnotes);
            Assert.Equal(0, result.DeletedOrphanedFootnotes);
            Assert.Equal(1, result.ReleaseVersionIdsSet);
            Assert.Equal(0, result.Remaining);
        }

        await using (var context = InMemoryStatisticsDbContext(contextId))
        {
            var savedFootnotes = await context
                .Footnote.Include(f => f.Releases)
                .Include(f => f.Subjects)
                .Include(f => f.Filters)
                .Include(f => f.FilterGroups)
                .Include(f => f.FilterItems)
                .Include(f => f.Indicators)
                .ToListAsync();

            Assert.Equal(3, savedFootnotes.Count);
            Assert.Contains(savedFootnotes, f => f.Id == footnote.Id);
            Assert.Equal(
                releaseVersions.Select(rv => rv.Id).Order(),
                savedFootnotes.Select(f => f.ReleaseVersionId!.Value).Order()
            );

            Assert.All(
                savedFootnotes,
                savedFootnote =>
                {
                    var releaseFootnote = Assert.Single(savedFootnote.Releases);
                    Assert.Equal(releaseFootnote.ReleaseVersionId, savedFootnote.ReleaseVersionId);

                    Assert.Equal(footnote.Content, savedFootnote.Content);
                    Assert.Equal(footnote.Order, savedFootnote.Order);
                    Assert.Equal(footnote.Created, savedFootnote.Created);
                    Assert.Equal(footnote.Updated, savedFootnote.Updated);

                    Assert.Equal(subject.Id, Assert.Single(savedFootnote.Subjects).SubjectId);
                    Assert.Equal(subject.Filters[0].Id, Assert.Single(savedFootnote.Filters).FilterId);
                    Assert.Equal(
                        subject.Filters[0].FilterGroups[0].Id,
                        Assert.Single(savedFootnote.FilterGroups).FilterGroupId
                    );
                    Assert.Equal(
                        subject.Filters[0].FilterGroups[0].FilterItems[0].Id,
                        Assert.Single(savedFootnote.FilterItems).FilterItemId
                    );
                    Assert.Equal(
                        subject.IndicatorGroups[0].Indicators[0].Id,
                        Assert.Single(savedFootnote.Indicators).IndicatorId
                    );
                }
            );
        }
    }

    [Fact]
    public async Task FootnoteWithoutReleaseVersionId_ReleaseVersionIdSet()
    {
        // Arrange
        ReleaseFootnote releaseFootnote = _fixture
            .DefaultReleaseFootnote()
            .WithReleaseVersion(_fixture.DefaultStatsReleaseVersion())
            .WithFootnote(_fixture.DefaultFootnote());

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryStatisticsDbContext(contextId))
        {
            context.ReleaseFootnote.Add(releaseFootnote);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryStatisticsDbContext(contextId))
        {
            var controller = BuildController(context);

            // Act
            var result = await controller.MigrateFootnoteReleaseVersions(dryRun: false);

            // Assert
            Assert.Equal(0, result.ForkedFootnotes);
            Assert.Equal(0, result.DeletedOrphanedFootnotes);
            Assert.Equal(1, result.ReleaseVersionIdsSet);
            Assert.Equal(0, result.Remaining);
        }

        await using (var context = InMemoryStatisticsDbContext(contextId))
        {
            var savedFootnote = Assert.Single(context.Footnote);
            Assert.Equal(releaseFootnote.ReleaseVersionId, savedFootnote.ReleaseVersionId);
            Assert.Equal(releaseFootnote.Footnote.Updated, savedFootnote.Updated);
        }
    }

    [Fact]
    public async Task FootnoteWithWrongReleaseVersionId_ReleaseVersionIdSet()
    {
        // Arrange
        ReleaseFootnote releaseFootnote = _fixture
            .DefaultReleaseFootnote()
            .WithReleaseVersion(_fixture.DefaultStatsReleaseVersion())
            .WithFootnote(_fixture.DefaultFootnote());

        releaseFootnote.Footnote.ReleaseVersionId = Guid.NewGuid();

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryStatisticsDbContext(contextId))
        {
            context.ReleaseFootnote.Add(releaseFootnote);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryStatisticsDbContext(contextId))
        {
            var controller = BuildController(context);

            // Act
            var result = await controller.MigrateFootnoteReleaseVersions(dryRun: false);

            // Assert
            Assert.Equal(1, result.ReleaseVersionIdsSet);
        }

        await using (var context = InMemoryStatisticsDbContext(contextId))
        {
            Assert.Equal(releaseFootnote.ReleaseVersionId, Assert.Single(context.Footnote).ReleaseVersionId);
        }
    }

    [Fact]
    public async Task FootnoteAlreadyMigrated_Unchanged()
    {
        // Arrange
        ReleaseFootnote releaseFootnote = _fixture
            .DefaultReleaseFootnote()
            .WithReleaseVersion(_fixture.DefaultStatsReleaseVersion())
            .WithFootnote(_fixture.DefaultFootnote());

        releaseFootnote.Footnote.ReleaseVersionId = releaseFootnote.ReleaseVersion.Id;

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryStatisticsDbContext(contextId))
        {
            context.ReleaseFootnote.Add(releaseFootnote);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryStatisticsDbContext(contextId))
        {
            var controller = BuildController(context);

            // Act
            var result = await controller.MigrateFootnoteReleaseVersions(dryRun: false);

            // Assert
            Assert.Equal(0, result.ForkedFootnotes);
            Assert.Equal(0, result.DeletedOrphanedFootnotes);
            Assert.Equal(0, result.ReleaseVersionIdsSet);
            Assert.Equal(0, result.Remaining);
        }

        await using (var context = InMemoryStatisticsDbContext(contextId))
        {
            var savedFootnote = Assert.Single(context.Footnote);
            Assert.Equal(releaseFootnote.ReleaseVersion.Id, savedFootnote.ReleaseVersionId);
            Assert.Single(context.ReleaseFootnote);
        }
    }

    [Fact]
    public async Task OrphanedFootnote_DeletedWithItsLinks()
    {
        // Arrange
        Subject subject = _fixture.DefaultSubject();

        Footnote footnote = _fixture.DefaultFootnote().WithSubjects(ListOf(subject));

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryStatisticsDbContext(contextId))
        {
            context.Footnote.Add(footnote);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryStatisticsDbContext(contextId))
        {
            var controller = BuildController(context);

            // Act
            var result = await controller.MigrateFootnoteReleaseVersions(dryRun: false);

            // Assert
            Assert.Equal(1, result.DeletedOrphanedFootnotes);
        }

        await using (var context = InMemoryStatisticsDbContext(contextId))
        {
            Assert.Empty(context.Footnote);
            Assert.Empty(context.SubjectFootnote);
            Assert.Single(context.Subject);
        }
    }

    [Fact]
    public async Task MoreFootnotesToSetThanNum_SetsNumAndReportsRemaining()
    {
        // Arrange
        var releaseVersion = _fixture.DefaultStatsReleaseVersion().Generate();

        var releaseFootnotes = _fixture
            .DefaultReleaseFootnote()
            .WithReleaseVersion(releaseVersion)
            .WithFootnotes(_fixture.DefaultFootnote().GenerateList(3))
            .GenerateList();

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryStatisticsDbContext(contextId))
        {
            context.ReleaseFootnote.AddRange(releaseFootnotes);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryStatisticsDbContext(contextId))
        {
            var controller = BuildController(context);

            // Act
            var result = await controller.MigrateFootnoteReleaseVersions(dryRun: false, num: 2);

            // Assert
            Assert.Equal(2, result.ReleaseVersionIdsSet);
            Assert.Equal(1, result.Remaining);
        }

        await using (var context = InMemoryStatisticsDbContext(contextId))
        {
            Assert.Equal(2, context.Footnote.Count(f => f.ReleaseVersionId == releaseVersion.Id));
            Assert.Equal(1, context.Footnote.Count(f => f.ReleaseVersionId == null));
        }
    }

    [Fact]
    public async Task DryRun_ReportsChangesWithoutApplyingThem()
    {
        // Arrange
        var releaseVersions = _fixture.DefaultStatsReleaseVersion().GenerateList(2);

        Footnote sharedFootnote = _fixture.DefaultFootnote();

        var releaseFootnotes = _fixture
            .DefaultReleaseFootnote()
            .ForIndex(0, s => s.SetReleaseVersion(releaseVersions[0]).SetFootnote(sharedFootnote))
            .ForIndex(1, s => s.SetReleaseVersion(releaseVersions[1]).SetFootnote(sharedFootnote))
            .ForIndex(2, s => s.SetReleaseVersion(releaseVersions[0]).SetFootnote(_fixture.DefaultFootnote()))
            .GenerateList();

        Footnote orphanedFootnote = _fixture.DefaultFootnote();

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryStatisticsDbContext(contextId))
        {
            context.ReleaseFootnote.AddRange(releaseFootnotes);
            context.Footnote.Add(orphanedFootnote);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryStatisticsDbContext(contextId))
        {
            var controller = BuildController(context);

            // Act
            var result = await controller.MigrateFootnoteReleaseVersions();

            // Assert
            Assert.True(result.IsDryRun);
            Assert.Equal(1, result.ForkedFootnotes);
            Assert.Equal(1, result.DeletedOrphanedFootnotes);
            Assert.Equal(2, result.ReleaseVersionIdsSet);
            Assert.Equal(0, result.Remaining);
        }

        await using (var context = InMemoryStatisticsDbContext(contextId))
        {
            Assert.Equal(3, context.Footnote.Count());
            Assert.All(context.Footnote, f => Assert.Null(f.ReleaseVersionId));
            Assert.Equal(3, context.ReleaseFootnote.Count());
        }
    }

    private static FootnoteReleaseVersionMigrationController BuildController(StatisticsDbContext context) =>
        new(context);
}
