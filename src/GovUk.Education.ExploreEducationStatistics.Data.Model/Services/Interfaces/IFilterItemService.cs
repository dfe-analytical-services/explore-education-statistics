using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface IFilterItemService : IRepository<FilterItem, long>
    {
        IEnumerable<FilterItem> GetFilterItems(Expression<Func<Observation, bool>> observationPredicate);
    }
}