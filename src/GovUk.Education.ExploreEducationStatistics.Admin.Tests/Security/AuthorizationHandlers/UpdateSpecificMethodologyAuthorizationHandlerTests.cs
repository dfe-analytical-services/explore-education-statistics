#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.
    AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseRole;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class UpdateSpecificMethodologyAuthorizationHandlerTests
    {
        private static readonly Guid UserId = Guid.NewGuid();

        private static readonly Methodology Methodology = new()
        {
            Id = Guid.NewGuid(),
            MethodologyParentId = Guid.NewGuid()
        };

        private static readonly Publication OwningPublication = new()
        {
            Id = Guid.NewGuid()
        };

        public class ClaimsTests
        {
            [Fact]
            public async Task NoClaimsAllowUpdatingApprovedMethodology()
            {
                var methodology = new Methodology
                {
                    Id = Guid.NewGuid(),
                    Status = Approved
                };

                await ForEachSecurityClaimAsync(async claim =>
                {
                    var (handler, methodologyRepository, _, _) = CreateHandlerAndDependencies();

                    var user = CreateClaimsPrincipal(UserId, claim);
                    var authContext =
                        CreateAuthorizationHandlerContext<UpdateSpecificMethodologyRequirement, Methodology>
                            (user, methodology);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(methodologyRepository);

                    // No claims should allow an Approved Methodology to be updated
                    Assert.False(authContext.HasSucceeded);
                });
            }

            [Fact]
            public async Task NoClaimsAllowUpdatingPubliclyAccessibleMethodology()
            {
                await ForEachSecurityClaimAsync(async claim =>
                {
                    var (handler, methodologyRepository, _, _) = CreateHandlerAndDependencies();

                    methodologyRepository.Setup(mock => mock.IsPubliclyAccessible(Methodology.Id))
                        .ReturnsAsync(true);

                    var user = CreateClaimsPrincipal(UserId, claim);
                    var authContext =
                        CreateAuthorizationHandlerContext<UpdateSpecificMethodologyRequirement, Methodology>
                            (user, Methodology);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(methodologyRepository);

                    // No claims should allow a publicly accessible Methodology to be updated
                    Assert.False(authContext.HasSucceeded);
                });
            }

            [Fact]
            public async Task UserWithCorrectClaimCanUpdateNonPubliclyAccessibleMethodology()
            {
                await ForEachSecurityClaimAsync(async claim =>
                {
                    var (
                        handler,
                        methodologyRepository,
                        userPublicationRoleRepository,
                        userReleaseRoleRepository
                        ) = CreateHandlerAndDependencies();

                    methodologyRepository.Setup(mock => mock.IsPubliclyAccessible(Methodology.Id))
                        .ReturnsAsync(false);

                    // Only the UpdateAllMethodologies claim should allow a non publicly accessible Methodology
                    // to be updated.
                    var expectedToPassByClaimAlone = claim == UpdateAllMethodologies;

                    if (!expectedToPassByClaimAlone)
                    {
                        methodologyRepository.Setup(s =>
                                s.GetOwningPublicationByMethodologyParent(Methodology.MethodologyParentId))
                            .ReturnsAsync(OwningPublication);

                        userPublicationRoleRepository.SetupPublicationOwnerRoleExpectations(
                            UserId, OwningPublication, false);

                        userReleaseRoleRepository
                            .Setup(s => s.IsUserEditorOrApproverOnLatestRelease(UserId, OwningPublication.Id))
                            .ReturnsAsync(false);
                    }

                    var user = CreateClaimsPrincipal(UserId, claim);
                    var authContext =
                        CreateAuthorizationHandlerContext<UpdateSpecificMethodologyRequirement, Methodology>
                            (user, Methodology);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(methodologyRepository, userPublicationRoleRepository, userReleaseRoleRepository);

                    Assert.Equal(expectedToPassByClaimAlone, authContext.HasSucceeded);
                });
            }
        }

        public class PublicationRoleTests
        {
            [Fact]
            public async Task PublicationOwnerCanUpdateMethodology()
            {
                await ForEachPublicationRoleAsync(async publicationRole =>
                {
                    var (
                        handler,
                        methodologyRepository,
                        userPublicationRoleRepository,
                        userReleaseRoleRepository
                        ) = CreateHandlerAndDependencies();

                    methodologyRepository.Setup(mock => mock.IsPubliclyAccessible(Methodology.Id))
                        .ReturnsAsync(false);

                    methodologyRepository.Setup(s =>
                            s.GetOwningPublicationByMethodologyParent(Methodology.MethodologyParentId))
                        .ReturnsAsync(OwningPublication);

                    userPublicationRoleRepository.SetupPublicationOwnerRoleExpectations(
                        UserId, OwningPublication, publicationRole == Owner);

                    var expectedToPassByRole = publicationRole == Owner;

                    if (!expectedToPassByRole)
                    {
                        userReleaseRoleRepository
                            .Setup(s => s.IsUserEditorOrApproverOnLatestRelease(UserId, OwningPublication.Id))
                            .ReturnsAsync(false);
                    }

                    var user = CreateClaimsPrincipal(UserId);
                    var authContext =
                        CreateAuthorizationHandlerContext<UpdateSpecificMethodologyRequirement, Methodology>
                            (user, Methodology);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(methodologyRepository, userPublicationRoleRepository, userReleaseRoleRepository);

                    // As the user has Publication Owner role on the owning Publication of this Methodology, they are
                    // allowed to update it.
                    Assert.Equal(expectedToPassByRole, authContext.HasSucceeded);
                });
            }
        }

        public class ReleaseRoleTests
        {
            [Fact]
            public async Task EditorsOrApproversOnOwningPublicationsLatestReleaseCanUpdateMethodology()
            {
                var expectedReleaseRolesToPass = ListOf(Approver, Contributor, Lead);

                await ForEachReleaseRoleAsync(async releaseRole =>
                {
                    var (
                        handler,
                        methodologyRepository,
                        userPublicationRoleRepository,
                        userReleaseRoleRepository
                        ) = CreateHandlerAndDependencies();

                    methodologyRepository.Setup(mock => mock.IsPubliclyAccessible(Methodology.Id))
                        .ReturnsAsync(false);

                    methodologyRepository.Setup(s =>
                            s.GetOwningPublicationByMethodologyParent(Methodology.MethodologyParentId))
                        .ReturnsAsync(OwningPublication);

                    userPublicationRoleRepository.SetupPublicationOwnerRoleExpectations(
                        UserId, OwningPublication, false);

                    userReleaseRoleRepository
                        .Setup(s => s.IsUserEditorOrApproverOnLatestRelease(UserId, OwningPublication.Id))
                        .ReturnsAsync(expectedReleaseRolesToPass.Contains(releaseRole));

                    var user = CreateClaimsPrincipal(UserId);
                    var authContext =
                        CreateAuthorizationHandlerContext<UpdateSpecificMethodologyRequirement, Methodology>
                            (user, Methodology);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(
                        methodologyRepository,
                        userPublicationRoleRepository,
                        userReleaseRoleRepository);

                    // As the user has a role on the latest Release of the owning Publication of this Methodology
                    // they are allowed to update it.
                    Assert.Equal(expectedReleaseRolesToPass.Contains(releaseRole), authContext.HasSucceeded);
                });
            }

            [Fact]
            public async Task UsersWithNoRolesOnOwningPublicationsLatestReleaseCannotUpdateMethodology()
            {
                var (
                    handler,
                    methodologyRepository,
                    userPublicationRoleRepository,
                    userReleaseRoleRepository
                    ) = CreateHandlerAndDependencies();

                methodologyRepository.Setup(mock => mock.IsPubliclyAccessible(Methodology.Id))
                    .ReturnsAsync(false);

                methodologyRepository.Setup(s =>
                        s.GetOwningPublicationByMethodologyParent(Methodology.MethodologyParentId))
                    .ReturnsAsync(OwningPublication);

                userPublicationRoleRepository.SetupPublicationOwnerRoleExpectations(UserId, OwningPublication, false);

                userReleaseRoleRepository
                    .Setup(s => s.IsUserEditorOrApproverOnLatestRelease(UserId, OwningPublication.Id))
                    .ReturnsAsync(false);

                var user = CreateClaimsPrincipal(UserId);
                var authContext =
                    CreateAuthorizationHandlerContext<UpdateSpecificMethodologyRequirement, Methodology>
                        (user, Methodology);

                await handler.HandleAsync(authContext);
                VerifyAllMocks(
                    methodologyRepository,
                    userPublicationRoleRepository,
                    userReleaseRoleRepository);

                // A user with no roles on the owning Publication of this Methodology is not allowed to update it.
                Assert.False(authContext.HasSucceeded);
            }
        }

        private static (
            UpdateSpecificMethodologyAuthorizationHandler,
            Mock<IMethodologyRepository>,
            Mock<IUserPublicationRoleRepository>,
            Mock<IUserReleaseRoleRepository>
            )
            CreateHandlerAndDependencies()
        {
            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);
            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(Strict);

            var handler = new UpdateSpecificMethodologyAuthorizationHandler(
                methodologyRepository.Object,
                userPublicationRoleRepository.Object,
                userReleaseRoleRepository.Object
            );

            return (
                handler,
                methodologyRepository,
                userPublicationRoleRepository,
                userReleaseRoleRepository
            );
        }
    }
}
