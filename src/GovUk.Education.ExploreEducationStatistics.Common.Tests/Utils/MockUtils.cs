using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils
{
    public static class MockUtils
    {
        public static Mock<IPersistenceHelper<TDbContext>> MockPersistenceHelper<TDbContext>()
            where TDbContext : DbContext
        {
            return new Mock<IPersistenceHelper<TDbContext>>();
        }

        public static Mock<IPersistenceHelper<TDbContext>> MockPersistenceHelper<TDbContext, TEntity>(
            Guid id, TEntity entity)
            where TDbContext : DbContext where TEntity : class
        {
            var helper = new Mock<IPersistenceHelper<TDbContext>>();
            SetupCall(helper, id, entity);
            return helper;
        }

        public static Mock<IPersistenceHelper<TDbContext>> MockPersistenceHelper<TDbContext, TEntity>()
            where TDbContext : DbContext
            where TEntity : class
        {
            var helper = new Mock<IPersistenceHelper<TDbContext>>();
            SetupCall<TDbContext, TEntity>(helper);
            return helper;
        }

        public static void SetupCall<TDbContext, TEntity>(
            Mock<IPersistenceHelper<TDbContext>> helper,
            Guid id,
            TEntity entity)
            where TDbContext : DbContext
            where TEntity : class
        {
            helper
                .Setup(s => s.CheckEntityExists(id,
                    It.IsAny<Func<IQueryable<TEntity>, IQueryable<TEntity>>>()))
                .ReturnsAsync(new Either<ActionResult, TEntity>(entity));
        }

        public static void SetupCall<TDbContext, TEntity>(
            Mock<IPersistenceHelper<TDbContext>> helper)
            where TDbContext : DbContext
            where TEntity : class
        {
            helper
                .Setup(s => s.CheckEntityExists(It.IsAny<Guid>(),
                    It.IsAny<Func<IQueryable<TEntity>, IQueryable<TEntity>>>()))
                .ReturnsAsync(new Either<ActionResult, TEntity>(Activator.CreateInstance<TEntity>()));
        }

        public static Mock<IUserService> AlwaysTrueUserService()
        {
            return AlwaysTrueUserService<Enum>();
        }

        public static Mock<IUserService> AlwaysTrueUserService<T>()
            where T : Enum
        {
            var userService = new Mock<IUserService>();

            userService
                .Setup(s => s.MatchesPolicy(It.IsAny<T>()))
                .ReturnsAsync(true);

            userService
                .Setup(s => s.MatchesPolicy(It.IsAny<object>(), It.IsAny<T>()))
                .ReturnsAsync(true);

            return userService;
        }

        public static void VerifyAllMocks(params object[] mocks)
        {
            mocks
                .ToList()
                .ForEach(m =>
                {
                    m.GetType().GetMethod("VerifyAll", Type.EmptyTypes).Invoke(m, null);
                    m.GetType().GetMethod("VerifyNoOtherCalls", Type.EmptyTypes).Invoke(m, null);
                });
        }
    }
}