using System.Net;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.WebApp;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Fixtures.Optimised;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Strategies;
using Microsoft.AspNetCore.Http;
using Snapshooter.Xunit;
using Xunit;
using File = System.IO.File;

#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers;

// ReSharper disable once ClassNeverInstantiated.Global
public class DataSetFilesControllerAnalyticsTestsFixture()
    : OptimisedContentApiCollectionFixture(capabilities: [ContentApiIntegrationTestCapability.Azurite])
{
    public IAnalyticsPathResolver AnalyticsPathResolver = null!;
    public IPublicBlobStorageService PublicBlobStorageService = null!;

    protected override void ConfigureServicesAndConfiguration(
        OptimisedServiceAndConfigModifications serviceModifications
    )
    {
        base.ConfigureServicesAndConfiguration(serviceModifications);

        var analyticsBasePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        serviceModifications.AddInMemoryCollection([
            new KeyValuePair<string, string?>("Analytics:BasePath", analyticsBasePath),
            new KeyValuePair<string, string?>("Analytics:Enabled", "true"),
        ]);
    }

    protected override async Task AfterFactoryConstructed(OptimisedServiceCollectionLookups<Startup> lookups)
    {
        await base.AfterFactoryConstructed(lookups);

        AnalyticsPathResolver = lookups.GetService<IAnalyticsPathResolver>();
        PublicBlobStorageService = lookups.GetService<IPublicBlobStorageService>();
    }
}

[CollectionDefinition(nameof(DataSetFilesControllerAnalyticsTestsFixture))]
public class DataSetFilesControllerAnalyticsTestsCollection
    : ICollectionFixture<DataSetFilesControllerAnalyticsTestsFixture>;

[Collection(nameof(DataSetFilesControllerAnalyticsTestsFixture))]
public abstract class DataSetFilesControllerAnalyticsTests(DataSetFilesControllerAnalyticsTestsFixture fixture)
    : OptimisedIntegrationTestBase<Startup>(fixture)
{
    private static readonly DataFixture DataFixture = new();

    public class DownloadDataSetFileAnalyticsTests(DataSetFilesControllerAnalyticsTestsFixture fixture)
        : DataSetFilesControllerAnalyticsTests(fixture)
    {
        [Fact]
        public async Task DownloadDataSetFile_RecordAnalytics()
        {
            Publication publication = DataFixture
                .DefaultPublication()
                .WithReleases(_ => [DataFixture.DefaultRelease(publishedVersions: 1)])
                .WithTheme(DataFixture.DefaultTheme());

            ReleaseFile releaseFile = DataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(publication.Releases[0].Versions[0])
                .WithFile(DataFixture.DefaultFile(FileType.Data));

            var formFile = CreateDataCsvFormFile("test file contents 2");

            await fixture.PublicBlobStorageService.UploadFile(
                containerName: BlobContainers.PublicReleaseFiles,
                releaseFile.PublicPath(),
                formFile
            );

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.ReleaseFiles.Add(releaseFile);
                });

            var client = fixture.CreateClient();

            var response = await client.GetAsync($"/api/data-set-files/{releaseFile.File.DataSetFileId}/download");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var analyticsRequestFiles = Directory
                .GetFiles(
                    fixture.AnalyticsPathResolver.BuildOutputDirectory(
                        AnalyticsWritePublicCsvDownloadStrategy.OutputSubPaths
                    )
                )
                .ToList();

            var requestFile = Assert.Single(analyticsRequestFiles);
            var requestFileContents = await File.ReadAllTextAsync(requestFile);
            Snapshot.Match(
                currentResult: requestFileContents,
                snapshotName: $"{nameof(DownloadDataSetFileAnalyticsTests)}.{nameof(DownloadDataSetFile_RecordAnalytics)}.snap"
            );
        }
    }

    private static IFormFile CreateDataCsvFormFile(string content)
    {
        var memoryStream = new MemoryStream();
        var writer = new StreamWriter(memoryStream);
        writer.Write(content);
        writer.Flush();
        memoryStream.Position = 0;

        var headerDictionary = new HeaderDictionary { ["ContentType"] = "text/csv" };

        return new FormFile(memoryStream, 0, memoryStream.Length, "id_from_form", "dataCsv.csv")
        {
            Headers = headerDictionary,
        };
    }
}
