#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.MethodologyStatusAuthorizationHandlers;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseRole;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ApproveSpecificMethodologyAuthorizationHandlerTests
    {
        private static readonly Guid UserId = Guid.NewGuid();

        public class ApproveSpecificMethodologyAuthorizationHandlerClaimsTests
        {
            [Fact]
            public async Task NoClaimsAllowApprovingPubliclyAccessibleMethodology()
            {
                var methodology = new Methodology
                {
                    Id = Guid.NewGuid()
                };

                await ForEachSecurityClaimAsync(async claim =>
                {
                    var (handler, methodologyRepository, _, _) = CreateHandlerAndDependencies();

                    methodologyRepository.Setup(mock => mock.IsPubliclyAccessible(methodology.Id))
                        .ReturnsAsync(true);

                    var user = CreateClaimsPrincipal(UserId, claim);
                    var authContext =
                        CreateAuthorizationHandlerContext<ApproveSpecificMethodologyRequirement, Methodology>
                            (user, methodology);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(methodologyRepository);

                    // No claims should allow a publicly accessible Methodology to be approved
                    Assert.False(authContext.HasSucceeded);
                });
            }

            [Fact]
            public async Task UserWithCorrectClaimCanApproveNonPubliclyAccessibleDraftMethodology()
            {
                var owningPublicationId = Guid.NewGuid();

                var methodology = new Methodology
                {
                    Id = Guid.NewGuid(),
                    MethodologyParent = new MethodologyParent
                    {
                        Publications = ListOf(new PublicationMethodology
                        {
                            PublicationId = owningPublicationId,
                            Owner = true
                        })
                    },
                    Status = Draft
                };

                await using var context = InMemoryApplicationDbContext(Guid.NewGuid().ToString());
                context.Attach(methodology);

                await ForEachSecurityClaimAsync(async claim =>
                {
                    
                    var (
                        handler,
                        methodologyRepository,
                        publicationRepository,
                        _
                        ) = CreateHandlerAndDependencies(context);

                    methodologyRepository.Setup(mock => mock.IsPubliclyAccessible(methodology.Id))
                        .ReturnsAsync(false);

                    // Only the ApproveAllMethodologies claim should allow approving a Methodology
                    var expectedToPassByClaimAlone = claim == ApproveAllMethodologies;

                    if (!expectedToPassByClaimAlone)
                    {
                        publicationRepository
                            .Setup(s => s.GetLatestReleaseForPublication(owningPublicationId))
                            .ReturnsAsync((Release?) null);
                    }

                    var user = CreateClaimsPrincipal(UserId, claim);
                    var authContext =
                        CreateAuthorizationHandlerContext<ApproveSpecificMethodologyRequirement, Methodology>
                            (user, methodology);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(methodologyRepository, publicationRepository);

                    Assert.Equal(expectedToPassByClaimAlone, authContext.HasSucceeded);
                });
            }

            [Fact]
            public async Task UserWithCorrectClaimCanApproveNonPubliclyAccessibleApprovedMethodology()
            {
                var owningPublicationId = Guid.NewGuid();

                var methodology = new Methodology
                {
                    Id = Guid.NewGuid(),
                    MethodologyParent = new MethodologyParent
                    {
                        Publications = ListOf(new PublicationMethodology
                        {
                            PublicationId = owningPublicationId,
                            Owner = true
                        })
                    },
                    Status = Approved
                };

                await using var context = InMemoryApplicationDbContext(Guid.NewGuid().ToString());
                context.Attach(methodology);

                await ForEachSecurityClaimAsync(async claim =>
                {
                    var (
                        handler,
                        methodologyRepository,
                        publicationRepository,
                        _
                        ) = CreateHandlerAndDependencies(context);

                    methodologyRepository.Setup(mock => mock.IsPubliclyAccessible(methodology.Id))
                        .ReturnsAsync(false);

                    // Only the ApproveAllMethodologies claim should allow approving a Methodology
                    var expectedToPassByClaimAlone = claim == ApproveAllMethodologies;

                    if (!expectedToPassByClaimAlone)
                    {
                        publicationRepository
                            .Setup(s => s.GetLatestReleaseForPublication(owningPublicationId))
                            .ReturnsAsync((Release?) null);
                    }

                    var user = CreateClaimsPrincipal(UserId, claim);
                    var authContext =
                        CreateAuthorizationHandlerContext<ApproveSpecificMethodologyRequirement, Methodology>
                            (user, methodology);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(methodologyRepository, publicationRepository);

                    Assert.Equal(expectedToPassByClaimAlone, authContext.HasSucceeded);
                });
            }
        }
        
        public class ApproveSpecificMethodologyAuthorizationHandlerReleaseRoleTests
        {
            [Fact]
            public async Task ApproversOnOwningPublicationsLatestReleaseCanApprove()
            {
                var owningPublicationId = Guid.NewGuid();

                var methodology = new Methodology
                {
                    Id = Guid.NewGuid(),
                    MethodologyParent = new MethodologyParent
                    {
                        Publications = ListOf(
                            new PublicationMethodology
                            {
                                PublicationId = Guid.NewGuid(),
                                Owner = false
                            },
                            new PublicationMethodology
                            {
                                PublicationId = owningPublicationId,
                                Owner = true
                            },
                            new PublicationMethodology
                            {
                                PublicationId = Guid.NewGuid(),
                                Owner = false
                            }
                        )
                    }
                };

                var latestRelease = new Release
                {
                    Id = Guid.NewGuid()
                };

                await using var context = InMemoryApplicationDbContext(Guid.NewGuid().ToString());
                context.Attach(methodology);

                await ForEachReleaseRoleAsync(async releaseRole =>
                {
                    var (
                        handler,
                        methodologyRepository,
                        publicationRepository,
                        userReleaseRoleRepository
                        ) = CreateHandlerAndDependencies(context);

                    methodologyRepository.Setup(mock => mock.IsPubliclyAccessible(methodology.Id))
                        .ReturnsAsync(false);

                    publicationRepository
                        .Setup(s => s.GetLatestReleaseForPublication(owningPublicationId))
                        .ReturnsAsync(latestRelease);

                    userReleaseRoleRepository
                        .Setup(s => s.GetAllRolesByUser(UserId, latestRelease.Id))
                        .ReturnsAsync(ListOf(releaseRole));

                    var user = CreateClaimsPrincipal(UserId);

                    var authContext = CreateAuthorizationHandlerContext<ApproveSpecificMethodologyRequirement, Methodology>(user, methodology);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(methodologyRepository, publicationRepository, userReleaseRoleRepository);

                    // If the user has the Approver role on the latest Release of the owning Publication of this
                    // Methodology they are allowed to approve it
                    var expectedToPass = releaseRole == Approver;

                    Assert.Equal(expectedToPass, authContext.HasSucceeded);
                });
            }

            [Fact]
            public async Task UsersWithNoRolesOnOwningPublicationsLatestReleaseCannotApprove()
            {
                var owningPublicationId = Guid.NewGuid();

                var methodology = new Methodology
                {
                    Id = Guid.NewGuid(),
                    MethodologyParent = new MethodologyParent
                    {
                        Publications = ListOf(
                            new PublicationMethodology
                            {
                                PublicationId = Guid.NewGuid(),
                                Owner = false
                            },
                            new PublicationMethodology
                            {
                                PublicationId = owningPublicationId,
                                Owner = true
                            },
                            new PublicationMethodology
                            {
                                PublicationId = Guid.NewGuid(),
                                Owner = false
                            }
                        )
                    }
                };

                var latestRelease = new Release
                {
                    Id = Guid.NewGuid()
                };

                await using var context = InMemoryApplicationDbContext(Guid.NewGuid().ToString());
                context.Attach(methodology);

                var (
                    handler,
                    methodologyRepository,
                    publicationRepository,
                    userReleaseRoleRepository
                    ) = CreateHandlerAndDependencies(context);

                methodologyRepository.Setup(mock => mock.IsPubliclyAccessible(methodology.Id))
                    .ReturnsAsync(false);

                publicationRepository
                    .Setup(s => s.GetLatestReleaseForPublication(owningPublicationId))
                    .ReturnsAsync(latestRelease);

                userReleaseRoleRepository
                    .Setup(s => s.GetAllRolesByUser(UserId, latestRelease.Id))
                    .ReturnsAsync(new List<ReleaseRole>());

                var user = CreateClaimsPrincipal(UserId);

                var authContext =
                    CreateAuthorizationHandlerContext<ApproveSpecificMethodologyRequirement, Methodology>
                        (user, methodology);

                await handler.HandleAsync(authContext);
                VerifyAllMocks(methodologyRepository, publicationRepository, userReleaseRoleRepository);

                // A user with no role on the owning Publication of this Methodology is not allowed to approve it
                Assert.False(authContext.HasSucceeded);
            }
        }

        private static
            (ApproveSpecificMethodologyAuthorizationHandler,
            Mock<IMethodologyRepository>,
            Mock<IPublicationRepository>,
            Mock<IUserReleaseRoleRepository>
            )
            CreateHandlerAndDependencies(ContentDbContext? contentDbContext = null)
        {
            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);
            var publicationRepository = new Mock<IPublicationRepository>(Strict);
            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(Strict);

            var handler = new ApproveSpecificMethodologyAuthorizationHandler(
                contentDbContext ?? Mock.Of<ContentDbContext>(),
                methodologyRepository.Object,
                publicationRepository.Object,
                userReleaseRoleRepository.Object);

            return (
                handler,
                methodologyRepository,
                publicationRepository,
                userReleaseRoleRepository
            );
        }
    }
}
