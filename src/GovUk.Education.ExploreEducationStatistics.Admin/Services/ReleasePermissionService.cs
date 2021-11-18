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
        private readonly IUserService _userService;

        public ReleasePermissionService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IUserReleaseRoleRepository userReleaseRoleRepository,
            IUserService userService)
        {
            _contentDbContext = contentDbContext;
            _persistenceHelper = persistenceHelper;
            _userReleaseRoleRepository = userReleaseRoleRepository;
            _userService = userService;
        }

        public async Task<Either<ActionResult, List<ContributorViewModel>>>
            GetReleaseContributorPermissions(Guid publicationId, Guid releaseId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Publication>(publicationId,
                    query =>
                        query.Include(r => r.Releases))
                .OnSuccessDo(publication => _userService
                    .CheckCanUpdateReleaseRole(publication, Contributor))
                .OnSuccess(async publication =>
                {
                    var release = publication.Releases.SingleOrDefault(r => r.Id == releaseId);

                    var allContributorsForRelease = await _contentDbContext.UserReleaseRoles
                        .Include(urr => urr.User)
                        .ToAsyncEnumerable()
                        .Where(urr =>
                            urr.ReleaseId == release?.Id
                            && urr.Role == Contributor)
                        .ToListAsync();

                    return allContributorsForRelease
                        .Select(urr => new ContributorViewModel
                        {
                            ReleaseRoleId = urr.Id,
                            UserId = urr.UserId,
                            UserFullName = urr.User.DisplayName,
                            UserEmail = urr.User.Email,
                        })
                        .OrderBy(model => model.UserFullName)
                        .ToList();
                });
        }

        public async Task<Either<ActionResult, List<ContributorViewModel>>>
            GetPublicationContributorList(Guid releaseId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId,
                    query =>
                        query.Include(r => r.Publication.Releases))
                .OnSuccessDo(release => _userService
                    .CheckCanUpdateReleaseRole(release.Publication, Contributor))
                .OnSuccess(async release =>
                {
                    var allLatestReleases = release.Publication.GetLatestVersionsOfAllReleases();

                    var allLatestReleaseIds = allLatestReleases
                        .Select(r => r.Id)
                        .ToList();

                    var allContributorReleaseRoles = await _contentDbContext.UserReleaseRoles
                        .Include(releaseRole => releaseRole.User)
                        .AsAsyncEnumerable()
                        .Where(userReleaseRole =>
                            allLatestReleaseIds.Contains(userReleaseRole.ReleaseId)
                            && userReleaseRole.Role == Contributor)
                        .ToListAsync();

                    var allPublicationContributors = allContributorReleaseRoles
                        .Select(releaseRole => releaseRole.User)
                        .Distinct()
                        .ToList();

                    return allPublicationContributors
                        .Select(user =>
                        {
                            var roleForThisRelease = allContributorReleaseRoles.SingleOrDefault(releaseRole =>
                                releaseRole.UserId == user.Id
                                && releaseRole.ReleaseId == releaseId);

                            return new ContributorViewModel
                            {
                                UserId = user.Id,
                                UserFullName = user.DisplayName,
                                UserEmail = user.Email,
                                ReleaseRoleId = roleForThisRelease?.Id,
                            };
                        })
                        .OrderBy(model => model.UserFullName)
                        .ToList();
                });
        }

        public async Task<Either<ActionResult, Unit>> UpdateReleaseContributors(
            Guid releaseId, List<Guid> userIds)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId,
                    query =>
                        query.Include(r => r.Publication.Releases))
                .OnSuccessDo(release => _userService
                    .CheckCanUpdateReleaseRole(release.Publication, Contributor))
                .OnSuccess(async release =>
                {
                    var releaseContributorReleaseRoles = await _contentDbContext.UserReleaseRoles
                        .Include(releaseRole => releaseRole.User)
                        .AsAsyncEnumerable()
                        .Where(userReleaseRole =>
                            userReleaseRole.ReleaseId == release.Id
                            && userReleaseRole.Role == Contributor)
                        .ToListAsync();

                    var releaseRolesToBeRemoved = releaseContributorReleaseRoles
                        .Where(urr => !userIds.Contains(urr.UserId))
                        .ToList();

                    await _userReleaseRoleRepository.RemoveAll(releaseRolesToBeRemoved, _userService.GetUserId());

                    var usersToBeAdded = userIds
                        .Where(userId =>
                        {
                            var userIdsWithReleaseRole = releaseContributorReleaseRoles
                                .Select(urr => urr.UserId)
                                .ToList();

                            return !userIdsWithReleaseRole.Contains(userId);
                        }).ToList();

                    await _userReleaseRoleRepository.CreateAll(usersToBeAdded, release.Id, Contributor,
                        _userService.GetUserId());

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
                    await _userReleaseRoleRepository.RemoveAllUserReleaseRolesForPublication(
                        userId, publication, Contributor, _userService.GetUserId());
                });
        }
    }
}
