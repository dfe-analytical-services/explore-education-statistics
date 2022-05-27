#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.
    ReleaseAuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class DeleteSpecificReleaseAuthorizationHandlerTests
    {
        public class ClaimsTests
        {
            [Fact]
            public async Task DeleteSpecificReleaseAuthorizationHandler_NotAmendment()
            {
                // Assert that no users can delete a non-amendment release
                await AssertReleaseHandlerSucceedsWithCorrectClaims<DeleteSpecificReleaseRequirement>(
                    CreateHandler,
                    new Release
                    {
                        ApprovalStatus = ReleaseApprovalStatus.Draft,
                        Version = 0
                    });
            }

            [Fact]
            public async Task DeleteSpecificReleaseAuthorizationHandler_AmendmentButApproved()
            {
                // Assert that no users can delete an amendment release that is approved
                await AssertReleaseHandlerSucceedsWithCorrectClaims<DeleteSpecificReleaseRequirement>(
                    CreateHandler,
                    new Release
                    {
                        ApprovalStatus = ReleaseApprovalStatus.Approved,
                        Version = 1
                    });
            }

            [Fact]
            public async Task DeleteSpecificReleaseAuthorizationHandler_UnapprovedAmendment()
            {
                // Assert that users with the "DeleteAllReleaseAmendments" claim can delete an amendment release that is not
                // yet approved
                await AssertReleaseHandlerSucceedsWithCorrectClaims<DeleteSpecificReleaseRequirement>(
                    CreateHandler,
                    new Release
                    {
                        ApprovalStatus = ReleaseApprovalStatus.Draft,
                        Version = 1
                    },
                    DeleteAllReleaseAmendments);
            }
        }

        public class PublicationRoleTests
        {
            [Fact]
            public async Task DeleteSpecificReleaseAuthorizationHandler_NotAmendment()
            {
                var release = new Release
                {
                    Publication = new Publication
                    {
                        Id = Guid.NewGuid()
                    },
                    ApprovalStatus = ReleaseApprovalStatus.Draft,
                    Version = 0
                };

                // Assert that no User Publication roles will allow a Release to be deleted that is not an Amendment
                await AssertReleaseHandlerSucceedsWithCorrectPublicationRoles<DeleteSpecificReleaseRequirement>(
                    CreateHandler,
                    release);
            }

            [Fact]
            public async Task DeleteSpecificReleaseAuthorizationHandler_AmendmentButApproved()
            {
                var release = new Release
                {
                    Publication = new Publication
                    {
                        Id = Guid.NewGuid()
                    },
                    ApprovalStatus = ReleaseApprovalStatus.Approved,
                    Version = 1
                };

                // Assert that no User Publication roles will allow an Amendment to be deleted when it is Approved
                await AssertReleaseHandlerSucceedsWithCorrectPublicationRoles<DeleteSpecificReleaseRequirement>(
                    CreateHandler,
                    release);
            }

            [Fact]
            public async Task DeleteSpecificReleaseAuthorizationHandler_UnapprovedAmendment()
            {
                var release = new Release
                {
                    Publication = new Publication
                    {
                        Id = Guid.NewGuid()
                    },
                    ApprovalStatus = ReleaseApprovalStatus.Draft,
                    Version = 1
                };

                // Assert that users with the Publication Owner role on the Release amendment can delete if it is not yet approved
                await AssertReleaseHandlerSucceedsWithCorrectPublicationRoles<DeleteSpecificReleaseRequirement>(
                    contentDbContext =>
                    {
                        contentDbContext.Add(release);
                        contentDbContext.SaveChanges();

                        return CreateHandler(contentDbContext);
                    },
                    release,
                    Owner);
            }
        }

        public class ReleaseRoleTests
        {
            [Fact]
            public async Task DeleteSpecificReleaseAuthorizationHandler_NotAmendment()
            {
                // Assert that no User Release roles will allow a Release to be deleted that is not an Amendment
                await AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<DeleteSpecificReleaseRequirement>(
                    CreateHandler,
                    new Release
                    {
                        Publication = new Publication
                        {
                            Id = Guid.NewGuid()
                        },
                        ApprovalStatus = ReleaseApprovalStatus.Draft,
                        Version = 0
                    });
            }

            [Fact]
            public async Task DeleteSpecificReleaseAuthorizationHandler_AmendmentButApproved()
            {
                // Assert that no User Release roles will allow an Amendment to be deleted when it is Approved
                await AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<DeleteSpecificReleaseRequirement>(
                    CreateHandler,
                    new Release
                    {
                        Publication = new Publication
                        {
                            Id = Guid.NewGuid()
                        },
                        ApprovalStatus = ReleaseApprovalStatus.Approved,
                        Version = 1
                    });
            }

            [Fact]
            public async Task DeleteSpecificReleaseAuthorizationHandler_UnapprovedAmendment()
            {
                // Assert that no User Release roles will allow an Amendment to be deleted if it is not yet approved
                await AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<DeleteSpecificReleaseRequirement>(
                    CreateHandler,
                    new Release
                    {
                        Publication = new Publication
                        {
                            Id = Guid.NewGuid()
                        },
                        ApprovalStatus = ReleaseApprovalStatus.Draft,
                        Version = 1
                    });
            }
        }

        private static DeleteSpecificReleaseAuthorizationHandler CreateHandler(ContentDbContext contentDbContext)
        {
            return new DeleteSpecificReleaseAuthorizationHandler(
                new AuthorizationHandlerResourceRoleService(
                    new UserReleaseRoleRepository(contentDbContext),
                    new UserPublicationRoleRepository(contentDbContext),
                    Mock.Of<IPublicationRepository>(MockBehavior.Strict)));
        }
    }
}
