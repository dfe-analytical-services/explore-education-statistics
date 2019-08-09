using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces
{
    public interface IDataService<TResult>
    {
        TResult Query(ObservationQueryContext queryContext);
    }
}