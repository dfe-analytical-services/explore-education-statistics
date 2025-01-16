#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
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

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api;

public class ReleasesControllerUnitTests
{
    private readonly Guid _releaseVersionId = Guid.NewGuid();
    private readonly Guid _publicationId = Guid.NewGuid();

    [Fact]
    public async Task Create_Release_Returns_Ok()
    {
        var returnedViewModel = new ReleaseVersionViewModel();

        var releaseVersionService = new Mock<IReleaseVersionService>(Strict);

        releaseVersionService
            .Setup(s => s.CreateRelease(It.IsAny<ReleaseCreateRequest>()))
            .ReturnsAsync(returnedViewModel);

        var controller = BuildController(releaseVersionService.Object);

        var result = await controller.CreateRelease(new ReleaseCreateRequest());
        VerifyAllMocks(releaseVersionService);

        result.AssertOkResult(returnedViewModel);
    }

    private static ReleasesController BuildController(
        IReleaseVersionService? releaseVersionService = null)
    {
        return new ReleasesController(
            releaseVersionService ?? Mock.Of<IReleaseVersionService>(Strict));
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
        [InlineData(2020, TimeIdentifier.AcademicYear, "initial  2", "initial  2", "2020-21-initial-2")]
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

            return await client.PostAsJsonAsync($"api/releases", request);
        }
    }
}
