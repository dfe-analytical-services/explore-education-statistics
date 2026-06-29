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
using Microsoft.Extensions.Logging;
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
                .WithStatus(DataSetUploadScreeningStatus.Screening)
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
                    ProgressReport = new DataSetScreenerProgressReport
                    {
                        PercentageComplete = 100.00,
                        Completed = true,
                        Passed = true,
                        Stage = "complete",
                    },
                },
                new()
                {
                    DataSetId = dataSetIds[0],
                    ProgressReport = new DataSetScreenerProgressReport
                    {
                        PercentageComplete = 80.99,
                        Completed = false,
                        Passed = false,
                        Stage = "screening",
                    },
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
                    PercentageComplete = 80.99,
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
        public async Task DataSetBeingScreened_LastUpdatedRecently_NoNewProgressUpdateNeededYet()
        {
            var utcNow = DateTimeOffset.UtcNow;

            // Arrange
            DataSetUpload dataSetUndergoingScreening = _dataFixture
                .DefaultDataSetUpload()
                .WithStatus(DataSetUploadScreeningStatus.Screening)
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
                .WithStatus(DataSetUploadScreeningStatus.PendingReview);

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
        public async Task DataSetsBeingScreened_NeedProgressUpdate_OnlyOneProgressUpdateInResponse_OnlyCheckedDateUpdatesForMissingOnes()
        {
            var utcNow = DateTimeOffset.UtcNow;

            // Arrange
            var dataSetsUndergoingScreening = _dataFixture
                .DefaultDataSetUpload()
                .WithStatus(DataSetUploadScreeningStatus.Screening)
                .WithScreenerProgress(
                    new DataSetScreenerProgress
                    {
                        Completed = false,
                        Passed = false,
                        PercentageComplete = 0,
                        Stage = "Not started",
                    }
                )
                .WithScreenerProgressLastChecked(utcNow.AddSeconds(-ScreenerProgressUpdateIntervalSeconds))
                .WithScreenerProgressLastUpdated(utcNow.AddSeconds(-ScreenerProgressUpdateIntervalSeconds))
                .GenerateList(2);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.DataSetUploads.AddRangeAsync(dataSetsUndergoingScreening);
                await context.SaveChangesAsync();
            }

            var dataSetIds = dataSetsUndergoingScreening.Select(u => u.Id).ToList();

            // Only include a progress update for one of the two requested data sets.
            List<DataSetScreenerProgressResponse> progressResponse =
            [
                new()
                {
                    DataSetId = dataSetIds[1],
                    ProgressReport = new DataSetScreenerProgressReport
                    {
                        PercentageComplete = 100.00,
                        Completed = true,
                        Passed = true,
                        Stage = "Complete",
                    },
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

                // Assert that no progress update has been received for this data set yet and so it still has its same
                // progress as before.
                var expectedDataSet1ScreenerProgress = new DataSetScreenerProgress
                {
                    PercentageComplete = 0,
                    Completed = false,
                    Passed = false,
                    Stage = "Not started",
                };

                Assert.Equal(expectedDataSet1ScreenerProgress, dataSetUpload1.ScreenerProgress);

                // Expect the "last checked" date to be updated to show that Admin requested
                // a progress update for this data set.
                Assert.Equal(utcNow, dataSetUpload1.ScreenerProgressLastChecked);

                // Expect the "last updated" date to be left alone as it didn't successfully receive
                // a progress update this time.
                Assert.Equal(
                    dataSetsUndergoingScreening[0].ScreenerProgressLastUpdated,
                    dataSetUpload1.ScreenerProgressLastUpdated
                );

                var dataSetUpload2 = updatedDataSetUploads[1];

                var expectedDataSet2ScreenerProgress = new DataSetScreenerProgress
                {
                    PercentageComplete = 100,
                    Completed = true,
                    Passed = true,
                    Stage = "Complete",
                };

                // Assert that the data set that did receive a progress update has had its progress updated.
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
        public async Task DataSetsBeingScreened_NeedProgressUpdate_NoProgressChanged_OnlyCheckedDateUpdated()
        {
            var utcNow = DateTimeOffset.UtcNow;

            // Arrange
            DataSetUpload dataSetUndergoingScreening = _dataFixture
                .DefaultDataSetUpload()
                .WithStatus(DataSetUploadScreeningStatus.Screening)
                .WithScreenerProgress(
                    new DataSetScreenerProgress
                    {
                        Completed = false,
                        Passed = false,
                        PercentageComplete = 50,
                        Stage = "validation",
                    }
                )
                .WithScreenerProgressLastChecked(utcNow.AddSeconds(-ScreenerProgressUpdateIntervalSeconds))
                .WithScreenerProgressLastUpdated(utcNow.AddSeconds(-ScreenerProgressUpdateIntervalSeconds));

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.DataSetUploads.AddRangeAsync(dataSetUndergoingScreening);
                await context.SaveChangesAsync();
            }

            // Return a progress response with no updates since the last time it was checked.
            List<DataSetScreenerProgressResponse> progressResponse =
            [
                new()
                {
                    DataSetId = dataSetUndergoingScreening.Id,
                    ProgressReport = new DataSetScreenerProgressReport
                    {
                        PercentageComplete = dataSetUndergoingScreening.ScreenerProgress!.PercentageComplete,
                        Completed = dataSetUndergoingScreening.ScreenerProgress!.Completed,
                        Passed = dataSetUndergoingScreening.ScreenerProgress!.Passed,
                        Stage = dataSetUndergoingScreening.ScreenerProgress!.Stage,
                    },
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
                var updatedDataSetUploads = context.DataSetUploads.ToList();

                var dataSetUpload = updatedDataSetUploads.Single();

                // Assert that no progress fields have been updated because nothing changed since the last check.
                Assert.Equal(dataSetUndergoingScreening.ScreenerProgress, dataSetUpload.ScreenerProgress);

                // Expect the "last checked" date to be updated to show that Admin requested
                // a progress update for this data set.
                Assert.Equal(utcNow, dataSetUpload.ScreenerProgressLastChecked);

                // Expect the "last updated" date to be left unchanged, as no progress changes occurred since last
                // time it was updated.
                Assert.Equal(
                    dataSetUndergoingScreening.ScreenerProgressLastUpdated,
                    dataSetUpload.ScreenerProgressLastUpdated
                );
            }
        }
    }

    public class GetScreenerProgressTests : DataSetScreenerServiceTests
    {
        [Fact]
        public async Task DataSetWithScreenerProgress_ProgressUpdatesReturned()
        {
            // Arrange
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease());

            var dataSetsUndergoingScreening = _dataFixture
                .DefaultDataSetUpload()
                .WithReleaseVersionId(releaseVersion.Id)
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
                // Add an unrelated DataSetUpload.
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
                    dataSetUploadId: dataSetsUndergoingScreening[0].Id,
                    cancellationToken: CancellationToken.None
                );

                // Assert
                var expectedProgressUpdate = new ScreenerProgressViewModel
                {
                    PercentageComplete = (int)dataSetsUndergoingScreening[0].ScreenerProgress!.PercentageComplete,
                    Stage = dataSetsUndergoingScreening[0].ScreenerProgress!.Stage,
                    Completed = dataSetsUndergoingScreening[0].ScreenerProgress!.Completed,
                    Status = dataSetsUndergoingScreening[0].ScreeningStatus,
                };

                result.AssertRight(expectedProgressUpdate);
            }
        }

        [Fact]
        public async Task DataSetBeingScreened_NoProgressUpdatesYet_NullProgressUpdateReturned()
        {
            // Arrange
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease());

            DataSetUpload dataSetUndergoingScreening = _dataFixture
                .DefaultDataSetUpload()
                .WithReleaseVersionId(releaseVersion.Id)
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
                    dataSetUploadId: dataSetUndergoingScreening.Id,
                    cancellationToken: CancellationToken.None
                );

                // Assert
                var response = result.AssertRight();
                Assert.Null(response);
            }
        }

        [Fact]
        public async Task DataSetBeingScreened_ReleaseVersionAndDataSetUploadMismatch_NotFound()
        {
            // Arrange
            ReleaseVersion unrelatedReleaseVersion = _dataFixture.DefaultReleaseVersion();

            DataSetUpload dataSetUndergoingScreening = _dataFixture
                .DefaultDataSetUpload()
                // Assign these DataSetUploads to a different ReleaseVersion.
                .WithReleaseVersionId(Guid.NewGuid())
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
                    dataSetUploadId: dataSetUndergoingScreening.Id,
                    cancellationToken: CancellationToken.None
                );

                // Assert
                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task DataSetBeingScreened_ReleaseVersionNotFound_NotFound()
        {
            // Arrange
            DataSetUpload dataSetUndergoingScreening = _dataFixture
                .DefaultDataSetUpload()
                // Assign these DataSetUploads to a different ReleaseVersion.
                .WithReleaseVersionId(Guid.NewGuid())
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
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildService(contentDbContext: context);

                // Act
                var result = await service.GetScreenerProgress(
                    releaseVersionId: Guid.NewGuid(), // Non-existent ReleaseVersion id.
                    dataSetUploadId: dataSetUndergoingScreening.Id,
                    cancellationToken: CancellationToken.None
                );

                // Assert
                result.AssertNotFound();
            }
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
                .WithStatus(DataSetUploadScreeningStatus.Screening)
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
                            .SetScreenerProgressLastUpdated(dataSet2LastChecked.AddDays(-1))
                )
                .GenerateList();

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.DataSetUploads.AddRangeAsync(dataSetsUndergoingScreening);
                await context.SaveChangesAsync();
            }

            var dataSetIds = dataSetsUndergoingScreening.Select(upload => upload.Id).ToList();

            var screenerClient = new Mock<IDataSetScreenerClient>(MockBehavior.Strict);

            screenerClient
                .Setup(s => s.DeleteScreenerProgressAndCompletionFiles(dataSetIds, CancellationToken.None))
                .Returns(Task.CompletedTask);

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildService(contentDbContext: context, screenerClient: screenerClient.Object);

                // Act
                var results = await service.MarkDataSetsWithoutProgressAsFailed(CancellationToken.None);

                screenerClient.VerifyAll();

                Assert.Equal(2, results.Count);

                results.ForEach(result =>
                {
                    Assert.Equal(nameof(DataSetUploadScreeningStatus.ScreenerError), result.ScreeningStatus);
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
                    Assert.Equal(DataSetUploadScreeningStatus.ScreenerError, upload.ScreeningStatus);
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
                .WithStatus(DataSetUploadScreeningStatus.Screening)
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
                Assert.Equal(DataSetUploadScreeningStatus.Screening, upload.ScreeningStatus);

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
                .WithStatus(DataSetUploadScreeningStatus.PendingReview)
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
                Assert.Equal(DataSetUploadScreeningStatus.PendingReview, upload.ScreeningStatus);

                // Expect it not to have had any ScreenerResult applied.
                Assert.Null(upload.ScreenerResult);
            }
        }
    }

    public class CompleteDataSetScreeningForFinishedDataSetsTests : DataSetScreenerServiceTests
    {
        [Fact]
        public async Task DataSetsWithCompletedScreeningProgress_MarkedAsComplete()
        {
            // Arrange
            var dataSetsUndergoingScreening = _dataFixture
                .DefaultDataSetUpload()
                .WithStatus(DataSetUploadScreeningStatus.Screening)
                // The first data set has had a progress update that shows it has completed successfully.
                .ForIndex(
                    0,
                    s =>
                        s.SetScreenerProgress(
                            new DataSetScreenerProgress
                            {
                                Completed = true,
                                Passed = false,
                                PercentageComplete = 100,
                                Stage = "Completed",
                            }
                        )
                )
                // The second data set has had a progress update that shows it has not yet completed.
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
                // The third data set has had a progress update that shows it has completed, but not successfully.
                .ForIndex(
                    2,
                    s =>
                        s.SetScreenerProgress(
                            new DataSetScreenerProgress
                            {
                                Completed = true,
                                Passed = false,
                                PercentageComplete = 70,
                                Stage = "failed",
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

            // Return the completion reports out of order, to ensure they're matched up correctly with the
            // correct data sets by id.
            List<DataSetScreenerCompletionReportResponse> completionReportsResponse =
            [
                new()
                {
                    DataSetId = dataSetsUndergoingScreening[2].Id,
                    CompletionReport = new DataSetScreenerResponse
                    {
                        OverallResult = "Failed screening at stage 2",
                        Passed = false,
                        PublicApiCompatible = false,
                        TestResults =
                        [
                            new DataScreenerTestResult
                            {
                                Stage = "stage 2",
                                TestFunctionName = "a test",
                                Notes = "Some notes",
                                GuidanceUrl = "https://example.com",
                                Result = TestResult.FAIL,
                            },
                        ],
                    },
                },
                new()
                {
                    DataSetId = dataSetsUndergoingScreening[0].Id,
                    CompletionReport = new DataSetScreenerResponse
                    {
                        OverallResult = "Passed all tests",
                        Passed = true,
                        PublicApiCompatible = true,
                        TestResults =
                        [
                            new DataScreenerTestResult
                            {
                                Stage = "stage 2",
                                TestFunctionName = "a test",
                                Notes = "Some notes",
                                GuidanceUrl = "https://example.com",
                                Result = TestResult.PASS,
                            },
                            new DataScreenerTestResult
                            {
                                Stage = "stage 3",
                                TestFunctionName = "another test",
                                Notes = "Some notes",
                                GuidanceUrl = "https://example.com",
                                Result = TestResult.PASS,
                            },
                        ],
                    },
                },
            ];

            var screenerClient = new Mock<IDataSetScreenerClient>(MockBehavior.Strict);

            screenerClient
                .Setup(s =>
                    s.GetScreenerCompletionReports(
                        new[] { dataSetsUndergoingScreening[0].Id, dataSetsUndergoingScreening[2].Id },
                        CancellationToken.None
                    )
                )
                .ReturnsAsync(completionReportsResponse);

            screenerClient
                .Setup(s =>
                    s.DeleteScreenerProgressAndCompletionFiles(
                        new[] { dataSetsUndergoingScreening[0].Id, dataSetsUndergoingScreening[2].Id },
                        CancellationToken.None
                    )
                )
                .Returns(Task.CompletedTask);

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildService(screenerClient: screenerClient.Object, contentDbContext: context);

                // Act
                var results = await service.CompleteDataSetScreeningForFinishedDataSets(CancellationToken.None);

                // Assert
                screenerClient.VerifyAll();

                Assert.Equal(2, results.Count);

                Assert.Equal(dataSetsUndergoingScreening[0].Id, results[0].Id);
                var expectedDataSet1CompletionReport = completionReportsResponse[1].CompletionReport;
                Assert.Equal(expectedDataSet1CompletionReport.OverallResult, results[0].ScreenerResult!.OverallResult);

                // TODO EES-6693 - remove the additional automatic warning from the TestResults count once
                // the external screener is no longer required.
                Assert.Equal(
                    expectedDataSet1CompletionReport.TestResults.Count + 1,
                    results[0].ScreenerResult!.TestResults.Count
                );

                Assert.Equal(dataSetsUndergoingScreening[2].Id, results[1].Id);
                var expectedDataSet3CompletionReport = completionReportsResponse[0].CompletionReport;
                Assert.Equal(expectedDataSet3CompletionReport.OverallResult, results[1].ScreenerResult!.OverallResult);

                // TODO EES-6693 - remove the additional automatic warning from the TestResults count once
                // the external screener is no longer required.
                Assert.Equal(
                    expectedDataSet3CompletionReport.TestResults.Count + 1,
                    results[1].ScreenerResult!.TestResults.Count
                );
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var updatedDataSetUploads = context.DataSetUploads.ToList();

                // Expect that data set 1 had its completion report applied and status updated to
                // show that it passed screening successfully.
                var dataSetUpload1 = updatedDataSetUploads[0];

                // TODO EES-6693 - remove the additional automatic warning from the TestResults once
                // the external screener is no longer required.
                var expectedDataSet1CompletionReport = completionReportsResponse[1].CompletionReport with
                {
                    TestResults =
                    [
                        .. completionReportsResponse[1].CompletionReport.TestResults,
                        DataScreenerTestResult.ExternalScreenerWarningResult,
                    ],
                };
                expectedDataSet1CompletionReport.AssertDeepEqualTo(dataSetUpload1.ScreenerResult);
                Assert.Equal(DataSetUploadScreeningStatus.PendingReview, dataSetUpload1.ScreeningStatus);

                // Expect that data set 2 has no updates as it has not yet completed.
                var dataSetUpload2 = updatedDataSetUploads[1];
                Assert.Null(dataSetUpload2.ScreenerResult);
                Assert.Equal(DataSetUploadScreeningStatus.Screening, dataSetUpload2.ScreeningStatus);

                // Expect that data set 3 had its completion report applied and status updated to
                // show that it failed screening.
                var dataSetUpload3 = updatedDataSetUploads[2];
                // TODO EES-6693 - remove the additional automatic warning from the TestResults once
                // the external screener is no longer required.
                var expectedDataSet3CompletionReport = completionReportsResponse[0].CompletionReport with
                {
                    TestResults =
                    [
                        .. completionReportsResponse[0].CompletionReport.TestResults,
                        DataScreenerTestResult.ExternalScreenerWarningResult,
                    ],
                };

                expectedDataSet3CompletionReport.AssertDeepEqualTo(dataSetUpload3.ScreenerResult);
                Assert.Equal(DataSetUploadScreeningStatus.FailedScreening, dataSetUpload3.ScreeningStatus);
            }
        }

        [Fact]
        public async Task DataSetWithoutCompletionReport_Ignored_NotUpdated()
        {
            // Arrange
            DataSetUpload dataSetUndergoingScreening = _dataFixture
                .DefaultDataSetUpload()
                .WithStatus(DataSetUploadScreeningStatus.Screening)
                .WithScreenerProgress(
                    new DataSetScreenerProgress
                    {
                        Completed = true,
                        Passed = false,
                        PercentageComplete = 100,
                        Stage = "Completed",
                    }
                );

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.DataSetUploads.AddRangeAsync(dataSetUndergoingScreening);
                await context.SaveChangesAsync();
            }

            var screenerClient = new Mock<IDataSetScreenerClient>(MockBehavior.Strict);

            // Return no completion reports, to simulate an issue getting it successfully
            // from the Screener API.
            screenerClient
                .Setup(s =>
                    s.GetScreenerCompletionReports(new[] { dataSetUndergoingScreening.Id }, CancellationToken.None)
                )
                .ReturnsAsync([]);

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildService(screenerClient: screenerClient.Object, contentDbContext: context);

                // Act
                var results = await service.CompleteDataSetScreeningForFinishedDataSets(CancellationToken.None);

                // Assert
                screenerClient.VerifyAll();

                Assert.Empty(results);
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var dataSetUpload = context.DataSetUploads.Single();

                // Expect that the data set has been ignored.
                dataSetUpload.AssertDeepEqualTo(dataSetUndergoingScreening);
            }
        }

        [Fact]
        public async Task DataSetNotUndergoingScreening_Ignored_NotMarkedAsComplete()
        {
            // Arrange
            DataSetUpload dataSetUndergoingScreening = _dataFixture
                .DefaultDataSetUpload()
                .WithStatus(DataSetUploadScreeningStatus.PendingImport)
                .WithScreenerProgress(
                    new DataSetScreenerProgress
                    {
                        Completed = true,
                        Passed = true,
                        PercentageComplete = 100,
                        Stage = "complete",
                    }
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
                var results = await service.CompleteDataSetScreeningForFinishedDataSets(CancellationToken.None);

                Assert.Empty(results);
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                // ReSharper disable once EntityFramework.NPlusOne.IncompleteDataQuery
                var upload = context.DataSetUploads.Single();

                // Expect it to be in its original state.
                Assert.Equal(DataSetUploadScreeningStatus.PendingImport, upload.ScreeningStatus);

                // Expect it not to have had any ScreenerResult applied.
                Assert.Null(upload.ScreenerResult);
            }
        }

        [Fact]
        public async Task DataSetWithCompletedScreeningProgress_FailureToDeleteProgressFiles_MarkedAsComplete()
        {
            // Arrange
            DataSetUpload dataSetUndergoingScreening = _dataFixture
                .DefaultDataSetUpload()
                .WithStatus(DataSetUploadScreeningStatus.Screening)
                .WithScreenerProgress(
                    new DataSetScreenerProgress
                    {
                        Completed = true,
                        Passed = false,
                        PercentageComplete = 100,
                        Stage = "Completed",
                    }
                );

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.DataSetUploads.AddRangeAsync(dataSetUndergoingScreening);
                await context.SaveChangesAsync();
            }

            List<DataSetScreenerCompletionReportResponse> completionReportsResponse =
            [
                new()
                {
                    DataSetId = dataSetUndergoingScreening.Id,
                    CompletionReport = new DataSetScreenerResponse
                    {
                        OverallResult = "Failed screening at stage 2",
                        Passed = false,
                        PublicApiCompatible = false,
                        TestResults =
                        [
                            new DataScreenerTestResult
                            {
                                Stage = "stage 2",
                                TestFunctionName = "a test",
                                Notes = "Some notes",
                                GuidanceUrl = "https://example.com",
                                Result = TestResult.FAIL,
                            },
                        ],
                    },
                },
            ];

            var screenerClient = new Mock<IDataSetScreenerClient>(MockBehavior.Strict);

            screenerClient
                .Setup(s =>
                    s.GetScreenerCompletionReports(new[] { dataSetUndergoingScreening.Id }, CancellationToken.None)
                )
                .ReturnsAsync(completionReportsResponse);

            screenerClient
                .Setup(s =>
                    s.DeleteScreenerProgressAndCompletionFiles(
                        new[] { dataSetUndergoingScreening.Id },
                        CancellationToken.None
                    )
                )
                .ThrowsAsync(new Exception("Failed to delete progress files"));

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildService(screenerClient: screenerClient.Object, contentDbContext: context);

                // Act
                var results = await service.CompleteDataSetScreeningForFinishedDataSets(CancellationToken.None);

                // Assert
                screenerClient.VerifyAll();

                var result = Assert.Single(results);

                Assert.Equal(dataSetUndergoingScreening.Id, result.Id);
                var expectedDataSet1CompletionReport = completionReportsResponse[0].CompletionReport;
                Assert.Equal(expectedDataSet1CompletionReport.OverallResult, result.ScreenerResult!.OverallResult);

                // TODO EES-6693 - remove the additional automatic warning from the TestResults count once
                // the external screener is no longer required.
                Assert.Equal(
                    expectedDataSet1CompletionReport.TestResults.Count + 1,
                    result.ScreenerResult!.TestResults.Count
                );
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var updatedDataSetUpload = context.DataSetUploads.Single();

                // Expect that the data set has its completion report applied and status updated to
                // show that it passed screening successfully, despite the failure to clean up the
                // progress files.
                // TODO EES-6693 - remove the additional automatic warning from the TestResults once
                // the external screener is no longer required.
                var expectedDataSetCompletionReport = completionReportsResponse[0].CompletionReport with
                {
                    TestResults =
                    [
                        .. completionReportsResponse[0].CompletionReport.TestResults,
                        DataScreenerTestResult.ExternalScreenerWarningResult,
                    ],
                };
                expectedDataSetCompletionReport.AssertDeepEqualTo(updatedDataSetUpload.ScreenerResult);
                Assert.Equal(DataSetUploadScreeningStatus.FailedScreening, updatedDataSetUpload.ScreeningStatus);
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
            }.ToOptionsWrapper(),
            logger: Mock.Of<ILogger<DataSetScreenerService>>()
        );
    }
}
