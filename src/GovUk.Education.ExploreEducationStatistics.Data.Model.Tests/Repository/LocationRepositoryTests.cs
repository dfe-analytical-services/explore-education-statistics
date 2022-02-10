#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Repository
{
    public class LocationRepositoryTests
    {
        private readonly Country _england = new("E92000001", "England");
        private readonly Region _northEast = new("E12000001", "North East");
        private readonly Region _northWest = new("E12000002", "North West");
        private readonly Region _eastMidlands = new("E12000004", "East Midlands");
        private readonly LocalAuthority _derby = new("E06000015", "", "Derby");
        private readonly LocalAuthority _nottingham = new("E06000018", "", "Nottingham");

        [Fact]
        public async Task GetDistinctForSubject()
        {
            var subject1Id = Guid.NewGuid();
            
            var location1 = new Location
            {
                Country = _england,
                Region = _eastMidlands,
                LocalAuthority = _derby
            };

            var location2 = new Location
            {
                Country = _england,
                Region = _northEast,
                LocalAuthority = _derby
            };
            
            var location3 = new Location
            {
                Country = _england,
                Region = _eastMidlands,
                LocalAuthority = _nottingham
            };
            
            // This Location is not used by subject1
            var location4 = new Location
            {
                Country = _england,
                Region = _northWest,
                LocalAuthority = _nottingham
            };
            
            var subject1Observations = new List<Observation>
            {
                new()
                {
                    Location = location1,
                    GeographicLevel = GeographicLevel.LocalAuthority,
                    SubjectId = subject1Id
                },
                new()
                {
                    Location = location2,
                    GeographicLevel = GeographicLevel.LocalAuthority,
                    SubjectId = subject1Id
                },
                new()
                {
                    Location = location3,
                    GeographicLevel = GeographicLevel.LocalAuthority,
                    SubjectId = subject1Id
                },
                new()
                {
                    Location = location1,
                    GeographicLevel = GeographicLevel.LocalAuthority,
                    SubjectId = subject1Id
                }
            };
            
            var subject2Observations = new List<Observation>
            {
                new()
                {
                    Location = location4,
                    GeographicLevel = GeographicLevel.LocalAuthority,
                    SubjectId = Guid.NewGuid()
                }
            };
            
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.Observation.AddRangeAsync(subject1Observations);
                await statisticsDbContext.Observation.AddRangeAsync(subject2Observations);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var repository = BuildRepository(statisticsDbContext);

                var result = await repository.GetDistinctForSubject(subject1Id);

                // Expect only 3 distinct Locations, as location1 is used twice
                Assert.Equal(3, result.Count);
                Assert.Contains(location1, result);
                Assert.Contains(location2, result);
                Assert.Contains(location3, result);
            }
        }

        private static LocationRepository BuildRepository(
            StatisticsDbContext? statisticsDbContext = null)
        {
            return new(statisticsDbContext ?? Mock.Of<StatisticsDbContext>());
        }
    }
}
