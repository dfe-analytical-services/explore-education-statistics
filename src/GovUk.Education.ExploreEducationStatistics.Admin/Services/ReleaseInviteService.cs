#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using IReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces.IReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class ReleaseInviteService(
    ContentDbContext contentDbContext,
    IPersistenceHelper<ContentDbContext> contentPersistenceHelper,
    IReleaseVersionRepository releaseVersionRepository,
    IUserRepository userRepository,
    IUserService userService,
    IUserRoleService userRoleService,
    IUserReleaseRoleRepository userReleaseRoleRepository,
    IUserResourceRoleNotificationService userResourceRoleNotificationService
) : IReleaseInviteService
{
    public async Task<Either<ActionResult, Unit>> InviteContributor(
        string email,
        Guid publicationId,
        HashSet<Guid> releaseVersionIds
    )
    {
        return await contentPersistenceHelper
            .CheckEntityExists<Publication>(publicationId)
            .OnSuccessDo(publication => userService.CheckCanUpdateReleaseRole(publication, ReleaseRole.Contributor))
            .OnSuccessDo(() => ValidateReleaseVersionIds(publicationId, releaseVersionIds))
            .OnSuccessVoid(async publication =>
                InviteContributor(
                    email: email,
                    publicationTitle: publication!.Title,
                    releaseVersionIds: releaseVersionIds
                )
            );
    }

    public async Task<Either<ActionResult, Unit>> RemoveByPublication(
        string email,
        Guid publicationId,
        ReleaseRole releaseRole
    )
    {
        return await contentPersistenceHelper
            .CheckEntityExists<Publication>(publicationId, query => query.Include(p => p.ReleaseVersions))
            .OnSuccessCombineWith(_ => GetPendingUserInvite(email))
            .OnSuccess(tuple =>
            {
                var (publication, user) = tuple;
                return (publication, user);
            })
            .OnSuccessDo(tuple => userService.CheckCanUpdateReleaseRole(tuple.publication, releaseRole))
            .OnSuccess(async tuple =>
            {
                await userReleaseRoleRepository.RemoveForPublicationAndUser(
                    publicationId: publicationId,
                    userId: tuple.user.Id,
                    rolesToInclude: releaseRole
                );
                return Unit.Instance;
            });
    }

    private async Task InviteContributor(string email, string publicationTitle, HashSet<Guid> releaseVersionIds)
    {
        var activeUser = await userRepository.FindActiveUserByEmail(email);

        await contentDbContext.RequireTransaction(async () =>
        {
            var user =
                activeUser
                ?? await userRepository.CreateOrUpdate(
                    email: email,
                    role: Role.Analyst,
                    createdById: userService.GetUserId()
                );

            var userReleaseRoles = releaseVersionIds
                .Select(releaseVersionId => new UserReleaseRole
                {
                    UserId = user.Id,
                    ReleaseVersionId = releaseVersionId,
                    Role = ReleaseRole.Contributor,
                    CreatedById = userService.GetUserId(),
                    Created = DateTime.UtcNow,
                })
                .ToList();

            await userReleaseRoleRepository.CreateManyIfNotExists(userReleaseRoles);

            await userResourceRoleNotificationService.NotifyUserOfNewContributorRoles(
                userId: user.Id,
                publicationTitle: publicationTitle,
                releaseVersionIds: releaseVersionIds
            );

            if (user.Active)
            {
                var globalRoleNameToSet = userRoleService.GetAssociatedGlobalRoleNameForReleaseRole(
                    ReleaseRole.Contributor
                );
                await userRoleService.UpgradeToGlobalRoleIfRequired(globalRoleNameToSet, user.Id);
            }
        });
    }

    private async Task<Either<ActionResult, Unit>> ValidateReleaseVersionIds(
        Guid publicationId,
        HashSet<Guid> releaseVersionIds
    )
    {
        var publicationReleaseVersionIds = await releaseVersionRepository.ListLatestReleaseVersionIds(publicationId);

        if (!releaseVersionIds.All(publicationReleaseVersionIds.Contains))
        {
            return ValidationActionResult(NotAllReleasesBelongToPublication);
        }

        return Unit.Instance;
    }

    private async Task<Either<ActionResult, User>> GetPendingUserInvite(string email) =>
        await userRepository.FindPendingUserInviteByEmail(email)
        ?? new Either<ActionResult, User>(ValidationActionResult(InviteNotFound));
}
