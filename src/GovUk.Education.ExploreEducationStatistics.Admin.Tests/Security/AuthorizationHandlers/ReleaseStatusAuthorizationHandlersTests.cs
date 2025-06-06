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
using Microsoft.AspNetCore.Authorization;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.
    AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.
    ReleaseVersionAuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.EnumUtil;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;
using static Moq.MockBehavior;
using ReleaseVersionRepository =
    GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.ReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers;

// ReSharper disable once ClassNeverInstantiated.Global
public class ReleaseStatusAuthorizationHandlersTests
{
    public class MarkReleaseAsDraftAuthorizationHandlerTests
    {
        [Fact]
        public async Task ClaimSuccess_MarkAllReleasesAsDraft_ReleaseUnpublished()
        {
            await AssertClaimSucceedsWhenReleaseUnpublished<MarkReleaseAsDraftRequirement>(
                BuildMarkReleaseAsDraftHandler,
                MarkAllReleasesAsDraft
            );
        }

        [Fact]
        public async Task AllClaimsFail_ReleasePublishing()
        {
            await AssertAllClaimsFailWhenReleasePublishing<MarkReleaseAsDraftRequirement>(
                BuildMarkReleaseAsDraftHandler
            );
        }

        [Fact]
        public async Task AllClaimsFail_ReleasePublished()
        {
            await AssertAllClaimsFailWhenReleasePublished<MarkReleaseAsDraftRequirement>(
                BuildMarkReleaseAsDraftHandler
            );
        }

        [Fact]
        public async Task ReleaseRoleSuccess_EditorOrApprover_ReleaseUnpublished()
        {
            await GetEnums<ReleaseApprovalStatus>()
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(
                    async status =>
                    {
                        var releaseVersion = new ReleaseVersion
                        {
                            Id = Guid.NewGuid(),
                            Publication = new Publication { Id = Guid.NewGuid() },
                            ApprovalStatus = status
                        };

                        var releaseStatusRepository = new Mock<IReleasePublishingStatusRepository>(Strict);

                        releaseStatusRepository.Setup(
                                s => s.GetAllByOverallStage(
                                    releaseVersion.Id,
                                    ReleasePublishingStatusOverallStage.Started,
                                    ReleasePublishingStatusOverallStage.Complete
                                )
                            )
                            .ReturnsAsync(new List<ReleasePublishingStatus>());

                        // Assert that a user who has the "Contributor" or "Approver"
                        // role on a Release can update its status if it is not Approved
                        if (status != ReleaseApprovalStatus.Approved)
                        {
                            await AssertReleaseVersionHandlerSucceedsWithCorrectReleaseRoles<
                                MarkReleaseAsDraftRequirement>(
                                context =>
                                {
                                    context.ReleaseVersions.Add(releaseVersion);
                                    context.SaveChanges();

                                    return CreateHandler(releaseStatusRepository, context);
                                },
                                releaseVersion,
                                ReleaseRole.Contributor,
                                ReleaseRole.Approver
                            );
                        }
                        else
                        {
                            // Assert that a user who has the "Approver" role on a
                            // Release can update its status if it is Approved
                            await AssertReleaseVersionHandlerSucceedsWithCorrectReleaseRoles<
                                MarkReleaseAsDraftRequirement>(
                                context =>
                                {
                                    context.ReleaseVersions.Add(releaseVersion);
                                    context.SaveChanges();

                                    return CreateHandler(releaseStatusRepository, context);
                                },
                                releaseVersion,
                                ReleaseRole.Approver
                            );
                        }
                    }
                );
        }

