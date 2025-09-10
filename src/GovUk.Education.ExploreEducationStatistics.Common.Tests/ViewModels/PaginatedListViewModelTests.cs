#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.ViewModels;

public class PaginatedListViewModelTests
{
    [Fact]
    public void InvalidPageThrows()
    {
        var allResults = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        Assert.Throws<ArgumentException>(() =>
            new PaginatedListViewModel<int>(allResults, allResults.Count, -1, 5));
    }

    [Fact]
    public void InvalidPageSizeThrows()
    {
        var allResults = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        Assert.Throws<ArgumentException>(() =>
            new PaginatedListViewModel<int>(allResults, allResults.Count, 1, -1));
    }

    [Fact]
    public void Paginate()
    {
        var allResults = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        const int page = 2;
        const int pageSize = 3;

        var paginatedViewModel = PaginatedListViewModel<int>.Paginate(
            allResults,
            page,
            pageSize);

        Assert.Equal([4, 5, 6], paginatedViewModel.Results);
        Assert.Equal(page, paginatedViewModel.Paging.Page);
        Assert.Equal(pageSize, paginatedViewModel.Paging.PageSize);
        Assert.Equal(allResults.Count, paginatedViewModel.Paging.TotalResults);
        Assert.Equal(4, paginatedViewModel.Paging.TotalPages);
    }

    [Fact]
    public void Paginate_LastPage()
    {
        var allResults = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        const int page = 4;
        const int pageSize = 3;

        var paginatedViewModel = PaginatedListViewModel<int>.Paginate(
            allResults,
            page,
            pageSize);

        Assert.Equal([10], paginatedViewModel.Results);
        Assert.Equal(page, paginatedViewModel.Paging.Page);
        Assert.Equal(pageSize, paginatedViewModel.Paging.PageSize);
        Assert.Equal(allResults.Count, paginatedViewModel.Paging.TotalResults);
        Assert.Equal(4, paginatedViewModel.Paging.TotalPages);
    }

    [Fact]
    public void Paginate_SinglePage()
    {
        var allResults = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        const int page = 1;
        const int pageSize = 100;

        var paginatedViewModel = PaginatedListViewModel<int>.Paginate(
            allResults,
            page,
            pageSize);

        Assert.Equal(allResults, paginatedViewModel.Results);
        Assert.Equal(page, paginatedViewModel.Paging.Page);
        Assert.Equal(pageSize, paginatedViewModel.Paging.PageSize);
        Assert.Equal(allResults.Count, paginatedViewModel.Paging.TotalResults);
        Assert.Equal(1, paginatedViewModel.Paging.TotalPages);
    }

    [Fact]
    public void Paginate_SingleResult()
    {
        var allResults = new List<int> { 1 };

        const int page = 1;
        const int pageSize = 10;

        var paginatedViewModel = PaginatedListViewModel<int>.Paginate(
            allResults,
            page,
            pageSize);

        Assert.Equal(allResults, paginatedViewModel.Results);
        Assert.Equal(page, paginatedViewModel.Paging.Page);
        Assert.Equal(pageSize, paginatedViewModel.Paging.PageSize);
        Assert.Equal(allResults.Count, paginatedViewModel.Paging.TotalResults);
        Assert.Equal(1, paginatedViewModel.Paging.TotalPages);
    }

    [Fact]
    public void Paginate_NoResults()
    {
        var allResults = new List<int>();

        const int page = 1;
        const int pageSize = 10;

        var paginatedViewModel = PaginatedListViewModel<int>.Paginate(
            allResults,
            page,
            pageSize);

        Assert.Equal(allResults, paginatedViewModel.Results);
        Assert.Equal(page, paginatedViewModel.Paging.Page);
        Assert.Equal(pageSize, paginatedViewModel.Paging.PageSize);
        Assert.Equal(allResults.Count, paginatedViewModel.Paging.TotalResults);
        Assert.Equal(1, paginatedViewModel.Paging.TotalPages);
    }

    [Fact]
    public void Paginate_PageDoesNotExist()
    {
        var allResults = new List<int>{ 1, 2, 3 };

        const int page = 2;
        const int pageSize = 10;

        var paginatedViewModel = PaginatedListViewModel<int>.Paginate(
            allResults,
            page,
            pageSize);

        Assert.Empty(paginatedViewModel.Results);
        Assert.Equal(page, paginatedViewModel.Paging.Page);
        Assert.Equal(pageSize, paginatedViewModel.Paging.PageSize);
        Assert.Equal(allResults.Count, paginatedViewModel.Paging.TotalResults);
        Assert.Equal(1, paginatedViewModel.Paging.TotalPages);
    }
}
