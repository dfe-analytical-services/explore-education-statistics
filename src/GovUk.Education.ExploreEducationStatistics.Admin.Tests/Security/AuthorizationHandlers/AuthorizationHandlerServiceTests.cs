using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Moq;
using IReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces.IReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers;

public class AuthorizationHandlerServiceTests
{
    private readonly DataFixture _fixture = new();

    public class IsReleaseVersionViewableByUserTests : AuthorizationHandlerServiceTests
    {
        [Theory]
        [InlineData(PublicationRole.Owner)]
        [InlineData(PublicationRole.Allower)]
        public async Task HasValidRoleOnPublication_ReturnsTrue(PublicationRole publicationRole)
        {
            var releaseVersion = _fixture.DefaultReleaseVersion();

            var user = _fixture.AuthenticatedUser();

            var userPublicationRoleAndInviteManagerMock = new Mock<IUserPublicationRoleRepository>();
            userPublicationRoleAndInviteManagerMock
                .Setup(rvr => rvr.GetAllRolesByUserAndPublication(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync([publicationRole]);

            var authorizationHandlerService = CreateService(
                userPublicationRoleRepository: userPublicationRoleAndInviteManagerMock.Object);

            var result = await authorizationHandlerService.IsReleaseVersionViewableByUser(releaseVersion, user);

            Assert.True(result);
        }

        [Theory]
        [InlineData(PublicationRole.Approver)]
        [InlineData(PublicationRole.Drafter)]
        public async Task HasInvalidRoleOnPublication_ReturnsFalse(PublicationRole publicationRole)
        {
            var releaseVersion = _fixture.DefaultReleaseVersion();

            var user = _fixture.AuthenticatedUser();

            var userPublicationRoleAndInviteManagerMock = new Mock<IUserPublicationRoleRepository>();
            userPublicationRoleAndInviteManagerMock
                .Setup(rvr => rvr.GetAllRolesByUserAndPublication(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync([publicationRole]);

            var authorizationHandlerService = CreateService(
                userPublicationRoleRepository: userPublicationRoleAndInviteManagerMock.Object);

            var result = await authorizationHandlerService.IsReleaseVersionViewableByUser(releaseVersion, user);

            Assert.False(result);
        }
    }

    private static AuthorizationHandlerService CreateService(
        IReleaseVersionRepository? releaseVersionRepository = null,
        IUserReleaseRoleRepository? userReleaseRoleRepository = null,
        IUserPublicationRoleRepository? userPublicationRoleRepository = null,
        IPreReleaseService? preReleaseService = null)
    {
        var releaseVersionRepositoryMock = new Mock<IReleaseVersionRepository>();
        releaseVersionRepositoryMock
            .Setup(rvr => rvr.IsLatestPublishedReleaseVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var userReleaseRoleAndInviteManagerMock = new Mock<IUserReleaseRoleRepository>();
        userReleaseRoleAndInviteManagerMock
            .Setup(rvr => rvr.GetAllRolesByUserAndReleaseVersion(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync([]);
        userReleaseRoleAndInviteManagerMock
            .Setup(rvr => rvr.GetAllRolesByUserAndPublication(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync([]);

        var userPublicationRoleAndInviteManagerMock = new Mock<IUserPublicationRoleRepository>();
        userPublicationRoleAndInviteManagerMock
            .Setup(rvr => rvr.GetAllRolesByUserAndPublication(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync([]);

        var preReleaseServiceMock = new Mock<IPreReleaseService>();
        preReleaseServiceMock
            .Setup(rvr => rvr.GetPreReleaseWindowStatus(It.IsAny<ReleaseVersion>(), It.IsAny<DateTime>()))
            .Returns(new PreReleaseWindowStatus
            {
                Access = PreReleaseAccess.NoneSet,
            });

        return new AuthorizationHandlerService(
            releaseVersionRepository ?? releaseVersionRepositoryMock.Object,
            userReleaseRoleRepository ?? userReleaseRoleAndInviteManagerMock.Object,
            userPublicationRoleRepository ?? userPublicationRoleAndInviteManagerMock.Object,
            preReleaseService ?? preReleaseServiceMock.Object);
    }
}