        [Fact]
        public async Task PublicationRoleSuccess_Owner_ReleaseUnpublished()
        {
            await GetEnums<ReleaseApprovalStatus>()
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(
                    async status =>
                    {
                        var releaseVersion = new ReleaseVersion
                        {
                            Id = Guid.NewGuid(),
                            Publication = new Publication { Id = Guid.NewGuid() },
                            ApprovalStatus = status
                        };

                        var releaseStatusRepository = new Mock<IReleasePublishingStatusRepository>(Strict);

                        releaseStatusRepository.Setup(
                                s => s.GetAllByOverallStage(
                                    releaseVersion.Id,
                                    ReleasePublishingStatusOverallStage.Started,
                                    ReleasePublishingStatusOverallStage.Complete
                                )
                            )
                            .ReturnsAsync(new List<ReleasePublishingStatus>());

                        // Assert that a User who has the Publication Owner or Approver role
                        // on a Release can mark its status as Draft if it is not yet Approved.
                        if (status != ReleaseApprovalStatus.Approved)
                        {
                            await AssertReleaseVersionHandlerSucceedsWithCorrectPublicationRoles<
                                MarkReleaseAsDraftRequirement>(
                                context =>
                                {
                                    context.ReleaseVersions.Add(releaseVersion);
                                    context.SaveChanges();

                                    return CreateHandler(releaseStatusRepository, context);
                                },
                                releaseVersion,
                                Owner,
                                Approver
                            );
                        }
                        else
                        {
                            // Assert that a User who has the Publication Approver role on a
                            // Release can mark its status as draft if it is currently Approved
                            // but not yet published, just as a Release Approver can.
                            await AssertReleaseVersionHandlerSucceedsWithCorrectPublicationRoles<
                                MarkReleaseAsDraftRequirement>(
                                context =>
                                {
                                    context.ReleaseVersions.Add(releaseVersion);
                                    context.SaveChanges();

                                    return CreateHandler(releaseStatusRepository, context);
                                },
                                releaseVersion,
                                Approver
                            );
                        }
                    }
                );
        }

        [Fact]
        public async Task AllRolesFail_ReleasePublishing()
        {
            await AssertAllRolesFailWhenReleasePublishing<MarkReleaseAsDraftRequirement>(
                BuildMarkReleaseAsDraftHandler
            );
        }

        [Fact]
        public async Task AllRolesFail_ReleasePublished()
        {
            await AssertAllRolesFailWhenReleasePublished<MarkReleaseAsDraftRequirement>(
                BuildMarkReleaseAsDraftHandler
            );
        }

        private static MarkReleaseAsDraftAuthorizationHandler CreateHandler(
            Mock<IReleasePublishingStatusRepository> releaseStatusRepository,
            ContentDbContext context)
        {
            return BuildMarkReleaseAsDraftHandler(
                releaseStatusRepository.Object,
                new UserPublicationRoleRepository(context),
                new UserReleaseRoleRepository(context));
        }
    }

    public class MarkReleaseAsHigherLevelReviewAuthorizationHandlerTests
    {
        [Fact]
        public async Task ClaimSuccess_SubmitAllReleasesToHigherReview_ReleaseUnpublished()
        {
            await AssertClaimSucceedsWhenReleaseUnpublished<MarkReleaseAsHigherLevelReviewRequirement>(
                BuildMarkReleaseAsHigherLevelReviewHandler,
                SubmitAllReleasesToHigherReview
            );
        }

        [Fact]
        public async Task AllClaimsFail_ReleasePublishing()
        {
            await AssertAllClaimsFailWhenReleasePublishing<MarkReleaseAsHigherLevelReviewRequirement>(
                BuildMarkReleaseAsHigherLevelReviewHandler
            );
        }

        [Fact]
        public async Task AllClaimsFail_ReleasePublished()
        {
            await AssertAllClaimsFailWhenReleasePublished<MarkReleaseAsHigherLevelReviewRequirement>(
                BuildMarkReleaseAsHigherLevelReviewHandler
            );
        }

