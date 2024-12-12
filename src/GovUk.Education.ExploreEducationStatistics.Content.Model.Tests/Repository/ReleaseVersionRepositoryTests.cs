using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.ComparerUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Repository;

public class ReleaseVersionRepositoryTests
{
    private readonly DataFixture _dataFixture = new();

    public class GetLatestPublishedReleaseVersionTests : ReleaseVersionRepositoryTests
    {
        [Fact]
        public async Task Success()
        {
            var publications = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => ListOf<Release>(
                    _dataFixture
                        .DefaultRelease(publishedVersions: 0, draftVersion: true, year: 2022),
                    _dataFixture
                        .DefaultRelease(publishedVersions: 2, draftVersion: true, year: 2021),
                    _dataFixture
                        .DefaultRelease(publishedVersions: 2, year: 2020)))
                .GenerateList(2);

            var contextId = await AddTestData(publications);
            await using var contentDbContext = InMemoryContentDbContext(contextId);
            var repository = BuildRepository(contentDbContext);

            var result = await repository.GetLatestPublishedReleaseVersion(publications[0].Id);

            // Expect the result to be the latest published version taken from releases of the specified publication in
            // reverse chronological order
            var expectedReleaseVersion = publications[0].ReleaseVersions
                .Single(rv => rv is { Published: not null, Year: 2021, Version: 1 });

            Assert.NotNull(result);
            Assert.Equal(expectedReleaseVersion.Id, result.Id);
        }

