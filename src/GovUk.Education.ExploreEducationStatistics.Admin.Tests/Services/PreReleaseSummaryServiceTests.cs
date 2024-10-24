#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class PreReleaseSummaryServiceTests
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

            var releaseVersion = new ReleaseVersion
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2020",
                Slug = "2020",
                TimePeriodCoverage = TimeIdentifier.AutumnSpringTerm,
                PublicationId = publication.Id
            };

            await using (var context = InMemoryApplicationDbContext("PreReleaseSummaryViewModel"))
            {
                context.Contacts.Add(contact);
                context.Publications.Add(publication);
                context.ReleaseVersions.Add(releaseVersion);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext("PreReleaseSummaryViewModel"))
            {
                var preReleaseSummaryService = new PreReleaseSummaryService(
                    new PersistenceHelper<ContentDbContext>(context),
                    MockUtils.AlwaysTrueUserService().Object);

                var viewModel = (await preReleaseSummaryService.GetPreReleaseSummaryViewModelAsync(releaseVersion.Id))
                    .Right;
                Assert.Equal(contact.TeamEmail, viewModel.ContactEmail);
                Assert.Equal(publication.Slug, viewModel.PublicationSlug);
                Assert.Equal(publication.Title, viewModel.PublicationTitle);
                Assert.Equal(releaseVersion.Title, viewModel.ReleaseTitle);
                Assert.Equal(releaseVersion.Slug, viewModel.ReleaseSlug);
            }
        }
    }
}
