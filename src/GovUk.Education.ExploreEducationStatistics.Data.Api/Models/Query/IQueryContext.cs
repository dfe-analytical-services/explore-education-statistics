using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query
{
    public interface IQueryContext<TEntity> where TEntity : class
    {
        Guid PublicationId { get; set; }
        ICollection<string> Attributes { get; set; }
        Expression<Func<TEntity, bool>> FindExpression();
    }
}