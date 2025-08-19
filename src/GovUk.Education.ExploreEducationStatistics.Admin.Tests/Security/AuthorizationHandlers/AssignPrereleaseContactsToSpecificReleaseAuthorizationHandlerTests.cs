#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.
    AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.
    ReleaseVersionAuthorizationHandlersTestUtil;
using static Moq.MockBehavior;
using ReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.ReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers;

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
            await AssertHandlerSucceedsWithCorrectClaims
                <ReleaseVersion, AssignPrereleaseContactsToSpecificReleaseRequirement>(
                    CreateHandler,
                    new ReleaseVersion
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
            await AssertHandlerSucceedsWithCorrectClaims
                <ReleaseVersion, AssignPrereleaseContactsToSpecificReleaseRequirement>(
                    CreateHandler,
                    new ReleaseVersion
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
            var releaseVersion = new ReleaseVersion
            {
                Publication = new Publication
                {
                    Id = Guid.NewGuid()
                },
                ApprovalStatus = ReleaseApprovalStatus.Draft
            };

            // Assert that users with the Publication Owner role can assign pre release contacts to a release
            // that's unapproved
            await AssertReleaseVersionHandlerSucceedsWithCorrectPublicationRoles<
                AssignPrereleaseContactsToSpecificReleaseRequirement>(
                contentDbContext =>
                {
                    contentDbContext.Add(releaseVersion);
                    contentDbContext.SaveChanges();

                    return CreateHandler(contentDbContext);
                },
                releaseVersion,
                PublicationRole.Owner, PublicationRole.Allower);
        }

        [Fact]
        public async Task AssignPrereleaseContactsToSpecificReleaseAuthorizationHandler_Approved()
        {
            var releaseVersion = new ReleaseVersion
            {
                Publication = new Publication
                {
                    Id = Guid.NewGuid()
                },
                ApprovalStatus = ReleaseApprovalStatus.Approved
            };

            // Assert that users with the Publication Owner role can assign pre release contacts to a release
            // that's approved
            await AssertReleaseVersionHandlerSucceedsWithCorrectPublicationRoles<
                AssignPrereleaseContactsToSpecificReleaseRequirement>(
                contentDbContext =>
                {
                    contentDbContext.Add(releaseVersion);
                    contentDbContext.SaveChanges();

                    return CreateHandler(contentDbContext);
                },
                releaseVersion,
                PublicationRole.Owner, PublicationRole.Allower);
        }
    }

    public class ReleaseRoleTests
    {
        [Fact]
        public async Task AssignPrereleaseContactsToSpecificReleaseAuthorizationHandler_NotApproved()
        {
            // Assert that users with an editor User Release role can assign pre release contacts to a release
            // that's unapproved
            await AssertReleaseVersionHandlerSucceedsWithCorrectReleaseRoles<
                AssignPrereleaseContactsToSpecificReleaseRequirement>(
                CreateHandler,
                new ReleaseVersion
                {
                    Id = Guid.NewGuid(),
                    ApprovalStatus = ReleaseApprovalStatus.Draft
                },
                ReleaseRole.Approver, ReleaseRole.Contributor);
        }

        [Fact]
        public async Task AssignPrereleaseContactsToSpecificReleaseAuthorizationHandler_Approved()
        {
            // Assert that users with an editor User Release role can assign pre release contacts to a release
            // that's approved
            await AssertReleaseVersionHandlerSucceedsWithCorrectReleaseRoles<
                AssignPrereleaseContactsToSpecificReleaseRequirement>(
                CreateHandler,
                new ReleaseVersion
                {
                    Id = Guid.NewGuid(),
                    ApprovalStatus = ReleaseApprovalStatus.Approved
                },
                ReleaseRole.Approver, ReleaseRole.Contributor);
        }
    }

    private static IAuthorizationHandler CreateHandler(ContentDbContext contentDbContext)
    {
        var userRepository = new UserRepository(contentDbContext);

        var userReleaseRoleRepository = new UserReleaseRoleRepository(
            contentDbContext,
            userRepository,
            logger: Mock.Of<ILogger<UserReleaseRoleRepository>>());

        var userPublicationRoleRepository = new UserPublicationRoleRepository(
            contentDbContext,
            userRepository,
            logger: Mock.Of<ILogger<UserPublicationRoleRepository>>());

        return new AssignPrereleaseContactsToSpecificReleaseAuthorizationHandler(
            new AuthorizationHandlerService(
                releaseVersionRepository: new ReleaseVersionRepository(contentDbContext),
                userReleaseRoleRepository: userReleaseRoleRepository,
                userPublicationRoleRepository: userPublicationRoleRepository,
                preReleaseService: Mock.Of<IPreReleaseService>(Strict))
        );
    }
}
