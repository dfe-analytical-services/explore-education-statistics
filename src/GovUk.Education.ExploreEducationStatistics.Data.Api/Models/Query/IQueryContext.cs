using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query
{
    public interface IQueryContext<TEntity> where TEntity : class
    {
        long SubjectId { get; set; }
        IEnumerable<long> Filters { get; set; }
        IEnumerable<long> Indicators { get; set; }
        Expression<Func<TEntity, bool>> FindExpression();
    }
}