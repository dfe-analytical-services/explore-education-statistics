#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Responses.Screener;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Screener;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Screener;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Screener;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.Extensions.Time.Testing;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Screener;

public abstract class DataSetScreenerServiceTests
{
    private readonly DataFixture _dataFixture = new();

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

    public class UpdateScreenerProgressTests : DataSetScreenerServiceTests
    {
        [Fact]
        public async Task DataSetsBeingScreened_NeedProgressUpdate_Success()
        {
            var utcNow = DateTimeOffset.UtcNow;

            // Arrange
            var dataSetsUndergoingScreening = _dataFixture
                .DefaultDataSetUpload()
                .WithStatus(DataSetUploadStatus.SCREENING)
                .ForIndex(
                    0,
                    s =>
                        s.SetScreenerProgress(
                            new DataSetScreenerProgress
                            {
                                Completed = false,
                                Passed = false,
                                PercentageComplete = 50,
                                Stage = "validation",
                                LastUpdated = utcNow.AddSeconds(-5),
                            }
                        )
                )
                .ForIndex(
                    1,
                    s =>
                        s.SetScreenerProgress(
                            new DataSetScreenerProgress
                            {
                                Completed = false,
                                Passed = false,
                                PercentageComplete = 70,
                                Stage = "screening",
                                LastUpdated = utcNow.AddSeconds(-6),
                            }
                        )
                )
                .GenerateList();

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.DataSetUploads.AddRangeAsync(dataSetsUndergoingScreening);
                await context.SaveChangesAsync();
            }

            var dataSetIds = dataSetsUndergoingScreening.Select(u => u.Id).ToList();

            // Return the progress responses out of order, to ensure they're matched up correctly with the
            // correct data sets by id.
            List<DataSetScreenerProgressResponse> progressResponse =
            [
                new()
                {
                    DataSetId = dataSetIds[1],
                    PercentageComplete = 100.00,
                    Completed = true,
                    Passed = true,
                    Stage = "complete",
                },
                new()
                {
                    DataSetId = dataSetIds[0],
                    PercentageComplete = 80.99,
                    Completed = false,
                    Passed = false,
                    Stage = "screening",
                },
            ];

            var screenerClient = new Mock<IDataSetScreenerClient>(MockBehavior.Strict);

            screenerClient
                .Setup(s => s.GetScreenerProgress(dataSetIds, CancellationToken.None))
                .ReturnsAsync(progressResponse);

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildService(
                    screenerClient: screenerClient.Object,
                    contentDbContext: context,
                    timeProvider: new FakeTimeProvider(utcNow)
                );

                // Act
                var result = await service.UpdateScreeningProgress(CancellationToken.None);

                // Assert
                screenerClient.VerifyAll();

                Assert.Equal(progressResponse, result);
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var updatedDataSetUploads = context.DataSetUploads.ToList();

                var dataSetUpload1 = updatedDataSetUploads[0];

                var expectedDataSet1ScreenerProgress = dataSetsUndergoingScreening[0].ScreenerProgress! with
                {
                    PercentageComplete = 80,
                    Stage = "screening",
                    LastUpdated = utcNow,
                };

                Assert.Equal(expectedDataSet1ScreenerProgress, dataSetUpload1.ScreenerProgress);

                var dataSetUpload2 = updatedDataSetUploads[1];

                var expectedDataSet2ScreenerProgress = dataSetsUndergoingScreening[1].ScreenerProgress! with
                {
                    PercentageComplete = 100,
                    Completed = true,
                    Passed = true,
                    Stage = "complete",
                    LastUpdated = utcNow,
                };

