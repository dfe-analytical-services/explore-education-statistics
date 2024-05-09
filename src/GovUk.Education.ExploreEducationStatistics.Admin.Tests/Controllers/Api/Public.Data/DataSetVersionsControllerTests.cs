#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using Microsoft.AspNetCore.WebUtilities;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.Utils.ClaimsPrincipalUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Public.Data;

public class DataSetVersionsControllerTests(TestApplicationFactory testApp) : IntegrationTestFixture(testApp)
{
    private const string BaseUrl = "api/public-data/data-set-versions";

    public class ListStatusesForReleaseVersionTests(TestApplicationFactory testApp) : DataSetsControllerTests(testApp)
    {
        [Fact]
        public async Task Success_ListReturned()
        {
            ReleaseVersion releaseVersion = DataFixture
                .DefaultReleaseVersion();

            var dataFiles = DataFixture
                .DefaultReleaseFile()
                .WithFile(DataFixture
                    .DefaultFile()
                    .WithType(FileType.Data))
                .WithReleaseVersion(releaseVersion)
                .GenerateList(2);

            var dataSetVersions = DataFixture
                .DefaultDataSetVersion(filters: 1,
                    indicators: 1,
                    locations: 1,
                    timePeriods: 2)
                .ForIndex(0, s => s
                    .SetStatus(DataSetVersionStatus.Draft)
                    .SetDataSet(DataFixture.DefaultDataSet())
                    .SetReleaseFileId(dataFiles[0].Id))
                .ForIndex(1, s => s
                    .SetStatus(DataSetVersionStatus.Processing)
                    .SetDataSet(DataFixture.DefaultDataSet())
                    .SetReleaseFileId(dataFiles[1].Id))
                .GenerateList();

            await TestApp.AddTestData<ContentDbContext>(context =>
                context.ReleaseFiles.AddRange(dataFiles));

            await TestApp.AddTestData<PublicDataDbContext>(context =>
                context.DataSetVersions.AddRange(dataSetVersions));

            var response = await TestApp
                .SetUser(BauUser())
                .CreateClient()
                .GetAsync($"{BaseUrl}/statuses?releaseVersionId={releaseVersion.Id}");

            var expectedList = dataSetVersions
                .Select(dataSetVersion => new DataSetVersionStatusViewModel(
                    Id: dataSetVersion.Id,
                    Title: dataSetVersion.DataSet.Title,
                    Status: dataSetVersion.Status))
                .ToList();
            
            var list = response.AssertOk<List<DataSetVersionStatusViewModel>>();
            Assert.Equal(expectedList, list);
        }
        
        [Fact]
        public async Task Success_EmptyList()
        {
            ReleaseVersion releaseVersion = DataFixture
                .DefaultReleaseVersion();
            
            ReleaseVersion otherReleaseVersion = DataFixture
                .DefaultReleaseVersion();

            var dataFiles = DataFixture
                .DefaultReleaseFile()
                .WithFile(DataFixture
                    .DefaultFile()
                    .WithType(FileType.Data))
                .WithReleaseVersion(releaseVersion)
                .GenerateList(2);

            var dataSetVersions = DataFixture
                .DefaultDataSetVersion(filters: 1,
                    indicators: 1,
                    locations: 1,
                    timePeriods: 2)
                .ForIndex(0, s => s
                    .SetStatus(DataSetVersionStatus.Draft)
                    .SetDataSet(DataFixture.DefaultDataSet())
                    .SetReleaseFileId(dataFiles[0].Id))
                .ForIndex(1, s => s
                    .SetStatus(DataSetVersionStatus.Processing)
                    .SetDataSet(DataFixture.DefaultDataSet())
                    .SetReleaseFileId(dataFiles[1].Id))
                .GenerateList();

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseFiles.AddRange(dataFiles);
                context.ReleaseVersions.Add(otherReleaseVersion);
            });

            await TestApp.AddTestData<PublicDataDbContext>(context =>
                context.DataSetVersions.AddRange(dataSetVersions));

            // Request statuses for otherReleaseVersion, which has no DataSetVersions or ReleaseFiles.
            var response = await TestApp
                .SetUser(BauUser())
                .CreateClient()
                .GetAsync($"{BaseUrl}/statuses?releaseVersionId={otherReleaseVersion.Id}");

            var list = response.AssertOk<List<DataSetVersionStatusViewModel>>();
            Assert.Empty(list);
        }
        
        [Fact]
        public async Task ReleaseVersionNotFound()
        {
            // Request statuses for a non-existent ReleaseVersion.
            var response = await TestApp
                .SetUser(BauUser())
                .CreateClient()
                .GetAsync($"{BaseUrl}/statuses?releaseVersionId={Guid.NewGuid()}");

            response.AssertNotFound();
        }
        
        [Fact]
        public async Task UserNotPermitted()
        {
            ReleaseVersion releaseVersion = DataFixture
                .DefaultReleaseVersion();

            await TestApp.AddTestData<ContentDbContext>(context =>
                context.ReleaseVersions.Add(releaseVersion));

            // Test with a user who has no permissions to view the given ReleaseVersion.
            var response = await TestApp
                .SetUser(AuthenticatedUser())
                .CreateClient()
                .GetAsync($"{BaseUrl}/statuses?releaseVersionId={releaseVersion.Id}");

            response.AssertForbidden();
        }
    }
}
