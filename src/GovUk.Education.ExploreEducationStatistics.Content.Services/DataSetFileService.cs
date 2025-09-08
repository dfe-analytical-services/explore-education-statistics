using System.Globalization;
using System.Linq.Expressions;
using CsvHelper;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Predicates;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.SortDirection;
using static GovUk.Education.ExploreEducationStatistics.Content.Requests.DataSetsListRequestSortBy;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseVersion;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services;

public class DataSetFileService(
    ContentDbContext contentDbContext,
    IReleaseVersionRepository releaseVersionRepository,
    IReleaseFileBlobService releaseFileBlobStorageService,
    IFootnoteRepository footnoteRepository,
    IAnalyticsManager analyticsManager,
    ILogger<DataSetFileService> logger
) : IDataSetFileService
{
    public async Task<Either<ActionResult, PaginatedListViewModel<DataSetFileSummaryViewModel>>> ListDataSetFiles(
        Guid? themeId,
        Guid? publicationId,
        Guid? releaseVersionId,
        GeographicLevel? geographicLevel,
        bool? latestOnly,
        DataSetType? dataSetType,
        string? searchTerm,
        DataSetsListRequestSortBy? sort,
        SortDirection? sortDirection,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        // If latestOnly is null default it to true except when a releaseVersionId is provided
        latestOnly ??= !releaseVersionId.HasValue;

        dataSetType ??= DataSetType.All;

        sort ??= searchTerm == null ? Title : Relevance;
        sortDirection ??= sort is Title or Natural ? Asc : Desc;

        var latestPublishedReleaseVersions = contentDbContext.ReleaseVersions.LatestReleaseVersions(
            publicationId,
            publishedOnly: true
        );

        var query = contentDbContext
            .ReleaseFiles.AsNoTracking()
            .OfFileType(FileType.Data)
            .HavingNoDataReplacementInProgress()
            .HavingThemeId(themeId)
            .HavingPublicationIdOrNoSupersededPublication(publicationId)
            .HavingReleaseVersionId(releaseVersionId)
            .HavingGeographicLevel(geographicLevel)
            .OfDataSetType(dataSetType.Value)
            .HavingLatestPublishedReleaseVersions(latestPublishedReleaseVersions, latestOnly.Value)
            .JoinFreeText(contentDbContext.ReleaseFilesFreeTextTable, rf => rf.Id, searchTerm);

        var results = await query
            .OrderBy(sort.Value, sortDirection.Value)
            .Paginate(page: page, pageSize: pageSize)
            .Select(BuildDataSetFileSummaryViewModel(includeGeographicLevels: false))
            .ToListAsync(cancellationToken: cancellationToken);

        // We cannot fetch results[x].Meta.GeographicLevels in the previous query, because of the JOIN in
        // `JoinFreeText`. That JOIN means we cannot fetch any collection, as that means we don't get a one-to-one
        // matching of search ranks with the results after filtering. So instead we fetch the geographic levels
        // in a separate query below
        var geogLvlsDict = await contentDbContext
            .Files.AsNoTracking()
            .Include(f => f.DataSetFileVersionGeographicLevels)
            .Where(file => results.Select(r => r.FileId).ToList().Contains(file.Id))
            .ToDictionaryAsync(
                file => file.Id,
                file =>
                    file.DataSetFileVersionGeographicLevels.Select(gl => gl.GeographicLevel.GetEnumLabel())
                        .Order()
                        .ToList(),
                cancellationToken: cancellationToken
            );
        foreach (var result in results)
        {
            result.Meta.GeographicLevels = geogLvlsDict[result.FileId];
        }

        return new PaginatedListViewModel<DataSetFileSummaryViewModel>(
            // TODO Remove ChangeSummaryHtmlToText once we do further work to remove all HTML at source
            await ChangeSummaryHtmlToText(results),
            totalResults: await query.CountAsync(cancellationToken: cancellationToken),
            page,
            pageSize
        );
    }

    private static Expression<
        Func<FreeTextValueResult<ReleaseFile>, DataSetFileSummaryViewModel>
    > BuildDataSetFileSummaryViewModel(bool includeGeographicLevels)
    {
        return result => new DataSetFileSummaryViewModel
        {
            Id = result.Value.File.DataSetFileId!.Value,
            FileId = result.Value.FileId,
            Filename = result.Value.File.Filename,
            FileSize = result.Value.File.DisplaySize(),
            Title = result.Value.Name ?? "",
            Content = result.Value.Summary ?? "",
            Theme = new IdTitleViewModel
            {
                Id = result.Value.ReleaseVersion.Release.Publication.ThemeId,
                Title = result.Value.ReleaseVersion.Release.Publication.Theme.Title,
            },
            Publication = new IdTitleSlugViewModel
            {
                Id = result.Value.ReleaseVersion.Release.Publication.Id,
                Title = result.Value.ReleaseVersion.Release.Publication.Title,
                Slug = result.Value.ReleaseVersion.Release.Publication.Slug,
            },
            Release = new IdTitleSlugViewModel
            {
                Id = result.Value.ReleaseVersionId,
                Title = result.Value.ReleaseVersion.Release.Title,
                Slug = result.Value.ReleaseVersion.Release.Slug,
            },
            LatestData =
                result.Value.ReleaseVersionId
                == result.Value.ReleaseVersion.Release.Publication.LatestPublishedReleaseVersionId,
            IsSuperseded =
                result.Value.ReleaseVersion.Release.Publication.SupersededBy != null
                && result.Value.ReleaseVersion.Release.Publication.SupersededBy.LatestPublishedReleaseVersionId != null,
            Published = result.Value.ReleaseVersion.Published!.Value,
            LastUpdated = result.Value.Published!.Value,
            Api = BuildDataSetFileApiViewModel(result.Value),
            Meta = BuildDataSetFileMetaViewModel(
                result.Value.File.SubjectId,
                includeGeographicLevels
                    ? result.Value.File.DataSetFileVersionGeographicLevels
                    : new List<DataSetFileVersionGeographicLevel>(),
                result.Value.File.DataSetFileMeta,
                result.Value.FilterSequence,
                result.Value.IndicatorSequence
            ),
        };
    }

    public async Task<Either<ActionResult, List<DataSetSitemapItemViewModel>>> ListSitemapItems(
        CancellationToken cancellationToken = default
    )
    {
        var latestReleaseVersions = contentDbContext.ReleaseVersions.LatestReleaseVersions(publishedOnly: true);

        var latestReleaseFiles = contentDbContext
            .ReleaseFiles.AsNoTracking()
            .OfFileType(FileType.Data)
            .HavingNoDataReplacementInProgress()
            .HavingLatestPublishedReleaseVersions(latestReleaseVersions, latestOnly: false);

        return await latestReleaseFiles
            .Select(rf => new DataSetSitemapItemViewModel
            {
                Id = rf.File.DataSetFileId!.Value.ToString(),
                LastModified = rf.Published,
            })
            .ToListAsync(cancellationToken);
    }

    private static async Task<List<DataSetFileSummaryViewModel>> ChangeSummaryHtmlToText(
        IList<DataSetFileSummaryViewModel> results
    )
    {
        return await results
            .ToAsyncEnumerable()
            .SelectAwait(async viewModel =>
                viewModel with
                {
                    Content = await HtmlToTextUtils.HtmlToText(viewModel.Content),
                }
            )
            .ToListAsync();
    }

    public async Task<Either<ActionResult, DataSetFileViewModel>> GetDataSetFile(
        Guid dataSetFileId,
        CancellationToken cancellationToken
    )
    {
        var releaseFile = await contentDbContext
            .ReleaseFiles.Include(rf => rf.ReleaseVersion.Release.Publication.Theme)
            .Include(rf => rf.ReleaseVersion.Release.Publication.SupersededBy)
            .Include(rf => rf.File.DataSetFileVersionGeographicLevels)
            .Where(rf =>
                rf.File.DataSetFileId == dataSetFileId
                && rf.ReleaseVersion.Published.HasValue
                && DateTime.UtcNow >= rf.ReleaseVersion.Published.Value
            )
            .OrderByDescending(rf => rf.ReleaseVersion.Version)
            .FirstOrDefaultAsync(cancellationToken);

        if (
            releaseFile == null
            || !await releaseVersionRepository.IsLatestPublishedReleaseVersion(
                releaseFile.ReleaseVersionId,
                cancellationToken
            )
        )
        {
            return new NotFoundResult();
        }

        var dataCsvPreviewResult = await GetDataCsvPreview(
            releaseFile: releaseFile,
            cancellationToken: cancellationToken
        );

        if (dataCsvPreviewResult.IsLeft)
        {
            return dataCsvPreviewResult.Left;
        }

        var variables = GetVariables(releaseFile.File.DataSetFileMeta!);

        var footnotes = await footnoteRepository.GetFootnotes(releaseFile.ReleaseVersionId, releaseFile.File.SubjectId);

        return new DataSetFileViewModel
        {
            Id = releaseFile.File.DataSetFileId!.Value, // we ensure this is set when fetching releaseFile
            Title = releaseFile.Name ?? string.Empty,
            Summary = releaseFile.Summary ?? string.Empty,
            Release = new DataSetFileReleaseViewModel
            {
                Id = releaseFile.ReleaseVersionId,
                Title = releaseFile.ReleaseVersion.Release.Title,
                Slug = releaseFile.ReleaseVersion.Release.Slug,
                Type = releaseFile.ReleaseVersion.Type,
                IsLatestPublishedRelease =
                    releaseFile.ReleaseVersion.Release.Publication.LatestPublishedReleaseVersionId
                    == releaseFile.ReleaseVersionId,
                IsSuperseded =
                    releaseFile.ReleaseVersion.Release.Publication.SupersededBy != null
                    && releaseFile.ReleaseVersion.Release.Publication.SupersededBy.LatestPublishedReleaseVersionId
                        != null,
                Published = releaseFile.ReleaseVersion.Published!.Value,
                LastUpdated = releaseFile.Published!.Value,
                Publication = new DataSetFilePublicationViewModel
                {
                    Id = releaseFile.ReleaseVersion.Release.Publication.Id,
                    Title = releaseFile.ReleaseVersion.Release.Publication.Title,
                    Slug = releaseFile.ReleaseVersion.Release.Publication.Slug,
                    ThemeTitle = releaseFile.ReleaseVersion.Release.Publication.Theme.Title,
                },
            },
            File = new DataSetFileFileViewModel
            {
                Id = releaseFile.FileId,
                Name = releaseFile.File.Filename,
                Size = releaseFile.File.DisplaySize(),
                Meta = BuildDataSetFileMetaViewModel(
                    releaseFile.File.SubjectId,
                    releaseFile.File.DataSetFileVersionGeographicLevels,
                    releaseFile.File.DataSetFileMeta,
                    releaseFile.FilterSequence,
                    releaseFile.IndicatorSequence
                ),
                DataCsvPreview = dataCsvPreviewResult.Right,
                Variables = variables,
                SubjectId = releaseFile.File.SubjectId!.Value,
            },
            Footnotes = FootnotesViewModelBuilder.BuildFootnotes(footnotes),
            Api = BuildDataSetFileApiViewModel(releaseFile),
        };
    }

    public async Task<ActionResult> DownloadDataSetFile(Guid dataSetFileId, CancellationToken cancellationToken)
    {
        var releaseFile = await contentDbContext
            .ReleaseFiles.Include(rf => rf.File)
            .Include(rf => rf.ReleaseVersion.Release.Publication)
            .Where(rf =>
                rf.File.DataSetFileId == dataSetFileId
                && rf.ReleaseVersion.Published.HasValue
                && DateTime.UtcNow >= rf.ReleaseVersion.Published.Value
            )
            .OrderByDescending(rf => rf.ReleaseVersion.Version)
            .FirstOrDefaultAsync(cancellationToken);

        if (
            releaseFile == null
            || !await releaseVersionRepository.IsLatestPublishedReleaseVersion(
                releaseFile.ReleaseVersionId,
                cancellationToken
            )
        )
        {
            return new NotFoundResult();
        }

        await RecordCsvDownloadAnalytics(releaseFile, cancellationToken);

        var streamResult = await releaseFileBlobStorageService.GetDownloadStream(
            releaseFile: releaseFile,
            cancellationToken: cancellationToken
        );

        if (streamResult.IsLeft)
        {
            return streamResult.Left;
        }

        return new FileStreamResult(streamResult.Right, "text/csv") { FileDownloadName = releaseFile.File.Filename };
    }

    private static DataSetFileMetaViewModel BuildDataSetFileMetaViewModel(
        Guid? subjectId,
        List<DataSetFileVersionGeographicLevel> dataSetFileVersionGeographicLevels,
        DataSetFileMeta? meta,
        List<FilterSequenceEntry>? filterSequence,
        List<IndicatorGroupSequenceEntry>? indicatorGroupSequence
    )
    {
        if (meta == null)
        {
            throw new InvalidDataException($"DataSetMeta should not be null. SubjectId: {subjectId}");
        }

        return new DataSetFileMetaViewModel
        {
            NumDataFileRows = meta.NumDataFileRows,
            GeographicLevels = dataSetFileVersionGeographicLevels
                .Select(gl => gl.GeographicLevel.GetEnumLabel())
                .ToList(),
            TimePeriodRange = new DataSetFileTimePeriodRangeViewModel
            {
                From = TimePeriodLabelFormatter.Format(
                    meta.TimePeriodRange.Start.Period,
                    meta.TimePeriodRange.Start.TimeIdentifier
                ),
                To = TimePeriodLabelFormatter.Format(
                    meta.TimePeriodRange.End.Period,
                    meta.TimePeriodRange.End.TimeIdentifier
                ),
            },
            Filters = GetOrderedFilters(meta.Filters, filterSequence),
            Indicators = GetOrderedIndicators(meta.Indicators, indicatorGroupSequence),
        };
    }

    private async Task<Either<ActionResult, DataSetFileCsvPreviewViewModel>> GetDataCsvPreview(
        ReleaseFile releaseFile,
        CancellationToken cancellationToken
    )
    {
        var streamResult = await releaseFileBlobStorageService.GetDownloadStream(
            releaseFile: releaseFile,
            cancellationToken: cancellationToken
        );

        if (streamResult.IsLeft)
        {
            return streamResult.Left;
        }

        await using var stream = streamResult.Right;
        using var streamReader = new StreamReader(stream);
        using var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture);
        await csvReader.ReadAsync();
        csvReader.ReadHeader();
        var headers = csvReader.HeaderRecord?.ToList() ?? new List<string>();

        using var csvDataReader = new CsvDataReader(csvReader);
        var rows = new List<List<string>>();
        var lastLine = false; // assume one line of data in CSV

        // Fetch first five data rows only. If there are less, fetch what you can
        for (var i = 0; i < 5 && !lastLine; i++)
        {
            var cellsPerRow = csvDataReader.FieldCount;

            var row = Enumerable.Range(0, cellsPerRow).Select(csvReader.GetField<string>).ToList();

            rows.Add(row);

            lastLine = !await csvReader.ReadAsync();
        }

        return new DataSetFileCsvPreviewViewModel { Headers = headers, Rows = rows };
    }

    private List<LabelValue> GetVariables(DataSetFileMeta meta)
    {
        var filterVariables = meta
            .Filters.Select(filter => new LabelValue(
                string.IsNullOrWhiteSpace(filter.Hint) ? filter.Label : $"{filter.Label} - {filter.Hint}",
                filter.ColumnName
            ))
            .ToList();
        var indicatorVariables = meta.Indicators.Select(indicator => new LabelValue(
            indicator.Label,
            indicator.ColumnName
        ));
        return filterVariables.Concat(indicatorVariables).OrderBy(variable => variable.Value).ToList();
    }

    private static List<string> GetOrderedFilters(
        List<FilterMeta> metaFilters,
        List<FilterSequenceEntry>? filterSequenceEntries
    )
    {
        var filterSequence = filterSequenceEntries?.Select(fs => fs.Id).ToArray();

        var filters =
            filterSequence == null
                ? metaFilters.OrderBy(f => f.Label)
                : metaFilters.OrderBy(f => Array.IndexOf(filterSequence, f.Id));

        return filters.Select(f => f.Label).ToList();
    }

    private static List<string> GetOrderedIndicators(
        List<IndicatorMeta> metaIndicators,
        List<IndicatorGroupSequenceEntry>? indicatorGroupSequenceEntries
    )
    {
        var indicatorSequence = indicatorGroupSequenceEntries?.SelectMany(seq => seq.ChildSequence).ToArray();

        var indicators =
            indicatorSequence == null
                ? metaIndicators.OrderBy(i => i.Label)
                : metaIndicators.OrderBy(i => Array.IndexOf(indicatorSequence, i.Id));

        return indicators.Select(i => i.Label).ToList();
    }

    private static DataSetFileApiViewModel? BuildDataSetFileApiViewModel(ReleaseFile releaseFile)
    {
        if (releaseFile.PublicApiDataSetId is null || releaseFile.PublicApiDataSetVersion is null)
        {
            return null;
        }

        return new DataSetFileApiViewModel
        {
            Id = releaseFile.PublicApiDataSetId.Value,
            Version = releaseFile.PublicApiDataSetVersionString!,
        };
    }

    private async Task RecordCsvDownloadAnalytics(ReleaseFile releaseFile, CancellationToken cancellationToken)
    {
        var subjectId = releaseFile.File.SubjectId;

        if (!subjectId.HasValue)
        {
            logger.LogWarning(
                "ReleaseFile does not have a SubjectId set. ReleaseId: {ReleaseVersionId} FileId: {FileId}",
                releaseFile.ReleaseVersionId,
                releaseFile.FileId
            );
            return;
        }

        try
        {
            await analyticsManager.Add(
                new CaptureCsvDownloadRequest(
                    releaseFile.ReleaseVersion.Release.Publication.Title,
                    releaseFile.ReleaseVersionId,
                    releaseFile.ReleaseVersion.Release.Title,
                    releaseFile.ReleaseVersion.Release.Label,
                    subjectId.Value,
                    releaseFile.Name ?? "No data set title"
                ),
                cancellationToken
            );
        }
        catch (Exception e)
        {
            logger.LogError(
                exception: e,
                message: "Error whilst capturing csv download analytics for subject {SubjectId}",
                subjectId
            );
        }
    }
}

