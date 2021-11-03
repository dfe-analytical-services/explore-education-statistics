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
        private readonly IUserService _userService;

        public ReleasePermissionService(
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            ContentDbContext contentDbContext,
            IUserService userService)
        {
            _persistenceHelper = persistenceHelper;
            _contentDbContext = contentDbContext;
            _userService = userService;
        }

        public async Task<Either<ActionResult, List<ReleaseContributorViewModel>>> GetReleaseContributors(Guid releaseId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId,
                    query =>
                        query.Include(r => r.Publication))
                .OnSuccessDo(release => _userService.CheckCanUpdatePublication(release.Publication)) // @MarkFix
                .OnSuccess(async release =>
                {
                    var allReleases = await _contentDbContext.Releases
                        .Include(r => r.Publication)
                        .AsAsyncEnumerable()
                        .Where(r => r.PublicationId == release.PublicationId)
                        .ToListAsync();

                    var allLatestReleaseIds = allReleases
                        .Where(r => r.Publication.IsLatestVersionOfRelease(r.Id))
                        .Select(r => r.Id)
                        .ToList();

                    var allContributorReleaseRoles = await _contentDbContext.UserReleaseRoles
                        .Include(usr => usr.User)
                        .AsAsyncEnumerable()
                        .Where(rr =>
                            allLatestReleaseIds.Contains(rr.ReleaseId)
                            && rr.Role == ReleaseRole.Contributor)
                        .ToListAsync();

                    var allPublicationContributors = allContributorReleaseRoles
                        .Select(rr => rr.User)
                        .Distinct()
                        .ToList();

                    return allPublicationContributors
                        .Select(user =>
                        {
                            var roleForThisRelease = allContributorReleaseRoles.SingleOrDefault(rr =>
                                rr.UserId == user.Id
                                && rr.ReleaseId == releaseId);

                            return new ReleaseContributorViewModel
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
