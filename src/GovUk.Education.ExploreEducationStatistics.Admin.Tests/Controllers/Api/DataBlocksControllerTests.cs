using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api
{
    public class DataBlocksControllerTests
    {
        private readonly Guid _releaseId = Guid.NewGuid();
        
        private readonly Task<Either<ValidationResult, Release>> _releaseExistsResult 
            = Task.FromResult(new Either<ValidationResult, Release>(new Release()));
        
        private readonly Task<Either<ActionResult, DataBlock>> _dataBlockExistsResult 
            = Task.FromResult(new Either<ActionResult, DataBlock>(new DataBlock()));
        
        private readonly Task<Either<ActionResult, DataBlock>> _dataBlockNotFoundResult 
            = Task.FromResult(new Either<ActionResult, DataBlock>(new NotFoundResult()));

        [Fact]
        public async void Create_DataBlock_Returns_Ok()
        {
            var (dataBlockService, releaseHelper, dataBlockHelper) = Mocks();

            var dataBlock = new CreateDataBlockViewModel();
            
            releaseHelper
                .Setup(s => s
                    .CheckEntityExistsChainable(_releaseId, null))
                .Returns(_releaseExistsResult);

            dataBlockService.Setup(s => s.CreateAsync(_releaseId, dataBlock))
                .Returns(Task.FromResult(new DataBlockViewModel()));

            var controller = ControllerWithMocks(dataBlockService, releaseHelper, dataBlockHelper);

            var result = await controller.CreateDataBlockAsync(_releaseId, dataBlock);
            AssertOkResult(result);
        }

        [Fact]
        public async void Delete_DataBlock_Returns_NoContent()
        {
            var (dataBlockService, releaseHelper, dataBlockHelper) = Mocks();

            var id = Guid.NewGuid();
            
            dataBlockHelper
                .Setup(s => s
                    .CheckEntityExistsChainableActionResult(id, null))
                .Returns(_dataBlockExistsResult);

            dataBlockService.Setup(s => s.GetAsync(id)).Returns(Task.FromResult(new DataBlockViewModel()));

            dataBlockService.Setup(s => s.DeleteAsync(id))
                .Returns(Task.CompletedTask);

            var controller = ControllerWithMocks(dataBlockService, releaseHelper, dataBlockHelper);

            var result = await controller.DeleteDataBlockAsync(id);
            Assert.IsAssignableFrom<NoContentResult>(result);
        }

        [Fact]
        public async void Delete_DataBlock_Returns_NotFound()
        {
            var (dataBlockService, releaseHelper, dataBlockHelper) = Mocks();

            var id = Guid.NewGuid();
            
            dataBlockHelper
                .Setup(s => s
                    .CheckEntityExistsChainableActionResult(id, null))
                .Returns(_dataBlockNotFoundResult);

            var controller = ControllerWithMocks(dataBlockService, releaseHelper, dataBlockHelper);

            var result = await controller.DeleteDataBlockAsync(id);
            Assert.IsAssignableFrom<NotFoundResult>(result);
        }

        [Fact]
        public async void Get_DataBlock_Returns_Ok()
        {
            var (dataBlockService, releaseHelper, dataBlockHelper) = Mocks();

            var id = Guid.NewGuid();

            dataBlockService.Setup(s => s.GetAsync(id)).Returns(Task.FromResult(new DataBlockViewModel()));

            var controller = ControllerWithMocks(dataBlockService, releaseHelper, dataBlockHelper);

            var result = await controller.GetDataBlockAsync(id);
            AssertOkResult(result);
        }

        [Fact]
        public async void Get_DataBlocks_Returns_Ok()
        {
            var (dataBlockService, releaseHelper, dataBlockHelper) = Mocks();

            var sampleRes = new List<DataBlockViewModel>
            {
                new DataBlockViewModel()
            };
            
            releaseHelper
                .Setup(s => s
                    .CheckEntityExistsChainable(_releaseId, null))
                .Returns(_releaseExistsResult);

            dataBlockService.Setup(s => s.ListAsync(_releaseId)).Returns(Task.FromResult(sampleRes));

            var controller = ControllerWithMocks(dataBlockService, releaseHelper, dataBlockHelper);

            var result = await controller.GetDataBlocksAsync(_releaseId);
            AssertOkResult(result);
        }

        [Fact]
        public async void Update_DataBlock_Returns_Ok()
        {
            var (dataBlockService, releaseHelper, dataBlockHelper) = Mocks();

            var id = Guid.NewGuid();
            var dataBlock = new UpdateDataBlockViewModel();

            dataBlockService.Setup(s => s.UpdateAsync(id, dataBlock))
                .Returns(Task.FromResult(new DataBlockViewModel()));

            var controller = ControllerWithMocks(dataBlockService, releaseHelper, dataBlockHelper);

            var result = await controller.UpdateDataBlockAsync(id, dataBlock);
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
            Mock<IDataBlockService> DataBlockService, 
            Mock<IPersistenceHelper<Release,Guid>> ReleaseHelper, 
            Mock<IPersistenceHelper<DataBlock,Guid>> DataBlockHelper) Mocks()
        {
            return (
                new Mock<IDataBlockService>(),
                new Mock<IPersistenceHelper<Release,Guid>>(),
                new Mock<IPersistenceHelper<DataBlock,Guid>>());
        }

        private static DataBlocksController ControllerWithMocks(
            Mock<IDataBlockService> dataBlockService, 
            Mock<IPersistenceHelper<Release,Guid>> releaseHelper, 
            Mock<IPersistenceHelper<DataBlock,Guid>> dataBlockHelper)
        {
            return new DataBlocksController(dataBlockService.Object, releaseHelper.Object, dataBlockHelper.Object);
        }
    }
}