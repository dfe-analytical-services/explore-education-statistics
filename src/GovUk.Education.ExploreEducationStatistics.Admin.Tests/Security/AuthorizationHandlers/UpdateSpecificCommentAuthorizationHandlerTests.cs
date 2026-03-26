#nullable enable
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers;

public abstract class UpdateSpecificCommentAuthorizationHandlerTests
{
    private static readonly DataFixture _dataFixture = new();

    private static readonly Guid _userId = Guid.NewGuid();

    private static readonly Comment _commentCreatedBySameUser = new() { Id = Guid.NewGuid(), CreatedById = _userId };

    private static readonly Comment _commentCreatedByDifferentUser = new()
    {
        Id = Guid.NewGuid(),
        CreatedById = Guid.NewGuid(),
    };

    private static readonly ReleaseVersion _draftReleaseVersionWithCommentCreatedBySameUser = _dataFixture
        .DefaultReleaseVersion()
        .WithApprovalStatus(ReleaseApprovalStatus.Draft)
        .WithContent([
            _dataFixture
                .DefaultContentSection()
                .WithContentBlocks([new DataBlock { Comments = [_commentCreatedBySameUser] }]),
        ])
        .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

    private static readonly ReleaseVersion _draftReleaseVersionWithCommentCreatedByDifferedUser = _dataFixture
        .DefaultReleaseVersion()
        .WithApprovalStatus(ReleaseApprovalStatus.Draft)
        .WithContent([
            _dataFixture
                .DefaultContentSection()
                .WithContentBlocks([new DataBlock { Comments = [_commentCreatedByDifferentUser] }]),
        ])
        .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

    private static readonly ReleaseVersion _approvedReleaseVersionWithCommentCreatedBySameUser = _dataFixture
        .DefaultReleaseVersion()
        .WithApprovalStatus(ReleaseApprovalStatus.Approved)
        .WithContent([
            _dataFixture
                .DefaultContentSection()
                .WithContentBlocks([new DataBlock { Comments = [_commentCreatedBySameUser] }]),
        ])
        .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

    private static readonly ReleaseVersion _approvedReleaseVersionWithCommentCreatedByDifferedUser = _dataFixture
        .DefaultReleaseVersion()
        .WithApprovalStatus(ReleaseApprovalStatus.Approved)
        .WithContent([
            _dataFixture
                .DefaultContentSection()
                .WithContentBlocks([new DataBlock { Comments = [_commentCreatedByDifferentUser] }]),
        ])
        .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

    public class ClaimsTests : UpdateSpecificCommentAuthorizationHandlerTests
    {
        [Fact]
        public async Task DraftReleaseVersionWithCommentCreatedBySameUser_SucceedsOnlyForValidClaims()
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                context.ReleaseVersions.Add(_draftReleaseVersionWithCommentCreatedBySameUser);
                await context.SaveChangesAsync();
            }

