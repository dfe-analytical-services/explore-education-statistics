#nullable enable
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Common.ViewModels
{
    public class PaginatedListViewModel<T>
    {
        public List<T> Results { get; }

        public PagingViewModel Paging { get; }

        private PaginatedListViewModel(List<T> pageResults, int totalResultCount, int page, int pageSize)
        {
            Results = pageResults;
            Paging = new PagingViewModel
            {
                Page = page,
                PageSize = pageSize,
                TotalResults = totalResultCount,
            };
        }

        public static Either<ActionResult, PaginatedListViewModel<T>> Create(
            List<T> allResults,
            int? page,
            int? pageSize)
        {
            PaginatedListViewModel<T> paginatedViewModel;

            if (!page.HasValue || !pageSize.HasValue)
            {
                // return all results on single page
                paginatedViewModel = new PaginatedListViewModel<T>(
                    allResults, allResults.Count, page: 1, pageSize: allResults.Count);
            }
            else
            {
                var pageResults = allResults
                    .Skip((page.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value)
                    .ToList();

                paginatedViewModel = new PaginatedListViewModel<T>(
                    pageResults, allResults.Count, page.Value, pageSize.Value);
            }

            if (page <= 0 || paginatedViewModel.Paging.Page > paginatedViewModel.Paging.TotalPages)
            {
                return new NotFoundResult();
            }

            return paginatedViewModel;
        }
    }

    public class PagingViewModel
    {
        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalResults { get; set; }

        public int TotalPages => ((TotalResults - 1) / PageSize) + 1;
    }

}
