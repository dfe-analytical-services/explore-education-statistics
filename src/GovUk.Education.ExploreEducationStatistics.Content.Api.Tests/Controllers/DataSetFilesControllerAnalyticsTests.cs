#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Snapshooter.Xunit;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers;

public abstract class DataSetFilesControllerAnalyticsTests : IntegrationTestFixture
{
    private readonly DataFixture _fixture = new();

    private DataSetFilesControllerAnalyticsTests(TestApplicationFactory testApp) : base(testApp)
    {
    }

    public class DownloadDataSetFileAnalyticsTests(TestApplicationFactory testApp)
        : DataSetFilesControllerAnalyticsTests(testApp)
    {
        public override async Task InitializeAsync() => await StartAzurite();

        [Fact]
        public async Task DownloadDataSetFile_RecordAnalytics()
        {
            Publication publication = _fixture.DefaultPublication()
                .WithReleases(_ => [_fixture.DefaultRelease(publishedVersions: 1)])
                .WithTheme(_fixture.DefaultTheme());

            ReleaseFile releaseFile = _fixture.DefaultReleaseFile()
                .WithReleaseVersion(publication.Releases[0].Versions[0])
                .WithFile(_fixture.DefaultFile(FileType.Data));

            var testApp = BuildApp(
                enableAzurite: true,
                enableAnalytics: true);
            var publicBlobStorageService = testApp.Services.GetRequiredService<IPublicBlobStorageService>();
            var analyticsPathResolver = testApp.Services.GetRequiredService<IAnalyticsPathResolver>();

            var formFile = CreateDataCsvFormFile("test file contents 2");

            await publicBlobStorageService.UploadFile(
                containerName: BlobContainers.PublicReleaseFiles,
                releaseFile.PublicPath(),
                formFile);

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseFiles.Add(releaseFile);
            });

            var client = testApp.CreateClient();

            var response = await client.GetAsync($"/api/data-set-files/{releaseFile.File.DataSetFileId}/download");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var analyticsRequestFiles = Directory.GetFiles(
                    analyticsPathResolver.PublicCsvDownloadsDirectoryPath())
                .ToList();

            var requestFile = Assert.Single(analyticsRequestFiles);
            var requestFileContents = await System.IO.File.ReadAllTextAsync(requestFile);
            Snapshot.Match(
                currentResult: requestFileContents,
                snapshotName: $"{nameof(DownloadDataSetFileAnalyticsTests)}.{nameof(DownloadDataSetFile_RecordAnalytics)}.snap");
        }
    }

    private WebApplicationFactory<Startup> BuildApp(
        ContentDbContext? contentDbContext = null,
        StatisticsDbContext? statisticsDbContext = null,
        bool enableAzurite = false,
        bool enableAnalytics = false)
    {
        List<Action<IWebHostBuilder>> configFuncs = [];

        if (enableAzurite)
        {
            configFuncs.Add(WithAzurite());
        }

        if (enableAnalytics)
        {
            configFuncs.Add(WithAnalytics());
        }

        return BuildWebApplicationFactory(configFuncs)
            .ConfigureServices(services =>
            {
                services.ReplaceService(MemoryCacheService);

                if (contentDbContext is not null)
                {
                    services.ReplaceService(contentDbContext);
                }

                if (statisticsDbContext is not null)
                {
                    services.ReplaceService(statisticsDbContext);
                }
            });
    }

    private static IFormFile CreateDataCsvFormFile(string content)
    {
        var memoryStream = new MemoryStream();
        var writer = new StreamWriter(memoryStream);
        writer.Write(content);
        writer.Flush();
        memoryStream.Position = 0;

        var headerDictionary = new HeaderDictionary
        {
            ["ContentType"] = "text/csv"
        };

        return new FormFile(
            memoryStream,
            0,
            memoryStream.Length,
            "id_from_form",
            "dataCsv.csv")
        {
            Headers = headerDictionary,
        };
    }
}
