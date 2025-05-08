#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers;

public abstract class ReleaseFileControllerTests(TestApplicationFactory testApp) : IntegrationTestFixture(testApp)
{
    public class ListReleaseFilesTests(TestApplicationFactory testApp)
        : ReleaseFileControllerTests(testApp)
    {
        [Fact]
        public async Task Success_FiltersByIds()
        {
            Publication publication = DataFixture.DefaultPublication()
                .WithReleases(DataFixture.DefaultRelease(publishedVersions: 1)
                    .GenerateList(1));

            var releaseFiles = DataFixture.DefaultReleaseFile()
                .WithFile(() => DataFixture.DefaultFile(FileType.Data))
                .WithReleaseVersion(publication.Releases[0].Versions[0])
                .GenerateList(4);

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.Publications.Add(publication);
                context.ReleaseFiles.AddRange(releaseFiles);
            });

            var request = new ReleaseFileListRequest
            {
                Ids = [releaseFiles[0].Id, releaseFiles[1].Id]
            };

            var response = await ListReleaseFiles(request);

            var viewModels = response.AssertOk<List<ReleaseFileViewModel>>();

            Assert.Equal(2, viewModels.Count);

            var expectedReleaseSummary = new ReleaseSummaryViewModel(
                publication.Releases[0].Versions[0],
                latestPublishedRelease: true) { Publication = new PublicationSummaryViewModel(publication) };

            Assert.Equal(releaseFiles[0].Id, viewModels[0].Id);
            Assert.Equal(releaseFiles[0].ToPublicFileInfo(), viewModels[0].File);
            Assert.Equal(expectedReleaseSummary, viewModels[0].Release);

