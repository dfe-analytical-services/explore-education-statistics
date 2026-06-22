#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers;

public abstract class DeleteSpecificCommentAuthorizationHandlerTests
{
    private readonly DataFixture _dataFixture = new();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Comment _commentForDraftReleaseVersion;
    private readonly Comment _commentForApprovedReleaseVersion;
    private readonly ReleaseVersion _draftReleaseVersion;
    private readonly ReleaseVersion _approvedReleaseVersion;

    protected DeleteSpecificCommentAuthorizationHandlerTests()
    {
        _commentForDraftReleaseVersion = new() { Id = Guid.NewGuid() };

        _commentForApprovedReleaseVersion = new() { Id = Guid.NewGuid() };

        _draftReleaseVersion = _dataFixture
            .DefaultReleaseVersion()
            .WithApprovalStatus(ReleaseApprovalStatus.Draft)
            .WithGenericContent([
                _dataFixture
                    .DefaultContentSection()
                    .WithContentBlocks([new DataBlock { Comments = [_commentForDraftReleaseVersion] }]),
            ])
            .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

        _approvedReleaseVersion = _dataFixture
            .DefaultReleaseVersion()
            .WithApprovalStatus(ReleaseApprovalStatus.Approved)
            .WithGenericContent([
                _dataFixture
                    .DefaultContentSection()
                    .WithContentBlocks([new DataBlock { Comments = [_commentForApprovedReleaseVersion] }]),
            ])
            .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));
    }

    public class ClaimsTests : DeleteSpecificCommentAuthorizationHandlerTests
    {
        [Fact]
        public async Task SucceedsOnlyForValidClaims()
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                context.ReleaseVersions.Add(_draftReleaseVersion);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                // If the claims check fails, it will check the user's roles on the publication, but since we're testing claims here,
                // we want that to fail too, to ensure the claim is what's allowing access. So we let the IAuthorizationHandlerService default
                // to failing any role check, within the SetupHandler method.
                await AssertHandlerSucceedsWithCorrectClaims<DeleteSpecificCommentRequirement, Comment>(
                    handler: BuildHandler(context),
                    entity: _commentForDraftReleaseVersion,
                    userId: _userId,
                    claimsExpectedToSucceed: [SecurityClaimTypes.UpdateAllReleases]
                );
            }
        }

        [Fact]
        public async Task ApprovedReleaseVersion_FailsForAllClaims()
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                context.ReleaseVersions.Add(_approvedReleaseVersion);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await AssertHandlerFailsForAllClaims<DeleteSpecificCommentRequirement, Comment>(
                    handler: BuildHandler(context),
                    entity: _commentForApprovedReleaseVersion,
                    userId: _userId
                );
            }
        }
    }

    public class PublicationRolesTests : DeleteSpecificCommentAuthorizationHandlerTests
    {
        [Fact]
        public async Task SucceedsOnlyForValidPublicationRoles()
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                context.ReleaseVersions.Add(_draftReleaseVersion);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var handlerSuppler = (IAuthorizationHandlerService authorizationHandlerService) =>
                    BuildHandler(context, authorizationHandlerService);

                await AssertHandlerSucceedsForAnyValidPublicationRole<DeleteSpecificCommentRequirement, Comment>(
                    handlerSupplier: handlerSuppler,
                    entity: _commentForDraftReleaseVersion,
                    publicationId: _draftReleaseVersion.Release.PublicationId,
                    publicationRolesExpectedToSucceed: [PublicationRole.Drafter, PublicationRole.Approver]
                );
            }
        }

        [Fact]
        public async Task ApprovedReleaseVersion_FailsWithoutCheckingRoles()
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                context.ReleaseVersions.Add(_approvedReleaseVersion);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var handlerSuppler = (IAuthorizationHandlerService authorizationHandlerService) =>
                    BuildHandler(context, authorizationHandlerService);

                await AssertHandlerFailsWithoutCheckingRoles<DeleteSpecificCommentRequirement, Comment>(
                    handlerSupplier: handlerSuppler,
                    entity: _commentForApprovedReleaseVersion
                );
            }
        }
    }

    private DeleteSpecificCommentAuthorizationHandler BuildHandler(
        ContentDbContext? contentDbContext = null,
        IAuthorizationHandlerService? authorizationHandlerService = null
    )
    {
        contentDbContext ??= InMemoryApplicationDbContext();
        authorizationHandlerService ??= CreateDefaultAuthorizationHandlerService();

        return new(contentDbContext, authorizationHandlerService);
    }

    private IAuthorizationHandlerService CreateDefaultAuthorizationHandlerService()
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
