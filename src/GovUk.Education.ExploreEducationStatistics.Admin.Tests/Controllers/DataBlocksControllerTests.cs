using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers
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