#nullable enable
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
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
        [Fact]
        public async Task HasValidRoleOnPublication_ReturnsTrue()
        {
            ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());

            ClaimsPrincipal user = _fixture.AuthenticatedUser();

            var userPublicationRoleRepositoryMock = new Mock<IUserPublicationRoleRepository>();
            userPublicationRoleRepositoryMock
                .Setup(mock =>
                    mock.UserHasAnyRoleOnPublication(
                        user.GetUserId(),
                        releaseVersion.Release.PublicationId,
                        ResourceRoleFilter.ActiveOnly,
                        It.IsAny<CancellationToken>(),
                        new[] { PublicationRole.Owner, PublicationRole.Allower }
                    )
                )
                .ReturnsAsync(true);

            var authorizationHandlerService = CreateService(
                userPublicationRoleRepository: userPublicationRoleRepositoryMock.Object
            );

            var result = await authorizationHandlerService.IsReleaseVersionViewableByUser(releaseVersion, user);

            Assert.True(result);
        }

        [Fact]
        public async Task HasInvalidRoleOnPublication_ReturnsFalse()
        {
            ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());

            ClaimsPrincipal user = _fixture.AuthenticatedUser();

            var userPublicationRoleAndInviteManagerMock = new Mock<IUserPublicationRoleRepository>();
            userPublicationRoleAndInviteManagerMock
                .Setup(mock =>
                    mock.UserHasAnyRoleOnPublication(
                        user.GetUserId(),
                        releaseVersion.Release.PublicationId,
                        ResourceRoleFilter.ActiveOnly,
                        It.IsAny<CancellationToken>(),
                        new[] { PublicationRole.Owner, PublicationRole.Allower }
                    )
                )
                .ReturnsAsync(false);

            var authorizationHandlerService = CreateService(
                userPublicationRoleRepository: userPublicationRoleAndInviteManagerMock.Object
            );

            var result = await authorizationHandlerService.IsReleaseVersionViewableByUser(releaseVersion, user);

            Assert.False(result);
        }
    }

    private static AuthorizationHandlerService CreateService(
        IReleaseVersionRepository? releaseVersionRepository = null,
        IUserReleaseRoleRepository? userReleaseRoleRepository = null,
        IUserPublicationRoleRepository? userPublicationRoleRepository = null,
        IPreReleaseService? preReleaseService = null
    )
    {
        var releaseVersionRepositoryMock = new Mock<IReleaseVersionRepository>();
        releaseVersionRepositoryMock
            .Setup(mock => mock.IsLatestPublishedReleaseVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var userReleaseRoleRepositoryMock = new Mock<IUserReleaseRoleRepository>();
        userReleaseRoleRepositoryMock
            .Setup(mock =>
                mock.UserHasAnyRoleOnPublication(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    ResourceRoleFilter.ActiveOnly,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(false);
        userReleaseRoleRepositoryMock
            .Setup(mock =>
                mock.UserHasAnyRoleOnReleaseVersion(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    ResourceRoleFilter.ActiveOnly,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(false);

        var userPublicationRoleRepositoryMock = new Mock<IUserPublicationRoleRepository>();
        userPublicationRoleRepositoryMock
            .Setup(mock =>
                mock.UserHasAnyRoleOnPublication(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    ResourceRoleFilter.ActiveOnly,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(false);

        var preReleaseServiceMock = new Mock<IPreReleaseService>();
        preReleaseServiceMock
            .Setup(mock => mock.GetPreReleaseWindowStatus(It.IsAny<ReleaseVersion>(), It.IsAny<DateTimeOffset>()))
            .Returns(new PreReleaseWindowStatus { Access = PreReleaseAccess.NoneSet });

        return new AuthorizationHandlerService(
            releaseVersionRepository ?? releaseVersionRepositoryMock.Object,
            userReleaseRoleRepository ?? userReleaseRoleRepositoryMock.Object,
            userPublicationRoleRepository ?? userPublicationRoleRepositoryMock.Object,
            preReleaseService ?? preReleaseServiceMock.Object
        );
    }
}
