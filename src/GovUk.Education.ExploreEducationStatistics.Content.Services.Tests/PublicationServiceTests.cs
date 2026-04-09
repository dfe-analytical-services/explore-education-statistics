using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests;

public abstract class PublicationServiceTests
{
    private readonly string _contentDbContextId = Guid.NewGuid().ToString();

    private readonly DataFixture _dataFixture = new();

    public class GetTests : PublicationServiceTests
    {
        private readonly Contact _contact = new()
        {
            TeamName = "Team name",
            TeamEmail = "team@email.com",
            ContactName = "Contact name",
            ContactTelNo = "1234",
        };

        private readonly ExternalMethodology _externalMethodology = new()
        {
            Title = "External methodology title",
            Url = "https://external.methodology.com",
        };

        [Fact]
        public async Task Success()
        {
            ReleaseSeriesItem legacyLink = _dataFixture.DefaultLegacyReleaseSeriesItem();

            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases([
                    _dataFixture.DefaultRelease(publishedVersions: 2, draftVersion: true, year: 2022),
                    _dataFixture.DefaultRelease(publishedVersions: 1, year: 2020),
                    _dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true, year: 2021),
                ])
                .WithContact(_contact)
                .WithExternalMethodology(_externalMethodology)
                .WithLegacyLinks([legacyLink])
                .WithTheme(_dataFixture.DefaultTheme())
                .FinishWith(p =>
                {
                    // Adjust the generated LatestPublishedReleaseVersion to make 2020 the latest published release
                    var release2020Version0 = p.Releases.Single(r => r.Year == 2020).Versions[0];
                    p.LatestPublishedReleaseVersion = release2020Version0;
                    p.LatestPublishedReleaseVersionId = release2020Version0.Id;

                    // Apply a different release series order rather than using the default
                    p.ReleaseSeries = [.. GenerateReleaseSeries(p.Releases, 2021, 2020, 2022), legacyLink];
                });

            var release2020 = publication.Releases.Single(r => r.Year == 2020);
            var release2022 = publication.Releases.Single(r => r.Year == 2022);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupPublicationService(contentDbContext);

                var result = await service.Get(publication.Slug);

                var publicationViewModel = result.AssertRight();

                Assert.Equal(publication.Id, publicationViewModel.Id);
                Assert.Equal(publication.Title, publicationViewModel.Title);
                Assert.Equal(publication.Summary, publicationViewModel.Summary);
                Assert.Equal(publication.Slug, publicationViewModel.Slug);
                Assert.False(publicationViewModel.IsSuperseded);

                Assert.Equal(2, publicationViewModel.Releases.Count);

                Assert.Equal(release2020.Versions[0].Id, publicationViewModel.LatestReleaseId);

                Assert.Equal(release2020.Id, publicationViewModel.Releases[0].Id);
                Assert.Equal(release2020.Slug, publicationViewModel.Releases[0].Slug);
                Assert.Equal(release2020.Title, publicationViewModel.Releases[0].Title);

                Assert.Equal(release2022.Id, publicationViewModel.Releases[1].Id);
                Assert.Equal(release2022.Slug, publicationViewModel.Releases[1].Slug);
                Assert.Equal(release2022.Title, publicationViewModel.Releases[1].Title);

                var releaseSeries = publicationViewModel.ReleaseSeries;

                Assert.Equal(3, releaseSeries.Count);

                Assert.False(releaseSeries[0].IsLegacyLink);
                Assert.Equal(release2020.Id, releaseSeries[0].ReleaseId);
                Assert.Equal(release2020.Title, releaseSeries[0].Description);
                Assert.Equal(release2020.Slug, releaseSeries[0].ReleaseSlug);
                Assert.Null(releaseSeries[0].LegacyLinkUrl);

                // NOTE: 2021 release does exist in the database's publication.ReleaseSeries, but is filtered out
                // because it's unpublished

                Assert.False(releaseSeries[1].IsLegacyLink);
                Assert.Equal(release2022.Id, releaseSeries[1].ReleaseId);
                Assert.Equal(release2022.Title, releaseSeries[1].Description);
                Assert.Equal(release2022.Slug, releaseSeries[1].ReleaseSlug);
                Assert.Null(releaseSeries[1].LegacyLinkUrl);

                Assert.True(releaseSeries[2].IsLegacyLink);
                Assert.Null(releaseSeries[2].ReleaseId);
                Assert.Equal(legacyLink.LegacyLinkDescription, releaseSeries[2].Description);
                Assert.Null(releaseSeries[2].ReleaseSlug);
                Assert.Equal(legacyLink.LegacyLinkUrl, releaseSeries[2].LegacyLinkUrl);

