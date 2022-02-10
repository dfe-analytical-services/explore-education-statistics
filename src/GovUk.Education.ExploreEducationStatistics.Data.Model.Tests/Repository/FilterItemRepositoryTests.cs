#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Repository
{
    public class FilterItemRepositoryTests
    {
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
            StatisticsDbContext context)
        {
            return new(context);
        }
    }
}