        [Fact]
        public async Task PublicationHasNoPublishedReleaseVersions_ReturnsNull()
        {
            var publications = _dataFixture
                .DefaultPublication()
                // Index 0 has an unpublished release version
                // Index 1 has a published release version
                .ForIndex(0, p => p.SetReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 0, draftVersion: true)
                    .Generate(1)))
                .ForIndex(1, p => p.SetReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 1)
                    .Generate(1)))
                .GenerateList(2);

            var contextId = await AddTestData(publications);
            await using var contentDbContext = InMemoryContentDbContext(contextId);
            var repository = BuildRepository(contentDbContext);

            Assert.Null(await repository.GetLatestPublishedReleaseVersion(publications[0].Id));
        }

        [Fact]
        public async Task PublicationHasNoReleaseVersions_ReturnsNull()
        {
            var publications = _dataFixture
                .DefaultPublication()
                // Index 0 has no release versions
                // Index 1 has a published release version
                .ForIndex(1, p => p.SetReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 1)
                    .Generate(1)))
                .GenerateList(2);

            var contextId = await AddTestData(publications);
            await using var contentDbContext = InMemoryContentDbContext(contextId);
            var repository = BuildRepository(contentDbContext);

            Assert.Null(await repository.GetLatestPublishedReleaseVersion(publications[0].Id));
        }

        [Fact]
        public async Task PublicationDoesNotExist_ReturnsNull()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 1)
                    .Generate(1));

            var contextId = await AddTestData(publication);
            await using var contentDbContext = InMemoryContentDbContext(contextId);
            var repository = BuildRepository(contentDbContext);

            Assert.Null(await repository.GetLatestPublishedReleaseVersion(Guid.NewGuid()));
        }

        [Fact]
        public async Task SpecificReleaseSlug_Success()
        {
            var publications = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => ListOf<Release>(
                    _dataFixture
                        .DefaultRelease(publishedVersions: 1, year: 2022),
                    _dataFixture
                        .DefaultRelease(publishedVersions: 2, draftVersion: true, year: 2021)))
                .GenerateList(2);

            var contextId = await AddTestData(publications);
            await using var contentDbContext = InMemoryContentDbContext(contextId);
            var repository = BuildRepository(contentDbContext);

            var result = await repository.GetLatestPublishedReleaseVersion(publications[0].Id, "2021-22");

            // Expect the result to be the latest published version for the 2021-22 release of the specified publication
            var expectedReleaseVersion = publications[0].ReleaseVersions
                .Single(rv => rv is { Published: not null, Year: 2021, Version: 1 });

            Assert.NotNull(result);
            Assert.Equal(expectedReleaseVersion.Id, result.Id);
        }

        [Fact]
        public async Task SpecificReleaseSlug_PublicationHasNoPublishedReleaseVersions_ReturnsNull()
        {
            var publications = _dataFixture
                .DefaultPublication()
                // Index 0 has an unpublished release version
                // Index 1 has a published release version
                .ForIndex(0, p => p.SetReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 0, draftVersion: true, year: 2021)
                    .Generate(1)))
                .ForIndex(1, p => p.SetReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 1, year: 2021)
                    .Generate(1)))
                .GenerateList(2);

            var contextId = await AddTestData(publications);
            await using var contentDbContext = InMemoryContentDbContext(contextId);
            var repository = BuildRepository(contentDbContext);

            Assert.Null(await repository.GetLatestPublishedReleaseVersion(publications[0].Id, "2021-22"));
        }

        [Fact]
        public async Task SpecificReleaseSlug_PublicationHasNoPublishedReleaseVersionsMatchingSlug_ReturnsNull()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 1, year: 2020)
                    .Generate(1));

            var contextId = await AddTestData(publication);
            await using var contentDbContext = InMemoryContentDbContext(contextId);
            var repository = BuildRepository(contentDbContext);

            Assert.Null(await repository.GetLatestPublishedReleaseVersion(publication.Id, "2021-22"));
        }

        [Fact]
        public async Task SpecificReleaseSlug_PublicationHasNoReleaseVersions_ReturnsNull()
        {
            var publications = _dataFixture
                .DefaultPublication()
                // Index 0 has no release versions
                // Index 1 has a published release version
                .ForIndex(1, p => p.SetReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 1, year: 2021)
                    .Generate(1)))
                .GenerateList(2);

            var contextId = await AddTestData(publications);
            await using var contentDbContext = InMemoryContentDbContext(contextId);
            var repository = BuildRepository(contentDbContext);

            Assert.Null(await repository.GetLatestPublishedReleaseVersion(publications[0].Id, "2021-22"));
        }

        [Fact]
        public async Task SpecificReleaseSlug_PublicationDoesNotExist_ReturnsNull()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 1, year: 2021)
                    .Generate(1));

            var contextId = await AddTestData(publication);
            await using var contentDbContext = InMemoryContentDbContext(contextId);
            var repository = BuildRepository(contentDbContext);

            Assert.Null(await repository.GetLatestPublishedReleaseVersion(Guid.NewGuid(), "2021-22"));
        }
    }

    public class GetLatestReleaseVersionTests : ReleaseVersionRepositoryTests
    {
        [Fact]
        public async Task Success()
        {
            var publications = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => ListOf<Release>(
                    _dataFixture
                        .DefaultRelease(publishedVersions: 0, draftVersion: true, year: 2022),
                    _dataFixture
                        .DefaultRelease(publishedVersions: 2, draftVersion: true, year: 2021),
                    _dataFixture
                        .DefaultRelease(publishedVersions: 2, year: 2020)))
                .GenerateList(2);

            var contextId = await AddTestData(publications);
            await using var contentDbContext = InMemoryContentDbContext(contextId);
            var repository = BuildRepository(contentDbContext);

            var result = await repository.GetLatestReleaseVersion(publications[0].Id);

            // Expect the result to be the latest version taken from releases of the specified publication in
            // reverse chronological order
            var expectedReleaseVersion = publications[0].ReleaseVersions
                .Single(rv => rv is { Year: 2022, Version: 0 });

            Assert.NotNull(result);
            Assert.Equal(expectedReleaseVersion.Id, result.Id);
        }

        [Fact]
        public async Task PublicationHasNoReleaseVersions_ReturnsNull()
        {
            var publications = _dataFixture
                .DefaultPublication()
                // Index 0 has no release versions
                // Index 1 has a release version
                .ForIndex(1, p => p.SetReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 1)
                    .Generate(1)))
                .GenerateList(2);

            var contextId = await AddTestData(publications);
            await using var contentDbContext = InMemoryContentDbContext(contextId);
            var repository = BuildRepository(contentDbContext);

            Assert.Null(await repository.GetLatestReleaseVersion(publications[0].Id));
        }

        [Fact]
        public async Task PublicationDoesNotExist_ReturnsNull()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 1)
                    .Generate(1));

            var contextId = await AddTestData(publication);
            await using var contentDbContext = InMemoryContentDbContext(contextId);
            var repository = BuildRepository(contentDbContext);

            Assert.Null(await repository.GetLatestReleaseVersion(Guid.NewGuid()));
        }
    }

    public class IsLatestPublishedReleaseVersionTests : ReleaseVersionRepositoryTests
    {
        [Fact]
        public async Task Success()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 2, draftVersion: true)
                    .Generate(1));

            var contextId = await AddTestData(publication);
            await using var contentDbContext = InMemoryContentDbContext(contextId);
            var repository = BuildRepository(contentDbContext);

            var releaseVersions = publication.ReleaseVersions;

            // Expect only the highest published version of the release to be the latest
            Assert.False(await repository.IsLatestPublishedReleaseVersion(releaseVersions[0].Id));
            Assert.True(await repository.IsLatestPublishedReleaseVersion(releaseVersions[1].Id));
            Assert.False(await repository.IsLatestPublishedReleaseVersion(releaseVersions[2].Id));
        }

        [Fact]
        public async Task ReleaseHasNoPublishedReleaseVersions_ReturnsFalse()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 0, draftVersion: true)
                    .Generate(1));

            var contextId = await AddTestData(publication);
            await using var contentDbContext = InMemoryContentDbContext(contextId);
            var repository = BuildRepository(contentDbContext);

            var releaseVersion = publication.ReleaseVersions.Single();

            Assert.False(await repository.IsLatestPublishedReleaseVersion(releaseVersion.Id));
        }

        [Fact]
        public async Task ReleaseVersionDoesNotExist_ReturnsFalse()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 1)
                    .Generate(1));

            var contextId = await AddTestData(publication);
            await using var contentDbContext = InMemoryContentDbContext(contextId);
            var repository = BuildRepository(contentDbContext);

            Assert.False(await repository.IsLatestPublishedReleaseVersion(Guid.NewGuid()));
        }
    }

    public class IsLatestReleaseVersionTests : ReleaseVersionRepositoryTests
    {
        [Fact]
        public async Task Success()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 2, draftVersion: true)
                    .Generate(1));

            var contextId = await AddTestData(publication);
            await using var contentDbContext = InMemoryContentDbContext(contextId);
            var repository = BuildRepository(contentDbContext);

            var releaseVersions = publication.ReleaseVersions;

            // Expect only the highest version of the release to be the latest
            Assert.False(await repository.IsLatestReleaseVersion(releaseVersions[0].Id));
            Assert.False(await repository.IsLatestReleaseVersion(releaseVersions[1].Id));
            Assert.True(await repository.IsLatestReleaseVersion(releaseVersions[2].Id));
        }

        [Fact]
        public async Task ReleaseVersionDoesNotExist_ReturnsFalse()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 1)
                    .Generate(1));

            var contextId = await AddTestData(publication);
            await using var contentDbContext = InMemoryContentDbContext(contextId);
            var repository = BuildRepository(contentDbContext);

            Assert.False(await repository.IsLatestReleaseVersion(Guid.NewGuid()));
        }
    }

    public class ListLatestPublishedReleaseVersionIdsTests : ReleaseVersionRepositoryTests
    {
        [Fact]
        public async Task Success()
        {
            var publications = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => ListOf<Release>(
                    _dataFixture
                        .DefaultRelease(publishedVersions: 0, draftVersion: true),
                    _dataFixture
                        .DefaultRelease(publishedVersions: 2, draftVersion: true),
                    _dataFixture
                        .DefaultRelease(publishedVersions: 2)))
                .GenerateList(2);

            var contextId = await AddTestData(publications);
            await using var contentDbContext = InMemoryContentDbContext(contextId);
            var repository = BuildRepository(contentDbContext);

            var result = await repository.ListLatestPublishedReleaseVersionIds(publications[0].Id);

            // Expect the result to contain the highest published version of each release for the specified publication
            AssertIdsAreEqualIgnoringOrder(
                [
                    publications[0].ReleaseVersions[2].Id,
                    publications[0].ReleaseVersions[5].Id
                ],
                result);
        }

        [Fact]
        public async Task PublicationHasNoPublishedReleaseVersions_ReturnsEmpty()
        {
            var publications = _dataFixture
                .DefaultPublication()
                // Index 0 has an unpublished release version
                // Index 1 has a published release version
                .ForIndex(0, p => p.SetReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 0, draftVersion: true)
                    .Generate(1)))
                .ForIndex(1, p => p.SetReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 1)
                    .Generate(1)))
                .GenerateList(2);

            var contextId = await AddTestData(publications);
            await using var contentDbContext = InMemoryContentDbContext(contextId);
            var repository = BuildRepository(contentDbContext);

            Assert.Empty(await repository.ListLatestPublishedReleaseVersionIds(publications[0].Id));
        }

        [Fact]
        public async Task PublicationHasNoReleaseVersions_ReturnsEmpty()
        {
            var publications = _dataFixture
                .DefaultPublication()
                .ForIndex(1, p => p.SetReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 1)
                    .Generate(1)))
                .GenerateList(2);

            var contextId = await AddTestData(publications);
            await using var contentDbContext = InMemoryContentDbContext(contextId);
            var repository = BuildRepository(contentDbContext);

            Assert.Empty(await repository.ListLatestPublishedReleaseVersionIds(publications[0].Id));
        }

        [Fact]
        public async Task PublicationDoesNotExist_ReturnsEmpty()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 1)
                    .Generate(1));

            var contextId = await AddTestData(publication);
            await using var contentDbContext = InMemoryContentDbContext(contextId);
            var repository = BuildRepository(contentDbContext);

            Assert.Empty(await repository.ListLatestPublishedReleaseVersionIds(Guid.NewGuid()));
        }
    }

    public class ListLatestPublishedReleaseVersionsTests : ReleaseVersionRepositoryTests
    {
        [Fact]
        public async Task Success()
        {
            var publications = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => ListOf<Release>(
                    _dataFixture
                        .DefaultRelease(publishedVersions: 0, draftVersion: true),
                    _dataFixture
                        .DefaultRelease(publishedVersions: 2, draftVersion: true),
                    _dataFixture
                        .DefaultRelease(publishedVersions: 2)))
                .GenerateList(2);

            var contextId = await AddTestData(publications);
            await using var contentDbContext = InMemoryContentDbContext(contextId);
            var repository = BuildRepository(contentDbContext);

            var result = await repository.ListLatestPublishedReleaseVersions(publications[0].Id);

            // Expect the result to contain the highest published version of each release for the specified publication
            AssertIdsAreEqualIgnoringOrder(
                [
                    publications[0].ReleaseVersions[2].Id,
                    publications[0].ReleaseVersions[5].Id
                ],
                result);
        }

        [Fact]
        public async Task PublicationHasNoPublishedReleaseVersions_ReturnsEmpty()
        {
            var publications = _dataFixture
                .DefaultPublication()
                // Index 0 has an unpublished release version
                // Index 1 has a published release version
                .ForIndex(0, p => p.SetReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 0, draftVersion: true)
                    .Generate(1)))
                .ForIndex(1, p => p.SetReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 1)
                    .Generate(1)))
                .GenerateList(2);

            var contextId = await AddTestData(publications);
            await using var contentDbContext = InMemoryContentDbContext(contextId);
            var repository = BuildRepository(contentDbContext);

            Assert.Empty(await repository.ListLatestPublishedReleaseVersions(publications[0].Id));
        }

        [Fact]
        public async Task PublicationHasNoReleaseVersions_ReturnsEmpty()
        {
            var publications = _dataFixture
                .DefaultPublication()
                // Index 0 has no release versions
                // Index 1 has a published release version
                .ForIndex(1, p => p.SetReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 1)
                    .Generate(1)))
                .GenerateList(2);

            var contextId = await AddTestData(publications);
            await using var contentDbContext = InMemoryContentDbContext(contextId);
            var repository = BuildRepository(contentDbContext);

            Assert.Empty(await repository.ListLatestPublishedReleaseVersions(publications[0].Id));
        }

        [Fact]
        public async Task PublicationDoesNotExist_ReturnsEmpty()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 1)
                    .Generate(1));

            var contextId = await AddTestData(publication);
            await using var contentDbContext = InMemoryContentDbContext(contextId);
            var repository = BuildRepository(contentDbContext);

            Assert.Empty(await repository.ListLatestPublishedReleaseVersions(Guid.NewGuid()));
        }
    }

    public class ListLatestReleaseVersionIdsTests : ReleaseVersionRepositoryTests
    {
        [Fact]
        public async Task Success()
        {
            var publications = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => ListOf<Release>(
                    _dataFixture
                        .DefaultRelease(publishedVersions: 0, draftVersion: true),
                    _dataFixture
                        .DefaultRelease(publishedVersions: 2, draftVersion: true),
                    _dataFixture
                        .DefaultRelease(publishedVersions: 2)))
                .GenerateList(2);

            var contextId = await AddTestData(publications);
            await using var contentDbContext = InMemoryContentDbContext(contextId);
            var repository = BuildRepository(contentDbContext);

            var result = await repository.ListLatestReleaseVersionIds(publications[0].Id);

            // Expect the result to contain the highest version of each release for the specified publication
            AssertIdsAreEqualIgnoringOrder(
                [
                    publications[0].ReleaseVersions[0].Id,
                    publications[0].ReleaseVersions[3].Id,
                    publications[0].ReleaseVersions[5].Id
                ],
                result);
        }

        [Fact]
        public async Task PublicationHasNoReleaseVersions_ReturnsEmpty()
        {
            var publications = _dataFixture
                .DefaultPublication()
                // Index 0 has no release versions
                // Index 1 has a published release version
                .ForIndex(1, p => p.SetReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 1)
                    .Generate(1)))
                .GenerateList(2);

            var contextId = await AddTestData(publications);
            await using var contentDbContext = InMemoryContentDbContext(contextId);
            var repository = BuildRepository(contentDbContext);

            Assert.Empty(await repository.ListLatestReleaseVersionIds(publications[0].Id));
        }

        [Fact]
        public async Task PublicationDoesNotExist_ReturnsEmpty()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 1)
                    .Generate(1));

            var contextId = await AddTestData(publication);
            await using var contentDbContext = InMemoryContentDbContext(contextId);
            var repository = BuildRepository(contentDbContext);

            Assert.Empty(await repository.ListLatestReleaseVersionIds(Guid.NewGuid()));
        }
    }

    private static void AssertIdsAreEqualIgnoringOrder(
        IReadOnlyCollection<Guid> expectedIds,
        IReadOnlyCollection<ReleaseVersion> actualReleaseVersions)
    {
        Assert.Equal(expectedIds.Count, actualReleaseVersions.Count);
        Assert.True(SequencesAreEqualIgnoringOrder(expectedIds, actualReleaseVersions.Select(rv => rv.Id)));
    }

    private static void AssertIdsAreEqualIgnoringOrder(
        IReadOnlyCollection<Guid> expectedIds,
        IReadOnlyCollection<Guid> actualIds)
    {
        Assert.Equal(expectedIds.Count, actualIds.Count);
        Assert.True(SequencesAreEqualIgnoringOrder(expectedIds, actualIds));
    }

    private static async Task<string> AddTestData(params Publication[] publications)
    {
        return await AddTestData(publications.ToList());
    }

    private static async Task<string> AddTestData(IReadOnlyCollection<Publication> publications)
    {
        var contextId = Guid.NewGuid().ToString();
        await using var contentDbContext = InMemoryContentDbContext(contextId);

        contentDbContext.Publications.AddRange(publications);
        await contentDbContext.SaveChangesAsync();

        return contextId;
    }

    private static ReleaseVersionRepository BuildRepository(
        ContentDbContext contentDbContext)
    {
        return new ReleaseVersionRepository(
            contentDbContext: contentDbContext
        );
    }
}
