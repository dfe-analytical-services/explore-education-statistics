#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.ReleaseVersionAuthorizationHandlersTestUtil;
using static Moq.MockBehavior;
using ReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.ReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers;

public class DeleteSpecificCommentAuthorizationHandlerTests
{
    private static readonly DataFixture _dataFixture = new();

    [Fact]
    public async Task CanDeleteCommentAuthorizationHandler()
    {
        var comment = new Comment { Id = Guid.NewGuid() };
        ReleaseVersion releaseVersion = _dataFixture
            .DefaultReleaseVersion()
            .WithApprovalStatus(ReleaseApprovalStatus.Draft)
            .WithContent([
                _dataFixture.DefaultContentSection().WithContentBlocks([new DataBlock { Comments = [comment] }]),
            ])
            .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

        await AssertHandlerOnlySucceedsWithReleaseRoles<DeleteSpecificCommentRequirement, Comment>(
            releaseVersion,
            comment,
            contentDbContext => contentDbContext.Add(releaseVersion),
            CreateHandler,
            ReleaseRole.Approver,
            ReleaseRole.Contributor
        );
    }

    private static DeleteSpecificCommentAuthorizationHandler CreateHandler(ContentDbContext contentDbContext)
    {
        var userReleaseRoleRepository = new UserReleaseRoleRepository(contentDbContext);

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
