using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.ViewModels;

public class PaginatedListViewModelTests
{
    [Fact]
    public void PaginatedListViewModel()
    {
        var allResults = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        var page = 2;
        var pageSize = 3;

        var result = PaginatedListViewModel<int>.Create(
            allResults, page, pageSize);

        var paginatedViewModel = result.AssertRight();

        Assert.Equal(new List<int> { 4, 5, 6 },  paginatedViewModel.Results);
        Assert.Equal(page, paginatedViewModel.Paging.Page);
        Assert.Equal(pageSize, paginatedViewModel.Paging.PageSize);
        Assert.Equal(allResults.Count, paginatedViewModel.Paging.TotalResults);
        Assert.Equal(4, paginatedViewModel.Paging.TotalPages);
    }

    [Fact]
    public void PaginatedListViewModel_NoPage()
    {
        var allResults = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        var pageSize = 3;

        var result = PaginatedListViewModel<int>.Create(
            allResults, page: null, pageSize);

        var paginatedViewModel = result.AssertRight();

        Assert.Equal(allResults,  paginatedViewModel.Results);
        Assert.Equal(1, paginatedViewModel.Paging.Page);
        Assert.Equal(allResults.Count, paginatedViewModel.Paging.PageSize);
        Assert.Equal(allResults.Count, paginatedViewModel.Paging.TotalResults);
        Assert.Equal(1, paginatedViewModel.Paging.TotalPages);
    }

    [Fact]
    public void PaginatedListViewModel_NoPageSize()
    {
        var allResults = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        var page = 2;

        var result = PaginatedListViewModel<int>.Create(
            allResults, page, pageSize: null);

        var paginatedViewModel = result.AssertRight();

        Assert.Equal(allResults,  paginatedViewModel.Results);
        Assert.Equal(1, paginatedViewModel.Paging.Page);
        Assert.Equal(allResults.Count, paginatedViewModel.Paging.PageSize);
        Assert.Equal(allResults.Count, paginatedViewModel.Paging.TotalResults);
        Assert.Equal(1, paginatedViewModel.Paging.TotalPages);
    }

    [Fact]
    public void PaginatedListViewModel_LastPage()
    {
        var allResults = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        var page = 4;
        var pageSize = 3;

        var result = PaginatedListViewModel<int>.Create(
            allResults, page, pageSize);

        var paginatedViewModel = result.AssertRight();

        Assert.Equal(new List<int> { 10 },  paginatedViewModel.Results);
        Assert.Equal(page, paginatedViewModel.Paging.Page);
        Assert.Equal(pageSize, paginatedViewModel.Paging.PageSize);
        Assert.Equal(allResults.Count, paginatedViewModel.Paging.TotalResults);
        Assert.Equal(4, paginatedViewModel.Paging.TotalPages);
    }

    [Fact]
    public void PaginatedListViewModel_SinglePage()
    {
        var allResults = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        var page = 1;
        var pageSize = 100;

        var result = PaginatedListViewModel<int>.Create(
            allResults, page, pageSize);

        var paginatedViewModel = result.AssertRight();

        Assert.Equal(allResults,  paginatedViewModel.Results);
        Assert.Equal(page, paginatedViewModel.Paging.Page);
        Assert.Equal(pageSize, paginatedViewModel.Paging.PageSize);
        Assert.Equal(allResults.Count, paginatedViewModel.Paging.TotalResults);
        Assert.Equal(1, paginatedViewModel.Paging.TotalPages);
    }

    [Fact]
    public void PaginatedListViewModel_SingleResult()
    {
        var allResults = new List<int> { 1 };

        var page = 1;
        var pageSize = 10;

        var result = PaginatedListViewModel<int>.Create(
            allResults, page, pageSize);

        var paginatedViewModel = result.AssertRight();

        Assert.Equal(allResults,  paginatedViewModel.Results);
        Assert.Equal(page, paginatedViewModel.Paging.Page);
        Assert.Equal(pageSize, paginatedViewModel.Paging.PageSize);
        Assert.Equal(allResults.Count, paginatedViewModel.Paging.TotalResults);
        Assert.Equal(1, paginatedViewModel.Paging.TotalPages);
    }

    [Fact]
    public void PaginatedListViewModel_NoResults()
    {
        var noResults = new List<int>();

        var page = 1;
        var pageSize = 10;

        var result = PaginatedListViewModel<int>.Create(
            noResults, page, pageSize);

        var paginatedViewModel = result.AssertRight();

        Assert.Equal(noResults,  paginatedViewModel.Results);
        Assert.Equal(page, paginatedViewModel.Paging.Page);
        Assert.Equal(pageSize, paginatedViewModel.Paging.PageSize);
        Assert.Equal(noResults.Count, paginatedViewModel.Paging.TotalResults);
        Assert.Equal(1, paginatedViewModel.Paging.TotalPages);
    }

    [Fact]
    public void PaginatedListViewModel_RequestedPageDoesNotExist()
    {
        var allResults = new List<int>{ 1, 2, 3 };

        var page = 2;
        var pageSize = 10;

        var result = PaginatedListViewModel<int>.Create(
            allResults, page, pageSize);

        result.AssertNotFound();
    }
}
