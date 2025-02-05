#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;
using ReleaseViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ReleaseViewModel;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api;

public class ReleasesControllerUnitTests
{
    [Fact]
    public async Task Create_Release_Returns_Ok()
    {
        var returnedViewModel = new ReleaseVersionViewModel();

        var releaseService = new Mock<IReleaseService>(Strict);

        releaseService
            .Setup(s => s.CreateRelease(It.IsAny<ReleaseCreateRequest>()))
            .ReturnsAsync(returnedViewModel);

        var controller = BuildController(releaseService.Object);

        var result = await controller.CreateRelease(new ReleaseCreateRequest());
        VerifyAllMocks(releaseService);

        result.AssertOkResult(returnedViewModel);
    }

    private static ReleasesController BuildController(
        IReleaseService? releaseService = null)
    {
        return new ReleasesController(
            releaseService ?? Mock.Of<IReleaseService>(Strict));
    }
}

public abstract class ReleasesControllerIntegrationTests(TestApplicationFactory testApp) : IntegrationTestFixture(testApp)
{
    public class CreateReleaseTests(TestApplicationFactory testApp) : ReleasesControllerIntegrationTests(testApp)
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
            string expectedSlug)
        {
            Publication publication = DataFixture.DefaultPublication();

            await TestApp.AddTestData<ContentDbContext>(
                context => context.Publications.Add(publication));

            var response = await CreateRelease(
                publicationId: publication.Id,
                year: year,
                timePeriodCoverage: timePeriodCoverage,
                label: label);

            var viewModel = response.AssertOk<ReleaseVersionViewModel>();

            var contentDbContext = TestApp.GetDbContext<ContentDbContext>();

            var updatedPublication = contentDbContext.Publications
                .Include(p => p.Releases)
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
                label: null);

            response.AssertNotFound();
        }

        [Fact]
        public async Task UserDoesNotHavePermission()
        {
            Publication publication = DataFixture.DefaultPublication();

            await TestApp.AddTestData<ContentDbContext>(
                context => context.Publications.Add(publication));

            var client = BuildApp(DataFixture.AuthenticatedUser()).CreateClient();

            var response = await CreateRelease(
                publicationId: publication.Id,
                year: 2020,
                timePeriodCoverage: TimeIdentifier.AcademicYear,
                label: null,
                client: client);

            response.AssertForbidden();
        }

        [Fact]
        public async Task ReleaseTypeInvalid()
        {
            Publication publication = DataFixture.DefaultPublication();

            await TestApp.AddTestData<ContentDbContext>(
                context => context.Publications.Add(publication));

            var response = await CreateRelease(
                publicationId: publication.Id,
                year: 2020,
                timePeriodCoverage: TimeIdentifier.AcademicYear,
                label: null,
                type: ReleaseType.ExperimentalStatistics);

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
            string existingReleaseSlug)
        {
            Publication publication = DataFixture.DefaultPublication()
                .WithReleases([
                    DataFixture
                        .DefaultRelease(publishedVersions: 1)
                        .WithSlug(existingReleaseSlug)
                    ]);

            await TestApp.AddTestData<ContentDbContext>(
                context => context.Publications.Add(publication));

            var response = await CreateRelease(
                publicationId: publication.Id,
                year: year,
                timePeriodCoverage: timePeriodCoverage,
                label: label);

            var validationProblem = response.AssertValidationProblem();

            var error = Assert.Single(validationProblem.Errors);

            Assert.Equal(SlugNotUnique.ToString(), error.Code);
        }

        [Fact]
        public async Task LabelOver50Characters()
        {
            Publication publication = DataFixture.DefaultPublication();

            await TestApp.AddTestData<ContentDbContext>(
                context => context.Publications.Add(publication));

            var response = await CreateRelease(
                publicationId: publication.Id,
                year: 2020,
                timePeriodCoverage: TimeIdentifier.AcademicYear,
                label: new string('a', 51));

            var validationProblem = response.AssertValidationProblem();

            var error = Assert.Single(validationProblem.Errors);

            Assert.Equal($"The field {nameof(ReleaseCreateRequest.Label)} must be a string or array type with a maximum length of '50'.", error.Message);
            Assert.Equal(nameof(ReleaseCreateRequest.Label), error.Path);
        }

        private WebApplicationFactory<TestStartup> BuildApp(
            ClaimsPrincipal? user = null)
        {
            return TestApp.SetUser(user ?? DataFixture.BauUser());
        }

        private async Task<HttpResponseMessage> CreateRelease(
            Guid publicationId,
            int year,
            TimeIdentifier timePeriodCoverage,
            string? label = null,
            ReleaseType? type = ReleaseType.OfficialStatistics,
            HttpClient? client = null)
        {
            client ??= BuildApp().CreateClient();

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

    public class UpdateReleaseTests(TestApplicationFactory testApp) : ReleasesControllerIntegrationTests(testApp)
    {
        public override async Task InitializeAsync() => await InitializeWithAzurite();

        [Theory]
        [InlineData(2020, TimeIdentifier.AcademicYear, "initial", "initial", "2020-21-initial", "Academic year 2020/21 initial")]
        [InlineData(2020, TimeIdentifier.AcademicYear, "Initial", "Initial", "2020-21-initial", "Academic year 2020/21 Initial")]
        [InlineData(2020, TimeIdentifier.AcademicYear, " initial", "initial", "2020-21-initial", "Academic year 2020/21 initial")]
        [InlineData(2020, TimeIdentifier.AcademicYear, "initial ", "initial", "2020-21-initial", "Academic year 2020/21 initial")]
        [InlineData(2020, TimeIdentifier.AcademicYear, "initial 2", "initial 2", "2020-21-initial-2", "Academic year 2020/21 initial 2")]
        [InlineData(2020, TimeIdentifier.AcademicYear, "initial  2", "initial 2", "2020-21-initial-2", "Academic year 2020/21 initial 2")]
        [InlineData(2020, TimeIdentifier.AcademicYear, "", null, "2020-21", "Academic year 2020/21")]
        [InlineData(2020, TimeIdentifier.AcademicYear, " ", null, "2020-21", "Academic year 2020/21")]
        [InlineData(2020, TimeIdentifier.AcademicYear, "  ", null, "2020-21", "Academic year 2020/21")]
        [InlineData(2020, TimeIdentifier.AcademicYear, null, null, "2020-21", "Academic year 2020/21")]
        [InlineData(2020, TimeIdentifier.AcademicYearQ1, "initial", "initial", "2020-21-q1-initial", "Academic year Q1 2020/21 initial")]
        public async Task Success(
            int year,
            TimeIdentifier timePeriodCoverage,
            string? label,
            string? expectedLabel,
            string expectedSlug,
            string expectedTitle)
        {
            Release release = DataFixture.DefaultRelease(publishedVersions: 1)
                .WithYear(year)
                .WithTimePeriodCoverage(timePeriodCoverage)
                .WithPublication(DataFixture.DefaultPublication());

            await TestApp.AddTestData<ContentDbContext>(
                context => context.Releases.Add(release));

            var response = await UpdateRelease(
                releaseId: release.Id,
                label: label);

            var viewModel = response.AssertOk<ReleaseViewModel>();

            Assert.Equal(release.PublicationId, viewModel.PublicationId);
            Assert.Equal(expectedSlug, viewModel.Slug);
            Assert.Equal(timePeriodCoverage, viewModel.TimePeriodCoverage);
            Assert.Equal(year, viewModel.Year);
            Assert.Equal(expectedLabel, viewModel.Label);
            Assert.Equal(expectedTitle, viewModel.Title);

            var contentDbContext = TestApp.GetDbContext<ContentDbContext>();

            var updatedRelease = await contentDbContext.Releases
                .Include(r => r.Publication)
                .SingleAsync(r => r.Id == release.Id);

            Assert.Equal(expectedLabel, updatedRelease.Label);
            Assert.Equal(expectedSlug, updatedRelease.Slug);
        }

        [Fact]
        public async Task CacheIsUpdatedForLiveRelease()
        {
            var oldSlug = "2020-21-initial";

            Publication publication = DataFixture.DefaultPublication();

            Release oldRelease = DataFixture.DefaultRelease(publishedVersions: 2, draftVersion: true)
                .WithYear(2020)
                .WithTimePeriodCoverage(TimeIdentifier.AcademicYear)
                .WithLabel("initial")
                .WithSlug(oldSlug)
                .WithPublication(publication);

            await TestApp.AddTestData<ContentDbContext>(
                context => context.Releases.Add(oldRelease));

            var app = BuildApp();
            var client = app.CreateClient();

            var publicBlobCacheService = app.Services.GetRequiredService<IPublicBlobCacheService>();

            var latestPublishedReleaseVersion = oldRelease.Versions[1];

            var oldReleaseCachedViewModel = new ReleaseCacheViewModel(id: latestPublishedReleaseVersion.Id);
            var oldReleaseCacheKey = new ReleaseCacheKey(
                publicationSlug: publication.Slug,
                releaseSlug: oldRelease.Slug);

            var oldLatestReleaseCachedViewModel = new ReleaseCacheViewModel(id: latestPublishedReleaseVersion.Id);
            var oldLatestReleaseCacheKey = new ReleaseCacheKey(
                publicationSlug: publication.Slug);

            // This represents the cache stored in the release-specific directory
            await publicBlobCacheService.SetItemAsync(oldReleaseCacheKey, oldReleaseCachedViewModel);
            // This represents the cache stored in the 'latest-release.json' path
            await publicBlobCacheService.SetItemAsync(oldLatestReleaseCacheKey, oldLatestReleaseCachedViewModel);

            // Testing that these two pieces of cache have actually been stored
            Assert.NotNull(await publicBlobCacheService.GetItemAsync(oldReleaseCacheKey, typeof(ReleaseCacheViewModel)));
            Assert.NotNull(await publicBlobCacheService.GetItemAsync(oldLatestReleaseCacheKey, typeof(ReleaseCacheViewModel)));

            var response = await UpdateRelease(
                releaseId: oldRelease.Id,
                label: "final",
                client: client);

            response.AssertOk<ReleaseViewModel>();

            var contentDbContext = TestApp.GetDbContext<ContentDbContext>();

            var updatedRelease = await contentDbContext.Releases
                .Include(r => r.Publication)
                .SingleAsync(r => r.Id == oldRelease.Id);

            var oldSlugReleaseCacheKey = new ReleaseCacheKey(
                publicationSlug: publication.Slug,
                releaseSlug: oldRelease.Slug);
            var oldSlugCachedValue = await publicBlobCacheService.GetItemAsync(oldSlugReleaseCacheKey, typeof(ReleaseCacheViewModel));

            var newSlugReleaseCacheKey = new ReleaseCacheKey(
                publicationSlug: publication.Slug,
                releaseSlug: updatedRelease.Slug);
            var newSlugCachedValue = await publicBlobCacheService.GetItemAsync(newSlugReleaseCacheKey, typeof(ReleaseCacheViewModel))
                as ReleaseCacheViewModel;

            var latestReleaseCacheKey = new ReleaseCacheKey(
                publicationSlug: publication.Slug);
            var latestReleaseCachedValue = await publicBlobCacheService.GetItemAsync(latestReleaseCacheKey, typeof(ReleaseCacheViewModel))
                as ReleaseCacheViewModel;

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
            Assert.NotNull(latestReleaseCachedValue);
            Assert.NotEqual(oldLatestReleaseCachedViewModel, latestReleaseCachedValue);
            Assert.Equal(latestPublishedReleaseVersion.Id, latestReleaseCachedValue.Id);
            Assert.Equal(updatedRelease.Slug, latestReleaseCachedValue.Slug);
        }

        [Fact]
        public async Task CacheIsUntouchedForUnpublishedRelease()
        {
            var oldSlug = "2020-21-initial";

            Publication publication = DataFixture.DefaultPublication();

            Release oldRelease = DataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true)
                .WithYear(2020)
                .WithTimePeriodCoverage(TimeIdentifier.AcademicYear)
                .WithLabel("initial")
                .WithSlug(oldSlug)
                .WithPublication(publication);

            await TestApp.AddTestData<ContentDbContext>(
                context => context.Releases.Add(oldRelease));

            var app = BuildApp();
            var client = app.CreateClient();

            var response = await UpdateRelease(
                releaseId: oldRelease.Id,
                label: "final",
                client: client);

            response.AssertOk<ReleaseViewModel>();

            var publicBlobCacheService = app.Services.GetRequiredService<IPublicBlobCacheService>();

            var contentDbContext = TestApp.GetDbContext<ContentDbContext>();

            var updatedRelease = await contentDbContext.Releases
                .Include(r => r.Publication)
                .SingleAsync(r => r.Id == oldRelease.Id);

            var oldSlugCacheKey = new ReleaseCacheKey(
                publicationSlug: publication.Slug,
                releaseSlug: oldRelease.Slug);
            var oldSlugCachedValue = await publicBlobCacheService.GetItemAsync(oldSlugCacheKey, typeof(ReleaseCacheViewModel));

            var newSlugCacheKey = new ReleaseCacheKey(
                publicationSlug: publication.Slug,
                releaseSlug: updatedRelease.Slug);
            var newSlugCachedValue = await publicBlobCacheService.GetItemAsync(newSlugCacheKey, typeof(ReleaseCacheViewModel));

            // Checking that there isn't any cache for the old release view-model
            Assert.Null(oldSlugCachedValue);

            // Checking that there isn't any cache for the new release view-model
            Assert.Null(newSlugCachedValue);
        }

        [Fact]
        public async Task ReleaseRedirectNotCreatedForUnpublishedRelease()
        {
            var oldSlug = "2020-21-initial";

            Release release = DataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true)
                .WithYear(2020)
                .WithTimePeriodCoverage(TimeIdentifier.AcademicYear)
                .WithLabel("initial")
                .WithSlug(oldSlug)
                .WithPublication(DataFixture.DefaultPublication());

            await TestApp.AddTestData<ContentDbContext>(
                context => context.Releases.Add(release));

            var response = await UpdateRelease(
                releaseId: release.Id,
                label: "final");

            response.AssertOk<ReleaseViewModel>();

            var contentDbContext = TestApp.GetDbContext<ContentDbContext>();

            var releaseRedirects = await contentDbContext.ReleaseRedirects
                .ToListAsync();

            Assert.Empty(releaseRedirects);
        }

        [Fact]
        public async Task ReleaseRedirectCreatedForLiveRelease()
        {
            var oldSlug = "2020-21-initial";

            Release release = DataFixture.DefaultRelease(publishedVersions: 1, draftVersion: true)
                .WithYear(2020)
                .WithTimePeriodCoverage(TimeIdentifier.AcademicYear)
                .WithLabel("initial")
                .WithSlug(oldSlug)
                .WithPublication(DataFixture.DefaultPublication());

            await TestApp.AddTestData<ContentDbContext>(
                context => context.Releases.Add(release));

            var response = await UpdateRelease(
                releaseId: release.Id,
                label: "final");

            response.AssertOk<ReleaseViewModel>();

            var contentDbContext = TestApp.GetDbContext<ContentDbContext>();

            var releaseRedirect = await contentDbContext.ReleaseRedirects
                .SingleAsync(r => r.ReleaseId == release.Id);

            Assert.Equal(oldSlug, releaseRedirect.Slug);
        }

        [Fact]
        public async Task ReleaseNotFound()
        {
            var response = await UpdateRelease(
                releaseId: Guid.NewGuid(),
                label: null);

            response.AssertNotFound();
        }

        [Fact]
        public async Task UserDoesNotHavePermission()
        {
            Release release = DataFixture.DefaultRelease(publishedVersions: 1)
                .WithPublication(DataFixture.DefaultPublication());

            await TestApp.AddTestData<ContentDbContext>(
                context => context.Releases.Add(release));

            var client = BuildApp(DataFixture.AuthenticatedUser()).CreateClient();

            var response = await UpdateRelease(
                releaseId: release.Id,
                label: null,
                client: client);

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
            string existingReleaseSlug)
        {
            Publication publication = DataFixture.DefaultPublication()
                .WithReleases(DataFixture.DefaultRelease(publishedVersions: 1)
                    .WithYear(year)
                    .WithTimePeriodCoverage(timePeriodCoverage)
                    .ForIndex(0, s => s.SetSlug(existingReleaseSlug))
                    .GenerateList(2));

            var releaseBeingUpdated = publication.Releases[1];

            await TestApp.AddTestData<ContentDbContext>(
                context => context.Publications.Add(publication));

            var response = await UpdateRelease(
                releaseId: releaseBeingUpdated.Id,
                label: label);

            var validationProblem = response.AssertValidationProblem();

            var error = Assert.Single(validationProblem.Errors);

            Assert.Equal(SlugNotUnique.ToString(), error.Code);
        }

        [Fact]
        public async Task ReleaseIsUndergoingPublishing()
        {
            Release release = DataFixture.DefaultRelease()
                .WithVersions(DataFixture.DefaultReleaseVersion()
                    .WithApprovalStatus(ReleaseApprovalStatus.Approved)
                    .ForIndex(0, s => s.SetPublished(DateTime.UtcNow))
                    .ForIndex(1, s => s.SetPublished(DateTime.UtcNow))
                    .GenerateList(3))
                .WithPublication(DataFixture.DefaultPublication());

            await TestApp.AddTestData<ContentDbContext>(
                context => context.Releases.Add(release));

            var response = await UpdateRelease(
                releaseId: release.Id,
                label: "new label");

            var validationProblem = response.AssertValidationProblem();

            var error = Assert.Single(validationProblem.Errors);

            Assert.Equal(ReleaseUndergoingPublishing.ToString(), error.Code);
        }

        [Fact]
        public async Task ReleaseRedirectExistsForNewSlug()
        {
            Release release = DataFixture.DefaultRelease(publishedVersions: 1)
                .WithYear(2020)
                .WithTimePeriodCoverage(TimeIdentifier.AcademicYear)
                .WithLabel("initial")
                .WithSlug("2020-21-initial")
                .WithRedirects([DataFixture.DefaultReleaseRedirect()
                    .WithSlug("2020-21-final")])
                .WithPublication(DataFixture.DefaultPublication());

            await TestApp.AddTestData<ContentDbContext>(
                context => context.Releases.Add(release));

            var response = await UpdateRelease(
                releaseId: release.Id,
                label: "final");

            var validationProblem = response.AssertValidationProblem();

            var error = Assert.Single(validationProblem.Errors);

            Assert.Equal(ReleaseSlugUsedByRedirect.ToString(), error.Code);
        }

        [Fact]
        public async Task LabelOver50Characters()
        {
            Release release = DataFixture.DefaultRelease(publishedVersions: 1)
                .WithPublication(DataFixture.DefaultPublication());

            await TestApp.AddTestData<ContentDbContext>(
                context => context.Releases.Add(release));

            var response = await UpdateRelease(
                releaseId: release.Id,
                label: new string('a', 51));

            var validationProblem = response.AssertValidationProblem();

            var error = Assert.Single(validationProblem.Errors);

            Assert.Equal($"The field {nameof(ReleaseUpdateRequest.Label)} must be a string or array type with a maximum length of '50'.", error.Message);
            Assert.Equal(nameof(ReleaseUpdateRequest.Label), error.Path);
        }

        private WebApplicationFactory<TestStartup> BuildApp(
            ClaimsPrincipal? user = null)
        {
            return WithAzurite(
                testApp: TestApp.SetUser(user ?? DataFixture.BauUser()), 
                enabled: true);
        }

        private async Task<HttpResponseMessage> UpdateRelease(
            Guid releaseId,
            string? label = null,
            HttpClient? client = null)
        {
            client ??= BuildApp().CreateClient();

            var request = new ReleaseUpdateRequest
            {
                Label = label,
            };

            return await client.PatchAsJsonAsync($"api/releases/{releaseId}", request);
        }
    }
}
