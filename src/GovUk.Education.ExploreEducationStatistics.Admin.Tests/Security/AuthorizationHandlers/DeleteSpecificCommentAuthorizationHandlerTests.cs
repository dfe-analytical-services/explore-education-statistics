#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.Extensions.Logging;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.ReleaseVersionAuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static Moq.MockBehavior;
using ReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.ReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers;

public class DeleteSpecificCommentAuthorizationHandlerTests
{
    [Fact]
    public async Task CanDeleteCommentAuthorizationHandler()
    {
        var comment = new Comment { Id = Guid.NewGuid() };
        var releaseVersion = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
            ApprovalStatus = ReleaseApprovalStatus.Draft,
            Content = ListOf(
                new ContentSection
                {
                    Content = new List<ContentBlock> { new DataBlock { Comments = new List<Comment> { comment } } },
                }
            ),
        };

        await AssertHandlerOnlySucceedsWithReleaseRoles<DeleteSpecificCommentRequirement, Comment>(
            releaseVersion.Id,
            comment,
            contentDbContext => contentDbContext.Add(releaseVersion),
            CreateHandler,
            ReleaseRole.Approver,
            ReleaseRole.Contributor
        );
    }

    private static DeleteSpecificCommentAuthorizationHandler CreateHandler(ContentDbContext contentDbContext)
    {
        var userReleaseRoleRepository = new UserReleaseRoleRepository(
            contentDbContext,
            logger: Mock.Of<ILogger<UserReleaseRoleRepository>>()
        );

        var userPublicationRoleRepository = new UserPublicationRoleRepository(contentDbContext);

        return new DeleteSpecificCommentAuthorizationHandler(
            contentDbContext,
            new AuthorizationHandlerService(
                releaseVersionRepository: new ReleaseVersionRepository(contentDbContext),
                userReleaseRoleRepository: userReleaseRoleRepository,
                userPublicationRoleRepository: userPublicationRoleRepository,
                preReleaseService: Mock.Of<IPreReleaseService>(Strict)
            )
        );
    }
}