            await using (var context = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                await AssertHandlerSucceedsWithCorrectClaims<UpdateSpecificCommentRequirement, Comment>(
                    handler: SetupHandler(context),
                    entity: _commentCreatedBySameUser,
                    userId: _userId,
                    claimsExpectedToSucceed: [SecurityClaimTypes.UpdateAllReleases]
                );
            }
        }

        [Fact]
        public async Task DraftReleaseVersionWithCommentCreatedByDifferentUser_FailsForAllClaims()
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                context.ReleaseVersions.Add(_draftReleaseVersionWithCommentCreatedByDifferedUser);
                await context.SaveChangesAsync();
            }

            await using (var context = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                await AssertHandlerFailsForAllClaims<UpdateSpecificCommentRequirement, Comment>(
                    handler: SetupHandler(context),
                    entity: _commentCreatedByDifferentUser,
                    userId: _userId
                );
            }
        }

        [Fact]
        public async Task ApprovedReleaseVersionWithCommentCreatedBySameUser_FailsForAllClaims()
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                context.ReleaseVersions.Add(_approvedReleaseVersionWithCommentCreatedBySameUser);
                await context.SaveChangesAsync();
            }

            await using (var context = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                await AssertHandlerFailsForAllClaims<UpdateSpecificCommentRequirement, Comment>(
                    handler: SetupHandler(context),
                    entity: _commentCreatedBySameUser,
                    userId: _userId
                );
            }
        }

        [Fact]
        public async Task ApprovedReleaseVersionWithCommentCreatedByDifferentUser_FailsForAllClaims()
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                context.ReleaseVersions.Add(_approvedReleaseVersionWithCommentCreatedByDifferedUser);
                await context.SaveChangesAsync();
            }

            await using (var context = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                await AssertHandlerFailsForAllClaims<UpdateSpecificCommentRequirement, Comment>(
                    handler: SetupHandler(context),
                    entity: _commentCreatedByDifferentUser,
                    userId: _userId
                );
            }
        }
    }

    public class PublicationRolesTests : UpdateSpecificCommentAuthorizationHandlerTests
    {
        [Fact]
        public async Task DraftReleaseVersionWithCommentCreatedBySameUser_SucceedsOnlyForValidPublicationRoles()
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                context.ReleaseVersions.Add(_draftReleaseVersionWithCommentCreatedBySameUser);
                await context.SaveChangesAsync();
            }

            await using (var context = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                var handlerSuppler = (IAuthorizationHandlerService authorizationHandlerService) =>
                    SetupHandler(context, authorizationHandlerService);

                await AssertHandlerSucceedsForAnyValidPublicationRole<UpdateSpecificCommentRequirement, Comment>(
                    handlerSupplier: handlerSuppler,
                    entity: _commentCreatedBySameUser,
                    publicationId: _draftReleaseVersionWithCommentCreatedBySameUser.Release.PublicationId,
                    publicationRolesExpectedToSucceed: [PublicationRole.Drafter, PublicationRole.Approver],
                    userId: _userId
                );
            }
        }

        [Fact]
        public async Task DraftReleaseVersionWithCommentCreatedByDifferentUser_PassesRolesCheck_StillFails()
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                context.ReleaseVersions.Add(_draftReleaseVersionWithCommentCreatedByDifferedUser);
                await context.SaveChangesAsync();
            }

            await using (var context = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                var authorizationHandlerService = new Mock<IAuthorizationHandlerService>(MockBehavior.Strict);
                authorizationHandlerService
                    .Setup(s =>
                        s.UserHasAnyPublicationRoleOnPublication(
                            _userId,
                            _draftReleaseVersionWithCommentCreatedByDifferedUser.Release.PublicationId,
                            CollectionUtils.SetOf(PublicationRole.Drafter, PublicationRole.Approver)
                        )
                    )
                    .ReturnsAsync(true);

                ClaimsPrincipal user = _dataFixture.AuthenticatedUser(_userId);

                var authContext = AuthorizationHandlerContextFactory.CreateAuthContext<
                    UpdateSpecificCommentRequirement,
                    Comment
                >(user, _commentCreatedByDifferentUser);

                var handler = SetupHandler(context, authorizationHandlerService.Object);

                await handler.HandleAsync(authContext);

                Assert.False(authContext.HasSucceeded);
            }
        }

        [Fact]
        public async Task ApprovedReleaseVersionWithCommentCreatedBySameUser_FailsWithoutCheckingRoles()
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                context.ReleaseVersions.Add(_approvedReleaseVersionWithCommentCreatedBySameUser);
                await context.SaveChangesAsync();
            }

            await using (var context = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                var handlerSuppler = (IAuthorizationHandlerService authorizationHandlerService) =>
                    SetupHandler(context, authorizationHandlerService);

                await AssertHandlerFailsWithoutCheckingRoles<UpdateSpecificCommentRequirement, Comment>(
                    handlerSupplier: handlerSuppler,
                    entity: _commentCreatedBySameUser,
                    userId: _userId
                );
            }
        }

        [Fact]
        public async Task ApprovedReleaseVersionWithCommentCreatedByDifferentUser_FailsWithoutCheckingRoles()
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                context.ReleaseVersions.Add(_approvedReleaseVersionWithCommentCreatedByDifferedUser);
                await context.SaveChangesAsync();
            }

            await using (var context = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                var handlerSuppler = (IAuthorizationHandlerService authorizationHandlerService) =>
                    SetupHandler(context, authorizationHandlerService);

                await AssertHandlerFailsWithoutCheckingRoles<UpdateSpecificCommentRequirement, Comment>(
                    handlerSupplier: handlerSuppler,
                    entity: _commentCreatedByDifferentUser,
                    userId: _userId
                );
            }
        }
    }

    private static UpdateSpecificCommentAuthorizationHandler SetupHandler(
        ContentDbContext? contentDbContext = null,
        IAuthorizationHandlerService? authorizationHandlerService = null
    )
    {
        contentDbContext ??= DbUtils.InMemoryApplicationDbContext();
        authorizationHandlerService ??= CreateDefaultAuthorizationHandlerService();

        return new(contentDbContext, authorizationHandlerService);
    }

    private static IAuthorizationHandlerService CreateDefaultAuthorizationHandlerService()
    {
        var mock = new Mock<IAuthorizationHandlerService>(MockBehavior.Strict);
        mock.Setup(s =>
                s.UserHasAnyPublicationRoleOnPublication(
                    _userId,
                    It.IsAny<Guid>(),
                    CollectionUtils.SetOf(PublicationRole.Drafter, PublicationRole.Approver)
                )
            )
            .ReturnsAsync(false);

        return mock.Object;
    }
}
