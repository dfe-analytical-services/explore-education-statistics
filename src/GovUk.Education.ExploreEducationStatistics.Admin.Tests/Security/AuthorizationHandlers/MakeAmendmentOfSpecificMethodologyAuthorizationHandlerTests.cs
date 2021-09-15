#nullable enable
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.
    AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.EnumUtil;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyPublishingStrategy;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class MakeAmendmentOfSpecificMethodologyAuthorizationHandlerTests
    {
        private static readonly Guid UserId = Guid.NewGuid();

        private static readonly MethodologyVersion MethodologyVersion = new()
        {
            Id = Guid.NewGuid(),
            PublishingStrategy = Immediately,
            MethodologyId = Guid.NewGuid()
        };

        private static readonly Publication OwningPublication = new()
        {
            Id = Guid.NewGuid()
        };

        public class ClaimTests
        {
            [Fact]
            public async Task UserWithCorrectClaimCanCreateAmendmentOfPubliclyAccessibleMethodology()
            {
                await ForEachSecurityClaimAsync(async claim =>
                {
                    var (
                        handler,
                        methodologyRepository,
                        methodologyVersionRepository,
                        publicationRoleRepository
                        ) = CreateHandlerAndDependencies();

                    methodologyVersionRepository.Setup(mock => mock.IsPubliclyAccessible(MethodologyVersion.Id))
                        .ReturnsAsync(true);

                    // Only the MakeAmendmentsOfAllMethodologies claim alongside the publicly-accessible Methodology
                    // should allow creating an Amendment
                    var expectedToPassByClaimAlone = claim == MakeAmendmentsOfAllMethodologies;

                    if (!expectedToPassByClaimAlone)
                    {
                        methodologyRepository.Setup(mock =>
                                mock.GetOwningPublication(MethodologyVersion.MethodologyId))
                            .ReturnsAsync(OwningPublication);

                        publicationRoleRepository.SetupPublicationOwnerRoleExpectations(
                            UserId, OwningPublication, false);
                    }

                    var user = CreateClaimsPrincipal(UserId, claim);
                    var authContext = CreateAuthContext(user, MethodologyVersion);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(methodologyRepository, methodologyVersionRepository,
                        publicationRoleRepository);

                    Assert.Equal(expectedToPassByClaimAlone, authContext.HasSucceeded);
                });
            }

            [Fact]
            public async Task UserWithCorrectClaimCannotCreateAmendmentOfPrivateMethodology()
            {
                await ForEachSecurityClaimAsync(async claim =>
                {
                    var (
                        handler,
                        _,
                        methodologyVersionRepository,
                        publicationRoleRepository
                        ) = CreateHandlerAndDependencies();

                    methodologyVersionRepository.Setup(mock => mock.IsPubliclyAccessible(MethodologyVersion.Id))
                        .ReturnsAsync(false);

                    var user = CreateClaimsPrincipal(UserId, claim);
                    var authContext = CreateAuthContext(user, MethodologyVersion);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(methodologyVersionRepository, publicationRoleRepository);

                    // Verify that no Claims can allow a user to create an Amendment of a non-publicly-accessible
                    // Methodology.
                    Assert.False(authContext.HasSucceeded);
                });
            }
        }

        public class PublicationRoleTests
        {
            [Fact]
            public async Task
                UserWithLinkedPublicationOwnerRoleCanCreateAmendmentOfPubliclyAccessibleMethodologyOwnedByPublication()
            {
                await GetEnumValues<PublicationRole>().ForEachAsync(async role =>
                {
                    var (
                        handler,
                        methodologyRepository,
                        methodologyVersionRepository,
                        publicationRoleRepository
                        ) = CreateHandlerAndDependencies();

                    methodologyVersionRepository.Setup(mock => mock.IsPubliclyAccessible(MethodologyVersion.Id))
                        .ReturnsAsync(true);

                    methodologyRepository.Setup(s =>
                            s.GetOwningPublication(MethodologyVersion.MethodologyId))
                        .ReturnsAsync(OwningPublication);

                    publicationRoleRepository.SetupPublicationOwnerRoleExpectations(
                        UserId, OwningPublication, role == Owner);

                    var user = CreateClaimsPrincipal(UserId);
                    var authContext = CreateAuthContext(user, MethodologyVersion);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(methodologyRepository, methodologyVersionRepository,
                        publicationRoleRepository);

                    // Verify that the user can create an Amendment, as they have a Publication Owner role on a Publication
                    // that uses this Methodology.
                    Assert.Equal(role == Owner, authContext.HasSucceeded);
                });
            }

            [Fact]
            public async Task
                UserWithoutLinkedPublicationOwnerRoleCannotCreateAmendmentOfPubliclyAccessibleMethodology()
            {
                var (
                    handler,
                    methodologyRepository,
                    methodologyVersionRepository,
                    publicationRoleRepository
                    ) = CreateHandlerAndDependencies();

                methodologyVersionRepository.Setup(mock => mock.IsPubliclyAccessible(MethodologyVersion.Id))
                    .ReturnsAsync(true);

                methodologyRepository.Setup(s =>
                        s.GetOwningPublication(MethodologyVersion.MethodologyId))
                    .ReturnsAsync(OwningPublication);

                publicationRoleRepository.SetupPublicationOwnerRoleExpectations(
                    UserId, OwningPublication, false);

                var user = CreateClaimsPrincipal(UserId);
                var authContext = CreateAuthContext(user, MethodologyVersion);

                await handler.HandleAsync(authContext);
                VerifyAllMocks(methodologyRepository, methodologyVersionRepository, publicationRoleRepository);

                // Verify that the user cannot create an Amendment, as they don't have a Publication Owner role on a
                // Publication that uses this Methodology.
                Assert.False(authContext.HasSucceeded);
            }

            [Fact]
            public async Task UserWithLinkedPublicationOwnerRoleCannotCreateAmendmentOfPrivateMethodology()
            {
                var (
                    handler,
                    _,
                    methodologyVersionRepository,
                    publicationRoleRepository) = CreateHandlerAndDependencies();

                methodologyVersionRepository.Setup(mock => mock.IsPubliclyAccessible(MethodologyVersion.Id))
                    .ReturnsAsync(false);

                var user = CreateClaimsPrincipal(UserId);
                var authContext = CreateAuthContext(user, MethodologyVersion);

                await handler.HandleAsync(authContext);
                VerifyAllMocks(methodologyVersionRepository, publicationRoleRepository);

                // Verify that the user cannot create an Amendment, as even though they have the Publication Owner role
                // on one of the Publications that use this Methodology, this Methodology is not yet publicly
                // accessible.
                Assert.False(authContext.HasSucceeded);
            }
        }

        private static AuthorizationHandlerContext CreateAuthContext(ClaimsPrincipal user,
            MethodologyVersion methodologyVersion)
        {
            return CreateAuthorizationHandlerContext<MakeAmendmentOfSpecificMethodologyRequirement, MethodologyVersion>
                (user, methodologyVersion);
        }

        private static (MakeAmendmentOfSpecificMethodologyAuthorizationHandler,
            Mock<IMethodologyRepository>,
            Mock<IMethodologyVersionRepository>,
            Mock<IUserPublicationRoleRepository>)
            CreateHandlerAndDependencies()
        {
            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);
            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);
            var publicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);

            var handler = new MakeAmendmentOfSpecificMethodologyAuthorizationHandler(
                methodologyVersionRepository.Object,
                methodologyRepository.Object,
                publicationRoleRepository.Object);

            return (handler, methodologyRepository, methodologyVersionRepository, publicationRoleRepository);
        }
    }
}
