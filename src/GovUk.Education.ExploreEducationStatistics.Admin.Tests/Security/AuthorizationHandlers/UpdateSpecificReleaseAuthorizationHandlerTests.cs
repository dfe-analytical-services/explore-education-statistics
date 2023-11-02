#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.ReleaseAuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.EnumUtil;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseApprovalStatus;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.ReleasePublishingStatusOverallStage;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class UpdateSpecificReleaseAuthorizationHandlerTests
    {
        public class ClaimTests
        {
            // Test that Releases that are in any approval state other than Approved can be updated
            // if the current user has the "UpdateAllReleases" Claim.
            [Fact]
            public async Task UpdateAllReleases_ClaimSucceedsIfNotApproved()
            {
                await GetEnumValues<ReleaseApprovalStatus>()
                    .ToAsyncEnumerable()
                    .ForEachAwaitAsync(async status =>
                    {
                        if (status == Approved)
                        {
                            return;
                        }

                        var release = new Release
                        {
                            Id = Guid.NewGuid(),
                            ApprovalStatus = status
                        };

                        await AssertReleaseHandlerSucceedsWithCorrectClaims<UpdateSpecificReleaseRequirement>(
                            HandlerSupplier(release),
                            release,
                            SecurityClaimTypes.UpdateAllReleases
                        );
                    });
            }

            // Test that Releases that are Approved cannot be updated by a user having any Claim.
            [Fact]
            public async Task AllClaimsFailIfApproved()
            {
                var release = new Release
                {
                    Id = Guid.NewGuid(),
                    ApprovalStatus = Approved
                };

                await AssertReleaseHandlerSucceedsWithCorrectClaims<UpdateSpecificReleaseRequirement>(
                    HandlerSupplier(release),
                    release,
                    claimsExpectedToSucceed: Array.Empty<SecurityClaimTypes>());
            }
        }

        public class PublicationRoleTests
        {
            [Fact]
            public async Task PublicationOwnerAndApproversCanUpdateUnapprovedRelease()
            {
                await GetEnumValues<ReleaseApprovalStatus>()
                    .ToAsyncEnumerable()
                    .ForEachAwaitAsync(
                        async status =>
                        {
                            if (status == Approved)
                            {
                                return;
                            }

                            var release = new Release
                            {
                                Id = Guid.NewGuid(),
                                Publication = new Publication
                                {
                                    Id = Guid.NewGuid()
                                },
                                Published = null,
                                ApprovalStatus = status
                            };

                            // Assert that a Publication Owner or Approver can update the Release in any approval
                            // state other than Admin
                            await AssertReleaseHandlerSucceedsWithCorrectPublicationRoles<
                                UpdateSpecificReleaseRequirement>(
                                HandlerSupplier(release),
                                release,
                                rolesExpectedToSucceed: new []
                                {
                                    PublicationRole.Owner,
                                    PublicationRole.Approver
                                });
                        }
                    );
            }

            [Fact]
            public async Task NoRolesCanUpdateApprovedRelease()
            {
                var release = new Release
                {
                    Id = Guid.NewGuid(),
                    Publication = new Publication
                    {
                        Id = Guid.NewGuid()
                    },
                    Published = null,
                    ApprovalStatus = Approved
                };

                // Assert that no Publication Role can update the Release if it is Approved.
                await AssertReleaseHandlerSucceedsWithCorrectPublicationRoles<
                    UpdateSpecificReleaseRequirement>(
                    HandlerSupplier(release),
                    release,
                    rolesExpectedToSucceed: Array.Empty<PublicationRole>());
            }
        }

        public class ReleaseRoleTests
        {
            [Fact]
            public async Task EditorsCanUpdateUnapprovedRelease()
            {
                await GetEnumValues<ReleaseApprovalStatus>()
                    .ToAsyncEnumerable()
                    .ForEachAwaitAsync(
                        async status =>
                        {
                            if (status == Approved)
                            {
                                return;
                            }

                            var release = new Release
                            {
                                Id = Guid.NewGuid(),
                                Publication = new Publication
                                {
                                    Id = Guid.NewGuid()
                                },
                                Published = null,
                                ApprovalStatus = status
                            };

                            // Assert that a Release Editor (Contributor, Lead, Approver) can update the Release
                            // in any approval state other than Approved.
                            await AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<
                                UpdateSpecificReleaseRequirement>(
                                HandlerSupplier(release),
                                release,
                                rolesExpectedToSucceed: new[]
                                {
                                    ReleaseRole.Contributor,
                                    ReleaseRole.Lead,
                                    ReleaseRole.Approver
                                });
                        }
                    );
            }

            [Fact]
            public async Task NoRolesCanUpdateApprovedRelease()
            {
                var release = new Release
                {
                    Id = Guid.NewGuid(),
                    Publication = new Publication
                    {
                        Id = Guid.NewGuid()
                    },
                    Published = null,
                    ApprovalStatus = Approved
                };

                // Assert that no Publication Role can update the Release if it is Approved.
                await AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<
                    UpdateSpecificReleaseRequirement>(
                    HandlerSupplier(release),
                    release,
                    rolesExpectedToSucceed: Array.Empty<ReleaseRole>());
            }
        }

        private static Func<ContentDbContext, UpdateSpecificReleaseAuthorizationHandler> HandlerSupplier(
            Release release,
            List<ReleasePublishingStatus>? publishingStatuses = null)
        {
            var releaseStatusRepository = new Mock<IReleasePublishingStatusRepository>();

            releaseStatusRepository.Setup(
                    s => s.GetAllByOverallStage(
                        release.Id,
                        Started,
                        Complete
                    )
                )
                .ReturnsAsync(publishingStatuses ?? new List<ReleasePublishingStatus>());

            return contentDbContext =>
            {
                contentDbContext.Add(release);
                contentDbContext.SaveChanges();

                return new UpdateSpecificReleaseAuthorizationHandler(
                    new AuthorizationHandlerResourceRoleService(
                        new UserReleaseRoleRepository(contentDbContext),
                        new UserPublicationRoleRepository(contentDbContext)));
            };
        }
    }
}