        [Fact]
        public async Task ReleaseRoleSuccess_EditorOrApprover_ReleaseUnpublished()
        {
            await GetEnums<ReleaseApprovalStatus>()
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(
                    async status =>
                    {
                        var releaseVersion = new ReleaseVersion
                        {
                            Id = Guid.NewGuid(),
                            Publication = new Publication { Id = Guid.NewGuid() },
                            ApprovalStatus = status
                        };

                        var releaseStatusRepository = new Mock<IReleasePublishingStatusRepository>(Strict);

                        releaseStatusRepository.Setup(
                                s => s.GetAllByOverallStage(
                                    releaseVersion.Id,
                                    ReleasePublishingStatusOverallStage.Started,
                                    ReleasePublishingStatusOverallStage.Complete
                                )
                            )
                            .ReturnsAsync(new List<ReleasePublishingStatus>());

                        // Assert that a user who has the "Contributor" or "Approver"
                        // role on a Release can update its status if it is not Approved
                        if (status != ReleaseApprovalStatus.Approved)
                        {
                            await AssertReleaseVersionHandlerSucceedsWithCorrectReleaseRoles<
                                MarkReleaseAsHigherLevelReviewRequirement>(
                                context =>
                                {
                                    context.ReleaseVersions.Add(releaseVersion);
                                    context.SaveChanges();

                                    return CreateHandler(releaseStatusRepository, context);
                                },
                                releaseVersion,
                                ReleaseRole.Contributor,
                                ReleaseRole.Approver
                            );
                        }
                        else
                        {
                            // Assert that a user who has the "Approver" role on a
                            // Release can update its status if it is Approved
                            await AssertReleaseVersionHandlerSucceedsWithCorrectReleaseRoles<
                                MarkReleaseAsHigherLevelReviewRequirement>(
                                context =>
                                {
                                    context.ReleaseVersions.Add(releaseVersion);
                                    context.SaveChanges();

                                    return CreateHandler(releaseStatusRepository, context);
                                },
                                releaseVersion,
                                ReleaseRole.Approver
                            );
                        }
                    }
                );
        }

        [Fact]
        public async Task PublicationRoleSuccess_Owner_ReleaseUnpublished()
        {
            await GetEnums<ReleaseApprovalStatus>()
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(
                    async status =>
                    {
                        var releaseVersion = new ReleaseVersion
                        {
                            Id = Guid.NewGuid(),
                            Publication = new Publication { Id = Guid.NewGuid() },
                            ApprovalStatus = status
                        };

                        var releaseStatusRepository = new Mock<IReleasePublishingStatusRepository>(Strict);

                        releaseStatusRepository.Setup(
                                s => s.GetAllByOverallStage(
                                    releaseVersion.Id,
                                    ReleasePublishingStatusOverallStage.Started,
                                    ReleasePublishingStatusOverallStage.Complete
                                )
                            )
                            .ReturnsAsync(new List<ReleasePublishingStatus>());

                        // Assert that a User who has the Publication Owner or Approver role on a
                        // Release can mark it for higher review if it is not Approved
                        if (status != ReleaseApprovalStatus.Approved)
                        {
                            await AssertReleaseVersionHandlerSucceedsWithCorrectPublicationRoles<
                                MarkReleaseAsHigherLevelReviewRequirement>(
                                context =>
                                {
                                    context.ReleaseVersions.Add(releaseVersion);
                                    context.SaveChanges();

                                    return CreateHandler(releaseStatusRepository, context);
                                },
                                releaseVersion,
                                Owner,
                                Approver
                            );
                        }
                        else
                        {
                            // Assert that a User who has the Publication Approver role on a
                            // Release can mark it for higher review even if it is not Approved
                            await AssertReleaseVersionHandlerSucceedsWithCorrectPublicationRoles<
                                MarkReleaseAsHigherLevelReviewRequirement>(
                                context =>
                                {
                                    context.ReleaseVersions.Add(releaseVersion);
                                    context.SaveChanges();

                                    return CreateHandler(releaseStatusRepository, context);
                                },
                                releaseVersion,
                                Approver
                            );
                        }
                    }
                );
        }

        [Fact]
        public async Task AllRolesFail_ReleasePublishing()
        {
            await AssertAllRolesFailWhenReleasePublishing<MarkReleaseAsHigherLevelReviewRequirement>(
                BuildMarkReleaseAsHigherLevelReviewHandler
            );
        }

        [Fact]
        public async Task AllRolesFail_ReleasePublished()
        {
            await AssertAllRolesFailWhenReleasePublished<MarkReleaseAsHigherLevelReviewRequirement>(
                BuildMarkReleaseAsHigherLevelReviewHandler
            );
        }

