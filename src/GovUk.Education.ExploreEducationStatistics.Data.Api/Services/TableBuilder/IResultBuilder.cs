using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.TableBuilder
{
    public interface IResultBuilder<in TData, out TResult>
        where TData : Observation
        where TResult : ITableBuilderData
    {
        TResult BuildResult(TData observation, ICollection<string> indicators);
    }
}