using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests;

public class DataSetServiceTests
{
    public class GetDataSetTests : DataSetServiceTests
    {
        private readonly DataFixture _fixture = new();

        [Fact]
        public async Task Success()
        {
            var publication = _fixture.DefaultPublication()
                .WithReleaseParents(
                    _fixture.DefaultReleaseParent(publishedVersions: 1, draftVersion: true,  year: 2000)
                        .Generate(2))
                .WithTopic(_fixture.DefaultTopic()
                    .WithTheme(_fixture.DefaultTheme()))
                .Generate();

            var releaseFile = _fixture.DefaultReleaseFile()
                .WithRelease(publication.Releases[0])
                .WithFile(_fixture.DefaultFile())
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.ReleaseFiles.AddRangeAsync(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var dataSetService = BuildService(contentDbContext);

                var result = await dataSetService.GetDataSet(
                    releaseFile.ReleaseId, releaseFile.FileId);
                var viewModel = result.AssertRight();

                Assert.Equal(releaseFile.Name, viewModel.Title);
                Assert.Equal(releaseFile.Summary, viewModel.Summary);

                var file = releaseFile.File;

                Assert.Equal(file.Id, viewModel.File.Id);
                Assert.Equal(file.Filename, viewModel.File.Name);
                Assert.Equal(file.DisplaySize(), viewModel.File.Size);

                Assert.Equal(releaseFile.ReleaseId, viewModel.Release.Id);
                Assert.Equal(releaseFile.Release.Title, viewModel.Release.Title);
                Assert.Equal(releaseFile.Release.Slug, viewModel.Release.Slug);
                Assert.Equal(releaseFile.Release.Type, viewModel.Release.Type);
                Assert.True(viewModel.Release.IsLatestPublishedRelease);
                Assert.Equal(releaseFile.Release.Published, viewModel.Release.Published);

                Assert.Equal(publication.Id, viewModel.Release.Publication.Id);
                Assert.Equal(publication.Title, viewModel.Release.Publication.Title);
                Assert.Equal(publication.Slug, viewModel.Release.Publication.Slug);
                Assert.Equal(publication.Topic.Theme.Title, viewModel.Release.Publication.ThemeTitle);
            }
        }

        [Fact]
        public async Task NoRelease()
        {
            var publication = _fixture.DefaultPublication()
                .WithTopic(_fixture.DefaultTopic()
                    .WithTheme(_fixture.DefaultTheme()))
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddRangeAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var dataSetService = BuildService(contentDbContext);

                var result = await dataSetService.GetDataSet(
                    Guid.NewGuid(), Guid.NewGuid());

                result.AssertNotFound();

            }
        }

        [Fact]
        public async Task NoFile()
        {
            var publication = _fixture.DefaultPublication()
                .WithReleaseParents(
                    _fixture.DefaultReleaseParent(publishedVersions: 1,  year: 2000)
                        .Generate(1))
                .WithTopic(_fixture.DefaultTopic()
                    .WithTheme(_fixture.DefaultTheme()))
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddRangeAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var dataSetService = BuildService(contentDbContext);

                var result = await dataSetService.GetDataSet(
                    publication.Releases[0].Id, Guid.NewGuid());

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task ReleaseNotPublished()
        {
            var publication = _fixture.DefaultPublication()
                .WithReleaseParents(
                    _fixture.DefaultReleaseParent(publishedVersions: 0, draftVersion: true, year: 2000)
                        .Generate(1))
                .WithTopic(_fixture.DefaultTopic()
                    .WithTheme(_fixture.DefaultTheme()))
                .Generate();

            var releaseFile = _fixture.DefaultReleaseFile()
                .WithRelease(publication.Releases[0])
                .WithFile(_fixture.DefaultFile())
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.ReleaseFiles.AddRangeAsync(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var dataSetService = BuildService(contentDbContext);

                var result = await dataSetService.GetDataSet(
                    publication.Releases[0].Id, Guid.NewGuid());

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task AmendmentNotPublished()
        {
            var publication = _fixture.DefaultPublication()
                .WithReleaseParents(
                    _fixture.DefaultReleaseParent(publishedVersions: 1, draftVersion: true, year: 2000)
                        .Generate(2))
                .WithTopic(_fixture.DefaultTopic()
                    .WithTheme(_fixture.DefaultTheme()))
                .Generate();

            var releaseFile = _fixture.DefaultReleaseFile()
                .WithRelease(publication.Releases[1])
                .WithFile(_fixture.DefaultFile())
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.ReleaseFiles.AddRangeAsync(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var dataSetService = BuildService(contentDbContext);

                var result = await dataSetService.GetDataSet(
                    publication.Releases[0].Id, Guid.NewGuid());

                result.AssertNotFound();
            }
        }
    }

    private static DataSetService BuildService(ContentDbContext contentDbContext)
    {
        return new DataSetService(
            contentDbContext,
            new ReleaseRepository(contentDbContext));
    }
}
