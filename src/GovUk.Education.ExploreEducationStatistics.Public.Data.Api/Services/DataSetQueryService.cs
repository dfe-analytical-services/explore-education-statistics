using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests.Query;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;

public class DataSetQueryService : IDataSetQueryService
{
    public async Task<Either<ActionResult, DataSetQueryPaginatedResultsViewModel>> Query(
        Guid dataSetId,
        DataSetQueryCriteria criteria)
    {
        return await Task.FromResult(new DataSetQueryPaginatedResultsViewModel
        {
            Paging = new PagingViewModel(page: 1, pageSize: 1000, totalResults: 2000),
            Warnings = [],
            Results = [
                new DataSetQueryResultViewModel
                {
                    Filters = [],
                    Locations = [],
                    TimePeriod = new TimePeriodViewModel
                    {
                      Code  = TimeIdentifier.December,
                      Period = "2020/21"
                    },
                    GeographicLevel = GeographicLevel.School,
                    Values = []
                }
            ]
        });
    }
}
