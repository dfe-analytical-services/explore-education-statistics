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
    AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.
    ReleaseAuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;
using static Moq.MockBehavior;
using ReleaseRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.ReleaseRepository;

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
                // Assert that no users can delete the first version of a release
                await AssertHandlerSucceedsWithCorrectClaims<ReleaseVersion, DeleteSpecificReleaseRequirement>(
                    CreateHandler,
                    new ReleaseVersion
                    {
                        ApprovalStatus = ReleaseApprovalStatus.Draft,
                        Version = 0
                    });
            }

            [Fact]
            public async Task DeleteSpecificReleaseAuthorizationHandler_AmendmentButApproved()
            {
                // Assert that no users can delete an amendment release version that is approved
                await AssertHandlerSucceedsWithCorrectClaims<ReleaseVersion, DeleteSpecificReleaseRequirement>(
                    CreateHandler,
                    new ReleaseVersion
                    {
                        ApprovalStatus = ReleaseApprovalStatus.Approved,
                        Version = 1
                    });
            }

            [Fact]
            public async Task DeleteSpecificReleaseAuthorizationHandler_UnapprovedAmendment()
            {
                // Assert that users with the "DeleteAllReleaseAmendments" claim can delete an amendment release version that is not yet approved
                await AssertHandlerSucceedsWithCorrectClaims<ReleaseVersion, DeleteSpecificReleaseRequirement>(
                    CreateHandler,
                    new ReleaseVersion
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
                var releaseVersion = new ReleaseVersion
                {
                    Publication = new Publication
                    {
                        Id = Guid.NewGuid()
                    },
                    ApprovalStatus = ReleaseApprovalStatus.Draft,
                    Version = 0
                };

                // Assert that no User Publication roles will allow deleting the first version of a release
                await AssertReleaseHandlerSucceedsWithCorrectPublicationRoles<DeleteSpecificReleaseRequirement>(
                    CreateHandler,
                    releaseVersion);
            }

            [Fact]
            public async Task DeleteSpecificReleaseAuthorizationHandler_AmendmentButApproved()
            {
                var releaseVersion = new ReleaseVersion
                {
                    Publication = new Publication
                    {
                        Id = Guid.NewGuid()
                    },
                    ApprovalStatus = ReleaseApprovalStatus.Approved,
                    Version = 1
                };

                // Assert that no User Publication roles will allow deleting an amendment release version when it is Approved
                await AssertReleaseHandlerSucceedsWithCorrectPublicationRoles<DeleteSpecificReleaseRequirement>(
                    CreateHandler,
                    releaseVersion);
            }

            [Fact]
            public async Task DeleteSpecificReleaseAuthorizationHandler_UnapprovedAmendment()
            {
                var releaseVersion = new ReleaseVersion
                {
                    Publication = new Publication
                    {
                        Id = Guid.NewGuid()
                    },
                    ApprovalStatus = ReleaseApprovalStatus.Draft,
                    Version = 1
                };

                // Assert that users with the Publication Owner role on an amendment release version can delete if it is not yet approved
                await AssertReleaseHandlerSucceedsWithCorrectPublicationRoles<DeleteSpecificReleaseRequirement>(
                    contentDbContext =>
                    {
                        contentDbContext.Add(releaseVersion);
                        contentDbContext.SaveChanges();

                        return CreateHandler(contentDbContext);
                    },
                    releaseVersion,
                    Owner);
            }
        }

        public class ReleaseRoleTests
        {
            [Fact]
            public async Task DeleteSpecificReleaseAuthorizationHandler_NotAmendment()
            {
                // Assert that no User Release roles will allow deleting the first version of a release
                await AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<DeleteSpecificReleaseRequirement>(
                    CreateHandler,
                    new ReleaseVersion
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
                // Assert that no User Release roles will allow deleting an amendment release version when it is Approved
                await AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<DeleteSpecificReleaseRequirement>(
                    CreateHandler,
                    new ReleaseVersion
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
                // Assert that no User Release roles will allow an amendment release version to be deleted if it is not yet approved
                await AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<DeleteSpecificReleaseRequirement>(
                    CreateHandler,
                    new ReleaseVersion
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
                new AuthorizationHandlerService(
                    new ReleaseRepository(contentDbContext),
                    new UserReleaseRoleRepository(contentDbContext),
                    new UserPublicationRoleRepository(contentDbContext),
                    Mock.Of<IPreReleaseService>(Strict)));
        }
    }
}
