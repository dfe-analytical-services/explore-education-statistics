using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces
{
    public interface IResultBuilder<in TData, out TResult>
        where TData : Observation
        where TResult : ObservationViewModel
    {
        TResult BuildResult(TData observation, IEnumerable<Guid> indicators);
    }
}