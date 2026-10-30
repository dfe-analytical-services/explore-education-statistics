using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Predicates;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;
using GovUk.Education.ExploreEducationStatistics.Content.Services.DataSets.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.DataSets;

public class DataSetSearchableDocumentsService(ContentDbContext contentDbContext) : IDataSetSearchableDocumentsService
{
    public async Task<Either<ActionResult, DataSetSearchableDocumentDto[]>> GetReleaseDataSetSearchableDocuments(
        string publicationSlug,
        string releaseSlug,
        CancellationToken cancellationToken = default
    ) =>
        await GetPublicationBySlug(publicationSlug, cancellationToken)
            .OnSuccess(publication =>
                contentDbContext
                    .ReleaseVersions.AsNoTracking()
                    .LatestReleaseVersions(publication.Id, releaseSlug, publishedOnly: true)
                    .SingleOrNotFoundAsync(cancellationToken)
            )
            .OnSuccess(releaseVersion =>
                GetDataSetSearchableDocuments(
                    DataSetReleaseFiles().Where(rf => rf.ReleaseVersionId == releaseVersion.Id),
                    cancellationToken
                )
            );

    public async Task<Either<ActionResult, DataSetSearchableDocumentDto[]>> GetPublicationDataSetSearchableDocuments(
        string publicationSlug,
        CancellationToken cancellationToken = default
    ) =>
        await GetPublicationBySlug(publicationSlug, cancellationToken)
            .OnSuccess(publication =>
                GetDataSetSearchableDocuments(
                    DataSetReleaseFiles()
                        .HavingLatestPublishedReleaseVersions(
                            contentDbContext.ReleaseVersions.LatestReleaseVersions(publication.Id, publishedOnly: true),
                            latestOnly: false
                        ),
                    cancellationToken
                )
            );

    private async Task<Either<ActionResult, Publication>> GetPublicationBySlug(
        string publicationSlug,
        CancellationToken cancellationToken
    ) =>
        await contentDbContext
            .Publications.AsNoTracking()
            .WhereHasPublishedRelease()
            .SingleOrNotFoundAsync(p => p.Slug == publicationSlug, cancellationToken);

    private IQueryable<ReleaseFile> DataSetReleaseFiles() =>
        contentDbContext.ReleaseFiles.AsNoTracking().OfFileType(FileType.Data).HavingNoDataReplacementInProgress();

    private static async Task<DataSetSearchableDocumentDto[]> GetDataSetSearchableDocuments(
        IQueryable<ReleaseFile> releaseFiles,
        CancellationToken cancellationToken
    )
    {
        var results = await releaseFiles
            .AsSplitQuery()
            .Include(rf => rf.File)
                .ThenInclude(f => f.DataSetFileVersionGeographicLevels)
            .Include(rf => rf.ReleaseVersion)
                .ThenInclude(rv => rv.PublishingOrganisations)
            .Include(rf => rf.ReleaseVersion.Release.Publication.Theme)
            .Include(rf => rf.ReleaseVersion.Release.Publication.SupersededBy)
            .ToListAsync(cancellationToken);

        return
        [
            .. results
                .Select(DataSetSearchableDocumentDto.FromReleaseFile)
                .OrderBy(document => document.ReleaseSlug)
                .ThenBy(document => document.Title)
                .ThenBy(document => document.FileId),
        ];
    }
}
