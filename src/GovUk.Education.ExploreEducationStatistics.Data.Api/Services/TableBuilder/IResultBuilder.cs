using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.TableBuilder
{
    public interface IResultBuilder<in TData, out TResult>
        where TData : ITidyData
        where TResult : ITableBuilderData
    {
        TResult BuildResult(TData data, ICollection<string> indicatorFilter);
    }
}