internal static class FreeTextReleaseFileValueResultQueryableExtensions
{
    internal static IOrderedQueryable<FreeTextValueResult<ReleaseFile>> OrderBy(
        this IQueryable<FreeTextValueResult<ReleaseFile>> query,
        DataSetsListRequestSortBy sort,
        SortDirection sortDirection
    )
    {
        var orderedQuery = sort switch
        {
            Natural => sortDirection == Asc
                ? query.OrderBy(result => result.Value.Order)
                : query.OrderByDescending(result => result.Value.Order),
            Published => sortDirection == Asc
                ? query.OrderBy(result => result.Value.ReleaseVersion.Published)
                : query.OrderByDescending(result => result.Value.ReleaseVersion.Published),
            Relevance => sortDirection == Asc
                ? query.OrderBy(result => result.Rank)
                : query.OrderByDescending(result => result.Rank),
            Title => sortDirection == Asc
                ? query.OrderBy(result => result.Value.Name)
                : query.OrderByDescending(result => result.Value.Name),
            _ => throw new ArgumentOutOfRangeException(nameof(sort), sort, message: null),
        };

        // Then sort by id to provide a stable sort order
        return orderedQuery.ThenBy(result => result.Value.Id);
    }
}

internal static class ReleaseFileQueryableExtensions
{
    internal static IQueryable<ReleaseFile> OfFileType(this IQueryable<ReleaseFile> query, FileType fileType)
    {
        return query.Where(rf => rf.File.Type == fileType);
    }

