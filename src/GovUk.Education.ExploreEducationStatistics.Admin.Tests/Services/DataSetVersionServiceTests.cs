#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.EnumUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class DataSetVersionServiceTests(TestApplicationFactory testApp) : IntegrationTestFixture(testApp)
{
    public class GetStatusesForReleaseVersionTests(TestApplicationFactory testApp) : DataSetVersionServiceTests(testApp)
    {
        /// <summary>
        /// Test that each DataSetVersion status is reported correctly.
        /// </summary>
        [Fact]
        public async Task AllStatuses()
        {
            await GetEnums<DataSetVersionStatus>()
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(AssertDataSetVersionStatusReturnedOk);
        }
        
        /// <summary>
        /// Test the scenario when data set versions exist, but for an unrelated ReleaseVersion.
        /// </summary>
        [Fact]
        public async Task DataSetVersionForDifferentReleaseVersion()
        {
            var unrelatedReleaseVersion = Guid.NewGuid();
            
            ReleaseVersion releaseVersion = DataFixture
                .DefaultReleaseVersion();

            ReleaseFile dataFile = DataFixture
                .DefaultReleaseFile()
                .WithFile(DataFixture.DefaultFile(FileType.Data))
                .WithReleaseVersion(releaseVersion);

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1,
                    indicators: 1,
                    locations: 1,
                    timePeriods: 2)
                .WithReleaseFileId(dataFile.Id)
                .WithDataSet(DataFixture.DefaultDataSet())
                .WithStatus(DataSetVersionStatus.Processing);

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseFiles.Add(dataFile);
            });

            await TestApp.AddTestData<PublicDataDbContext>(context =>
                context.DataSetVersions.Add(dataSetVersion));

            var service = testApp.Services.GetRequiredService<IDataSetVersionService>();

            Assert.Empty(await service.GetStatusesForReleaseVersion(unrelatedReleaseVersion));
        }

        private async Task AssertDataSetVersionStatusReturnedOk(DataSetVersionStatus status)
        {
            ReleaseVersion releaseVersion = DataFixture
                .DefaultReleaseVersion();

            ReleaseFile dataFile = DataFixture
                .DefaultReleaseFile()
                .WithFile(DataFixture.DefaultFile(FileType.Data))
                .WithReleaseVersion(releaseVersion);

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1,
                    indicators: 1,
                    locations: 1,
                    timePeriods: 2)
                .WithReleaseFileId(dataFile.Id)
                .WithDataSet(DataFixture.DefaultDataSet())
                .WithStatus(status);

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseFiles.Add(dataFile);
            });

            await TestApp.AddTestData<PublicDataDbContext>(context =>
                context.DataSetVersions.Add(dataSetVersion));

            var service = testApp.Services.GetRequiredService<IDataSetVersionService>();

            var statusSummary = Assert.Single(await service.GetStatusesForReleaseVersion(dataFile.ReleaseVersionId));
            Assert.Equal(dataSetVersion.Id, statusSummary.Id);
            Assert.Equal(dataSetVersion.DataSet.Title, statusSummary.Title);
            Assert.Equal(status, statusSummary.Status);
        }
    }
}
