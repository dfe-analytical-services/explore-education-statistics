using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.AspNetCore.Authorization;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.
    ReleaseAuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.EnumUtil;
using PublisherReleaseStatus = GovUk.Education.ExploreEducationStatistics.Publisher.Model.ReleaseStatus;
using ReleaseStatus = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseStatus;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class ReleaseStatusAuthorizationHandlersTests
    {
        public class MarkReleaseAsDraftAuthorizationHandlerTests
        {
            [Fact]
            public void ClaimSuccess_MarkAllReleasesAsDraft_ReleaseUnpublished()
            {
                AssertClaimSucceedsWhenReleaseUnpublished<MarkReleaseAsDraftRequirement>(
                    (context, releaseStatusRepository) =>
                        new MarkReleaseAsDraftAuthorizationHandler(context, releaseStatusRepository),
                    MarkAllReleasesAsDraft
                );
            }

            [Fact]
            public void AllClaimsFail_ReleasePublishing()
            {
                AssertAllClaimsFailWhenReleasePublishing<MarkReleaseAsDraftRequirement>(
                    (context, releaseStatusRepository) =>
                        new MarkReleaseAsDraftAuthorizationHandler(context, releaseStatusRepository)
                );
            }

            [Fact]
            public void AllClaimsFail_ReleasePublished()
            {
                AssertAllClaimsFailWhenReleasePublished<MarkReleaseAsDraftRequirement>(
                    (context, releaseStatusRepository) =>
                        new MarkReleaseAsDraftAuthorizationHandler(context, releaseStatusRepository)
                );
            }

            [Fact]
            public void RoleSuccess_EditorOrApprover_ReleaseUnpublished()
            {
                GetEnumValues<ReleaseStatus>().ForEach(
                    status =>
                    {
                        var release = new Release
                        {
                            Id = Guid.NewGuid(),
                            Status = status
                        };

                        var releaseStatusRepository = new Mock<IReleaseStatusRepository>();

                        releaseStatusRepository.Setup(
                                s => s.GetAllByOverallStage(
                                    release.Id,
                                    ReleaseStatusOverallStage.Started,
                                    ReleaseStatusOverallStage.Complete
                                )
                            )
                            .ReturnsAsync(new List<PublisherReleaseStatus>());

                        // Assert that a user who has the "Contributor", "Lead" or "Approver"
                        // role on a Release can update its status if it is not Approved
                        if (status != ReleaseStatus.Approved)
                        {
                            AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<MarkReleaseAsDraftRequirement>(
                                context => new MarkReleaseAsDraftAuthorizationHandler(
                                    context,
                                    releaseStatusRepository.Object
                                ),
                                release,
                                ReleaseRole.Contributor,
                                ReleaseRole.Lead,
                                ReleaseRole.Approver
                            );
                        }
                        else
                        {
                            // Assert that a user who has the "Approver" role on a
                            // Release can update its status if it is Approved
                            AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<MarkReleaseAsDraftRequirement>(
                                context => new MarkReleaseAsDraftAuthorizationHandler(
                                    context,
                                    releaseStatusRepository.Object
                                ),
                                release,
                                ReleaseRole.Approver
                            );
                        }
                    }
                );
            }

            [Fact]
            public void AllRolesFail_ReleasePublishing()
            {
                AssertAllRolesFailWhenReleasePublishing<MarkReleaseAsDraftRequirement>(
                    (context, releaseStatusRepository) =>
                        new MarkReleaseAsDraftAuthorizationHandler(context, releaseStatusRepository)
                );
            }

            [Fact]
            public void AllRolesFail_ReleasePublished()
            {
                AssertAllRolesFailWhenReleasePublished<MarkReleaseAsDraftRequirement>(
                    (context, releaseStatusRepository) =>
                        new MarkReleaseAsDraftAuthorizationHandler(context, releaseStatusRepository)
                );
            }
        }

        public class MarkReleaseAsHigherLevelReviewAuthorizationHandlerTests
        {
            [Fact]
            public void ClaimSuccess_SubmitAllReleasesToHigherReview_ReleaseUnpublished()
            {
                AssertClaimSucceedsWhenReleaseUnpublished<MarkReleaseAsHigherLevelReviewRequirement>(
                    (context, releaseStatusRepository) =>
                        new MarkReleaseAsHigherLevelReviewAuthorizationHandler(context, releaseStatusRepository),
                    SubmitAllReleasesToHigherReview
                );
            }

            [Fact]
            public void AllClaimsFail_ReleasePublishing()
            {
                AssertAllClaimsFailWhenReleasePublishing<MarkReleaseAsHigherLevelReviewRequirement>(
                    (context, releaseStatusRepository) =>
                        new MarkReleaseAsHigherLevelReviewAuthorizationHandler(context, releaseStatusRepository)
                );
            }

            [Fact]
            public void AllClaimsFail_ReleasePublished()
            {
                AssertAllClaimsFailWhenReleasePublished<MarkReleaseAsHigherLevelReviewRequirement>(
                    (context, releaseStatusRepository) =>
                        new MarkReleaseAsHigherLevelReviewAuthorizationHandler(context, releaseStatusRepository)
                );
            }

            [Fact]
            public void RoleSuccess_EditorOrApprover_ReleaseUnpublished()
            {
                GetEnumValues<ReleaseStatus>().ForEach(
                    status =>
                    {
                        var release = new Release
                        {
                            Id = Guid.NewGuid(),
                            Status = status
                        };

                        var releaseStatusRepository = new Mock<IReleaseStatusRepository>();

                        releaseStatusRepository.Setup(
                                s => s.GetAllByOverallStage(
                                    release.Id,
                                    ReleaseStatusOverallStage.Started,
                                    ReleaseStatusOverallStage.Complete
                                )
                            )
                            .ReturnsAsync(new List<PublisherReleaseStatus>());

                        // Assert that a user who has the "Contributor", "Lead" or "Approver"
                        // role on a Release can update its status if it is not Approved
                        if (status != ReleaseStatus.Approved)
                        {
                            AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<
                                MarkReleaseAsHigherLevelReviewRequirement>(
                                context => new MarkReleaseAsHigherLevelReviewAuthorizationHandler(
                                    context,
                                    releaseStatusRepository.Object
                                ),
                                release,
                                ReleaseRole.Contributor,
                                ReleaseRole.Lead,
                                ReleaseRole.Approver
                            );
                        }
                        else
                        {
                            // Assert that a user who has the "Approver" role on a
                            // Release can update its status if it is Approved
                            AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<
                                MarkReleaseAsHigherLevelReviewRequirement>(
                                context => new MarkReleaseAsHigherLevelReviewAuthorizationHandler(
                                    context,
                                    releaseStatusRepository.Object
                                ),
                                release,
                                ReleaseRole.Approver
                            );
                        }
                    }
                );
            }

            [Fact]
            public void AllRolesFail_ReleasePublishing()
            {
                AssertAllRolesFailWhenReleasePublishing<MarkReleaseAsHigherLevelReviewRequirement>(
                    (context, releaseStatusRepository) =>
                        new MarkReleaseAsHigherLevelReviewAuthorizationHandler(context, releaseStatusRepository)
                );
            }

            [Fact]
            public void AllRolesFail_ReleasePublished()
            {
                AssertAllRolesFailWhenReleasePublished<MarkReleaseAsHigherLevelReviewRequirement>(
                    (context, releaseStatusRepository) =>
                        new MarkReleaseAsHigherLevelReviewAuthorizationHandler(context, releaseStatusRepository)
                );
            }
        }

        public class MarkReleaseAsApprovedAuthorizationHandlerTests
        {
            [Fact]
            public void ClaimSuccess_ApproveAllReleases_ReleaseUnpublished()
            {
                AssertClaimSucceedsWhenReleaseUnpublished<MarkReleaseAsApprovedRequirement>(
                    (context, releaseStatusRepository) =>
                        new MarkReleaseAsApprovedAuthorizationHandler(context, releaseStatusRepository),
                    ApproveAllReleases
                );
            }

            [Fact]
            public void AllClaimsFail_ReleasePublishing()
            {
                AssertAllClaimsFailWhenReleasePublishing<MarkReleaseAsApprovedRequirement>(
                    (context, releaseStatusRepository) =>
                        new MarkReleaseAsApprovedAuthorizationHandler(context, releaseStatusRepository)
                );
            }

            [Fact]
            public void AllClaimsFail_ReleasePublished()
            {
                AssertAllClaimsFailWhenReleasePublished<MarkReleaseAsApprovedRequirement>(
                    (context, releaseStatusRepository) =>
                        new MarkReleaseAsApprovedAuthorizationHandler(context, releaseStatusRepository)
                );
            }

            [Fact]
            public void RoleSuccess_Approver_ReleaseUnpublished()
            {
                GetEnumValues<ReleaseStatus>().ForEach(
                    status =>
                    {
                        var release = new Release
                        {
                            Id = Guid.NewGuid(),
                            Status = status
                        };

                        var releaseStatusRepository = new Mock<IReleaseStatusRepository>();

                        releaseStatusRepository.Setup(
                                s => s.GetAllByOverallStage(
                                    release.Id,
                                    ReleaseStatusOverallStage.Started,
                                    ReleaseStatusOverallStage.Complete
                                )
                            )
                            .ReturnsAsync(new List<PublisherReleaseStatus>());

                        // Assert that a user who has the "Approver" role on a
                        // Release can update its status if it is Approved
                        AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<MarkReleaseAsApprovedRequirement>(
                            context => new MarkReleaseAsApprovedAuthorizationHandler(
                                context,
                                releaseStatusRepository.Object
                            ),
                            release,
                            ReleaseRole.Approver
                        );
                    }
                );
            }

            [Fact]
            public void AllRolesFail_ReleasePublishing()
            {
                AssertAllRolesFailWhenReleasePublishing<MarkReleaseAsApprovedRequirement>(
                    (context, releaseStatusRepository) =>
                        new MarkReleaseAsHigherLevelReviewAuthorizationHandler(context, releaseStatusRepository)
                );
            }

            [Fact]
            public void AllRolesFail_ReleasePublished()
            {
                AssertAllRolesFailWhenReleasePublished<MarkReleaseAsApprovedRequirement>(
                    (context, releaseStatusRepository) =>
                        new MarkReleaseAsHigherLevelReviewAuthorizationHandler(context, releaseStatusRepository)
                );
            }
        }

        private static void AssertClaimSucceedsWhenReleaseUnpublished<TRequirement>(
            Func<ContentDbContext, IReleaseStatusRepository, IAuthorizationHandler> authorizationHandler,
            params SecurityClaimTypes[] claims)
            where TRequirement : IAuthorizationRequirement
        {
            GetEnumValues<ReleaseStatus>().ForEach(
                status =>
                {
                    var release = new Release
                    {
                        Id = Guid.NewGuid(),
                        Status = status
                    };

                    var releaseStatusRepository = new Mock<IReleaseStatusRepository>();

                    // Assert that users with the specified claims can update the
                    // Release status if it has not started publishing
                    AssertReleaseHandlerSucceedsWithCorrectClaims<TRequirement>(
                        context => authorizationHandler(context, releaseStatusRepository.Object),
                        release,
                        claims
                    );
                }
            );
        }

        private static void AssertAllClaimsFailWhenReleasePublishing<TRequirement>(
            Func<ContentDbContext, IReleaseStatusRepository, IAuthorizationHandler> authorizationHandler)
            where TRequirement : IAuthorizationRequirement
        {
            GetEnumValues<ReleaseStatus>().ForEach(
                status =>
                {
                    var release = new Release
                    {
                        Id = Guid.NewGuid(),
                        Status = status,
                    };

                    var releaseStatusRepository = new Mock<IReleaseStatusRepository>();

                    releaseStatusRepository.Setup(
                            s => s.GetAllByOverallStage(
                                release.Id,
                                ReleaseStatusOverallStage.Started,
                                ReleaseStatusOverallStage.Complete
                            )
                        )
                        .ReturnsAsync(
                            new List<PublisherReleaseStatus>
                            {
                                new PublisherReleaseStatus()
                            }
                        );

                    // Assert that no users can update a Release status once it has started publishing
                    AssertReleaseHandlerSucceedsWithCorrectClaims<TRequirement>(
                        context => authorizationHandler(context, releaseStatusRepository.Object),
                        release
                    );
                }
            );
        }

        private static void AssertAllClaimsFailWhenReleasePublished<TRequirement>(
            Func<ContentDbContext, IReleaseStatusRepository, IAuthorizationHandler> authorizationHandler)
            where TRequirement : IAuthorizationRequirement
        {
            GetEnumValues<ReleaseStatus>().ForEach(
                status =>
                {
                    var release = new Release
                    {
                        Id = Guid.NewGuid(),
                        Status = status,
                        Published = DateTime.Now
                    };

                    var releaseStatusRepository = new Mock<IReleaseStatusRepository>();

                    releaseStatusRepository.Setup(
                            s => s.GetAllByOverallStage(
                                release.Id,
                                ReleaseStatusOverallStage.Started,
                                ReleaseStatusOverallStage.Complete
                            )
                        )
                        .ReturnsAsync(new List<PublisherReleaseStatus>());

                    // Assert that no users can update a Release status once it has been published
                    AssertReleaseHandlerSucceedsWithCorrectClaims<TRequirement>(
                        context => authorizationHandler(context, releaseStatusRepository.Object),
                        release
                    );
                }
            );
        }

        private static void AssertAllRolesFailWhenReleasePublishing<TRequirement>(
            Func<ContentDbContext, IReleaseStatusRepository, IAuthorizationHandler> authorizationHandler)
            where TRequirement : IAuthorizationRequirement
        {
            GetEnumValues<ReleaseStatus>().ForEach(
                status =>
                {
                    var release = new Release
                    {
                        Id = Guid.NewGuid(),
                        Status = status
                    };

                    var releaseStatusRepository = new Mock<IReleaseStatusRepository>();

                    releaseStatusRepository.Setup(
                            s => s.GetAllByOverallStage(
                                release.Id,
                                ReleaseStatusOverallStage.Started,
                                ReleaseStatusOverallStage.Complete
                            )
                        )
                        .ReturnsAsync(
                            new List<PublisherReleaseStatus>
                            {
                                new PublisherReleaseStatus()
                            }
                        );

                    // Assert that no user can update a Release status once it has started publishing
                    AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<TRequirement>(
                        context => authorizationHandler(context, releaseStatusRepository.Object),
                        release
                    );
                }
            );
        }

        private static void AssertAllRolesFailWhenReleasePublished<TRequirement>(
            Func<ContentDbContext, IReleaseStatusRepository, IAuthorizationHandler> authorizationHandler)
            where TRequirement : IAuthorizationRequirement
        {
            GetEnumValues<ReleaseStatus>().ForEach(
                status =>
                {
                    var release = new Release
                    {
                        Id = Guid.NewGuid(),
                        Status = status,
                        Published = DateTime.Now,
                    };

                    var releaseStatusRepository = new Mock<IReleaseStatusRepository>();

                    releaseStatusRepository.Setup(
                            s => s.GetAllByOverallStage(
                                release.Id,
                                ReleaseStatusOverallStage.Started,
                                ReleaseStatusOverallStage.Complete
                            )
                        )
                        .ReturnsAsync(new List<PublisherReleaseStatus>());

                    // Assert that no user can update a Release status once it has been published
                    AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<TRequirement>(
                        context => authorizationHandler(context, releaseStatusRepository.Object),
                        release
                    );
                }
            );
        }
    }
}