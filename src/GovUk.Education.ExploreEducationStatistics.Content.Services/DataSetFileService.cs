#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Predicates;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.SortDirection;
using static GovUk.Education.ExploreEducationStatistics.Content.Requests.DataSetsListRequestSortBy;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseVersion;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services;

public class DataSetFileService : IDataSetFileService
{
    private readonly ContentDbContext _contentDbContext;
    private readonly StatisticsDbContext _statisticsDbContext;
    private readonly IReleaseVersionRepository _releaseVersionRepository;

    public DataSetFileService(
        ContentDbContext contentDbContext,
        StatisticsDbContext statisticsDbContext,
        IReleaseVersionRepository releaseVersionRepository)
    {
        _contentDbContext = contentDbContext;
        _statisticsDbContext = statisticsDbContext;
        _releaseVersionRepository = releaseVersionRepository;
    }

    public async Task<Either<ActionResult, PaginatedListViewModel<DataSetFileSummaryViewModel>>> ListDataSetFiles(
        Guid? themeId,
        Guid? publicationId,
        Guid? releaseVersionId,
        bool? latestOnly,
        string? searchTerm,
        DataSetsListRequestSortBy? sort,
        SortDirection? sortDirection,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        // If latestOnly is null default it to true except when a releaseVersionId is provided
        latestOnly ??= !releaseVersionId.HasValue;

        sort ??= searchTerm == null ? Title : Relevance;
        sortDirection ??= sort is Title or Natural ? Asc : Desc;

        var latestPublishedReleaseVersions =
            _contentDbContext.ReleaseVersions.LatestReleaseVersions(publicationId, publishedOnly: true);

        var query = _contentDbContext.ReleaseFiles
            .Include(rf => rf.File)
            .Include(rf => rf.ReleaseVersion.Publication.Topic.Theme)
            .OfFileType(FileType.Data)
            .HavingNoDataReplacementInProgress()
            .HavingThemeId(themeId)
            .HavingPublicationIdOrNoSupersededPublication(publicationId)
            .HavingReleaseVersionId(releaseVersionId)
            .HavingLatestPublishedReleaseVersions(latestPublishedReleaseVersions, latestOnly.Value)
            .JoinFreeText(_contentDbContext.ReleaseFilesFreeTextTable, rf => rf.Id, searchTerm);

        var results = await query
            .OrderBy(sort.Value, sortDirection.Value)
            .Paginate(page: page, pageSize: pageSize)
            .ToListAsync(cancellationToken: cancellationToken);

        var viewModels = (await results
            .SelectAsync(BuildDataSetFileSummaryViewModel)).ToList();

        return new PaginatedListViewModel<DataSetFileSummaryViewModel>(
            // TODO Remove ChangeSummaryHtmlToText once we do further work to remove all HTML at source
            await ChangeSummaryHtmlToText(viewModels),
            totalResults: await query.CountAsync(cancellationToken: cancellationToken),
            page,
            pageSize);
    }

    private async Task<DataSetFileSummaryViewModel> BuildDataSetFileSummaryViewModel(
        FreeTextValueResult<ReleaseFile> result)
    {
        var releaseFile = result.Value;
        var (filters, indicators) = await GetOrderedFiltersAndIndicators(
            result.Value.ReleaseVersionId, result.Value.File.SubjectId!.Value,
            releaseFile.File.DataSetFileMeta!.Filters, releaseFile.File.DataSetFileMeta.Indicators);

        return new DataSetFileSummaryViewModel
        {
            Id = result.Value.File.DataSetFileId!.Value, // we fetch OfFileType(FileType.Data), so this must be set
            FileId = result.Value.FileId,
            Filename = result.Value.File.Filename,
            FileSize = result.Value.File.DisplaySize(),
            Title = result.Value.Name ?? "",
            Content = result.Value.Summary ?? "",
            Theme = new IdTitleViewModel
            {
                Id = result.Value.ReleaseVersion.Publication.Topic.ThemeId,
                Title = result.Value.ReleaseVersion.Publication.Topic.Theme.Title
            },
            Publication = new IdTitleViewModel
            {
                Id = result.Value.ReleaseVersion.PublicationId,
                Title = result.Value.ReleaseVersion.Publication.Title
            },
            Release = new IdTitleViewModel
            {
                Id = result.Value.ReleaseVersionId,
                Title = result.Value.ReleaseVersion.Title
            },
            LatestData = result.Value.ReleaseVersionId ==
                         result.Value.ReleaseVersion.Publication.LatestPublishedReleaseVersionId,
            Published = result.Value.ReleaseVersion.Published!.Value,
            Meta = (releaseFile.File.DataSetFileMeta == null)
                ? throw new InvalidDataException(
                    $"DataSetFileMeta should not be null. FileId: {releaseFile.FileId}")
                : new DataSetFileMetaViewModel
                {
                    GeographicLevels  = releaseFile.File.DataSetFileMeta.GeographicLevels,
                    TimePeriod = new DataSetFileTimePeriodViewModel
                    {
                        TimeIdentifier = releaseFile.File.DataSetFileMeta.TimeIdentifier.GetEnumLabel(),
                        From = TimePeriodLabelFormatter.Format(
                            releaseFile.File.DataSetFileMeta.Years.First(),
                            releaseFile.File.DataSetFileMeta.TimeIdentifier),
                        To = TimePeriodLabelFormatter.Format(
                            releaseFile.File.DataSetFileMeta.Years.Last(),
                            releaseFile.File.DataSetFileMeta.TimeIdentifier),
                    },
                    Filters = filters,
                    Indicators = indicators,
                },
        };
    }

