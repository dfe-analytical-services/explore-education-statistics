using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests;

public class DataSetFileServiceTests
{
    private readonly DataFixture _dataFixture = new();

    [Fact]
    public async Task DownloadDataSetFile_DirectDownloadsEnabled_RedirectsLatestPublishedCsv()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)]);
        ReleaseFile releaseFile = _dataFixture
            .DefaultReleaseFile()
            .WithReleaseVersion(publication.Releases[0].Versions[0])
            .WithFile(_dataFixture.DefaultFile(FileType.Data));
        await using var contentDbContext = InMemoryContentDbContext(Guid.NewGuid().ToString());
        contentDbContext.ReleaseFiles.Add(releaseFile);
        await contentDbContext.SaveChangesAsync();

        var releaseVersionRepository = new Mock<IReleaseVersionRepository>(MockBehavior.Strict);
        releaseVersionRepository
            .Setup(repository =>
                repository.IsLatestPublishedReleaseVersion(releaseFile.ReleaseVersionId, CancellationToken.None)
            )
            .ReturnsAsync(true);
        var releaseFileBlobService = new Mock<IPublicReleaseFileBlobService>(MockBehavior.Strict);
        releaseFileBlobService
            .Setup(service => service.GetDownloadRedirectPath(releaseFile, CancellationToken.None))
            .ReturnsAsync("/downloads/release/data/file.csv");

        var service = new DataSetFileService(
            contentDbContext,
            releaseVersionRepository.Object,
            releaseFileBlobService.Object,
            Mock.Of<IFootnoteRepository>(),
            Mock.Of<IAnalyticsManager>(),
            Options.Create(new DirectBlobDownloadsOptions { Enabled = true }),
            Mock.Of<ILogger<DataSetFileService>>()
        );

        var result = await service.DownloadDataSetFile(releaseFile.File.DataSetFileId!.Value, CancellationToken.None);

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/downloads/release/data/file.csv", redirect.Url);
        releaseFileBlobService.VerifyAll();
    }
}
