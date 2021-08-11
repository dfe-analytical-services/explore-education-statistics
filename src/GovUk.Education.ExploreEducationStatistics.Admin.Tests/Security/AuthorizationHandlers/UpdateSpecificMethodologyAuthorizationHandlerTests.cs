#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.
    AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
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

        public class UpdateSpecificMethodologyAuthorizationHandlerClaimsTests
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
                    var (handler, methodologyRepository, _, _, _) = CreateHandlerAndDependencies();

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
                var methodology = new Methodology
                {
                    Id = Guid.NewGuid()
                };

                await ForEachSecurityClaimAsync(async claim =>
                {
                    var (handler, methodologyRepository, _, _, _) = CreateHandlerAndDependencies();

                    methodologyRepository.Setup(mock => mock.IsPubliclyAccessible(methodology.Id))
                        .ReturnsAsync(true);

                    var user = CreateClaimsPrincipal(UserId, claim);
                    var authContext =
                        CreateAuthorizationHandlerContext<UpdateSpecificMethodologyRequirement, Methodology>
                            (user, methodology);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(methodologyRepository);

                    // No claims should allow a publicly accessible Methodology to be updated
                    Assert.False(authContext.HasSucceeded);
                });
            }

            [Fact]
            public async Task UserWithCorrectClaimCanUpdateNonPubliclyAccessibleMethodology()
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
                    }
                };

                await using var context = InMemoryApplicationDbContext(Guid.NewGuid().ToString());
                context.Attach(methodology);

                await ForEachSecurityClaimAsync(async claim =>
                {
                    var (
                        handler,
                        methodologyRepository,
                        publicationRepository,
                        userPublicationRoleRepository,
                        _
                        ) = CreateHandlerAndDependencies(context);

                    methodologyRepository.Setup(mock => mock.IsPubliclyAccessible(methodology.Id))
                        .ReturnsAsync(false);

                    // Only the UpdateAllMethodologies claim should allow a non publicly accessible Methodology
                    // to be updated.
                    var expectedToPassByClaimAlone = claim == UpdateAllMethodologies;

                    if (!expectedToPassByClaimAlone)
                    {
                        userPublicationRoleRepository
                            .Setup(s => s.GetAllRolesByUser(UserId, owningPublicationId))
                            .ReturnsAsync(new List<PublicationRole>());

                        publicationRepository
                            .Setup(s => s.GetLatestReleaseForPublication(owningPublicationId))
                            .ReturnsAsync((Release?) null);
                    }

                    var user = CreateClaimsPrincipal(UserId, claim);
                    var authContext =
                        CreateAuthorizationHandlerContext<UpdateSpecificMethodologyRequirement, Methodology>
                            (user, methodology);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(methodologyRepository, publicationRepository, userPublicationRoleRepository);

                    Assert.Equal(expectedToPassByClaimAlone, authContext.HasSucceeded);
                });
            }
        }

        public class UpdateSpecificMethodologyAuthorizationHandlerPublicationRoleTests
        {
            [Fact]
            public async Task PublicationOwnerCanUpdateMethodology()
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

                await using var context = InMemoryApplicationDbContext(Guid.NewGuid().ToString());
                context.Attach(methodology);

                await ForEachPublicationRoleAsync(async publicationRole =>
                {
                    var (
                        handler,
                        methodologyRepository,
                        publicationRepository,
                        userPublicationRoleRepository,
                        _
                        ) = CreateHandlerAndDependencies(context);

                    methodologyRepository.Setup(mock => mock.IsPubliclyAccessible(methodology.Id))
                        .ReturnsAsync(false);

                    var expectedToPassByRole = publicationRole == Owner;

                    userPublicationRoleRepository
                        .Setup(s => s.GetAllRolesByUser(UserId, owningPublicationId))
                        .ReturnsAsync(ListOf(publicationRole));

                    if (!expectedToPassByRole)
                    {
                        publicationRepository
                            .Setup(s => s.GetLatestReleaseForPublication(owningPublicationId))
                            .ReturnsAsync((Release?) null);
                    }

                    var user = CreateClaimsPrincipal(UserId);

                    var authContext =
                        CreateAuthorizationHandlerContext<UpdateSpecificMethodologyRequirement, Methodology>
                            (user, methodology);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(methodologyRepository, publicationRepository, userPublicationRoleRepository);

                    // As the user has Publication Owner role on the owning Publication of this Methodology, they are
                    // allowed to update it.
                    Assert.Equal(expectedToPassByRole, authContext.HasSucceeded);
                });
            }
        }

        public class UpdateSpecificMethodologyAuthorizationHandlerReleaseRoleTests
        {
            [Fact]
            public async Task EditorsOrApproversOnOwningPublicationsLatestReleaseCanUpdateMethodology()
            {
                var expectedReleaseRolesToPass = ListOf(Approver, Contributor, Lead);

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
                        userPublicationRoleRepository,
                        userReleaseRoleRepository
                        ) = CreateHandlerAndDependencies(context);

                    methodologyRepository.Setup(mock => mock.IsPubliclyAccessible(methodology.Id))
                        .ReturnsAsync(false);

                    userPublicationRoleRepository
                        .Setup(s => s.GetAllRolesByUser(UserId, owningPublicationId))
                        .ReturnsAsync(new List<PublicationRole>());

                    publicationRepository
                        .Setup(s => s.GetLatestReleaseForPublication(owningPublicationId))
                        .ReturnsAsync(latestRelease);

                    userReleaseRoleRepository
                        .Setup(s => s.GetAllRolesByUser(UserId, latestRelease.Id))
                        .ReturnsAsync(ListOf(releaseRole));

                    var user = CreateClaimsPrincipal(UserId);

                    var authContext =
                        CreateAuthorizationHandlerContext<UpdateSpecificMethodologyRequirement, Methodology>
                            (user, methodology);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(
                        methodologyRepository,
                        publicationRepository,
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
                    userPublicationRoleRepository,
                    userReleaseRoleRepository
                    ) = CreateHandlerAndDependencies(context);

                methodologyRepository.Setup(mock => mock.IsPubliclyAccessible(methodology.Id))
                    .ReturnsAsync(false);

                userPublicationRoleRepository
                    .Setup(s => s.GetAllRolesByUser(UserId, owningPublicationId))
                    .ReturnsAsync(new List<PublicationRole>());

                publicationRepository
                    .Setup(s => s.GetLatestReleaseForPublication(owningPublicationId))
                    .ReturnsAsync(latestRelease);

                userReleaseRoleRepository
                    .Setup(s => s.GetAllRolesByUser(UserId, latestRelease.Id))
                    .ReturnsAsync(new List<ReleaseRole>());

                var user = CreateClaimsPrincipal(UserId);

                var authContext =
                    CreateAuthorizationHandlerContext<UpdateSpecificMethodologyRequirement, Methodology>
                        (user, methodology);

                await handler.HandleAsync(authContext);
                VerifyAllMocks(
                    methodologyRepository,
                    publicationRepository,
                    userPublicationRoleRepository,
                    userReleaseRoleRepository);

                // A user with no roles on the owning Publication of this Methodology is not allowed to update it.
                Assert.False(authContext.HasSucceeded);
            }
        }

        private static (
            UpdateSpecificMethodologyAuthorizationHandler,
            Mock<IMethodologyRepository>,
            Mock<IPublicationRepository>,
            Mock<IUserPublicationRoleRepository>,
            Mock<IUserReleaseRoleRepository>
            )
            CreateHandlerAndDependencies(ContentDbContext? contentDbContext = null)
        {
            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);
            var publicationRepository = new Mock<IPublicationRepository>(Strict);
            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(Strict);

            var handler = new UpdateSpecificMethodologyAuthorizationHandler(
                contentDbContext ?? Mock.Of<ContentDbContext>(),
                methodologyRepository.Object,
                publicationRepository.Object,
                userPublicationRoleRepository.Object,
                userReleaseRoleRepository.Object
            );

            return (
                handler,
                methodologyRepository,
                publicationRepository,
                userPublicationRoleRepository,
                userReleaseRoleRepository
            );
        }
    }
}
