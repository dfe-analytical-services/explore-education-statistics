using System;
using System.Threading.Tasks;
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
        private readonly Guid _releaseId = Guid.NewGuid();
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
        public async Task UpdateStatus_AlreadyComplete()
        {
            var tableStorageService = new Mock<ITableStorageService>(MockBehavior.Strict);

            var importsTable = SetupImportsTableMockForDataFileImport(tableStorageService: tableStorageService,
                importStatus: COMPLETE,
                percentageComplete: 100);

            var service = BuildImportStatusService(tableStorageService: tableStorageService.Object);
            
            Assert.True(await service.UpdateStatus(_releaseId, FileName, STAGE_1));

            importsTable.Verify(mock => 
                mock.ExecuteAsync(It.Is<TableOperation>(operation =>
                operation.OperationType == TableOperationType.Retrieve)), Times.Once);

            importsTable.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task UpdateStatus_AlreadyFailed()
        {
            var tableStorageService = new Mock<ITableStorageService>(MockBehavior.Strict);

            var importsTable = SetupImportsTableMockForDataFileImport(tableStorageService: tableStorageService,
                importStatus: FAILED,
                percentageComplete: 0);

            var service = BuildImportStatusService(tableStorageService: tableStorageService.Object);
            
            Assert.False(await service.UpdateStatus(_releaseId, FileName, STAGE_1));

            importsTable.Verify(mock => 
                mock.ExecuteAsync(It.Is<TableOperation>(operation =>
                    operation.OperationType == TableOperationType.Retrieve)), Times.Once);

            importsTable.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task UpdateStatus_UpdateToCompleteStatus()
        {
            var tableStorageService = new Mock<ITableStorageService>(MockBehavior.Strict);

            var importsTable = SetupImportsTableMockForDataFileImport(tableStorageService: tableStorageService,
                importStatus: STAGE_4,
                percentageComplete: 0);

            importsTable.Setup(mock => mock.ExecuteAsync(It.Is<TableOperation>(operation =>
                    operation.OperationType == TableOperationType.Replace)))
                .ReturnsAsync(new TableResult
                {
                    Result = null
                });

            var service = BuildImportStatusService(tableStorageService: tableStorageService.Object);

            Assert.True(await service.UpdateStatus(_releaseId, FileName, COMPLETE));

            importsTable.Verify(mock =>
                mock.ExecuteAsync(It.Is<TableOperation>(operation =>
                    operation.OperationType == TableOperationType.Retrieve)), Times.Once);

            // TODO ideally we want to verify the percentage complete in the replacement
            
            importsTable.Verify(mock =>
                mock.ExecuteAsync(It.Is<TableOperation>(operation =>
                    operation.OperationType == TableOperationType.Replace)), Times.Once);

            importsTable.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task UpdateStatus_UpdateToOtherStatus()
        {
            var tableStorageService = new Mock<ITableStorageService>(MockBehavior.Strict);

            var importsTable = SetupImportsTableMockForDataFileImport(tableStorageService: tableStorageService,
                importStatus: STAGE_1,
                percentageComplete: 0);

            importsTable.Setup(mock => mock.ExecuteAsync(It.Is<TableOperation>(operation =>
                    operation.OperationType == TableOperationType.Replace)))
                .ReturnsAsync(new TableResult
                {
                    Result = null
                });

            var service = BuildImportStatusService(tableStorageService: tableStorageService.Object);

            Assert.True(await service.UpdateStatus(_releaseId, FileName, STAGE_2));

            importsTable.Verify(mock =>
                mock.ExecuteAsync(It.Is<TableOperation>(operation =>
                    operation.OperationType == TableOperationType.Retrieve)), Times.Once);

            // TODO ideally we want to verify the percentage complete in the replacement
            
            importsTable.Verify(mock =>
                mock.ExecuteAsync(It.Is<TableOperation>(operation =>
                    operation.OperationType == TableOperationType.Replace)), Times.Once);

            importsTable.VerifyNoOtherCalls();
        }
        
        [Fact]
        public async Task UpdateStatus_UpdateAsSameStatus()
        {
            var tableStorageService = new Mock<ITableStorageService>(MockBehavior.Strict);

            var importsTable = SetupImportsTableMockForDataFileImport(tableStorageService: tableStorageService,
                importStatus: STAGE_1,
                percentageComplete: 0);

            var service = BuildImportStatusService(tableStorageService: tableStorageService.Object);
            
            Assert.True(await service.UpdateStatus(_releaseId, FileName, STAGE_1));

            importsTable.Verify(mock => 
                mock.ExecuteAsync(It.Is<TableOperation>(operation =>
                    operation.OperationType == TableOperationType.Retrieve)), Times.Once);

            importsTable.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task UpdateStatus_UpdateThrowsException()
        {
            var tableStorageService = new Mock<ITableStorageService>(MockBehavior.Strict);

            var importsTable = SetupImportsTableMockForDataFileImport(tableStorageService: tableStorageService,
                importStatus: STAGE_1,
                percentageComplete: 0);

            importsTable.Setup(mock => mock.ExecuteAsync(It.Is<TableOperation>(operation =>
                    operation.OperationType == TableOperationType.Replace)))
                .ThrowsAsync(new StorageException());

            var service = BuildImportStatusService(tableStorageService: tableStorageService.Object);

            Assert.True(await service.UpdateStatus(_releaseId, FileName, STAGE_2));

            importsTable.Verify(mock =>
                mock.ExecuteAsync(It.Is<TableOperation>(operation =>
                    operation.OperationType == TableOperationType.Retrieve)), Times.Once);

            importsTable.Verify(mock =>
                mock.ExecuteAsync(It.Is<TableOperation>(operation =>
                    operation.OperationType == TableOperationType.Replace)), Times.Once);

            importsTable.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task UpdateProgress()
        {
            var tableStorageService = new Mock<ITableStorageService>(MockBehavior.Strict);

            var importsTable = SetupImportsTableMockForDataFileImport(tableStorageService: tableStorageService,
                importStatus: STAGE_1,
                percentageComplete: 0);

            importsTable.Setup(mock => mock.ExecuteAsync(It.Is<TableOperation>(operation =>
                    operation.OperationType == TableOperationType.Replace)))
                .ReturnsAsync(new TableResult
                {
                    Result = null
                });

            var service = BuildImportStatusService(tableStorageService: tableStorageService.Object);

            await service.UpdateProgress(_releaseId, FileName, 50.0);

            importsTable.Verify(mock =>
                mock.ExecuteAsync(It.Is<TableOperation>(operation =>
                    operation.OperationType == TableOperationType.Retrieve)), Times.Once);

            // TODO ideally we want to verify the percentage complete in the replacement
            
            importsTable.Verify(mock =>
                mock.ExecuteAsync(It.Is<TableOperation>(operation =>
                    operation.OperationType == TableOperationType.Replace)), Times.Once);

            importsTable.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task UpdateProgress_PercentageCompleteGreaterThanUpdate()
        {
            var tableStorageService = new Mock<ITableStorageService>(MockBehavior.Strict);

            var importsTable = SetupImportsTableMockForDataFileImport(tableStorageService: tableStorageService,
                importStatus: STAGE_1,
                percentageComplete: 50);

            var service = BuildImportStatusService(tableStorageService: tableStorageService.Object);

            await service.UpdateProgress(_releaseId, FileName, 25.0);

            importsTable.Verify(mock =>
                mock.ExecuteAsync(It.Is<TableOperation>(operation =>
                    operation.OperationType == TableOperationType.Retrieve)), Times.Once);

            importsTable.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task UpdateProgress_UpdateExceedsUpperBound()
        {
            var tableStorageService = new Mock<ITableStorageService>(MockBehavior.Strict);

            var importsTable = SetupImportsTableMockForDataFileImport(tableStorageService: tableStorageService,
                importStatus: STAGE_1,
                percentageComplete: 0);

            importsTable.Setup(mock => mock.ExecuteAsync(It.Is<TableOperation>(operation =>
                    operation.OperationType == TableOperationType.Replace)))
                .ReturnsAsync(new TableResult
                {
                    Result = null
                });

            var service = BuildImportStatusService(tableStorageService: tableStorageService.Object);

            await service.UpdateProgress(_releaseId, FileName, 101.0);

            importsTable.Verify(mock =>
                mock.ExecuteAsync(It.Is<TableOperation>(operation =>
                    operation.OperationType == TableOperationType.Retrieve)), Times.Once);

            // TODO ideally we want to verify the percentage complete in the replacement
            
            importsTable.Verify(mock =>
                mock.ExecuteAsync(It.Is<TableOperation>(operation =>
                    operation.OperationType == TableOperationType.Replace)), Times.Once);
            
            importsTable.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task UpdateProgress_UpdateThrowsException()
        {
            var tableStorageService = new Mock<ITableStorageService>(MockBehavior.Strict);

            var importsTable = SetupImportsTableMockForDataFileImport(tableStorageService: tableStorageService,
                importStatus: STAGE_1,
                percentageComplete: 0);

            importsTable.Setup(mock => mock.ExecuteAsync(It.Is<TableOperation>(operation =>
                    operation.OperationType == TableOperationType.Replace)))
                .ThrowsAsync(new StorageException());

            var service = BuildImportStatusService(tableStorageService: tableStorageService.Object);

            await service.UpdateProgress(_releaseId, FileName, 50.0);

            importsTable.Verify(mock =>
                mock.ExecuteAsync(It.Is<TableOperation>(operation =>
                    operation.OperationType == TableOperationType.Retrieve)), Times.Once);

            importsTable.Verify(mock =>
                mock.ExecuteAsync(It.Is<TableOperation>(operation =>
                    operation.OperationType == TableOperationType.Replace)), Times.Once);

            importsTable.VerifyNoOtherCalls();
        }
        
        private static Mock<CloudTable> SetupImportsTableMockForDataFileImport(Mock<ITableStorageService> tableStorageService,
            IStatus importStatus,
            int percentageComplete,
            string errors = null,
            int numberOfRows = 100)
        {
            return SetupImportsTableMockForDataFileImportResponse(tableStorageService, new DatafileImport
            {
                ETag = "*",
                Errors = errors,
                NumberOfRows = numberOfRows,
                PercentageComplete = percentageComplete,
                Status = importStatus
            });
        }

        private static void SetupImportsTableMockForDataFileNotFound(Mock<ITableStorageService> tableStorageService)
        {
            SetupImportsTableMockForDataFileImportResponse(tableStorageService, null);
        }

        private static Mock<CloudTable> SetupImportsTableMockForDataFileImportResponse(Mock<ITableStorageService> tableStorageService,
            DatafileImport response)
        {
            var importsTable = new Mock<CloudTable>(MockBehavior.Strict, new Uri("http://127.0.0.1:10002/devstoreaccount1/imports"),
                It.IsAny<TableClientConfiguration>());

            tableStorageService.Setup(mock =>
                mock.GetTableAsync("imports", true)).ReturnsAsync(importsTable.Object);

            importsTable.Setup(mock => mock.ExecuteAsync(It.Is<TableOperation>(operation =>
                    operation.OperationType == TableOperationType.Retrieve)))
                .ReturnsAsync(new TableResult
                {
                    Result = response
                });

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