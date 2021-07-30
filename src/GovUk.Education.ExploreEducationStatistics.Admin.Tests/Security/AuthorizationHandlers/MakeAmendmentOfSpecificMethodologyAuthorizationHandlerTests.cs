using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
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

        public class MakeAmendmentOfSpecificMethodologyAuthorizationHandlerClaimTests
        {
            private static readonly Methodology MethodologyNoPublication = new Methodology
            {
                Id = Guid.NewGuid(),
                PublishingStrategy = Immediately,
                MethodologyParent = new MethodologyParent()
            };
      
            [Fact]
            public async Task UserWithCorrectClaimCanCreateAmendmentOfPubliclyAccessibleMethodology()
            {
                await ForEachSecurityClaimAsync(async claim =>
                {
                    await using var context = InMemoryApplicationDbContext(Guid.NewGuid().ToString());
                    context.Attach(MethodologyNoPublication);

                    var (handler, methodologyRepository, publicationRoleRepository) = CreateHandlerAndDependencies(context);

                    methodologyRepository.Setup(mock => mock.IsPubliclyAccessible(MethodologyNoPublication.Id))
                        .ReturnsAsync(true);

                    var user = CreateClaimsPrincipal(UserId, claim);
                    var authContext = CreateAuthContext(user, MethodologyNoPublication);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(methodologyRepository, publicationRoleRepository);

                    // Verify that only the combination of the "MakeAmendmentsOfAllMethodologies" Claim
                    // alongside the publicly-accessible Methodology will pass the handler test
                    var expectedToPass = claim == MakeAmendmentsOfAllMethodologies;
                    Assert.Equal(expectedToPass, authContext.HasSucceeded);
                });
            }

            [Fact]
            public async Task UserWithCorrectClaimCannotCreateAmendmentOfPrivateMethodology()
            {
                await ForEachSecurityClaimAsync(async claim =>
                {
                    await using var context = InMemoryApplicationDbContext(Guid.NewGuid().ToString());
                    context.Attach(MethodologyNoPublication);

                    var (handler, methodologyRepository, publicationRoleRepository) = CreateHandlerAndDependencies(context);

                    methodologyRepository.Setup(mock => mock.IsPubliclyAccessible(MethodologyNoPublication.Id))
                        .ReturnsAsync(false);

                    var user = CreateClaimsPrincipal(UserId, claim);
                    var authContext = CreateAuthContext(user, MethodologyNoPublication);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(methodologyRepository, publicationRoleRepository);

                    // Verify that no Claims can allow a user to create an Amendment of a non-publicly-accessible
                    // Methodology. 
                    Assert.False(authContext.HasSucceeded);
                });
            }
        }

        public class MakeAmendmentOfSpecificMethodologyAuthorizationHandlerPublicationRoleTests
        {
            private static readonly Methodology MethodologyWithPublication = new Methodology
            {
                Id = Guid.NewGuid(),
                PublishingStrategy = Immediately,
                MethodologyParent = new MethodologyParent
                {
                    Publications = AsList(new PublicationMethodology
                    {
                        Owner = true,
                        PublicationId = Guid.NewGuid()
                    })
                }
            };

            private static readonly Methodology MethodologyWithPublicationNotOwner = new Methodology
            {
                Id = Guid.NewGuid(),
                PublishingStrategy = Immediately,
                MethodologyParent = new MethodologyParent
                {
                    Publications = AsList(new PublicationMethodology
                    {
                        Owner = false,
                        PublicationId = Guid.NewGuid()
                    })
                }
            };
            
            [Fact]
            public async Task UserWithLinkedPublicationOwnerRoleCanCreateAmendmentOfPubliclyAccessibleMethodologyOwnedByPublication()
            {
                await GetEnumValues<PublicationRole>().ForEachAsync(async role =>
                {
                    await using var context = InMemoryApplicationDbContext(Guid.NewGuid().ToString());
                    context.Attach(MethodologyWithPublication);

                    var (handler, methodologyRepository, publicationRoleRepository) = CreateHandlerAndDependencies(context);

                    methodologyRepository.Setup(mock => mock.IsPubliclyAccessible(MethodologyWithPublication.Id))
                        .ReturnsAsync(true);

                    var user = CreateClaimsPrincipal(UserId);
                    var authContext = CreateAuthContext(user, MethodologyWithPublication);

                    var publicationId = MethodologyWithPublication.MethodologyParent.Publications[0].PublicationId;
                    
                    publicationRoleRepository
                        .Setup(s => s.GetAllRolesByUser(UserId, publicationId))
                        .ReturnsAsync(AsList(role));

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(methodologyRepository, publicationRoleRepository);

                    // Verify that the user can create an Amendment, as they have a Publication Owner role on a Publication
                    // that uses this Methodology.
                    Assert.Equal(role == Owner, authContext.HasSucceeded);
                });
            }
            
            [Fact]
            public async Task UserWithoutLinkedPublicationOwnerRoleCannotCreateAmendmentOfPubliclyAccessibleMethodology()
            {
                await using var context = InMemoryApplicationDbContext(Guid.NewGuid().ToString());
                context.Attach(MethodologyWithPublication);

                var user = CreateClaimsPrincipal(UserId);
                var authContext = CreateAuthContext(user, MethodologyWithPublication);

                var (handler, methodologyRepository, publicationRoleRepository) = CreateHandlerAndDependencies(context);

                methodologyRepository.Setup(mock => mock.IsPubliclyAccessible(MethodologyWithPublication.Id))
                    .ReturnsAsync(true);

                var publicationId = MethodologyWithPublication.MethodologyParent.Publications[0].PublicationId;
                    
                publicationRoleRepository
                    .Setup(s => s.GetAllRolesByUser(UserId, publicationId))
                    .ReturnsAsync(new List<PublicationRole>());

                await handler.HandleAsync(authContext);
                VerifyAllMocks(methodologyRepository, publicationRoleRepository);

                // Verify that the user cannot create an Amendment, as they don't have a Publication Owner role on a
                // Publication that uses this Methodology.
                Assert.False(authContext.HasSucceeded);
            }

            [Fact]
            public async Task UserWithLinkedPublicationOwnerRoleCannotCreateAmendmentOfPubliclyAccessibleMethodologyNotOwnedByPublication()
            {
                await using var context = InMemoryApplicationDbContext(Guid.NewGuid().ToString());
                context.Attach(MethodologyWithPublicationNotOwner);

                var user = CreateClaimsPrincipal(UserId);
                var authContext = CreateAuthContext(user, MethodologyWithPublicationNotOwner);

                var (handler, methodologyRepository, publicationRoleRepository) = CreateHandlerAndDependencies(context);

                methodologyRepository
                    .Setup(mock => mock.IsPubliclyAccessible(MethodologyWithPublicationNotOwner.Id))
                    .ReturnsAsync(false);

                await handler.HandleAsync(authContext);

                // Verify that the handler does not even check the roles the user has on a Publication if the
                // Publication does not own this Methodology.
                VerifyAllMocks(methodologyRepository, publicationRoleRepository);
                Assert.False(authContext.HasSucceeded);
            }

            [Fact]
            public async Task UserWithLinkedPublicationOwnerRoleCannotCreateAmendmentOfPrivateMethodology()
            {
                await using var context = InMemoryApplicationDbContext(Guid.NewGuid().ToString());
                context.Attach(MethodologyWithPublication);

                var (handler, methodologyRepository, publicationRoleRepository) = CreateHandlerAndDependencies(context);

                methodologyRepository.Setup(mock => mock.IsPubliclyAccessible(MethodologyWithPublication.Id))
                    .ReturnsAsync(false);

                var user = CreateClaimsPrincipal(UserId);
                var authContext = CreateAuthContext(user, MethodologyWithPublication);

                await handler.HandleAsync(authContext);
                VerifyAllMocks(methodologyRepository, publicationRoleRepository);

                // Verify that the user cannot create an Amendment, as even though they have the Publication Owner role
                // on one of the Publications that use this Methodology, this Methodology is not yet publicly
                // accessible.
                Assert.False(authContext.HasSucceeded);
            }
        }

        private static AuthorizationHandlerContext CreateAuthContext(ClaimsPrincipal user, Methodology methodology)
        {
            return CreateAuthorizationHandlerContext<MakeAmendmentOfSpecificMethodologyRequirement, Methodology>
                (user, methodology);
        }

        private static (MakeAmendmentOfSpecificMethodologyAuthorizationHandler,
            Mock<IMethodologyRepository>,
            Mock<IUserPublicationRoleRepository>)
            CreateHandlerAndDependencies(ContentDbContext context)
        {
            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);
            var publicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);

            var handler = new MakeAmendmentOfSpecificMethodologyAuthorizationHandler(
                context,
                methodologyRepository.Object,
                publicationRoleRepository.Object);

            return (handler, methodologyRepository, publicationRoleRepository);
        }
    }
}
