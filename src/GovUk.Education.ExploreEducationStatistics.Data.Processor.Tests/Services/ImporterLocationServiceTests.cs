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
        public async Task Find_EnglishDevolvedAreaCaseSensitiveLocationName()
        {
            var country = new Country("1", "england");
            var lowerCase = new EnglishDevolvedArea("2", "casesensitive");
            var upperCase = new EnglishDevolvedArea("1", "CASESENSITIVE");
            var statisticsDbContextId = Guid.NewGuid().ToString();
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildImporterLocationService();
                var result1 = service.Find(statisticsDbContext, country,
                    englishDevolvedArea: lowerCase);
                Assert.NotNull(result1);
                Assert.Equal("casesensitive", result1.EnglishDevolvedArea_Name);
                var result2 = service.Find(statisticsDbContext, country,
                    englishDevolvedArea: upperCase);
                Assert.NotNull(result2);
                Assert.Equal("CASESENSITIVE", result2.EnglishDevolvedArea_Name);
                await statisticsDbContext.SaveChangesAsync();
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
