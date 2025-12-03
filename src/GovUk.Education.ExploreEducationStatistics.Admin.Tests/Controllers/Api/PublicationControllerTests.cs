#nullable enable
using System.Net.Http.Json;
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture.Optimised;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.UserAuth;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api;

// ReSharper disable once ClassNeverInstantiated.Global
public class PublicationControllerTestsFixture()
    : OptimisedAdminCollectionFixture(capabilities: [AdminIntegrationTestCapability.UserAuth]);

[CollectionDefinition(nameof(PublicationControllerTestsFixture))]
public class PublicationControllerTestsCollection : ICollectionFixture<PublicationControllerTestsFixture>;

[Collection(nameof(PublicationControllerTestsFixture))]
public class PublicationControllerTests
{
    private static readonly DataFixture DataFixture = new();

    public class CreatePublicationTests(PublicationControllerTestsFixture fixture) : PublicationControllerTests
    {
        [Fact]
        public async Task Success()
        {
            var theme = DataFixture.DefaultTheme().Generate();

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
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
                    ContactTelNo = "01234567890",
                },
                ThemeId = theme.Id,
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

            var saved = await fixture
                .GetContentDbContext()
                .Publications.Include(p => p.Contact)
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
            var existingPublication = DataFixture.DefaultPublication().WithTheme(DataFixture.DefaultTheme()).Generate();

            var theme = DataFixture.DefaultTheme().Generate();

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.Publications.Add(existingPublication);
                    context.Themes.Add(theme);
                });

            var request = new PublicationCreateRequest
            {
                Title = "Publication",
                Slug = existingPublication.Slug, // Same slug as an existing saved publication
                Summary = "Publication summary",
                Contact = new ContactSaveRequest
                {
                    TeamName = "Team",
                    TeamEmail = "team@test.com",
                    ContactName = "Contact",
                    ContactTelNo = "01234567890",
                },
                ThemeId = theme.Id,
            };

            var response = await CreatePublication(request);

            var validationProblem = response.AssertValidationProblem();

            validationProblem.AssertHasGlobalError(ValidationErrorMessages.PublicationSlugNotUnique);

            // Assert the existing publication is still available.
            Assert.Single(fixture.GetContentDbContext().Publications.Where(p => p.Id == existingPublication.Id));

            // Assert the new publication was not created under the other theme.
            Assert.Null(fixture.GetContentDbContext().Publications.SingleOrDefault(p => p.ThemeId == theme.Id));
        }

        [Fact]
        public async Task RequiredFieldsAreEmpty_ReturnsValidationError()
        {
            var request = new PublicationCreateRequest
            {
                Title = "", // Empty title
                Summary = "", // Empty summary
                Contact = new ContactSaveRequest
                {
                    TeamName = "Team",
                    TeamEmail = "team@test.com",
                    ContactName = "Contact",
                    ContactTelNo = "01234567890",
                },
                ThemeId = Guid.NewGuid(),
            };

            var response = await CreatePublication(request);

            var validationProblem = response.AssertValidationProblem();

            Assert.Equal(2, validationProblem.Errors.Count);
            validationProblem.AssertHasNotEmptyError(nameof(PublicationCreateRequest.Title).ToLowerFirst());
            validationProblem.AssertHasNotEmptyError(nameof(PublicationCreateRequest.Summary).ToLowerFirst());
        }

        [Fact]
        public async Task FieldsExceedMaxLength_ReturnsValidationError()
        {
            var request = new PublicationCreateRequest
            {
                Title = new string('A', count: 66), // Title exceeds max length of 65
                Summary = new string('A', count: 161), // Summary exceeds max length of 160
                Contact = new ContactSaveRequest
                {
                    TeamName = "Team",
                    TeamEmail = "team@test.com",
                    ContactName = "Contact",
                    ContactTelNo = "01234567890",
                },
                ThemeId = Guid.NewGuid(),
            };

            var response = await CreatePublication(request);

            var validationProblem = response.AssertValidationProblem();

            Assert.Equal(2, validationProblem.Errors.Count);
            validationProblem.AssertHasMaximumLengthError(
                nameof(PublicationCreateRequest.Title).ToLowerFirst(),
                maxLength: 65
            );
            validationProblem.AssertHasMaximumLengthError(
                nameof(PublicationCreateRequest.Summary).ToLowerFirst(),
                maxLength: 160
            );
        }

        [Fact]
        public async Task UserHasNoAccessToCreatePublication_ReturnsForbidden()
        {
            var theme = DataFixture.DefaultTheme().Generate();

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
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
                    ContactTelNo = "01234567890",
                },
                ThemeId = theme.Id,
            };

            var response = await CreatePublication(request, user: OptimisedTestUsers.Authenticated);

            response.AssertForbidden();
        }

        private async Task<HttpResponseMessage> CreatePublication(
            PublicationCreateRequest request,
            ClaimsPrincipal? user = null
        )
        {
            var defaultUser = DataFixture
                .AuthenticatedUser()
                .WithClaim(nameof(SecurityClaimTypes.CreateAnyPublication));

            fixture.RegisterTestUser(user ?? defaultUser);

            var client = fixture.CreateClient().WithUser(user ?? defaultUser);

            return await client.PostAsJsonAsync("api/publications", request);
        }
    }

    public class UpdatePublicationTests(PublicationControllerTestsFixture fixture) : PublicationControllerTests
    {
        [Fact]
        public async Task RequiredFieldsAreEmpty_ReturnsValidationError()
        {
            var publicationId = Guid.NewGuid();
            var request = new PublicationSaveRequest
            {
                Title = "", // Empty title
                Summary = "", // Empty summary
                ThemeId = Guid.NewGuid(),
            };

            var response = await UpdatePublication(publicationId, request);

            var validationProblem = response.AssertValidationProblem();

            Assert.Equal(2, validationProblem.Errors.Count);
            validationProblem.AssertHasNotEmptyError(nameof(PublicationSaveRequest.Title).ToLowerFirst());
            validationProblem.AssertHasNotEmptyError(nameof(PublicationSaveRequest.Summary).ToLowerFirst());
        }

        [Fact]
        public async Task FieldsExceedMaxLength_ReturnsValidationError()
        {
            var publicationId = Guid.NewGuid();
            var request = new PublicationSaveRequest
            {
                Title = new string('A', count: 66), // Title exceeds max length of 65
                Summary = new string('A', count: 161), // Summary exceeds max length of 160
                ThemeId = Guid.NewGuid(),
            };

            var response = await UpdatePublication(publicationId, request);

            var validationProblem = response.AssertValidationProblem();

            Assert.Equal(2, validationProblem.Errors.Count);
            validationProblem.AssertHasMaximumLengthError(
                nameof(PublicationSaveRequest.Title).ToLowerFirst(),
                maxLength: 65
            );
            validationProblem.AssertHasMaximumLengthError(
                nameof(PublicationSaveRequest.Summary).ToLowerFirst(),
                maxLength: 160
            );
        }

        private async Task<HttpResponseMessage> UpdatePublication(
            Guid publicationId,
            PublicationSaveRequest request,
            ClaimsPrincipal? user = null
        )
        {
            var defaultUser = DataFixture
                .AuthenticatedUser()
                .WithClaim(nameof(SecurityClaimTypes.UpdateAllPublications));

            fixture.RegisterTestUser(user ?? defaultUser);

            var client = fixture.CreateClient().WithUser(user ?? defaultUser);

            return await client.PutAsJsonAsync($"api/publications/{publicationId}", request);
        }
    }
}
