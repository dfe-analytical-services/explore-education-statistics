using System;
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
    public class PreReleaseSummaryService : IPreReleaseSummaryService
    {
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IUserService _userService;

        public PreReleaseSummaryService(IPersistenceHelper<ContentDbContext> persistenceHelper,
            IUserService userService)
        {
            _persistenceHelper = persistenceHelper;
            _userService = userService;
        }

        public async Task<Either<ActionResult, PreReleaseSummaryViewModel>> GetPreReleaseSummaryViewModelAsync(
            Guid releaseId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId, queryable =>
                    queryable.Include(r => r.Publication)
                        .ThenInclude(publication => publication.Contact))
                .OnSuccess(_userService.CheckCanViewPreReleaseSummary)
                .OnSuccess(release => new PreReleaseSummaryViewModel(
                    release.Publication.Slug,
                    release.Publication.Title,
                    release.Slug,
                    release.Title,
                    release.Publication.Contact.TeamEmail,
                    release.Publication.Contact.TeamName));
        }
    }
}