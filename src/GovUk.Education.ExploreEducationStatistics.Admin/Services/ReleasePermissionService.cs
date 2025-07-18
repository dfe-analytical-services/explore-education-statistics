#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseRole;
using IReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces.IReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class ReleasePermissionService : IReleasePermissionService
{
    private readonly ContentDbContext _contentDbContext;
    private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
    private readonly IReleaseVersionRepository _releaseVersionRepository;
    private readonly IUserReleaseRoleRepository _userReleaseRoleRepository;
    private readonly IUserReleaseInviteRepository _userReleaseInviteRepository;
    private readonly IUserService _userService;

    public ReleasePermissionService(
        ContentDbContext contentDbContext,
        IPersistenceHelper<ContentDbContext> persistenceHelper,
        IReleaseVersionRepository releaseVersionRepository,
        IUserReleaseRoleRepository userReleaseRoleRepository,
        IUserReleaseInviteRepository userReleaseInviteRepository,
        IUserService userService)
    {
        _contentDbContext = contentDbContext;
        _persistenceHelper = persistenceHelper;
        _releaseVersionRepository = releaseVersionRepository;
        _userReleaseRoleRepository = userReleaseRoleRepository;
        _userReleaseInviteRepository = userReleaseInviteRepository;
        _userService = userService;
    }

    public async Task<Either<ActionResult, List<UserReleaseRoleSummaryViewModel>>>
        ListReleaseRoles(Guid releaseVersionId, ReleaseRole[]? rolesToInclude = null)
    {
        return await _persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId,
                query =>
                    query.Include(rv => rv.Publication))
            .OnSuccessDo(releaseVersion => _userService.CheckCanViewReleaseTeamAccess(releaseVersion.Publication))
            .OnSuccess(async _ =>
            {
                var users = await _userReleaseRoleRepository
                    .ListUserReleaseRoles(releaseVersionId, rolesToInclude);

                return users
                    .Select(userReleaseRole =>
                        new UserReleaseRoleSummaryViewModel(
                            userReleaseRole.UserId,
                            userReleaseRole.User.DisplayName,
                            userReleaseRole.User.Email,
                            userReleaseRole.Role))
                    .OrderBy(model => model.UserDisplayName)
                    .ToList();
            });
    }

    public async Task<Either<ActionResult, List<UserReleaseInviteViewModel>>>
        ListReleaseInvites(Guid releaseVersionId, ReleaseRole[]? rolesToInclude = null)
    {
        var rolesToCheck = rolesToInclude ?? EnumUtil.GetEnumsArray<ReleaseRole>();

        return await _persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId,
                query =>
                    query.Include(rv => rv.Publication))
            .OnSuccessDo(releaseVersion => _userService.CheckCanViewReleaseTeamAccess(releaseVersion.Publication))
            .OnSuccess(async _ =>
            {
                var invites = await _contentDbContext
                    .UserReleaseInvites
                    .Where(i =>
                        i.ReleaseVersionId == releaseVersionId
                        && rolesToCheck.Contains(i.Role))
                    .ToListAsync();

                return invites
                    .Select(i => new UserReleaseInviteViewModel(i.Email, i.Role))
                    .OrderBy(model => model.Email)
                    .ToList();
            });
    }

    public async Task<Either<ActionResult, List<UserReleaseRoleSummaryViewModel>>>
        ListPublicationContributors(Guid publicationId)
    {
        return await _persistenceHelper
            .CheckEntityExists<Publication>(publicationId)
            .OnSuccessDo(publication => _userService
                .CheckCanUpdateReleaseRole(publication, Contributor))
            .OnSuccess(async () =>
            {
                var releaseVersionIds = await _releaseVersionRepository.ListLatestReleaseVersionIds(publicationId);

                var users = await _contentDbContext
                    .UserReleaseRoles
                    .Include(releaseRole => releaseRole.User)
                    .Where(userReleaseRole =>
                        releaseVersionIds.Contains(userReleaseRole.ReleaseVersionId)
                        && userReleaseRole.Role == Contributor)
                    .Select(userReleaseRole =>
                        userReleaseRole.User)
                    .Distinct()
                    .ToListAsync();

                return users
                    .Select(user =>
                        new UserReleaseRoleSummaryViewModel(
                            user.Id,
                            user.DisplayName,
                            user.Email,
                            Contributor))
                    .OrderBy(model => model.UserDisplayName)
                    .ToList();
            });
    }

    public async Task<Either<ActionResult, Unit>> UpdateReleaseContributors(
        Guid releaseVersionId, List<Guid> userIds)
    {
        return await _persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId,
                query =>
                    query.Include(rv => rv.Release)
                        .ThenInclude(r => r.Publication))
            .OnSuccessDo(releaseVersion => _userService
                .CheckCanUpdateReleaseRole(releaseVersion.Release.Publication, Contributor))
            .OnSuccess(async releaseVersion =>
            {
                var releaseContributorReleaseRoles = await _contentDbContext.UserReleaseRoles
                    .Include(releaseRole => releaseRole.User)
                    .Where(userReleaseRole =>
                        userReleaseRole.ReleaseVersionId == releaseVersion.Id
                        && userReleaseRole.Role == Contributor)
                    .ToListAsync();

                var releaseRolesToBeRemoved = releaseContributorReleaseRoles
                    .Where(urr => !userIds.Contains(urr.UserId))
                    .ToList();

                await _userReleaseRoleRepository.RemoveMany(
                    userReleaseRoles: releaseRolesToBeRemoved,
                    deletedById: _userService.GetUserId());

                var usersToBeAdded = userIds
                    .Where(userId =>
                    {
                        var userIdsWithReleaseRole = releaseContributorReleaseRoles
                            .Select(urr => urr.UserId)
                            .ToList();

                        return !userIdsWithReleaseRole.Contains(userId);
                    }).ToList();

                await _userReleaseRoleRepository.CreateManyIfNotExists(
                    userIds: usersToBeAdded,
                    releaseVersionId: releaseVersion.Id,
                    role: Contributor,
                    createdById: _userService.GetUserId());

                return Unit.Instance;
            });
    }

    public async Task<Either<ActionResult, Unit>> RemoveAllUserContributorPermissionsForPublication(
        Guid publicationId, Guid userId)
    {
        return await _persistenceHelper
            .CheckEntityExists<Publication>(publicationId,
                query =>
                    query.Include(p => p.ReleaseVersions))
            .OnSuccessDo(publication => _userService
                .CheckCanUpdateReleaseRole(publication, Contributor))
            .OnSuccessVoid(async publication =>
            {
                var user = _contentDbContext
                    .Users
                    .Single(u => u.Id == userId);

                await _userReleaseRoleRepository.RemoveAllForPublication(
                    userId: userId,
                    publication: publication,
                    role: Contributor,
                    deletedById: _userService.GetUserId());

                await _userReleaseInviteRepository.RemoveByPublication(
                    publication: publication,
                    email: user.Email,
                    role: Contributor);
            });
    }
}
