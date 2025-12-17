using System.Net.Mime;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.WebApp;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Fixtures.Optimised;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers;

// ReSharper disable once ClassNeverInstantiated.Global
public class ReleaseFileControllerStreamTestsFixture : OptimisedContentApiCollectionFixture
{
    public Mock<IReleaseFileService> ReleaseFileServiceMock = null!;

    protected override void ConfigureServicesAndConfiguration(
        OptimisedServiceAndConfigModifications serviceModifications
    )
    {
        base.ConfigureServicesAndConfiguration(serviceModifications);

        ReleaseFileServiceMock = new Mock<IReleaseFileService>();
        serviceModifications.ReplaceService(ReleaseFileServiceMock.Object);
    }

    public override async Task BeforeEachTest()
    {
        await base.BeforeEachTest();

        ReleaseFileServiceMock.Reset();
    }
}

[CollectionDefinition(nameof(ReleaseFileControllerStreamTestsFixture))]
public class ReleaseFileControllerStreamTestsCollection : ICollectionFixture<ReleaseFileControllerStreamTestsFixture>;

[Collection(nameof(ReleaseFileControllerStreamTestsFixture))]
public abstract class ReleaseFileControllerStreamTests(ReleaseFileControllerStreamTestsFixture fixture)
    : OptimisedIntegrationTestBase<Startup>(fixture)
{
    private static readonly DataFixture DataFixture = new();

    public class StreamTests(ReleaseFileControllerStreamTestsFixture fixture)
        : ReleaseFileControllerStreamTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            Publication publication = DataFixture
                .DefaultPublication()
                .WithReleases(DataFixture.DefaultRelease(publishedVersions: 1).GenerateList(1));

            var releaseVersion = publication.Releases.Single().Versions.Single();

            await using var stream = "Test file".ToStream();

            var fileId = Guid.NewGuid();

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.Publications.Add(publication);
                });

            fixture
                .ReleaseFileServiceMock.Setup(s => s.StreamFile(releaseVersion.Id, fileId))
                .ReturnsAsync(
                    new FileStreamResult(stream, MediaTypeNames.Application.Pdf) { FileDownloadName = "test-file.pdf" }
                );

            var response = await fixture.CreateClient().GetAsync($"/api/releases/{releaseVersion.Id}/files/{fileId}");

            MockUtils.VerifyAllMocks(fixture.ReleaseFileServiceMock);

            response.AssertOk("Test file");
        }
    }

    public class StreamFilesToZipTests(ReleaseFileControllerStreamTestsFixture fixture)
        : ReleaseFileControllerStreamTests(fixture)
    {
        [Fact]
        public async Task ZipWithSpecificFile_Success()
        {
            Publication publication = DataFixture
                .DefaultPublication()
                .WithReleases(DataFixture.DefaultRelease(publishedVersions: 1).GenerateList(1));

            var releaseVersion = publication.Releases.Single().Versions.Single();

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.Publications.Add(publication);
                });

            var fileId = Guid.NewGuid();

            fixture
                .ReleaseFileServiceMock.Setup(s =>
                    s.ZipFilesToStream(
                        releaseVersion.Id,
                        It.IsAny<Stream>(),
                        AnalyticsFromPage.ReleaseUsefulInfo,
                        It.Is<IEnumerable<Guid>>(ids => ids.SequenceEqual(ListOf(fileId))),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(Unit.Instance)
                .Callback<Guid, Stream, AnalyticsFromPage, IEnumerable<Guid>, CancellationToken?>(
                    (_, stream, _, _, _) => stream.WriteText("Test zip")
                );

            var response = await fixture
                .CreateClient()
                .GetAsync($"/api/releases/{releaseVersion.Id}/files?fromPage=ReleaseUsefulInfo&fileIds={fileId}");

            MockUtils.VerifyAllMocks(fixture.ReleaseFileServiceMock);

            response.AssertOk("Test zip");
        }

        [Fact]
        public async Task Success_NoFileIds()
        {
            Publication publication = DataFixture
                .DefaultPublication()
                .WithReleases(DataFixture.DefaultRelease(publishedVersions: 1).GenerateList(1));

            var releaseVersion = publication.Releases.Single().Versions.Single();

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.Publications.Add(publication);
                });

            fixture
                .ReleaseFileServiceMock.Setup(s =>
                    s.ZipFilesToStream(
                        releaseVersion.Id,
                        It.IsAny<Stream>(),
                        AnalyticsFromPage.ReleaseDownloads,
                        null,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(Unit.Instance)
                .Callback<Guid, Stream, AnalyticsFromPage, IEnumerable<Guid>?, CancellationToken?>(
                    (_, stream, _, _, _) => stream.WriteText("Test zip")
                );

            var response = await fixture
                .CreateClient()
                .GetAsync($"/api/releases/{releaseVersion.Id}/files?fromPage=ReleaseDownloads");

            MockUtils.VerifyAllMocks(fixture.ReleaseFileServiceMock);

            response.AssertOk("Test zip");
        }
    }
}
