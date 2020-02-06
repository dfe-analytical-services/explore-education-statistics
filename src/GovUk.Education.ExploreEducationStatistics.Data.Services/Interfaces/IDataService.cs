using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using ReleaseId = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces
{
    public interface IDataService<out TResult>
    {
        TResult Query(ObservationQueryContext queryContext, ReleaseId? releaseId = null);
    }
}