        private static MarkReleaseAsHigherLevelReviewAuthorizationHandler CreateHandler(
            Mock<IReleasePublishingStatusRepository> releaseStatusRepository,
            ContentDbContext context)
        {
            return BuildMarkReleaseAsHigherLevelReviewHandler(
                releaseStatusRepository.Object,
                new UserPublicationRoleRepository(context),
                new UserReleaseRoleRepository(context));
        }
    }

    public class MarkReleaseAsApprovedAuthorizationHandlerTests
    {
        [Fact]
        public async Task ClaimSuccess_ApproveAllReleases_ReleaseUnpublished()
        {
            await AssertClaimSucceedsWhenReleaseUnpublished<MarkReleaseAsApprovedRequirement>(
                BuildMarkReleaseAsApprovedHandler,
                ApproveAllReleases
            );
        }

        [Fact]
        public async Task AllClaimsFail_ReleasePublishing()
        {
            await AssertAllClaimsFailWhenReleasePublishing<MarkReleaseAsApprovedRequirement>(
                BuildMarkReleaseAsApprovedHandler
            );
        }

        [Fact]
        public async Task AllClaimsFail_ReleasePublished()
        {
            await AssertAllClaimsFailWhenReleasePublished<MarkReleaseAsApprovedRequirement>(
                BuildMarkReleaseAsApprovedHandler
            );
        }

        [Fact]
        public async Task ReleaseRoleSuccess_Approver_ReleaseUnpublished()
        {
            await GetEnums<ReleaseApprovalStatus>()
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(
                    async status =>
                    {
                        var releaseVersion = new ReleaseVersion
                        {
                            Id = Guid.NewGuid(),
                            Publication = new Publication { Id = Guid.NewGuid() },
                            ApprovalStatus = status
                        };

                        var releaseStatusRepository = new Mock<IReleasePublishingStatusRepository>(Strict);

                        releaseStatusRepository.Setup(
                                s => s.GetAllByOverallStage(
                                    releaseVersion.Id,
                                    ReleasePublishingStatusOverallStage.Started,
                                    ReleasePublishingStatusOverallStage.Complete
                                )
                            )
                            .ReturnsAsync(new List<ReleasePublishingStatus>());

                        // Assert that a user who has the "Approver" role on a
                        // Release can update its status if it is Approved
                        await AssertReleaseVersionHandlerSucceedsWithCorrectReleaseRoles<MarkReleaseAsApprovedRequirement>(
                            context =>
                            {
                                context.ReleaseVersions.Add(releaseVersion);
                                context.SaveChanges();

                                return CreateHandler(releaseStatusRepository, context);
                            },
                            releaseVersion,
                            ReleaseRole.Approver
                        );
                    }
                );
        }

        [Fact]
        public async Task PublicationRoleSuccess_Approver_ReleaseUnpublished()
        {
            await GetEnums<ReleaseApprovalStatus>()
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(
                    async status =>
                    {
                        var releaseVersion = new ReleaseVersion
                        {
                            Id = Guid.NewGuid(),
                            Publication = new Publication { Id = Guid.NewGuid() },
                            ApprovalStatus = status
                        };

                        var releaseStatusRepository = new Mock<IReleasePublishingStatusRepository>(Strict);

                        releaseStatusRepository.Setup(
                                s => s.GetAllByOverallStage(
                                    releaseVersion.Id,
                                    ReleasePublishingStatusOverallStage.Started,
                                    ReleasePublishingStatusOverallStage.Complete
                                )
                            )
                            .ReturnsAsync(new List<ReleasePublishingStatus>());

                        // Assert that a user who has the "Approver" role on the
                        // Publication for the Release can update its status
                        await AssertReleaseVersionHandlerSucceedsWithCorrectPublicationRoles<
                            MarkReleaseAsApprovedRequirement>(
                            context =>
                            {
                                context.ReleaseVersions.Add(releaseVersion);
                                context.SaveChanges();

                                return CreateHandler(releaseStatusRepository, context);
                            },
                            releaseVersion,
                            Approver
                        );
                    }
                );
        }

