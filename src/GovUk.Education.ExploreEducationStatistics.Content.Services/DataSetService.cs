#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.SortOrder;
using static GovUk.Education.ExploreEducationStatistics.Content.Requests.DataSetsListRequestOrderBy;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services;

public class DataSetService : IDataSetService
{
    private readonly ContentDbContext _contentDbContext;

    public DataSetService(ContentDbContext contentDbContext)
    {
        _contentDbContext = contentDbContext;
    }

    public async Task<Either<ActionResult, PaginatedListViewModel<DataSetListViewModel>>> ListDataSets(
        Guid? themeId = null,
        Guid? publicationId = null,
        Guid? releaseId = null,
        bool? latestOnly = true,
        string? searchTerm = null,
        DataSetsListRequestOrderBy? orderBy = null,
        SortOrder? sort = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        orderBy ??= searchTerm == null ? Title : Relevance;
        sort ??= orderBy is Title or Natural ? Asc : Desc;

        var query = _contentDbContext.ReleaseFiles
            .OfFileType(FileType.Data)
            .HavingNoDataReplacementInProgress()
            .HavingThemeId(themeId)
            .HavingPublicationIdOrNoSupersededPublication(publicationId)
            .HavingReleaseIdOrLatestReleases(releaseId, latestOnly)
            .JoinFreeText(_contentDbContext.ReleaseFilesFreeTextTable, rf => rf.Id, searchTerm);

        var results = await query
            .OrderBy(orderBy.Value, sort.Value)
            .Paginate(page: page, pageSize: pageSize)
            .Select(BuildResultViewModel())
            .ToListAsync(cancellationToken: cancellationToken);

        return new PaginatedListViewModel<DataSetListViewModel>(
            // TODO Remove ChangeSummaryHtmlToText once we do further work to remove all HTML at source
            await ChangeSummaryHtmlToText(results),
            totalResults: await query.CountAsync(cancellationToken: cancellationToken),
            page,
            pageSize);
    }

    private static Expression<Func<FreeTextValueResult<ReleaseFile>, DataSetListViewModel>>
        BuildResultViewModel()
    {
        return result =>
            new DataSetListViewModel
            {
                FileId = result.Value.FileId,
                Filename = result.Value.File.Filename,
                FileSize = result.Value.File.DisplaySize(),
                Title = result.Value.Name ?? "",
                Content = result.Value.Summary ?? "",
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

    private static async Task<List<DataSetListViewModel>> ChangeSummaryHtmlToText(
        IList<DataSetListViewModel> results)
    {
        return await results
            .ToAsyncEnumerable()
            .SelectAwait(async viewModel => viewModel with
            {
                Content = await HtmlToTextUtils.HtmlToText(viewModel.Content)
            })
            .ToListAsync();
    }
}

internal static class FreeTextReleaseFileValueResultQueryableExtensions
{
    internal static IOrderedQueryable<FreeTextValueResult<ReleaseFile>> OrderBy(
        this IQueryable<FreeTextValueResult<ReleaseFile>> query,
        DataSetsListRequestOrderBy orderBy,
        SortOrder sort)
    {
        var orderedQuery = orderBy switch
        {
            Natural =>
                sort == Asc
                    ? query.OrderBy(result => result.Value.Order)
                    : query.OrderByDescending(result => result.Value.Order),
            Published =>
                sort == Asc
                    ? query.OrderBy(result => result.Value.Release.Published)
                    : query.OrderByDescending(result => result.Value.Release.Published),
            Relevance =>
                sort == Asc
                    ? query.OrderBy(result => result.Rank)
                    : query.OrderByDescending(result => result.Rank),
            Title =>
                sort == Asc
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

    internal static IQueryable<ReleaseFile> HavingReleaseIdOrLatestReleases(
        this IQueryable<ReleaseFile> query,
        Guid? releaseId,
        bool? latestOnly)
    {
        if (releaseId.HasValue)
        {
            // TODO In EES-4665 we will allow filtering by a specific release version ensuring that it's the latest
            // published version of that release. Until then, if a release id is provided,
            // it must be the latest published release version by time series in a publication
            return query.Where(rf => rf.Release.Publication.LatestPublishedReleaseId == releaseId.Value);
        }

        if (!latestOnly.HasValue || latestOnly.Value)
        {
            // Restrict data set files to be for the latest published release version by time series in a publication
            return query.Where(rf => rf.Release.Publication.LatestPublishedReleaseId == rf.ReleaseId);
        }

        // Restrict data set files to be for any latest published release version in a publication
        // TODO EES-4665
        throw new NotSupportedException("Querying by non-latest data is not yet supported");
    }
}
