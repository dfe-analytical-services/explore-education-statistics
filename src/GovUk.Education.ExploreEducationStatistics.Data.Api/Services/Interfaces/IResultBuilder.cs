using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces
{
    public interface IResultBuilder<in TData, out TResult>
        where TData : Observation
        where TResult : ObservationViewModel
    {
        TResult BuildResult(TData observation, IEnumerable<long> indicators);
    }
}