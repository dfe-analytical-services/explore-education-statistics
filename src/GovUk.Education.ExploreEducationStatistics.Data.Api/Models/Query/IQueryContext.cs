using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query
{
    public interface IQueryContext<TCollection> where TCollection : ITidyData
    {
        Guid PublicationId { get; set; }
        ICollection<string> Attributes { get; set; }
        Expression<Func<TCollection, bool>> FindExpression();
    }
}