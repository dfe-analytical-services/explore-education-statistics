using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.ComparerUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Repository;

public class ReleaseVersionRepositoryTests
{
    private readonly DataFixture _dataFixture = new();

    public class GetLatestPublishedReleaseVersionByReleaseSlugTests : ReleaseVersionRepositoryTests
    {
        [Fact]
        public async Task Success()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(
                    [
                        _dataFixture.DefaultRelease(publishedVersions: 1, year: 2022),
                        _dataFixture.DefaultRelease(
                            publishedVersions: 2,
                            draftVersion: true,
                            year: 2021
                        ),
                    ]
                );

            var contextId = await AddTestData(publication);
            await using var contentDbContext = InMemoryContentDbContext(contextId);
            var repository = BuildRepository(contentDbContext);

            var result = await repository.GetLatestPublishedReleaseVersionByReleaseSlug(
                publication.Id,
                releaseSlug: "2021-22"
            );

            // Expect the result to be the latest published version for the 2021-22 release
            var expectedReleaseVersion = publication
                .Releases.Single(r => r is { Year: 2021 })
                .Versions[1];

            Assert.NotNull(result);
            Assert.Equal(expectedReleaseVersion.Id, result.Id);
        }

        [Fact]
        public async Task MultiplePublications_ReturnsReleaseVersionAssociatedWithPublication()
        {
            var (publication1, publication2) = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1, year: 2021)])
                .GenerateTuple2();

            var contextId = await AddTestData(publication1, publication2);
            await using var contentDbContext = InMemoryContentDbContext(contextId);
            var repository = BuildRepository(contentDbContext);

            var result = await repository.GetLatestPublishedReleaseVersionByReleaseSlug(
                publication1.Id,
                releaseSlug: "2021-22"
            );

            // Expect the result to be from the specified publication
            var expectedReleaseVersion = publication1.Releases.Single().Versions.Single();

            Assert.NotNull(result);
            Assert.Equal(expectedReleaseVersion.Id, result.Id);
        }

        [Fact]
        public async Task PublicationHasNoPublishedReleaseVersions_ReturnsNull()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(
                    [
                        _dataFixture.DefaultRelease(
                            publishedVersions: 0,
                            draftVersion: true,
                            year: 2021
                        ),
                    ]
                );

            var contextId = await AddTestData(publication);
            await using var contentDbContext = InMemoryContentDbContext(contextId);
            var repository = BuildRepository(contentDbContext);

            Assert.Null(
                await repository.GetLatestPublishedReleaseVersionByReleaseSlug(
                    publication.Id,
                    releaseSlug: "2021-22"
                )
            );
        }

        [Fact]
        public async Task PublicationHasNoPublishedReleaseVersionsMatchingSlug_ReturnsNull()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1, year: 2020)]);

            var contextId = await AddTestData(publication);
            await using var contentDbContext = InMemoryContentDbContext(contextId);
            var repository = BuildRepository(contentDbContext);

            Assert.Null(
                await repository.GetLatestPublishedReleaseVersionByReleaseSlug(
                    publication.Id,
                    releaseSlug: "2021-22"
                )
            );
        }

        [Fact]
        public async Task PublicationHasNoReleaseVersions_ReturnsNull()
        {
            Publication publication = _dataFixture.DefaultPublication();

            var contextId = await AddTestData(publication);
            await using var contentDbContext = InMemoryContentDbContext(contextId);
            var repository = BuildRepository(contentDbContext);

            Assert.Null(
                await repository.GetLatestPublishedReleaseVersionByReleaseSlug(
                    publication.Id,
                    releaseSlug: "2021-22"
                )
            );
        }

        [Fact]
        public async Task PublicationDoesNotExist_ReturnsNull()
        {
            var repository = BuildRepository();

            Assert.Null(
                await repository.GetLatestPublishedReleaseVersionByReleaseSlug(
                    publicationId: Guid.NewGuid(),
                    releaseSlug: "2021-22"
                )
            );
        }
    }

    public class GetLatestReleaseVersionTests : ReleaseVersionRepositoryTests
    {
        [Fact]
        public async Task Success()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(
                    [
                        _dataFixture.DefaultRelease(
                            publishedVersions: 2,
                            draftVersion: true,
                            year: 2022
                        ),
                        _dataFixture.DefaultRelease(
                            publishedVersions: 0,
                            draftVersion: true,
                            year: 2021
                        ),
                        _dataFixture.DefaultRelease(publishedVersions: 2, year: 2020),
                    ]
                )
                .FinishWith(p =>
                    p.ReleaseSeries = GenerateReleaseSeries(p.Releases, 2021, 2020, 2022)
                );

            var contextId = await AddTestData(publication);
            await using var contentDbContext = InMemoryContentDbContext(contextId);
            var repository = BuildRepository(contentDbContext);

            var result = await repository.GetLatestReleaseVersion(publication.Id);

            // Expect the result to be the latest version of the latest release in the release series
            var expectedReleaseVersion = publication
                .Releases.Single(r => r is { Year: 2021 })
                .Versions[0];

            Assert.NotNull(result);
            Assert.Equal(expectedReleaseVersion.Id, result.Id);
        }

        [Fact]
        public async Task MultiplePublications_ReturnsReleaseVersionAssociatedWithPublication()
        {
            var (publication1, publication2) = _dataFixture
                .DefaultPublication()
                .WithReleases(_ =>
                    [_dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true)]
                )
                .GenerateTuple2();

            var contextId = await AddTestData(publication1, publication2);
            await using var contentDbContext = InMemoryContentDbContext(contextId);
            var repository = BuildRepository(contentDbContext);

            var result = await repository.GetLatestReleaseVersion(publication1.Id);

            // Expect the result to be from the specified publication
            var expectedReleaseVersion = publication1.Releases.Single().Versions.Single();

            Assert.NotNull(result);
            Assert.Equal(expectedReleaseVersion.Id, result.Id);
        }

        [Fact]
        public async Task PublicationHasNoReleaseVersions_ReturnsNull()
        {
            Publication publication = _dataFixture.DefaultPublication();

            var contextId = await AddTestData(publication);
            await using var contentDbContext = InMemoryContentDbContext(contextId);
            var repository = BuildRepository(contentDbContext);

            Assert.Null(await repository.GetLatestReleaseVersion(publication.Id));
        }

        [Fact]
        public async Task PublicationDoesNotExist_ReturnsNull()
        {
            var repository = BuildRepository();

            Assert.Null(await repository.GetLatestReleaseVersion(publicationId: Guid.NewGuid()));
        }
    }

    public class IsLatestPublishedReleaseVersionTests : ReleaseVersionRepositoryTests
    {
        [Fact]
        public async Task Success()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(
                    [_dataFixture.DefaultRelease(publishedVersions: 2, draftVersion: true)]
                );

            var contextId = await AddTestData(publication);
            await using var contentDbContext = InMemoryContentDbContext(contextId);
            var repository = BuildRepository(contentDbContext);

            var release = publication.Releases.Single();

            // Expect only the latest published version of the release to be returned as the latest
            Assert.False(await repository.IsLatestPublishedReleaseVersion(release.Versions[0].Id));
            Assert.True(await repository.IsLatestPublishedReleaseVersion(release.Versions[1].Id));
            Assert.False(await repository.IsLatestPublishedReleaseVersion(release.Versions[2].Id));
        }

        [Fact]
        public async Task ReleaseHasNoPublishedReleaseVersions_ReturnsFalse()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(
                    [_dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true)]
                );

            var contextId = await AddTestData(publication);
            await using var contentDbContext = InMemoryContentDbContext(contextId);
            var repository = BuildRepository(contentDbContext);

            var releaseVersion = publication.Releases.Single().Versions.Single();

            Assert.False(await repository.IsLatestPublishedReleaseVersion(releaseVersion.Id));
        }

        [Fact]
        public async Task ReleaseVersionDoesNotExist_ReturnsFalse()
        {
            var repository = BuildRepository();

            Assert.False(
                await repository.IsLatestPublishedReleaseVersion(releaseVersionId: Guid.NewGuid())
            );
        }
    }

    public class IsLatestReleaseVersionTests : ReleaseVersionRepositoryTests
    {
        [Fact]
        public async Task Success()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(
                    [_dataFixture.DefaultRelease(publishedVersions: 2, draftVersion: true)]
                );

            var contextId = await AddTestData(publication);
            await using var contentDbContext = InMemoryContentDbContext(contextId);
            var repository = BuildRepository(contentDbContext);

            var release = publication.Releases.Single();

            // Expect only the latest draft version of the release to be returned as the latest
            Assert.False(await repository.IsLatestReleaseVersion(release.Versions[0].Id));
            Assert.False(await repository.IsLatestReleaseVersion(release.Versions[1].Id));
            Assert.True(await repository.IsLatestReleaseVersion(release.Versions[2].Id));
        }

        [Fact]
        public async Task ReleaseVersionDoesNotExist_ReturnsFalse()
        {
            var repository = BuildRepository();

            Assert.False(await repository.IsLatestReleaseVersion(releaseVersionId: Guid.NewGuid()));
        }
    }

    public class ListLatestReleaseVersionsTests : ReleaseVersionRepositoryTests
    {
        public class AnyPublishedStateTests : ListLatestReleaseVersionIdsTests
        {
            [Fact]
            public async Task Success()
            {
                Publication publication = _dataFixture
                    .DefaultPublication()
                    .WithReleases(
                        [
                            _dataFixture.DefaultRelease(
                                publishedVersions: 2,
                                draftVersion: true,
                                year: 2022
                            ),
                            _dataFixture.DefaultRelease(
                                publishedVersions: 0,
                                draftVersion: true,
                                year: 2021
                            ),
                            _dataFixture.DefaultRelease(publishedVersions: 2, year: 2020),
                        ]
                    )
                    .FinishWith(p =>
                        p.ReleaseSeries = GenerateReleaseSeries(p.Releases, 2021, 2020, 2022)
                    );

                var contextId = await AddTestData(publication);
                await using var contentDbContext = InMemoryContentDbContext(contextId);
                var repository = BuildRepository(contentDbContext);

                var result = await ListLatestReleaseVersions(repository, publication.Id);

                // Expect the latest versions of each release, ordered by release series
                Guid[] expectedReleaseVersionIds =
                [
                    publication.Releases.Single(r => r is { Year: 2021 }).Versions[0].Id,
                    publication.Releases.Single(r => r is { Year: 2020 }).Versions[1].Id,
                    publication.Releases.Single(r => r is { Year: 2022 }).Versions[2].Id,
                ];

                Assert.Equal(expectedReleaseVersionIds, result.Select(rv => rv.Id));
            }

            [Fact]
            public async Task MultiplePublications_ReturnsReleaseVersionsAssociatedWithPublication()
            {
                var (publication1, publication2) = _dataFixture
                    .DefaultPublication()
                    .WithReleases(_ =>
                        [
                            _dataFixture.DefaultRelease(
                                publishedVersions: 0,
                                draftVersion: true,
                                year: 2022
                            ),
                            _dataFixture.DefaultRelease(
                                publishedVersions: 0,
                                draftVersion: true,
                                year: 2021
                            ),
                        ]
                    )
                    .GenerateTuple2();

                var contextId = await AddTestData(publication1, publication2);
                await using var contentDbContext = InMemoryContentDbContext(contextId);
                var repository = BuildRepository(contentDbContext);

                var result = await ListLatestReleaseVersions(repository, publication1.Id);

                // Expect the results to be from the specified publication
                AssertIdsAreEqualIgnoringOrder(
                    [
                        publication1.Releases.Single(r => r is { Year: 2022 }).Versions[0].Id,
                        publication1.Releases.Single(r => r is { Year: 2021 }).Versions[0].Id,
                    ],
                    result
                );
            }

            [Fact]
            public async Task PublicationHasNoReleaseVersions_ReturnsEmpty()
            {
                Publication publication = _dataFixture.DefaultPublication();

                var contextId = await AddTestData(publication);
                await using var contentDbContext = InMemoryContentDbContext(contextId);
                var repository = BuildRepository(contentDbContext);

                Assert.Empty(await ListLatestReleaseVersions(repository, publication.Id));
            }

            [Fact]
            public async Task PublicationDoesNotExist_ReturnsEmpty()
            {
                var repository = BuildRepository();

                Assert.Empty(
                    await ListLatestReleaseVersions(repository, publicationId: Guid.NewGuid())
                );
            }

            private static async Task<List<ReleaseVersion>> ListLatestReleaseVersions(
                ReleaseVersionRepository repository,
                Guid publicationId
            )
            {
                return await repository.ListLatestReleaseVersions(
                    publicationId,
                    publishedOnly: false
                );
            }
        }

        public class PublishedOnlyTests : ListLatestReleaseVersionsTests
        {
            [Fact]
            public async Task Success()
            {
                Publication publication = _dataFixture
                    .DefaultPublication()
                    .WithReleases(
                        [
                            _dataFixture.DefaultRelease(
                                publishedVersions: 2,
                                draftVersion: true,
                                year: 2022
                            ),
                            _dataFixture.DefaultRelease(
                                publishedVersions: 0,
                                draftVersion: true,
                                year: 2021
                            ),
                            _dataFixture.DefaultRelease(publishedVersions: 2, year: 2020),
                        ]
                    )
                    .FinishWith(p =>
                        p.ReleaseSeries = GenerateReleaseSeries(p.Releases, 2021, 2020, 2022)
                    );

                var contextId = await AddTestData(publication);
                await using var contentDbContext = InMemoryContentDbContext(contextId);
                var repository = BuildRepository(contentDbContext);

                var result = await ListLatestReleaseVersions(repository, publication.Id);

                // Expect the latest published version of each release, ordered by release series
                Guid[] expectedReleaseVersionIds =
                [
                    publication.Releases.Single(r => r is { Year: 2020 }).Versions[1].Id,
                    publication.Releases.Single(r => r is { Year: 2022 }).Versions[1].Id,
                ];

                Assert.Equal(expectedReleaseVersionIds, result.Select(rv => rv.Id));
            }

            [Fact]
            public async Task MultiplePublications_ReturnsReleaseVersionsAssociatedWithPublication()
            {
                var (publication1, publication2) = _dataFixture
                    .DefaultPublication()
                    .WithReleases(_ =>
                        [
                            _dataFixture.DefaultRelease(publishedVersions: 1, year: 2022),
                            _dataFixture.DefaultRelease(publishedVersions: 1, year: 2021),
                        ]
                    )
                    .GenerateTuple2();

                var contextId = await AddTestData(publication1, publication2);
                await using var contentDbContext = InMemoryContentDbContext(contextId);
                var repository = BuildRepository(contentDbContext);

                var result = await ListLatestReleaseVersions(repository, publication1.Id);

                // Expect the results to be from the specified publication
                AssertIdsAreEqualIgnoringOrder(
                    [
                        publication1.Releases.Single(r => r is { Year: 2022 }).Versions[0].Id,
                        publication1.Releases.Single(r => r is { Year: 2021 }).Versions[0].Id,
                    ],
                    result
                );
            }

            [Fact]
            public async Task PublicationHasNoPublishedReleaseVersions_ReturnsEmpty()
            {
                Publication publication = _dataFixture
                    .DefaultPublication()
                    .WithReleases(
                        [_dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true)]
                    );

                var contextId = await AddTestData(publication);
                await using var contentDbContext = InMemoryContentDbContext(contextId);
                var repository = BuildRepository(contentDbContext);

                Assert.Empty(await ListLatestReleaseVersions(repository, publication.Id));
            }

            [Fact]
            public async Task PublicationHasNoReleaseVersions_ReturnsEmpty()
            {
                Publication publication = _dataFixture.DefaultPublication();

                var contextId = await AddTestData(publication);
                await using var contentDbContext = InMemoryContentDbContext(contextId);
                var repository = BuildRepository(contentDbContext);

                Assert.Empty(await ListLatestReleaseVersions(repository, publication.Id));
            }

            [Fact]
            public async Task PublicationDoesNotExist_ReturnsEmpty()
            {
                var repository = BuildRepository();

                Assert.Empty(
                    await ListLatestReleaseVersions(repository, publicationId: Guid.NewGuid())
                );
            }

            private static async Task<List<ReleaseVersion>> ListLatestReleaseVersions(
                ReleaseVersionRepository repository,
                Guid publicationId
            )
            {
                return await repository.ListLatestReleaseVersions(
                    publicationId,
                    publishedOnly: true
                );
            }
        }
    }

    public class ListLatestReleaseVersionIdsTests : ReleaseVersionRepositoryTests
    {
        public class AnyPublishedStateTests : ListLatestReleaseVersionIdsTests
        {
            [Fact]
            public async Task Success()
            {
                Publication publication = _dataFixture
                    .DefaultPublication()
                    .WithReleases(
                        [
                            _dataFixture.DefaultRelease(
                                publishedVersions: 2,
                                draftVersion: true,
                                year: 2022
                            ),
                            _dataFixture.DefaultRelease(
                                publishedVersions: 0,
                                draftVersion: true,
                                year: 2021
                            ),
                            _dataFixture.DefaultRelease(publishedVersions: 2, year: 2020),
                        ]
                    );

                var contextId = await AddTestData(publication);
                await using var contentDbContext = InMemoryContentDbContext(contextId);
                var repository = BuildRepository(contentDbContext);

                var result = await ListLatestReleaseVersionIds(repository, publication.Id);

                // Expect the latest version id's of each release
                AssertIdsAreEqualIgnoringOrder(
                    [
                        publication.Releases.Single(r => r is { Year: 2022 }).Versions[2].Id,
                        publication.Releases.Single(r => r is { Year: 2021 }).Versions[0].Id,
                        publication.Releases.Single(r => r is { Year: 2020 }).Versions[1].Id,
                    ],
                    result
                );
            }

            [Fact]
            public async Task MultiplePublications_ReturnsReleaseVersionsAssociatedWithPublication()
            {
                var (publication1, publication2) = _dataFixture
                    .DefaultPublication()
                    .WithReleases(_ =>
                        [
                            _dataFixture.DefaultRelease(
                                publishedVersions: 0,
                                draftVersion: true,
                                year: 2022
                            ),
                            _dataFixture.DefaultRelease(
                                publishedVersions: 0,
                                draftVersion: true,
                                year: 2021
                            ),
                        ]
                    )
                    .GenerateTuple2();

                var contextId = await AddTestData(publication1, publication2);
                await using var contentDbContext = InMemoryContentDbContext(contextId);
                var repository = BuildRepository(contentDbContext);

                var result = await ListLatestReleaseVersionIds(repository, publication1.Id);

                // Expect the results to be from the specified publication
                AssertIdsAreEqualIgnoringOrder(
                    [
                        publication1.Releases.Single(r => r is { Year: 2022 }).Versions[0].Id,
                        publication1.Releases.Single(r => r is { Year: 2021 }).Versions[0].Id,
                    ],
                    result
                );
            }

            [Fact]
            public async Task PublicationHasNoReleaseVersions_ReturnsEmpty()
            {
                Publication publication = _dataFixture.DefaultPublication();

                var contextId = await AddTestData(publication);
                await using var contentDbContext = InMemoryContentDbContext(contextId);
                var repository = BuildRepository(contentDbContext);

                Assert.Empty(await ListLatestReleaseVersionIds(repository, publication.Id));
            }

            [Fact]
            public async Task PublicationDoesNotExist_ReturnsEmpty()
            {
                var repository = BuildRepository();

                Assert.Empty(
                    await ListLatestReleaseVersionIds(repository, publicationId: Guid.NewGuid())
                );
            }

            private static async Task<List<Guid>> ListLatestReleaseVersionIds(
                ReleaseVersionRepository repository,
                Guid publicationId
            )
            {
                return await repository.ListLatestReleaseVersionIds(
                    publicationId,
                    publishedOnly: false
                );
            }
        }

        public class PublishedOnlyTests : ListLatestReleaseVersionIdsTests
        {
            [Fact]
            public async Task Success()
            {
                Publication publication = _dataFixture
                    .DefaultPublication()
                    .WithReleases(
                        [
                            _dataFixture.DefaultRelease(
                                publishedVersions: 2,
                                draftVersion: true,
                                year: 2022
                            ),
                            _dataFixture.DefaultRelease(
                                publishedVersions: 0,
                                draftVersion: true,
                                year: 2021
                            ),
                            _dataFixture.DefaultRelease(publishedVersions: 2, year: 2020),
                        ]
                    );

                var contextId = await AddTestData(publication);
                await using var contentDbContext = InMemoryContentDbContext(contextId);
                var repository = BuildRepository(contentDbContext);

                var result = await ListLatestReleaseVersionIds(repository, publication.Id);

                // Expect the latest published version id's of each release
                AssertIdsAreEqualIgnoringOrder(
                    [
                        publication.Releases.Single(r => r is { Year: 2022 }).Versions[1].Id,
                        publication.Releases.Single(r => r is { Year: 2020 }).Versions[1].Id,
                    ],
                    result
                );
            }

            [Fact]
            public async Task MultiplePublications_ReturnsReleaseVersionsAssociatedWithPublication()
            {
                var (publication1, publication2) = _dataFixture
                    .DefaultPublication()
                    .WithReleases(_ =>
                        [
                            _dataFixture.DefaultRelease(publishedVersions: 1, year: 2022),
                            _dataFixture.DefaultRelease(publishedVersions: 1, year: 2021),
                        ]
                    )
                    .GenerateTuple2();

                var contextId = await AddTestData(publication1, publication2);
                await using var contentDbContext = InMemoryContentDbContext(contextId);
                var repository = BuildRepository(contentDbContext);

                var result = await ListLatestReleaseVersionIds(repository, publication1.Id);

                // Expect the results to be from the specified publication
                AssertIdsAreEqualIgnoringOrder(
                    [
                        publication1.Releases.Single(r => r is { Year: 2022 }).Versions[0].Id,
                        publication1.Releases.Single(r => r is { Year: 2021 }).Versions[0].Id,
                    ],
                    result
                );
            }

            [Fact]
            public async Task PublicationHasNoPublishedReleaseVersions_ReturnsEmpty()
            {
                Publication publication = _dataFixture
                    .DefaultPublication()
                    .WithReleases(
                        [_dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true)]
                    );

                var contextId = await AddTestData(publication);
                await using var contentDbContext = InMemoryContentDbContext(contextId);
                var repository = BuildRepository(contentDbContext);

                Assert.Empty(await ListLatestReleaseVersionIds(repository, publication.Id));
            }

            [Fact]
            public async Task PublicationHasNoReleaseVersions_ReturnsEmpty()
            {
                Publication publication = _dataFixture.DefaultPublication();

                var contextId = await AddTestData(publication);
                await using var contentDbContext = InMemoryContentDbContext(contextId);
                var repository = BuildRepository(contentDbContext);

                Assert.Empty(await ListLatestReleaseVersionIds(repository, publication.Id));
            }

            [Fact]
            public async Task PublicationDoesNotExist_ReturnsEmpty()
            {
                var repository = BuildRepository();

                Assert.Empty(
                    await ListLatestReleaseVersionIds(repository, publicationId: Guid.NewGuid())
                );
            }

            private static async Task<List<Guid>> ListLatestReleaseVersionIds(
                ReleaseVersionRepository repository,
                Guid publicationId
            )
            {
                return await repository.ListLatestReleaseVersionIds(
                    publicationId,
                    publishedOnly: true
                );
            }
        }
    }

    private List<ReleaseSeriesItem> GenerateReleaseSeries(
        IReadOnlyList<Release> releases,
        params int[] years
    )
    {
        return years
            .Select(year =>
            {
                var release = releases.Single(r => r.Year == year);
                return _dataFixture.DefaultReleaseSeriesItem().WithReleaseId(release.Id).Generate();
            })
            .ToList();
    }

    private static void AssertIdsAreEqualIgnoringOrder(
        IReadOnlyCollection<Guid> expectedIds,
        IReadOnlyCollection<ReleaseVersion> actualReleaseVersions
    )
    {
        Assert.Equal(expectedIds.Count, actualReleaseVersions.Count);
        Assert.True(
            SequencesAreEqualIgnoringOrder(expectedIds, actualReleaseVersions.Select(rv => rv.Id))
        );
    }

    private static void AssertIdsAreEqualIgnoringOrder(
        IReadOnlyCollection<Guid> expectedIds,
        IReadOnlyCollection<Guid> actualIds
    )
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
        ContentDbContext? contentDbContext = null
    )
    {
        return new ReleaseVersionRepository(
            contentDbContext: contentDbContext ?? InMemoryContentDbContext()
        );
    }
}
