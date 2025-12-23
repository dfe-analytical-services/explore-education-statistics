#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyApprovalStatus;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers;

// ReSharper disable once ClassNeverInstantiated.Global
public class UpdateSpecificMethodologyAuthorizationHandlerTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private static readonly MethodologyVersion MethodologyVersion = new()
    {
        Id = Guid.NewGuid(),
        MethodologyId = Guid.NewGuid(),
    };

    private static readonly Publication OwningPublication = new() { Id = Guid.NewGuid() };

    private static readonly DataFixture DataFixture = new();

    public class ClaimsTests
    {
        [Fact]
        public async Task NoClaimsAllowUpdatingApprovedMethodology()
        {
            var methodologyVersion = new MethodologyVersion { Id = Guid.NewGuid(), Status = Approved };

            await ForEachSecurityClaimAsync(async claim =>
            {
                var (handler, _, _, _) = CreateHandlerAndDependencies();

                var user = DataFixture.AuthenticatedUser(userId: UserId).WithClaim(claim.ToString());

                var authContext = CreateAuthorizationHandlerContext<
                    UpdateSpecificMethodologyRequirement,
                    MethodologyVersion
                >(user, methodologyVersion);

                await handler.HandleAsync(authContext);

                Assert.False(authContext.HasSucceeded);
            });
        }
    }

    public class PublicationRoleTests
    {
        [Fact]
        public async Task PublicationOwnersAndApproversCanUpdateMethodology()
        {
            await ForEachPublicationRoleAsync(async publicationRole =>
            {
                var isApproverOrOwner = publicationRole is PublicationRole.Owner or PublicationRole.Allower;

                var (handler, methodologyRepository, userPublicationRoleRepository, userReleaseRoleRepository) =
                    CreateHandlerAndDependencies();

                methodologyRepository
                    .Setup(s => s.GetOwningPublication(MethodologyVersion.MethodologyId))
                    .ReturnsAsync(OwningPublication);

                userPublicationRoleRepository
                    .Setup(rvr =>
                        rvr.UserHasAnyRoleOnPublication(
                            UserId,
                            OwningPublication.Id,
                            ResourceRoleFilter.ActiveOnly,
                            It.IsAny<CancellationToken>(),
                            new[] { PublicationRole.Owner, PublicationRole.Allower }
                        )
                    )
                    .ReturnsAsync(isApproverOrOwner);

                if (!isApproverOrOwner)
                {
                    userReleaseRoleRepository
                        .Setup(rvr =>
                            rvr.UserHasAnyRoleOnPublication(
                                UserId,
                                OwningPublication.Id,
                                ResourceRoleFilter.ActiveOnly,
                                It.IsAny<CancellationToken>(),
                                AuthorizationHandlerService.ReleaseEditorAndApproverRoles.ToArray()
                            )
                        )
                        .ReturnsAsync(false);
                }

                var user = DataFixture.AuthenticatedUser(userId: UserId);

                var authContext = CreateAuthorizationHandlerContext<
                    UpdateSpecificMethodologyRequirement,
                    MethodologyVersion
                >(user, MethodologyVersion);

                await handler.HandleAsync(authContext);

                VerifyAllMocks(methodologyRepository, userReleaseRoleRepository, userPublicationRoleRepository);

                Assert.Equal(isApproverOrOwner, authContext.HasSucceeded);
            });
        }
    }

    public class ReleaseRoleTests
    {
        [Fact]
        public async Task EditorsOrApproversOnAnyOwningPublicationReleaseCanUpdateMethodology()
        {
            await ForEachReleaseRoleAsync(async releaseRole =>
            {
                var isEditorOrApprover = AuthorizationHandlerService.ReleaseEditorAndApproverRoles.Contains(
                    releaseRole
                );

                var (handler, methodologyRepository, userPublicationRoleRepository, userReleaseRoleRepository) =
                    CreateHandlerAndDependencies();

                methodologyRepository
                    .Setup(s => s.GetOwningPublication(MethodologyVersion.MethodologyId))
                    .ReturnsAsync(OwningPublication);

                userPublicationRoleRepository
                    .Setup(rvr =>
                        rvr.UserHasAnyRoleOnPublication(
                            UserId,
                            OwningPublication.Id,
                            ResourceRoleFilter.ActiveOnly,
                            It.IsAny<CancellationToken>(),
                            new[] { PublicationRole.Owner, PublicationRole.Allower }
                        )
                    )
                    .ReturnsAsync(false);

                userReleaseRoleRepository
                    .Setup(rvr =>
                        rvr.UserHasAnyRoleOnPublication(
                            UserId,
                            OwningPublication.Id,
                            ResourceRoleFilter.ActiveOnly,
                            It.IsAny<CancellationToken>(),
                            AuthorizationHandlerService.ReleaseEditorAndApproverRoles.ToArray()
                        )
                    )
                    .ReturnsAsync(isEditorOrApprover);

                var user = DataFixture.AuthenticatedUser(userId: UserId);

                var authContext = CreateAuthorizationHandlerContext<
                    UpdateSpecificMethodologyRequirement,
                    MethodologyVersion
                >(user, MethodologyVersion);

                await handler.HandleAsync(authContext);

                VerifyAllMocks(methodologyRepository, userPublicationRoleRepository, userReleaseRoleRepository);

                Assert.Equal(isEditorOrApprover, authContext.HasSucceeded);
            });
        }

        [Fact]
        public async Task NoReleaseRolesOnOwningPublicationReleasesSoUsersCannotUpdateMethodology()
        {
            var (handler, methodologyRepository, userPublicationRoleRepository, userReleaseRoleRepository) =
                CreateHandlerAndDependencies();

            methodologyRepository
                .Setup(s => s.GetOwningPublication(MethodologyVersion.MethodologyId))
                .ReturnsAsync(OwningPublication);

            userPublicationRoleRepository
                .Setup(rvr =>
                    rvr.UserHasAnyRoleOnPublication(
                        UserId,
                        OwningPublication.Id,
                        ResourceRoleFilter.ActiveOnly,
                        It.IsAny<CancellationToken>(),
                        new[] { PublicationRole.Owner, PublicationRole.Allower }
                    )
                )
                .ReturnsAsync(false);

            userReleaseRoleRepository
                .Setup(rvr =>
                    rvr.UserHasAnyRoleOnPublication(
                        UserId,
                        OwningPublication.Id,
                        ResourceRoleFilter.ActiveOnly,
                        It.IsAny<CancellationToken>(),
                        AuthorizationHandlerService.ReleaseEditorAndApproverRoles.ToArray()
                    )
                )
                .ReturnsAsync(false);

            var user = DataFixture.AuthenticatedUser(userId: UserId);

            var authContext = CreateAuthorizationHandlerContext<
                UpdateSpecificMethodologyRequirement,
                MethodologyVersion
            >(user, MethodologyVersion);

            await handler.HandleAsync(authContext);

            VerifyAllMocks(methodologyRepository, userPublicationRoleRepository, userReleaseRoleRepository);

            Assert.False(authContext.HasSucceeded);
        }
    }

    private static (
        UpdateSpecificMethodologyAuthorizationHandler,
        Mock<IMethodologyRepository>,
        Mock<IUserPublicationRoleRepository>,
        Mock<IUserReleaseRoleRepository>
    ) CreateHandlerAndDependencies()
    {
        var methodologyRepository = new Mock<IMethodologyRepository>(Strict);
        var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
        var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(Strict);

        var handler = new UpdateSpecificMethodologyAuthorizationHandler(
            methodologyRepository.Object,
            new AuthorizationHandlerService(
                new ReleaseVersionRepository(InMemoryApplicationDbContext()),
                userReleaseRoleRepository.Object,
                userPublicationRoleRepository.Object,
                Mock.Of<IPreReleaseService>(Strict)
            )
        );

        return (handler, methodologyRepository, userPublicationRoleRepository, userReleaseRoleRepository);
    }
}
