#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.
    AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.
    ReleaseVersionAuthorizationHandlersTestUtil;
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
            CreateHandler,
                MarkAllReleasesAsDraft
            );
        }

        [Fact]
        public async Task AllClaimsFail_ReleasePublishing()
        {
            await AssertAllClaimsFailWhenReleasePublishing<MarkReleaseAsDraftRequirement>(
            CreateHandler
            );
        }

        [Fact]
        public async Task AllClaimsFail_ReleasePublished()
        {
            await AssertAllClaimsFailWhenReleasePublished<MarkReleaseAsDraftRequirement>(
            CreateHandler
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

                                    return CreateHandler(context, releaseStatusRepository.Object);
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

                                    return CreateHandler(context, releaseStatusRepository.Object);
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

                                    return CreateHandler(context, releaseStatusRepository.Object);
                                },
                                releaseVersion,
                                Owner,
                                Allower
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

                                    return CreateHandler(context, releaseStatusRepository.Object);
                                },
                                releaseVersion,
                                Allower
                            );
                        }
                    }
                );
        }

        [Fact]
        public async Task AllRolesFail_ReleasePublishing()
        {
            await AssertAllRolesFailWhenReleasePublishing<MarkReleaseAsDraftRequirement>(
            CreateHandler
            );
        }

        [Fact]
        public async Task AllRolesFail_ReleasePublished()
        {
            await AssertAllRolesFailWhenReleasePublished<MarkReleaseAsDraftRequirement>(
            CreateHandler
            );
        }

        private static MarkReleaseAsDraftAuthorizationHandler CreateHandler(
        ContentDbContext context,
        IReleasePublishingStatusRepository releasePublishingStatusRepository)
        {
            return new MarkReleaseAsDraftAuthorizationHandler(
                releasePublishingStatusRepository: releasePublishingStatusRepository,
                new AuthorizationHandlerService(
                    releaseVersionRepository: new ReleaseVersionRepository(context),
                    userPublicationRoleRepository: new UserPublicationRoleRepository(
                        contentDbContext: context),
                    userReleaseRoleRepository: new UserReleaseRoleRepository(
                        contentDbContext: context,
                        logger: Mock.Of<ILogger<UserReleaseRoleRepository>>()),
                    preReleaseService: Mock.Of<IPreReleaseService>(Strict)));
        }
    }

    public class MarkReleaseAsHigherLevelReviewAuthorizationHandlerTests
    {
        [Fact]
        public async Task ClaimSuccess_SubmitAllReleasesToHigherReview_ReleaseUnpublished()
        {
            await AssertClaimSucceedsWhenReleaseUnpublished<MarkReleaseAsHigherLevelReviewRequirement>(
            CreateHandler,
                SubmitAllReleasesToHigherReview
            );
        }

        [Fact]
        public async Task AllClaimsFail_ReleasePublishing()
        {
            await AssertAllClaimsFailWhenReleasePublishing<MarkReleaseAsHigherLevelReviewRequirement>(
            CreateHandler
            );
        }

        [Fact]
        public async Task AllClaimsFail_ReleasePublished()
        {
            await AssertAllClaimsFailWhenReleasePublished<MarkReleaseAsHigherLevelReviewRequirement>(
            CreateHandler
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

                                    return CreateHandler(context, releaseStatusRepository.Object);
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

                                    return CreateHandler(context, releaseStatusRepository.Object);
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

                                    return CreateHandler(context, releaseStatusRepository.Object);
                                },
                                releaseVersion,
                                Owner,
                                Allower
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

                                    return CreateHandler(context, releaseStatusRepository.Object);
                                },
                                releaseVersion,
                                Allower
                            );
                        }
                    }
                );
        }

        [Fact]
        public async Task AllRolesFail_ReleasePublishing()
        {
            await AssertAllRolesFailWhenReleasePublishing<MarkReleaseAsHigherLevelReviewRequirement>(
            CreateHandler
            );
        }

        [Fact]
        public async Task AllRolesFail_ReleasePublished()
        {
            await AssertAllRolesFailWhenReleasePublished<MarkReleaseAsHigherLevelReviewRequirement>(
            CreateHandler
            );
        }

        private static MarkReleaseAsHigherLevelReviewAuthorizationHandler CreateHandler(
        ContentDbContext context,
        IReleasePublishingStatusRepository releasePublishingStatusRepository)
        {
            return new MarkReleaseAsHigherLevelReviewAuthorizationHandler(
                releasePublishingStatusRepository: releasePublishingStatusRepository,
                new AuthorizationHandlerService(
                    releaseVersionRepository: new ReleaseVersionRepository(context),
                    userPublicationRoleRepository: new UserPublicationRoleRepository(
                        contentDbContext: context),
                    userReleaseRoleRepository: new UserReleaseRoleRepository(
                        contentDbContext: context,
                        logger: Mock.Of<ILogger<UserReleaseRoleRepository>>()),
                    preReleaseService: Mock.Of<IPreReleaseService>(Strict)));
        }
    }

    public class MarkReleaseAsApprovedAuthorizationHandlerTests
    {
        [Fact]
        public async Task ClaimSuccess_ApproveAllReleases_ReleaseUnpublished()
        {
            await AssertClaimSucceedsWhenReleaseUnpublished<MarkReleaseAsApprovedRequirement>(
            CreateHandler,
                ApproveAllReleases
            );
        }

        [Fact]
        public async Task AllClaimsFail_ReleasePublishing()
        {
            await AssertAllClaimsFailWhenReleasePublishing<MarkReleaseAsApprovedRequirement>(
            CreateHandler
            );
        }

        [Fact]
        public async Task AllClaimsFail_ReleasePublished()
        {
            await AssertAllClaimsFailWhenReleasePublished<MarkReleaseAsApprovedRequirement>(
            CreateHandler
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

                                return CreateHandler(context, releaseStatusRepository.Object);
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

                                var userRepository = new UserRepository(context);

                                return CreateHandler(context, releaseStatusRepository.Object);
                            },
                            releaseVersion,
                            Allower
                        );
                    }
                );
        }

        [Fact]
        public async Task AllRolesFail_ReleasePublishing()
        {
            await AssertAllRolesFailWhenReleasePublishing<MarkReleaseAsApprovedRequirement>(
            CreateHandler
            );
        }

        [Fact]
        public async Task AllRolesFail_ReleasePublished()
        {
            await AssertAllRolesFailWhenReleasePublished<MarkReleaseAsApprovedRequirement>(
            CreateHandler
            );
        }

        private static MarkReleaseAsApprovedAuthorizationHandler CreateHandler(
        ContentDbContext context,
        IReleasePublishingStatusRepository releasePublishingStatusRepository)
        {
            return new MarkReleaseAsApprovedAuthorizationHandler(
                releasePublishingStatusRepository: releasePublishingStatusRepository,
                new AuthorizationHandlerService(
                    releaseVersionRepository: new ReleaseVersionRepository(context),
                    userPublicationRoleRepository: new UserPublicationRoleRepository(
                        contentDbContext: context),
                    userReleaseRoleRepository: new UserReleaseRoleRepository(
                        contentDbContext: context,
                        logger: Mock.Of<ILogger<UserReleaseRoleRepository>>()),
                    preReleaseService: Mock.Of<IPreReleaseService>(Strict)));
        }
    }

    private static async Task AssertClaimSucceedsWhenReleaseUnpublished<TRequirement>(
    Func<ContentDbContext, IReleasePublishingStatusRepository, IAuthorizationHandler> authorizationHandlerSupplier,
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

                            return authorizationHandlerSupplier(context, releaseStatusRepository.Object);
                        },
                        releaseVersion,
                        claims
                    );
                }
            );
    }

    private static async Task AssertAllClaimsFailWhenReleasePublishing<TRequirement>(
    Func<ContentDbContext, IReleasePublishingStatusRepository, IAuthorizationHandler> authorizationHandlerSupplier)
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

                            return authorizationHandlerSupplier(context, releaseStatusRepository.Object);
                        },
                        releaseVersion
                    );
                }
            );
    }

    private static async Task AssertAllClaimsFailWhenReleasePublished<TRequirement>(
    Func<ContentDbContext, IReleasePublishingStatusRepository, IAuthorizationHandler> authorizationHandlerSupplier)
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

                            return authorizationHandlerSupplier(context, releaseStatusRepository.Object);
                        },
                        releaseVersion
                    );
                }
            );
    }

    private static async Task AssertAllRolesFailWhenReleasePublishing<TRequirement>(
    Func<ContentDbContext, IReleasePublishingStatusRepository, IAuthorizationHandler> authorizationHandlerSupplier)
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

                            return authorizationHandlerSupplier(context, releaseStatusRepository.Object);
                        },
                        releaseVersion
                    );

                    // Assert that no user publication roles allow updating a Release status once it has started publishing
                    await AssertReleaseVersionHandlerSucceedsWithCorrectPublicationRoles<TRequirement>(context =>
                        {
                            context.ReleaseVersions.Add(releaseVersion);
                            context.SaveChanges();

                            return authorizationHandlerSupplier(context, releaseStatusRepository.Object);
                        },
                        releaseVersion
                    );
                }
            );
    }

    private static async Task AssertAllRolesFailWhenReleasePublished<TRequirement>(
    Func<ContentDbContext, IReleasePublishingStatusRepository, IAuthorizationHandler> authorizationHandlerSupplier)
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

                            var userRepository = new UserRepository(context);

                            return authorizationHandlerSupplier(context, releaseStatusRepository.Object);
                        },
                        releaseVersion
                    );

                    // Assert that no user publication roles allow updating a Release status once it has started publishing
                    await AssertReleaseVersionHandlerSucceedsWithCorrectPublicationRoles<TRequirement>(context =>
                        {
                            context.ReleaseVersions.Add(releaseVersion);
                            context.SaveChanges();

                            var userRepository = new UserRepository(context);

                            return authorizationHandlerSupplier(context, releaseStatusRepository.Object);
                        },
                        releaseVersion
                    );
                }
            );
    }
}
