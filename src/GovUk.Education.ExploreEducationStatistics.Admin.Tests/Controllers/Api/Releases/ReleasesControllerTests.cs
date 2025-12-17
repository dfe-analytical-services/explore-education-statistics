#nullable enable
using System.Net.Http.Json;
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Releases;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture.Optimised;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.WebApp;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.EntityFrameworkCore;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;
using ReleaseViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ReleaseViewModel;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Releases;

public class ReleasesControllerUnitTests
{
    [Fact]
    public async Task Create_Release_Returns_Ok()
    {
        var returnedViewModel = new ReleaseVersionViewModel();

        var releaseService = new Mock<IReleaseService>(Strict);

        releaseService.Setup(s => s.CreateRelease(It.IsAny<ReleaseCreateRequest>())).ReturnsAsync(returnedViewModel);

        var controller = BuildController(releaseService.Object);

        var result = await controller.CreateRelease(new ReleaseCreateRequest());
        VerifyAllMocks(releaseService);

        result.AssertOkResult(returnedViewModel);
    }

    private static ReleasesController BuildController(IReleaseService? releaseService = null)
    {
        return new ReleasesController(releaseService ?? Mock.Of<IReleaseService>(Strict));
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class ReleasesControllerIntegrationTestsFixture()
    : OptimisedAdminCollectionFixture(
        capabilities: [AdminIntegrationTestCapability.UserAuth, AdminIntegrationTestCapability.Azurite]
    )
{
    public IPublicBlobCacheService PublicBlobCacheService = null!;
    public IPublisherTableStorageService PublisherTableStorageService = null!;

    protected override async Task AfterFactoryConstructed(OptimisedServiceCollectionLookups<Startup> lookups)
    {
        await base.AfterFactoryConstructed(lookups);
        PublicBlobCacheService = lookups.GetService<IPublicBlobCacheService>();
        PublisherTableStorageService = lookups.GetService<IPublisherTableStorageService>();
    }
}

[CollectionDefinition(nameof(ReleasesControllerIntegrationTestsFixture))]
public class ReleasesControllerIntegrationTestsCollection
    : ICollectionFixture<ReleasesControllerIntegrationTestsFixture>;

[Collection(nameof(ReleasesControllerIntegrationTestsFixture))]
public abstract class ReleasesControllerIntegrationTests
{
    private static readonly DataFixture DataFixture = new();

    public class CreateReleaseTests(ReleasesControllerIntegrationTestsFixture fixture)
        : ReleasesControllerIntegrationTests
    {
        [Theory]
        [InlineData(2020, TimeIdentifier.AcademicYear, "initial", "initial", "2020-21-initial")]
        [InlineData(2020, TimeIdentifier.AcademicYear, "Initial", "Initial", "2020-21-initial")]
        [InlineData(2020, TimeIdentifier.AcademicYear, " initial", "initial", "2020-21-initial")]
        [InlineData(2020, TimeIdentifier.AcademicYear, "initial ", "initial", "2020-21-initial")]
        [InlineData(2020, TimeIdentifier.AcademicYear, "initial 2", "initial 2", "2020-21-initial-2")]
        [InlineData(2020, TimeIdentifier.AcademicYear, "initial  2", "initial 2", "2020-21-initial-2")]
        [InlineData(2020, TimeIdentifier.AcademicYear, "", null, "2020-21")]
        [InlineData(2020, TimeIdentifier.AcademicYear, " ", null, "2020-21")]
        [InlineData(2020, TimeIdentifier.AcademicYear, "  ", null, "2020-21")]
        [InlineData(2020, TimeIdentifier.AcademicYear, null, null, "2020-21")]
        public async Task Success(
            int year,
            TimeIdentifier timePeriodCoverage,
            string? label,
            string? expectedLabel,
            string expectedSlug
        )
        {
            Publication publication = DataFixture.DefaultPublication();

            await fixture.GetContentDbContext().AddTestData(context => context.Publications.Add(publication));

            var response = await CreateRelease(
                publicationId: publication.Id,
                year: year,
                timePeriodCoverage: timePeriodCoverage,
                label: label
            );

            var viewModel = response.AssertOk<ReleaseVersionViewModel>();

            var updatedPublication = fixture
                .GetContentDbContext()
                .Publications.Include(p => p.Releases)
                    .ThenInclude(r => r.Versions)
                .Single(p => p.Id == publication.Id);

            Assert.Equal(publication.Id, viewModel.PublicationId);
            Assert.Equal(year, viewModel.Year);
            Assert.Equal(timePeriodCoverage, viewModel.TimePeriodCoverage);
            Assert.Equal(expectedSlug, viewModel.Slug);

            var release = Assert.Single(updatedPublication.Releases);
            Assert.Equal(publication.Id, release.PublicationId);
            Assert.Equal(year, release.Year);
            Assert.Equal(timePeriodCoverage, release.TimePeriodCoverage);
            Assert.Equal(expectedLabel, release.Label);
            Assert.Equal(expectedSlug, release.Slug);

            var releaseVersion = Assert.Single(release.Versions);
            Assert.Equal(publication.Id, releaseVersion.PublicationId);
            Assert.Equal(release.Id, releaseVersion.ReleaseId);

            var releaseSeriesItem = Assert.Single(updatedPublication.ReleaseSeries);
            Assert.Equal(release.Id, releaseSeriesItem.ReleaseId);
        }

        [Fact]
        public async Task PublicationNotFound()
        {
            var response = await CreateRelease(
                publicationId: Guid.NewGuid(),
                year: 2020,
                timePeriodCoverage: TimeIdentifier.AcademicYear,
                label: null
            );

            response.AssertNotFound();
        }

        [Fact]
        public async Task UserDoesNotHavePermission()
        {
            Publication publication = DataFixture.DefaultPublication();

            await fixture.GetContentDbContext().AddTestData(context => context.Publications.Add(publication));

            var response = await CreateRelease(
                publicationId: publication.Id,
                year: 2020,
                timePeriodCoverage: TimeIdentifier.AcademicYear,
                label: null,
                user: OptimisedTestUsers.Authenticated
            );

            response.AssertForbidden();
        }

        [Fact]
        public async Task ReleaseTypeInvalid()
        {
            Publication publication = DataFixture.DefaultPublication();

            await fixture.GetContentDbContext().AddTestData(context => context.Publications.Add(publication));

            var response = await CreateRelease(
                publicationId: publication.Id,
                year: 2020,
                timePeriodCoverage: TimeIdentifier.AcademicYear,
                label: null,
                type: ReleaseType.ExperimentalStatistics
            );

            var validationProblem = response.AssertValidationProblem();

            var error = Assert.Single(validationProblem.Errors);

            Assert.Equal(ValidationErrorMessages.ReleaseTypeInvalid.ToString(), error.Code);
        }

        [Theory]
        [InlineData(2020, TimeIdentifier.AcademicYear, "initial", "2020-21-initial")]
        [InlineData(2020, TimeIdentifier.AcademicYear, "Initial", "2020-21-initial")]
        [InlineData(2020, TimeIdentifier.AcademicYear, " initial ", "2020-21-initial")]
        [InlineData(2020, TimeIdentifier.AcademicYear, null, "2020-21")]
        [InlineData(2020, TimeIdentifier.AcademicYear, "", "2020-21")]
        [InlineData(2020, TimeIdentifier.AcademicYear, " ", "2020-21")]
        [InlineData(2020, TimeIdentifier.AcademicYear, "  ", "2020-21")]
        [InlineData(2020, TimeIdentifier.AcademicYearQ1, "initial", "2020-21-q1-initial")]
        public async Task SlugNotUniqueToPublication(
            int year,
            TimeIdentifier timePeriodCoverage,
            string? label,
            string existingReleaseSlug
        )
        {
            Publication publication = DataFixture
                .DefaultPublication()
                .WithReleases([DataFixture.DefaultRelease(publishedVersions: 1).WithSlug(existingReleaseSlug)]);

            await fixture.GetContentDbContext().AddTestData(context => context.Publications.Add(publication));

            var response = await CreateRelease(
                publicationId: publication.Id,
                year: year,
                timePeriodCoverage: timePeriodCoverage,
                label: label
            );

            var validationProblem = response.AssertValidationProblem();

            var error = Assert.Single(validationProblem.Errors);

            Assert.Equal(SlugNotUnique.ToString(), error.Code);
        }

        [Fact]
        public async Task ReleaseRedirectExistsForSlugForDifferentReleaseInSamePublication()
        {
            Publication publication = DataFixture
                .DefaultPublication()
                .WithReleases([
                    DataFixture
                        .DefaultRelease(publishedVersions: 1)
                        .WithYear(2020)
                        .WithTimePeriodCoverage(TimeIdentifier.AcademicYear)
                        .WithLabel("intermediate")
                        .WithSlug("2020-21-intermediate")
                        .WithRedirects([DataFixture.DefaultReleaseRedirect().WithSlug("2020-21-final")]),
                ]);

            await fixture.GetContentDbContext().AddTestData(context => context.Publications.Add(publication));

            var response = await CreateRelease(
                publicationId: publication.Id,
                year: 2020,
                timePeriodCoverage: TimeIdentifier.AcademicYear,
                label: "final"
            );

            var validationProblem = response.AssertValidationProblem();

            var error = Assert.Single(validationProblem.Errors);

            Assert.Equal(ReleaseSlugUsedByRedirect.ToString(), error.Code);
        }

        [Fact]
        public async Task ReleaseRedirectExistsForSlugForReleaseInDifferentPublication()
        {
            Publication otherPublication = DataFixture
                .DefaultPublication()
                .WithReleases([
                    DataFixture
                        .DefaultRelease(publishedVersions: 1)
                        .WithYear(2020)
                        .WithTimePeriodCoverage(TimeIdentifier.AcademicYear)
                        .WithLabel("intermediate")
                        .WithSlug("2020-21-intermediate")
                        .WithRedirects([DataFixture.DefaultReleaseRedirect().WithSlug("2020-21-final")]),
                ]);

            Publication targetPublication = DataFixture.DefaultPublication();

            await fixture
                .GetContentDbContext()
                .AddTestData(context => context.Publications.AddRange(otherPublication, targetPublication));

            var response = await CreateRelease(
                publicationId: targetPublication.Id,
                year: 2020,
                timePeriodCoverage: TimeIdentifier.AcademicYear,
                label: "final"
            );

            response.AssertOk();
        }

        [Fact]
        public async Task LabelOver20Characters()
        {
            Publication publication = DataFixture.DefaultPublication();

            await fixture.GetContentDbContext().AddTestData(context => context.Publications.Add(publication));

            var response = await CreateRelease(
                publicationId: publication.Id,
                year: 2020,
                timePeriodCoverage: TimeIdentifier.AcademicYear,
                label: new string('a', 21)
            );

            var validationProblem = response.AssertValidationProblem();

            var error = Assert.Single(validationProblem.Errors);

            Assert.Equal(
                $"The field {nameof(ReleaseCreateRequest.Label)} must be a string or array type with a maximum length of '20'.",
                error.Message
            );
            Assert.Equal(nameof(ReleaseCreateRequest.Label), error.Path);
        }

        private async Task<HttpResponseMessage> CreateRelease(
            Guid publicationId,
            int year,
            TimeIdentifier timePeriodCoverage,
            string? label = null,
            ReleaseType? type = ReleaseType.OfficialStatistics,
            ClaimsPrincipal? user = null
        )
        {
            var client = fixture.CreateClient(user: user ?? OptimisedTestUsers.Bau);

            var request = new
            {
                PublicationId = publicationId,
                Type = type,
                Year = year,
                TimePeriodCoverage = new { Value = timePeriodCoverage.GetEnumValue() },
                Label = label,
            };

            return await client.PostAsJsonAsync("api/releases", request);
        }
    }

    public class UpdateReleaseTests(ReleasesControllerIntegrationTestsFixture fixture)
        : ReleasesControllerIntegrationTests
    {
        [Theory]
        [InlineData(
            2020,
            TimeIdentifier.AcademicYear,
            "initial",
            "initial",
            "2020-21-initial",
            "Academic year 2020/21 initial"
        )]
        [InlineData(
            2020,
            TimeIdentifier.AcademicYear,
            "Initial",
            "Initial",
            "2020-21-initial",
            "Academic year 2020/21 Initial"
        )]
        [InlineData(
            2020,
            TimeIdentifier.AcademicYear,
            " initial",
            "initial",
            "2020-21-initial",
            "Academic year 2020/21 initial"
        )]
        [InlineData(
            2020,
            TimeIdentifier.AcademicYear,
            "initial ",
            "initial",
            "2020-21-initial",
            "Academic year 2020/21 initial"
        )]
        [InlineData(
            2020,
            TimeIdentifier.AcademicYear,
            "initial 2",
            "initial 2",
            "2020-21-initial-2",
            "Academic year 2020/21 initial 2"
        )]
        [InlineData(
            2020,
            TimeIdentifier.AcademicYear,
            "initial  2",
            "initial 2",
            "2020-21-initial-2",
            "Academic year 2020/21 initial 2"
        )]
        [InlineData(2020, TimeIdentifier.AcademicYear, "", null, "2020-21", "Academic year 2020/21")]
        [InlineData(2020, TimeIdentifier.AcademicYear, " ", null, "2020-21", "Academic year 2020/21")]
        [InlineData(2020, TimeIdentifier.AcademicYear, "  ", null, "2020-21", "Academic year 2020/21")]
        [InlineData(2020, TimeIdentifier.AcademicYear, null, null, "2020-21", "Academic year 2020/21")]
        [InlineData(
            2020,
            TimeIdentifier.AcademicYearQ1,
            "initial",
            "initial",
            "2020-21-q1-initial",
            "Academic year Q1 2020/21 initial"
        )]
        public async Task Success(
            int year,
            TimeIdentifier timePeriodCoverage,
            string? label,
            string? expectedLabel,
            string expectedSlug,
            string expectedTitle
        )
        {
            Release release = DataFixture
                .DefaultRelease(publishedVersions: 1)
                .WithYear(year)
                .WithTimePeriodCoverage(timePeriodCoverage)
                .WithPublication(DataFixture.DefaultPublication());

            await fixture.GetContentDbContext().AddTestData(context => context.Releases.Add(release));

            var response = await UpdateRelease(releaseId: release.Id, label: label);

            var viewModel = response.AssertOk<ReleaseViewModel>();

            Assert.Equal(release.PublicationId, viewModel.PublicationId);
            Assert.Equal(expectedSlug, viewModel.Slug);
            Assert.Equal(timePeriodCoverage, viewModel.TimePeriodCoverage);
            Assert.Equal(year, viewModel.Year);
            Assert.Equal(expectedLabel, viewModel.Label);
            Assert.Equal(expectedTitle, viewModel.Title);

            var updatedRelease = await fixture
                .GetContentDbContext()
                .Releases.Include(r => r.Publication)
                .SingleAsync(r => r.Id == release.Id);

            Assert.Equal(expectedLabel, updatedRelease.Label);
            Assert.Equal(expectedSlug, updatedRelease.Slug);
        }

        [Fact]
        public async Task SlugChanged_OldCacheIsRemovedAndUpdatedForLiveRelease()
        {
            var oldSlug = "2020-21-initial";

            Publication publication = DataFixture.DefaultPublication().WithTheme(DataFixture.DefaultTheme());

            Release oldRelease = DataFixture
                .DefaultRelease(publishedVersions: 2, draftVersion: true)
                .WithYear(2020)
                .WithTimePeriodCoverage(TimeIdentifier.AcademicYear)
                .WithLabel("initial")
                .WithSlug(oldSlug)
                .WithPublication(publication);

            await fixture.GetContentDbContext().AddTestData(context => context.Releases.Add(oldRelease));

            var latestPublishedReleaseVersion = oldRelease.Versions[1];

            var oldReleaseCachedViewModel = new ReleaseCacheViewModel(latestPublishedReleaseVersion.Id);
            var oldReleaseCacheKey = new ReleaseCacheKey(
                publicationSlug: publication.Slug,
                releaseSlug: oldRelease.Slug
            );

            var oldLatestReleaseCachedViewModel = new ReleaseCacheViewModel(latestPublishedReleaseVersion.Id);
            var oldLatestReleaseCacheKey = new ReleaseCacheKey(publicationSlug: publication.Slug);

            var oldPublicationCachedViewModel = new PublicationCacheViewModel();
            var oldPublicationCacheKey = new PublicationCacheKey(publication.Slug);

            var oldReleaseParentPathTestDataCachedViewModel1 = new TestReleaseParentPathDataViewModel();
            var oldReleaseParentPathTestDataCacheKey1 = new TestReleaseParentPathDataCacheKey(
                PublicationSlug: publication.Slug,
                ReleaseSlug: oldRelease.Slug,
                FileParentPath: "test-folder-1"
            );

            var oldReleaseParentPathTestDataCachedViewModel2 = new TestReleaseParentPathDataViewModel();
            var oldReleaseParentPathTestDataCacheKey2 = new TestReleaseParentPathDataCacheKey(
                PublicationSlug: publication.Slug,
                ReleaseSlug: oldRelease.Slug,
                FileParentPath: "test-folder-2"
            );

            var publicBlobCacheService = fixture.PublicBlobCacheService;

            // This represents the cache stored in the release-specific directory
            await publicBlobCacheService.SetItemAsync(oldReleaseCacheKey, oldReleaseCachedViewModel);
            // This represents the cache stored in the 'latest-release.json' path
            await publicBlobCacheService.SetItemAsync(oldLatestReleaseCacheKey, oldLatestReleaseCachedViewModel);
            // This represents the publication cache
            await publicBlobCacheService.SetItemAsync(oldPublicationCacheKey, oldPublicationCachedViewModel);
            // This represents the release parent path cache folder, and some test data cached within it (in nested folders)
            await publicBlobCacheService.SetItemAsync(
                oldReleaseParentPathTestDataCacheKey1,
                oldReleaseParentPathTestDataCachedViewModel1
            );
            await publicBlobCacheService.SetItemAsync(
                oldReleaseParentPathTestDataCacheKey2,
                oldReleaseParentPathTestDataCachedViewModel2
            );

            // Testing that these pieces of cache have actually been stored
            Assert.NotNull(
                await publicBlobCacheService.GetItemAsync(oldReleaseCacheKey, typeof(ReleaseCacheViewModel))
            );
            Assert.NotNull(
                await publicBlobCacheService.GetItemAsync(oldLatestReleaseCacheKey, typeof(ReleaseCacheViewModel))
            );
            Assert.NotNull(
                await publicBlobCacheService.GetItemAsync(oldPublicationCacheKey, typeof(PublicationCacheViewModel))
            );
            Assert.NotNull(
                await publicBlobCacheService.GetItemAsync(
                    oldReleaseParentPathTestDataCacheKey1,
                    typeof(TestReleaseParentPathDataViewModel)
                )
            );
            Assert.NotNull(
                await publicBlobCacheService.GetItemAsync(
                    oldReleaseParentPathTestDataCacheKey2,
                    typeof(TestReleaseParentPathDataViewModel)
                )
            );

            var newLabel = "final";
            var response = await UpdateRelease(releaseId: oldRelease.Id, label: newLabel);

            response.AssertOk<ReleaseViewModel>();

            var updatedRelease = await fixture
                .GetContentDbContext()
                .Releases.Include(r => r.Publication)
                .SingleAsync(r => r.Id == oldRelease.Id);

            var oldSlugCachedValue = await publicBlobCacheService.GetItemAsync(
                oldReleaseCacheKey,
                typeof(ReleaseCacheViewModel)
            );

            var newSlugReleaseCacheKey = new ReleaseCacheKey(
                publicationSlug: publication.Slug,
                releaseSlug: updatedRelease.Slug
            );
            var newSlugCachedValue =
                await publicBlobCacheService.GetItemAsync(newSlugReleaseCacheKey, typeof(ReleaseCacheViewModel))
                as ReleaseCacheViewModel;

            var newLatestReleaseCachedValue =
                await publicBlobCacheService.GetItemAsync(oldLatestReleaseCacheKey, typeof(ReleaseCacheViewModel))
                as ReleaseCacheViewModel;

            var newPublicationCachedValue =
                await publicBlobCacheService.GetItemAsync(oldPublicationCacheKey, typeof(PublicationCacheViewModel))
                as PublicationCacheViewModel;

            var oldReleaseParentPathTestDataCachedValue1 = await publicBlobCacheService.GetItemAsync(
                oldReleaseParentPathTestDataCacheKey1,
                typeof(TestReleaseParentPathDataViewModel)
            );
            var oldReleaseParentPathTestDataCachedValue2 = await publicBlobCacheService.GetItemAsync(
                oldReleaseParentPathTestDataCacheKey2,
                typeof(TestReleaseParentPathDataViewModel)
            );

            // Checking that the Release Label and Slug are UPDATED
            Assert.Equal(newLabel, updatedRelease.Label);
            Assert.Equal($"2020-21-{newLabel}", updatedRelease.Slug);

            // Checking that the cache for the old release slug has been deleted
            Assert.Null(oldSlugCachedValue);

            // Checking that the release-specific cache for the new release slug has been created, and is not the same as the old cache.
            // Also checking that the cache is for the LATEST release version via it's ID.
            Assert.NotNull(newSlugCachedValue);
            Assert.NotEqual(oldReleaseCachedViewModel, newSlugCachedValue);
            Assert.Equal(latestPublishedReleaseVersion.Id, newSlugCachedValue.Id);
            Assert.Equal(updatedRelease.Slug, newSlugCachedValue.Slug);

            // Checking that the latest release cache has been updated, and is not the same as the old cache.
            // Also checking that the cache is for the LATEST release version via it's ID.
            Assert.NotNull(newLatestReleaseCachedValue);
            Assert.NotEqual(oldLatestReleaseCachedViewModel, newLatestReleaseCachedValue);
            Assert.Equal(latestPublishedReleaseVersion.Id, newLatestReleaseCachedValue.Id);
            Assert.Equal(updatedRelease.Slug, newLatestReleaseCachedValue.Slug);

            // Checking that the publication cache has been updated, and is not the same as the old cache.
            Assert.NotNull(newPublicationCachedValue);
            Assert.NotEqual(oldPublicationCachedViewModel, newPublicationCachedValue);
            Assert.All(newPublicationCachedValue.Releases, r => Assert.Equal(updatedRelease.Slug, r.Slug));
            Assert.All(
                newPublicationCachedValue.ReleaseSeries,
                rs => Assert.Equal(updatedRelease.Slug, rs.ReleaseSlug)
            );

            // Checking that all of the cache within the release parent path has been deleted
            Assert.Null(oldReleaseParentPathTestDataCachedValue1);
            Assert.Null(oldReleaseParentPathTestDataCachedValue2);
        }

        [Fact]
        public async Task SlugUnchanged_CacheIsUpdatedOnSameBlobPathsForLiveRelease()
        {
            var oldLabel = "initial";
            var oldSlug = $"2020-21-{oldLabel}";

            Publication publication = DataFixture.DefaultPublication().WithTheme(DataFixture.DefaultTheme());

            Release oldRelease = DataFixture
                .DefaultRelease(publishedVersions: 2, draftVersion: true)
                .WithYear(2020)
                .WithTimePeriodCoverage(TimeIdentifier.AcademicYear)
                .WithLabel(oldLabel)
                .WithSlug(oldSlug)
                .WithPublication(publication);

            await fixture.GetContentDbContext().AddTestData(context => context.Releases.Add(oldRelease));

            var latestPublishedReleaseVersion = oldRelease.Versions[1];

            var oldReleaseCachedViewModel = new ReleaseCacheViewModel(latestPublishedReleaseVersion.Id);
            var oldReleaseCacheKey = new ReleaseCacheKey(
                publicationSlug: publication.Slug,
                releaseSlug: oldRelease.Slug
            );

            var oldLatestReleaseCachedViewModel = new ReleaseCacheViewModel(latestPublishedReleaseVersion.Id);
            var oldLatestReleaseCacheKey = new ReleaseCacheKey(publicationSlug: publication.Slug);

            var oldPublicationCachedViewModel = new PublicationCacheViewModel();
            var oldPublicationCacheKey = new PublicationCacheKey(publication.Slug);

            var oldReleaseParentPathTestDataCachedViewModel1 = new TestReleaseParentPathDataViewModel();
            var oldReleaseParentPathTestDataCacheKey1 = new TestReleaseParentPathDataCacheKey(
                PublicationSlug: publication.Slug,
                ReleaseSlug: oldRelease.Slug,
                FileParentPath: "test-folder-1"
            );

            var oldReleaseParentPathTestDataCachedViewModel2 = new TestReleaseParentPathDataViewModel();
            var oldReleaseParentPathTestDataCacheKey2 = new TestReleaseParentPathDataCacheKey(
                PublicationSlug: publication.Slug,
                ReleaseSlug: oldRelease.Slug,
                FileParentPath: "test-folder-2"
            );

            var publicBlobCacheService = fixture.PublicBlobCacheService;

            // This represents the cache stored in the release-specific directory
            await publicBlobCacheService.SetItemAsync(oldReleaseCacheKey, oldReleaseCachedViewModel);
            // This represents the cache stored in the 'latest-release.json' path
            await publicBlobCacheService.SetItemAsync(oldLatestReleaseCacheKey, oldLatestReleaseCachedViewModel);
            // This represents the publication cache
            await publicBlobCacheService.SetItemAsync(oldPublicationCacheKey, oldPublicationCachedViewModel);
            // This represents the release parent path cache folder, and some test data cached within it (in nested folders)
            await publicBlobCacheService.SetItemAsync(
                oldReleaseParentPathTestDataCacheKey1,
                oldReleaseParentPathTestDataCachedViewModel1
            );
            await publicBlobCacheService.SetItemAsync(
                oldReleaseParentPathTestDataCacheKey2,
                oldReleaseParentPathTestDataCachedViewModel2
            );

            // Testing that these pieces of cache have actually been stored
            Assert.NotNull(
                await publicBlobCacheService.GetItemAsync(oldReleaseCacheKey, typeof(ReleaseCacheViewModel))
            );
            Assert.NotNull(
                await publicBlobCacheService.GetItemAsync(oldLatestReleaseCacheKey, typeof(ReleaseCacheViewModel))
            );
            Assert.NotNull(
                await publicBlobCacheService.GetItemAsync(oldPublicationCacheKey, typeof(PublicationCacheViewModel))
            );
            Assert.NotNull(
                await publicBlobCacheService.GetItemAsync(
                    oldReleaseParentPathTestDataCacheKey1,
                    typeof(TestReleaseParentPathDataViewModel)
                )
            );
            Assert.NotNull(
                await publicBlobCacheService.GetItemAsync(
                    oldReleaseParentPathTestDataCacheKey2,
                    typeof(TestReleaseParentPathDataViewModel)
                )
            );

            var response = await UpdateRelease(releaseId: oldRelease.Id, label: oldLabel);

            response.AssertOk<ReleaseViewModel>();

            var updatedRelease = await fixture
                .GetContentDbContext()
                .Releases.Include(r => r.Publication)
                .SingleAsync(r => r.Id == oldRelease.Id);

            var newSlugCachedValue =
                await publicBlobCacheService.GetItemAsync(oldReleaseCacheKey, typeof(ReleaseCacheViewModel))
                as ReleaseCacheViewModel;

            var newLatestReleaseCachedValue =
                await publicBlobCacheService.GetItemAsync(oldLatestReleaseCacheKey, typeof(ReleaseCacheViewModel))
                as ReleaseCacheViewModel;

            var newPublicationCachedValue =
                await publicBlobCacheService.GetItemAsync(oldPublicationCacheKey, typeof(PublicationCacheViewModel))
                as PublicationCacheViewModel;

            var oldReleaseParentPathTestDataCachedValue1 = await publicBlobCacheService.GetItemAsync(
                oldReleaseParentPathTestDataCacheKey1,
                typeof(TestReleaseParentPathDataViewModel)
            );
            var oldReleaseParentPathTestDataCachedValue2 = await publicBlobCacheService.GetItemAsync(
                oldReleaseParentPathTestDataCacheKey2,
                typeof(TestReleaseParentPathDataViewModel)
            );

            // Checking that the Release Label and Slug are UNCHANGED
            Assert.Equal(oldLabel, updatedRelease.Label);
            Assert.Equal(oldSlug, updatedRelease.Slug);

            // Checking that the release-specific cache for the new release slug has been updated, and is not the same as the old cache.
            // Also checking that the cache is for the LATEST release version via it's ID, and the slug is UNCHANGED.
            Assert.NotNull(newSlugCachedValue);
            Assert.NotEqual(oldReleaseCachedViewModel, newSlugCachedValue);
            Assert.Equal(latestPublishedReleaseVersion.Id, newSlugCachedValue.Id);
            Assert.Equal(oldSlug, newSlugCachedValue.Slug);

            // Checking that the latest release cache has been updated, and is not the same as the old cache.
            // Also checking that the cache is for the LATEST release version via it's ID, and the slug is UNCHANGED.
            Assert.NotNull(newLatestReleaseCachedValue);
            Assert.NotEqual(oldLatestReleaseCachedViewModel, newLatestReleaseCachedValue);
            Assert.Equal(latestPublishedReleaseVersion.Id, newLatestReleaseCachedValue.Id);
            Assert.Equal(oldSlug, newLatestReleaseCachedValue.Slug);

            // Checking that the publication cache has been updated, and is not the same as the old cache.
            Assert.NotNull(newPublicationCachedValue);
            Assert.NotEqual(oldPublicationCachedViewModel, newPublicationCachedValue);
            Assert.All(newPublicationCachedValue.Releases, r => Assert.Equal(oldSlug, r.Slug));
            Assert.All(newPublicationCachedValue.ReleaseSeries, rs => Assert.Equal(oldSlug, rs.ReleaseSlug));

            // Checking that all of the cache within the release parent path is left unchanged
            Assert.Equal(oldReleaseParentPathTestDataCachedViewModel1, oldReleaseParentPathTestDataCachedValue1);
            Assert.Equal(oldReleaseParentPathTestDataCachedViewModel2, oldReleaseParentPathTestDataCachedValue2);
        }

        [Fact]
        public async Task SlugChanged_CacheIsUntouchedForUnpublishedRelease()
        {
            var oldSlug = "2020-21-initial";

            Publication publication = DataFixture.DefaultPublication();

            Release oldRelease = DataFixture
                .DefaultRelease(publishedVersions: 0, draftVersion: true)
                .WithYear(2020)
                .WithTimePeriodCoverage(TimeIdentifier.AcademicYear)
                .WithLabel("initial")
                .WithSlug(oldSlug)
                .WithPublication(publication);

            await fixture.GetContentDbContext().AddTestData(context => context.Releases.Add(oldRelease));

            var response = await UpdateRelease(releaseId: oldRelease.Id, label: "final");

            response.AssertOk<ReleaseViewModel>();

            var updatedRelease = await fixture
                .GetContentDbContext()
                .Releases.Include(r => r.Publication)
                .SingleAsync(r => r.Id == oldRelease.Id);

            var oldSlugReleaseCacheKey = new ReleaseCacheKey(
                publicationSlug: publication.Slug,
                releaseSlug: oldRelease.Slug
            );

            var publicBlobCacheService = fixture.PublicBlobCacheService;

            var oldSlugReleaseCachedValue = await publicBlobCacheService.GetItemAsync(
                oldSlugReleaseCacheKey,
                typeof(ReleaseCacheViewModel)
            );

            var newSlugReleaseCacheKey = new ReleaseCacheKey(
                publicationSlug: publication.Slug,
                releaseSlug: updatedRelease.Slug
            );
            var newSlugReleaseCachedValue = await publicBlobCacheService.GetItemAsync(
                newSlugReleaseCacheKey,
                typeof(ReleaseCacheViewModel)
            );

            var publicationCacheKey = new PublicationCacheKey(publication.Slug);
            var publicationCacheValue = await publicBlobCacheService.GetItemAsync(
                publicationCacheKey,
                typeof(ReleaseCacheViewModel)
            );

            // Checking that there isn't any cache for the old release view-model
            Assert.Null(oldSlugReleaseCachedValue);

            // Checking that there isn't any cache for the new release view-model
            Assert.Null(newSlugReleaseCachedValue);

            // Checking that there isn't any cache for the publication view-model
            Assert.Null(publicationCacheValue);
        }

        [Fact]
        public async Task SlugChanged_ReleaseRedirectNotCreatedForUnpublishedRelease_RedirectCacheUntouched()
        {
            var oldSlug = "2020-21-initial";

            Release release = DataFixture
                .DefaultRelease(publishedVersions: 0, draftVersion: true)
                .WithYear(2020)
                .WithTimePeriodCoverage(TimeIdentifier.AcademicYear)
                .WithLabel("initial")
                .WithSlug(oldSlug)
                .WithPublication(DataFixture.DefaultPublication());

            await fixture.GetContentDbContext().AddTestData(context => context.Releases.Add(release));

            var oldRedirectsCachedViewModel = new RedirectsViewModel(
                PublicationRedirects: [],
                MethodologyRedirects: [],
                ReleaseRedirectsByPublicationSlug: []
            );
            var redirectsCacheKey = new RedirectsCacheKey();

            var publicBlobCacheService = fixture.PublicBlobCacheService;

            await publicBlobCacheService.SetItemAsync(redirectsCacheKey, oldRedirectsCachedViewModel);

            // Testing that the redirects cache has actually been stored
            Assert.NotNull(await publicBlobCacheService.GetItemAsync(redirectsCacheKey, typeof(RedirectsViewModel)));

            var response = await UpdateRelease(releaseId: release.Id, label: "final");

            response.AssertOk<ReleaseViewModel>();

            var releaseRedirectsExist = await fixture
                .GetContentDbContext()
                .ReleaseRedirects.AnyAsync(r => r.ReleaseId == release.Id);

            Assert.False(releaseRedirectsExist);

            // Check that the redirects cache is untouched
            var newRedirectsCachedValue = Assert.IsType<RedirectsViewModel>(
                await publicBlobCacheService.GetItemAsync(redirectsCacheKey, typeof(RedirectsViewModel))
            );

            Assert.DoesNotContain(
                newRedirectsCachedValue.PublicationRedirects,
                r => r.FromSlug == release.Publication.Slug
            );
            Assert.False(
                newRedirectsCachedValue.ReleaseRedirectsByPublicationSlug.ContainsKey(release.Publication.Slug)
            );
        }

        [Fact]
        public async Task SlugChanged_ReleaseRedirectCreatedForLiveRelease_RedirectsCacheUpdated()
        {
            var oldLabel = "initial";
            var oldSlug = $"2020-21-{oldLabel}";

            Release release = DataFixture
                .DefaultRelease(publishedVersions: 1, draftVersion: true)
                .WithYear(2020)
                .WithTimePeriodCoverage(TimeIdentifier.AcademicYear)
                .WithLabel(oldLabel)
                .WithSlug(oldSlug)
                .WithPublication(DataFixture.DefaultPublication());

            await fixture.GetContentDbContext().AddTestData(context => context.Releases.Add(release));

            var oldRedirectsCachedViewModel = new RedirectsViewModel(
                PublicationRedirects: [],
                MethodologyRedirects: [],
                ReleaseRedirectsByPublicationSlug: []
            );
            var redirectsCacheKey = new RedirectsCacheKey();

            var publicBlobCacheService = fixture.PublicBlobCacheService;

            await publicBlobCacheService.SetItemAsync(redirectsCacheKey, oldRedirectsCachedViewModel);

            // Testing that the redirects cache has actually been stored
            Assert.NotNull(await publicBlobCacheService.GetItemAsync(redirectsCacheKey, typeof(RedirectsViewModel)));

            var newLabel = "final";
            var response = await UpdateRelease(releaseId: release.Id, label: newLabel);

            response.AssertOk<ReleaseViewModel>();

            // Check that a release redirect was created
            var releaseRedirect = await fixture
                .GetContentDbContext()
                .ReleaseRedirects.SingleAsync(r => r.ReleaseId == release.Id);

            Assert.Equal(oldSlug, releaseRedirect.Slug);

            // Check that the redirects cache has been updated
            var newRedirectsCachedValue =
                await publicBlobCacheService.GetItemAsync(redirectsCacheKey, typeof(RedirectsViewModel))
                as RedirectsViewModel;

            Assert.Empty(newRedirectsCachedValue!.PublicationRedirects);
            Assert.Empty(newRedirectsCachedValue.MethodologyRedirects);

            var releaseRedirectsViewModel = newRedirectsCachedValue.ReleaseRedirectsByPublicationSlug[
                release.Publication.Slug
            ];

            var releaseRedirectViewModel = Assert.Single(releaseRedirectsViewModel);
            Assert.Equal($"2020-21-{newLabel}", releaseRedirectViewModel.ToSlug);
        }

        [Fact]
        public async Task SlugUnchanged_ReleaseRedirectNotCreatedForLiveRelease_RedirectCacheUntouched()
        {
            var oldLabel = "initial";
            var oldSlug = $"2020-21-{oldLabel}";

            Release release = DataFixture
                .DefaultRelease(publishedVersions: 1, draftVersion: true)
                .WithYear(2020)
                .WithTimePeriodCoverage(TimeIdentifier.AcademicYear)
                .WithLabel(oldLabel)
                .WithSlug(oldSlug)
                .WithPublication(DataFixture.DefaultPublication());

            await fixture.GetContentDbContext().AddTestData(context => context.Releases.Add(release));

            var oldRedirectsCachedViewModel = new RedirectsViewModel(
                PublicationRedirects: [],
                MethodologyRedirects: [],
                ReleaseRedirectsByPublicationSlug: []
            );
            var redirectsCacheKey = new RedirectsCacheKey();

            var publicBlobCacheService = fixture.PublicBlobCacheService;

            await publicBlobCacheService.SetItemAsync(redirectsCacheKey, oldRedirectsCachedViewModel);

            // Testing that the redirects cache has actually been stored
            Assert.NotNull(await publicBlobCacheService.GetItemAsync(redirectsCacheKey, typeof(RedirectsViewModel)));

            var response = await UpdateRelease(releaseId: release.Id, label: oldLabel);

            response.AssertOk<ReleaseViewModel>();

            // Check that a release redirect was NOT created
            var releaseRedirectExists = await fixture
                .GetContentDbContext()
                .ReleaseRedirects.AnyAsync(r => r.ReleaseId == release.Id);

            Assert.False(releaseRedirectExists);

            // Check that the redirects cache is untouched
            var newRedirectsCachedViewModel = Assert.IsType<RedirectsViewModel>(
                await publicBlobCacheService.GetItemAsync(redirectsCacheKey, typeof(RedirectsViewModel))
            );

            Assert.Empty(newRedirectsCachedViewModel.PublicationRedirects);
            Assert.Empty(newRedirectsCachedViewModel.MethodologyRedirects);
            Assert.Empty(newRedirectsCachedViewModel.ReleaseRedirectsByPublicationSlug);
        }

        [Fact]
        public async Task ReleaseNotFound()
        {
            var response = await UpdateRelease(releaseId: Guid.NewGuid(), label: null);

            response.AssertNotFound();
        }

        [Fact]
        public async Task UserDoesNotHavePermission()
        {
            Release release = DataFixture
                .DefaultRelease(publishedVersions: 1)
                .WithPublication(DataFixture.DefaultPublication());

            await fixture.GetContentDbContext().AddTestData(context => context.Releases.Add(release));

            var response = await UpdateRelease(
                releaseId: release.Id,
                label: null,
                user: OptimisedTestUsers.Authenticated
            );

            response.AssertForbidden();
        }

        [Theory]
        [InlineData(2020, TimeIdentifier.AcademicYear, "initial", "2020-21-initial")]
        [InlineData(2020, TimeIdentifier.AcademicYear, "Initial", "2020-21-initial")]
        [InlineData(2020, TimeIdentifier.AcademicYear, " initial ", "2020-21-initial")]
        [InlineData(2020, TimeIdentifier.AcademicYear, null, "2020-21")]
        [InlineData(2020, TimeIdentifier.AcademicYear, "", "2020-21")]
        [InlineData(2020, TimeIdentifier.AcademicYear, " ", "2020-21")]
        [InlineData(2020, TimeIdentifier.AcademicYear, "  ", "2020-21")]
        [InlineData(2020, TimeIdentifier.AcademicYearQ1, "initial", "2020-21-q1-initial")]
        public async Task SlugNotUniqueToPublication(
            int year,
            TimeIdentifier timePeriodCoverage,
            string? label,
            string existingReleaseSlug
        )
        {
            Publication publication = DataFixture
                .DefaultPublication()
                .WithReleases(
                    DataFixture
                        .DefaultRelease(publishedVersions: 1)
                        .WithYear(year)
                        .WithTimePeriodCoverage(timePeriodCoverage)
                        .ForIndex(0, s => s.SetSlug(existingReleaseSlug))
                        .GenerateList(2)
                );

            var releaseBeingUpdated = publication.Releases[1];

            await fixture.GetContentDbContext().AddTestData(context => context.Publications.Add(publication));

            var response = await UpdateRelease(releaseId: releaseBeingUpdated.Id, label: label);

            var validationProblem = response.AssertValidationProblem();

            var error = Assert.Single(validationProblem.Errors);

            Assert.Equal(SlugNotUnique.ToString(), error.Code);
        }

        [Fact]
        public async Task ReleaseIsUndergoingPublishing()
        {
            Release release = DataFixture
                .DefaultRelease(publishedVersions: 2, draftVersion: true)
                .WithPublication(DataFixture.DefaultPublication());

            var latestReleaseVersion = release.Versions[2];

            await fixture.GetContentDbContext().AddTestData(context => context.Releases.Add(release));

            var releaseStatusId = Guid.NewGuid();
            var releasePublishingStatus = new ReleasePublishingStatus(
                releaseVersionId: latestReleaseVersion.Id,
                releaseStatusId: releaseStatusId,
                publicationSlug: release.Publication.Slug,
                publish: null,
                releaseSlug: release.Slug,
                state: ReleasePublishingStatusStates.ImmediateReleaseStartedState,
                immediate: true
            );

            var publisherTableStorageService = fixture.PublisherTableStorageService;

            await publisherTableStorageService.CreateEntity(
                tableName: TableStorageTableNames.PublisherReleaseStatusTableName,
                entity: releasePublishingStatus
            );

            // Testing that the release publishing status has actually been stored
            Assert.NotNull(
                await publisherTableStorageService.GetEntityIfExists<ReleasePublishingStatus>(
                    tableName: TableStorageTableNames.PublisherReleaseStatusTableName,
                    partitionKey: latestReleaseVersion.Id.ToString(),
                    rowKey: releaseStatusId.ToString()
                )
            );

            var response = await UpdateRelease(releaseId: release.Id, label: "new label");

            var validationProblem = response.AssertValidationProblem();

            var error = Assert.Single(validationProblem.Errors);

            Assert.Equal(ReleaseUndergoingPublishing.ToString(), error.Code);
        }

        [Fact]
        public async Task ReleaseRedirectExistsForNewSlugForSameRelease()
        {
            Release release = DataFixture
                .DefaultRelease(publishedVersions: 1)
                .WithYear(2020)
                .WithTimePeriodCoverage(TimeIdentifier.AcademicYear)
                .WithLabel("initial")
                .WithSlug("2020-21-initial")
                .WithRedirects([DataFixture.DefaultReleaseRedirect().WithSlug("2020-21-final")])
                .WithPublication(DataFixture.DefaultPublication());

            await fixture.GetContentDbContext().AddTestData(context => context.Releases.Add(release));

            var response = await UpdateRelease(releaseId: release.Id, label: "final");

            var validationProblem = response.AssertValidationProblem();

            var error = Assert.Single(validationProblem.Errors);

            Assert.Equal(ReleaseSlugUsedByRedirect.ToString(), error.Code);
        }

        [Fact]
        public async Task ReleaseRedirectExistsForNewSlugForDifferentReleaseInSamePublication()
        {
            Publication publication = DataFixture.DefaultPublication();

            Release targetRelease = DataFixture
                .DefaultRelease(publishedVersions: 1)
                .WithYear(2020)
                .WithTimePeriodCoverage(TimeIdentifier.AcademicYear)
                .WithLabel("initial")
                .WithSlug("2020-21-initial")
                .WithPublication(publication);

            Release otherRelease = DataFixture
                .DefaultRelease(publishedVersions: 1)
                .WithYear(2020)
                .WithTimePeriodCoverage(TimeIdentifier.AcademicYear)
                .WithLabel("intermediate")
                .WithSlug("2020-21-intermediate")
                .WithRedirects([DataFixture.DefaultReleaseRedirect().WithSlug("2020-21-final")])
                .WithPublication(publication);

            await fixture
                .GetContentDbContext()
                .AddTestData(context => context.Releases.AddRange(targetRelease, otherRelease));

            var response = await UpdateRelease(releaseId: targetRelease.Id, label: "final");

            var validationProblem = response.AssertValidationProblem();

            var error = Assert.Single(validationProblem.Errors);

            Assert.Equal(ReleaseSlugUsedByRedirect.ToString(), error.Code);
        }

        [Fact]
        public async Task ReleaseRedirectExistsForNewSlugForReleaseInDifferentPublication()
        {
            Release targetRelease = DataFixture
                .DefaultRelease(publishedVersions: 1)
                .WithYear(2020)
                .WithTimePeriodCoverage(TimeIdentifier.AcademicYear)
                .WithLabel("initial")
                .WithSlug("2020-21-initial")
                .WithPublication(DataFixture.DefaultPublication());

            Release otherRelease = DataFixture
                .DefaultRelease(publishedVersions: 1)
                .WithYear(2020)
                .WithTimePeriodCoverage(TimeIdentifier.AcademicYear)
                .WithLabel("intermediate")
                .WithSlug("2020-21-intermediate")
                .WithRedirects([DataFixture.DefaultReleaseRedirect().WithSlug("2020-21-final")])
                .WithPublication(DataFixture.DefaultPublication());

            await fixture
                .GetContentDbContext()
                .AddTestData(context => context.Releases.AddRange(targetRelease, otherRelease));

            var response = await UpdateRelease(releaseId: targetRelease.Id, label: "final");

            response.AssertOk();
        }

        [Fact]
        public async Task LabelOver20Characters()
        {
            Release release = DataFixture
                .DefaultRelease(publishedVersions: 1)
                .WithPublication(DataFixture.DefaultPublication());

            await fixture.GetContentDbContext().AddTestData(context => context.Releases.Add(release));

            var response = await UpdateRelease(releaseId: release.Id, label: new string('a', 21));

            var validationProblem = response.AssertValidationProblem();

            var error = Assert.Single(validationProblem.Errors);

            Assert.Equal(
                $"The field {nameof(ReleaseUpdateRequest.Label)} must be a string or array type with a maximum length of '20'.",
                error.Message
            );
            Assert.Equal(nameof(ReleaseUpdateRequest.Label), error.Path);
        }

        private async Task<HttpResponseMessage> UpdateRelease(
            Guid releaseId,
            string? label = null,
            ClaimsPrincipal? user = null
        )
        {
            var client = fixture.CreateClient(user: user ?? OptimisedTestUsers.Bau);

            var request = new ReleaseUpdateRequest { Label = label };

            return await client.PatchAsJsonAsync($"api/releases/{releaseId}", request);
        }

        private record TestReleaseParentPathDataCacheKey(
            string PublicationSlug,
            string ReleaseSlug,
            string FileParentPath
        ) : IBlobCacheKey
        {
            public IBlobContainer Container => BlobContainers.PublicContent;

            public string Key =>
                $"{FileStoragePathUtils.PublicContentReleaseParentPath(
                publicationSlug: PublicationSlug, 
                releaseSlug: ReleaseSlug)}/{FileParentPath}/test-data.json";
        }

        private record TestReleaseParentPathDataViewModel;
    }
}