        [Fact]
        public async Task AllRolesFail_ReleasePublishing()
        {
            await AssertAllRolesFailWhenReleasePublishing<MarkReleaseAsApprovedRequirement>(
                BuildMarkReleaseAsHigherLevelReviewHandler
            );
        }

        [Fact]
        public async Task AllRolesFail_ReleasePublished()
        {
            await AssertAllRolesFailWhenReleasePublished<MarkReleaseAsApprovedRequirement>(
                BuildMarkReleaseAsHigherLevelReviewHandler
            );
        }

        private static MarkReleaseAsApprovedAuthorizationHandler CreateHandler(
            Mock<IReleasePublishingStatusRepository> releaseStatusRepository,
            ContentDbContext context)
        {
            return BuildMarkReleaseAsApprovedHandler(
                releaseStatusRepository.Object,
                new UserPublicationRoleRepository(context),
                new UserReleaseRoleRepository(context)
            );
        }
    }

    private static async Task AssertClaimSucceedsWhenReleaseUnpublished<TRequirement>(
        Func<IReleasePublishingStatusRepository, IUserPublicationRoleRepository, IUserReleaseRoleRepository,
            IAuthorizationHandler> authorizationHandler,
        params SecurityClaimTypes[] claims)
        where TRequirement : IAuthorizationRequirement
    {
        await GetEnums<ReleaseApprovalStatus>()
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(
                async status =>
                {
                    var releaseVersion = new ReleaseVersion
                    {
                        Id = Guid.NewGuid(),
                        Publication = new Publication { Id = Guid.NewGuid() },
                        ApprovalStatus = status
                    };

                    var releaseStatusRepository = new Mock<IReleasePublishingStatusRepository>(Strict);

                    releaseStatusRepository.Setup(
                            s => s.GetAllByOverallStage(
                                releaseVersion.Id,
                                ReleasePublishingStatusOverallStage.Started,
                                ReleasePublishingStatusOverallStage.Complete
                            )
                        )
                        .ReturnsAsync(new List<ReleasePublishingStatus>());

                    // Assert that users with the specified claims can update the
                    // Release status if it has not started publishing
                    await AssertHandlerSucceedsWithCorrectClaims<ReleaseVersion, TRequirement>(context =>
                        {
                            context.ReleaseVersions.Add(releaseVersion);
                            context.SaveChanges();

                            return authorizationHandler(releaseStatusRepository.Object,
                                new UserPublicationRoleRepository(context),
                                new UserReleaseRoleRepository(context));
                        },
                        releaseVersion,
                        claims
                    );
                }
            );
    }

    private static async Task AssertAllClaimsFailWhenReleasePublishing<TRequirement>(
        Func<IReleasePublishingStatusRepository, IUserPublicationRoleRepository, IUserReleaseRoleRepository,
            IAuthorizationHandler> authorizationHandler)
        where TRequirement : IAuthorizationRequirement
    {
        await GetEnums<ReleaseApprovalStatus>()
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(
                async status =>
                {
                    var releaseVersion = new ReleaseVersion
                    {
                        Id = Guid.NewGuid(),
                        Publication = new Publication { Id = Guid.NewGuid() },
                        ApprovalStatus = status
                    };

                    var releaseStatusRepository = new Mock<IReleasePublishingStatusRepository>(Strict);

                    releaseStatusRepository.Setup(
                            s => s.GetAllByOverallStage(
                                releaseVersion.Id,
                                ReleasePublishingStatusOverallStage.Started,
                                ReleasePublishingStatusOverallStage.Complete
                            )
                        )
                        .ReturnsAsync(
                            new List<ReleasePublishingStatus> { new() }
                        );

                    // Assert that no users can update a Release status once it has started publishing
                    await AssertHandlerSucceedsWithCorrectClaims<ReleaseVersion, TRequirement>(context =>
                        {
                            context.ReleaseVersions.Add(releaseVersion);
                            context.SaveChanges();

                            return authorizationHandler(releaseStatusRepository.Object,
                                new UserPublicationRoleRepository(context),
                                new UserReleaseRoleRepository(context));
                        },
                        releaseVersion
                    );
                }
            );
    }

