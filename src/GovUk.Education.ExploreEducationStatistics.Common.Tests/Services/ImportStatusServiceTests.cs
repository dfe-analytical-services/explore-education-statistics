using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.IStatus;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Services
{
    public class ImportStatusServiceTests
    {
        private readonly StorageException _concurrentUpdateException = new StorageException(new RequestResult
        {
            HttpStatusCode = 412
        }, "Precondition failure as expected while updating progress. ETag does not match for update", null);

        private readonly StorageException _genericStorageException = new StorageException(new RequestResult
        {
            HttpStatusCode = 400
        }, "Error", null);
        
        private static readonly List<IStatus> _finishedStatuses = new List<IStatus>
        {
            COMPLETE,
            FAILED,
            NOT_FOUND,
            CANCELLED,
        };
        
        private static readonly List<IStatus> _abortingStatuses = new List<IStatus>
        {
            CANCELLING
        };

        private static readonly List<IStatus> _finishedAndAbortingStatuses = 
            _finishedStatuses.Concat(_abortingStatuses).ToList();

        private readonly Guid _releaseId = Guid.NewGuid();

        private readonly Expression<Func<TableOperation, bool>> _tableReplaceExpression = operation =>
            operation.OperationType == TableOperationType.Replace;

        private readonly Expression<Func<TableOperation, bool>> _tableMergeExpression = operation =>
            operation.OperationType == TableOperationType.Merge;

        private const string FileName = "data.csv";

        [Fact]
        public async Task GetImportStatus_Stage1()
        {
            var tableStorageService = new Mock<ITableStorageService>(MockBehavior.Strict);

            const int percentageComplete = 50;

            SetupImportsTableMockForDataFileImport(tableStorageService: tableStorageService,
                importStatus: STAGE_1,
                percentageComplete: percentageComplete);

            var service = BuildImportStatusService(tableStorageService: tableStorageService.Object);
            var result = await service.GetImportStatus(_releaseId, FileName);

            Assert.Equal(STAGE_1, result.Status);
            Assert.Equal(percentageComplete * 0.1, result.PercentageComplete);
            Assert.Equal(percentageComplete, result.PhasePercentageComplete);
            Assert.Null(result.Errors);
            Assert.Equal(100, result.NumberOfRows);
        }

        [Fact]
        public async Task GetImportStatus_Stage2()
        {
            var tableStorageService = new Mock<ITableStorageService>(MockBehavior.Strict);

            const int percentageComplete = 50;

            SetupImportsTableMockForDataFileImport(tableStorageService: tableStorageService,
                importStatus: STAGE_2,
                percentageComplete: percentageComplete);

            var service = BuildImportStatusService(tableStorageService: tableStorageService.Object);
            var result = await service.GetImportStatus(_releaseId, FileName);

            Assert.Equal(STAGE_2, result.Status);
            Assert.Equal(100 * 0.1 + percentageComplete * 0.1, result.PercentageComplete);
            Assert.Equal(percentageComplete, result.PhasePercentageComplete);
            Assert.Null(result.Errors);
            Assert.Equal(100, result.NumberOfRows);
        }

        [Fact]
        public async Task GetImportStatus_Stage3()
        {
            var tableStorageService = new Mock<ITableStorageService>(MockBehavior.Strict);

            const int percentageComplete = 50;

            SetupImportsTableMockForDataFileImport(tableStorageService: tableStorageService,
                importStatus: STAGE_3,
                percentageComplete: percentageComplete);

            var service = BuildImportStatusService(tableStorageService: tableStorageService.Object);
            var result = await service.GetImportStatus(_releaseId, FileName);

            Assert.Equal(STAGE_3, result.Status);
            Assert.Equal(100 * 0.1 + 100 * 0.1 + percentageComplete * 0.1, result.PercentageComplete);
            Assert.Equal(percentageComplete, result.PhasePercentageComplete);
            Assert.Null(result.Errors);
            Assert.Equal(100, result.NumberOfRows);
        }

        [Fact]
        public async Task GetImportStatus_Stage4()
        {
            var tableStorageService = new Mock<ITableStorageService>(MockBehavior.Strict);

            const int percentageComplete = 50;

            SetupImportsTableMockForDataFileImport(tableStorageService: tableStorageService,
                importStatus: STAGE_4,
                percentageComplete: percentageComplete);

            var service = BuildImportStatusService(tableStorageService: tableStorageService.Object);
            var result = await service.GetImportStatus(_releaseId, FileName);

            Assert.Equal(STAGE_4, result.Status);
            Assert.Equal(100 * 0.1 + 100 * 0.1 + 100 * 0.1 + percentageComplete * 0.7, result.PercentageComplete);
            Assert.Equal(percentageComplete, result.PhasePercentageComplete);
            Assert.Null(result.Errors);
            Assert.Equal(100, result.NumberOfRows);
        }

        [Fact]
        public async Task GetImportStatus_Complete()
        {
            var tableStorageService = new Mock<ITableStorageService>(MockBehavior.Strict);

            const int percentageComplete = 50;

            SetupImportsTableMockForDataFileImport(tableStorageService: tableStorageService,
                importStatus: COMPLETE,
                percentageComplete: percentageComplete);

            var service = BuildImportStatusService(tableStorageService: tableStorageService.Object);
            var result = await service.GetImportStatus(_releaseId, FileName);

            Assert.Equal(COMPLETE, result.Status);
            Assert.Equal(100, result.PercentageComplete);
            Assert.Equal(percentageComplete, result.PhasePercentageComplete);
            Assert.Null(result.Errors);
            Assert.Equal(100, result.NumberOfRows);
        }

        [Fact]
        public async Task GetImportStatus_Failed()
        {
            var tableStorageService = new Mock<ITableStorageService>(MockBehavior.Strict);

            const int percentageComplete = 50;

            SetupImportsTableMockForDataFileImport(tableStorageService: tableStorageService,
                importStatus: FAILED,
                percentageComplete: percentageComplete,
                errors: "Error message");

            var service = BuildImportStatusService(tableStorageService: tableStorageService.Object);
            var result = await service.GetImportStatus(_releaseId, FileName);

            Assert.Equal(FAILED, result.Status);
            Assert.Equal(0, result.PercentageComplete);
            Assert.Equal(percentageComplete, result.PhasePercentageComplete);
            Assert.Equal("Error message", result.Errors);
            Assert.Equal(100, result.NumberOfRows);
        }

        [Fact]
        public async Task GetImportStatus_NotFound()
        {
            var tableStorageService = new Mock<ITableStorageService>(MockBehavior.Strict);

            SetupImportsTableMockForDataFileNotFound(tableStorageService);

            var service = BuildImportStatusService(tableStorageService: tableStorageService.Object);

            var result = await service.GetImportStatus(_releaseId, FileName);

            Assert.Equal(NOT_FOUND, result.Status);
            Assert.Equal(0, result.PercentageComplete);
            Assert.Equal(0, result.PhasePercentageComplete);
            Assert.Null(result.Errors);
            Assert.Equal(0, result.NumberOfRows);
        }

        [Fact]
        public async Task IsImportFinished_TrueWhenComplete()
        {
            var tableStorageService = new Mock<ITableStorageService>(MockBehavior.Strict);

            SetupImportsTableMockForDataFileImport(tableStorageService: tableStorageService,
                importStatus: COMPLETE,
                percentageComplete: 50);

            var service = BuildImportStatusService(tableStorageService: tableStorageService.Object);
            Assert.True(await service.IsImportFinished(_releaseId, FileName));
        }

        [Fact]
        public async Task IsImportFinished_TrueFailed()
        {
            var tableStorageService = new Mock<ITableStorageService>(MockBehavior.Strict);

            SetupImportsTableMockForDataFileImport(tableStorageService: tableStorageService,
                importStatus: FAILED,
                percentageComplete: 50,
                errors: "Error message");

            var service = BuildImportStatusService(tableStorageService: tableStorageService.Object);
            Assert.True(await service.IsImportFinished(_releaseId, FileName));
        }

        [Fact]
        public async Task IsImportFinished_TrueWhenNotFound()
        {
            var tableStorageService = new Mock<ITableStorageService>(MockBehavior.Strict);

            SetupImportsTableMockForDataFileNotFound(tableStorageService);

            var service = BuildImportStatusService(tableStorageService: tableStorageService.Object);
            Assert.True(await service.IsImportFinished(_releaseId, FileName));
        }

        [Fact]
        public async Task IsImportFinished_FalseWhenInProgress()
        {
            var tableStorageService = new Mock<ITableStorageService>(MockBehavior.Strict);

            SetupImportsTableMockForDataFileImport(tableStorageService: tableStorageService,
                importStatus: STAGE_1,
                percentageComplete: 50);

            var service = BuildImportStatusService(tableStorageService: tableStorageService.Object);
            Assert.False(await service.IsImportFinished(_releaseId, FileName));
        }

        [Fact]
        public async Task UpdateStatus_UpdateWhenAlreadyFinishedOrAbortingIsIgnored()
        {
            await _finishedStatuses.ForEachAsync(async status =>
            {
                var tableStorageService = new Mock<ITableStorageService>(MockBehavior.Strict);

                var importsTable = SetupImportsTableMockForDataFileImport(tableStorageService: tableStorageService,
                    importStatus: status,
                    percentageComplete: 100);

                var service = BuildImportStatusService(tableStorageService: tableStorageService.Object);

                await service.UpdateStatus(_releaseId, FileName, STAGE_1);

                importsTable.Verify(mock =>
                    mock.ExecuteAsync(It.Is<TableOperation>(operation =>
                        operation.OperationType == TableOperationType.Retrieve)), Times.Once);

                importsTable.VerifyNoOtherCalls();
            });
        }

        [Fact]
        public async Task UpdateStatus_UpdateNormalStateToFinishedOrAbortingStatuses_AlwaysSucceeds()
        {
            await _finishedAndAbortingStatuses.ForEachAsync(async status =>
            {
                var tableStorageService = new Mock<ITableStorageService>(MockBehavior.Strict);

                var importsTable = SetupImportsTableMockForDataFileImport(tableStorageService: tableStorageService,
                    importStatus: STAGE_4,
                    percentageComplete: 0);

                importsTable.Setup(mock => mock.ExecuteAsync(It.Is(_tableMergeExpression)))
                    .ReturnsAsync(new TableResult
                    {
                        Result = null
                    });

                var service = BuildImportStatusService(tableStorageService: tableStorageService.Object);

                await service.UpdateStatus(_releaseId, FileName, status, 100);

                importsTable.Verify(mock =>
                    mock.ExecuteAsync(It.Is<TableOperation>(operation =>
                        operation.OperationType == TableOperationType.Retrieve)), Times.Once);

                importsTable.Verify(mock =>
                    mock.ExecuteAsync(It.Is<TableOperation>(operation =>
                        operation.OperationType == TableOperationType.Merge
                        && (operation.Entity as DatafileImport).PercentageComplete == 100
                        && (operation.Entity as DatafileImport).Status == status
                        && (operation.Entity.ETag == "*"))), Times.Once);

                importsTable.VerifyNoOtherCalls();
            });
        }
        
        /**
         * Test the special case whereby a terminal status can overwrite an aborting status
         */
        [Fact]
        public async Task UpdateStatus_UpdateAbortingStateToFinishedState_AlwaysSucceeds()
        {
            await _abortingStatuses.ForEachAsync(async abortingState =>
            {
                await _finishedStatuses.ForEachAsync(async finishedState =>
                {
                    var tableStorageService = new Mock<ITableStorageService>(MockBehavior.Strict);

                    var importsTable = SetupImportsTableMockForDataFileImport(tableStorageService,
                        abortingState,
                        90);
            
                    importsTable.Setup(mock => mock.ExecuteAsync(It.Is(_tableMergeExpression)))
                        .ReturnsAsync(new TableResult
                        {
                            Result = null
                        });

                    var service = BuildImportStatusService(tableStorageService: tableStorageService.Object);

                    await service.UpdateStatus(_releaseId, FileName, finishedState, 100);

                    importsTable.Verify(mock =>
                        mock.ExecuteAsync(It.Is<TableOperation>(operation =>
                            operation.OperationType == TableOperationType.Retrieve)), Times.Once);

                    importsTable.Verify(mock =>
                        mock.ExecuteAsync(It.Is<TableOperation>(operation =>
                            operation.OperationType == TableOperationType.Merge
                            && (operation.Entity as DatafileImport).PercentageComplete == 100
                            && (operation.Entity as DatafileImport).Status == finishedState
                            && (operation.Entity.ETag == "*"))), Times.Once);

                    importsTable.VerifyNoOtherCalls();
                });
            });
        }
        
        [Fact]
        public async Task UpdateStatus_UpdateFinishedStatesToAbortingStates_AlwaysIgnored()
        {
            await _abortingStatuses.ForEachAsync(async abortingState =>
            {
                await _finishedStatuses.ForEachAsync(async finishedState =>
                {
                    var tableStorageService = new Mock<ITableStorageService>(MockBehavior.Strict);

                    var importsTable = SetupImportsTableMockForDataFileImport(tableStorageService,
                        finishedState,
                        90);
            
                    var service = BuildImportStatusService(tableStorageService: tableStorageService.Object);

                    await service.UpdateStatus(_releaseId, FileName, abortingState, 100);

                    importsTable.Verify(mock =>
                        mock.ExecuteAsync(It.Is<TableOperation>(operation =>
                            operation.OperationType == TableOperationType.Retrieve)), Times.Once);

                    importsTable.VerifyNoOtherCalls();
                });
            });
        }

        [Fact]
        public async Task UpdateStatus_UpdateToLowerStatusIsIgnored()
        {
            var tableStorageService = new Mock<ITableStorageService>(MockBehavior.Strict);

            var importsTable = SetupImportsTableMockForDataFileImport(tableStorageService: tableStorageService,
                importStatus: STAGE_4,
                percentageComplete: 50);

            var service = BuildImportStatusService(tableStorageService: tableStorageService.Object);

            await service.UpdateStatus(_releaseId, FileName, STAGE_3);

            importsTable.Verify(mock =>
                mock.ExecuteAsync(It.Is<TableOperation>(operation =>
                    operation.OperationType == TableOperationType.Retrieve)), Times.Once);

            importsTable.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task UpdateStatus_UpdateToLowerPercentageCompleteAtSameStatusIsIgnored()
        {
            var tableStorageService = new Mock<ITableStorageService>(MockBehavior.Strict);

            var importsTable = SetupImportsTableMockForDataFileImport(tableStorageService: tableStorageService,
                importStatus: STAGE_1,
                percentageComplete: 50);

            var service = BuildImportStatusService(tableStorageService: tableStorageService.Object);

            await service.UpdateStatus(_releaseId, FileName, STAGE_1, 25.0);

            importsTable.Verify(mock =>
                mock.ExecuteAsync(It.Is<TableOperation>(operation =>
                    operation.OperationType == TableOperationType.Retrieve)), Times.Once);

            importsTable.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task UpdateStatus_UpdateToSamePercentageCompleteAtSameStatusIsIgnored()
        {
            var tableStorageService = new Mock<ITableStorageService>(MockBehavior.Strict);

            var importsTable = SetupImportsTableMockForDataFileImport(tableStorageService: tableStorageService,
                importStatus: STAGE_2,
                percentageComplete: 50);

            var service = BuildImportStatusService(tableStorageService: tableStorageService.Object);

            await service.UpdateStatus(_releaseId, FileName, STAGE_2, 50);

            importsTable.Verify(mock =>
                mock.ExecuteAsync(It.Is<TableOperation>(operation =>
                    operation.OperationType == TableOperationType.Retrieve)), Times.Once);

            importsTable.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task UpdateStatus_UpdateToGreaterStatus()
        {
            var tableStorageService = new Mock<ITableStorageService>(MockBehavior.Strict);

            var importsTable = SetupImportsTableMockForDataFileImport(tableStorageService: tableStorageService,
                importStatus: STAGE_1,
                percentageComplete: 100);

            importsTable.Setup(mock => mock.ExecuteAsync(It.Is(_tableReplaceExpression)))
                .ReturnsAsync(new TableResult
                {
                    Result = null
                });

            var service = BuildImportStatusService(tableStorageService: tableStorageService.Object);

            await service.UpdateStatus(_releaseId, FileName, STAGE_2);

            importsTable.Verify(mock =>
                mock.ExecuteAsync(It.Is<TableOperation>(operation =>
                    operation.OperationType == TableOperationType.Retrieve)), Times.Once);

            importsTable.Verify(mock =>
                mock.ExecuteAsync(It.Is<TableOperation>(operation =>
                    operation.OperationType == TableOperationType.Replace
                    && (operation.Entity as DatafileImport).PercentageComplete == 0
                    && (operation.Entity as DatafileImport).Status == STAGE_2)), Times.Once);

            importsTable.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task UpdateStatus_PercentageCompleteExceedsUpperBound()
        {
            var tableStorageService = new Mock<ITableStorageService>(MockBehavior.Strict);

            var importsTable = SetupImportsTableMockForDataFileImport(tableStorageService: tableStorageService,
                importStatus: STAGE_1,
                percentageComplete: 0);

            importsTable.Setup(mock => mock.ExecuteAsync(It.Is(_tableReplaceExpression)))
                .ReturnsAsync(new TableResult
                {
                    Result = null
                });

            var service = BuildImportStatusService(tableStorageService: tableStorageService.Object);

            await service.UpdateStatus(_releaseId, FileName, STAGE_1, 101.0);

            importsTable.Verify(mock =>
                mock.ExecuteAsync(It.Is<TableOperation>(operation =>
                    operation.OperationType == TableOperationType.Retrieve)), Times.Once);

            importsTable.Verify(mock =>
                mock.ExecuteAsync(It.Is<TableOperation>(operation =>
                    operation.OperationType == TableOperationType.Replace
                    && (operation.Entity as DatafileImport).PercentageComplete == 100
                    && (operation.Entity as DatafileImport).Status == STAGE_1)), Times.Once);

            importsTable.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task UpdateStatus_UpdateThrowsException()
        {
            var tableStorageService = new Mock<ITableStorageService>(MockBehavior.Strict);

            var importsTable = SetupImportsTableMockForDataFileImport(tableStorageService: tableStorageService,
                importStatus: STAGE_1,
                percentageComplete: 100);

            importsTable.Setup(mock => mock.ExecuteAsync(It.Is(_tableReplaceExpression)))
                .ThrowsAsync(_genericStorageException);

            var service = BuildImportStatusService(tableStorageService: tableStorageService.Object);

            await Assert.ThrowsAsync<StorageException>(() => service.UpdateStatus(_releaseId, FileName, STAGE_2));

            importsTable.Verify(mock =>
                mock.ExecuteAsync(It.Is<TableOperation>(operation =>
                    operation.OperationType == TableOperationType.Retrieve)), Times.Once);

            importsTable.Verify(mock =>
                mock.ExecuteAsync(It.Is<TableOperation>(operation =>
                    operation.OperationType == TableOperationType.Replace
                    && (operation.Entity as DatafileImport).PercentageComplete == 0
                    && (operation.Entity as DatafileImport).Status == STAGE_2)), Times.Once);

            importsTable.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task UpdateStatus_UpdateIsIgnoredIfImportWasChangedByConcurrentUpdate()
        {
            var tableStorageService = new Mock<ITableStorageService>(MockBehavior.Strict);

            var importsTable = SetupImportsTableMockForDataFileImport(tableStorageService: tableStorageService,
                importStatus: STAGE_1,
                percentageComplete: 100);

            importsTable.SetupSequence(mock =>
                    mock.ExecuteAsync(It.Is(_tableReplaceExpression)))
                .ThrowsAsync(_concurrentUpdateException)
                .ReturnsAsync(new TableResult
                {
                    Result = null
                });

            var service = BuildImportStatusService(tableStorageService: tableStorageService.Object);

            await service.UpdateStatus(_releaseId, FileName, STAGE_2);

            importsTable.Verify(mock =>
                mock.ExecuteAsync(It.Is<TableOperation>(operation =>
                    operation.OperationType == TableOperationType.Retrieve)), Times.Once());

            importsTable.Verify(mock =>
                mock.ExecuteAsync(It.Is<TableOperation>(operation =>
                    operation.OperationType == TableOperationType.Replace
                    && (operation.Entity as DatafileImport).PercentageComplete == 0
                    && (operation.Entity as DatafileImport).Status == STAGE_2)), Times.Once());

            importsTable.VerifyNoOtherCalls();
        }

        private static Mock<CloudTable> SetupImportsTableMockForDataFileImport(
            Mock<ITableStorageService> tableStorageService,
            IStatus importStatus,
            int percentageComplete,
            string errors = null,
            int numberOfRows = 100)
        {
            return SetupImportsTableMockForDataFileImportResponse(tableStorageService, () => new TableResult
            {
                Result = new DatafileImport
                {
                    ETag = "*",
                    Errors = errors,
                    NumberOfRows = numberOfRows,
                    PercentageComplete = percentageComplete,
                    Status = importStatus
                }
            });
        }

        private static void SetupImportsTableMockForDataFileNotFound(Mock<ITableStorageService> tableStorageService)
        {
            SetupImportsTableMockForDataFileImportResponse(tableStorageService, () => new TableResult
            {
                Result = null
            });
        }

        private static Mock<CloudTable> SetupImportsTableMockForDataFileImportResponse(
            Mock<ITableStorageService> tableStorageService,
            Func<TableResult> responseFunc)
        {
            var importsTable = new Mock<CloudTable>(MockBehavior.Strict,
                new Uri("http://127.0.0.1:10002/devstoreaccount1/imports"),
                It.IsAny<TableClientConfiguration>());

            tableStorageService.Setup(mock =>
                mock.GetTableAsync("imports", true)).ReturnsAsync(importsTable.Object);

            importsTable.Setup(mock => mock.ExecuteAsync(It.Is<TableOperation>(operation =>
                    operation.OperationType == TableOperationType.Retrieve)))
                .ReturnsAsync(responseFunc.Invoke);

            return importsTable;
        }

        private static ImportStatusService BuildImportStatusService(
            ITableStorageService tableStorageService = null)
        {
            return new ImportStatusService(
                tableStorageService ?? new Mock<ITableStorageService>().Object,
                new Mock<ILogger<ImportStatusService>>().Object
            );
        }
    }
}