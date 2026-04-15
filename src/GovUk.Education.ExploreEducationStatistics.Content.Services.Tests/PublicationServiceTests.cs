using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests;

public abstract class PublicationServiceTests
{
    private readonly string _contentDbContextId = Guid.NewGuid().ToString();

    private readonly DataFixture _dataFixture = new();

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

    private static PublicationService SetupPublicationService(ContentDbContext? contentDbContext = null) =>
        new(contentDbContext ?? InMemoryContentDbContext());
}
