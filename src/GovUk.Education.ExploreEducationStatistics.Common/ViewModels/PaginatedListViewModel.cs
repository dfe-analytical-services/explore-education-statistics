#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Common.ViewModels;

public record PaginatedListViewModel<T>
{
    public List<T> Results { get; }

    public PagingViewModel Paging { get; }

    public PaginatedListViewModel(List<T> results, int totalResults, int page, int pageSize)
    {
        Results = results;
        Paging = new PagingViewModel(page: page, pageSize: pageSize, totalResults: totalResults);
    }

    /// <summary>
    /// Paginate some results (in memory) that have not been paginated yet.
    /// </summary>
    /// <remarks>
    /// Construct <see cref="PaginatedListViewModel{T}"/> directly when the results can be
    /// paginated beforehand i.e. in the database. This is more efficient as
    /// pagination should aim to avoid pulling all results into memory. 
    /// This method is intended for cases where it is not possible to do this.
    /// </remarks>
    /// <param name="allResults">All of the un-paginated results</param>
    /// <param name="page">The current page</param>
    /// <param name="pageSize">The size of each page</param>
    public static Either<ActionResult, PaginatedListViewModel<T>> Paginate(
        List<T> allResults,
        int page,
        int pageSize)
    {
        var pagedResults = allResults
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var paginatedViewModel = new PaginatedListViewModel<T>(
            results: pagedResults,
            totalResults: allResults.Count,
            page: page,
            pageSize: pageSize
        );

        if (paginatedViewModel.Paging.Page > paginatedViewModel.Paging.TotalPages)
        {
            return new NotFoundResult();
        }

        return paginatedViewModel;
    }
}

public record PagingViewModel
{
    public int Page { get; }

    public int PageSize { get; }

    public int TotalResults { get; }

    public int TotalPages => ((TotalResults - 1) / PageSize) + 1;

    public PagingViewModel(int page, int pageSize, int totalResults)
    {
        if (page < 1)
        {
            throw new ArgumentException("Page cannot be less than 1");
        }

        if (pageSize < 0)
        {
            throw new ArgumentException("Page size cannot be less than 0");
        }
        
        Page = page;
        PageSize = pageSize;
        TotalResults = totalResults;
    }
}