using System.Text.RegularExpressions;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Predicates;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Releases;

public class ReleaseContentService(ContentDbContext contentDbContext) : IReleaseContentService
{
    private static readonly Regex CommentsRegex =
        new(ContentFilterUtils.CommentsFilterPattern, RegexOptions.Compiled);

    public async Task<Either<ActionResult, ReleaseContentDto>> GetReleaseContent(
        string publicationSlug,
        string releaseSlug,
        CancellationToken cancellationToken = default) =>
        await GetPublicationBySlug(publicationSlug, cancellationToken)
            .OnSuccess(publication => GetLatestPublishedReleaseVersionWithContentByReleaseSlug(
                publication,
                releaseSlug,
                cancellationToken))
            .OnSuccess(releaseVersion =>
            {
                var releaseContentDto = ReleaseContentDto.FromReleaseVersion(releaseVersion);

                // Filter content blocks to remove any comments
                FilterContentSection(releaseContentDto.HeadlinesSection);
                FilterContentSection(releaseContentDto.SummarySection);
                FilterContentSection(releaseContentDto.KeyStatisticsSecondarySection);

                foreach (var section in releaseContentDto.Content)
                {
                    FilterContentSection(section);
                }

                return releaseContentDto;
            });

    private Task<Either<ActionResult, Publication>> GetPublicationBySlug(
        string publicationSlug,
        CancellationToken cancellationToken) =>
        contentDbContext.Publications
            .AsNoTracking()
            .WhereHasPublishedRelease()
            .SingleOrNotFoundAsync(p => p.Slug == publicationSlug, cancellationToken);

    private Task<Either<ActionResult, ReleaseVersion>> GetLatestPublishedReleaseVersionWithContentByReleaseSlug(
        Publication publication,
        string releaseSlug,
        CancellationToken cancellationToken) =>
        contentDbContext.ReleaseVersions
            .AsNoTracking()
            .Include(rv => rv.Content)
            .ThenInclude(cs => cs.Content)
            .ThenInclude(cb => (cb as EmbedBlockLink)!.EmbedBlock)
            .Include(rv => rv.KeyStatistics)
            .LatestReleaseVersions(publication.Id, releaseSlug, publishedOnly: true)
            .SingleOrNotFoundAsync(cancellationToken);

    private static void FilterContentSection(ContentSectionDto section)
    {
        foreach (var contentBlock in section.Content)
        {
            if (contentBlock is HtmlBlockBaseDto htmlBlock)
            {
                htmlBlock.Body = CommentsRegex.Replace(htmlBlock.Body, string.Empty);
            }
        }
    }
}
