using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces
{
    public interface IDataService<out TResult>
    {
        TResult Query(ObservationQueryContext queryContext);
    }
}