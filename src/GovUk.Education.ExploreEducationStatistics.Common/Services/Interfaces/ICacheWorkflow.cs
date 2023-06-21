using System;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;

public interface ICacheWorkflow
{
    Task<object> GetOrCreateItemAsync(object cacheKey, Func<Task<object>> createItemFn);
}