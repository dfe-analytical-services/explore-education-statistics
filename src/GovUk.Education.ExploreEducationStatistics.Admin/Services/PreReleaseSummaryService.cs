#nullable enable
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

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

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
        Guid releaseVersionId)
    {
        return await _persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId, queryable =>
                queryable.Include(releaseVersion => releaseVersion.Publication)
                    .ThenInclude(publication => publication.Contact))
            .OnSuccess(_userService.CheckCanViewPreReleaseSummary)
            .OnSuccess(releaseVersion => new PreReleaseSummaryViewModel(
                releaseVersion.Publication.Slug,
                releaseVersion.Publication.Title,
                releaseVersion.Slug,
                releaseVersion.Title,
                releaseVersion.Publication.Contact.TeamEmail,
                releaseVersion.Publication.Contact.TeamName));
    }
}
