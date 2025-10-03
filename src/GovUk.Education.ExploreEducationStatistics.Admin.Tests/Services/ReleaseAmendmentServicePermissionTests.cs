#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class ReleaseAmendmentServicePermissionTests
{
    private readonly DataFixture _fixture = new();

    [Fact]
    public async Task CreateReleaseAmendment()
    {
        Publication publication = _fixture
            .DefaultPublication()
            .WithReleases(_fixture.DefaultRelease(publishedVersions: 1).Generate(1));

        var releaseVersion = publication.Releases.Single().Versions.Single();

        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(releaseVersion, CanMakeAmendmentOfSpecificReleaseVersion)
            .AssertForbidden(userService =>
            {
                using var contentDbContext = InMemoryApplicationDbContext();
                contentDbContext.Publications.Add(publication);
                contentDbContext.SaveChanges();

                var service = BuildService(userService.Object, contentDbContext);
                return service.CreateReleaseAmendment(releaseVersion.Id);
            });
    }

    private ReleaseAmendmentService BuildService(IUserService userService, ContentDbContext? context = null)
    {
        return new ReleaseAmendmentService(
            context ?? Mock.Of<ContentDbContext>(),
            userService,
            Mock.Of<IFootnoteRepository>(MockBehavior.Strict),
            Mock.Of<StatisticsDbContext>(MockBehavior.Strict)
        );
    }
}
