#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.
    MethodologyStatusAuthorizationHandlers;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.
    AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseRole;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class MarkSpecificMethodologyAsDraftAuthorizationHandlerTests
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
            public async Task NoClaimsAllowMarkingPubliclyAccessibleMethodologyAsDraft()
            {
                await ForEachSecurityClaimAsync(async claim =>
                {
                    var (handler, methodologyRepository, _) = CreateHandlerAndDependencies();

                    methodologyRepository.Setup(mock => mock.IsPubliclyAccessible(Methodology.Id))
                        .ReturnsAsync(true);

                    var user = CreateClaimsPrincipal(UserId, claim);
                    var authContext =
                        CreateAuthorizationHandlerContext<MarkSpecificMethodologyAsDraftRequirement, Methodology>
                            (user, Methodology);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(methodologyRepository);

                    // No claims should allow a publicly accessible Methodology to be marked as draft
                    Assert.False(authContext.HasSucceeded);
                });
            }

            [Fact]
            public async Task UserWithCorrectClaimCanMarkNonPubliclyAccessibleMethodologyAsDraft()
            {
                await ForEachSecurityClaimAsync(async claim =>
                {
                    var (
                        handler,
                        methodologyRepository,
                        userReleaseRoleRepository
                        ) = CreateHandlerAndDependencies();

                    methodologyRepository.Setup(mock => mock.IsPubliclyAccessible(Methodology.Id))
                        .ReturnsAsync(false);

                    // Only the MarkAllMethodologiesDraft claim should allow a non publicly accessible Methodology to
                    // be marked as draft
                    var expectedToPassByClaimAlone = claim == MarkAllMethodologiesDraft;

                    if (!expectedToPassByClaimAlone)
                    {
                        methodologyRepository.Setup(s =>
                                s.GetOwningPublicationByMethodologyParent(Methodology.MethodologyParentId))
                            .ReturnsAsync(OwningPublication);

                        userReleaseRoleRepository
                            .Setup(s => s.IsUserApproverOnLatestRelease(UserId, OwningPublication.Id))
                            .ReturnsAsync(false);
                    }

                    var user = CreateClaimsPrincipal(UserId, claim);
                    var authContext =
                        CreateAuthorizationHandlerContext<MarkSpecificMethodologyAsDraftRequirement, Methodology>
                            (user, Methodology);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(methodologyRepository, userReleaseRoleRepository);

                    Assert.Equal(expectedToPassByClaimAlone, authContext.HasSucceeded);
                });
            }
        }

        public class ReleaseRoleTests
        {
            [Fact]
            public async Task ApproversOnOwningPublicationsLatestReleaseCanMarkMethodologyAsDraft()
            {
                await ForEachReleaseRoleAsync(async releaseRole =>
                {
                    var (
                        handler,
                        methodologyRepository,
                        userReleaseRoleRepository
                        ) = CreateHandlerAndDependencies();

                    methodologyRepository.Setup(mock => mock.IsPubliclyAccessible(Methodology.Id))
                        .ReturnsAsync(false);

                    methodologyRepository.Setup(s =>
                            s.GetOwningPublicationByMethodologyParent(Methodology.MethodologyParentId))
                        .ReturnsAsync(OwningPublication);

                    userReleaseRoleRepository
                        .Setup(s => s.IsUserApproverOnLatestRelease(UserId, OwningPublication.Id))
                        .ReturnsAsync(ApproverRoles.Contains(releaseRole));

                    var user = CreateClaimsPrincipal(UserId);

                    var authContext =
                        CreateAuthorizationHandlerContext<MarkSpecificMethodologyAsDraftRequirement, Methodology>(user,
                            Methodology);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(methodologyRepository, userReleaseRoleRepository);

                    // If the user has the Approver role on the latest Release of the owning Publication of this
                    // Methodology they are allowed to mark it as draft
                    var expectedToPass = releaseRole == Approver;

                    Assert.Equal(expectedToPass, authContext.HasSucceeded);
                });
            }

            [Fact]
            public async Task UsersWithNoRolesOnOwningPublicationsLatestReleaseCannotMarkMethodologyAsDraft()
            {
                var (
                    handler,
                    methodologyRepository,
                    userReleaseRoleRepository
                    ) = CreateHandlerAndDependencies();

                methodologyRepository.Setup(mock => mock.IsPubliclyAccessible(Methodology.Id))
                    .ReturnsAsync(false);

                methodologyRepository.Setup(s =>
                        s.GetOwningPublicationByMethodologyParent(Methodology.MethodologyParentId))
                    .ReturnsAsync(OwningPublication);

                userReleaseRoleRepository
                    .Setup(s => s.IsUserApproverOnLatestRelease(UserId, OwningPublication.Id))
                    .ReturnsAsync(false);

                var user = CreateClaimsPrincipal(UserId);

                var authContext =
                    CreateAuthorizationHandlerContext<MarkSpecificMethodologyAsDraftRequirement, Methodology>
                        (user, Methodology);

                await handler.HandleAsync(authContext);
                VerifyAllMocks(methodologyRepository, userReleaseRoleRepository);

                // A user with no role on the owning Publication of this Methodology is not allowed to mark it as draft
                Assert.False(authContext.HasSucceeded);
            }
        }

        private static (
            MarkSpecificMethodologyAsDraftAuthorizationHandler,
            Mock<IMethodologyRepository>,
            Mock<IUserReleaseRoleRepository>
            )
            CreateHandlerAndDependencies()
        {
            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);
            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(Strict);

            var handler = new MarkSpecificMethodologyAsDraftAuthorizationHandler(
                methodologyRepository.Object,
                userReleaseRoleRepository.Object
            );

            return (
                handler,
                methodologyRepository,
                userReleaseRoleRepository
            );
        }
    }
}
