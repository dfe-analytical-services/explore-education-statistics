using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api
{
    public class DataBlocksControllerTests
    {
        private readonly Guid _releaseId = Guid.NewGuid();
        private readonly Guid _dataBlockId = Guid.NewGuid();
        
        private readonly Task<Either<ActionResult, Release>> _releaseExistsResult 
            = Task.FromResult(new Either<ActionResult, Release>(new Release()));
        
        private readonly Task<Either<ActionResult, DataBlock>> _dataBlockExistsResult 
            = Task.FromResult(new Either<ActionResult, DataBlock>(new DataBlock()));
        
        private readonly Task<Either<ActionResult, DataBlock>> _dataBlockNotFoundResult 
            = Task.FromResult(new Either<ActionResult, DataBlock>(new NotFoundResult()));

        [Fact]
        public async void Create_DataBlock_Returns_Ok()
        {
            var (dataBlockService, persistenceHelper) = Mocks();
            SetupReleaseExistsResult(persistenceHelper);
            
            var dataBlock = new CreateDataBlockViewModel();

            dataBlockService.Setup(s => s.CreateAsync(_releaseId, dataBlock))
                .ReturnsAsync(new DataBlockViewModel());

            var controller = ControllerWithMocks(dataBlockService, persistenceHelper);

            var result = await controller.CreateDataBlockAsync(_releaseId, dataBlock);
            AssertOkResult(result);
        }

        [Fact]
        public async void Delete_DataBlock_Returns_NoContent()
        {
            var (dataBlockService, persistenceHelper) = Mocks();
            SetupDataBlockExistsResult(persistenceHelper);

            dataBlockService.Setup(s => s.DeleteAsync(_dataBlockId))
                .Returns(Task.FromResult(new Either<ActionResult, bool>(true)));

            var controller = ControllerWithMocks(dataBlockService, persistenceHelper);

            var result = await controller.DeleteDataBlockAsync(_dataBlockId);
            Assert.IsAssignableFrom<NoContentResult>(result);
        }

        [Fact]
        public async void Delete_DataBlock_Returns_NotFound()
        {
            var (dataBlockService, persistenceHelper) = Mocks();
            SetupDataBlockNotFoundResult(persistenceHelper);

            var controller = ControllerWithMocks(dataBlockService, persistenceHelper);

            var result = await controller.DeleteDataBlockAsync(_dataBlockId);
            Assert.IsAssignableFrom<NotFoundResult>(result);
        }

        [Fact]
        public async void Get_DataBlock_Returns_Ok()
        {
            var (dataBlockService, persistenceHelper) = Mocks();
            SetupDataBlockExistsResult(persistenceHelper);
            dataBlockService.Setup(s => s.GetAsync(_dataBlockId)).ReturnsAsync(new DataBlockViewModel());
            
            var controller = ControllerWithMocks(dataBlockService, persistenceHelper);

            var result = await controller.GetDataBlockAsync(_dataBlockId);
            AssertOkResult(result);
        }

        [Fact]
        public async void Get_DataBlocks_Returns_Ok()
        {
            var (dataBlockService, persistenceHelper) = Mocks();
            SetupReleaseExistsResult(persistenceHelper);

            var sampleRes = new List<DataBlockViewModel>
            {
                new DataBlockViewModel()
            };

            dataBlockService.Setup(s => s.ListAsync(_releaseId)).ReturnsAsync(sampleRes);

            var controller = ControllerWithMocks(dataBlockService, persistenceHelper);

            var result = await controller.GetDataBlocksAsync(_releaseId);
            AssertOkResult(result);
        }

        [Fact]
        public async void Update_DataBlock_Returns_Ok()
        {
            var (dataBlockService, persistenceHelper) = Mocks();
            SetupDataBlockExistsResult(persistenceHelper);

            var dataBlock = new UpdateDataBlockViewModel();

            dataBlockService.Setup(s => s.UpdateAsync(_dataBlockId, dataBlock))
                .ReturnsAsync(new DataBlockViewModel());

            var controller = ControllerWithMocks(dataBlockService, persistenceHelper);

            var result = await controller.UpdateDataBlockAsync(_dataBlockId, dataBlock);
            AssertOkResult(result);
        }

        private static T AssertOkResult<T>(ActionResult<T> result) where T : class
        {
            Assert.IsAssignableFrom<ActionResult<T>>(result);
            Assert.IsAssignableFrom<OkObjectResult>(result.Result);
            var okObjectResult = result.Result as OkObjectResult;
            Assert.IsAssignableFrom<T>(okObjectResult?.Value);
            return okObjectResult?.Value as T;
        }

        private static (
            Mock<IDataBlockService>, 
            Mock<IPersistenceHelper<ContentDbContext>>) Mocks()
        {
            return (
                new Mock<IDataBlockService>(),
                new Mock<IPersistenceHelper<ContentDbContext>>());
        }

        private static DataBlocksController ControllerWithMocks(
            Mock<IDataBlockService> dataBlockService, 
            Mock<IPersistenceHelper<ContentDbContext>> persistenceHelper)
        {
            return new DataBlocksController(dataBlockService.Object, persistenceHelper.Object);
        }
        
        private void SetupReleaseExistsResult(Mock<IPersistenceHelper<ContentDbContext>> persistenceHelper)
        {
            persistenceHelper
                .Setup(s => s
                    .CheckEntityExists<Release>(_releaseId, null))
                .Returns(_releaseExistsResult);
        }
        
        private void SetupDataBlockExistsResult(Mock<IPersistenceHelper<ContentDbContext>> persistenceHelper)
        {
            persistenceHelper
                .Setup(s => s
                    .CheckEntityExists<DataBlock>(_dataBlockId, null))
                .Returns(_dataBlockExistsResult);
        }
        
        private void SetupDataBlockNotFoundResult(Mock<IPersistenceHelper<ContentDbContext>> persistenceHelper)
        {
            persistenceHelper
                .Setup(s => s
                    .CheckEntityExists<DataBlock>(_dataBlockId, null))
                .Returns(_dataBlockNotFoundResult);
        }
    }
}