    private static async Task AssertAllClaimsFailWhenReleasePublished<TRequirement>(
        Func<IReleasePublishingStatusRepository, IUserPublicationRoleRepository, IUserReleaseRoleRepository,
            IAuthorizationHandler> authorizationHandler)
        where TRequirement : IAuthorizationRequirement
    {
        await GetEnums<ReleaseApprovalStatus>()
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(
                async status =>
                {
                    var releaseVersion = new ReleaseVersion
                    {
                        Id = Guid.NewGuid(),
                        Publication = new Publication { Id = Guid.NewGuid() },
                        ApprovalStatus = status,
                        Published = DateTime.Now
                    };

                    var releaseStatusRepository = new Mock<IReleasePublishingStatusRepository>(Strict);

                    releaseStatusRepository.Setup(
                            s => s.GetAllByOverallStage(
                                releaseVersion.Id,
                                ReleasePublishingStatusOverallStage.Started,
                                ReleasePublishingStatusOverallStage.Complete
                            )
                        )
                        .ReturnsAsync(new List<ReleasePublishingStatus>());

                    // Assert that no users can update a Release status once it has been published
                    await AssertHandlerSucceedsWithCorrectClaims<ReleaseVersion, TRequirement>(context =>
                        {
                            context.ReleaseVersions.Add(releaseVersion);
                            context.SaveChanges();

                            return authorizationHandler(releaseStatusRepository.Object,
                                new UserPublicationRoleRepository(context),
                                new UserReleaseRoleRepository(context));
                        },
                        releaseVersion
                    );
                }
            );
    }

    private static async Task AssertAllRolesFailWhenReleasePublishing<TRequirement>(
        Func<IReleasePublishingStatusRepository, IUserPublicationRoleRepository, IUserReleaseRoleRepository,
            IAuthorizationHandler> authorizationHandler)
        where TRequirement : IAuthorizationRequirement
    {
        await GetEnums<ReleaseApprovalStatus>()
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(
                async status =>
                {
                    var releaseVersion = new ReleaseVersion
                    {
                        Id = Guid.NewGuid(),
                        Publication = new Publication { Id = Guid.NewGuid() },
                        ApprovalStatus = status
                    };

                    var releaseStatusRepository = new Mock<IReleasePublishingStatusRepository>(Strict);

                    releaseStatusRepository.Setup(
                            s => s.GetAllByOverallStage(
                                releaseVersion.Id,
                                ReleasePublishingStatusOverallStage.Started,
                                ReleasePublishingStatusOverallStage.Complete
                            )
                        )
                        .ReturnsAsync(
                            new List<ReleasePublishingStatus> { new() }
                        );

                    // Assert that no user release roles allow updating a Release status once it has started publishing
                    await AssertReleaseVersionHandlerSucceedsWithCorrectReleaseRoles<TRequirement>(context =>
                        {
                            context.ReleaseVersions.Add(releaseVersion);
                            context.SaveChanges();

                            return authorizationHandler(releaseStatusRepository.Object,
                                new UserPublicationRoleRepository(context),
                                new UserReleaseRoleRepository(context));
                        },
                        releaseVersion
                    );

                    // Assert that no user publication roles allow updating a Release status once it has started publishing
                    await AssertReleaseVersionHandlerSucceedsWithCorrectPublicationRoles<TRequirement>(context =>
                        {
                            context.ReleaseVersions.Add(releaseVersion);
                            context.SaveChanges();

                            return authorizationHandler(releaseStatusRepository.Object,
                                new UserPublicationRoleRepository(context),
                                new UserReleaseRoleRepository(context));
                        },
                        releaseVersion
                    );
                }
            );
    }

