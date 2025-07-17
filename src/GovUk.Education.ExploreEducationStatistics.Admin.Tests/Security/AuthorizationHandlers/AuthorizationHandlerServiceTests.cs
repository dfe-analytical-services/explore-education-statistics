using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;

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

            var userPublicationRoleRepositoryMock = new Mock<IUserPublicationRoleRepository>();
            userPublicationRoleRepositoryMock
                .Setup(rvr => rvr.GetAllRolesByUserAndPublication(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync([publicationRole]);

            var authorizationHandlerService = CreateService(
                userPublicationRoleRepository: userPublicationRoleRepositoryMock.Object);

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

            var userPublicationRoleRepositoryMock = new Mock<IUserPublicationRoleRepository>();
            userPublicationRoleRepositoryMock
                .Setup(rvr => rvr.GetAllRolesByUserAndPublication(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync([publicationRole]);

            var authorizationHandlerService = CreateService(
                userPublicationRoleRepository: userPublicationRoleRepositoryMock.Object);

            var result = await authorizationHandlerService.IsReleaseVersionViewableByUser(releaseVersion, user);

            Assert.False(result);
        }
    }

    private static AuthorizationHandlerService CreateService(
        Content.Model.Repository.Interfaces.IReleaseVersionRepository? releaseVersionRepository = null,
        IUserReleaseRoleRepository? userReleaseRoleRepository = null,
        IUserPublicationRoleRepository? userPublicationRoleRepository = null,
        IPreReleaseService? preReleaseService = null)
    {
        var releaseVersionRepositoryMock = new Mock<Content.Model.Repository.Interfaces.IReleaseVersionRepository>();
        releaseVersionRepositoryMock
            .Setup(rvr => rvr.IsLatestPublishedReleaseVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var userReleaseRoleRepositoryMock = new Mock<IUserReleaseRoleRepository>();
        userReleaseRoleRepositoryMock
            .Setup(rvr => rvr.GetAllRolesByUserAndRelease(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync([]);
        userReleaseRoleRepositoryMock
            .Setup(rvr => rvr.GetAllRolesByUserAndPublication(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync([]);

        var userPublicationRoleRepositoryMock = new Mock<IUserPublicationRoleRepository>();
        userPublicationRoleRepositoryMock
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
            userReleaseRoleRepository ?? userReleaseRoleRepositoryMock.Object,
            userPublicationRoleRepository ?? userPublicationRoleRepositoryMock.Object,
            preReleaseService ?? preReleaseServiceMock.Object);
    }
}