    internal static IQueryable<ReleaseFile> HavingNoDataReplacementInProgress(this IQueryable<ReleaseFile> query)
    {
        return query.Where(rf => rf.File.ReplacingId == null);
    }

    internal static IQueryable<ReleaseFile> HavingThemeId(this IQueryable<ReleaseFile> query, Guid? themeId)
    {
        return themeId.HasValue
            ? query.Where(rf => rf.ReleaseVersion.Release.Publication.ThemeId == themeId.Value)
            : query;
    }

    internal static IQueryable<ReleaseFile> HavingPublicationIdOrNoSupersededPublication(
        this IQueryable<ReleaseFile> query,
        Guid? publicationId
    )
    {
        return publicationId.HasValue
            ? query.HavingPublicationId(publicationId)
            : query.HavingNoSupersededPublication();
    }

    private static IQueryable<ReleaseFile> HavingPublicationId(this IQueryable<ReleaseFile> query, Guid? publicationId)
    {
        return publicationId.HasValue
            ? query.Where(rf => rf.ReleaseVersion.Release.PublicationId == publicationId.Value)
            : query;
    }

    private static IQueryable<ReleaseFile> HavingNoSupersededPublication(this IQueryable<ReleaseFile> query)
    {
        return query.Where(rf =>
            rf.ReleaseVersion.Release.Publication.SupersededById == null
            || !rf.ReleaseVersion.Release.Publication.SupersededBy!.LatestPublishedReleaseVersionId.HasValue
        );
    }

