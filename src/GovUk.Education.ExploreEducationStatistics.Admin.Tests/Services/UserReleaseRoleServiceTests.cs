#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using MockQueryable;
using Moq;
using IReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces.IReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class UserReleaseRoleServiceTests
{
    private readonly DataFixture _dataFixture = new();

    [Fact]
    public async Task ListLatestUserReleaseRolesByPublication()
    {
        var (publication, publicationIgnored) = _dataFixture
            .DefaultPublication()
            .WithReleases(_dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true).Generate(2))
            .GenerateTuple2();

        UserReleaseRole userReleaseRole1 = _dataFixture
            .DefaultUserReleaseRole()
            .WithUser(_dataFixture.DefaultUser())
            .WithReleaseVersion(publication.Releases[0].Versions[0])
            .WithRole(ReleaseRole.Contributor);
        UserReleaseRole userReleaseRole2 = _dataFixture
            .DefaultUserReleaseRole()
            .WithUser(_dataFixture.DefaultUser())
            .WithReleaseVersion(publication.Releases[0].Versions[0])
            .WithRole(ReleaseRole.Contributor);
        UserReleaseRole userReleaseRole3 = _dataFixture
            .DefaultUserReleaseRole()
            .WithUser(_dataFixture.DefaultUser())
            .WithReleaseVersion(publication.Releases[1].Versions[0])
            .WithRole(ReleaseRole.Contributor);

        // Should be ignored because not Contributor role
        UserReleaseRole userReleaseRoleIgnored1 = _dataFixture
            .DefaultUserReleaseRole()
            .WithUser(_dataFixture.DefaultUser())
            .WithReleaseVersion(publication.Releases[0].Versions[0])
            .WithRole(ReleaseRole.Approver);
        // Should be ignored due to release under different publication
        UserReleaseRole userReleaseRoleIgnored2 = _dataFixture
            .DefaultUserReleaseRole()
            .WithUser(_dataFixture.DefaultUser())
            .WithReleaseVersion(publicationIgnored.Releases[0].Versions[0])
            .WithRole(ReleaseRole.Contributor);

        var releaseVersionRepository = new Mock<IReleaseVersionRepository>(MockBehavior.Strict);
        releaseVersionRepository
            .Setup(m => m.ListLatestReleaseVersionIds(publication.Id, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync([publication.Releases[0].Versions[0].Id, publication.Releases[1].Versions[0].Id]);

        var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(MockBehavior.Strict);
        userReleaseRoleRepository.SetupQuery(
            ResourceRoleFilter.ActiveOnly,
            [userReleaseRole1, userReleaseRole2, userReleaseRole3, userReleaseRoleIgnored1, userReleaseRoleIgnored2]
        );

        var service = BuildService(
            userReleaseRoleRepository: userReleaseRoleRepository.Object,
            releaseVersionRepository: releaseVersionRepository.Object
        );
        var latestActiveUserReleaseRoles = await service.ListLatestActiveUserReleaseRolesByPublication(
            publication.Id,
            ReleaseRole.Contributor
        );

        Assert.Equal(3, latestActiveUserReleaseRoles.Count);

        Assert.Equal(userReleaseRole1.Id, latestActiveUserReleaseRoles[0].Id);
        Assert.Equal(userReleaseRole1.UserId, latestActiveUserReleaseRoles[0].UserId);
        Assert.Equal(userReleaseRole1.ReleaseVersionId, latestActiveUserReleaseRoles[0].ReleaseVersionId);
        Assert.Equal(userReleaseRole1.Role, latestActiveUserReleaseRoles[0].Role);

        Assert.Equal(userReleaseRole2.Id, latestActiveUserReleaseRoles[1].Id);
        Assert.Equal(userReleaseRole2.UserId, latestActiveUserReleaseRoles[1].UserId);
        Assert.Equal(userReleaseRole2.ReleaseVersionId, latestActiveUserReleaseRoles[1].ReleaseVersionId);
        Assert.Equal(userReleaseRole2.Role, latestActiveUserReleaseRoles[1].Role);

        Assert.Equal(userReleaseRole3.Id, latestActiveUserReleaseRoles[2].Id);
        Assert.Equal(userReleaseRole3.UserId, latestActiveUserReleaseRoles[2].UserId);
        Assert.Equal(userReleaseRole3.ReleaseVersionId, latestActiveUserReleaseRoles[2].ReleaseVersionId);
        Assert.Equal(userReleaseRole3.Role, latestActiveUserReleaseRoles[2].Role);

        MockUtils.VerifyAllMocks(userReleaseRoleRepository, releaseVersionRepository);
    }

    [Fact]
    public async Task ListLatestUserReleaseRolesByPublication_EmptyRolesToInclude_IncludesAllRolesTypes()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true)]);

        var userReleaseRoles = _dataFixture
            .DefaultUserReleaseRole()
            .WithUser(_dataFixture.DefaultUser())
            .WithReleaseVersion(publication.Releases[0].Versions[0])
            .ForIndex(0, s => s.SetRole(ReleaseRole.Approver))
            .ForIndex(1, s => s.SetRole(ReleaseRole.Contributor))
            .ForIndex(2, s => s.SetRole(ReleaseRole.PrereleaseViewer))
            .GenerateList(3);

        var releaseVersionRepository = new Mock<IReleaseVersionRepository>(MockBehavior.Strict);
        releaseVersionRepository
            .Setup(m => m.ListLatestReleaseVersionIds(publication.Id, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync([publication.Releases[0].Versions[0].Id]);

        var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(MockBehavior.Strict);
        userReleaseRoleRepository.SetupQuery(ResourceRoleFilter.ActiveOnly, [.. userReleaseRoles]);

        var service = BuildService(
            userReleaseRoleRepository: userReleaseRoleRepository.Object,
            releaseVersionRepository: releaseVersionRepository.Object
        );
        var latestActiveUserReleaseRoles = await service.ListLatestActiveUserReleaseRolesByPublication(publication.Id);

        Assert.Equal(3, userReleaseRoles.Count);

        MockUtils.VerifyAllMocks(userReleaseRoleRepository, releaseVersionRepository);
    }

    private static UserReleaseRoleService BuildService(
        IReleaseVersionRepository? releaseVersionRepository = null,
        IUserReleaseRoleRepository? userReleaseRoleRepository = null
    )
    {
        return new(
            releaseVersionRepository ?? Mock.Of<IReleaseVersionRepository>(MockBehavior.Strict),
            userReleaseRoleRepository ?? Mock.Of<IUserReleaseRoleRepository>(MockBehavior.Strict)
        );
    }
}
