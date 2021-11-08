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

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ReleasePermissionService : IReleasePermissionService
    {
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly ContentDbContext _contentDbContext;
        private readonly IPublicationRepository _publicationRepository;
        private readonly IUserService _userService;

        public ReleasePermissionService(
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            ContentDbContext contentDbContext,
            IPublicationRepository publicationRepository,
            IUserService userService)
        {
            _persistenceHelper = persistenceHelper;
            _contentDbContext = contentDbContext;
            _publicationRepository = publicationRepository;
            _userService = userService;
        }

        public async Task<Either<ActionResult, List<ManageAccessPageContributorViewModel>>> GetManageAccessPageContributorList(Guid releaseId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId,
                    query =>
                        query.Include(r => r.Publication))
                .OnSuccessDo(release => _userService
                    .CheckCanUpdateReleaseRole(release.Publication, ReleaseRole.Contributor))
                .OnSuccess(async release =>
                {
                    var allLatestReleases = await _publicationRepository
                        .GetLatestVersionsOfAllReleases(release.PublicationId);

                    var allLatestReleaseIds = allLatestReleases
                        .Select(r => r.Id)
                        .ToList();

                    var allContributorReleaseRoles = await _contentDbContext.UserReleaseRoles
                        .Include(releaseRole => releaseRole.User)
                        .AsAsyncEnumerable()
                        .Where(releaseRole =>
                            allLatestReleaseIds.Contains(releaseRole.ReleaseId)
                            && releaseRole.Role == ReleaseRole.Contributor)
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

                            return new ManageAccessPageContributorViewModel
                            {
                                UserId = user.Id,
                                UserFullName = $"{user.FirstName} {user.LastName}",
                                ReleaseId = releaseId,
                                // Some don't have a contributor release role for this release, as they are a
                                // contributor on a different release on the same publication
                                ReleaseRoleId = roleForThisRelease?.Id,
                            };
                        }).ToList();
                });
        }
    }
}
