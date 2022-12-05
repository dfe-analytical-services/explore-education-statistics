#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.ReleaseAuthorizationHandlersTestUtil;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class AssignPrereleaseContactsToSpecificReleaseAuthorizationHandlerTests
    {
        public class ClaimsTests
        {
            [Fact]
            public async Task AssignPrereleaseContactsToSpecificReleaseAuthorizationHandler_NotApproved()
            {
                // Assert that users with the "UpdateAllReleases" claim can assign pre release contacts to a release
                // that's unapproved
                await AssertReleaseHandlerSucceedsWithCorrectClaims
                    <AssignPrereleaseContactsToSpecificReleaseRequirement>(
                        CreateHandler,
                        new Release
                        {
                            ApprovalStatus = ReleaseApprovalStatus.Draft
                        },
                        UpdateAllReleases);
            }

            [Fact]
            public async Task AssignPrereleaseContactsToSpecificReleaseAuthorizationHandler_Approved()
            {
                // Assert that users with the "UpdateAllReleases" claim can assign pre release contacts to a release
                // that's approved
                await AssertReleaseHandlerSucceedsWithCorrectClaims
                    <AssignPrereleaseContactsToSpecificReleaseRequirement>(
                        CreateHandler,
                        new Release
                        {
                            ApprovalStatus = ReleaseApprovalStatus.Approved
                        },
                        UpdateAllReleases);
            }
        }

        public class AssignPrereleaseContactsToSpecificReleaseAuthorizationHandlerPublicationRoleTests
        {
            [Fact]
            public async Task AssignPrereleaseContactsToSpecificReleaseAuthorizationHandler_NotApproved()
            {
                var release = new Release
                {
                    Publication = new Publication
                    {
                        Id = Guid.NewGuid()
                    },
                    ApprovalStatus = ReleaseApprovalStatus.Draft
                };

                // Assert that users with the Publication Owner role can assign pre release contacts to a release
                // that's unapproved
                await AssertReleaseHandlerSucceedsWithCorrectPublicationRoles<
                    AssignPrereleaseContactsToSpecificReleaseRequirement>(
                    contentDbContext =>
                    {
                        contentDbContext.Add(release);
                        contentDbContext.SaveChanges();

                        return CreateHandler(contentDbContext);
                    },
                    release,
                    PublicationRole.Owner, PublicationRole.Approver);
            }

            [Fact]
            public async Task AssignPrereleaseContactsToSpecificReleaseAuthorizationHandler_Approved()
            {
                var release = new Release
                {
                    Publication = new Publication
                    {
                        Id = Guid.NewGuid()
                    },
                    ApprovalStatus = ReleaseApprovalStatus.Approved
                };

                // Assert that users with the Publication Owner role can assign pre release contacts to a release
                // that's approved
                await AssertReleaseHandlerSucceedsWithCorrectPublicationRoles<
                    AssignPrereleaseContactsToSpecificReleaseRequirement>(
                    contentDbContext =>
                    {
                        contentDbContext.Add(release);
                        contentDbContext.SaveChanges();

                        return CreateHandler(contentDbContext);
                    },
                    release,
                    PublicationRole.Owner, PublicationRole.Approver);
            }
        }

        public class ReleaseRoleTests
        {
            [Fact]
            public async Task AssignPrereleaseContactsToSpecificReleaseAuthorizationHandler_NotApproved()
            {
                // Assert that users with an editor User Release role can assign pre release contacts to a release
                // that's unapproved
                await AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<
                    AssignPrereleaseContactsToSpecificReleaseRequirement>(
                    CreateHandler,
                    new Release
                    {
                        ApprovalStatus = ReleaseApprovalStatus.Draft
                    },
                    ReleaseRole.Approver, ReleaseRole.Contributor, ReleaseRole.Lead);
            }

            [Fact]
            public async Task AssignPrereleaseContactsToSpecificReleaseAuthorizationHandler_Approved()
            {
                // Assert that users with an editor User Release role can assign pre release contacts to a release
                // that's approved
                await AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<
                    AssignPrereleaseContactsToSpecificReleaseRequirement>(
                    CreateHandler,
                    new Release
                    {
                        ApprovalStatus = ReleaseApprovalStatus.Approved
                    },
                    ReleaseRole.Approver, ReleaseRole.Contributor, ReleaseRole.Lead);
            }
        }

        private static IAuthorizationHandler CreateHandler(ContentDbContext contentDbContext)
        {
            return new AssignPrereleaseContactsToSpecificReleaseAuthorizationHandler(
                new AuthorizationHandlerResourceRoleService(
                    new UserReleaseRoleRepository(contentDbContext),
                    new UserPublicationRoleRepository(contentDbContext),
                    Mock.Of<IPublicationRepository>(Strict))
            );
        }
    }
}
