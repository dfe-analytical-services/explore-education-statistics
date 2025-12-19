using System.Net.Http.Json;
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
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers;

// ReSharper disable once ClassNeverInstantiated.Global
public class ReleaseFileControllerTestsFixture : OptimisedContentApiCollectionFixture;

[CollectionDefinition(nameof(ReleaseFileControllerTestsFixture))]
public class ReleaseFileControllerTestsCollection : ICollectionFixture<ReleaseFileControllerTestsFixture>;

[Collection(nameof(ReleaseFileControllerTestsFixture))]
public abstract class ReleaseFileControllerTests(ReleaseFileControllerTestsFixture fixture)
    : OptimisedIntegrationTestBase<Startup>(fixture)
{
    private static readonly DataFixture DataFixture = new();

    public class ListReleaseFilesTests(ReleaseFileControllerTestsFixture fixture) : ReleaseFileControllerTests(fixture)
    {
        [Fact]
        public async Task Success_FiltersByIds()
        {
            Publication publication = DataFixture
                .DefaultPublication()
                .WithReleases(DataFixture.DefaultRelease(publishedVersions: 1).GenerateList(1));

            var releaseFiles = DataFixture
                .DefaultReleaseFile()
                .WithFile(() => DataFixture.DefaultFile(FileType.Data))
                .WithReleaseVersion(publication.Releases[0].Versions[0])
                .GenerateList(4);

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.Publications.Add(publication);
                    context.ReleaseFiles.AddRange(releaseFiles);
                });

            var request = new ReleaseFileListRequest { Ids = [releaseFiles[0].Id, releaseFiles[1].Id] };

            var response = await ListReleaseFiles(request);

            var viewModels = response.AssertOk<List<ReleaseFileViewModel>>();

            Assert.Equal(2, viewModels.Count);

            var expectedReleaseSummary = new ReleaseSummaryViewModel(
                publication.Releases[0].Versions[0],
                latestPublishedRelease: true
            )
            {
                Publication = new PublicationSummaryViewModel(publication),
            };

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
            Publication publication = DataFixture
                .DefaultPublication()
                .WithReleases(DataFixture.DefaultRelease(publishedVersions: 1, draftVersion: true).GenerateList(1));

            var publishedReleaseVersion = publication.Releases[0].Versions[0];
            var unpublishedReleaseVersion = publication.Releases[0].Versions[1];

            var releaseFiles = DataFixture
                .DefaultReleaseFile()
                .WithFile(() => DataFixture.DefaultFile(FileType.Data))
                .ForRange(..2, rf => rf.SetReleaseVersion(unpublishedReleaseVersion))
                .ForRange(2..4, rf => rf.SetReleaseVersion(publishedReleaseVersion))
                .GenerateList();

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.Publications.Add(publication);
                    context.ReleaseFiles.AddRange(releaseFiles);
                });

            var request = new ReleaseFileListRequest { Ids = [.. releaseFiles.Select(rf => rf.Id)] };

            var response = await ListReleaseFiles(request);

            var viewModels = response.AssertOk<List<ReleaseFileViewModel>>();

            Assert.Equal(2, viewModels.Count);

            var expectedReleaseSummary = new ReleaseSummaryViewModel(
                publishedReleaseVersion,
                latestPublishedRelease: true
            )
            {
                Publication = new PublicationSummaryViewModel(publication),
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
            Publication publication = DataFixture
                .DefaultPublication()
                .WithReleases(DataFixture.DefaultRelease(publishedVersions: 3).GenerateList(1));

            var latestPublishedReleaseVersion = publication.Releases[0].Versions[2];

            var releaseFiles = DataFixture
                .DefaultReleaseFile()
                .WithFile(() => DataFixture.DefaultFile(FileType.Data))
                .ForRange(..2, rf => rf.SetReleaseVersion(publication.Releases[0].Versions[0]))
                .ForRange(2..4, rf => rf.SetReleaseVersion(publication.Releases[0].Versions[1]))
                .ForRange(4..6, rf => rf.SetReleaseVersion(latestPublishedReleaseVersion))
                .GenerateList();

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.Publications.Add(publication);
                    context.ReleaseFiles.AddRange(releaseFiles);
                });

            var request = new ReleaseFileListRequest { Ids = [.. releaseFiles.Select(rf => rf.Id)] };

            var response = await ListReleaseFiles(request);

            var viewModels = response.AssertOk<List<ReleaseFileViewModel>>();

            Assert.Equal(2, viewModels.Count);

            var expectedReleaseSummary = new ReleaseSummaryViewModel(
                latestPublishedReleaseVersion,
                latestPublishedRelease: true
            )
            {
                Publication = new PublicationSummaryViewModel(publication),
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
            return await fixture.CreateClient().PostAsJsonAsync("/api/release-files", request);
        }
    }
}
