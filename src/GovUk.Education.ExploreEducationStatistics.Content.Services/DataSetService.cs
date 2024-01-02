#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.SortOrder;
using static GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.IDataSetService;
using static
    GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.IDataSetService.DataSetServiceOrderBy;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services;

public class DataSetService : IDataSetService
{
    private readonly ContentDbContext _contentDbContext;

    public DataSetService(ContentDbContext contentDbContext)
    {
        _contentDbContext = contentDbContext;
    }

    public async Task<Either<ActionResult, PaginatedListViewModel<DataSetSearchResultViewModel>>> ListDataSets(
        Guid? themeId = null,
        Guid? publicationId = null,
        Guid? releaseId = null,
        string? searchTerm = null,
        DataSetServiceOrderBy? orderBy = null,
        SortOrder? sortOrder = null,
        int page = 1,
        int pageSize = 10)
    {
        // TODO EES-4669 sort the below out according to the rules
        orderBy ??= searchTerm == null ? Title : Relevance;
        sortOrder ??= orderBy is Title or Natural ? Asc : Desc;

        var query = _contentDbContext.ReleaseFiles
            .OfFileType(FileType.Data)
            .HavingNoDataReplacementInProgress()
            .HavingThemeId(themeId)
            .HavingPublicationIdOrNoSupersededPublication(publicationId)
            .HavingReleaseId(releaseId)
            .JoinFreeText(_contentDbContext.ReleaseFilesFreeTextTable, rf => rf.Id, searchTerm);

        var results = await query
            .Sort(orderBy.Value, sortOrder.Value)
            .Paginate(page, pageSize)
            .Select(BuildResultViewModel())
            .ToListAsync();

        return new PaginatedListViewModel<DataSetSearchResultViewModel>(
            // TODO Remove ChangeSummaryHtmlToText once we do further work to remove all HTML at source
            await ChangeSummaryHtmlToText(results),
            totalResults: await query.CountAsync(),
            page,
            pageSize);
    }

    private static Expression<Func<FreeTextValueResult<ReleaseFile>, DataSetSearchResultViewModel>>
        BuildResultViewModel()
    {
        return result =>
            new DataSetSearchResultViewModel
            {
                FileId = result.Value.FileId,
                Filename = result.Value.File.Filename,
                FileSize = result.Value.File.DisplaySize(),
                Title = result.Value.Name ?? "",
                Summary = result.Value.Summary ?? "",
                Theme = new IdTitleViewModel
                {
                    Id = result.Value.Release.Publication.Topic.ThemeId,
                    Title = result.Value.Release.Publication.Topic.Theme.Title
                },
                Publication = new IdTitleViewModel
                {
                    Id = result.Value.Release.PublicationId,
                    Title = result.Value.Release.Publication.Title
                },
                Release = new IdTitleViewModel
                {
                    Id = result.Value.ReleaseId,
                    Title = result.Value.Release.Title
                },
                LatestData = result.Value.ReleaseId == result.Value.Release.Publication.LatestPublishedReleaseId,
                Published = result.Value.Release.Published!.Value
            };
    }

    private static async Task<List<DataSetSearchResultViewModel>> ChangeSummaryHtmlToText(
        IList<DataSetSearchResultViewModel> results)
    {
        return await results
            .ToAsyncEnumerable()
            .SelectAwait(async viewModel => viewModel with
            {
                Summary = await HtmlToTextUtils.HtmlToText(viewModel.Summary)
            })
            .ToListAsync();
    }
}

internal static class FreeTextReleaseFileValueResultQueryableExtensions
{
    internal static IOrderedQueryable<FreeTextValueResult<ReleaseFile>> Sort(
        this IQueryable<FreeTextValueResult<ReleaseFile>> query,
        DataSetServiceOrderBy orderBy,
        SortOrder sortOrder)
    {
        var orderedQuery = orderBy switch
        {
            Natural =>
                sortOrder == Asc
                    ? query.OrderBy(result => result.Value.Release.Published)
                    : query.OrderByDescending(result => result.Value.Release.Published),
            Published =>
                sortOrder == Asc
                    ? query.OrderBy(result => result.Value.Release.Published)
                    : query.OrderByDescending(result => result.Value.Release.Published),
            Relevance =>
                sortOrder == Asc
                    ? query.OrderBy(result => result.Rank)
                    : query.OrderByDescending(result => result.Rank),
            Title =>
                sortOrder == Asc
                    ? query.OrderBy(result => result.Value.Name)
                    : query.OrderByDescending(result => result.Value.Name),
            _ => throw new ArgumentOutOfRangeException(nameof(orderBy), orderBy, message: null)
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
        return themeId.HasValue ? query.Where(rf => rf.Release.Publication.Topic.ThemeId == themeId.Value) : query;
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
        return publicationId.HasValue ? query.Where(rf => rf.Release.PublicationId == publicationId.Value) : query;
    }

    private static IQueryable<ReleaseFile> HavingNoSupersededPublication(
        this IQueryable<ReleaseFile> query)
    {
        return query.Where(rf => rf.Release.Publication.SupersededById == null ||
                                 !rf.Release.Publication.SupersededBy!.LatestPublishedReleaseId.HasValue);
    }

    internal static IQueryable<ReleaseFile> HavingReleaseId(
        this IQueryable<ReleaseFile> query,
        Guid? releaseId)
    {
        // Force data sets to be for the latest published release version by time series in a publication
        // This will change when we do further work to allow the following filter options:
        // - Filter by a specific latest published release version in a publication
        // - Filter by any latest published release version in a publication
        // - Filter by the latest published release version by time series in a publication
        if (releaseId.HasValue)
        {
            return query;
        }
        else
        {
            // Restrict data sets to be for the latest published release version by time series in a publication
            return query.Where(rf => rf.Release.Publication.LatestPublishedReleaseId == rf.ReleaseId);

            // TODO add an option to allow filtering by any latest published release version in a publication
        }
    }
}
