using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.Language.Flow;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public static class DbSetTestUtils
    {
        public static IReturnsResult<TDbContext> ReturnsDbSet<TDbContext, T>(this ISetup<TDbContext, DbSet<T>> setup, IEnumerable<T> data) where T : class where TDbContext : class
        {
            var queryable = data.AsQueryable();
            
            var mockSet = new Mock<DbSet<T>>();

            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

            return setup.Returns(mockSet.Object);
        }
    }
}