                Assert.Equal(publication.Theme.Id, publicationViewModel.Theme.Id);
                Assert.Equal(publication.Theme.Slug, publicationViewModel.Theme.Slug);
                Assert.Equal(publication.Theme.Title, publicationViewModel.Theme.Title);
                Assert.Equal(publication.Theme.Summary, publicationViewModel.Theme.Summary);

                Assert.Equal(_contact.TeamName, publicationViewModel.Contact.TeamName);
                Assert.Equal(_contact.TeamEmail, publicationViewModel.Contact.TeamEmail);
                Assert.Equal(_contact.ContactName, publicationViewModel.Contact.ContactName);
                Assert.Equal(_contact.ContactTelNo, publicationViewModel.Contact.ContactTelNo);

                Assert.NotNull(publicationViewModel.ExternalMethodology);
                Assert.Equal(_externalMethodology.Title, publicationViewModel.ExternalMethodology!.Title);
                Assert.Equal(_externalMethodology.Url, publicationViewModel.ExternalMethodology.Url);
            }
        }

        [Fact]
        public async Task IsSuperseded_SupersedingPublicationHasPublishedRelease()
        {
            Publication supersedingPublication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture.DefaultRelease(publishedVersions: 1).Generate(1));

            Publication publication = _dataFixture
                .DefaultPublication()
                .WithSupersededBy(supersedingPublication)
                .WithReleases(_dataFixture.DefaultRelease(publishedVersions: 1).Generate(1))
                .WithContact(_contact)
                .WithExternalMethodology(_externalMethodology)
                .WithTheme(_dataFixture.DefaultTheme());

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupPublicationService(contentDbContext);

                var result = await service.Get(publication.Slug);

                var publicationViewModel = result.AssertRight();

                Assert.Equal(publication.Id, publicationViewModel.Id);
                Assert.True(publicationViewModel.IsSuperseded);

                Assert.NotNull(publicationViewModel.SupersededBy);
                Assert.Equal(supersedingPublication.Id, publicationViewModel.SupersededBy!.Id);
                Assert.Equal(supersedingPublication.Title, publicationViewModel.SupersededBy.Title);
                Assert.Equal(supersedingPublication.Slug, publicationViewModel.SupersededBy.Slug);
            }
        }

        [Fact]
        public async Task IsSuperseded_SupersedingPublicationHasNoPublishedRelease()
        {
            Publication supersedingPublication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true).Generate(1));

            Publication publication = _dataFixture
                .DefaultPublication()
                .WithSupersededBy(supersedingPublication)
                .WithReleases(_dataFixture.DefaultRelease(publishedVersions: 1).Generate(1))
                .WithContact(_contact)
                .WithExternalMethodology(_externalMethodology)
                .WithTheme(_dataFixture.DefaultTheme());

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupPublicationService(contentDbContext);

                var result = await service.Get(publication.Slug);

                var publicationViewModel = result.AssertRight();

                Assert.Equal(publication.Id, publicationViewModel.Id);
                Assert.False(publicationViewModel.IsSuperseded);
                Assert.Null(publicationViewModel.SupersededBy);
            }
        }

        [Fact]
        public async Task PublicationHasNoPublishedRelease_ReturnsNotFound()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true).Generate(1))
                .WithContact(_contact)
                .WithExternalMethodology(_externalMethodology)
                .WithTheme(_dataFixture.DefaultTheme());

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupPublicationService(contentDbContext);

                var result = await service.Get(publication.Slug);

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task NoPublication_ReturnsNotFound()
        {
            var service = SetupPublicationService();

            var result = await service.Get("nonexistent-publication");

            result.AssertNotFound();
        }
    }

    public class ListPublicationInfosTests : PublicationServiceTests
    {
        [Fact]
        public async Task GivenPublishedPublications_WhenListPublicationInfos_ThenReturnsPublicationInfos()
        {
            // Arrange
            var publishedPublications = Enumerable.Range(1, 3).Select(_ => GeneratePublishedPublication()).ToArray();

            var themes = new List<Theme>
            {
                GenerateTheme(publishedPublications.Take(2)),
                GenerateTheme(publishedPublications.Skip(2)),
            };

            await AddToDatabase(themes);

            // Act
            var results = await ListPublicationInfos();

            // Assert
            AssertPublicationInfosAsExpected(publishedPublications, results);
        }

        [Fact]
        public async Task GivenPublishedAndUnpublishedPublications_WhenListPublicationInfos_ThenReturnsOnlyPublishedPublicationInfos()
        {
            // Arrange
            var publishedPublications = Enumerable.Range(1, 3).Select(_ => GeneratePublishedPublication()).ToArray();

            var unpublishedPublications = Enumerable
                .Range(1, 3)
                .Select(_ => GenerateUnpublishedPublication())
                .ToArray();

            var themes = new List<Theme>
            {
                GenerateTheme(publishedPublications.Take(2), unpublishedPublications.Take(1)),
                GenerateTheme(publishedPublications.Skip(2), unpublishedPublications.Skip(1)),
            };

            await AddToDatabase(themes);

            // Act
            var results = await ListPublicationInfos();

            // Assert
            AssertPublicationInfosAsExpected(publishedPublications, results);
        }

        [Fact]
        public async Task GivenPublishedPublicationsSomeOfWhichAreSuperseded_WhenListPublicationInfos_ThenReturnsOnlyUnsupersededPublicationInfos()
        {
            // Arrange
            var publishedPublications = Enumerable.Range(1, 3).Select(_ => GeneratePublishedPublication()).ToArray();

            var publishedPublicationsWithUnpublishedSuperseded = Enumerable
                .Range(1, 4)
                .Select(_ => GeneratePublicationWithUnpublishedSuperseded())
                .ToArray();

            var supersededPublications = Enumerable
                .Range(1, 5)
                .Select(_ => GeneratePublicationWithPublishedSuperseded())
                .ToArray();

            var themes = new List<Theme>
            {
                GenerateTheme(
                    publishedPublications.Take(1),
                    supersededPublications.Take(1),
                    publishedPublicationsWithUnpublishedSuperseded.Take(2)
                ),
                GenerateTheme(
                    publishedPublications.Skip(1),
                    supersededPublications.Skip(1),
                    publishedPublicationsWithUnpublishedSuperseded.Skip(2)
                ),
            };

            await AddToDatabase(themes);

            // Act
            var results = await ListPublicationInfos();

            // Assert
            var expectedPublications = publishedPublications
                .Concat(publishedPublicationsWithUnpublishedSuperseded)
                .Concat(supersededPublications.Select(p => p.SupersededBy).OfType<Publication>())
                .ToArray();

            AssertPublicationInfosAsExpected(expectedPublications, results);
        }

        [Fact]
        public async Task GivenPublishedPublicationsSomeOfWhichAreSupersededByEachOther_WhenListPublicationInfos_ThenReturnsOnlyUnsupersededPublicationInfosOnce()
        {
            // Arrange
            var publishedPublications = Enumerable.Range(1, 3).Select(_ => GeneratePublishedPublication()).ToArray();

            var supersededPublications = Enumerable
                .Range(1, 3)
                .Select(i => GeneratePublicationWithPublishedSuperseded(publishedPublications[i - 1]))
                .ToArray();

            var themes = new List<Theme>
            {
                GenerateTheme(publishedPublications.Take(1), supersededPublications.Take(1)),
                GenerateTheme(publishedPublications.Skip(1), supersededPublications.Skip(1)),
            };

            await AddToDatabase(themes);

            // Act
            var results = await ListPublicationInfos();

            // Assert
            var expectedPublications = publishedPublications.ToArray();
            AssertPublicationInfosAsExpected(expectedPublications, results);
        }

        [Fact]
        public async Task GivenAVarietyOfPublications_WhenListPublicationInfosByThemeIdIsCalled_ThenReturnsPublishedPublicationInfosInTheme()
        {
            // Arrange
            var publishedPublications = Enumerable.Range(1, 6).Select(_ => GeneratePublishedPublication()).ToArray();

            var unpublishedPublications = Enumerable
                .Range(1, 6)
                .Select(_ => GenerateUnpublishedPublication())
                .ToArray();

            var publishedPublicationsWithUnpublishedSuperseded = Enumerable
                .Range(1, 6)
                .Select(_ => GeneratePublicationWithUnpublishedSuperseded())
                .ToArray();

            var supersededPublications = Enumerable
                .Range(1, 6)
                .Select(_ => GeneratePublicationWithPublishedSuperseded())
                .ToArray();

            var allPublicationsSplitInto3Groups = publishedPublications
                .Concat(unpublishedPublications)
                .Concat(publishedPublicationsWithUnpublishedSuperseded)
                .Concat(supersededPublications)
                .Concat(supersededPublications.Select(p => p.SupersededBy).OfType<Publication>())
                .ToArray()
                .DistributeIntoGroups(3); // Distribute all of the different types of publications into 3 groups

            var themes = new List<Theme>
            {
                GenerateTheme(allPublicationsSplitInto3Groups[0].Shuffle()),
                GenerateTheme(allPublicationsSplitInto3Groups[1].Shuffle()),
                GenerateTheme(allPublicationsSplitInto3Groups[2].Shuffle()),
            };

            await AddToDatabase(themes);

            // Act
            IList<PublicationInfoViewModel>[] resultsByTheme =
            [
                await ListPublicationInfos(themes[0].Id),
                await ListPublicationInfos(themes[1].Id),
                await ListPublicationInfos(themes[2].Id),
            ];

            // Assert
            var expectedPublications = publishedPublications
                .Concat(publishedPublicationsWithUnpublishedSuperseded)
                .Concat(supersededPublications.Select(p => p.SupersededBy).OfType<Publication>())
                .ToArray();

            IList<Publication>[] expectedPublicationsByTheme =
            [
                expectedPublications.Intersect(allPublicationsSplitInto3Groups[0]).ToList(),
                expectedPublications.Intersect(allPublicationsSplitInto3Groups[1]).ToList(),
                expectedPublications.Intersect(allPublicationsSplitInto3Groups[2]).ToList(),
            ];

            Assert.All(
                [0, 1, 2],
                i =>
                {
                    AssertPublicationInfosAsExpected(expectedPublicationsByTheme[i], resultsByTheme[i]);
                }
            );
        }

        private void AssertPublicationInfosAsExpected(
            IList<Publication> expectedPublications,
            IList<PublicationInfoViewModel> actualPublicationInfoViewModels
        )
        {
            Assert.Equal(expectedPublications.Count, actualPublicationInfoViewModels.Count);
            Assert.All(
                expectedPublications
                    .OrderBy(p => p.Id)
                    .Zip(actualPublicationInfoViewModels.OrderBy(p => p.PublicationId)),
                x =>
                {
                    var (expected, actual) = x;
                    Assert.Equal(expected.Id, actual.PublicationId);
                    Assert.Equal(expected.Slug, actual.PublicationSlug);
                    Assert.NotNull(actual.LatestPublishedRelease);
                    Assert.Equal(
                        expected.LatestPublishedReleaseVersion?.Release.Slug,
                        actual.LatestPublishedRelease.ReleaseSlug
                    );
                    Assert.Equal(
                        expected.LatestPublishedReleaseVersion?.Release.Id,
                        actual.LatestPublishedRelease.ReleaseId
                    );
                }
            );
        }

        private Theme GenerateTheme(params IEnumerable<Publication>[] publications) =>
            _dataFixture.DefaultTheme().WithPublications(publications.SelectMany(p => p)).Generate();

        private Publication GeneratePublishedPublication() =>
            _dataFixture
                .DefaultPublication()
                .WithLatestPublishedReleaseVersion(
                    _dataFixture.DefaultReleaseVersion().WithRelease(_dataFixture.DefaultRelease())
                )
                .Generate();

        private Publication GenerateUnpublishedPublication() => _dataFixture.DefaultPublication().Generate();

        private Publication GeneratePublicationWithPublishedSuperseded(Publication? supersededBy = null) =>
            _dataFixture
                .DefaultPublication()
                .WithSupersededBy(supersededBy ?? GeneratePublishedPublication())
                .WithLatestPublishedReleaseVersion(
                    _dataFixture.DefaultReleaseVersion().WithRelease(_dataFixture.DefaultRelease())
                )
                .Generate();

        private Publication GeneratePublicationWithUnpublishedSuperseded() =>
            _dataFixture
                .DefaultPublication()
                .WithSupersededBy(GenerateUnpublishedPublication())
                .WithLatestPublishedReleaseVersion(
                    _dataFixture.DefaultReleaseVersion().WithRelease(_dataFixture.DefaultRelease())
                )
                .Generate();

        private async Task AddToDatabase(IEnumerable<Theme> themes)
        {
            await using var contentDbContext = InMemoryContentDbContext(_contentDbContextId);
            await contentDbContext.Themes.AddRangeAsync(themes);
            await contentDbContext.SaveChangesAsync();
        }

        private async Task<IList<PublicationInfoViewModel>> ListPublicationInfos(Guid? themeId = null)
        {
            await using var contentDbContext = InMemoryContentDbContext(_contentDbContextId);
            var service = SetupPublicationService(contentDbContext);
            return await service.ListPublicationInfos(themeId);
        }
    }

    private List<ReleaseSeriesItem> GenerateReleaseSeries(IReadOnlyList<Release> releases, params int[] years)
    {
        return years
            .Select(year =>
            {
                var release = releases.Single(r => r.Year == year);
                return _dataFixture.DefaultReleaseSeriesItem().WithReleaseId(release.Id).Generate();
            })
            .ToList();
    }

    private static PublicationService SetupPublicationService(
        ContentDbContext? contentDbContext = null,
        IPublicationRepository? publicationRepository = null,
        IReleaseRepository? releaseRepository = null
    )
    {
        contentDbContext ??= InMemoryContentDbContext();

        return new PublicationService(
            contentDbContext,
            new PersistenceHelper<ContentDbContext>(contentDbContext),
            publicationRepository ?? new PublicationRepository(contentDbContext),
            releaseRepository ?? new ReleaseRepository(contentDbContext)
        );
    }
}