    private static async Task<List<DataSetFileSummaryViewModel>> ChangeSummaryHtmlToText(
        IList<DataSetFileSummaryViewModel> results)
    {
        return await results
            .ToAsyncEnumerable()
            .SelectAwait(async viewModel => viewModel with
            {
                Content = await HtmlToTextUtils.HtmlToText(viewModel.Content)
            })
            .ToListAsync();
    }

    // ReSharper disable EntityFramework.NPlusOne.IncompleteDataQuery
    // ReSharper disable EntityFramework.NPlusOne.IncompleteDataUsage
    public async Task<Either<ActionResult, DataSetFileViewModel>> GetDataSetFile(
        Guid dataSetId)
    {
        var releaseFile = await _contentDbContext.ReleaseFiles
            .Include(rf => rf.ReleaseVersion.Publication.Topic.Theme)
            .Include(rf => rf.File)
            .Where(rf =>
                rf.File.DataSetFileId == dataSetId
                && rf.ReleaseVersion.Published.HasValue
                && DateTime.UtcNow >= rf.ReleaseVersion.Published.Value)
            .OrderByDescending(rf => rf.ReleaseVersion.Version)
            .FirstOrDefaultAsync();

        if (releaseFile == null
            || !await _releaseVersionRepository.IsLatestPublishedReleaseVersion(
                releaseFile.ReleaseVersionId))
        {
            return new NotFoundResult();
        }

        var (filters, indicators) = await GetOrderedFiltersAndIndicators(
            releaseFile.ReleaseVersionId, releaseFile.File.SubjectId!.Value,
            releaseFile.File.DataSetFileMeta!.Filters, releaseFile.File.DataSetFileMeta.Indicators);

        return new DataSetFileViewModel
        {
            Id = releaseFile.File.DataSetFileId!.Value, // we ensure this is set when fetching releaseFile
            Title = releaseFile.Name ?? string.Empty,
            Summary = releaseFile.Summary ?? string.Empty,
            Release = new DataSetFileReleaseViewModel
            {
                Id = releaseFile.ReleaseVersionId,
                Title = releaseFile.ReleaseVersion.Title,
                Slug = releaseFile.ReleaseVersion.Slug,
                Type = releaseFile.ReleaseVersion.Type,
                IsLatestPublishedRelease =
                    releaseFile.ReleaseVersion.Publication.LatestPublishedReleaseVersionId ==
                    releaseFile.ReleaseVersionId,
                Published = releaseFile.ReleaseVersion.Published!.Value,
                Publication = new DataSetFilePublicationViewModel
                {
                    Id = releaseFile.ReleaseVersion.PublicationId,
                    Title = releaseFile.ReleaseVersion.Publication.Title,
                    Slug = releaseFile.ReleaseVersion.Publication.Slug,
                    ThemeTitle = releaseFile.ReleaseVersion.Publication.Topic.Theme.Title,
                },
            },
            File = new DataSetFileFileViewModel
            {
                Id = releaseFile.FileId,
                Name = releaseFile.File.Filename,
                Size = releaseFile.File.DisplaySize(),
            },
            Meta = (releaseFile.File.DataSetFileMeta == null)
                ? new DataSetFileMetaViewModel()
                : new DataSetFileMetaViewModel
                {
                    GeographicLevels  = releaseFile.File.DataSetFileMeta.GeographicLevels,
                    TimePeriod = new DataSetFileTimePeriodViewModel
                    {
                        TimeIdentifier = releaseFile.File.DataSetFileMeta.TimeIdentifier.GetEnumLabel(),
                        From = TimePeriodLabelFormatter.Format(
                            releaseFile.File.DataSetFileMeta.Years.First(),
                            releaseFile.File.DataSetFileMeta.TimeIdentifier),
                        To = TimePeriodLabelFormatter.Format(
                            releaseFile.File.DataSetFileMeta.Years.Last(),
                            releaseFile.File.DataSetFileMeta.TimeIdentifier),
                    },
                    Filters = filters,
                    Indicators = indicators,
                },
        };
    }

