#nullable enable
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Moq;
using IReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces.IReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers;

public abstract class AuthorizationHandlerServiceTests
{
    private readonly DataFixture _dataFixture = new();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _publicationId = Guid.NewGuid();
    private readonly ClaimsPrincipal _user;
    private readonly ReleaseVersion _releaseVersion;

    protected AuthorizationHandlerServiceTests()
    {
        _user = _dataFixture.AuthenticatedUser(_userId);

        _releaseVersion = _dataFixture.DefaultReleaseVersion().WithRelease(_dataFixture.DefaultRelease());
    }

    public class UserHasAnyPublicationRoleOnPublicationTests : AuthorizationHandlerServiceTests
    {
        [Fact]
        public async Task NoRolesSupplied_Throws()
        {
            var authorizationHandlerService = BuildService();

            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await authorizationHandlerService.UserHasAnyPublicationRoleOnPublication(
                    userId: _userId,
                    publicationId: _publicationId,
                    rolesToInclude: []
                )
            );
        }

        [Theory]
        [InlineData(PublicationRole.Drafter)]
        [InlineData(PublicationRole.Approver)]
        [InlineData(PublicationRole.Drafter, PublicationRole.Approver)]
        public async Task UserDoesNotHaveAnySuppliedRoleOnPublication_ReturnsFalse(
            params PublicationRole[] publicationRolesToInclude
        )
        {
            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>();
            userPublicationRoleRepository
                .Setup(mock =>
                    mock.UserHasAnyRoleOnPublication(
                        _userId,
                        _publicationId,
                        ResourceRoleFilter.ActiveOnly,
                        It.IsAny<CancellationToken>(),
                        publicationRolesToInclude
                    )
                )
                .ReturnsAsync(false);

            var authorizationHandlerService = BuildService(
                userPublicationRoleRepository: userPublicationRoleRepository.Object
            );

            var result = await authorizationHandlerService.UserHasAnyPublicationRoleOnPublication(
                userId: _userId,
                publicationId: _publicationId,
                rolesToInclude: [.. publicationRolesToInclude]
            );

            MockUtils.VerifyAllMocks(userPublicationRoleRepository);

            Assert.False(result);
        }

