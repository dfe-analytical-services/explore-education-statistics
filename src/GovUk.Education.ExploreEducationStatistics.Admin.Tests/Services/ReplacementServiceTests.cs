using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.ValidationTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReplacementServiceTests
    {
        [Fact]
        public async Task GetReplacementPlan_SubjectsBelongToDifferentReleases()
        {
            var originalSubject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var replacementSubject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var contentRelease1 = new Release
            {
                Id = new Guid()
            };

            var contentRelease2 = new Release
            {
                Id = new Guid()
            };

            var statsRelease1 = new Data.Model.Release
            {
                Id = contentRelease1.Id
            };

            var statsRelease2 = new Data.Model.Release
            {
                Id = contentRelease2.Id
            };

            var originalReleaseSubject = new ReleaseSubject
            {
                Release = statsRelease1,
                Subject = originalSubject
            };

            var replacementReleaseSubject = new ReleaseSubject
            {
                Release = statsRelease2,
                Subject = replacementSubject
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(contentRelease1, contentRelease2);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddRangeAsync(statsRelease1, statsRelease2);
                await statisticsDbContext.AddRangeAsync(originalSubject, replacementSubject);
                await statisticsDbContext.AddRangeAsync(originalReleaseSubject, replacementReleaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacementService = BuildReplacementService(contentDbContext, statisticsDbContext, Mocks());

                var result = await replacementService.GetReplacementPlan(originalSubject.Id, replacementSubject.Id);

                Assert.True(result.IsLeft);
                AssertValidationProblem(result.Left, ReplacementDataFileMustBeForSameRelease);
            }
        }

        [Fact]
        public async Task GetReplacementPlan_OriginalSubjectNotUsed()
        {
            var originalSubject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var replacementSubject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var contentRelease = new Release
            {
                Id = new Guid()
            };

            var statsRelease = new Data.Model.Release
            {
                Id = contentRelease.Id
            };

            var originalReleaseSubject = new ReleaseSubject
            {
                Release = statsRelease,
                Subject = originalSubject
            };

            var replacementReleaseSubject = new ReleaseSubject
            {
                Release = statsRelease,
                Subject = replacementSubject
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(contentRelease);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddRangeAsync(statsRelease);
                await statisticsDbContext.AddRangeAsync(originalSubject, replacementSubject);
                await statisticsDbContext.AddRangeAsync(originalReleaseSubject, replacementReleaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacementService = BuildReplacementService(contentDbContext, statisticsDbContext, Mocks());

                var result = await replacementService.GetReplacementPlan(originalSubject.Id, replacementSubject.Id);

                Assert.True(result.IsRight);
                var replacementPlan = result.Right;
                Assert.True(replacementPlan.Valid);
                Assert.Empty(replacementPlan.DataBlocks);
                Assert.Empty(replacementPlan.Footnotes);
            }
        }

        private static ReplacementService BuildReplacementService(
            ContentDbContext contentDbContext,
            StatisticsDbContext statisticsDbContext,
            (
                Mock<IFilterService> filterService,
                Mock<IIndicatorService> indicatorService,
                Mock<ILocationService> locationService,
                Mock<IFootnoteService> footnoteService,
                Mock<ITimePeriodService> timePeriodService) mocks)
        {
            var (filterService, indicatorService, locationService, footnoteService, timePeriodService) = mocks;

            return new ReplacementService(
                contentDbContext,
                statisticsDbContext,
                filterService.Object,
                indicatorService.Object,
                locationService.Object,
                footnoteService.Object,
                timePeriodService.Object,
                new PersistenceHelper<StatisticsDbContext>(statisticsDbContext));
        }

        private static (Mock<IFilterService>,
            Mock<IIndicatorService>,
            Mock<ILocationService>,
            Mock<IFootnoteService>,
            Mock<ITimePeriodService>) Mocks()
        {
            return (
                new Mock<IFilterService>(),
                new Mock<IIndicatorService>(),
                new Mock<ILocationService>(),
                new Mock<IFootnoteService>(),
                new Mock<ITimePeriodService>());
        }
    }
}