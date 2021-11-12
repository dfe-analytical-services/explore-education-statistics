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
        private readonly IPublicationRepository _publicationRepository;
        private readonly IUserService _userService;

        public ReleasePermissionService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IPublicationRepository publicationRepository,
            IUserService userService)
        {
            _contentDbContext = contentDbContext;
            _persistenceHelper = persistenceHelper;
            _publicationRepository = publicationRepository;
            _userService = userService;
        }

        public async Task<Either<ActionResult, ManageAccessPageViewModel>>
            GetManageAccessPageContributorList(Guid publicationId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Publication>(publicationId,
                    query =>
                        query.Include(r => r.Releases))
                .OnSuccessDo(publication => _userService
                    .CheckCanUpdateReleaseRole(publication, Contributor))
                .OnSuccess(async publication =>
                {
                    var allLatestReleases = await _publicationRepository
                        .GetLatestVersionsOfAllReleases(publicationId);

                    var allLatestReleasesOrdered = allLatestReleases
                        .OrderBy(r => r.Year)
                        .ThenBy(r => r.TimePeriodCoverage)
                        .ToList();

                    var allLatestReleaseIds = allLatestReleasesOrdered
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

                    var releases = allLatestReleasesOrdered
                        .Select(release =>
                        {
                            var contributorList = allPublicationContributors
                                .Select(user =>
                                {
                                    var roleForThisRelease = allContributorReleaseRoles.SingleOrDefault(releaseRole =>
                                        releaseRole.UserId == user.Id
                                        && releaseRole.ReleaseId == release.Id);

                                    return new ManageAccessPageContributorViewModel
                                    {
                                        UserId = user.Id,
                                        UserFullName = user.DisplayName,
                                        ReleaseId = release.Id,
                                        // Some don't have a contributor release role for this release, as they are a
                                        // contributor on a different release on the same publication
                                        ReleaseRoleId = roleForThisRelease?.Id,
                                    };
                                }).ToList();

                            return new ManageAccessPageReleaseViewModel
                            {
                                ReleaseId = release.Id,
                                ReleaseTitle = release.Title,
                                UserList = contributorList,
                            };
                        }).ToList();

                    return new ManageAccessPageViewModel
                    {
                        PublicationId = publication.Id,
                        PublicationTitle = publication.Title,
                        Releases = releases,
                    };
                });
        }
    }
}
