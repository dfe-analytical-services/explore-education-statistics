#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces.IReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class ReleasePermissionService(
    ContentDbContext contentDbContext,
    IPersistenceHelper<ContentDbContext> persistenceHelper,
    IReleaseVersionRepository releaseVersionRepository,
    IUserReleaseRoleRepository userReleaseRoleRepository,
    IUserService userService
) : IReleasePermissionService
{
    public async Task<Either<ActionResult, List<UserReleaseRoleSummaryViewModel>>> ListReleaseRoles(
        Guid releaseVersionId,
        ReleaseRole[]? rolesToInclude = null
    )
    {
        return await persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId, query => query.Include(rv => rv.Publication))
            .OnSuccessDo(releaseVersion => userService.CheckCanViewReleaseTeamAccess(releaseVersion.Publication))
            .OnSuccess(async _ =>
            {
                var users = await userReleaseRoleRepository.ListUserReleaseRoles(releaseVersionId, rolesToInclude);

                return users
                    .Select(userReleaseRole => new UserReleaseRoleSummaryViewModel(
                        userReleaseRole.UserId,
                        userReleaseRole.User.DisplayName,
                        userReleaseRole.User.Email,
                        userReleaseRole.Role
                    ))
                    .OrderBy(model => model.UserDisplayName)
                    .ToList();
            });
    }

    public async Task<Either<ActionResult, List<UserReleaseInviteViewModel>>> ListReleaseInvites(
        Guid releaseVersionId,
        ReleaseRole[]? rolesToInclude = null
    )
    {
        var rolesToCheck = rolesToInclude ?? EnumUtil.GetEnumsArray<ReleaseRole>();

        return await persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId, query => query.Include(rv => rv.Publication))
            .OnSuccessDo(releaseVersion => userService.CheckCanViewReleaseTeamAccess(releaseVersion.Publication))
            .OnSuccess(async _ =>
                await contentDbContext
                    .UserReleaseRoles.Where(urr => urr.ReleaseVersionId == releaseVersionId)
                    .Where(urr => rolesToCheck.Contains(urr.Role))
                    .Join(
                        contentDbContext.Users.WhereInvitePending(),
                        urr => urr.UserId,
                        u => u.Id,
                        (urr, u) => new UserReleaseInviteViewModel(u.Email, urr.Role)
                    )
                    .OrderBy(model => model.Email)
                    .ToListAsync()
            );
    }

    public async Task<Either<ActionResult, List<UserReleaseRoleSummaryViewModel>>> ListPublicationContributors(
        Guid publicationId
    )
    {
        return await persistenceHelper
            .CheckEntityExists<Publication>(publicationId)
            .OnSuccessDo(publication => userService.CheckCanUpdateReleaseRole(publication, ReleaseRole.Contributor))
            .OnSuccess(async () =>
            {
                var releaseVersionIds = await releaseVersionRepository.ListLatestReleaseVersionIds(publicationId);

                var users = await contentDbContext
                    .UserReleaseRoles.Include(releaseRole => releaseRole.User)
                    .Where(userReleaseRole =>
                        releaseVersionIds.Contains(userReleaseRole.ReleaseVersionId)
                        && userReleaseRole.Role == ReleaseRole.Contributor
                    )
                    .Select(userReleaseRole => userReleaseRole.User)
                    .Distinct()
                    .ToListAsync();

                return users
                    .Select(user => new UserReleaseRoleSummaryViewModel(
                        user.Id,
                        user.DisplayName,
                        user.Email,
                        ReleaseRole.Contributor
                    ))
                    .OrderBy(model => model.UserDisplayName)
                    .ToList();
            });
    }

    public async Task<Either<ActionResult, Unit>> UpdateReleaseContributors(Guid releaseVersionId, List<Guid> userIds)
    {
        return await persistenceHelper
            .CheckEntityExists<ReleaseVersion>(
                releaseVersionId,
                query => query.Include(rv => rv.Release).ThenInclude(r => r.Publication)
            )
            .OnSuccessDo(releaseVersion =>
                userService.CheckCanUpdateReleaseRole(releaseVersion.Release.Publication, ReleaseRole.Contributor)
            )
            .OnSuccessVoid(async releaseVersion =>
            {
                var releaseContributorReleaseRolesByUserId = await contentDbContext
                    .UserReleaseRoles.Include(releaseRole => releaseRole.User)
                    .Where(urr => urr.ReleaseVersionId == releaseVersion.Id)
                    .Where(urr => urr.Role == ReleaseRole.Contributor)
                    .ToDictionaryAsync(urr => urr.UserId);

                var releaseRolesToBeRemoved = releaseContributorReleaseRolesByUserId
                    .Where(kv => !userIds.Contains(kv.Key))
                    .Select(kv => kv.Value)
                    .ToList();

                var usersToBeAdded = userIds
                    .Where(userId => !releaseContributorReleaseRolesByUserId.ContainsKey(userId))
                    .ToList();

                await userReleaseRoleRepository.RemoveMany(releaseRolesToBeRemoved);

                await userReleaseRoleRepository.CreateManyIfNotExists(
                    userIds: usersToBeAdded,
                    releaseVersionId: releaseVersion.Id,
                    role: ReleaseRole.Contributor,
                    createdById: userService.GetUserId()
                );
            });
    }

    public async Task<Either<ActionResult, Unit>> RemoveAllUserContributorPermissionsForPublication(
        Guid publicationId,
        Guid userId
    )
    {
        return await persistenceHelper
            .CheckEntityExists<Publication>(publicationId)
            .OnSuccessDo(publication => userService.CheckCanUpdateReleaseRole(publication, ReleaseRole.Contributor))
            .OnSuccessVoid(async publication =>
            {
                await userReleaseRoleRepository.RemoveForPublicationAndUser(
                    userId: userId,
                    publicationId: publicationId,
                    rolesToInclude: ReleaseRole.Contributor
                );
            });
    }
}
