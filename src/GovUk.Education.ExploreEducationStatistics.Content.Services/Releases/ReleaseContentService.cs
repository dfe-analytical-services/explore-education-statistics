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
    public async Task<Either<ActionResult, ReleaseContentDto>> GetReleaseContent(
        string publicationSlug,
        string releaseSlug,
        CancellationToken cancellationToken = default
    ) =>
        await GetPublicationBySlug(publicationSlug, cancellationToken)
            .OnSuccess(publication =>
                GetLatestPublishedReleaseVersionWithContentByReleaseSlug(publication, releaseSlug, cancellationToken)
            )
            .OnSuccess(releaseVersion =>
            {
                var releaseContentDto = ReleaseContentDto.FromReleaseVersion(releaseVersion);
                RemoveCommentsFromContent(releaseContentDto);
                return releaseContentDto;
            });

    private Task<Either<ActionResult, Publication>> GetPublicationBySlug(
        string publicationSlug,
        CancellationToken cancellationToken
    ) =>
        contentDbContext
            .Publications.AsNoTracking()
            .WhereHasPublishedRelease()
            .SingleOrNotFoundAsync(p => p.Slug == publicationSlug, cancellationToken);

    private Task<Either<ActionResult, ReleaseVersion>> GetLatestPublishedReleaseVersionWithContentByReleaseSlug(
        Publication publication,
        string releaseSlug,
        CancellationToken cancellationToken
    ) =>
        contentDbContext
            .ReleaseVersions.AsNoTracking()
            .Include(rv => rv.Content)
                .ThenInclude(cs => cs.Content)
            .Include(rv => rv.KeyStatistics)
            .LatestReleaseVersions(publication.Id, releaseSlug, publishedOnly: true)
            .SingleOrNotFoundAsync(cancellationToken);

    private static void RemoveCommentsFromContent(ReleaseContentDto content) =>
        content.GetAllSections().ForEach(RemoveCommentsFromContentSection);

    private static void RemoveCommentsFromContentSection(ContentSectionDto section) =>
        section.Content.OfType<HtmlBlockDto>().ForEach(RemoveCommentsFromHtmlBlock);

    private static void RemoveCommentsFromHtmlBlock(HtmlBlockDto htmlBlock) =>
        htmlBlock.Body = ContentFilterUtils.CommentsRegex().Replace(htmlBlock.Body, string.Empty);
}
