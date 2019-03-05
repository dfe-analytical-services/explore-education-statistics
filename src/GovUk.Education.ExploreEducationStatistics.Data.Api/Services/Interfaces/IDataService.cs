using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces
{
    public interface IDataService<TEntity> where TEntity : class
    {
        int Count();

        int Count(Expression<Func<TEntity, bool>> expression);

        IEnumerable<TEntity> FindMany(Expression<Func<TEntity, bool>> expression);
    }
}