            Assert.Equal(releaseFiles[1].Id, viewModels[1].Id);
            Assert.Equal(releaseFiles[1].ToPublicFileInfo(), viewModels[1].File);
            Assert.Equal(expectedReleaseSummary, viewModels[1].Release);
        }

        [Fact]
        public async Task Success_FiltersUnpublishedReleaseFiles()
        {
            Publication publication = DataFixture.DefaultPublication()
                .WithReleases(DataFixture.DefaultRelease(publishedVersions: 1, draftVersion: true)
                    .GenerateList(1));

            var publishedReleaseVersion = publication.Releases[0].Versions[0];
            var unpublishedReleaseVersion = publication.Releases[0].Versions[1];

            var releaseFiles = DataFixture.DefaultReleaseFile()
                .WithFile(() => DataFixture.DefaultFile(FileType.Data))
                .ForRange(..2, rf => rf
                    .SetReleaseVersion(unpublishedReleaseVersion))
                .ForRange(2..4, rf => rf
                    .SetReleaseVersion(publishedReleaseVersion))
                .GenerateList();

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.Publications.Add(publication);
                context.ReleaseFiles.AddRange(releaseFiles);
            });

            var request = new ReleaseFileListRequest
            {
                Ids = [..releaseFiles.Select(rf => rf.Id)]
            };

            var response = await ListReleaseFiles(request);

            var viewModels = response.AssertOk<List<ReleaseFileViewModel>>();

            Assert.Equal(2, viewModels.Count);

            var expectedReleaseSummary = new ReleaseSummaryViewModel(
                publishedReleaseVersion,
                latestPublishedRelease: true)
            {
                Publication = new PublicationSummaryViewModel(publication)
            };

            Assert.Equal(releaseFiles[2].Id, viewModels[0].Id);
            Assert.Equal(releaseFiles[2].ToPublicFileInfo(), viewModels[0].File);
            Assert.Equal(releaseFiles[2].File.DataSetFileId, viewModels[0].DataSetFileId);
            Assert.Equal(expectedReleaseSummary, viewModels[0].Release);

            Assert.Equal(releaseFiles[3].Id, viewModels[1].Id);
            Assert.Equal(releaseFiles[3].ToPublicFileInfo(), viewModels[1].File);
            Assert.Equal(releaseFiles[3].File.DataSetFileId, viewModels[1].DataSetFileId);
            Assert.Equal(expectedReleaseSummary, viewModels[1].Release);
        }

        [Fact]
        public async Task Success_FiltersNotLatestPublishedReleaseFiles()
        {
            Publication publication = DataFixture.DefaultPublication()
                .WithReleases(DataFixture.DefaultRelease(publishedVersions: 3)
                    .GenerateList(1));

            var latestPublishedReleaseVersion = publication.Releases[0].Versions[2];

            var releaseFiles = DataFixture.DefaultReleaseFile()
                .WithFile(() => DataFixture.DefaultFile(FileType.Data))
                .ForRange(..2, rf => rf
                        .SetReleaseVersion(publication.Releases[0].Versions[0]))
                .ForRange(2..4, rf => rf
                        .SetReleaseVersion(publication.Releases[0].Versions[1]))
                .ForRange(4..6, rf => rf
                        .SetReleaseVersion(latestPublishedReleaseVersion))
                .GenerateList();

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.Publications.Add(publication);
                context.ReleaseFiles.AddRange(releaseFiles);
            });

            var request = new ReleaseFileListRequest
            {
                Ids = [..releaseFiles.Select(rf => rf.Id)]
            };

            var response = await ListReleaseFiles(request);

            var viewModels = response.AssertOk<List<ReleaseFileViewModel>>();

            Assert.Equal(2, viewModels.Count);

            var expectedReleaseSummary = new ReleaseSummaryViewModel(
                latestPublishedReleaseVersion,
                latestPublishedRelease: true)
            {
                Publication = new PublicationSummaryViewModel(publication)
            };

            Assert.Equal(releaseFiles[4].Id, viewModels[0].Id);
            Assert.Equal(releaseFiles[4].ToPublicFileInfo(), viewModels[0].File);
            Assert.Equal(releaseFiles[4].File.DataSetFileId, viewModels[0].DataSetFileId);
            Assert.Equal(expectedReleaseSummary, viewModels[0].Release);

            Assert.Equal(releaseFiles[5].Id, viewModels[1].Id);
            Assert.Equal(releaseFiles[5].ToPublicFileInfo(), viewModels[1].File);
            Assert.Equal(releaseFiles[5].File.DataSetFileId, viewModels[1].DataSetFileId);
            Assert.Equal(expectedReleaseSummary, viewModels[1].Release);
        }

        private async Task<HttpResponseMessage> ListReleaseFiles(ReleaseFileListRequest request)
        {
            var client = BuildApp().CreateClient();

            return await client.PostAsJsonAsync("/api/release-files", request);
        }
    }

    public class StreamTests(TestApplicationFactory testApp) : ReleaseFileControllerTests(testApp)
    {
        [Fact]
        public async Task Success()
        {
            Publication publication = DataFixture.DefaultPublication()
                .WithReleases(DataFixture.DefaultRelease(publishedVersions: 1)
                    .GenerateList(1));

            var releaseVersion = publication.Releases.Single().Versions.Single();

            await using var stream = "Test file".ToStream();

            var fileId = Guid.NewGuid();

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.Publications.Add(publication);
            });

            var releaseFileService = new Mock<IReleaseFileService>(Strict);

            releaseFileService
                .Setup(s => s.StreamFile(releaseVersion.Id, fileId))
                .ReturnsAsync(
                    new FileStreamResult(stream, MediaTypeNames.Application.Pdf)
                    {
                        FileDownloadName = "test-file.pdf",
                    }
                );

            var client = BuildApp(releaseFileService: releaseFileService.Object)
                .CreateClient();

            var response = await client
                .GetAsync($"/api/releases/{releaseVersion.Id}/files/{fileId}");

            MockUtils.VerifyAllMocks(releaseFileService);

            response.AssertOk("Test file");
        }
    }

    public class StreamFilesToZipTests(TestApplicationFactory testApp)
        : ReleaseFileControllerTests(testApp)
    {
        [Fact]
        public async Task ZipWithSpecificFile_Success()
        {
            Publication publication = DataFixture.DefaultPublication()
                .WithReleases(DataFixture.DefaultRelease(publishedVersions: 1)
                    .GenerateList(1));

            var releaseVersion = publication.Releases.Single().Versions.Single();

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.Publications.Add(publication);
            });

            var fileId = Guid.NewGuid();

            var releaseFileService = new Mock<IReleaseFileService>(Strict);

            releaseFileService
                .Setup(
                    s => s.ZipFilesToStream(
                        releaseVersion.Id,
                        It.IsAny<Stream>(),
                        FromPage.ReleaseUsefulInfo,
                        It.Is<IEnumerable<Guid>>(
                            ids => ids.SequenceEqual(ListOf(fileId))),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(Unit.Instance)
                .Callback<Guid, Stream, FromPage, IEnumerable<Guid>, CancellationToken?>(
                    (_, stream, _, _, _) => stream.WriteText("Test zip"));

            var client = BuildApp(releaseFileService: releaseFileService.Object)
                .CreateClient();

            var response = await client
                .GetAsync($"/api/releases/{releaseVersion.Id}/files?fromPage=ReleaseUsefulInfo&fileIds={fileId}");

            MockUtils.VerifyAllMocks(releaseFileService);

            response.AssertOk("Test zip");
        }

        [Fact]
        public async Task Success_NoFileIds()
        {
            Publication publication = DataFixture.DefaultPublication()
                .WithReleases(DataFixture.DefaultRelease(publishedVersions: 1)
                    .GenerateList(1));

            var releaseVersion = publication.Releases.Single().Versions.Single();

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.Publications.Add(publication);
            });

            var releaseFileService = new Mock<IReleaseFileService>(Strict);

            releaseFileService
                .Setup(
                    s => s.ZipFilesToStream(
                        releaseVersion.Id,
                        It.IsAny<Stream>(),
                        FromPage.ReleaseDownloads,
                        null,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(Unit.Instance)
                .Callback<Guid, Stream, FromPage, IEnumerable<Guid>?, CancellationToken?>(
                    (_, stream, _, _, _) => stream.WriteText("Test zip"));

            var client = BuildApp(releaseFileService: releaseFileService.Object)
                .CreateClient();

            var response = await client
                .GetAsync($"/api/releases/{releaseVersion.Id}/files?fromPage=ReleaseDownloads");

            MockUtils.VerifyAllMocks(releaseFileService);

            response.AssertOk("Test zip");
        }
    }

    private WebApplicationFactory<Startup> BuildApp(IReleaseFileService? releaseFileService = null)
    {
        return TestApp
            .ConfigureServices(services =>
            {
                if (releaseFileService is not null)
                {
                    services.ReplaceService(releaseFileService);
                }
            });
    }
}
