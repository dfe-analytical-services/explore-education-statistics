using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api
{
    public class DataBlocksControllerTests
    {
        private readonly Guid _releaseId = Guid.NewGuid();

        [Fact]
        public async void Create_DataBlock_Returns_Ok()
        {
            var mocks = Mocks();

            var dataBlock = new CreateDataBlockViewModel();

            mocks.DataBlockService.Setup(s => s.CreateAsync(_releaseId, dataBlock))
                .Returns(Task.FromResult(new DataBlockViewModel()));

            mocks.ReleaseService.Setup(s => s.GetAsync(_releaseId))
                .Returns(Task.FromResult(new Release
                {
                    Id = _releaseId
                }));

            var controller = ControllerWithMocks(mocks);

            var result = await controller.CreateDataBlockAsync(_releaseId, dataBlock);
            AssertOkResult(result);
        }

        [Fact]
        public async void Delete_DataBlock_Returns_NoContent()
        {
            var mocks = Mocks();

            var id = Guid.NewGuid();

            mocks.DataBlockService.Setup(s => s.GetAsync(id)).Returns(Task.FromResult(new DataBlockViewModel()));

            mocks.DataBlockService.Setup(s => s.DeleteAsync(id))
                .Returns(Task.CompletedTask);

            var controller = ControllerWithMocks(mocks);

            var result = await controller.DeleteDataBlockAsync(id);
            Assert.IsAssignableFrom<NoContentResult>(result);
        }

        [Fact]
        public async void Delete_DataBlock_Returns_NotFound()
        {
            var mocks = Mocks();

            var id = Guid.NewGuid();

            var controller = ControllerWithMocks(mocks);

            var result = await controller.DeleteDataBlockAsync(id);
            Assert.IsAssignableFrom<NotFoundResult>(result);
        }

        [Fact]
        public async void Get_DataBlock_Returns_Ok()
        {
            var mocks = Mocks();

            var id = Guid.NewGuid();

            mocks.DataBlockService.Setup(s => s.GetAsync(id)).Returns(Task.FromResult(new DataBlockViewModel()));

            mocks.ReleaseService.Setup(s => s.GetAsync(_releaseId))
                .Returns(Task.FromResult(new Release
                {
                    Id = _releaseId
                }));

            var controller = ControllerWithMocks(mocks);

            var result = await controller.GetDataBlockAsync(id);
            AssertOkResult(result);
        }

        [Fact]
        public async void Get_DataBlocks_Returns_Ok()
        {
            var mocks = Mocks();

            var sampleRes = new List<DataBlockViewModel>
            {
                new DataBlockViewModel()
            };

            mocks.DataBlockService.Setup(s => s.ListAsync(_releaseId)).Returns(Task.FromResult(sampleRes));

            mocks.ReleaseService.Setup(s => s.GetAsync(_releaseId))
                .Returns(Task.FromResult(new Release
                {
                    Id = _releaseId
                }));

            var controller = ControllerWithMocks(mocks);

            var result = await controller.GetDataBlocksAsync(_releaseId);
            AssertOkResult(result);
        }

        [Fact]
        public async void Update_DataBlock_Returns_Ok()
        {
            var mocks = Mocks();

            var id = Guid.NewGuid();
            var dataBlock = new UpdateDataBlockViewModel();

            mocks.DataBlockService.Setup(s => s.UpdateAsync(id, dataBlock))
                .Returns(Task.FromResult(new DataBlockViewModel()));

            var controller = ControllerWithMocks(mocks);

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

        private static (Mock<IDataBlockService> DataBlockService, Mock<IReleaseService> ReleaseService) Mocks()
        {
            return (new Mock<IDataBlockService>(),
                new Mock<IReleaseService>());
        }

        private static DataBlocksController ControllerWithMocks((Mock<IDataBlockService> DataBlockService,
            Mock<IReleaseService> ReleaseService) mocks)
        {
            var (dataBlockService, releaseService) = mocks;
            return new DataBlocksController(dataBlockService.Object, releaseService.Object);
        }
    }
}