#nullable enable
using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Thinktecture.EntityFrameworkCore.TempTables;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests;

public abstract class StatisticsDbDataSetTests
{
    private static readonly DataFixture Fixture = new();

    public class ListObservationsTests : StatisticsDbDataSetTests
    {
        [Fact]
        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        public async Task MatchedObservations_ReturnedWithLocationAndFilterItems()
        {
            var subject = new Subject { Id = Guid.NewGuid() };

            Filter filter = Fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 2).WithSubject(subject);
            var filterItems = filter.FilterGroups.SelectMany(fg => fg.FilterItems).ToList();

            Location location = Fixture.DefaultLocation().WithPresetRegion();

            var matchedObservations = Fixture
                .DefaultObservation()
                .WithSubject(subject)
                .WithLocation(location)
                .WithFilterItems(filterItems)
                .WithTimePeriod(2022, AcademicYear)
                .GenerateList(2);

            var unmatchedObservations = Fixture
                .DefaultObservation()
                .WithSubject(subject)
                .WithLocation(location)
                .WithFilterItems(filterItems)
                .WithTimePeriod(2021, AcademicYear)
                .GenerateList(2);

            var query = new FullTableQuery { SubjectId = subject.Id };

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                statisticsDbContext.Filter.Add(filter);
                statisticsDbContext.Observation.AddRange(matchedObservations.Concat(unmatchedObservations));
                statisticsDbContext.MatchedObservations.AddRange(
                    matchedObservations.Select(o => new MatchedObservation(o.Id))
                );
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var observationService = new Mock<IObservationService>(Strict);

                observationService
                    .Setup(s => s.GetMatchedObservations(query, default))
                    .ReturnsAsync(Mock.Of<ITempTableReference>());

                var dataSet = BuildDataSet(
                    statisticsDbContext,
                    subjectId: subject.Id,
                    observationService: observationService.Object
                );

                var result = await dataSet.ListObservations(query);

                VerifyAllMocks(observationService);

