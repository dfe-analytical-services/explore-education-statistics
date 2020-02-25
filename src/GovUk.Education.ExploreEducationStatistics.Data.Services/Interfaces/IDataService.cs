using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces
{
    public interface IDataService<TResult>
    {
        Task<Either<ActionResult, TResult>> Query(ObservationQueryContext queryContext);
    }
}