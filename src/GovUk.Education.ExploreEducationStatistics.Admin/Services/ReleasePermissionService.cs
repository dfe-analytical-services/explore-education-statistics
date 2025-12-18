#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class ReleasePermissionService(
    IPersistenceHelper<ContentDbContext> persistenceHelper,
    IUserReleaseRoleRepository userReleaseRoleRepository,
    IUserReleaseRoleService userReleaseRoleService,
    IUserService userService
) : IReleasePermissionService
{
    public async Task<Either<ActionResult, List<UserReleaseRoleSummaryViewModel>>> ListReleaseRoles(
        Guid releaseVersionId,
        ReleaseRole[]? rolesToInclude = null
    )
    {
        var rolesToCheck = rolesToInclude.IsNullOrEmpty() ? EnumUtil.GetEnumsArray<ReleaseRole>() : rolesToInclude;

        return await persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId, query => query.Include(rv => rv.Publication))
            .OnSuccessDo(releaseVersion => userService.CheckCanViewReleaseTeamAccess(releaseVersion.Publication))
            .OnSuccess(async _ =>
            {
                var userReleaseRoles = await userReleaseRoleRepository
                    .Query()
                    .AsNoTracking()
                    .WhereForReleaseVersion(releaseVersionId)
                    .WhereRolesIn(rolesToCheck)
                    .ToListAsync();

                return userReleaseRoles
                    .Select(urr => new UserReleaseRoleSummaryViewModel(
                        urr.UserId,
                        urr.User.DisplayName,
                        urr.User.Email,
                        urr.Role
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
                await userReleaseRoleRepository
                    .Query(ResourceRoleFilter.PendingOnly)
                    .AsNoTracking()
                    .WhereForReleaseVersion(releaseVersionId)
                    .WhereRolesIn(rolesToCheck)
                    .Where(urr => rolesToCheck.Contains(urr.Role))
                    .Select(urr => new UserReleaseInviteViewModel(urr.User.Email, urr.Role))
                    .OrderBy(viewModel => viewModel.Email)
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
                var users = (
                    await userReleaseRoleService.ListLatestActiveUserReleaseRolesByPublication(
                        publicationId: publicationId,
                        rolesToInclude: ReleaseRole.Contributor
                    )
                )
                    .Select(userReleaseRole => userReleaseRole.User)
                    .Distinct()
                    .ToList();

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
                var releaseContributorReleaseRolesByUserId = await userReleaseRoleRepository
                    .Query()
                    .WhereForReleaseVersion(releaseVersion.Id)
                    .WhereRolesIn(ReleaseRole.Contributor)
                    .ToDictionaryAsync(urr => urr.UserId);

                var releaseRolesToBeRemoved = releaseContributorReleaseRolesByUserId
                    .Where(kv => !userIds.Contains(kv.Key))
                    .Select(kv => kv.Value)
                    .ToList();

                var usersToBeAdded = userIds
                    .Where(userId => !releaseContributorReleaseRolesByUserId.ContainsKey(userId))
                    .ToList();

                await userReleaseRoleRepository.RemoveMany(releaseRolesToBeRemoved);

                var newUserReleaseRoles = usersToBeAdded
                    .Select(userId => new UserReleaseRole
                    {
                        UserId = userId,
                        ReleaseVersionId = releaseVersion.Id,
                        Role = ReleaseRole.Contributor,
                        CreatedById = userService.GetUserId(),
                        Created = DateTime.UtcNow,
                    })
                    .ToList();

                await userReleaseRoleRepository.CreateManyIfNotExists(newUserReleaseRoles);
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
                var releaseRolesToRemove = await userReleaseRoleRepository
                    .Query()
                    .WhereForUser(userId)
                    .WhereForPublication(publicationId)
                    .WhereRolesIn(ReleaseRole.Contributor)
                    .ToListAsync();

                await userReleaseRoleRepository.RemoveMany(releaseRolesToRemove);
            });
    }
}