                Assert.Equal(2, result.Count);
                Assert.All(
                    result,
                    observation =>
                    {
                        Assert.Contains(observation.Id, matchedObservations.Select(o => o.Id));
                        Assert.Equal(location.Id, observation.Location.Id);
                        Assert.Equal(2, observation.FilterItems.Count);
                    }
                );
            }
        }

        [Fact]
        public async Task QuerySubjectIdDoesNotMatch_ThrowsArgumentException()
        {
            await using var statisticsDbContext = InMemoryStatisticsDbContext();

            var dataSet = BuildDataSet(statisticsDbContext, subjectId: Guid.NewGuid());

            var query = new FullTableQuery { SubjectId = Guid.NewGuid() };

            await Assert.ThrowsAsync<ArgumentException>(() => dataSet.ListObservations(query));
        }
    }

    public class ListObservationBatchesTests : StatisticsDbDataSetTests
    {
        [Fact]
        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        public async Task FiveMatchedObservationsWithBatchSizeTwo_YieldsThreeBatches()
        {
            var subject = new Subject { Id = Guid.NewGuid() };

            Location location = Fixture.DefaultLocation().WithPresetRegion();

            var observations = Fixture.DefaultObservation().WithSubject(subject).WithLocation(location).GenerateList(5);

            var query = new FullTableQuery { SubjectId = subject.Id };

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                statisticsDbContext.Observation.AddRange(observations);
                statisticsDbContext.MatchedObservations.AddRange(
                    observations.Select(o => new MatchedObservation(o.Id))
                );
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var observationService = new Mock<IObservationService>(Strict);

                observationService
                    .Setup(s => s.GetMatchedObservations(query, default))
                    .ReturnsAsync(Mock.Of<ITempTableReference>());

                var dataSet = BuildDataSet(
                    statisticsDbContext,
                    subjectId: subject.Id,
                    observationService: observationService.Object
                );

                var batches = new List<IReadOnlyList<Observation>>();

                await foreach (var batch in dataSet.ListObservationBatches(query, batchSize: 2))
                {
                    batches.Add(batch);
                }

                VerifyAllMocks(observationService);

                Assert.Equal(3, batches.Count);
                Assert.Equal(2, batches[0].Count);
                Assert.Equal(2, batches[1].Count);
                Assert.Single(batches[2]);

                var returnedIds = batches.SelectMany(batch => batch.Select(o => o.Id)).ToHashSet();
                Assert.Equal(observations.Select(o => o.Id).ToHashSet(), returnedIds);
            }
        }

        [Fact]
        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        public async Task NoMatchedObservations_YieldsNothing()
        {
            var subject = new Subject { Id = Guid.NewGuid() };

            Location location = Fixture.DefaultLocation().WithPresetRegion();

            var observations = Fixture.DefaultObservation().WithSubject(subject).WithLocation(location).GenerateList(2);

            var query = new FullTableQuery { SubjectId = subject.Id };

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                statisticsDbContext.Observation.AddRange(observations);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var observationService = new Mock<IObservationService>(Strict);

                observationService
                    .Setup(s => s.GetMatchedObservations(query, default))
                    .ReturnsAsync(Mock.Of<ITempTableReference>());

                var dataSet = BuildDataSet(
                    statisticsDbContext,
                    subjectId: subject.Id,
                    observationService: observationService.Object
                );

                var batches = new List<IReadOnlyList<Observation>>();

                await foreach (var batch in dataSet.ListObservationBatches(query, batchSize: 2))
                {
                    batches.Add(batch);
                }

                VerifyAllMocks(observationService);

                Assert.Empty(batches);
            }
        }
    }

    public class ListFilterItemsForQueryTests : StatisticsDbDataSetTests
    {
        [Fact]
        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        public async Task AllObservationsMatched_AllObservationsStrategyChosen()
        {
            var subjectId = Guid.NewGuid();

            var allObservationsForSubject = Fixture
                .DefaultObservation()
                .WithSubject(new Subject { Id = subjectId })
                .GenerateList(10);

            var observationsForAnotherSubject = Fixture
                .DefaultObservation()
                .WithSubject(new Subject { Id = Guid.NewGuid() })
                .GenerateList(10);

            // Fill the #MatchedObservation temp table with matches for every Observation for the Subject.
            var matchedObservationIds = allObservationsForSubject.Select(o => new MatchedObservation(o.Id)).ToList();

            var query = new FullTableQuery { SubjectId = subjectId };

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.Observation.AddRangeAsync(
                    allObservationsForSubject.Concat(observationsForAnotherSubject)
                );
                await statisticsDbContext.MatchedObservations.AddRangeAsync(matchedObservationIds);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using var context = InMemoryStatisticsDbContext(statisticsDbContextId);

            var observationService = new Mock<IObservationService>(Strict);
            var allObservationsStrategy = new Mock<IAllObservationsMatchedFilterItemsStrategy>(Strict);
            var sparseObservationsStrategy = new Mock<ISparseObservationsMatchedFilterItemsStrategy>(Strict);
            var denseObservationsStrategy = new Mock<IDenseObservationsMatchedFilterItemsStrategy>(Strict);

            observationService
                .Setup(s => s.GetMatchedObservations(query, default))
                .ReturnsAsync(Mock.Of<ITempTableReference>());

            var filterItemsToReturn = Fixture.DefaultFilterItem().GenerateList(2);

            allObservationsStrategy
                .Setup(s => s.GetFilterItemsFromMatchedObservationIds(subjectId, default))
                .ReturnsAsync(filterItemsToReturn);

            var dataSet = BuildDataSet(
                context,
                subjectId: subjectId,
                observationService: observationService.Object,
                allObservationsMatchedFilterItemsStrategy: allObservationsStrategy.Object,
                sparseObservationsMatchedFilterItemsStrategy: sparseObservationsStrategy.Object,
                denseObservationsMatchedFilterItemsStrategy: denseObservationsStrategy.Object
            );

            var result = await dataSet.ListFilterItemsForQuery(query);

            VerifyAllMocks(observationService, allObservationsStrategy);

            Assert.Equal(filterItemsToReturn, result);
        }

        [Theory]
        [InlineData(75)]
        [InlineData(76)]
        [InlineData(99)]
        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        public async Task PercentThresholdObservationsMatchedOrExceeded_DenseStrategyChosen(
            int percentageObservationsMatched
        )
        {
            var subjectId = Guid.NewGuid();

            var allObservationsForSubject = Fixture
                .DefaultObservation()
                .WithSubject(new Subject { Id = subjectId })
                .GenerateList(100);

            var observationsForAnotherSubject = Fixture
                .DefaultObservation()
                .WithSubject(new Subject { Id = Guid.NewGuid() })
                .GenerateList(100);

            // Fill the #MatchedObservation temp table with a high percentage of the Observations for the Subject.
            var matchedObservationIds = allObservationsForSubject
                .Take(percentageObservationsMatched)
                .Select(o => new MatchedObservation(o.Id))
                .ToList();

            var query = new FullTableQuery { SubjectId = subjectId };

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.Observation.AddRangeAsync(
                    allObservationsForSubject.Concat(observationsForAnotherSubject)
                );
                await statisticsDbContext.MatchedObservations.AddRangeAsync(matchedObservationIds);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using var context = InMemoryStatisticsDbContext(statisticsDbContextId);

            var observationService = new Mock<IObservationService>(Strict);
            var allObservationsStrategy = new Mock<IAllObservationsMatchedFilterItemsStrategy>(Strict);
            var sparseObservationsStrategy = new Mock<ISparseObservationsMatchedFilterItemsStrategy>(Strict);
            var denseObservationsStrategy = new Mock<IDenseObservationsMatchedFilterItemsStrategy>(Strict);
            var matchedObservationTempTable = Mock.Of<ITempTableReference>();

            observationService
                .Setup(s => s.GetMatchedObservations(query, default))
                .ReturnsAsync(matchedObservationTempTable);

            var filterItemsToReturn = Fixture.DefaultFilterItem().GenerateList(2);

            denseObservationsStrategy
                .Setup(s => s.GetFilterItemsFromMatchedObservationIds(subjectId, matchedObservationTempTable, default))
                .ReturnsAsync(filterItemsToReturn);

            var dataSet = BuildDataSet(
                context,
                subjectId: subjectId,
                observationService: observationService.Object,
                allObservationsMatchedFilterItemsStrategy: allObservationsStrategy.Object,
                sparseObservationsMatchedFilterItemsStrategy: sparseObservationsStrategy.Object,
                denseObservationsMatchedFilterItemsStrategy: denseObservationsStrategy.Object
            );

            var result = await dataSet.ListFilterItemsForQuery(query);

            VerifyAllMocks(observationService, denseObservationsStrategy);

            Assert.Equal(filterItemsToReturn, result);
        }

        [Theory]
        [InlineData(74)]
        [InlineData(73)]
        [InlineData(1)]
        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        public async Task LessThanPercentThresholdObservationsMatched_SparseStrategyChosen(
            int percentageObservationsMatched
        )
        {
            var subjectId = Guid.NewGuid();

            var allObservationsForSubject = Fixture
                .DefaultObservation()
                .WithSubject(new Subject { Id = subjectId })
                .GenerateList(100);

            var observationsForAnotherSubject = Fixture
                .DefaultObservation()
                .WithSubject(new Subject { Id = Guid.NewGuid() })
                .GenerateList(10);

            // Fill the #MatchedObservation temp table with a smaller percentage of the Observations for the Subject.
            var matchedObservationIds = allObservationsForSubject
                .Take(percentageObservationsMatched)
                .Select(o => new MatchedObservation(o.Id))
                .ToList();

            var query = new FullTableQuery { SubjectId = subjectId };

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.Observation.AddRangeAsync(
                    allObservationsForSubject.Concat(observationsForAnotherSubject)
                );
                await statisticsDbContext.MatchedObservations.AddRangeAsync(matchedObservationIds);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using var context = InMemoryStatisticsDbContext(statisticsDbContextId);

            var observationService = new Mock<IObservationService>(Strict);
            var allObservationsStrategy = new Mock<IAllObservationsMatchedFilterItemsStrategy>(Strict);
            var sparseObservationsStrategy = new Mock<ISparseObservationsMatchedFilterItemsStrategy>(Strict);
            var denseObservationsStrategy = new Mock<IDenseObservationsMatchedFilterItemsStrategy>(Strict);
            var matchedObservationTempTable = Mock.Of<ITempTableReference>();

            observationService
                .Setup(s => s.GetMatchedObservations(query, default))
                .ReturnsAsync(matchedObservationTempTable);

            var filterItemsToReturn = Fixture.DefaultFilterItem().GenerateList(2);

            sparseObservationsStrategy
                .Setup(s => s.GetFilterItemsFromMatchedObservationIds(subjectId, matchedObservationTempTable, default))
                .ReturnsAsync(filterItemsToReturn);

            var dataSet = BuildDataSet(
                context,
                subjectId: subjectId,
                observationService: observationService.Object,
                allObservationsMatchedFilterItemsStrategy: allObservationsStrategy.Object,
                sparseObservationsMatchedFilterItemsStrategy: sparseObservationsStrategy.Object,
                denseObservationsMatchedFilterItemsStrategy: denseObservationsStrategy.Object
            );

            var result = await dataSet.ListFilterItemsForQuery(query);

            VerifyAllMocks(observationService, sparseObservationsStrategy);

            Assert.Equal(filterItemsToReturn, result);
        }

        [Fact]
        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        public async Task NoObservationsMatched_NoFilterItemsReturned()
        {
            var subjectId = Guid.NewGuid();

            var allObservationsForSubject = Fixture
                .DefaultObservation()
                .WithSubject(new Subject { Id = subjectId })
                .GenerateList(100);

            var query = new FullTableQuery { SubjectId = subjectId };

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                // Populate Observations for the Subject, but not any MatchedObservations.
                await statisticsDbContext.Observation.AddRangeAsync(allObservationsForSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using var context = InMemoryStatisticsDbContext(statisticsDbContextId);

            var observationService = new Mock<IObservationService>(Strict);
            var allObservationsStrategy = new Mock<IAllObservationsMatchedFilterItemsStrategy>(Strict);
            var sparseObservationsStrategy = new Mock<ISparseObservationsMatchedFilterItemsStrategy>(Strict);
            var denseObservationsStrategy = new Mock<IDenseObservationsMatchedFilterItemsStrategy>(Strict);

            observationService
                .Setup(s => s.GetMatchedObservations(query, default))
                .ReturnsAsync(Mock.Of<ITempTableReference>());

            var dataSet = BuildDataSet(
                context,
                subjectId: subjectId,
                observationService: observationService.Object,
                allObservationsMatchedFilterItemsStrategy: allObservationsStrategy.Object,
                sparseObservationsMatchedFilterItemsStrategy: sparseObservationsStrategy.Object,
                denseObservationsMatchedFilterItemsStrategy: denseObservationsStrategy.Object
            );

            var result = await dataSet.ListFilterItemsForQuery(query);

            VerifyAllMocks(observationService);

            Assert.Empty(result);
        }

        [Fact]
        public async Task QuerySubjectIdDoesNotMatch_ThrowsArgumentException()
        {
            await using var statisticsDbContext = InMemoryStatisticsDbContext();

            var dataSet = BuildDataSet(statisticsDbContext, subjectId: Guid.NewGuid());

            var query = new FullTableQuery { SubjectId = Guid.NewGuid() };

            await Assert.ThrowsAsync<ArgumentException>(() => dataSet.ListFilterItemsForQuery(query));
        }
    }

    public class ListFilterItemsTests : StatisticsDbDataSetTests
    {
        [Fact]
        public async Task FilterItemIds_ReturnedWithFilterGroupAndFilterForSubjectOnly()
        {
            var subjectId = Guid.NewGuid();

            var filter = new Filter
            {
                SubjectId = subjectId,
                FilterGroups = [new() { FilterItems = [new(), new()] }, new() { FilterItems = [new()] }],
            };

            var otherSubjectFilter = new Filter
            {
                SubjectId = Guid.NewGuid(),
                FilterGroups = [new() { FilterItems = [new()] }],
            };

            var filterItems = filter.FilterGroups.SelectMany(fg => fg.FilterItems).ToList();
            var otherSubjectFilterItem = otherSubjectFilter.FilterGroups[0].FilterItems[0];

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                statisticsDbContext.Filter.AddRange(filter, otherSubjectFilter);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var dataSet = BuildDataSet(statisticsDbContext, subjectId);

                var result = await dataSet.ListFilterItems([
                    filterItems[0].Id,
                    filterItems[2].Id,
                    otherSubjectFilterItem.Id,
                    Guid.NewGuid(),
                ]);

                Assert.Equal(2, result.Count);

                var resultsById = result.ToDictionary(fi => fi.Id);
                Assert.Equal(filter.Id, resultsById[filterItems[0].Id].FilterGroup.Filter.Id);
                Assert.Equal(filter.Id, resultsById[filterItems[2].Id].FilterGroup.Filter.Id);
                Assert.Equal(filter.FilterGroups[0].Id, resultsById[filterItems[0].Id].FilterGroupId);
                Assert.Equal(filter.FilterGroups[1].Id, resultsById[filterItems[2].Id].FilterGroupId);
            }
        }
    }

    public class ListFiltersTests : StatisticsDbDataSetTests
    {
        [Fact]
        public async Task FiltersForSubject_ReturnedWithFilterGroupsAndFilterItems()
        {
            Subject subject = Fixture
                .DefaultSubject()
                .WithFilters(Fixture.DefaultFilter(filterGroupCount: 2, filterItemCount: 2).Generate(2));

            Subject otherSubject = Fixture
                .DefaultSubject()
                .WithFilters(Fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 1).Generate(1));

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                statisticsDbContext.Subject.AddRange(subject, otherSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var dataSet = BuildDataSet(statisticsDbContext, subjectId: subject.Id);

                var result = await dataSet.ListFilters();

                Assert.Equal(2, result.Count);
                Assert.All(
                    result,
                    filter =>
                    {
                        Assert.Contains(filter.Id, subject.Filters.Select(f => f.Id));
                        Assert.Equal(2, filter.FilterGroups.Count);
                        Assert.All(filter.FilterGroups, fg => Assert.Equal(2, fg.FilterItems.Count));
                    }
                );
            }
        }
    }

    public class ListFilterItemRelationshipsTests : StatisticsDbDataSetTests
    {
        [Fact]
        public async Task ParentAndChildFilterItemsOnSameObservation_ReturnedAsDistinctPairs()
        {
            var subject = new Subject { Id = Guid.NewGuid() };

            Filter parentFilter = Fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 2).WithSubject(subject);
            Filter childFilter = Fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 2).WithSubject(subject);

            var parentItems = parentFilter.FilterGroups[0].FilterItems;
            var childItems = childFilter.FilterGroups[0].FilterItems;

            var observations = new List<Observation>
            {
                Fixture.DefaultObservation().WithSubject(subject).WithFilterItems([parentItems[0], childItems[0]]),
                // Same pair as above, which should only be returned once
                Fixture.DefaultObservation().WithSubject(subject).WithFilterItems([parentItems[0], childItems[0]]),
                Fixture.DefaultObservation().WithSubject(subject).WithFilterItems([parentItems[1], childItems[0]]),
                Fixture.DefaultObservation().WithSubject(subject).WithFilterItems([parentItems[1], childItems[1]]),
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                statisticsDbContext.Filter.AddRange(parentFilter, childFilter);
                statisticsDbContext.Observation.AddRange(observations);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var dataSet = BuildDataSet(statisticsDbContext, subjectId: subject.Id);

                var result = await dataSet.ListFilterItemRelationships(
                    parentFilterId: parentFilter.Id,
                    childFilterId: childFilter.Id
                );

                Assert.Equal(
                    new HashSet<(Guid, Guid)>
                    {
                        (parentItems[0].Id, childItems[0].Id),
                        (parentItems[1].Id, childItems[0].Id),
                        (parentItems[1].Id, childItems[1].Id),
                    },
                    result.ToHashSet()
                );
            }
        }

        [Fact]
        public async Task FiltersBelongToOtherSubject_ReturnsEmpty()
        {
            var otherSubject = new Subject { Id = Guid.NewGuid() };

            Filter parentFilter = Fixture
                .DefaultFilter(filterGroupCount: 1, filterItemCount: 1)
                .WithSubject(otherSubject);
            Filter childFilter = Fixture
                .DefaultFilter(filterGroupCount: 1, filterItemCount: 1)
                .WithSubject(otherSubject);

            Observation observation = Fixture
                .DefaultObservation()
                .WithSubject(otherSubject)
                .WithFilterItems([
                    parentFilter.FilterGroups[0].FilterItems[0],
                    childFilter.FilterGroups[0].FilterItems[0],
                ]);

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                statisticsDbContext.Filter.AddRange(parentFilter, childFilter);
                statisticsDbContext.Observation.Add(observation);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var dataSet = BuildDataSet(statisticsDbContext, subjectId: Guid.NewGuid());

                var result = await dataSet.ListFilterItemRelationships(
                    parentFilterId: parentFilter.Id,
                    childFilterId: childFilter.Id
                );

                Assert.Empty(result);
            }
        }
    }

    public class ListFiltersExcludingItemsTests : StatisticsDbDataSetTests
    {
        [Fact]
        public async Task FiltersForSubject_ReturnedWithoutFilterGroups()
        {
            Subject subject = Fixture
                .DefaultSubject()
                .WithFilters(Fixture.DefaultFilter(filterGroupCount: 2, filterItemCount: 2).Generate(2));

            Subject otherSubject = Fixture
                .DefaultSubject()
                .WithFilters(Fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 1).Generate(1));

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                statisticsDbContext.Subject.AddRange(subject, otherSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var dataSet = BuildDataSet(statisticsDbContext, subjectId: subject.Id);

                var result = await dataSet.ListFiltersExcludingItems();

                Assert.Equal(2, result.Count);
                Assert.All(
                    result,
                    filter =>
                    {
                        Assert.Contains(filter.Id, subject.Filters.Select(f => f.Id));
                        Assert.Empty(filter.FilterGroups);
                    }
                );
            }
        }
    }

    public class ListIndicatorGroupsTests : StatisticsDbDataSetTests
    {
        [Fact]
        public async Task IndicatorGroupsForSubject_ReturnedWithIndicators()
        {
            Subject subject = Fixture
                .DefaultSubject()
                .WithIndicatorGroups(Fixture.DefaultIndicatorGroup(indicatorCount: 2).Generate(2));

            Subject otherSubject = Fixture
                .DefaultSubject()
                .WithIndicatorGroups(Fixture.DefaultIndicatorGroup(indicatorCount: 1).Generate(1));

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                statisticsDbContext.Subject.AddRange(subject, otherSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var dataSet = BuildDataSet(statisticsDbContext, subjectId: subject.Id);

                var result = await dataSet.ListIndicatorGroups();

                Assert.Equal(2, result.Count);
                Assert.All(
                    result,
                    indicatorGroup =>
                    {
                        Assert.Contains(indicatorGroup.Id, subject.IndicatorGroups.Select(ig => ig.Id));
                        Assert.Equal(2, indicatorGroup.Indicators.Count);
                    }
                );
            }
        }
    }

    public class ListIndicatorsTests : StatisticsDbDataSetTests
    {
        [Fact]
        public async Task ReturnsAllIndicatorsForSubject()
        {
            Subject subject = Fixture
                .DefaultSubject()
                .WithIndicatorGroups(Fixture.DefaultIndicatorGroup(indicatorCount: 2).Generate(2));

            Subject otherSubject = Fixture
                .DefaultSubject()
                .WithIndicatorGroups(Fixture.DefaultIndicatorGroup(indicatorCount: 1).Generate(1));

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                statisticsDbContext.Subject.AddRange(subject, otherSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var dataSet = BuildDataSet(statisticsDbContext, subjectId: subject.Id);

                var result = await dataSet.ListIndicators();

                var expectedIds = subject.IndicatorGroups.SelectMany(ig => ig.Indicators).Select(i => i.Id).ToHashSet();

                Assert.Equal(4, result.Count);
                Assert.Equal(expectedIds, result.Select(i => i.Id).ToHashSet());
            }
        }
    }

    public class ListIndicatorsWithIdsProvidedTests : StatisticsDbDataSetTests
    {
        [Fact]
        public async Task ReturnsMatchingIndicatorsForSubjectOnly()
        {
            Subject subject = Fixture
                .DefaultSubject()
                .WithIndicatorGroups(Fixture.DefaultIndicatorGroup(indicatorCount: 2).Generate(1));

            Subject otherSubject = Fixture
                .DefaultSubject()
                .WithIndicatorGroups(Fixture.DefaultIndicatorGroup(indicatorCount: 1).Generate(1));

            var subjectIndicators = subject.IndicatorGroups.SelectMany(ig => ig.Indicators).ToList();
            var otherSubjectIndicator = otherSubject.IndicatorGroups.SelectMany(ig => ig.Indicators).Single();

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                statisticsDbContext.Subject.AddRange(subject, otherSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var dataSet = BuildDataSet(statisticsDbContext, subjectId: subject.Id);

                var result = await dataSet.ListIndicators([
                    subjectIndicators[0].Id,
                    otherSubjectIndicator.Id,
                    Guid.NewGuid(),
                ]);

                var indicator = Assert.Single(result);
                Assert.Equal(subjectIndicators[0].Id, indicator.Id);
            }
        }
    }

    public class ListLocationsTests : StatisticsDbDataSetTests
    {
        [Fact]
        public async Task ReturnsDistinctLocationsForSubject()
        {
            var subject = new Subject { Id = Guid.NewGuid() };
            var otherSubject = new Subject { Id = Guid.NewGuid() };

            var locations = Fixture.DefaultLocation().WithPresetRegion().GenerateList(3);

            var subjectObservations = Fixture
                .DefaultObservation()
                .WithSubject(subject)
                .ForRange(..2, o => o.SetLocation(locations[0]))
                .ForRange(2..4, o => o.SetLocation(locations[1]))
                .GenerateList(4);

            var otherSubjectObservations = Fixture
                .DefaultObservation()
                .WithSubject(otherSubject)
                .WithLocation(locations[2])
                .GenerateList(2);

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                statisticsDbContext.Observation.AddRange(subjectObservations.Concat(otherSubjectObservations));
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var dataSet = BuildDataSet(statisticsDbContext, subjectId: subject.Id);

                var result = await dataSet.ListLocations();

                Assert.Equal(2, result.Count);
                Assert.Equal(
                    new HashSet<Guid> { locations[0].Id, locations[1].Id },
                    result.Select(l => l.Id).ToHashSet()
                );
            }
        }
    }

    public class ListLocationsWithIdsProvidedTests : StatisticsDbDataSetTests
    {
        [Fact]
        public async Task ReturnsMatchingLocationsRegardlessOfSubject()
        {
            var locations = Fixture.DefaultLocation().WithPresetRegion().GenerateList(3);

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                statisticsDbContext.Location.AddRange(locations);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var dataSet = BuildDataSet(statisticsDbContext, subjectId: Guid.NewGuid());

                var result = await dataSet.ListLocations([locations[0].Id, locations[2].Id, Guid.NewGuid()]);

                Assert.Equal(2, result.Count);
                Assert.Equal(
                    new HashSet<Guid> { locations[0].Id, locations[2].Id },
                    result.Select(l => l.Id).ToHashSet()
                );
            }
        }
    }

    public class ListTimePeriodsTests : StatisticsDbDataSetTests
    {
        [Fact]
        public async Task ReturnsDistinctTimePeriodsForSubjectOrderedByDefinition()
        {
            var subject = new Subject { Id = Guid.NewGuid() };
            var otherSubject = new Subject { Id = Guid.NewGuid() };

            var observations = new List<Observation>
            {
                new()
                {
                    SubjectId = subject.Id,
                    Year = 2001,
                    TimeIdentifier = Week1,
                },
                new()
                {
                    SubjectId = subject.Id,
                    Year = 2000,
                    TimeIdentifier = Week20,
                },
                new()
                {
                    SubjectId = subject.Id,
                    Year = 2000,
                    TimeIdentifier = Week2,
                },
                new()
                {
                    SubjectId = subject.Id,
                    Year = 2000,
                    TimeIdentifier = Week2,
                },
                new()
                {
                    SubjectId = subject.Id,
                    Year = 2000,
                    TimeIdentifier = Week1,
                },
                new()
                {
                    SubjectId = subject.Id,
                    Year = 2000,
                    TimeIdentifier = Week10,
                },
                new()
                {
                    SubjectId = otherSubject.Id,
                    Year = 1999,
                    TimeIdentifier = Week1,
                },
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                statisticsDbContext.Subject.AddRange(subject, otherSubject);
                statisticsDbContext.Observation.AddRange(observations);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var dataSet = BuildDataSet(statisticsDbContext, subjectId: subject.Id);

                var result = await dataSet.ListTimePeriods();

                Assert.Equal(5, result.Count);
                Assert.Equal((2000, Week1), result[0]);
                Assert.Equal((2000, Week2), result[1]);
                Assert.Equal((2000, Week10), result[2]);
                Assert.Equal((2000, Week20), result[3]);
                Assert.Equal((2001, Week1), result[4]);
            }
        }

        [Fact]
        public async Task MonthlyTimePeriods_OrderedByDefinition()
        {
            var subject = new Subject { Id = Guid.NewGuid() };

            var observations = new List<Observation>
            {
                new()
                {
                    SubjectId = subject.Id,
                    Year = 2001,
                    TimeIdentifier = January,
                },
                new()
                {
                    SubjectId = subject.Id,
                    Year = 2000,
                    TimeIdentifier = February,
                },
                new()
                {
                    SubjectId = subject.Id,
                    Year = 2000,
                    TimeIdentifier = December,
                },
                new()
                {
                    SubjectId = subject.Id,
                    Year = 2000,
                    TimeIdentifier = October,
                },
                new()
                {
                    SubjectId = subject.Id,
                    Year = 2000,
                    TimeIdentifier = March,
                },
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                statisticsDbContext.Subject.Add(subject);
                statisticsDbContext.Observation.AddRange(observations);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var dataSet = BuildDataSet(statisticsDbContext, subjectId: subject.Id);

                var result = await dataSet.ListTimePeriods();

                Assert.Equal(5, result.Count);
                Assert.Equal((2000, February), result[0]);
                Assert.Equal((2000, March), result[1]);
                Assert.Equal((2000, October), result[2]);
                Assert.Equal((2000, December), result[3]);
                Assert.Equal((2001, January), result[4]);
            }
        }
    }

    public class ListTimePeriodsWithLocationIdsProvidedTests : StatisticsDbDataSetTests
    {
        [Fact]
        public async Task ReturnsTimePeriodsForSubjectAtLocationsOnlyOrderedByDefinition()
        {
            var subject = new Subject { Id = Guid.NewGuid() };
            var otherSubject = new Subject { Id = Guid.NewGuid() };

            var locations = Fixture.DefaultLocation().WithPresetRegion().GenerateList(3);

            var observations = new List<Observation>
            {
                new()
                {
                    SubjectId = subject.Id,
                    Location = locations[0],
                    Year = 2001,
                    TimeIdentifier = Week1,
                },
                new()
                {
                    SubjectId = subject.Id,
                    Location = locations[0],
                    Year = 2000,
                    TimeIdentifier = Week20,
                },
                new()
                {
                    SubjectId = subject.Id,
                    Location = locations[1],
                    Year = 2000,
                    TimeIdentifier = Week2,
                },
                new()
                {
                    SubjectId = subject.Id,
                    Location = locations[2],
                    Year = 2000,
                    TimeIdentifier = Week1,
                },
                new()
                {
                    SubjectId = otherSubject.Id,
                    Location = locations[0],
                    Year = 1999,
                    TimeIdentifier = Week1,
                },
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                statisticsDbContext.Subject.AddRange(subject, otherSubject);
                statisticsDbContext.Observation.AddRange(observations);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var dataSet = BuildDataSet(statisticsDbContext, subjectId: subject.Id);

                var result = await dataSet.ListTimePeriods([locations[0].Id, locations[1].Id]);

                Assert.Equal(3, result.Count);
                Assert.Equal((2000, Week2), result[0]);
                Assert.Equal((2000, Week20), result[1]);
                Assert.Equal((2001, Week1), result[2]);
            }
        }
    }

    private static StatisticsDbDataSet BuildDataSet(
        StatisticsDbContext statisticsDbContext,
        Guid subjectId,
        IObservationService? observationService = null,
        IAllObservationsMatchedFilterItemsStrategy? allObservationsMatchedFilterItemsStrategy = null,
        ISparseObservationsMatchedFilterItemsStrategy? sparseObservationsMatchedFilterItemsStrategy = null,
        IDenseObservationsMatchedFilterItemsStrategy? denseObservationsMatchedFilterItemsStrategy = null
    )
    {
        return new(
            subjectId: subjectId,
            context: statisticsDbContext,
            observationService: observationService ?? Mock.Of<IObservationService>(Strict),
            allObservationsMatchedFilterItemsStrategy: allObservationsMatchedFilterItemsStrategy
                ?? Mock.Of<IAllObservationsMatchedFilterItemsStrategy>(Strict),
            sparseObservationsMatchedFilterItemsStrategy: sparseObservationsMatchedFilterItemsStrategy
                ?? Mock.Of<ISparseObservationsMatchedFilterItemsStrategy>(Strict),
            denseObservationsMatchedFilterItemsStrategy: denseObservationsMatchedFilterItemsStrategy
                ?? Mock.Of<IDenseObservationsMatchedFilterItemsStrategy>(Strict),
            logger: Mock.Of<ILogger<StatisticsDbDataSet>>()
        );
    }
}
