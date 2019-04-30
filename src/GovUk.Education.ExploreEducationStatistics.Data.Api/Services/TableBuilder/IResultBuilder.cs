using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.TableBuilder;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.TableBuilder
{
    public interface IResultBuilder<in TData, out TResult>
        where TData : Observation
        where TResult : TableBuilderObservationViewModel
    {
        TResult BuildResult(TData observation, IEnumerable<long> indicators);
    }
}