using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Tests.Functions
{
    public class ProcessorTests
    {
        [Fact]
        public void ProcessUploadsUnpackArchive()
        {
            var mocks = Mocks();
            var (processorService, importStatusService, batchService, fileImportService) = mocks;
            var importStagesMessageQueue = new Mock<ICollector<ImportMessage>>();
            var datafileProcessingMessageQueue = new Mock<ICollector<ImportObservationsMessage>>();

            var message = new ImportMessage
            {
                ArchiveFileName = "an_archive",
                Release = new Release
                {
                    Id = Guid.NewGuid()
                },
                DataFileName = "my_data_file",
            };

            var processor = new Processor.Functions.Processor(
                fileImportService.Object,
                batchService.Object,
                importStatusService.Object,
                processorService.Object,
                new Mock<ILogger<Processor.Functions.Processor>>().Object);

            processorService
                .Setup(s => s.ProcessUnpackingArchive(message))
                .Returns(Task.CompletedTask);

            importStatusService
                .Setup(s => s.GetImportStatus(message.Release.Id, message.DataFileName))
                .ReturnsAsync(new ImportStatus
                {
                    Status = IStatus.QUEUED
                });

            importStatusService
                .Setup(s => s.UpdateStatus(message.Release.Id, message.DataFileName, IStatus.STAGE_1, 0))
                .Returns(Task.CompletedTask);

            importStagesMessageQueue
                .Setup(s => s.Add(message));

            processor.ProcessUploads(
                message,
                null,
                importStagesMessageQueue.Object,
                datafileProcessingMessageQueue.Object
            );

            MockUtils.VerifyAllMocks(processorService, importStatusService, batchService,
                fileImportService, importStagesMessageQueue, datafileProcessingMessageQueue);
        }

        [Fact]
        public void ProcessUploadsUnpackArchiveWithNoArchive()
        {
            var mocks = Mocks();
            var (processorService, importStatusService, batchService, fileImportService) = mocks;
            var importStagesMessageQueue = new Mock<ICollector<ImportMessage>>();
            var datafileProcessingMessageQueue = new Mock<ICollector<ImportObservationsMessage>>();

            var message = new ImportMessage
            {
                ArchiveFileName = "",
                Release = new Release
                {
                    Id = Guid.NewGuid()
                },
                DataFileName = "my_data_file",
            };

            var processor = new Processor.Functions.Processor(
                fileImportService.Object,
                batchService.Object,
                importStatusService.Object,
                processorService.Object,
                new Mock<ILogger<Processor.Functions.Processor>>().Object);

            importStatusService
                .Setup(s => s.GetImportStatus(message.Release.Id, message.DataFileName))
                .ReturnsAsync(new ImportStatus
                {
                    Status = IStatus.QUEUED
                });

            importStatusService
                .Setup(s => s.UpdateStatus(message.Release.Id, message.DataFileName, IStatus.STAGE_1, 0))
                .Returns(Task.CompletedTask);

            importStagesMessageQueue
                .Setup(s => s.Add(message));

            processor.ProcessUploads(
                message,
                null,
                importStagesMessageQueue.Object,
                datafileProcessingMessageQueue.Object
            );

            MockUtils.VerifyAllMocks(processorService, importStatusService, batchService,
                fileImportService, importStagesMessageQueue, datafileProcessingMessageQueue);
        }

        [Fact]
        public void ProcessUploadsButImportIsFinished()
        {
            var finishedStates = EnumUtil
                .GetEnumValues<IStatus>()
                .Where(ImportStatus.IsFinishedState)
                .ToList();

            finishedStates.ForEach(currentState =>
            {
                var mocks = Mocks();
                var (processorService, importStatusService, batchService, fileImportService) = mocks;
                var importStagesMessageQueue = new Mock<ICollector<ImportMessage>>();
                var datafileProcessingMessageQueue = new Mock<ICollector<ImportObservationsMessage>>();

                var message = new ImportMessage
                {
                    ArchiveFileName = "an_archive",
                    Release = new Release
                    {
                        Id = Guid.NewGuid()
                    },
                    DataFileName = "my_data_file",
                };

                var processor = new Processor.Functions.Processor(
                    fileImportService.Object,
                    batchService.Object,
                    importStatusService.Object,
                    processorService.Object,
                    new Mock<ILogger<Processor.Functions.Processor>>().Object);

                importStatusService
                    .Setup(s => s.GetImportStatus(message.Release.Id, message.DataFileName))
                    .ReturnsAsync(new ImportStatus
                    {
                        Status = currentState
                    });

                processor.ProcessUploads(
                    message,
                    null,
                    importStagesMessageQueue.Object,
                    datafileProcessingMessageQueue.Object
                );

                // Verify that no Status updates occurred and that no further attempt to add further processing
                // messages to queues occurred.
                MockUtils.VerifyAllMocks(processorService, importStatusService, batchService,
                    fileImportService, importStagesMessageQueue, datafileProcessingMessageQueue);
            });
        }

        [Fact]
        public void ProcessUploadsButImportIsBeingCancelled()
        {
            var mocks = Mocks();
            var (processorService, importStatusService, batchService, fileImportService) = mocks;
            var importStagesMessageQueue = new Mock<ICollector<ImportMessage>>();
            var datafileProcessingMessageQueue = new Mock<ICollector<ImportObservationsMessage>>();

            var message = new ImportMessage
            {
                ArchiveFileName = "an_archive",
                Release = new Release
                {
                    Id = Guid.NewGuid()
                },
                DataFileName = "my_data_file",
            };

            var processor = new Processor.Functions.Processor(
                fileImportService.Object,
                batchService.Object,
                importStatusService.Object,
                processorService.Object,
                new Mock<ILogger<Processor.Functions.Processor>>().Object);

            importStatusService
                .Setup(s => s.GetImportStatus(message.Release.Id, message.DataFileName))
                .ReturnsAsync(new ImportStatus
                {
                    Status = IStatus.CANCELLING
                });

            importStatusService
                .Setup(s => s.UpdateStatus(message.Release.Id, message.DataFileName, IStatus.CANCELLED, 100))
                .Returns(Task.CompletedTask);

            processor.ProcessUploads(
                message,
                null,
                importStagesMessageQueue.Object,
                datafileProcessingMessageQueue.Object
            );

            // Verify that an import with the current Status of CANCELLING will be updated to be CANCELLED, and that
            // no further processing messages are added to any queues.
            MockUtils.VerifyAllMocks(processorService, importStatusService, batchService,
                fileImportService, importStagesMessageQueue, datafileProcessingMessageQueue);
        }

        [Fact]
        public void ProcessUploadsStage1()
        {
            var mocks = Mocks();
            var (processorService, importStatusService, batchService, fileImportService) = mocks;
            var importStagesMessageQueue = new Mock<ICollector<ImportMessage>>();
            var datafileProcessingMessageQueue = new Mock<ICollector<ImportObservationsMessage>>();

            var message = new ImportMessage
            {
                ArchiveFileName = "",
                Release = new Release
                {
                    Id = Guid.NewGuid()
                },
                DataFileName = "my_data_file",
            };

            var executionContext = new ExecutionContext();

            var processor = new Processor.Functions.Processor(
                fileImportService.Object,
                batchService.Object,
                importStatusService.Object,
                processorService.Object,
                new Mock<ILogger<Processor.Functions.Processor>>().Object);

            importStatusService
                .Setup(s => s.GetImportStatus(message.Release.Id, message.DataFileName))
                .ReturnsAsync(new ImportStatus
                {
                    Status = IStatus.STAGE_1
                });

            processorService
                .Setup(s => s.ProcessStage1(message, executionContext))
                .Returns(Task.CompletedTask);

            importStatusService
                .Setup(s => s.UpdateStatus(message.Release.Id, message.DataFileName, IStatus.STAGE_2, 0))
                .Returns(Task.CompletedTask);

            importStagesMessageQueue
                .Setup(s => s.Add(message));

            processor.ProcessUploads(
                message,
                executionContext,
                importStagesMessageQueue.Object,
                datafileProcessingMessageQueue.Object
            );

            MockUtils.VerifyAllMocks(processorService, importStatusService, batchService,
                fileImportService, importStagesMessageQueue, datafileProcessingMessageQueue);
        }

        [Fact]
        public void ProcessUploadsStage2()
        {
            var mocks = Mocks();
            var (processorService, importStatusService, batchService, fileImportService) = mocks;
            var importStagesMessageQueue = new Mock<ICollector<ImportMessage>>();
            var datafileProcessingMessageQueue = new Mock<ICollector<ImportObservationsMessage>>();

            var message = new ImportMessage
            {
                ArchiveFileName = "",
                Release = new Release
                {
                    Id = Guid.NewGuid()
                },
                DataFileName = "my_data_file",
            };

            var processor = new Processor.Functions.Processor(
                fileImportService.Object,
                batchService.Object,
                importStatusService.Object,
                processorService.Object,
                new Mock<ILogger<Processor.Functions.Processor>>().Object);

            importStatusService
                .Setup(s => s.GetImportStatus(message.Release.Id, message.DataFileName))
                .ReturnsAsync(new ImportStatus
                {
                    Status = IStatus.STAGE_2
                });

            processorService
                .Setup(s => s.ProcessStage2(message))
                .Returns(Task.CompletedTask);

            importStatusService
                .Setup(s => s.UpdateStatus(message.Release.Id, message.DataFileName, IStatus.STAGE_3, 0))
                .Returns(Task.CompletedTask);

            importStagesMessageQueue
                .Setup(s => s.Add(message));

            processor.ProcessUploads(
                message,
                null,
                importStagesMessageQueue.Object,
                datafileProcessingMessageQueue.Object
            );

            MockUtils.VerifyAllMocks(processorService, importStatusService, batchService,
                fileImportService, importStagesMessageQueue, datafileProcessingMessageQueue);
        }

        [Fact]
        public void ProcessUploadsStage3()
        {
            var mocks = Mocks();
            var (processorService, importStatusService, batchService, fileImportService) = mocks;
            var importStagesMessageQueue = new Mock<ICollector<ImportMessage>>();
            var datafileProcessingMessageQueue = new Mock<ICollector<ImportObservationsMessage>>();

            var message = new ImportMessage
            {
                ArchiveFileName = "",
                Release = new Release
                {
                    Id = Guid.NewGuid()
                },
                DataFileName = "my_data_file",
            };

            var processor = new Processor.Functions.Processor(
                fileImportService.Object,
                batchService.Object,
                importStatusService.Object,
                processorService.Object,
                new Mock<ILogger<Processor.Functions.Processor>>().Object);

            importStatusService
                .Setup(s => s.GetImportStatus(message.Release.Id, message.DataFileName))
                .ReturnsAsync(new ImportStatus
                {
                    Status = IStatus.STAGE_3
                });

            processorService
                .Setup(s => s.ProcessStage3(message))
                .Returns(Task.CompletedTask);

            importStatusService
                .Setup(s => s.UpdateStatus(message.Release.Id, message.DataFileName, IStatus.STAGE_4, 0))
                .Returns(Task.CompletedTask);

            importStagesMessageQueue
                .Setup(s => s.Add(message));

            processor.ProcessUploads(
                message,
                null,
                importStagesMessageQueue.Object,
                datafileProcessingMessageQueue.Object
            );

            MockUtils.VerifyAllMocks(processorService, importStatusService, batchService,
                fileImportService, importStagesMessageQueue, datafileProcessingMessageQueue);
        }

        [Fact]
        public void ProcessUploadsStage4Messages()
        {
            var mocks = Mocks();
            var (processorService, importStatusService, batchService, fileImportService) = mocks;
            var importStagesMessageQueue = new Mock<ICollector<ImportMessage>>();
            var datafileProcessingMessageQueue = new Mock<ICollector<ImportObservationsMessage>>();

            var message = new ImportMessage
            {
                ArchiveFileName = "",
                Release = new Release
                {
                    Id = Guid.NewGuid()
                },
                DataFileName = "my_data_file",
            };

            var processor = new Processor.Functions.Processor(
                fileImportService.Object,
                batchService.Object,
                importStatusService.Object,
                processorService.Object,
                new Mock<ILogger<Processor.Functions.Processor>>().Object);

            importStatusService
                .Setup(s => s.GetImportStatus(message.Release.Id, message.DataFileName))
                .ReturnsAsync(new ImportStatus
                {
                    Status = IStatus.STAGE_4
                });

            processorService
                .Setup(s => s.ProcessStage4Messages(message, datafileProcessingMessageQueue.Object))
                .Returns(Task.CompletedTask);

            processor.ProcessUploads(
                message,
                null,
                importStagesMessageQueue.Object,
                datafileProcessingMessageQueue.Object
            );

            MockUtils.VerifyAllMocks(processorService, importStatusService, batchService,
                fileImportService, importStagesMessageQueue, datafileProcessingMessageQueue);
        }

        [Fact]
        public void CancelImport()
        {
            var mocks = Mocks();
            var (processorService, importStatusService, batchService, fileImportService) = mocks;

            var message = new CancelImportMessage
            {
                ReleaseId = Guid.NewGuid(),
                DataFileName = "my_data_file"
            };

            var processor = new Processor.Functions.Processor(
                fileImportService.Object,
                batchService.Object,
                importStatusService.Object,
                processorService.Object,
                new Mock<ILogger<Processor.Functions.Processor>>().Object);

            var currentImportStatus = new ImportStatus
            {
                Status = IStatus.STAGE_4,
                PercentageComplete = 10,
                PhasePercentageComplete = 50,
            };

            importStatusService
                .Setup(s => s.GetImportStatus(message.ReleaseId, message.DataFileName))
                .ReturnsAsync(currentImportStatus);

            importStatusService
                .Setup(s => s.UpdateStatus(message.ReleaseId, message.DataFileName, IStatus.CANCELLING, currentImportStatus.PercentageComplete))
                .Returns(Task.CompletedTask);

            processor.CancelImports(
                message,
                null);

            // Verify that the  status has been updated to CANCELLING.
            MockUtils.VerifyAllMocks(processorService, importStatusService, batchService, fileImportService);
        }

        private (
            Mock<IProcessorService>,
            Mock<IImportStatusService>,
            Mock<IBatchService>,
            Mock<IFileImportService>
        ) Mocks() {
            return (
                new Mock<IProcessorService>(),
                new Mock<IImportStatusService>(),
                new Mock<IBatchService>(),
                new Mock<IFileImportService>()
            );
        }
    }
}