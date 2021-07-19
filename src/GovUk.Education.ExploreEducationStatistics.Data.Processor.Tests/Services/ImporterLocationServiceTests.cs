using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Database.StatisticsDbUtils;
namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Tests.Services
{
    public class ImporterLocationServiceTests
    {
        [Fact]
        public async Task Find_PreExistingLocation()
        {
            var location = new Location
            {
                Country = new Country("1", "England"),
                LocalAuthority = new LocalAuthority("2", "3", "Bedford")
            };

            var service = BuildImporterLocationService();
            
            var statisticsDbContextId = Guid.NewGuid().ToString();
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                statisticsDbContext.Add(location);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var result1 = service.FindOrCreate(statisticsDbContext, location.Country,
                    localAuthority: location.LocalAuthority);
                Assert.NotNull(result1);
                Assert.Equal("Bedford", result1.LocalAuthority_Name);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var statDbLocations = statisticsDbContext.Location.ToList();
                Assert.Single(statDbLocations);
                Assert.Equal("Bedford", statDbLocations[0].LocalAuthority_Name);
            }
        }

        [Fact]
        public async Task Find_CaseSensitiveLocationName()
        {
            var country = new Country("1", "england");
            var lowerCase = new EnglishDevolvedArea("2", "casesensitive");
            var upperCase = new EnglishDevolvedArea("2", "CASESENSITIVE");

            var service = BuildImporterLocationService();

            var statisticsDbContextId = Guid.NewGuid().ToString();
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var result1 = service.FindOrCreate(statisticsDbContext, country,
                    englishDevolvedArea: lowerCase);
                Assert.NotNull(result1);
                Assert.Equal("casesensitive", result1.EnglishDevolvedArea_Name);

                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var result2 = service.FindOrCreate(statisticsDbContext, country,
                    englishDevolvedArea: upperCase);
                Assert.NotNull(result2);

                // NOTE: The below assert doesn't actual catch the error fixed by EES-2427 because the
                // in-memory DB in the EF version we're currently using is case sensitive - while a
                // MsSQL DBs are case insensitive by default: https://github.com/dotnet/efcore/issues/6153
                Assert.Equal("CASESENSITIVE", result2.EnglishDevolvedArea_Name);

                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var statDbLocations = statisticsDbContext.Location.ToList();
                Assert.Equal(2, statDbLocations.Count);
                Assert.Equal("casesensitive", statDbLocations[0].EnglishDevolvedArea_Name);
                Assert.Equal("CASESENSITIVE", statDbLocations[1].EnglishDevolvedArea_Name);
            }
        }

        private static ImporterLocationService BuildImporterLocationService()
        {
            return new ImporterLocationService(
                new Mock<IGuidGenerator>().Object,
                new ImporterMemoryCache());
        }
    }
}