        [Theory]
        [InlineData(PublicationRole.Drafter)]
        [InlineData(PublicationRole.Approver)]
        [InlineData(PublicationRole.Drafter, PublicationRole.Approver)]
        public async Task UserHasAnySuppliedRoleOnPublication_ReturnsTrue(
            params PublicationRole[] publicationRolesToInclude
        )
        {
            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>();
            userPublicationRoleRepository
                .Setup(mock =>
                    mock.UserHasAnyRoleOnPublication(
                        _userId,
                        _publicationId,
                        ResourceRoleFilter.ActiveOnly,
                        It.IsAny<CancellationToken>(),
                        publicationRolesToInclude
                    )
                )
                .ReturnsAsync(true);

            var authorizationHandlerService = BuildService(
                userPublicationRoleRepository: userPublicationRoleRepository.Object
            );

            var result = await authorizationHandlerService.UserHasAnyPublicationRoleOnPublication(
                userId: _userId,
                publicationId: _publicationId,
                rolesToInclude: [.. publicationRolesToInclude]
            );

            MockUtils.VerifyAllMocks(userPublicationRoleRepository);

            Assert.True(result);
        }
    }

    public class UserHasAnyRoleOnPublicationTests : AuthorizationHandlerServiceTests
    {
        [Fact]
        public async Task UserDoesNotHaveAnyPublicationRoleOnPublication_AndHasNoReleaseRoleOnPublication_ReturnsFalse()
        {
            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>();
            userPublicationRoleRepository
                .Setup(mock =>
                    mock.UserHasAnyRoleOnPublication(
                        _userId,
                        _publicationId,
                        ResourceRoleFilter.ActiveOnly,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(false);

            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>();
            userPreReleaseRoleRepository
                .Setup(mock =>
                    mock.UserHasPreReleaseRoleOnPublication(
                        _userId,
                        _publicationId,
                        ResourceRoleFilter.ActiveOnly,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(false);

            var authorizationHandlerService = BuildService(
                userPublicationRoleRepository: userPublicationRoleRepository.Object,
                userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object
            );

            var result = await authorizationHandlerService.UserHasAnyRoleOnPublication(
                userId: _userId,
                publicationId: _publicationId
            );

            Assert.False(result);
        }

        [Fact]
        public async Task UserDoesNotHaveAnyPublicationRoleOnPublication_ButDoesHaveAnyReleaseRoleOnPublication_ReturnsTrue()
        {
            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>();
            userPublicationRoleRepository
                .Setup(mock =>
                    mock.UserHasAnyRoleOnPublication(
                        _userId,
                        _publicationId,
                        ResourceRoleFilter.ActiveOnly,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(false);

            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>();
            userPreReleaseRoleRepository
                .Setup(mock =>
                    mock.UserHasPreReleaseRoleOnPublication(
                        _userId,
                        _publicationId,
                        ResourceRoleFilter.ActiveOnly,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(true);

            var authorizationHandlerService = BuildService(
                userPublicationRoleRepository: userPublicationRoleRepository.Object,
                userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object
            );

            var result = await authorizationHandlerService.UserHasAnyRoleOnPublication(
                userId: _userId,
                publicationId: _publicationId
            );

            Assert.True(result);
        }

        [Fact]
        public async Task UserHasAnyPublicationRoleOnPublication_ButHasNoReleaseRoleOnPublication_ReturnsTrue()
        {
            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>();
            userPublicationRoleRepository
                .Setup(mock =>
                    mock.UserHasAnyRoleOnPublication(
                        _userId,
                        _publicationId,
                        ResourceRoleFilter.ActiveOnly,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(true);

            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>();
            userPreReleaseRoleRepository
                .Setup(mock =>
                    mock.UserHasPreReleaseRoleOnPublication(
                        _userId,
                        _publicationId,
                        ResourceRoleFilter.ActiveOnly,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(false);

            var authorizationHandlerService = BuildService(
                userPublicationRoleRepository: userPublicationRoleRepository.Object,
                userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object
            );

            var result = await authorizationHandlerService.UserHasAnyRoleOnPublication(
                userId: _userId,
                publicationId: _publicationId
            );

            Assert.True(result);
        }

        [Fact]
        public async Task UserHasAnyPublicationRoleOnPublication_AndHasAnyReleaseRoleOnPublication_ReturnsTrue()
        {
            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>();
            userPublicationRoleRepository
                .Setup(mock =>
                    mock.UserHasAnyRoleOnPublication(
                        _userId,
                        _publicationId,
                        ResourceRoleFilter.ActiveOnly,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(true);

            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>();
            userPreReleaseRoleRepository
                .Setup(mock =>
                    mock.UserHasPreReleaseRoleOnPublication(
                        _userId,
                        _publicationId,
                        ResourceRoleFilter.ActiveOnly,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(true);

            var authorizationHandlerService = BuildService(
                userPublicationRoleRepository: userPublicationRoleRepository.Object,
                userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object
            );

            var result = await authorizationHandlerService.UserHasAnyRoleOnPublication(
                userId: _userId,
                publicationId: _publicationId
            );

            Assert.True(result);
        }
    }

    public class UserHasPreReleaseRoleOnReleaseVersionTests : AuthorizationHandlerServiceTests
    {
        [Fact]
        public async Task UserDoesNotHaveAnySuppliedRoleOnPublication_ReturnsFalse()
        {
            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>();
            userPreReleaseRoleRepository
                .Setup(mock =>
                    mock.UserHasPreReleaseRoleOnReleaseVersion(
                        _userId,
                        _releaseVersion.Id,
                        ResourceRoleFilter.ActiveOnly,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(false);

            var authorizationHandlerService = BuildService(
                userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object
            );

            var result = await authorizationHandlerService.UserHasPreReleaseRoleOnReleaseVersion(
                userId: _userId,
                releaseVersionId: _releaseVersion.Id
            );

            MockUtils.VerifyAllMocks(userPreReleaseRoleRepository);

            Assert.False(result);
        }

        [Fact]
        public async Task UserHasAnySuppliedRoleOnPublication_ReturnsTrue()
        {
            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>();
            userPreReleaseRoleRepository
                .Setup(mock =>
                    mock.UserHasPreReleaseRoleOnReleaseVersion(
                        _userId,
                        _releaseVersion.Id,
                        ResourceRoleFilter.ActiveOnly,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(true);

            var authorizationHandlerService = BuildService(
                userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object
            );

            var result = await authorizationHandlerService.UserHasPreReleaseRoleOnReleaseVersion(
                userId: _userId,
                releaseVersionId: _releaseVersion.Id
            );

            MockUtils.VerifyAllMocks(userPreReleaseRoleRepository);

            Assert.True(result);
        }
    }

    public class IsReleaseVersionViewableByUserTests : AuthorizationHandlerServiceTests
    {
        [Fact]
        public async Task UserHasNoValidClaimsOrRoles()
        {
            var authorizationHandlerService = BuildService();

            var result = await authorizationHandlerService.IsReleaseVersionViewableByUser(_releaseVersion, _user);

            Assert.False(result);
        }

        [Fact]
        public async Task UserHasValidClaim_ReturnsTrue()
        {
            var user = _dataFixture
                .AuthenticatedUser(_userId)
                .WithClaim(SecurityClaimTypes.AccessAllReleases.ToString());

            var authorizationHandlerService = BuildService();

            var result = await authorizationHandlerService.IsReleaseVersionViewableByUser(_releaseVersion, user);

            Assert.True(result);
        }

        [Fact]
        public async Task UserHasValidPublicationRoleOnPublication_ReturnsTrue()
        {
            var userPublicationRoleRepositoryMock = new Mock<IUserPublicationRoleRepository>();
            userPublicationRoleRepositoryMock
                .Setup(mock =>
                    mock.UserHasAnyRoleOnPublication(
                        _userId,
                        _releaseVersion.Release.PublicationId,
                        ResourceRoleFilter.ActiveOnly,
                        It.IsAny<CancellationToken>(),
                        new[] { PublicationRole.Drafter, PublicationRole.Approver }
                    )
                )
                .ReturnsAsync(true);

            var authorizationHandlerService = BuildService(
                userPublicationRoleRepository: userPublicationRoleRepositoryMock.Object
            );

            var result = await authorizationHandlerService.IsReleaseVersionViewableByUser(_releaseVersion, _user);

            Assert.True(result);
        }

        [Fact]
        public async Task UserHasPreReleaseRoleOnReleaseVersion_ButWeAreNotInThePreReleaseWindow_ReturnsFalse()
        {
            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>();
            userPreReleaseRoleRepository
                .Setup(mock =>
                    mock.UserHasPreReleaseRoleOnReleaseVersion(
                        _userId,
                        _releaseVersion.Id,
                        ResourceRoleFilter.ActiveOnly,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(true);

            var authorizationHandlerService = BuildService(
                userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object
            );

            var result = await authorizationHandlerService.IsReleaseVersionViewableByUser(_releaseVersion, _user);

            Assert.False(result);
        }

        [Fact]
        public async Task UserDoesNotHavePreReleaseRoleOnReleaseVersion_ButWeInThePreReleaseWindow_ReturnsFalse()
        {
            var preReleaseService = new Mock<IPreReleaseService>();
            preReleaseService
                .Setup(mock => mock.GetPreReleaseWindowStatus(_releaseVersion, It.IsAny<DateTimeOffset>()))
                .Returns(new PreReleaseWindowStatus { Access = PreReleaseAccess.Within });

            var authorizationHandlerService = BuildService(preReleaseService: preReleaseService.Object);

            var result = await authorizationHandlerService.IsReleaseVersionViewableByUser(_releaseVersion, _user);

            Assert.False(result);
        }

        [Fact]
        public async Task UserHasPreReleaseRoleOnReleaseVersion_AndWeAreInThePreReleaseWindow_ReturnsTrue()
        {
            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>();
            userPreReleaseRoleRepository
                .Setup(mock =>
                    mock.UserHasPreReleaseRoleOnReleaseVersion(
                        _userId,
                        _releaseVersion.Id,
                        ResourceRoleFilter.ActiveOnly,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(true);

            var preReleaseService = new Mock<IPreReleaseService>();
            preReleaseService
                .Setup(mock => mock.GetPreReleaseWindowStatus(_releaseVersion, It.IsAny<DateTimeOffset>()))
                .Returns(new PreReleaseWindowStatus { Access = PreReleaseAccess.Within });

            var authorizationHandlerService = BuildService(
                userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object,
                preReleaseService: preReleaseService.Object
            );

            var result = await authorizationHandlerService.IsReleaseVersionViewableByUser(_releaseVersion, _user);

            Assert.True(result);
        }

        [Fact]
        public async Task IsLatestPublishedReleaseVersion_ReturnsTrue()
        {
            var releaseVersionRepository = new Mock<IReleaseVersionRepository>();
            releaseVersionRepository
                .Setup(mock => mock.IsLatestPublishedReleaseVersion(_releaseVersion.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var authorizationHandlerService = BuildService(releaseVersionRepository: releaseVersionRepository.Object);

            var result = await authorizationHandlerService.IsReleaseVersionViewableByUser(_releaseVersion, _user);

            Assert.True(result);
        }
    }

    private static AuthorizationHandlerService BuildService(
        IReleaseVersionRepository? releaseVersionRepository = null,
        IUserPreReleaseRoleRepository? userPreReleaseRoleRepository = null,
        IUserPublicationRoleRepository? userPublicationRoleRepository = null,
        IPreReleaseService? preReleaseService = null
    )
    {
        releaseVersionRepository ??= CreateDefaultReleaseVersionRepository();
        userPreReleaseRoleRepository ??= CreateDefaultuserPreReleaseRoleRepository();
        userPublicationRoleRepository ??= CreateDefaultUserPublicationRoleRepository();
        preReleaseService ??= CreateDefaultPreReleaseService();

        return new(
            releaseVersionRepository,
            userPreReleaseRoleRepository,
            userPublicationRoleRepository,
            preReleaseService
        );
    }

    private static IReleaseVersionRepository CreateDefaultReleaseVersionRepository()
    {
        var releaseVersionRepository = new Mock<IReleaseVersionRepository>();
        releaseVersionRepository
            .Setup(mock => mock.IsLatestPublishedReleaseVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        return releaseVersionRepository.Object;
    }

    private static IUserPreReleaseRoleRepository CreateDefaultuserPreReleaseRoleRepository()
    {
        var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>();
        userPreReleaseRoleRepository
            .Setup(mock =>
                mock.UserHasPreReleaseRoleOnPublication(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    ResourceRoleFilter.ActiveOnly,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(false);
        userPreReleaseRoleRepository
            .Setup(mock =>
                mock.UserHasPreReleaseRoleOnReleaseVersion(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    ResourceRoleFilter.ActiveOnly,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(false);

        return userPreReleaseRoleRepository.Object;
    }

    private static IUserPublicationRoleRepository CreateDefaultUserPublicationRoleRepository()
    {
        var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>();
        userPublicationRoleRepository
            .Setup(mock =>
                mock.UserHasAnyRoleOnPublication(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    ResourceRoleFilter.ActiveOnly,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(false);

        return userPublicationRoleRepository.Object;
    }

    private static IPreReleaseService CreateDefaultPreReleaseService()
    {
        var preReleaseService = new Mock<IPreReleaseService>();
        preReleaseService
            .Setup(mock => mock.GetPreReleaseWindowStatus(It.IsAny<ReleaseVersion>(), It.IsAny<DateTimeOffset>()))
            .Returns(new PreReleaseWindowStatus { Access = PreReleaseAccess.NoneSet });

        return preReleaseService.Object;
    }
}
