using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Repository;

public abstract class AllObservationsMatchedFilterItemsStrategyTests
{
    public class GetFilterItemsFromMatchedObservationIdsTests : AllObservationsMatchedFilterItemsStrategyTests
    {
        private static readonly DataFixture Fixture = new();

        [Fact]
        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        public async Task AllFilterItemsReturnedSuccessfully()
        {
            // Create 4 filters for this Subject.
            var filters = Fixture
                .DefaultFilter(filterGroupCount: 2, filterItemCount: 2)
                .WithSubject(Fixture.DefaultSubject())
                .GenerateList(4);

            // Create some filters for another Subject too.
            var otherSubjectFilters = Fixture
                .DefaultFilter(filterGroupCount: 2, filterItemCount: 2)
                .WithSubject(Fixture.DefaultSubject())
                .GenerateList(4);

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.Filter.AddRangeAsync(filters.Concat(otherSubjectFilters));
                await statisticsDbContext.SaveChangesAsync();
            }

            var subjectId = filters[0].SubjectId;

            var filterItemsForSubject = filters
                .SelectMany(f => f.FilterGroups)
                .SelectMany(fg => fg.FilterItems)
                .ToList();

            await using var context = InMemoryStatisticsDbContext(statisticsDbContextId);

            var strategy = new AllObservationsMatchedFilterItemsStrategy(context);
            
            var result = await strategy.GetFilterItemsFromMatchedObservationIds(
                subjectId: subjectId,
                cancellationToken: default);

            Assert.Equal(filterItemsForSubject, result);
        }
    }
}