    internal static IQueryable<ReleaseFile> HavingReleaseVersionId(
        this IQueryable<ReleaseFile> query,
        Guid? releaseVersionId
    )
    {
        return releaseVersionId.HasValue ? query.Where(rf => rf.ReleaseVersionId == releaseVersionId.Value) : query;
    }

    internal static IQueryable<ReleaseFile> HavingGeographicLevel(
        this IQueryable<ReleaseFile> query,
        GeographicLevel? geographicLevel
    )
    {
        return geographicLevel.HasValue
            ? query.Where(rf =>
                rf.File.DataSetFileVersionGeographicLevels.Any(gl => gl.GeographicLevel == geographicLevel)
            )
            : query;
    }

    internal static IQueryable<ReleaseFile> OfDataSetType(this IQueryable<ReleaseFile> query, DataSetType dataSetType)
    {
        return dataSetType switch
        {
            DataSetType.All => query,
            DataSetType.Api => query.Where(rf => rf.PublicApiDataSetId.HasValue),
            _ => throw new ArgumentOutOfRangeException(nameof(dataSetType)),
        };
    }

    internal static IQueryable<ReleaseFile> HavingLatestPublishedReleaseVersions(
        this IQueryable<ReleaseFile> query,
        IQueryable<ReleaseVersion> latestPublishedReleaseVersions,
        bool latestOnly
    )
    {
        // Data set files must only ever be those associated with a latest published release version.
        // The latestOnly parameter allows further restricting data set files to be from the latest published release
        // of the publication.
        if (latestOnly)
        {
            // Restrict data set files to be from the latest published release version of the publication
            return query.Where(rf =>
                rf.ReleaseVersionId == rf.ReleaseVersion.Release.Publication.LatestPublishedReleaseVersionId
            );
        }

        // Restrict data set files to be for *any* latest published release version
        return query.Join(latestPublishedReleaseVersions, rf => rf.ReleaseVersionId, rv => rv.Id, (rf, _) => rf);
    }
}
