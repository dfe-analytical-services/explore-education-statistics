#nullable enable
using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;
using Microsoft.Extensions.Logging;
using Moq;
using Thinktecture.EntityFrameworkCore.TempTables;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Repository;

public class FilterItemRepositoryTests
{
    private static readonly DataFixture Fixture = new();
        
    [Fact]
    [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    public async Task GetMatchedFilterItems_AllObservationsMatched_AllObservationsStrategyChosen()
    {
        var subjectId = Guid.NewGuid();
        
        var allObservationsForSubject = Fixture
            .DefaultObservation()
            .WithSubject(new Subject
            {
                Id = subjectId
            })
            .GenerateList(10);
        
        var observationsForAnotherSubject = Fixture
            .DefaultObservation()
            .WithSubject(new Subject
            {
                Id = Guid.NewGuid()
            })
            .GenerateList(10);
        
        // Fill the #MatchedObservation temp table with matches for every Observation for the Subject.
        var matchedObservationIds = allObservationsForSubject
            .Select(o => new MatchedObservation(o.Id))
            .ToList();

        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.Observation.AddRangeAsync(
                allObservationsForSubject.Concat(observationsForAnotherSubject));
            await statisticsDbContext.MatchedObservations.AddRangeAsync(matchedObservationIds);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using var context = InMemoryStatisticsDbContext(statisticsDbContextId);

        var allObservationsStrategy = new Mock<IAllObservationsMatchedFilterItemsStrategy>(Strict);
        var sparseObservationsStrategy = new Mock<ISparseObservationsMatchedFilterItemsStrategy>(Strict);
        var denseObservationsStrategy = new Mock<IDenseObservationsMatchedFilterItemsStrategy>(Strict);

        var filterItemsToReturn = Fixture
            .DefaultFilterItem()
            .GenerateList(2);

        allObservationsStrategy
            .Setup(s => s.GetFilterItemsFromMatchedObservationIds(
                subjectId,
                default))
            .ReturnsAsync(filterItemsToReturn);
        
        var repository = BuildFilterItemRepository(
            context: context,
            allObservationsMatchedFilterItemsStrategy: allObservationsStrategy.Object,
            sparseObservationsMatchedFilterItemsStrategy: sparseObservationsStrategy.Object,
            denseObservationsMatchedFilterItemsStrategy: denseObservationsStrategy.Object);

        var result = await repository.GetFilterItemsFromMatchedObservationIds(
            subjectId: subjectId,
            matchedObservationsTableReference: Mock.Of<ITempTableReference>(),
            cancellationToken: default);
        
        VerifyAllMocks(allObservationsStrategy);
        
        Assert.Equal(filterItemsToReturn, result);
    }
    
    [Fact]
    [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    public async Task GetMatchedFilterItems_PercentThresholdObservationsMatched_DenseObservationsStrategyChosen()
    {
        var subjectId = Guid.NewGuid();
        
        var allObservationsForSubject = Fixture
            .DefaultObservation()
            .WithSubject(new Subject
            {
                Id = subjectId
            })
            .GenerateList(100);
        
        var observationsForAnotherSubject = Fixture
            .DefaultObservation()
            .WithSubject(new Subject
            {
                Id = Guid.NewGuid()
            })
            .GenerateList(100);
        
        // Fill the #MatchedObservation temp table with 80% of the Observations for the Subject.
        var matchedObservationIds = allObservationsForSubject
            .Take(75)
            .Select(o => new MatchedObservation(o.Id))
            .ToList();

        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.Observation.AddRangeAsync(
                allObservationsForSubject.Concat(observationsForAnotherSubject));
            await statisticsDbContext.MatchedObservations.AddRangeAsync(matchedObservationIds);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using var context = InMemoryStatisticsDbContext(statisticsDbContextId);

        var allObservationsStrategy = new Mock<IAllObservationsMatchedFilterItemsStrategy>(Strict);
        var sparseObservationsStrategy = new Mock<ISparseObservationsMatchedFilterItemsStrategy>(Strict);
        var denseObservationsStrategy = new Mock<IDenseObservationsMatchedFilterItemsStrategy>(Strict);
        var matchedObservationTempTable = Mock.Of<ITempTableReference>();

        var filterItemsToReturn = Fixture
            .DefaultFilterItem()
            .GenerateList(2);

        denseObservationsStrategy
            .Setup(s => s.GetFilterItemsFromMatchedObservationIds(
                subjectId,
                matchedObservationTempTable,
                default))
            .ReturnsAsync(filterItemsToReturn);
        
        var repository = BuildFilterItemRepository(
            context: context,
            allObservationsMatchedFilterItemsStrategy: allObservationsStrategy.Object,
            sparseObservationsMatchedFilterItemsStrategy: sparseObservationsStrategy.Object,
            denseObservationsMatchedFilterItemsStrategy: denseObservationsStrategy.Object);

        var result = await repository.GetFilterItemsFromMatchedObservationIds(
            subjectId: subjectId,
            matchedObservationsTableReference: matchedObservationTempTable,
            cancellationToken: default);
        
        VerifyAllMocks(denseObservationsStrategy);
        
        Assert.Equal(filterItemsToReturn, result);
    }
    
    [Fact]
    [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    public async Task GetMatchedFilterItems_LessThanPercentThresholdObservationsMatched_SparseObservationsStrategyChosen()
    {
        var subjectId = Guid.NewGuid();
        
        var allObservationsForSubject = Fixture
            .DefaultObservation()
            .WithSubject(new Subject
            {
                Id = subjectId
            })
            .GenerateList(100);
        
        var observationsForAnotherSubject = Fixture
            .DefaultObservation()
            .WithSubject(new Subject
            {
                Id = Guid.NewGuid()
            })
            .GenerateList(10);
        
        // Fill the #MatchedObservation temp table with less than 80% of the Observations for the Subject.
        var matchedObservationIds = allObservationsForSubject
            .Take(74)
            .Select(o => new MatchedObservation(o.Id))
            .ToList();

        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.Observation.AddRangeAsync(
                allObservationsForSubject.Concat(observationsForAnotherSubject));
            await statisticsDbContext.MatchedObservations.AddRangeAsync(matchedObservationIds);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using var context = InMemoryStatisticsDbContext(statisticsDbContextId);

        var allObservationsStrategy = new Mock<IAllObservationsMatchedFilterItemsStrategy>(Strict);
        var sparseObservationsStrategy = new Mock<ISparseObservationsMatchedFilterItemsStrategy>(Strict);
        var denseObservationsStrategy = new Mock<IDenseObservationsMatchedFilterItemsStrategy>(Strict);
        var matchedObservationTempTable = Mock.Of<ITempTableReference>();

        var filterItemsToReturn = Fixture
            .DefaultFilterItem()
            .GenerateList(2);

        sparseObservationsStrategy
            .Setup(s => s.GetFilterItemsFromMatchedObservationIds(
                subjectId,
                matchedObservationTempTable,
                default))
            .ReturnsAsync(filterItemsToReturn);
        
        var repository = BuildFilterItemRepository(
            context: context,
            allObservationsMatchedFilterItemsStrategy: allObservationsStrategy.Object,
            sparseObservationsMatchedFilterItemsStrategy: sparseObservationsStrategy.Object,
            denseObservationsMatchedFilterItemsStrategy: denseObservationsStrategy.Object);

        var result = await repository.GetFilterItemsFromMatchedObservationIds(
            subjectId: subjectId,
            matchedObservationsTableReference: matchedObservationTempTable,
            cancellationToken: default);
        
        VerifyAllMocks(sparseObservationsStrategy);
        
        Assert.Equal(filterItemsToReturn, result);
    }
    
    [Fact]
    public async Task CountFilterItemsByFilter()
    {
        var filterItemCharacteristicSchoolYear1 = new FilterItem();
        var filterItemCharacteristicFsmEligible = new FilterItem();
        var filterItemCharacteristicFsmNotEligible = new FilterItem();
        var filterItemSchoolTypePrimary = new FilterItem();
        var filterItemSchoolTypeSecondary = new FilterItem();

        var filterCharacteristic = new Filter
        {
            FilterGroups = new List<FilterGroup>
            {
                new()
                {
                    FilterItems = new List<FilterItem>
                    {
                        filterItemCharacteristicSchoolYear1
                    }
                },
                new()
                {
                    FilterItems = new List<FilterItem>
                    {
                        filterItemCharacteristicFsmEligible,
                        filterItemCharacteristicFsmNotEligible
                    }
                }
            }
        };

        var filterSchoolType = new Filter
        {
            FilterGroups = new List<FilterGroup>
            {
                new()
                {
                    FilterItems = new List<FilterItem>
                    {
                        filterItemSchoolTypePrimary,
                        filterItemSchoolTypeSecondary
                    }
                }
            }
        };

        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.Filter.AddRangeAsync(filterCharacteristic, filterSchoolType);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var context = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var repository = BuildFilterItemRepository(context);
            var filterItemIds = new List<Guid>
            {
                filterItemCharacteristicSchoolYear1.Id,
                filterItemCharacteristicFsmEligible.Id,
                filterItemCharacteristicFsmNotEligible.Id,
                filterItemSchoolTypePrimary.Id,
                filterItemSchoolTypeSecondary.Id
            };
            var result = await repository.CountFilterItemsByFilter(filterItemIds);

            // Result should contain the counts of filter items in both filters
            Assert.Equal(2, result.Count);

            // 3 of the filter items belong to the Characteristic filter 
            Assert.Equal(3, result[filterCharacteristic.Id]);

            // 2 of the filter items belong to the School Type filter
            Assert.Equal(2, result[filterSchoolType.Id]);
        }
    }

    [Fact]
    public async Task CountFilterItemsByFilter_EmptyFilterItemsIsEmpty()
    {
        var filter = new Filter
        {
            FilterGroups = new List<FilterGroup>
            {
                new()
                {
                    FilterItems = new List<FilterItem>
                    {
                        new()
                    }
                }
            }
        };

        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.Filter.AddRangeAsync(filter);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var context = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var repository = BuildFilterItemRepository(context);
            var result = await repository.CountFilterItemsByFilter(new List<Guid>());
            Assert.Empty(result);
        }
    }

    [Fact]
    public async Task CountFilterItemsByFilter_FilterItemsNotFoundThrowsException()
    {
        var filterItem = new FilterItem();
        var filter = new Filter
        {
            FilterGroups = new List<FilterGroup>
            {
                new()
                {
                    FilterItems = new List<FilterItem>
                    {
                        filterItem
                    }
                }
            }
        };

        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.Filter.AddRangeAsync(filter);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var context = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var repository = BuildFilterItemRepository(context);

            var filterItemNotFound1 = Guid.NewGuid();
            var filterItemNotFound2 = Guid.NewGuid();

            var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await repository.CountFilterItemsByFilter(
                    ListOf(
                        filterItem.Id,
                        filterItemNotFound1,
                        filterItemNotFound2
                    ));
            });

            Assert.Equal($"Could not find filter items: {filterItemNotFound1}, {filterItemNotFound2}", exception.Message);
        }
    }

    private static FilterItemRepository BuildFilterItemRepository(
        StatisticsDbContext context,
        IAllObservationsMatchedFilterItemsStrategy? allObservationsMatchedFilterItemsStrategy = null,
        ISparseObservationsMatchedFilterItemsStrategy? sparseObservationsMatchedFilterItemsStrategy = null,
        IDenseObservationsMatchedFilterItemsStrategy? denseObservationsMatchedFilterItemsStrategy = null)
    {
        return new(
            statisticsDbContext: context, 
            allObservationsMatchedFilterItemsStrategy ?? Mock.Of<IAllObservationsMatchedFilterItemsStrategy>(),
            sparseObservationsMatchedFilterItemsStrategy ?? Mock.Of<ISparseObservationsMatchedFilterItemsStrategy>(),
            denseObservationsMatchedFilterItemsStrategy ?? Mock.Of<IDenseObservationsMatchedFilterItemsStrategy>(),
            Mock.Of<ILogger<FilterItemRepository>>());
    }
}
