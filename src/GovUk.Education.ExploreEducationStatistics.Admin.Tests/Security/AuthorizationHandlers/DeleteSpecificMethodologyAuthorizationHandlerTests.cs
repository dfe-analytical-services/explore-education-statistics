#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.Utils.ClaimsPrincipalUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.EnumUtil;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;
using static Moq.MockBehavior;
using IPublicationRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IPublicationRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class DeleteSpecificMethodologyAuthorizationHandlerTests
    {
        private static readonly Guid UserId = Guid.NewGuid();

        private static readonly MethodologyVersion DraftFirstVersion = new()
        {
            Id = Guid.NewGuid(),
            MethodologyId = Guid.NewGuid(),
            Status = Draft
        };

        private static readonly MethodologyVersion ApprovedFirstVersion = new()
        {
            Id = Guid.NewGuid(),
            MethodologyId = Guid.NewGuid(),
            Status = Approved
        };

        private static readonly MethodologyVersion DraftAmendmentVersion = new()
        {
            Id = Guid.NewGuid(),
            MethodologyId = Guid.NewGuid(),
            Status = Draft,
            PreviousVersionId = new Guid(),
            Version = 0
        };

        private static readonly MethodologyVersion ApprovedAmendmentVersion = new()
        {
            Id = Guid.NewGuid(),
            MethodologyId = Guid.NewGuid(),
            Status = Approved,
            PreviousVersionId = new Guid(),
            Version = 1
        };

        private static readonly Publication OwningPublication = new()
        {
            Id = Guid.NewGuid()
        };

        public class ClaimTests
        {
            [Fact]
            public async Task UserWithCorrectClaimCanDeleteDraftMethodology()
            {
                await ForEachSecurityClaimAsync(async claim =>
                {
                    var expectClaimToSucceed = claim == DeleteAllMethodologies;

                    var (
                        handler,
                        methodologyRepository,
                        methodologyVersionRepository,
                        userPublicationRoleRepository
                        ) = CreateHandlerAndDependencies();

                    methodologyVersionRepository
                        .Setup(s => s.IsPubliclyAccessible(DraftFirstVersion.Id))
                        .ReturnsAsync(false);

                    // If the Claim given to the handler isn't enough to make the handler succeed, it'll go on to check
                    // the user's Publication Roles.  
                    if (!expectClaimToSucceed)
                    {
                        methodologyRepository.Setup(s =>
                                s.GetOwningPublication(DraftFirstVersion.MethodologyId))
                            .ReturnsAsync(OwningPublication);

                        userPublicationRoleRepository
                            .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                            .ReturnsAsync(new List<PublicationRole>());
                    }

                    var user = CreateClaimsPrincipal(UserId, claim);
                    var authContext = CreateAuthContext(user, DraftFirstVersion);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(methodologyRepository, methodologyVersionRepository,
                        userPublicationRoleRepository);

                    // Verify that the presence of the "DeleteAllMethodologies" Claim and no other will pass the handler test
                    Assert.Equal(expectClaimToSucceed, authContext.HasSucceeded);
                });
            }

            [Fact]
            public async Task UserWithCorrectClaimCannotDeletePubliclyAccessibleMethodology()
            {
                await ForEachSecurityClaimAsync(async claim =>
                {
                    var (
                        handler,
                        _,
                        methodologyVersionRepository,
                        userPublicationRoleRepository
                        ) = CreateHandlerAndDependencies();

                    methodologyVersionRepository
                        .Setup(s => s.IsPubliclyAccessible(DraftFirstVersion.Id))
                        .ReturnsAsync(true);

                    var user = CreateClaimsPrincipal(UserId, claim);
                    var authContext = CreateAuthContext(user, DraftFirstVersion);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(methodologyVersionRepository, userPublicationRoleRepository);

                    // Verify that the presence of the "DeleteAllMethodologies" Claim still doesn't allow the user to
                    // do this with a publicly accessible version.
                    Assert.False(authContext.HasSucceeded);
                });
            }

            [Fact]
            public async Task UserWithCorrectClaimCannotDeleteApprovedMethodology()
            {
                await ForEachSecurityClaimAsync(async claim =>
                {
                    var (
                        handler,
                        _,
                        methodologyVersionRepository,
                        userPublicationRoleRepository
                        ) = CreateHandlerAndDependencies();

                    methodologyVersionRepository
                        .Setup(s => s.IsPubliclyAccessible(ApprovedFirstVersion.Id))
                        .ReturnsAsync(false);

                    var user = CreateClaimsPrincipal(UserId, claim);
                    var authContext = CreateAuthContext(user, ApprovedFirstVersion);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(methodologyVersionRepository, userPublicationRoleRepository);

                    // Verify that the presence of the "DeleteAllMethodologies" Claim still doesn't allow the user to
                    // do this with an approved version.
                    Assert.False(authContext.HasSucceeded);
                });
            }

            [Fact]
            public async Task UserWithCorrectClaimCanDeleteDraftMethodologyAmendment()
            {
                await ForEachSecurityClaimAsync(async claim =>
                {
                    var expectClaimToSucceed = claim == DeleteAllMethodologies;

                    var (
                        handler,
                        methodologyRepository,
                        methodologyVersionRepository,
                        userPublicationRoleRepository
                        ) = CreateHandlerAndDependencies();

                    methodologyVersionRepository
                        .Setup(s => s.IsPubliclyAccessible(DraftAmendmentVersion.Id))
                        .ReturnsAsync(false);

                    // If the Claim given to the handler isn't enough to make the handler succeed, it'll go on to check
                    // the user's Publication Roles.  
                    if (!expectClaimToSucceed)
                    {
                        methodologyRepository.Setup(s =>
                                s.GetOwningPublication(DraftAmendmentVersion.MethodologyId))
                            .ReturnsAsync(OwningPublication);

                        userPublicationRoleRepository
                            .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                            .ReturnsAsync(new List<PublicationRole>());
                    }

                    var user = CreateClaimsPrincipal(UserId, claim);
                    var authContext = CreateAuthContext(user, DraftAmendmentVersion);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(methodologyRepository, methodologyVersionRepository,
                        userPublicationRoleRepository);

                    // Verify that the presence of the "DeleteAllMethodologies" Claim and no other will pass the handler test
                    Assert.Equal(expectClaimToSucceed, authContext.HasSucceeded);
                });
            }

            [Fact]
            public async Task UserWithCorrectClaimCannotDeleteApprovedMethodologyAmendment()
            {
                await ForEachSecurityClaimAsync(async claim =>
                {
                    var (
                        handler,
                        _,
                        methodologyVersionRepository,
                        userPublicationRoleRepository
                        ) = CreateHandlerAndDependencies();

                    methodologyVersionRepository
                        .Setup(s => s.IsPubliclyAccessible(ApprovedAmendmentVersion.Id))
                        .ReturnsAsync(false);

                    var user = CreateClaimsPrincipal(UserId, claim);
                    var authContext = CreateAuthContext(user, ApprovedAmendmentVersion);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(methodologyVersionRepository, userPublicationRoleRepository);

                    // Verify that the presence of the "DeleteAllMethodologies" Claim still doesn't allow the user to
                    // do this with an approved version.
                    Assert.False(authContext.HasSucceeded);
                });
            }
        }

        public class PublicationRoleTests
        {
            [Fact]
            public async Task UserWithPublicationOwnerRoleCanDeleteDraftMethodology()
            {
                await GetEnumValues<PublicationRole>()
                    .ToAsyncEnumerable()
                    .ForEachAwaitAsync(async role =>
                    {
                        var (
                            handler,
                            methodologyRepository,
                            methodologyVersionRepository,
                            userPublicationRoleRepository
                            ) = CreateHandlerAndDependencies();

                        methodologyVersionRepository
                            .Setup(s => s.IsPubliclyAccessible(DraftFirstVersion.Id))
                            .ReturnsAsync(false);

                        methodologyRepository.Setup(s =>
                                s.GetOwningPublication(DraftFirstVersion.MethodologyId))
                            .ReturnsAsync(OwningPublication);

                        userPublicationRoleRepository
                            .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                            .ReturnsAsync(ListOf(role));

                        var user = CreateClaimsPrincipal(UserId);
                        var authContext = CreateAuthContext(user, DraftFirstVersion);

                        await handler.HandleAsync(authContext);
                        VerifyAllMocks(methodologyRepository, methodologyVersionRepository,
                            userPublicationRoleRepository);

                        // Verify that the presence of a Publication Owner role for this user that is related to a
                        // Publication that uses this Methodology will pass the handler test
                        Assert.Equal(role == Owner, authContext.HasSucceeded);
                    });
            }

            [Fact]
            public async Task UserWithNoPublicationRolesCannotDeleteDraftMethodology()
            {
                var (
                    handler,
                    methodologyRepository,
                    methodologyVersionRepository,
                    userPublicationRoleRepository
                    ) = CreateHandlerAndDependencies();

                methodologyVersionRepository
                    .Setup(s => s.IsPubliclyAccessible(DraftFirstVersion.Id))
                    .ReturnsAsync(false);

                methodologyRepository.Setup(s =>
                        s.GetOwningPublication(DraftFirstVersion.MethodologyId))
                    .ReturnsAsync(OwningPublication);

                userPublicationRoleRepository
                    .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                    .ReturnsAsync(new List<PublicationRole>());

                var user = CreateClaimsPrincipal(UserId);
                var authContext = CreateAuthContext(user, DraftFirstVersion);

                await handler.HandleAsync(authContext);
                VerifyAllMocks(methodologyRepository, methodologyVersionRepository, userPublicationRoleRepository);

                // Verify that the user cannot perform this action if they have no Publication Roles.
                Assert.False(authContext.HasSucceeded);
            }

            [Fact]
            public async Task UserWithPublicationOwnerRoleCannotDeletePubliclyAccessibleMethodology()
            {
                var (
                    handler,
                    _,
                    methodologyVersionRepository,
                    userPublicationRoleRepository
                    ) = CreateHandlerAndDependencies();

                methodologyVersionRepository
                    .Setup(s => s.IsPubliclyAccessible(DraftFirstVersion.Id))
                    .ReturnsAsync(true);

                var user = CreateClaimsPrincipal(UserId);
                var authContext = CreateAuthContext(user, DraftFirstVersion);

                await handler.HandleAsync(authContext);

                // Verify that the fact that the version is publicly accessible doesn't even need to check the
                // users' Publication Roles to determine whether or not they can do this.
                VerifyAllMocks(methodologyVersionRepository, userPublicationRoleRepository);
                Assert.False(authContext.HasSucceeded);
            }

            [Fact]
            public async Task UserWithPublicationOwnerRoleCannotDeleteApprovedMethodology()
            {
                var (
                    handler,
                    _,
                    methodologyVersionRepository,
                    userPublicationRoleRepository
                    ) = CreateHandlerAndDependencies();

                methodologyVersionRepository
                    .Setup(s => s.IsPubliclyAccessible(ApprovedFirstVersion.Id))
                    .ReturnsAsync(false);

                var user = CreateClaimsPrincipal(UserId);
                var authContext = CreateAuthContext(user, ApprovedFirstVersion);

                await handler.HandleAsync(authContext);

                // Verify that the fact that the version is approved doesn't even need to check the
                // users' Publication Roles to determine whether or not they can do this.
                VerifyAllMocks(methodologyVersionRepository, userPublicationRoleRepository);
                Assert.False(authContext.HasSucceeded);
            }

            [Fact]
            public async Task UserWithPublicationOwnerRoleCanDeleteDraftMethodologyAmendment()
            {
                await GetEnumValues<PublicationRole>()
                    .ToAsyncEnumerable()
                    .ForEachAwaitAsync(async role =>
                    {
                        var (
                            handler,
                            methodologyRepository,
                            methodologyVersionRepository,
                            userPublicationRoleRepository
                            ) = CreateHandlerAndDependencies();

                        methodologyVersionRepository
                            .Setup(s => s.IsPubliclyAccessible(DraftAmendmentVersion.Id))
                            .ReturnsAsync(false);

                        methodologyRepository.Setup(s =>
                                s.GetOwningPublication(DraftAmendmentVersion.MethodologyId))
                            .ReturnsAsync(OwningPublication);

                        userPublicationRoleRepository
                            .Setup(s => s.GetAllRolesByUserAndPublication(UserId, OwningPublication.Id))
                            .ReturnsAsync(ListOf(role));

                        var user = CreateClaimsPrincipal(UserId);
                        var authContext = CreateAuthContext(user, DraftAmendmentVersion);

                        await handler.HandleAsync(authContext);
                        VerifyAllMocks(methodologyRepository, methodologyVersionRepository,
                            userPublicationRoleRepository);

                        // Verify that the presence of a Publication Owner role for this user that is related to a
                        // Publication that uses this Methodology will pass the handler test
                        Assert.Equal(role == Owner, authContext.HasSucceeded);
                    });
            }

            [Fact]
            public async Task UserWithPublicationOwnerRoleCannotDeleteApprovedMethodologyAmendment()
            {
                var (
                    handler,
                    _,
                    methodologyVersionRepository,
                    userPublicationRoleRepository
                    ) = CreateHandlerAndDependencies();

                methodologyVersionRepository
                    .Setup(s => s.IsPubliclyAccessible(ApprovedAmendmentVersion.Id))
                    .ReturnsAsync(false);

                var user = CreateClaimsPrincipal(UserId);
                var authContext = CreateAuthContext(user, ApprovedAmendmentVersion);

                await handler.HandleAsync(authContext);

                // Verify that the fact that the version is approved doesn't even need to check the
                // users' Publication Roles to determine whether or not they can do this.
                VerifyAllMocks(methodologyVersionRepository, userPublicationRoleRepository);
                Assert.False(authContext.HasSucceeded);
            }
        }

        private static AuthorizationHandlerContext CreateAuthContext(ClaimsPrincipal user,
            MethodologyVersion methodologyVersion)
        {
            return CreateAuthorizationHandlerContext<DeleteSpecificMethodologyRequirement, MethodologyVersion>
                (user, methodologyVersion);
        }

        private static (
            DeleteSpecificMethodologyAuthorizationHandler,
            Mock<IMethodologyRepository>,
            Mock<IMethodologyVersionRepository>,
            Mock<IUserPublicationRoleRepository>)
            CreateHandlerAndDependencies()
        {
            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);
            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);

            var handler = new DeleteSpecificMethodologyAuthorizationHandler(
                methodologyVersionRepository.Object,
                methodologyRepository.Object,
                new AuthorizationHandlerResourceRoleService(
                    Mock.Of<IUserReleaseRoleRepository>(Strict),
                    userPublicationRoleRepository.Object,
                    Mock.Of<IPublicationRepository>(Strict)));

            return (handler, methodologyRepository, methodologyVersionRepository, userPublicationRoleRepository);
        }
    }
}
