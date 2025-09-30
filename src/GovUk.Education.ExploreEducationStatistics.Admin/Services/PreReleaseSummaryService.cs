#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class PreReleaseSummaryService(ContentDbContext contentDbContext, IUserService userService)
    : IPreReleaseSummaryService
{
    public async Task<
        Either<ActionResult, PreReleaseSummaryViewModel>
    > GetPreReleaseSummaryViewModel(Guid releaseVersionId, CancellationToken cancellationToken)
    {
        return await GetReleaseVersion(releaseVersionId, cancellationToken)
            .OnSuccess(userService.CheckCanViewPreReleaseSummary)
            .OnSuccess(releaseVersion => new PreReleaseSummaryViewModel
            {
                PublicationSlug = releaseVersion.Release.Publication.Slug,
                PublicationTitle = releaseVersion.Release.Publication.Title,
                ReleaseSlug = releaseVersion.Release.Slug,
                ReleaseTitle = releaseVersion.Release.Title,
                ContactEmail = releaseVersion.Publication.Contact.TeamEmail,
                ContactTeam = releaseVersion.Publication.Contact.TeamName,
            });
    }

    private async Task<Either<ActionResult, ReleaseVersion>> GetReleaseVersion(
        Guid releaseVersionId,
        CancellationToken cancellationToken
    )
    {
        return await contentDbContext
            .ReleaseVersions.Include(rv => rv.Release)
            .ThenInclude(r => r.Publication)
            .ThenInclude(p => p.Contact)
            .SingleOrNotFoundAsync(
                rv => rv.Id == releaseVersionId,
                cancellationToken: cancellationToken
            );
    }
}