    private async Task<Tuple<List<string>, List<string>>> GetOrderedFiltersAndIndicators(
        Guid releaseVersionId, Guid subjectId,
        List<FilterMeta> metaFilters, List<IndicatorMeta> metaIndicators)
    {
        var releaseSubject = await _statisticsDbContext.ReleaseSubject
            .SingleAsync(rs => rs.ReleaseVersionId == releaseVersionId
                          && rs.SubjectId == subjectId);

        var filterSequence = releaseSubject.FilterSequence?
            .Select(fs => fs.Id)
            .ToArray();
        var filters = filterSequence == null
            ? metaFilters.OrderBy(f => f.Label).ToList()
            : metaFilters
                .OrderBy(f => Array.IndexOf(filterSequence, f.Id)).ToList();

        var indicatorSequence = releaseSubject.IndicatorSequence?
            .SelectMany(seq => seq.ChildSequence)
            .ToArray();
        var indicators = indicatorSequence == null
            ? metaIndicators.OrderBy(i => i.Label).ToList()
            : metaIndicators
                .OrderBy(i => Array.IndexOf(indicatorSequence, i.Id)).ToList();

        return new (
            filters.Select(f => f.Label).ToList(),
            indicators.Select(i => i.Label).ToList());
    }
}

internal static class FreeTextReleaseFileValueResultQueryableExtensions
{
    internal static IOrderedQueryable<FreeTextValueResult<ReleaseFile>> OrderBy(
        this IQueryable<FreeTextValueResult<ReleaseFile>> query,
        DataSetsListRequestSortBy sort,
        SortDirection sortDirection)
    {
        var orderedQuery = sort switch
        {
            Natural =>
                sortDirection == Asc
                    ? query.OrderBy(result => result.Value.Order)
                    : query.OrderByDescending(result => result.Value.Order),
            Published =>
                sortDirection == Asc
                    ? query.OrderBy(result => result.Value.ReleaseVersion.Published)
                    : query.OrderByDescending(result => result.Value.ReleaseVersion.Published),
            Relevance =>
                sortDirection == Asc
                    ? query.OrderBy(result => result.Rank)
                    : query.OrderByDescending(result => result.Rank),
            Title =>
                sortDirection == Asc
                    ? query.OrderBy(result => result.Value.Name)
                    : query.OrderByDescending(result => result.Value.Name),
            _ => throw new ArgumentOutOfRangeException(nameof(sort), sort, message: null)
        };

        // Then sort by id to provide a stable sort order
        return orderedQuery.ThenBy(result => result.Value.Id);
    }
}

internal static class ReleaseFileQueryableExtensions
{
    internal static IQueryable<ReleaseFile> OfFileType(
        this IQueryable<ReleaseFile> query,
        FileType fileType)
    {
        return query.Where(rf => rf.File.Type == fileType);
    }

    internal static IQueryable<ReleaseFile> HavingNoDataReplacementInProgress(
        this IQueryable<ReleaseFile> query)
    {
        return query.Where(rf => rf.File.ReplacingId == null);
    }

    internal static IQueryable<ReleaseFile> HavingThemeId(
        this IQueryable<ReleaseFile> query,
        Guid? themeId)
    {
        return themeId.HasValue
            ? query.Where(rf => rf.ReleaseVersion.Publication.Topic.ThemeId == themeId.Value)
            : query;
    }

    internal static IQueryable<ReleaseFile> HavingPublicationIdOrNoSupersededPublication(
        this IQueryable<ReleaseFile> query,
        Guid? publicationId)
    {
        return publicationId.HasValue
            ? query.HavingPublicationId(publicationId)
            : query.HavingNoSupersededPublication();
    }

    private static IQueryable<ReleaseFile> HavingPublicationId(
        this IQueryable<ReleaseFile> query,
        Guid? publicationId)
    {
        return publicationId.HasValue
            ? query.Where(rf => rf.ReleaseVersion.PublicationId == publicationId.Value)
            : query;
    }

    private static IQueryable<ReleaseFile> HavingNoSupersededPublication(
        this IQueryable<ReleaseFile> query)
    {
        return query.Where(rf => rf.ReleaseVersion.Publication.SupersededById == null ||
                                 !rf.ReleaseVersion.Publication.SupersededBy!.LatestPublishedReleaseVersionId.HasValue);
    }

    internal static IQueryable<ReleaseFile> HavingReleaseVersionId(
        this IQueryable<ReleaseFile> query,
        Guid? releaseVersionId)
    {
        return releaseVersionId.HasValue ? query.Where(rf => rf.ReleaseVersionId == releaseVersionId.Value) : query;
    }

    internal static IQueryable<ReleaseFile> HavingLatestPublishedReleaseVersions(
        this IQueryable<ReleaseFile> query,
        IQueryable<ReleaseVersion> latestPublishedReleaseVersions,
        bool latestOnly)
    {
        // Data set files must only ever be those associated with a latest published release version.
        // The latestOnly parameter allows further restricting data set files to be from the latest published release
        // of the publication.
        if (latestOnly)
        {
            // Restrict data set files to be from the latest published release version of the publication
            return query.Where(rf =>
                rf.ReleaseVersionId == rf.ReleaseVersion.Publication.LatestPublishedReleaseVersionId);
        }

        // Restrict data set files to be for *any* latest published release version
        return query.Join(latestPublishedReleaseVersions,
            rf => rf.ReleaseVersionId,
            rv => rv.Id,
            (rf, _) => rf);
    }
}
