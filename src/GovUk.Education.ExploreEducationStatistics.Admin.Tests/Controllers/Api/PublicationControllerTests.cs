#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api;

public class PublicationControllerTests(TestApplicationFactory testApp) : IntegrationTestFixture(testApp)
{
    public class CreatePublicationTests(TestApplicationFactory testApp) : PublicationControllerTests(testApp)
    {
        [Fact]
        public async Task Success()
        {
            var theme = DataFixture.DefaultTheme().Generate();

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.Themes.Add(theme);
            });

            var request = new PublicationCreateRequest
            {
                Title = "Publication",
                Summary = "Publication summary",
                Contact = new ContactSaveRequest
                {
                    TeamName = "Team",
                    TeamEmail = "team@test.com",
                    ContactName = "Contact",
                    ContactTelNo = "01234567890"
                },
                ThemeId = theme.Id
            };

            var response = await CreatePublication(request);

            var result = response.AssertOk<PublicationCreateViewModel>();

            Assert.Multiple(
                () => Assert.Equal(request.Title, result.Title),
                () => Assert.Equal(request.Summary, result.Summary),
                () => Assert.Equal("publication", result.Slug),
                () => Assert.Equal(request.Contact.TeamName, result.Contact.TeamName),
                () => Assert.Equal(request.Contact.TeamEmail, result.Contact.TeamEmail),
                () => Assert.Equal(request.Contact.ContactName, result.Contact.ContactName),
                () => Assert.Equal(request.Contact.ContactTelNo, result.Contact.ContactTelNo),
                () => Assert.Equal(theme.Id, result.Theme.Id),
                () => Assert.Equal(theme.Title, result.Theme.Title),
                () => Assert.Null(result.SupersededById),
                () => Assert.False(result.IsSuperseded)
            );

            await using var context = TestApp.GetDbContext<ContentDbContext>();

            var saved = await context.Publications
                .Include(p => p.Contact)
                .Include(p => p.Theme)
                .SingleAsync(p => p.Id == result.Id);

            Assert.Multiple(
                () => Assert.Equal(request.Title, saved.Title),
                () => Assert.Equal(request.Summary, saved.Summary),
                () => Assert.Equal("publication", saved.Slug),
                () => Assert.Equal(request.Contact.TeamName, saved.Contact.TeamName),
                () => Assert.Equal(request.Contact.TeamEmail, saved.Contact.TeamEmail),
                () => Assert.Equal(request.Contact.ContactName, saved.Contact.ContactName),
                () => Assert.Equal(request.Contact.ContactTelNo, saved.Contact.ContactTelNo),
                () => Assert.Equal(theme.Id, saved.Theme.Id),
                () => Assert.Equal(theme.Title, saved.Theme.Title)
            );
        }

        [Fact]
        public async Task PublicationSlugNotUnique_ReturnsValidationError()
        {
            var publication = DataFixture.DefaultPublication()
                .WithTheme(DataFixture.DefaultTheme()).Generate();

            var theme = DataFixture.DefaultTheme().Generate();

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.Publications.Add(publication);
                context.Themes.Add(theme);
            });

            var request = new PublicationCreateRequest
            {
                Title = "Publication",
                Slug = publication.Slug, // Same slug as an existing saved publication
                Summary = "Publication summary",
                Contact = new ContactSaveRequest
                {
                    TeamName = "Team",
                    TeamEmail = "team@test.com",
                    ContactName = "Contact",
                    ContactTelNo = "01234567890"
                },
                ThemeId = theme.Id
            };

            var response = await CreatePublication(request);

            var validationProblem = response.AssertValidationProblem();

            validationProblem.AssertHasGlobalError(ValidationErrorMessages.PublicationSlugNotUnique);

            await using var context = TestApp.GetDbContext<ContentDbContext>();

            // Assert a new publication was not created
            var saved = Assert.Single(await context.Publications.ToListAsync());
            Assert.Equal(publication.Id, saved.Id);
        }

        [Fact]
        public async Task UserHasNoAccessToCreatePublication_ReturnsForbidden()
        {
            var theme = DataFixture.DefaultTheme().Generate();

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.Themes.Add(theme);
            });

            var request = new PublicationCreateRequest
            {
                Title = "Publication",
                Summary = "Publication summary",
                Contact = new ContactSaveRequest
                {
                    TeamName = "Team",
                    TeamEmail = "team@test.com",
                    ContactName = "Contact",
                    ContactTelNo = "01234567890"
                },
                ThemeId = theme.Id
            };

            var client = TestApp
                .SetUser(DataFixture.AuthenticatedUser())
                .CreateClient();

            var response = await CreatePublication(request, client);

            response.AssertForbidden();
        }

        private async Task<HttpResponseMessage> CreatePublication(
            PublicationCreateRequest request,
            HttpClient? client = null)
        {
            client ??= TestApp
                .SetUser(DataFixture
                    .AuthenticatedUser()
                    .WithClaim(SecurityClaimTypes.CreateAnyPublication.ToString()))
                .CreateClient();

            return await client.PostAsJsonAsync("api/publications", request);
        }
    }
}
