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
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.
    AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.
    ReleaseAuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.EnumUtil;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseApprovalStatus;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.ReleasePublishingStatusOverallStage;
using static Moq.MockBehavior;
using ReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.ReleaseVersionRepository;

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
                await GetEnums<ReleaseApprovalStatus>()
                    .Where(releaseStatus => releaseStatus != Approved)
                    .ToAsyncEnumerable()
                    .ForEachAwaitAsync(async status =>
                    {
                        var releaseVersion = new ReleaseVersion
                        {
                            Id = Guid.NewGuid(),
                            ApprovalStatus = status
                        };

                        await AssertHandlerSucceedsWithCorrectClaims<ReleaseVersion, UpdateSpecificReleaseRequirement>(
                            HandlerSupplier(releaseVersion),
                            releaseVersion,
                            SecurityClaimTypes.UpdateAllReleases
                        );
                    });
            }

            // Test that Releases that are Approved cannot be updated by a user having any Claim.
            [Fact]
            public async Task AllClaimsFailIfApproved()
            {
                var releaseVersion = new ReleaseVersion
                {
                    Id = Guid.NewGuid(),
                    ApprovalStatus = Approved
                };

                await AssertHandlerSucceedsWithCorrectClaims<ReleaseVersion, UpdateSpecificReleaseRequirement>(
                    HandlerSupplier(releaseVersion),
                    releaseVersion,
                    claimsExpectedToSucceed: Array.Empty<SecurityClaimTypes>());
            }
        }

        public class PublicationRoleTests
        {
            [Fact]
            public async Task PublicationOwnerAndApproversCanUpdateUnapprovedRelease()
            {
                await GetEnums<ReleaseApprovalStatus>()
                    .Where(releaseStatus => releaseStatus != Approved)
                    .ToAsyncEnumerable()
                    .ForEachAwaitAsync(
                        async status =>
                        {
                            var releaseVersion = new ReleaseVersion
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
                                HandlerSupplier(releaseVersion),
                                releaseVersion,
                                rolesExpectedToSucceed: new[]
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
                var releaseVersion = new ReleaseVersion
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
                    HandlerSupplier(releaseVersion),
                    releaseVersion,
                    rolesExpectedToSucceed: Array.Empty<PublicationRole>());
            }
        }

        public class ReleaseRoleTests
        {
            [Fact]
            public async Task EditorsCanUpdateUnapprovedRelease()
            {
                await GetEnums<ReleaseApprovalStatus>()
                    .Where(releaseStatus => releaseStatus != Approved)
                    .ToAsyncEnumerable()
                    .ForEachAwaitAsync(
                        async status =>
                        {
                            var releaseVersion = new ReleaseVersion
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
                                HandlerSupplier(releaseVersion),
                                releaseVersion,
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
                var releaseVersion = new ReleaseVersion
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
                    HandlerSupplier(releaseVersion),
                    releaseVersion,
                    rolesExpectedToSucceed: Array.Empty<ReleaseRole>());
            }
        }

        private static Func<ContentDbContext, UpdateSpecificReleaseAuthorizationHandler> HandlerSupplier(
            ReleaseVersion releaseVersion,
            List<ReleasePublishingStatus>? publishingStatuses = null)
        {
            var releaseStatusRepository = new Mock<IReleasePublishingStatusRepository>();

            releaseStatusRepository.Setup(
                    s => s.GetAllByOverallStage(
                        releaseVersion.Id,
                        Started,
                        Complete
                    )
                )
                .ReturnsAsync(publishingStatuses ?? new List<ReleasePublishingStatus>());

            return contentDbContext =>
            {
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                contentDbContext.SaveChanges();

                return new UpdateSpecificReleaseAuthorizationHandler(
                    new AuthorizationHandlerService(
                        new ReleaseVersionRepository(contentDbContext),
                        new UserReleaseRoleRepository(contentDbContext),
                        new UserPublicationRoleRepository(contentDbContext),
                        Mock.Of<IPreReleaseService>(Strict)));
            };
        }
    }
}
