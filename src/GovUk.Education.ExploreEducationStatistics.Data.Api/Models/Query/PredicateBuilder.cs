using System;
using System.Linq.Expressions;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query
{
    public static class PredicateBuilder
    {   
        public static Expression<Func<T, bool>> True<T>()
        {
            return input => true;
        }

        public static Expression<Func<T, bool>> False<T>()
        {
            return input => false;
        }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expression1,
            Expression<Func<T, bool>> expression2)
        {
            var invocationExpression =
                Expression.Invoke(expression2, expression1.Parameters);
            return Expression.Lambda<Func<T, bool>>(
                Expression.OrElse(expression1.Body, invocationExpression),
                expression1.Parameters);
        }

        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expression1,
            Expression<Func<T, bool>> expression2)
        {
            var invocationExpression = Expression.Invoke(expression2, expression1.Parameters);
            return Expression.Lambda<Func<T, bool>>(
                Expression.AndAlso(expression1.Body, invocationExpression),
                expression1.Parameters);
        }
    }
}