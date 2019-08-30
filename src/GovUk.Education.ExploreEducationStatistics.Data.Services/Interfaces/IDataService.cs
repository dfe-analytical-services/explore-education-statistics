using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces
{
    public interface IDataService<out TResult>
    {
        TResult Query(ObservationQueryContext queryContext);
    }
}