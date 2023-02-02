#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ReleasePermissionService : IReleasePermissionService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IUserReleaseRoleRepository _userReleaseRoleRepository;
        private readonly IUserReleaseInviteRepository _userReleaseInviteRepository;
        private readonly IUserService _userService;

        public ReleasePermissionService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IUserReleaseRoleRepository userReleaseRoleRepository,
            IUserReleaseInviteRepository userReleaseInviteRepository,
            IUserService userService)
        {
            _contentDbContext = contentDbContext;
            _persistenceHelper = persistenceHelper;
            _userReleaseRoleRepository = userReleaseRoleRepository;
            _userReleaseInviteRepository = userReleaseInviteRepository;
            _userService = userService;
        }

        public async Task<Either<ActionResult, List<UserReleaseRoleSummaryViewModel>>>
            ListReleaseRoles(Guid releaseId, ReleaseRole[]? rolesToInclude = null)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId,
                    query =>
                        query.Include(r => r.Publication))
                .OnSuccessDo(release => _userService.CheckCanViewReleaseTeamAccess(release.Publication))
                .OnSuccess(async _ =>
                {
                    var users = await _userReleaseRoleRepository
                        .ListUserReleaseRoles(releaseId, rolesToInclude);

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
            ListReleaseInvites(Guid releaseId, ReleaseRole[]? rolesToInclude = null)
        {
            var rolesToCheck = rolesToInclude ?? EnumUtil.GetEnumValuesAsArray<ReleaseRole>();
            
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId,
                    query =>
                        query.Include(r => r.Publication))
                .OnSuccessDo(release => _userService.CheckCanViewReleaseTeamAccess(release.Publication))
                .OnSuccess(async _ =>
                {
                    var invites = await _contentDbContext
                        .UserReleaseInvites
                        .Where(i =>
                            i.ReleaseId == releaseId
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
                .CheckEntityExists<Publication>(publicationId, query =>
                    query.Include(p => p.Releases))
                .OnSuccessDo(publication => _userService
                    .CheckCanUpdateReleaseRole(publication, Contributor))
                .OnSuccess(async publication =>
                {
                    var allLatestReleases = publication.ListActiveReleases();

                    var allLatestReleaseIds = allLatestReleases
                        .Select(r => r.Id)
                        .ToList();

                    var users = await _contentDbContext
                        .UserReleaseRoles
                        .Include(releaseRole => releaseRole.User)
                        .Where(userReleaseRole =>
                            allLatestReleaseIds.Contains(userReleaseRole.ReleaseId)
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
            Guid releaseId, List<Guid> userIds)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId,
                    query =>
                        query.Include(r => r.Publication))
                .OnSuccessDo(release => _userService
                    .CheckCanUpdateReleaseRole(release.Publication, Contributor))
                .OnSuccess(async release =>
                {
                    var releaseContributorReleaseRoles = await _contentDbContext.UserReleaseRoles
                        .Include(releaseRole => releaseRole.User)
                        .Where(userReleaseRole =>
                            userReleaseRole.ReleaseId == release.Id
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
                        releaseId: release.Id,
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
                        query.Include(p => p.Releases))
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
}
