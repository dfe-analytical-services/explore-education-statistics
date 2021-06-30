using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Database.StatisticsDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests
{
    public class TimePeriodServiceTests
    {
        [Fact]
        public async Task GetTimePeriods_Subject_CorrectlyOrderWeeklyTimeIdentifiers()
        {
            var subject = new Subject
            {
                Observations = new List<Observation>
                {
                    new Observation {Year = 2001, TimeIdentifier = Week1},
                    new Observation {Year = 2000, TimeIdentifier = Week20},
                    new Observation {Year = 2000, TimeIdentifier = Week2},
                    new Observation {Year = 2000, TimeIdentifier = Week1},
                    new Observation {Year = 2000, TimeIdentifier = Week10},
                }
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddAsync(subject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = new TimePeriodService(statisticsDbContext);
                var result = service.GetTimePeriods(subject.Id).ToList();

                Assert.Equal(5, result.Count);
                Assert.Equal((2000, Week1), result[0]);
                Assert.Equal((2000, Week2), result[1]);
                Assert.Equal((2000, Week10), result[2]);
                Assert.Equal((2000, Week20), result[3]);
                Assert.Equal((2001, Week1), result[4]);
            }
        }

        [Fact]
        public async Task GetTimePeriods_Subject_CorrectlyOrderMonthlyTimeIdentifiers()
        {
            var subject = new Subject
            {
                Observations = new List<Observation>
                {
                    new Observation {Year = 2001, TimeIdentifier = January},
                    new Observation {Year = 2000, TimeIdentifier = February},
                    new Observation {Year = 2000, TimeIdentifier = December},
                    new Observation {Year = 2000, TimeIdentifier = October},
                    new Observation {Year = 2000, TimeIdentifier = March},
                }
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddAsync(subject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = new TimePeriodService(statisticsDbContext);
                var result = service.GetTimePeriods(subject.Id).ToList();

                Assert.Equal(5, result.Count);
                Assert.Equal((2000, February), result[0]);
                Assert.Equal((2000, March), result[1]);
                Assert.Equal((2000, October), result[2]);
                Assert.Equal((2000, December), result[3]);
                Assert.Equal((2001, January), result[4]);
            }
        }

        [Fact]
        public async Task GetTimePeriods_Observations_CorrectlyOrderWeeklyTimeIdentifiers()
        {
            var obs1 = new Observation {Year = 2001, TimeIdentifier = Week1};
            var obs2 = new Observation {Year = 2000, TimeIdentifier = Week20};
            var obs3 = new Observation {Year = 2000, TimeIdentifier = Week2};
            var obs4 = new Observation {Year = 2000, TimeIdentifier = Week1};
            var obs5 = new Observation {Year = 2000, TimeIdentifier = Week10};

            var statisticsDbContextId = Guid.NewGuid().ToString();
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddRangeAsync(obs1, obs2, obs3, obs4, obs5);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = new TimePeriodService(statisticsDbContext);
                var queryableObservations = statisticsDbContext.Observation.AsQueryable();
                var result = service.GetTimePeriods(queryableObservations).ToList();

                Assert.Equal(5, result.Count);
                Assert.Equal((2000, Week1), result[0]);
                Assert.Equal((2000, Week2), result[1]);
                Assert.Equal((2000, Week10), result[2]);
                Assert.Equal((2000, Week20), result[3]);
                Assert.Equal((2001, Week1), result[4]);
            }
        }
    }
}
