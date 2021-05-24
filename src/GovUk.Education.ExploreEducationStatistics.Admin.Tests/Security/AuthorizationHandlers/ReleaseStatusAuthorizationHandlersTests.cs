using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
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
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ReleaseStatusAuthorizationHandlersTests
    {
        public class MarkReleaseAsDraftAuthorizationHandlerTests
        {
            [Fact]
            public async Task ClaimSuccess_MarkAllReleasesAsDraft_ReleaseUnpublished()
            {
                await AssertClaimSucceedsWhenReleaseUnpublished<MarkReleaseAsDraftRequirement>(
                    (releaseStatusRepository, userReleaseRoleRepository) =>
                        new MarkReleaseAsDraftAuthorizationHandler(releaseStatusRepository, userReleaseRoleRepository),
                    MarkAllReleasesAsDraft
                );
            }
        
            [Fact]
            public async Task AllClaimsFail_ReleasePublishing()
            {
                await AssertAllClaimsFailWhenReleasePublishing<MarkReleaseAsDraftRequirement>(
                    (releaseStatusRepository, userReleaseRoleRepository) =>
                        new MarkReleaseAsDraftAuthorizationHandler(releaseStatusRepository, userReleaseRoleRepository)
                );
            }
        
            [Fact]
            public async Task AllClaimsFail_ReleasePublished()
            {
                await AssertAllClaimsFailWhenReleasePublished<MarkReleaseAsDraftRequirement>(
                    (releaseStatusRepository, userReleaseRoleRepository) =>
                        new MarkReleaseAsDraftAuthorizationHandler(releaseStatusRepository, userReleaseRoleRepository)
                );
            }
        
            [Fact]
            public async Task RoleSuccess_EditorOrApprover_ReleaseUnpublished()
            {
                await GetEnumValues<ReleaseStatus>().ForEachAsync(
                    async status =>
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
                            await AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<MarkReleaseAsDraftRequirement>(
                                context => new MarkReleaseAsDraftAuthorizationHandler(
                                    releaseStatusRepository.Object,
                                    new UserReleaseRoleRepository(context)
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
                            await AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<MarkReleaseAsDraftRequirement>(
                                context => new MarkReleaseAsDraftAuthorizationHandler(
                                    releaseStatusRepository.Object,
                                    new UserReleaseRoleRepository(context)
                                ),
                                release,
                                ReleaseRole.Approver
                            );
                        }
                    }
                );
            }
        
            [Fact]
            public async Task AllRolesFail_ReleasePublishing()
            {
                await AssertAllRolesFailWhenReleasePublishing<MarkReleaseAsDraftRequirement>(
                    (releaseStatusRepository, userReleaseRoleRepository) =>
                        new MarkReleaseAsDraftAuthorizationHandler(releaseStatusRepository, userReleaseRoleRepository)
                );
            }
        
            [Fact]
            public async Task AllRolesFail_ReleasePublished()
            {
                await AssertAllRolesFailWhenReleasePublished<MarkReleaseAsDraftRequirement>(
                    (releaseStatusRepository, userReleaseRoleRepository) =>
                        new MarkReleaseAsDraftAuthorizationHandler(releaseStatusRepository, userReleaseRoleRepository)
                );
            }
        }
        
        public class MarkReleaseAsHigherLevelReviewAuthorizationHandlerTests
        {
            [Fact]
            public async Task ClaimSuccess_SubmitAllReleasesToHigherReview_ReleaseUnpublished()
            {
                await AssertClaimSucceedsWhenReleaseUnpublished<MarkReleaseAsHigherLevelReviewRequirement>(
                    (releaseStatusRepository, userReleaseRoleRepository) =>
                        new MarkReleaseAsHigherLevelReviewAuthorizationHandler(releaseStatusRepository, userReleaseRoleRepository),
                    SubmitAllReleasesToHigherReview
                );
            }
        
            [Fact]
            public async Task AllClaimsFail_ReleasePublishing()
            {
                await AssertAllClaimsFailWhenReleasePublishing<MarkReleaseAsHigherLevelReviewRequirement>(
                    (releaseStatusRepository, userReleaseRoleRepository) =>
                        new MarkReleaseAsHigherLevelReviewAuthorizationHandler(releaseStatusRepository, userReleaseRoleRepository)
                );
            }
        
            [Fact]
            public async Task AllClaimsFail_ReleasePublished()
            {
                await AssertAllClaimsFailWhenReleasePublished<MarkReleaseAsHigherLevelReviewRequirement>(
                    (releaseStatusRepository, userReleaseRoleRepository) =>
                        new MarkReleaseAsHigherLevelReviewAuthorizationHandler(releaseStatusRepository, userReleaseRoleRepository)
                );
            }
        
            [Fact]
            public async Task RoleSuccess_EditorOrApprover_ReleaseUnpublished()
            {
                await GetEnumValues<ReleaseStatus>().ForEachAsync(
                    async status =>
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
                            await AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<
                                MarkReleaseAsHigherLevelReviewRequirement>(
                                context => new MarkReleaseAsHigherLevelReviewAuthorizationHandler(
                                    releaseStatusRepository.Object,
                                    new UserReleaseRoleRepository(context)
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
                            await AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<
                                MarkReleaseAsHigherLevelReviewRequirement>(
                                context => new MarkReleaseAsHigherLevelReviewAuthorizationHandler(
                                    releaseStatusRepository.Object,
                                    new UserReleaseRoleRepository(context)
                                ),
                                release,
                                ReleaseRole.Approver
                            );
                        }
                    }
                );
            }
        
            [Fact]
            public async Task AllRolesFail_ReleasePublishing()
            {
                await AssertAllRolesFailWhenReleasePublishing<MarkReleaseAsHigherLevelReviewRequirement>(
                    (releaseStatusRepository, userReleaseRoleRepository) =>
                        new MarkReleaseAsHigherLevelReviewAuthorizationHandler(releaseStatusRepository, userReleaseRoleRepository)
                );
            }
        
            [Fact]
            public async Task AllRolesFail_ReleasePublished()
            {
                await AssertAllRolesFailWhenReleasePublished<MarkReleaseAsHigherLevelReviewRequirement>(
                    (releaseStatusRepository, userReleaseRoleRepository) =>
                        new MarkReleaseAsHigherLevelReviewAuthorizationHandler(releaseStatusRepository, userReleaseRoleRepository)
                );
            }
        }
        
        public class MarkReleaseAsApprovedAuthorizationHandlerTests
        {
            [Fact]
            public async Task ClaimSuccess_ApproveAllReleases_ReleaseUnpublished()
            {
                await AssertClaimSucceedsWhenReleaseUnpublished<MarkReleaseAsApprovedRequirement>(
                    (releaseStatusRepository, userReleaseRoleRepository) =>
                        new MarkReleaseAsApprovedAuthorizationHandler(releaseStatusRepository, userReleaseRoleRepository),
                    ApproveAllReleases
                );
            }
        
            [Fact]
            public async Task AllClaimsFail_ReleasePublishing()
            {
                await AssertAllClaimsFailWhenReleasePublishing<MarkReleaseAsApprovedRequirement>(
                    (releaseStatusRepository, userReleaseRoleRepository) =>
                        new MarkReleaseAsApprovedAuthorizationHandler(releaseStatusRepository, userReleaseRoleRepository)
                );
            }
        
            [Fact]
            public async Task AllClaimsFail_ReleasePublished()
            {
                await AssertAllClaimsFailWhenReleasePublished<MarkReleaseAsApprovedRequirement>(
                    (releaseStatusRepository, userReleaseRoleRepository) =>
                        new MarkReleaseAsApprovedAuthorizationHandler(releaseStatusRepository, userReleaseRoleRepository)
                );
            }
        
            [Fact]
            public async Task RoleSuccess_Approver_ReleaseUnpublished()
            {
                await GetEnumValues<ReleaseStatus>().ForEachAsync(
                    async status =>
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
                        await AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<MarkReleaseAsApprovedRequirement>(
                            context => new MarkReleaseAsApprovedAuthorizationHandler(
                                releaseStatusRepository.Object,
                                new UserReleaseRoleRepository(context)
                            ),
                            release,
                            ReleaseRole.Approver
                        );
                    }
                );
            }
        
            [Fact]
            public async Task AllRolesFail_ReleasePublishing()
            {
                await AssertAllRolesFailWhenReleasePublishing<MarkReleaseAsApprovedRequirement>(
                    (releaseStatusRepository, userReleaseRoleRepository) =>
                        new MarkReleaseAsHigherLevelReviewAuthorizationHandler(releaseStatusRepository, userReleaseRoleRepository)
                );
            }
        
            [Fact]
            public async Task AllRolesFail_ReleasePublished()
            {
                await AssertAllRolesFailWhenReleasePublished<MarkReleaseAsApprovedRequirement>(
                    (releaseStatusRepository, userReleaseRoleRepository) =>
                        new MarkReleaseAsHigherLevelReviewAuthorizationHandler(releaseStatusRepository, userReleaseRoleRepository)
                );
            }
        }
        
        private static async Task AssertClaimSucceedsWhenReleaseUnpublished<TRequirement>(
            Func<IReleaseStatusRepository, IUserReleaseRoleRepository, IAuthorizationHandler> authorizationHandler,
            params SecurityClaimTypes[] claims)
            where TRequirement : IAuthorizationRequirement
        {
            await GetEnumValues<ReleaseStatus>().ForEachAsync(
                async status =>
                {
                    var release = new Release
                    {
                        Id = Guid.NewGuid(),
                        Status = status
                    };
        
                    var releaseStatusRepository = new Mock<IReleaseStatusRepository>();
        
                    // Assert that users with the specified claims can update the
                    // Release status if it has not started publishing
                    await AssertReleaseHandlerSucceedsWithCorrectClaims<TRequirement>(context =>
                            authorizationHandler(releaseStatusRepository.Object,
                            new UserReleaseRoleRepository(context)),
                        release,
                        claims
                    );
                }
            );
        }

        private static async Task AssertAllClaimsFailWhenReleasePublishing<TRequirement>(
            Func<IReleaseStatusRepository, IUserReleaseRoleRepository, IAuthorizationHandler> authorizationHandler)
            where TRequirement : IAuthorizationRequirement
        {
            await GetEnumValues<ReleaseStatus>().ForEachAsync(
                async status =>
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
                    await AssertReleaseHandlerSucceedsWithCorrectClaims<TRequirement>(context =>
                            authorizationHandler(releaseStatusRepository.Object,
                                new UserReleaseRoleRepository(context)),
                        release
                    );
                }
            );
        }

        private static async Task AssertAllClaimsFailWhenReleasePublished<TRequirement>(
            Func<IReleaseStatusRepository, IUserReleaseRoleRepository, IAuthorizationHandler> authorizationHandler)
            where TRequirement : IAuthorizationRequirement
        {
            await GetEnumValues<ReleaseStatus>().ForEachAsync(
                async status =>
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
                    await AssertReleaseHandlerSucceedsWithCorrectClaims<TRequirement>(context =>
                            authorizationHandler(releaseStatusRepository.Object,
                                new UserReleaseRoleRepository(context)),
                        release
                    );
                }
            );
        }

        private static async Task AssertAllRolesFailWhenReleasePublishing<TRequirement>(
            Func<IReleaseStatusRepository, IUserReleaseRoleRepository, IAuthorizationHandler> authorizationHandler)
            where TRequirement : IAuthorizationRequirement
        {
            await GetEnumValues<ReleaseStatus>().ForEachAsync(
                async status =>
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
                    await AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<TRequirement>(context =>
                            authorizationHandler(releaseStatusRepository.Object,
                                new UserReleaseRoleRepository(context)),
                        release
                    );
                }
            );
        }

        private static async Task AssertAllRolesFailWhenReleasePublished<TRequirement>(
            Func<IReleaseStatusRepository, IUserReleaseRoleRepository, IAuthorizationHandler> authorizationHandler)
            where TRequirement : IAuthorizationRequirement
        {
            await GetEnumValues<ReleaseStatus>().ForEachAsync(
                async status =>
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
                    await AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<TRequirement>(context =>
                            authorizationHandler(releaseStatusRepository.Object,
                                new UserReleaseRoleRepository(context)),
                        release
                    );
                }
            );
        }
    }
}
