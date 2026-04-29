#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Responses.Screener;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Screener;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Screener;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Screener;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Screener;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.Extensions.Time.Testing;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Screener;

public abstract class DataSetScreenerServiceTests
{
    private const int ScreenerProgressUpdateIntervalSeconds = 5;
    private const int ScreenerProgressUpdateFailureIntervalMinutes = 10;

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
                                }
                            )
                            .SetScreenerProgressLastChecked(utcNow.AddSeconds(-ScreenerProgressUpdateIntervalSeconds))
                            .SetScreenerProgressLastUpdated(utcNow.AddSeconds(-ScreenerProgressUpdateIntervalSeconds))
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
                                }
                            )
                            .SetScreenerProgressLastChecked(
                                utcNow.AddSeconds(-ScreenerProgressUpdateIntervalSeconds - 1)
                            )
                            .SetScreenerProgressLastUpdated(
                                utcNow.AddSeconds(-ScreenerProgressUpdateIntervalSeconds - 1)
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
                };

                Assert.Equal(expectedDataSet1ScreenerProgress, dataSetUpload1.ScreenerProgress);

                // Expect the "last checked" date to be updated to show that Admin requested
                // a progress update for this data set.
                Assert.Equal(utcNow, dataSetUpload1.ScreenerProgressLastChecked);

                // Expect the "last updated" date to be updated to show that Admin successfully
                // received and applied a progress update for this data set.
                Assert.Equal(utcNow, dataSetUpload1.ScreenerProgressLastUpdated);

                var dataSetUpload2 = updatedDataSetUploads[1];

                var expectedDataSet2ScreenerProgress = dataSetsUndergoingScreening[1].ScreenerProgress! with
                {
                    PercentageComplete = 100,
                    Completed = true,
                    Passed = true,
                    Stage = "complete",
                };

                Assert.Equal(expectedDataSet2ScreenerProgress, dataSetUpload2.ScreenerProgress);

                // Expect the "last checked" date to be updated to show that Admin requested
                // a progress update for this data set.
                Assert.Equal(utcNow, dataSetUpload2.ScreenerProgressLastChecked);

                // Expect the "last updated" date to be updated to show that Admin successfully
                // received and applied a progress update for this data set.
                Assert.Equal(utcNow, dataSetUpload2.ScreenerProgressLastUpdated);
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
                .WithScreenerProgress(null)
                .WithScreenerProgressLastChecked(null)
                .WithScreenerProgressLastUpdated(null);

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
                };

                // Expect the progress to be updated.
                Assert.Equal(expectedDataSetScreenerProgress, updatedDataSetUpload.ScreenerProgress);

                // Expect the "last checked" date to be updated to show that Admin requested
                // a progress update for this data set.
                Assert.Equal(utcNow, updatedDataSetUpload.ScreenerProgressLastChecked);

                // Expect the "last updated" date to be updated to show that Admin successfully
                // received and applied a progress update for this data set.
                Assert.Equal(utcNow, updatedDataSetUpload.ScreenerProgressLastUpdated);
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
                    }
                )
                // Recently checked, so no need for another progress update yet.
                .WithScreenerProgressLastChecked(utcNow.AddSeconds(-ScreenerProgressUpdateIntervalSeconds + 1))
                .WithScreenerProgressLastUpdated(utcNow.AddSeconds(-ScreenerProgressUpdateIntervalSeconds - 5));

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

                // Expect no updates to progress or checked / updated dates.
                Assert.Equal(dataSetUndergoingScreening.ScreenerProgress, updatedDataSetUpload.ScreenerProgress);
                Assert.Equal(
                    dataSetUndergoingScreening.ScreenerProgressLastChecked,
                    updatedDataSetUpload.ScreenerProgressLastChecked
                );
                Assert.Equal(
                    dataSetUndergoingScreening.ScreenerProgressLastUpdated,
                    updatedDataSetUpload.ScreenerProgressLastUpdated
                );
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
                Assert.Null(updatedDataSetUpload.ScreenerProgress);
                Assert.Null(updatedDataSetUpload.ScreenerProgressLastChecked);
                Assert.Null(updatedDataSetUpload.ScreenerProgressLastUpdated);
            }
        }

        [Fact]
        public async Task DataSetsBeingScreened_NeedProgressUpdate_OnlyOneProgressUpdateInResponse_OnlyDateUpdatesForMissingOnes()
        {
            var utcNow = DateTimeOffset.UtcNow;

            // Arrange
            var dataSetsUndergoingScreening = _dataFixture
                .DefaultDataSetUpload()
                .WithStatus(DataSetUploadStatus.SCREENING)
                // For one of the data sets missing a progress update, mark it as having never been checked before
                .ForIndex(
                    0,
                    s =>
                        s.SetScreenerProgress(null)
                            .SetScreenerProgressLastChecked(null)
                            .SetScreenerProgressLastUpdated(null)
                )
                // For one of the data sets missing a progress update, mark it as having been checked at least once already.
                .ForIndex(
                    2,
                    s =>
                        s.SetScreenerProgress(new DataSetScreenerProgress { PercentageComplete = 10 })
                            .SetScreenerProgressLastChecked(utcNow.AddSeconds(-ScreenerProgressUpdateIntervalSeconds))
                            .SetScreenerProgressLastUpdated(utcNow.AddSeconds(-ScreenerProgressUpdateIntervalSeconds))
                )
                .GenerateList();

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
                // some default values and a LastChecked date. Because it's never been checked before, it also
                // receives a LastUpdated date as a once-off.
                var expectedDataSet1ScreenerProgress = new DataSetScreenerProgress
                {
                    PercentageComplete = 0,
                    Completed = false,
                    Passed = false,
                    Stage = "",
                };

                Assert.Equal(expectedDataSet1ScreenerProgress, dataSetUpload1.ScreenerProgress);

                // Expect the "last checked" date to be updated to show that Admin requested
                // a progress update for this data set.
                Assert.Equal(utcNow, dataSetUpload1.ScreenerProgressLastChecked);

                // Expect the "last updated" date to be set as a once-off for the first time
                // a progress check is carried out for this data set.
                Assert.Equal(utcNow, dataSetUpload1.ScreenerProgressLastUpdated);

                var dataSetUpload2 = updatedDataSetUploads[1];

                var expectedDataSet2ScreenerProgress = new DataSetScreenerProgress
                {
                    PercentageComplete = 100,
                    Completed = true,
                    Passed = true,
                    Stage = "complete",
                };

                // Assert that the data set that did receive a progress update has had its progress updated.
                Assert.Equal(expectedDataSet2ScreenerProgress, dataSetUpload2.ScreenerProgress);

                // Expect the "last checked" date to be updated to show that Admin requested
                // a progress update for this data set.
                Assert.Equal(utcNow, dataSetUpload2.ScreenerProgressLastChecked);

                // Expect the "last updated" date to be updated to show that Admin successfully
                // received and applied a progress update for this data set.
                Assert.Equal(utcNow, dataSetUpload2.ScreenerProgressLastUpdated);

                var dataSetUpload3 = updatedDataSetUploads[2];

                // Assert that no progress update has been received for this data set yet, so its progress
                // remains the same. Its "last checked" date will be updated to
                Assert.Equal(dataSetsUndergoingScreening[2].ScreenerProgress, dataSetUpload3.ScreenerProgress);

                // Expect the "last checked" date to be updated to show that Admin requested
                // a progress update for this data set.
                Assert.Equal(utcNow, dataSetUpload3.ScreenerProgressLastChecked);

                // Expect the "last updated" date to remain the same because its progress wasn't updated
                // during this call.
                Assert.Equal(
                    dataSetsUndergoingScreening[2].ScreenerProgressLastUpdated,
                    dataSetUpload3.ScreenerProgressLastUpdated
                );
            }
        }
    }

    public class GetScreenerProgressTests : DataSetScreenerServiceTests
    {
        [Fact]
        public async Task DataSetsBeingScreened_ProgressUpdatesReturned()
        {
            // Arrange
            ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion();

            var dataSetsUndergoingScreening = _dataFixture
                .DefaultDataSetUpload()
                .WithReleaseVersionId(releaseVersion.Id)
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
                            }
                        )
                )
                .GenerateList();

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.DataSetUploads.AddRangeAsync(dataSetsUndergoingScreening);
                await context.ReleaseVersions.AddRangeAsync(releaseVersion);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildService(contentDbContext: context);

                // Act
                var result = await service.GetScreenerProgress(
                    releaseVersionId: releaseVersion.Id,
                    cancellationToken: CancellationToken.None
                );

                // Assert
                List<ScreenerProgressWithDataSetUploadIdViewModel> expectedProgressUpdates =
                [
                    new()
                    {
                        DataSetUploadId = dataSetsUndergoingScreening[0].Id,
                        PercentageComplete = dataSetsUndergoingScreening[0].ScreenerProgress!.PercentageComplete,
                        Stage = dataSetsUndergoingScreening[0].ScreenerProgress!.Stage,
                    },
                    new()
                    {
                        DataSetUploadId = dataSetsUndergoingScreening[1].Id,
                        PercentageComplete = dataSetsUndergoingScreening[1].ScreenerProgress!.PercentageComplete,
                        Stage = dataSetsUndergoingScreening[1].ScreenerProgress!.Stage,
                    },
                ];

                result.AssertRight(expectedProgressUpdates);
            }
        }

        [Fact]
        public async Task DataSetBeingScreened_DifferentReleaseVersion_ProgressUpdatesNotReturned()
        {
            // Arrange
            ReleaseVersion unrelatedReleaseVersion = _dataFixture.DefaultReleaseVersion();

            DataSetUpload dataSetUndergoingScreening = _dataFixture
                .DefaultDataSetUpload()
                // Assign these DataSetUploads to a different ReleaseVersion.
                .WithReleaseVersionId(Guid.NewGuid())
                .WithStatus(DataSetUploadStatus.SCREENING)
                .WithScreenerProgress(
                    new DataSetScreenerProgress
                    {
                        Completed = false,
                        Passed = false,
                        PercentageComplete = 50,
                        Stage = "validation",
                    }
                );

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.DataSetUploads.AddRangeAsync(dataSetUndergoingScreening);
                await context.ReleaseVersions.AddRangeAsync(unrelatedReleaseVersion);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildService(contentDbContext: context);

                // Act
                var result = await service.GetScreenerProgress(
                    releaseVersionId: unrelatedReleaseVersion.Id,
                    cancellationToken: CancellationToken.None
                );

                // Assert
                result.AssertRight([]);
            }
        }

        [Fact]
        public async Task DataSetBeingScreened_NoProgressUpdatesYet_DefaultProgressUpdateReturned()
        {
            // Arrange
            ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion();

            DataSetUpload dataSetUndergoingScreening = _dataFixture
                .DefaultDataSetUpload()
                .WithReleaseVersionId(releaseVersion.Id)
                .WithStatus(DataSetUploadStatus.SCREENING)
                // No progress update has yet been fetched for this data set.
                .WithScreenerProgress(null);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.DataSetUploads.AddRangeAsync(dataSetUndergoingScreening);
                await context.ReleaseVersions.AddRangeAsync(releaseVersion);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildService(contentDbContext: context);

                // Act
                var result = await service.GetScreenerProgress(
                    releaseVersionId: releaseVersion.Id,
                    cancellationToken: CancellationToken.None
                );

                List<ScreenerProgressWithDataSetUploadIdViewModel> expectedProgressUpdates =
                [
                    new()
                    {
                        DataSetUploadId = dataSetUndergoingScreening.Id,
                        PercentageComplete = 0,
                        Stage = null,
                    },
                ];

                // Assert
                result.AssertRight(expectedProgressUpdates);
            }
        }

        [Fact]
        public async Task DataSetNotCurrentlyUndergoingScreening_NoProgressUpdateReturned()
        {
            // Arrange
            ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion();

            DataSetUpload dataSetNotUndergoingScreening = _dataFixture
                .DefaultDataSetUpload()
                .WithReleaseVersionId(releaseVersion.Id)
                // Not currently undergoing screening.
                .WithStatus(DataSetUploadStatus.PENDING_REVIEW);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.DataSetUploads.AddRangeAsync(dataSetNotUndergoingScreening);
                await context.ReleaseVersions.AddRangeAsync(releaseVersion);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildService(contentDbContext: context);

                // Act
                var result = await service.GetScreenerProgress(
                    releaseVersionId: releaseVersion.Id,
                    cancellationToken: CancellationToken.None
                );

                // Assert
                result.AssertRight([]);
            }
        }

        [Fact]
        public async Task ReleaseVersionDoesNotExist_ReturnsNotFound()
        {
            await using var context = InMemoryContentDbContext();
            var service = BuildService(contentDbContext: context);

            // Act
            var result = await service.GetScreenerProgress(
                releaseVersionId: Guid.NewGuid(),
                cancellationToken: CancellationToken.None
            );

            // Assert
            result.AssertNotFound();
        }
    }

    public class MarkDataSetsWithoutProgressAsFailedTests : DataSetScreenerServiceTests
    {
        [Fact]
        public async Task DataSetsWithoutRecentProgressUpdates_MarkedAsScreenerErrors()
        {
            // Arrange
            var dataSet1LastChecked = DateTimeOffset.UtcNow;
            var dataSet2LastChecked = DateTimeOffset.UtcNow.AddDays(-1);

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
                                }
                            )
                            .SetScreenerProgressLastChecked(dataSet1LastChecked)
                            .SetScreenerProgressLastUpdated(
                                dataSet1LastChecked.AddMinutes(-ScreenerProgressUpdateFailureIntervalMinutes)
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
                                }
                            )
                            .SetScreenerProgressLastChecked(dataSet2LastChecked)
                            .SetScreenerProgressLastUpdated(
                                dataSet2LastChecked.AddMinutes(-ScreenerProgressUpdateFailureIntervalMinutes - 1)
                            )
                )
                .GenerateList();

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.DataSetUploads.AddRangeAsync(dataSetsUndergoingScreening);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildService(contentDbContext: context);

                // Act
                var results = await service.MarkDataSetsWithoutProgressAsFailed(CancellationToken.None);

                Assert.Equal(2, results.Count);

                results.ForEach(result =>
                {
                    Assert.Equal(nameof(DataSetUploadStatus.FAILED_SCREENING), result.Status);
                    Assert.NotNull(result.ScreenerResult);
                    Assert.Equal("Failed to retrieve progress updates", result.ScreenerResult.OverallResult);
                    Assert.Empty(result.ScreenerResult.TestResults);
                });
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                // ReSharper disable once EntityFramework.NPlusOne.IncompleteDataQuery
                var updatedUploads = context.DataSetUploads.ToList();

                updatedUploads.ForEach(upload =>
                {
                    Assert.Equal(DataSetUploadStatus.SCREENER_ERROR, upload.Status);
                    Assert.NotNull(upload.ScreenerResult);
                    Assert.Equal("Failed to retrieve progress updates", upload.ScreenerResult.OverallResult);
                    Assert.False(upload.ScreenerResult.Passed);
                    Assert.False(upload.ScreenerResult.PublicApiCompatible);
                    // ReSharper disable once EntityFramework.NPlusOne.IncompleteDataUsage
                    Assert.Empty(upload.ScreenerResult.TestResults);
                });
            }
        }

        [Fact]
        public async Task DataSetWithRecentProgressUpdates_Ignored_NotMarkedAsScreenerErrors()
        {
            // Arrange
            var dataSetLastChecked = DateTimeOffset.UtcNow;

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
                    }
                )
                .WithScreenerProgressLastChecked(dataSetLastChecked)
                .WithScreenerProgressLastUpdated(
                    dataSetLastChecked
                        .AddMinutes(-ScreenerProgressUpdateFailureIntervalMinutes)
                        // Make the last updated date a little closer to the last
                        // checked date so that the interval isn't as large as
                        // ScreenerProgressUpdateFailureIntervalMinutes.
                        .AddSeconds(1)
                );

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.DataSetUploads.AddRangeAsync(dataSetUndergoingScreening);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildService(contentDbContext: context);

                // Act
                var results = await service.MarkDataSetsWithoutProgressAsFailed(CancellationToken.None);

                Assert.Empty(results);
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                // ReSharper disable once EntityFramework.NPlusOne.IncompleteDataQuery
                var upload = context.DataSetUploads.Single();

                // Expect it to be in its original state.
                Assert.Equal(DataSetUploadStatus.SCREENING, upload.Status);

                // Expect it not to have had any ScreenerResult applied.
                Assert.Null(upload.ScreenerResult);
            }
        }

        [Fact]
        public async Task DataSetNotUndergoingScreening_Ignored_NotMarkedAsScreenerErrors()
        {
            // Arrange
            var dataSetLastChecked = DateTimeOffset.UtcNow;

            DataSetUpload dataSetUndergoingScreening = _dataFixture
                .DefaultDataSetUpload()
                .WithStatus(DataSetUploadStatus.PENDING_REVIEW)
                .WithScreenerProgress(
                    new DataSetScreenerProgress
                    {
                        Completed = true,
                        Passed = true,
                        PercentageComplete = 100,
                        Stage = "complete",
                    }
                )
                .WithScreenerProgressLastChecked(dataSetLastChecked)
                .WithScreenerProgressLastUpdated(
                    dataSetLastChecked.AddMinutes(-ScreenerProgressUpdateFailureIntervalMinutes)
                );

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.DataSetUploads.AddRangeAsync(dataSetUndergoingScreening);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildService(contentDbContext: context);

                // Act
                var results = await service.MarkDataSetsWithoutProgressAsFailed(CancellationToken.None);

                Assert.Empty(results);
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                // ReSharper disable once EntityFramework.NPlusOne.IncompleteDataQuery
                var upload = context.DataSetUploads.Single();

                // Expect it to be in its original state.
                Assert.Equal(DataSetUploadStatus.PENDING_REVIEW, upload.Status);

                // Expect it not to have had any ScreenerResult applied.
                Assert.Null(upload.ScreenerResult);
            }
        }
    }

    private DataSetScreenerService BuildService(
        IDataSetScreenerClient? screenerClient = null,
        IQueueServiceClient? queueServiceClient = null,
        IUserService? userService = null,
        ContentDbContext? contentDbContext = null,
        TimeProvider? timeProvider = null
    )
    {
        return new DataSetScreenerService(
            dataSetScreenerClient: screenerClient ?? Mock.Of<IDataSetScreenerClient>(MockBehavior.Strict),
            queueServiceClient: queueServiceClient ?? Mock.Of<IQueueServiceClient>(MockBehavior.Strict),
            userService: userService ?? MockUtils.AlwaysTrueUserService().Object,
            contentDbContext: contentDbContext ?? Mock.Of<ContentDbContext>(),
            timeProvider: timeProvider ?? TimeProvider.System,
            mapper: MapperUtils.AdminMapper(),
            options: new DataScreenerOptions
            {
                ScreenerProgressUpdateIntervalSeconds = ScreenerProgressUpdateIntervalSeconds,
                ScreenerProgressUpdateFailureIntervalMinutes = ScreenerProgressUpdateFailureIntervalMinutes,
            }.ToOptionsWrapper()
        );
    }
}
