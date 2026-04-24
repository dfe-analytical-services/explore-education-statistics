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
/// This service contains a <c>List</c> method retained after the Release page redesign, which is required by the
/// Public site frontend Data Catalogue page. Newer release-related services can be found in the
/// <see cref="GovUk.Education.ExploreEducationStatistics.Content.Services.Releases"/> namespace.
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
