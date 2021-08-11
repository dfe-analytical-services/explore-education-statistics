#nullable enable
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.
    AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.EnumUtil;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class DeleteSpecificMethodologyAuthorizationHandlerTests
    {
        private static readonly Guid UserId = Guid.NewGuid();

        private static readonly Methodology DraftMethodology = new()
        {
            Id = Guid.NewGuid(),
            MethodologyParentId = Guid.NewGuid(),
            Status = Draft
        };

        private static readonly Methodology ApprovedMethodology = new()
        {
            Id = Guid.NewGuid(),
            MethodologyParentId = Guid.NewGuid(),
            Status = Approved
        };

        private static readonly Publication OwningPublication = new()
        {
            Id = Guid.NewGuid()
        };

        public class DeleteSpecificMethodologyAuthorizationHandlerClaimTests
        {
            [Fact]
            public async Task UserWithCorrectClaimCanDeleteDraftMethodology()
            {
                await ForEachSecurityClaimAsync(async claim =>
                {
                    var expectClaimToSucceed = claim == DeleteAllMethodologies;

                    await using var context = InMemoryApplicationDbContext(Guid.NewGuid().ToString());
                    context.Attach(DraftMethodology);

                    var (handler, methodologyRepository, publicationRoleRepository) =
                        CreateHandlerAndDependencies();

                    methodologyRepository
                        .Setup(s => s.IsPubliclyAccessible(DraftMethodology.Id))
                        .ReturnsAsync(false);

                    // If the Claim given to the handler isn't enough to make the handler succeed, it'll go on to check
                    // the user's Publication Roles.  
                    if (!expectClaimToSucceed)
                    {
                        methodologyRepository.Setup(s =>
                                s.GetOwningPublicationByMethodologyParent(DraftMethodology.MethodologyParentId))
                            .ReturnsAsync(OwningPublication);

                        publicationRoleRepository
                            .Setup(s => s.UserHasRoleOnPublication(UserId, OwningPublication.Id, Owner))
                            .ReturnsAsync(false);
                    }

                    var user = CreateClaimsPrincipal(UserId, claim);
                    var authContext = CreateAuthContext(user, DraftMethodology);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(methodologyRepository, publicationRoleRepository);

                    // Verify that the presence of the "DeleteAllMethodologies" Claim and no other will pass the handler test
                    Assert.Equal(expectClaimToSucceed, authContext.HasSucceeded);
                });
            }

            [Fact]
            public async Task UserWithCorrectClaimCannotDeletePubliclyAccessibleMethodology()
            {
                await ForEachSecurityClaimAsync(async claim =>
                {
                    var (handler, methodologyRepository, publicationRoleRepository) =
                        CreateHandlerAndDependencies();

                    methodologyRepository
                        .Setup(s => s.IsPubliclyAccessible(DraftMethodology.Id))
                        .ReturnsAsync(true);

                    var user = CreateClaimsPrincipal(UserId, claim);
                    var authContext = CreateAuthContext(user, DraftMethodology);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(methodologyRepository, publicationRoleRepository);

                    // Verify that the presence of the "DeleteAllMethodologies" Claim still doesn't allow the user to
                    // do this with a Methodology in this state.
                    Assert.False(authContext.HasSucceeded);
                });
            }

            [Fact]
            public async Task UserWithCorrectClaimCannotDeleteApprovedMethodology()
            {
                await ForEachSecurityClaimAsync(async claim =>
                {
                    var (handler, methodologyRepository, publicationRoleRepository) =
                        CreateHandlerAndDependencies();

                    methodologyRepository
                        .Setup(s => s.IsPubliclyAccessible(ApprovedMethodology.Id))
                        .ReturnsAsync(false);

                    var user = CreateClaimsPrincipal(UserId, claim);
                    var authContext = CreateAuthContext(user, ApprovedMethodology);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(methodologyRepository, publicationRoleRepository);

                    // Verify that the presence of the "DeleteAllMethodologies" Claim still doesn't allow the user to
                    // do this with a Methodology in this state.
                    Assert.False(authContext.HasSucceeded);
                });
            }
        }

        public class DeleteSpecificMethodologyAuthorizationHandlerPublicationRoleTests
        {
            [Fact]
            public async Task UserWithPublicationOwnerRoleCanDeleteDraftMethodology()
            {
                await GetEnumValues<PublicationRole>().ForEachAsync(async role =>
                {
                    var (handler, methodologyRepository, publicationRoleRepository) =
                        CreateHandlerAndDependencies();

                    methodologyRepository
                        .Setup(s => s.IsPubliclyAccessible(DraftMethodology.Id))
                        .ReturnsAsync(false);

                    methodologyRepository.Setup(s =>
                            s.GetOwningPublicationByMethodologyParent(DraftMethodology.MethodologyParentId))
                        .ReturnsAsync(OwningPublication);

                    publicationRoleRepository
                        .Setup(s => s.UserHasRoleOnPublication(UserId, OwningPublication.Id, Owner))
                        .ReturnsAsync(role == Owner);

                    var user = CreateClaimsPrincipal(UserId);
                    var authContext = CreateAuthContext(user, DraftMethodology);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(methodologyRepository, publicationRoleRepository);

                    // Verify that the presence of a Publication Owner role for this user that is related to a
                    // Publication that uses this Methodology will pass the handler test
                    Assert.Equal(role == Owner, authContext.HasSucceeded);
                });
            }

            [Fact]
            public async Task UserWithNoPublicationRolesCannotDeleteDraftMethodology()
            {
                var (handler, methodologyRepository, publicationRoleRepository) =
                    CreateHandlerAndDependencies();

                methodologyRepository
                    .Setup(s => s.IsPubliclyAccessible(DraftMethodology.Id))
                    .ReturnsAsync(false);

                methodologyRepository.Setup(s =>
                        s.GetOwningPublicationByMethodologyParent(DraftMethodology.MethodologyParentId))
                    .ReturnsAsync(OwningPublication);

                publicationRoleRepository
                    .Setup(s => s.UserHasRoleOnPublication(UserId, OwningPublication.Id, Owner))
                    .ReturnsAsync(false);

                var user = CreateClaimsPrincipal(UserId);
                var authContext = CreateAuthContext(user, DraftMethodology);

                await handler.HandleAsync(authContext);
                VerifyAllMocks(methodologyRepository, publicationRoleRepository);

                // Verify that the user cannot perform this action if they have no Publication Roles.
                Assert.False(authContext.HasSucceeded);
            }

            [Fact]
            public async Task UserWithPublicationOwnerRoleCannotDeletePubliclyAccessibleMethodology()
            {
                var (handler, methodologyRepository, publicationRoleRepository) =
                    CreateHandlerAndDependencies();

                methodologyRepository
                    .Setup(s => s.IsPubliclyAccessible(ApprovedMethodology.Id))
                    .ReturnsAsync(true);

                var user = CreateClaimsPrincipal(UserId);
                var authContext = CreateAuthContext(user, ApprovedMethodology);

                await handler.HandleAsync(authContext);

                // Verify that the fact that the Methodology is publicly accessible doesn't even need to check the
                // users' Publication Roles to determine whether or not they can do this.
                VerifyAllMocks(methodologyRepository, publicationRoleRepository);
                Assert.False(authContext.HasSucceeded);
            }

            [Fact]
            public async Task UserWithPublicationOwnerRoleCannotDeleteApprovedMethodology()
            {
                var (handler, methodologyRepository, publicationRoleRepository) =
                    CreateHandlerAndDependencies();

                methodologyRepository
                    .Setup(s => s.IsPubliclyAccessible(ApprovedMethodology.Id))
                    .ReturnsAsync(false);

                var user = CreateClaimsPrincipal(UserId);
                var authContext = CreateAuthContext(user, ApprovedMethodology);

                await handler.HandleAsync(authContext);

                // Verify that the fact that the Methodology is Approved doesn't even need to check the
                // users' Publication Roles to determine whether or not they can do this.
                VerifyAllMocks(methodologyRepository, publicationRoleRepository);
                Assert.False(authContext.HasSucceeded);
            }
        }

        private static AuthorizationHandlerContext CreateAuthContext(ClaimsPrincipal user, Methodology methodology)
        {
            return CreateAuthorizationHandlerContext<DeleteSpecificMethodologyRequirement, Methodology>
                (user, methodology);
        }

        private static (
            DeleteSpecificMethodologyAuthorizationHandler,
            Mock<IMethodologyRepository>,
            Mock<IUserPublicationRoleRepository>)
            CreateHandlerAndDependencies()
        {
            var publicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);

            var handler = new DeleteSpecificMethodologyAuthorizationHandler(
                methodologyRepository.Object,
                publicationRoleRepository.Object);

            return (handler, methodologyRepository, publicationRoleRepository);
        }
    }
}
