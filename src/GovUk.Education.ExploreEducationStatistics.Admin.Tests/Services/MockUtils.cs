using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
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
                .Setup(s => s.CheckEntityExists<TEntity>(id,
                                It.IsAny<Func<IQueryable<TEntity>, IQueryable<TEntity>>>()))
                .ReturnsAsync(new Either<ActionResult, TEntity>(entity));
        }
        
        public static void SetupCall<TDbContext, TEntity>(
            Mock<IPersistenceHelper<TDbContext>> helper) 
            where TDbContext : DbContext 
            where TEntity : class
        { 
            helper
                .Setup(s => s.CheckEntityExists<TEntity>(It.IsAny<Guid>(),
                    It.IsAny<Func<IQueryable<TEntity>, IQueryable<TEntity>>>()))
                .ReturnsAsync(new Either<ActionResult, TEntity>(Activator.CreateInstance<TEntity>()));
        }

        public static Mock<IUserService> AlwaysTrueUserService()
        {
            var userService = new Mock<IUserService>();

            userService
                .Setup(s => s.MatchesPolicy(It.IsAny<SecurityPolicies>()))
                .ReturnsAsync(true);

            userService
                .Setup(s => s.MatchesPolicy(It.IsAny<object>(), It.IsAny<SecurityPolicies>()))
                .ReturnsAsync(true);

            return userService;
        }
    }
}