using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.Extensions.Options;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class PreReleaseServiceTests
    {
        [Fact]
        public async Task GetPreReleaseSummaryViewModelAsync()
        {
            var contact = new Contact
            {
                Id = Guid.NewGuid(),
                TeamEmail = "first.last@education.gov.uk"
            };

            var publication = new Publication
            {
                Id = Guid.NewGuid(),
                Slug = "prerelease-publication",
                Title = "PreRelease Publication",
                ContactId = contact.Id
            };

            var release = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.AutumnSpringTerm,
                PublicationId = publication.Id
            };

            await using (var context = InMemoryApplicationDbContext("PreReleaseSummaryViewModel"))
            {
                context.Add(contact);
                context.Add(publication);
                context.Add(release);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext("PreReleaseSummaryViewModel"))
            {
                var preReleaseService = new PreReleaseService(Options.Create(new PreReleaseOptions
                    {
                        PreReleaseAccess = new PreReleaseAccessOptions()
                    }),
                    new PersistenceHelper<ContentDbContext>(context));

                var viewModel = (await preReleaseService.GetPreReleaseSummaryViewModelAsync(release.Id)).Right;
                Assert.Equal(contact.TeamEmail, viewModel.ContactEmail);
                Assert.Equal(publication.Slug, viewModel.PublicationSlug);
                Assert.Equal(publication.Title, viewModel.PublicationTitle);
                Assert.Equal(release.Title, viewModel.ReleaseTitle);
            }
        }
    }
}