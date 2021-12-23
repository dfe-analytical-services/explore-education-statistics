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

        public async Task<Either<ActionResult, List<ContributorViewModel>>>
            ListReleaseContributors(Guid releaseId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId,
                    query =>
                        query.Include(r => r.Publication).
                            ThenInclude(p => p.Releases))
                .OnSuccessDo(release => _userService
                    .CheckCanUpdateReleaseRole(release.Publication, Contributor))
                .OnSuccess(async release =>
                {
                    var allContributorsForRelease = await _contentDbContext.UserReleaseRoles
                        .Include(urr => urr.User)
                        .Where(urr =>
                            urr.ReleaseId == release.Id
                            && urr.Role == Contributor)
                        .ToListAsync();

                    var contributorList = allContributorsForRelease
                        .Select(urr => new ContributorViewModel
                        {
                            UserId = urr.UserId,
                            UserDisplayName = urr.User.DisplayName,
                            UserEmail = urr.User.Email,
                        })
                        .OrderBy(model => model.UserDisplayName)
                        .ToList();

                    return contributorList;
                });
        }

        public async Task<Either<ActionResult, List<ContributorViewModel>>>
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

                    var users = await _contentDbContext.UserReleaseRoles
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
                            new ContributorViewModel
                            {
                                UserId = user.Id,
                                UserDisplayName = user.DisplayName,
                                UserEmail = user.Email,
                            }
                        )
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
                        .AsQueryable()
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
