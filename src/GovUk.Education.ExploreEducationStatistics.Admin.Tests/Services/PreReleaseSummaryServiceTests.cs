#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class PreReleaseSummaryServiceTests
{
    private readonly DataFixture _dataFixture = new();

    [Fact]
    public async Task GetPreReleaseSummaryViewModel()
    {
        ReleaseVersion releaseVersion = _dataFixture
            .DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = BuildService(contentDbContext);

            var result = await service.GetPreReleaseSummaryViewModel(releaseVersion.Id, CancellationToken.None);

            var viewModel = result.AssertRight();

            Assert.Equal(releaseVersion.Release.Publication.Contact.TeamEmail, viewModel.ContactEmail);
            Assert.Equal(releaseVersion.Release.Publication.Contact.TeamName, viewModel.ContactTeam);
            Assert.Equal(releaseVersion.Release.Publication.Slug, viewModel.PublicationSlug);
            Assert.Equal(releaseVersion.Release.Publication.Title, viewModel.PublicationTitle);
            Assert.Equal(releaseVersion.Release.Slug, viewModel.ReleaseSlug);
            Assert.Equal(releaseVersion.Release.Title, viewModel.ReleaseTitle);
        }
    }

    private static PreReleaseSummaryService BuildService(
        ContentDbContext? contentDbContext = null,
        UserService? userService = null
    )
    {
        return new PreReleaseSummaryService(
            contentDbContext ?? Mock.Of<ContentDbContext>(),
            userService ?? MockUtils.AlwaysTrueUserService().Object
        );
    }
}
