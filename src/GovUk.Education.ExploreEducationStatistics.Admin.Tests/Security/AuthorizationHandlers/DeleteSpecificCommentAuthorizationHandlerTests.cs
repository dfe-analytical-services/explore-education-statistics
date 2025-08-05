#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.
    ReleaseVersionAuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static Moq.MockBehavior;
using ReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.ReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers;

public class DeleteSpecificCommentAuthorizationHandlerTests
{
    [Fact]
    public async Task CanDeleteCommentAuthorizationHandler()
    {
        var comment = new Comment { Id = Guid.NewGuid(), };
        var releaseVersion = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
            ApprovalStatus = ReleaseApprovalStatus.Draft,
            Content = ListOf(
                new ContentSection
                {
                    Content = new List<ContentBlock> { new DataBlock { Comments = new List<Comment> { comment }, } }
                })
        };

        await AssertHandlerOnlySucceedsWithReleaseRoles<DeleteSpecificCommentRequirement, Comment>(
            releaseVersion.Id,
            comment,
            contentDbContext => contentDbContext.Add(releaseVersion),
            CreateHandler,
            ReleaseRole.Approver,
            ReleaseRole.Contributor);
    }

    private static DeleteSpecificCommentAuthorizationHandler CreateHandler(ContentDbContext contentDbContext)
    {
        var userRepository = new UserRepository(contentDbContext);

        var userReleaseRoleAndInviteManager = new UserReleaseRoleAndInviteManager(
            contentDbContext,
            new UserReleaseInviteRepository(contentDbContext),
            userRepository);

        var userPublicationRoleAndInviteManager = new UserPublicationRoleAndInviteManager(
            contentDbContext,
            new UserPublicationInviteRepository(contentDbContext),
            userRepository);

        return new DeleteSpecificCommentAuthorizationHandler(
            contentDbContext,
            new AuthorizationHandlerService(
                releaseVersionRepository: new ReleaseVersionRepository(contentDbContext),
                userReleaseRoleAndInviteManager: userReleaseRoleAndInviteManager,
                userPublicationRoleAndInviteManager: userPublicationRoleAndInviteManager,
                preReleaseService: Mock.Of<IPreReleaseService>(Strict)));
    }
}    
