using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Security.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services;

/// <summary>
/// TODO EES-6432 EES-6433: This service is due to be removed by EES-6432/EES-6433 once the release page redesign is live.
/// </summary>
public class ReleaseService(
    ContentDbContext contentDbContext,
    IReleaseVersionRepository releaseVersionRepository,
    IUserService userService
) : IReleaseService
{
    public async Task<Either<ActionResult, List<ReleaseSummaryViewModel>>> List(string publicationSlug) =>
        await contentDbContext
            .Publications.SingleOrNotFoundAsync(p => p.Slug == publicationSlug)
            .OnSuccess(userService.CheckCanViewPublication)
            .OnSuccess(async publication =>
            {
                var publishedReleaseVersions = await releaseVersionRepository.ListLatestReleaseVersions(
                    publication.Id,
                    publishedOnly: true
                );

                return await publishedReleaseVersions
                    .ToAsyncEnumerable()
                    .SelectAwait(async releaseVersion =>
                    {
                        await contentDbContext
                            .ReleaseVersions.Entry(releaseVersion)
                            .Reference(rv => rv.Release)
                            .LoadAsync();

                        return new ReleaseSummaryViewModel(
                            releaseVersion,
                            latestPublishedRelease: releaseVersion.Id == publication.LatestPublishedReleaseVersionId
                        );
                    })
                    .ToListAsync();
            });
}
