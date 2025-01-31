#nullable enable
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
            Guid id,
            TEntity entity)
            where TDbContext : DbContext where TEntity : class
        {
            var helper = new Mock<IPersistenceHelper<TDbContext>>();
            SetupCall(helper, id, entity);
            return helper;
        }

        public static Mock<IPersistenceHelper<TDbContext>> MockPersistenceHelper<TDbContext, TEntity>(TEntity? entity)
            where TDbContext : DbContext where TEntity : class
        {
            var helper = new Mock<IPersistenceHelper<TDbContext>>();
            SetupCall(helper, entity);
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
            TEntity? entity)
            where TDbContext : DbContext
            where TEntity : class
        {
            helper
                .Setup(s =>
                    s.CheckEntityExists(id, It.IsAny<Func<IQueryable<TEntity>, IQueryable<TEntity>>>()))
                .ReturnsAsync(EntityOrNotFoundResult(entity));
        }

        public static void SetupCall<TDbContext, TEntity>(
            Mock<IPersistenceHelper<TDbContext>> helper,
            TEntity? entity)
            where TDbContext : DbContext
            where TEntity : class
        {
            helper
                .Setup(
                    s => s.CheckEntityExists(
                        It.IsAny<Func<IQueryable<TEntity>, IQueryable<TEntity>>>()
                    )
                )
                .ReturnsAsync(EntityOrNotFoundResult(entity));
        }

        public static void SetupCall<TDbContext, TEntity>(Mock<IPersistenceHelper<TDbContext>> helper)
            where TDbContext : DbContext
            where TEntity : class
        {
            helper
                .Setup(s =>
                    s.CheckEntityExists(It.IsAny<Guid>(), It.IsAny<Func<IQueryable<TEntity>, IQueryable<TEntity>>>()))
                .ReturnsAsync(new Either<ActionResult, TEntity>(Activator.CreateInstance<TEntity>()));
        }

        private static Func<Either<ActionResult, TEntity>> EntityOrNotFoundResult<TEntity>(TEntity? entity)
            where TEntity : class
        {
            return () =>
            {
                if (entity != null)
                {
                    return entity;
                }

                return new NotFoundResult();
            };
        }

        public static Mock<IUserService> AlwaysTrueUserService(Guid? userId = null)
        {
            return AlwaysTrueUserService<Enum>(userId);
        }

        public static Mock<IUserService> AlwaysTrueUserService<T>(Guid? userId = null)
            where T : Enum
        {
            var userService = new Mock<IUserService>();

            userService
                .Setup(s => s.MatchesPolicy(It.IsAny<T>()))
                .ReturnsAsync(true);

            userService
                .Setup(s => s.MatchesPolicy(It.IsAny<object>(), It.IsAny<T>()))
                .ReturnsAsync(true);

            if (userId.HasValue)
            {
                userService.Setup(s => s.GetUserId())
                    .Returns(userId.Value);
            }

            return userService;
        }

        public static void VerifyAllMocks(params Mock[] mocks)
        {
            mocks
                .ToList()
                .ForEach(mock =>
                {
                    mock.VerifyAll();

                    // We can't cast from the non-generic Mock Type to the generic Mock<?> Type which has the
                    // VerifyNoOtherCalls() method on it, so we need to look it up with reflection.
                    var verifyNoOtherCallsMethod = mock.GetType().GetMethod("VerifyNoOtherCalls", Type.EmptyTypes);

                    if (verifyNoOtherCallsMethod != null)
                    {
                        verifyNoOtherCallsMethod.Invoke(mock, null);
                    }
                });
        }
        
        public static void VerifyAllMocks(ITuple mocks)
        {
            Mock[] values = mocks
                .GetType()
                .GetFields()
                .Select(f => f.GetValue(mocks))
                .Cast<Mock>()
                .ToArray();
            
            VerifyAllMocks(values);
        }

        public static Mock<IConfiguration> CreateMockConfiguration(params Tuple<string, string>[] keysAndValues)
        {
            var configuration = new Mock<IConfiguration>(MockBehavior.Strict);
            return PopulateMockConfiguration(keysAndValues, configuration);
        }

        public static Mock<IConfigurationSection> CreateMockConfigurationSection(params Tuple<string, string>[] keysAndValues)
        {
            var configuration = new Mock<IConfigurationSection>(MockBehavior.Strict);
            return PopulateMockConfiguration(keysAndValues, configuration);
        }

        private static Mock<TConfiguration> PopulateMockConfiguration<TConfiguration>(Tuple<string, string>[] keysAndValues, Mock<TConfiguration> configuration)
            where TConfiguration : class, IConfiguration
        {
            foreach (var keyValue in keysAndValues)
            {
                var (key, value) = keyValue;

                var section = new Mock<IConfigurationSection>();

                section
                    .Setup(s => s.Value)
                    .Returns(value);

                configuration
                    .Setup(c => c.GetSection(key))
                    .Returns(section.Object);
            }

            return configuration;
        }
    }
}
