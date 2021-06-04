using System;
using System.Collections.Generic;
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class MakeAmendmentOfSpecificMethodologyAuthorizationHandlerTests
    {
        public class MakeAmendmentOfSpecificMethodologyAuthorizationHandlerClaimTests
        {
            private static readonly Guid UserId = Guid.NewGuid();
            
            private static readonly Publication Publication = new Publication
            {
                Id = Guid.NewGuid()
            };

            private static readonly Methodology PublicMethodology = new Methodology
            {
                Status = Approved,
                PublishingStrategy = MethodologyPublishingStrategy.Immediately
            };
                
            private static readonly Methodology PrivateMethodology = new Methodology
            {
                Status = Draft
            };
            
            private static readonly Methodology PublicMethodologyWithPublication = new Methodology
            {
                Status = Approved,
                PublishingStrategy = MethodologyPublishingStrategy.Immediately,
                Publications = AsList(Publication)
            };
            
            private static readonly Methodology PrivateMethodologyWithPublication = new Methodology
            {
                Status = Draft,
                Publications = AsList(Publication)
            };
            
            [Fact]
            public async void UserWithCorrectClaimCanCreateAmendmentOfPubliclyAccessibleMethodology()
            {
                await ForEachSecurityClaimAsync(async claim => {
                    
                    var (handler, publicationRoleRepository) = CreateHandlerAndDependencies();
                    
                    var user = CreateClaimsPrincipal(UserId, claim);
                    var authContext = CreateAuthContext(user, PublicMethodology);
                    
                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(publicationRoleRepository);

                    // Verify that only the combination of the "MakeAmendmentsOfAllMethodologies" Claim
                    // alongside the publicly-accessible Methodology will pass the handler test
                    var expectedToPass = claim == MakeAmendmentsOfAllMethodologies;
                    Assert.Equal(expectedToPass, authContext.HasSucceeded);
                });
            }
            
            [Fact]
            public async void UserWithCorrectClaimCannotCreateAmendmentOfPrivateMethodology()
            {
                await ForEachSecurityClaimAsync(async claim => 
                {
                    var (handler, publicationRoleRepository) = CreateHandlerAndDependencies();
                    
                    var user = CreateClaimsPrincipal(UserId, claim);
                    var authContext = CreateAuthContext(user, PrivateMethodology);
                    
                    await handler.HandleAsync(authContext);
                    VerifyAllMocks(publicationRoleRepository);

                    // Verify that no Claims can allow a user to create an Amendment of a non-publicly-accessible
                    // Methodology. 
                    Assert.False(authContext.HasSucceeded);
                });
            }
            
            [Fact]
            public async void UserWithLinkedPublicationOwnerRoleCanCreateAmendmentOfPubliclyAccessibleMethodology()
            {
                var (handler, publicationRoleRepository) = CreateHandlerAndDependencies();
                    
                var user = CreateClaimsPrincipal(UserId);
                var authContext = CreateAuthContext(user, PublicMethodologyWithPublication);
                
                publicationRoleRepository.Setup(s => 
                        s.GetAllRolesByUser(UserId, Publication.Id))
                    .ReturnsAsync(AsList(PublicationRole.Owner));

                await handler.HandleAsync(authContext);
                VerifyAllMocks(publicationRoleRepository);

                // Verify that the user can create an Amendment, as they have a Publication Owner role on a Publication
                // that uses this Methodology.
                Assert.True(authContext.HasSucceeded);
            }
            
            [Fact]
            public async void UserWithoutLinkedPublicationOwnerRoleCannotCreateAmendmentOfPubliclyAccessibleMethodology()
            {
                var (handler, publicationRoleRepository) = CreateHandlerAndDependencies();
                    
                var user = CreateClaimsPrincipal(UserId);
                var authContext = CreateAuthContext(user, PublicMethodologyWithPublication);
                
                publicationRoleRepository.Setup(s => 
                        s.GetAllRolesByUser(UserId, Publication.Id))
                    .ReturnsAsync(new List<PublicationRole>());

                await handler.HandleAsync(authContext);
                VerifyAllMocks(publicationRoleRepository);

                // Verify that the user cannot create an Amendment, as they don't have a Publication Owner role on a
                // Publication that uses this Methodology.
                Assert.False(authContext.HasSucceeded);
            }
            
            [Fact]
            public async void UserWithLinkedPublicationOwnerRoleCannotCreateAmendmentOfPrivateMethodology()
            {
                var (handler, publicationRoleRepository) = CreateHandlerAndDependencies();
                    
                var user = CreateClaimsPrincipal(UserId);
                var authContext = CreateAuthContext(user, PrivateMethodologyWithPublication);
                
                await handler.HandleAsync(authContext);
                VerifyAllMocks(publicationRoleRepository);

                // Verify that the user cannot create an Amendment, as even though they have the Publication Owner role
                // on one of the Publications that use this Methodology, this Methodology is not yet publicly
                // accessible.
                Assert.False(authContext.HasSucceeded);
            }

            private static AuthorizationHandlerContext CreateAuthContext(ClaimsPrincipal user, Methodology methodology)
            {
                return CreateAuthorizationHandlerContext<MakeAmendmentOfSpecificMethodologyRequirement, Methodology>
                    (user, methodology);
            }

            private static (MakeAmendmentOfSpecificMethodologyAuthorizationHandler, Mock<IUserPublicationRoleRepository>)
                CreateHandlerAndDependencies()
            {
                var publicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);

                var handler = new MakeAmendmentOfSpecificMethodologyAuthorizationHandler(
                    publicationRoleRepository.Object);

                return (handler, publicationRoleRepository);
            }
        }
    }
}
