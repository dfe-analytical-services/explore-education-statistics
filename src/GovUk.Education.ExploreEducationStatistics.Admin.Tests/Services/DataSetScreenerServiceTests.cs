#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Screener;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public abstract class DataSetScreenerServiceTests
{
    public class ScreenDataSetTests : DataSetScreenerServiceTests
    {
        [Fact]
        public async Task Success()
        {
            // Arrange
            var screenRequest = new DataSetScreenerRequest
            {
                DataFileName = "data-file-name",
                DataFilePath = "data-file-path",
                MetaFileName = "meta-file-name",
                MetaFilePath = "meta-file-path",
                StorageContainerName = "storage-container-name",
            };

            var screenResponse = new DataSetScreenerResponse { OverallResult = "Success", TestResults = [] };

            var screenerClient = new Mock<IDataSetScreenerClient>(MockBehavior.Strict);

            screenerClient
                .Setup(s => s.ScreenDataSet(screenRequest, CancellationToken.None))
                .ReturnsAsync(screenResponse);

            var service = BuildService(screenerClient: screenerClient.Object);

            // Act
            var result = await service.ScreenDataSet(dataSetScreenerRequest: screenRequest, CancellationToken.None);

            // Assert
            screenerClient.VerifyAll();

            Assert.Equal(screenResponse, result);
        }
    }

    public class StartScreeningTests : DataSetScreenerServiceTests
    {
        [Fact]
        public async Task Success()
        {
            // Arrange
            var screenRequest = new DataSetStartScreeningRequest
            {
                DataSetId = Guid.NewGuid(),
                DataFileName = "data-file-name",
                DataFilePath = "data-file-path",
                MetaFileName = "meta-file-name",
                MetaFilePath = "meta-file-path",
                StorageContainerName = "storage-container-name",
            };

            var queueServiceClient = new Mock<IQueueServiceClient>(MockBehavior.Strict);

            queueServiceClient
                .Setup(s =>
                    s.SendMessageAsJson(
                        DataSetScreenerService.StartScreeningQueue,
                        screenRequest,
                        CancellationToken.None
                    )
                )
                .Returns(Task.CompletedTask);

            var service = BuildService(queueServiceClient: queueServiceClient.Object);

            // Act
            await service.StartScreening(dataSetScreenRequest: screenRequest, CancellationToken.None);

            // Assert
            queueServiceClient.VerifyAll();
        }
    }

    private DataSetScreenerService BuildService(
        IDataSetScreenerClient? screenerClient = null,
        IQueueServiceClient? queueServiceClient = null
    )
    {
        return new DataSetScreenerService(
            dataSetScreenerClient: screenerClient ?? Mock.Of<IDataSetScreenerClient>(MockBehavior.Strict),
            queueServiceClient: queueServiceClient ?? Mock.Of<IQueueServiceClient>(MockBehavior.Strict)
        );
    }
}
