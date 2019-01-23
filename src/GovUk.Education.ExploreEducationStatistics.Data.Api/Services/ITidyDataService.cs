using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public interface ITidyDataService<out TCollection, in TQueryContext>
        where TCollection : ITidyData
        where TQueryContext : IQueryContext<TCollection>
    {
        IEnumerable<TCollection> FindMany(TQueryContext queryContext);
    }
}