    private static async Task AssertAllRolesFailWhenReleasePublished<TRequirement>(
        Func<IReleasePublishingStatusRepository, IUserPublicationRoleRepository, IUserReleaseRoleRepository,
            IAuthorizationHandler> authorizationHandler)
        where TRequirement : IAuthorizationRequirement
    {
        await GetEnums<ReleaseApprovalStatus>()
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(
                async status =>
                {
                    var releaseVersion = new ReleaseVersion
                    {
                        Id = Guid.NewGuid(),
                        Publication = new Publication { Id = Guid.NewGuid() },
                        ApprovalStatus = status,
                        Published = DateTime.Now,
                    };

                    var releaseStatusRepository = new Mock<IReleasePublishingStatusRepository>(Strict);

                    releaseStatusRepository.Setup(
                            s => s.GetAllByOverallStage(
                                releaseVersion.Id,
                                ReleasePublishingStatusOverallStage.Started,
                                ReleasePublishingStatusOverallStage.Complete
                            )
                        )
                        .ReturnsAsync(new List<ReleasePublishingStatus>());

                    // Assert that no user release roles allow updating a Release status once it has been published
                    await AssertReleaseVersionHandlerSucceedsWithCorrectReleaseRoles<TRequirement>(context =>
                        {
                            context.ReleaseVersions.Add(releaseVersion);
                            context.SaveChanges();

                            return authorizationHandler(releaseStatusRepository.Object,
                                new UserPublicationRoleRepository(context),
                                new UserReleaseRoleRepository(context));
                        },
                        releaseVersion
                    );

                    // Assert that no user publication roles allow updating a Release status once it has started publishing
                    await AssertReleaseVersionHandlerSucceedsWithCorrectPublicationRoles<TRequirement>(context =>
                        {
                            context.ReleaseVersions.Add(releaseVersion);
                            context.SaveChanges();

                            return authorizationHandler(releaseStatusRepository.Object,
                                new UserPublicationRoleRepository(context),
                                new UserReleaseRoleRepository(context));
                        },
                        releaseVersion
                    );
                }
            );
    }

    private static MarkReleaseAsDraftAuthorizationHandler BuildMarkReleaseAsDraftHandler(
        IReleasePublishingStatusRepository releasePublishingStatusRepository,
        IUserPublicationRoleRepository userPublicationRoleRepository,
        IUserReleaseRoleRepository userReleaseRoleRepository)
    {
        return new MarkReleaseAsDraftAuthorizationHandler(
            releasePublishingStatusRepository,
            new AuthorizationHandlerService(
                new ReleaseVersionRepository(InMemoryApplicationDbContext()),
                userReleaseRoleRepository,
                userPublicationRoleRepository,
                Mock.Of<IPreReleaseService>(Strict)));
    }

    private static MarkReleaseAsHigherLevelReviewAuthorizationHandler BuildMarkReleaseAsHigherLevelReviewHandler(
        IReleasePublishingStatusRepository releasePublishingStatusRepository,
        IUserPublicationRoleRepository userPublicationRoleRepository,
        IUserReleaseRoleRepository userReleaseRoleRepository)
    {
        return new MarkReleaseAsHigherLevelReviewAuthorizationHandler(
            releasePublishingStatusRepository,
            new AuthorizationHandlerService(
                new ReleaseVersionRepository(InMemoryApplicationDbContext()),
                userReleaseRoleRepository,
                userPublicationRoleRepository,
                Mock.Of<IPreReleaseService>(Strict)));
    }

    private static MarkReleaseAsApprovedAuthorizationHandler BuildMarkReleaseAsApprovedHandler(
        IReleasePublishingStatusRepository releasePublishingStatusRepository,
        IUserPublicationRoleRepository userPublicationRoleRepository,
        IUserReleaseRoleRepository userReleaseRoleRepository)
    {
        return new MarkReleaseAsApprovedAuthorizationHandler(
            releasePublishingStatusRepository,
            new AuthorizationHandlerService(
                new ReleaseVersionRepository(InMemoryApplicationDbContext()),
                userReleaseRoleRepository,
                userPublicationRoleRepository,
                Mock.Of<IPreReleaseService>(Strict)));
    }
}