                Assert.Equal(expectedDataSet2ScreenerProgress, dataSetUpload2.ScreenerProgress);
            }
        }

        [Fact]
        public async Task DataSetBeingScreened_NoExistingProgressUpdates_NeedProgressUpdate_Success()
        {
            var utcNow = DateTimeOffset.UtcNow;

            // Arrange
            DataSetUpload dataSetUndergoingScreening = _dataFixture
                .DefaultDataSetUpload()
                .WithStatus(DataSetUploadStatus.SCREENING)
                // No existing progress updates yet.
                .WithScreenerProgress(null);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.DataSetUploads.AddRangeAsync(dataSetUndergoingScreening);
                await context.SaveChangesAsync();
            }

            // Return the progress responses out of order, to ensure they're matched up correctly with the
            // correct data sets by id.
            List<DataSetScreenerProgressResponse> progressResponse =
            [
                new()
                {
                    DataSetId = dataSetUndergoingScreening.Id,
                    PercentageComplete = 10,
                    Completed = false,
                    Passed = false,
                    Stage = "started",
                },
            ];

            var screenerClient = new Mock<IDataSetScreenerClient>(MockBehavior.Strict);

            screenerClient
                .Setup(s => s.GetScreenerProgress(new[] { dataSetUndergoingScreening.Id }, CancellationToken.None))
                .ReturnsAsync(progressResponse);

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildService(
                    screenerClient: screenerClient.Object,
                    contentDbContext: context,
                    timeProvider: new FakeTimeProvider(utcNow)
                );

                // Act
                var result = await service.UpdateScreeningProgress(CancellationToken.None);

                // Assert
                screenerClient.VerifyAll();

                Assert.Equal(progressResponse, result);
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var updatedDataSetUpload = context.DataSetUploads.Single();

                var expectedDataSetScreenerProgress = new DataSetScreenerProgress
                {
                    PercentageComplete = 10,
                    Completed = false,
                    Passed = false,
                    Stage = "started",
                    LastUpdated = utcNow,
                };

                Assert.Equal(expectedDataSetScreenerProgress, updatedDataSetUpload.ScreenerProgress);
            }
        }

        [Fact]
        public async Task DataSetBeingScreened_LastUpdatedRecently_NoNewProgressUpdateNeededYet()
        {
            var utcNow = DateTimeOffset.UtcNow;

            // Arrange
            DataSetUpload dataSetUndergoingScreening = _dataFixture
                .DefaultDataSetUpload()
                .WithStatus(DataSetUploadStatus.SCREENING)
                .WithScreenerProgress(
                    new DataSetScreenerProgress
                    {
                        Completed = false,
                        Passed = false,
                        PercentageComplete = 50,
                        Stage = "validation",
                        // Recently updated, so no need for another progress update yet.
                        LastUpdated = utcNow.AddSeconds(-4),
                    }
                );

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.DataSetUploads.AddRangeAsync(dataSetUndergoingScreening);
                await context.SaveChangesAsync();
            }

            var screenerClient = new Mock<IDataSetScreenerClient>(MockBehavior.Strict);

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildService(
                    screenerClient: screenerClient.Object,
                    contentDbContext: context,
                    timeProvider: new FakeTimeProvider(utcNow)
                );

                // Act
                var result = await service.UpdateScreeningProgress(CancellationToken.None);

                // Assert
                screenerClient.VerifyNoOtherCalls();

                Assert.Equal([], result);
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var updatedDataSetUpload = context.DataSetUploads.Single();

                // Expect no updates to progress.
                Assert.Equal(dataSetUndergoingScreening.ScreenerProgress, updatedDataSetUpload.ScreenerProgress);
            }
        }

        [Fact]
        public async Task DataSetNotBeingScreened_NoProgressUpdateNeededIfNotScreening()
        {
            // Arrange
            DataSetUpload dataSetUndergoingScreening = _dataFixture
                .DefaultDataSetUpload()
                // Not currently being screened.
                .WithStatus(DataSetUploadStatus.PENDING_REVIEW);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.DataSetUploads.AddRangeAsync(dataSetUndergoingScreening);
                await context.SaveChangesAsync();
            }

            var screenerClient = new Mock<IDataSetScreenerClient>(MockBehavior.Strict);

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildService(screenerClient: screenerClient.Object, contentDbContext: context);

                // Act
                var result = await service.UpdateScreeningProgress(CancellationToken.None);

                // Assert
                screenerClient.VerifyNoOtherCalls();

                Assert.Equal([], result);
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var updatedDataSetUpload = context.DataSetUploads.Single();

                // Expect no updates to progress.
                Assert.Equal(dataSetUndergoingScreening.ScreenerProgress, updatedDataSetUpload.ScreenerProgress);
            }
        }

        [Fact]
        public async Task DataSetsBeingScreened_NeedProgressUpdate_OneProgressUpdateNotInResponse_NoUpdate()
        {
            var utcNow = DateTimeOffset.UtcNow;

            // Arrange
            var dataSetsUndergoingScreening = _dataFixture
                .DefaultDataSetUpload()
                .WithStatus(DataSetUploadStatus.SCREENING)
                .GenerateList(2);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.DataSetUploads.AddRangeAsync(dataSetsUndergoingScreening);
                await context.SaveChangesAsync();
            }

            var dataSetIds = dataSetsUndergoingScreening.Select(u => u.Id).ToList();

            // Only include a progress update for one of the requested data sets.
            List<DataSetScreenerProgressResponse> progressResponse =
            [
                new()
                {
                    DataSetId = dataSetIds[1],
                    PercentageComplete = 100.00,
                    Completed = true,
                    Passed = true,
                    Stage = "complete",
                },
            ];

            var screenerClient = new Mock<IDataSetScreenerClient>(MockBehavior.Strict);

            screenerClient
                .Setup(s => s.GetScreenerProgress(dataSetIds, CancellationToken.None))
                .ReturnsAsync(progressResponse);

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildService(
                    screenerClient: screenerClient.Object,
                    contentDbContext: context,
                    timeProvider: new FakeTimeProvider(utcNow)
                );

                // Act
                var result = await service.UpdateScreeningProgress(CancellationToken.None);

                // Assert
                screenerClient.VerifyAll();

                Assert.Equal(progressResponse, result);
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var updatedDataSetUploads = context.DataSetUploads.ToList();

                var dataSetUpload1 = updatedDataSetUploads[0];

                // Assert that no progress update has been received for this data set yet, but it has received
                // some default values and a LastUpdated date.
                var expectedDataSet1ScreenerProgress = new DataSetScreenerProgress
                {
                    PercentageComplete = 0,
                    Completed = false,
                    Passed = false,
                    Stage = "",
                    LastUpdated = utcNow,
                };

                Assert.Equal(expectedDataSet1ScreenerProgress, dataSetUpload1.ScreenerProgress);

                var dataSetUpload2 = updatedDataSetUploads[1];

                var expectedDataSet2ScreenerProgress = new DataSetScreenerProgress
                {
                    PercentageComplete = 100,
                    Completed = true,
                    Passed = true,
                    Stage = "complete",
                    LastUpdated = utcNow,
                };

                // Assert that the data set that did receive a progress update has had its progress updated.
                Assert.Equal(expectedDataSet2ScreenerProgress, dataSetUpload2.ScreenerProgress);
            }
        }
    }

    private DataSetScreenerService BuildService(
        IDataSetScreenerClient? screenerClient = null,
        IQueueServiceClient? queueServiceClient = null,
        ContentDbContext? contentDbContext = null,
        TimeProvider? timeProvider = null
    )
    {
        return new DataSetScreenerService(
            dataSetScreenerClient: screenerClient ?? Mock.Of<IDataSetScreenerClient>(MockBehavior.Strict),
            queueServiceClient: queueServiceClient ?? Mock.Of<IQueueServiceClient>(MockBehavior.Strict),
            contentDbContext: contentDbContext ?? Mock.Of<ContentDbContext>(),
            timeProvider: timeProvider ?? TimeProvider.System,
            options: new DataScreenerOptions { ScreenerProgressUpdateIntervalSeconds = 5 }.ToOptionsWrapper()
        );
    }
}
