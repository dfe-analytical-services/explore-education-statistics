#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.
    ReleaseAuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseRole;

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
                // Assert that no users can assign pre release contacts to a release that's not approved
                await AssertReleaseHandlerSucceedsWithCorrectClaims
                    <AssignPrereleaseContactsToSpecificReleaseRequirement>(
                        contentDbContext =>
                            new AssignPrereleaseContactsToSpecificReleaseAuthorizationHandler(
                                new UserPublicationRoleRepository(contentDbContext),
                                new UserReleaseRoleRepository(contentDbContext)),
                        new Release
                        {
                            ApprovalStatus = ReleaseApprovalStatus.Draft
                        });
            }

            [Fact]
            public async Task AssignPrereleaseContactsToSpecificReleaseAuthorizationHandler_Approved()
            {
                // Assert that users with the "UpdateAllReleases" claim can assign pre release contacts to a release that's approved
                await AssertReleaseHandlerSucceedsWithCorrectClaims
                    <AssignPrereleaseContactsToSpecificReleaseRequirement>(
                        contentDbContext =>
                            new AssignPrereleaseContactsToSpecificReleaseAuthorizationHandler(
                                new UserPublicationRoleRepository(contentDbContext),
                                new UserReleaseRoleRepository(contentDbContext)),
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

                // Assert that no User Publication roles will allow assigning pre release contacts to a release that's not approved
                await AssertReleaseHandlerSucceedsWithCorrectPublicationRoles<
                    AssignPrereleaseContactsToSpecificReleaseRequirement>(
                    contentDbContext =>
                    {
                        contentDbContext.Add(release);
                        contentDbContext.SaveChanges();

                        return new AssignPrereleaseContactsToSpecificReleaseAuthorizationHandler(
                            new UserPublicationRoleRepository(contentDbContext),
                            new UserReleaseRoleRepository(contentDbContext));
                    },
                    release);
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

                // Assert that users with the Publication Owner role can assign pre release contacts to a release that's approved
                await AssertReleaseHandlerSucceedsWithCorrectPublicationRoles<
                    AssignPrereleaseContactsToSpecificReleaseRequirement>(
                    contentDbContext =>
                    {
                        contentDbContext.Add(release);
                        contentDbContext.SaveChanges();

                        return new AssignPrereleaseContactsToSpecificReleaseAuthorizationHandler(
                            new UserPublicationRoleRepository(contentDbContext),
                            new UserReleaseRoleRepository(contentDbContext));
                    },
                    release,
                    Owner);
            }
        }

        public class ReleaseRoleTests
        {
            [Fact]
            public async Task AssignPrereleaseContactsToSpecificReleaseAuthorizationHandler_NotApproved()
            {
                // Assert that no User Release roles will allow assigning pre release contacts to a release that's not approved
                await AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<
                    AssignPrereleaseContactsToSpecificReleaseRequirement>(
                    contentDbContext =>
                        new AssignPrereleaseContactsToSpecificReleaseAuthorizationHandler(
                            new UserPublicationRoleRepository(contentDbContext),
                            new UserReleaseRoleRepository(contentDbContext)),
                    new Release
                    {
                        ApprovalStatus = ReleaseApprovalStatus.Draft
                    });
            }

            [Fact]
            public async Task AssignPrereleaseContactsToSpecificReleaseAuthorizationHandler_Approved()
            {
                // Assert that users with an editor User Release role can assign pre release contacts to a release that's approved
                await AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<
                    AssignPrereleaseContactsToSpecificReleaseRequirement>(
                    contentDbContext =>
                        new AssignPrereleaseContactsToSpecificReleaseAuthorizationHandler(
                            new UserPublicationRoleRepository(contentDbContext),
                            new UserReleaseRoleRepository(contentDbContext)),
                    new Release
                    {
                        ApprovalStatus = ReleaseApprovalStatus.Approved
                    },
                    Approver, Contributor, Lead);
            }
        }
    }
}
