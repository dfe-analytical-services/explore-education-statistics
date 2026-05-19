#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

public static class PaginatedViewModelTestExtensions
{
    public static void AssertHasExpectedPagingAndResultCount<T>(
        this PaginatedListViewModel<T> paginatedListViewModel,
        int expectedTotalResults,
        int expectedPage = 1,
        int expectedPageSize = 10
    )
    {
        Assert.Multiple(
            () => Assert.Equal(expectedTotalResults, paginatedListViewModel.Paging.TotalResults),
            () => Assert.Equal(expectedPage, paginatedListViewModel.Paging.Page),
            () => Assert.Equal(expectedPageSize, paginatedListViewModel.Paging.PageSize),
            () =>
                Assert.Equal(
                    paginatedListViewModel.Paging.TotalPages <= 1 ? expectedTotalResults : expectedPageSize,
                    paginatedListViewModel.Results.Count
                )
        );
    }

    public static void AssertEmptyResults<T>(this PaginatedListViewModel<T> paginatedListViewModel)
    {
        Assert.Multiple(
            () => Assert.Equal(0, paginatedListViewModel.Paging.TotalResults),
            () => Assert.Equal(1, paginatedListViewModel.Paging.Page),
            () => Assert.Equal(1, paginatedListViewModel.Paging.TotalPages),
            () => Assert.Empty(paginatedListViewModel.Results)
        );
    }

    public static void AssertSinglePage<T>(
        this PaginatedListViewModel<T> paginatedListViewModel,
        int expectedTotalResults,
        int? expectedPageSize = null
    )
    {
        Assert.Multiple(
            () => Assert.Equal(expectedTotalResults, paginatedListViewModel.Paging.TotalResults),
            () => Assert.Equal(1, paginatedListViewModel.Paging.Page),
            () => Assert.Equal(expectedPageSize ?? expectedTotalResults, paginatedListViewModel.Paging.PageSize),
            () => Assert.Equal(1, paginatedListViewModel.Paging.TotalPages),
            () => Assert.Equal(expectedTotalResults, paginatedListViewModel.Results.Count)
        );
    }

    public static T AssertSingleResult<T>(this PaginatedListViewModel<T> paginatedListViewModel)
    {
        Assert.Multiple(
            () => Assert.Equal(1, paginatedListViewModel.Paging.TotalResults),
            () => Assert.Equal(1, paginatedListViewModel.Paging.Page),
            () => Assert.Equal(1, paginatedListViewModel.Paging.TotalPages),
            () => Assert.Single(paginatedListViewModel.Results)
        );

        return paginatedListViewModel.Results.Single();
    }
}
