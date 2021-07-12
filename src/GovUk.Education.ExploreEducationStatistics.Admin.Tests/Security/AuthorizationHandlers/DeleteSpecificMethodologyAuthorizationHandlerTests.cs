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
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class DeleteSpecificMethodologyAuthorizationHandlerTests
    {
        private static readonly Guid UserId = Guid.NewGuid();
        private static readonly Guid PublicationId = Guid.NewGuid();
            
        public class DeleteSpecificMethodologyAuthorizationHandlerClaimTests
        {
            private static readonly Methodology DraftMethodology = new Methodology
            {
                Id = Guid.NewGuid(),
                MethodologyParent = new MethodologyParent
                {
                    Publications = AsList(new PublicationMethodology
                    {
                        Owner = true,
                        PublicationId = PublicationId
                    })
                }
            };

            private static readonly Methodology ApprovedMethodology = new Methodology
            {
                Id = Guid.NewGuid(),
                Status = Approved,
                MethodologyParent = new MethodologyParent
                {
                    Publications = AsList(new PublicationMethodology
                    {
                        Owner = true,
                        PublicationId = PublicationId
                    })
                }
            };
        
            [Fact]
            public async Task UserWithCorrectClaimCanDeleteNonPublicDraftMethodology()
            {
                await ForEachSecurityClaimAsync(async claim =>
                {
                    var expectClaimToSucceed = claim == DeleteAllMethodologies;

                    await using var context = InMemoryApplicationDbContext(Guid.NewGuid().ToString());
                    context.Attach(DraftMethodology);

                    var (handler, methodologyRepository, publicationRoleRepository) =
                        CreateHandlerAndDependencies(context);

                    methodologyRepository
                        .Setup(s => s.IsPubliclyAccessible(DraftMethodology.Id))
                        .ReturnsAsync(false);

                    // If the Claim given to the handler isn't enough to make the handler succeed, it'll go on to check
                    // the user's Publication Roles.  
                    if (!expectClaimToSucceed)
                    {
                        publicationRoleRepository
                            .Setup(s => s.GetAllRolesByUser(UserId, PublicationId))
                            .ReturnsAsync(new List<PublicationRole>());
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
                    await using var context = InMemoryApplicationDbContext(Guid.NewGuid().ToString());
                    context.Attach(DraftMethodology);

                    var (handler, methodologyRepository, publicationRoleRepository) =
                        CreateHandlerAndDependencies(context);

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
                    await using var context = InMemoryApplicationDbContext(Guid.NewGuid().ToString());
                    context.Attach(ApprovedMethodology);

                    var (handler, methodologyRepository, publicationRoleRepository) =
                        CreateHandlerAndDependencies(context);

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
            private static readonly Methodology OwnedMethodology = new Methodology
            {
                Id = Guid.NewGuid(),
                MethodologyParent = new MethodologyParent
                {
                    Publications = AsList(new PublicationMethodology
                    {
                        Owner = true,
                        PublicationId = PublicationId
                    })
                }
            };
            
            private static readonly Methodology AdoptedMethodology = new Methodology
            {
                Id = Guid.NewGuid(),
                MethodologyParent = new MethodologyParent
                {
                    Publications = AsList(new PublicationMethodology
                    {
                        Owner = false,
                        PublicationId = PublicationId
                    })
                }
            };
            
            private static readonly Methodology ApprovedMethodology = new Methodology
            {
                Id = Guid.NewGuid(),
                Status = Approved,
                MethodologyParent = new MethodologyParent
                {
                    Publications = AsList(new PublicationMethodology
                    {
                        Owner = true,
                        PublicationId = PublicationId
                    })
                }
            };
            
            [Fact]
            public async Task UserWithPublicationOwnerRoleCanDeleteNonPublicDraftMethodologyOfOwningPublications()
            {
                await GetEnumValues<PublicationRole>().ForEachAsync(async role =>
                {
                    await using var context = InMemoryApplicationDbContext(Guid.NewGuid().ToString());
                    context.Attach(OwnedMethodology);

                    var (handler, methodologyRepository, publicationRoleRepository) =
                        CreateHandlerAndDependencies(context);

                    methodologyRepository
                        .Setup(s => s.IsPubliclyAccessible(OwnedMethodology.Id))
                        .ReturnsAsync(false);

                    publicationRoleRepository
                        .Setup(s => s.GetAllRolesByUser(UserId, PublicationId))
                        .ReturnsAsync(AsList(role));
                    
                    var user = CreateClaimsPrincipal(UserId);
                    var authContext = CreateAuthContext(user, OwnedMethodology);

                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(methodologyRepository, publicationRoleRepository);

                    // Verify that the presence of a Publication Owner role for this user that is related to a
                    // Publication that uses this Methodology will pass the handler test
                    Assert.Equal(role == PublicationRole.Owner, authContext.HasSucceeded);
                });
            }
            
            [Fact]
            public async Task UserWithNoPublicationRolesCannotDeleteNonPublicDraftMethodologyOfOwningPublications()
            {
                await using var context = InMemoryApplicationDbContext(Guid.NewGuid().ToString());
                context.Attach(OwnedMethodology);

                var (handler, methodologyRepository, publicationRoleRepository) =
                    CreateHandlerAndDependencies(context);

                methodologyRepository
                    .Setup(s => s.IsPubliclyAccessible(OwnedMethodology.Id))
                    .ReturnsAsync(false);

                publicationRoleRepository
                    .Setup(s => s.GetAllRolesByUser(UserId, PublicationId))
                    .ReturnsAsync(new List<PublicationRole>());
                
                var user = CreateClaimsPrincipal(UserId);
                var authContext = CreateAuthContext(user, OwnedMethodology);

                await handler.HandleAsync(authContext);
                VerifyAllMocks(methodologyRepository, publicationRoleRepository);

                // Verify that the user cannot perform this action if they have no Publication Roles.
                Assert.False(authContext.HasSucceeded);
            }
            
            [Fact]
            public async Task UserWithPublicationOwnerRoleCannotDeleteNonPublicDraftMethodologyOfNonOwningPublications()
            {
                await using var context = InMemoryApplicationDbContext(Guid.NewGuid().ToString());
                context.Attach(AdoptedMethodology);

                var (handler, methodologyRepository, publicationRoleRepository) =
                    CreateHandlerAndDependencies(context);

                methodologyRepository
                    .Setup(s => s.IsPubliclyAccessible(AdoptedMethodology.Id))
                    .ReturnsAsync(false);

                var user = CreateClaimsPrincipal(UserId);
                var authContext = CreateAuthContext(user, AdoptedMethodology);

                await handler.HandleAsync(authContext);

                // Verify that the fact that the Publication related to this Methodology doesn't own it stops the 
                // handler for even needing to check if the user has any Publication Owner role on that Publication.
                VerifyAllMocks(methodologyRepository, publicationRoleRepository);
                Assert.False(authContext.HasSucceeded);
            }
            
            [Fact]
            public async Task UserWithPublicationOwnerRoleCannotDeletePubliclyAccessibleMethodology()
            {
                await using var context = InMemoryApplicationDbContext(Guid.NewGuid().ToString());
                context.Attach(OwnedMethodology);

                var (handler, methodologyRepository, publicationRoleRepository) =
                    CreateHandlerAndDependencies(context);

                methodologyRepository
                    .Setup(s => s.IsPubliclyAccessible(OwnedMethodology.Id))
                    .ReturnsAsync(true);

                var user = CreateClaimsPrincipal(UserId);
                var authContext = CreateAuthContext(user, OwnedMethodology);

                await handler.HandleAsync(authContext);

                // Verify that the fact that the Methodology is publicly accessible doesn't even need to check the
                // users' Publication Roles to determine whether or not they can do this.
                VerifyAllMocks(methodologyRepository, publicationRoleRepository);
                Assert.False(authContext.HasSucceeded);
            }
            
            [Fact]
            public async Task UserWithPublicationOwnerRoleCannotDeleteApprovedMethodology()
            {
                await using var context = InMemoryApplicationDbContext(Guid.NewGuid().ToString());
                context.Attach(ApprovedMethodology);

                var (handler, methodologyRepository, publicationRoleRepository) =
                    CreateHandlerAndDependencies(context);

                methodologyRepository
                    .Setup(s => s.IsPubliclyAccessible(ApprovedMethodology.Id))
                    .ReturnsAsync(true);

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
            CreateHandlerAndDependencies(ContentDbContext context)
        {
            var publicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);

            var handler = new DeleteSpecificMethodologyAuthorizationHandler(
                context,
                methodologyRepository.Object,
                publicationRoleRepository.Object);

            return (handler, methodologyRepository, publicationRoleRepository);
        }
    }
}
