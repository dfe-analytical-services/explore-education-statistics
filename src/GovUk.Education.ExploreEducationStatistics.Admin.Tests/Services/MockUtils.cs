using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public static class MockUtils
    {
        public static Mock<IPersistenceHelper<TEntity, TEntityId>> MockPersistenceHelper<TEntity, TEntityId>(
            TEntityId id, TEntity entity) where TEntity : class
        {
            var persistenceHelper = new Mock<IPersistenceHelper<TEntity, TEntityId>>();

            persistenceHelper
                .Setup(s => s.CheckEntityExists(id, 
                    It.IsAny<Func<IQueryable<TEntity>, IQueryable<TEntity>>>()))
                .ReturnsAsync(new Either<ActionResult, TEntity>(entity));

            return persistenceHelper;
        }
        
        public static Mock<IPersistenceHelper<TEntity, TEntityId>> MockPersistenceHelper<TEntity, TEntityId>() 
            where TEntity : class
        {
            var persistenceHelper = new Mock<IPersistenceHelper<TEntity, TEntityId>>();

            persistenceHelper
                .Setup(s => s.CheckEntityExists(It.IsAny<TEntityId>(), 
                    It.IsAny<Func<IQueryable<TEntity>, IQueryable<TEntity>>>()))
                .ReturnsAsync(new Either<ActionResult, TEntity>(Activator.CreateInstance<TEntity>()));

            return persistenceHelper;
        }
    }
}