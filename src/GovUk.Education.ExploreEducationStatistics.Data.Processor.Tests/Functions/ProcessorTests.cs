using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
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
            var datafileProcessingMessageQueue = new Mock<ICollector<ImportMessage>>();

            var message = new ImportMessage
            {
                ArchiveFileName = "an_archive",
                Release = new Release
                {
                    Id = Guid.NewGuid()
                },
                DataFileName = "my_data_file",
                OrigDataFileName = "my_data_file_original"
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
                .Setup(s => s.GetImportStatus(message.Release.Id, message.OrigDataFileName))
                .ReturnsAsync(new ImportStatus
                {
                    Status = IStatus.QUEUED
                });

            importStatusService
                .Setup(s => s.UpdateStatus(message.Release.Id, message.OrigDataFileName, IStatus.STAGE_1, 0, 0))
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
            var datafileProcessingMessageQueue = new Mock<ICollector<ImportMessage>>();

            var message = new ImportMessage
            {
                ArchiveFileName = "",
                Release = new Release
                {
                    Id = Guid.NewGuid()
                },
                DataFileName = "my_data_file",
                OrigDataFileName = "my_data_file_original"
            };

            var processor = new Processor.Functions.Processor(
                fileImportService.Object,
                batchService.Object,
                importStatusService.Object,
                processorService.Object,
                new Mock<ILogger<Processor.Functions.Processor>>().Object);

            importStatusService
                .Setup(s => s.GetImportStatus(message.Release.Id, message.OrigDataFileName))
                .ReturnsAsync(new ImportStatus
                {
                    Status = IStatus.QUEUED
                });

            importStatusService
                .Setup(s => s.UpdateStatus(message.Release.Id, message.OrigDataFileName, IStatus.STAGE_1, 0, 0))
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
        public void ProcessUploadsStage1()
        {
            var mocks = Mocks();
            var (processorService, importStatusService, batchService, fileImportService) = mocks;
            var importStagesMessageQueue = new Mock<ICollector<ImportMessage>>();
            var datafileProcessingMessageQueue = new Mock<ICollector<ImportMessage>>();

            var message = new ImportMessage
            {
                ArchiveFileName = "",
                Release = new Release
                {
                    Id = Guid.NewGuid()
                },
                DataFileName = "my_data_file",
                OrigDataFileName = "my_data_file_original"
            };
            
            var executionContext = new ExecutionContext();
            
            var processor = new Processor.Functions.Processor(
                fileImportService.Object,
                batchService.Object,
                importStatusService.Object, 
                processorService.Object,
                new Mock<ILogger<Processor.Functions.Processor>>().Object);

            importStatusService
                .Setup(s => s.GetImportStatus(message.Release.Id, message.OrigDataFileName))
                .ReturnsAsync(new ImportStatus
                {
                    Status = IStatus.STAGE_1
                });

            processorService
                .Setup(s => s.ProcessStage1(message, executionContext))
                .Returns(Task.CompletedTask);

            importStatusService
                .Setup(s => s.UpdateStatus(message.Release.Id, message.OrigDataFileName, IStatus.STAGE_2, 0, 0))
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
            var datafileProcessingMessageQueue = new Mock<ICollector<ImportMessage>>();

            var message = new ImportMessage
            {
                ArchiveFileName = "",
                Release = new Release
                {
                    Id = Guid.NewGuid()
                },
                DataFileName = "my_data_file",
                OrigDataFileName = "my_data_file_original"
            };
            
            var processor = new Processor.Functions.Processor(
                fileImportService.Object,
                batchService.Object,
                importStatusService.Object, 
                processorService.Object,
                new Mock<ILogger<Processor.Functions.Processor>>().Object);

            importStatusService
                .Setup(s => s.GetImportStatus(message.Release.Id, message.OrigDataFileName))
                .ReturnsAsync(new ImportStatus
                {
                    Status = IStatus.STAGE_2
                });

            processorService
                .Setup(s => s.ProcessStage2(message))
                .Returns(Task.CompletedTask);

            importStatusService
                .Setup(s => s.UpdateStatus(message.Release.Id, message.OrigDataFileName, IStatus.STAGE_3, 0, 0))
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
            var datafileProcessingMessageQueue = new Mock<ICollector<ImportMessage>>();

            var message = new ImportMessage
            {
                ArchiveFileName = "",
                Release = new Release
                {
                    Id = Guid.NewGuid()
                },
                DataFileName = "my_data_file",
                OrigDataFileName = "my_data_file_original"
            };
            
            var processor = new Processor.Functions.Processor(
                fileImportService.Object,
                batchService.Object,
                importStatusService.Object, 
                processorService.Object,
                new Mock<ILogger<Processor.Functions.Processor>>().Object);

            importStatusService
                .Setup(s => s.GetImportStatus(message.Release.Id, message.OrigDataFileName))
                .ReturnsAsync(new ImportStatus
                {
                    Status = IStatus.STAGE_3
                });

            processorService
                .Setup(s => s.ProcessStage3(message))
                .Returns(Task.CompletedTask);

            importStatusService
                .Setup(s => s.UpdateStatus(message.Release.Id, message.OrigDataFileName, IStatus.STAGE_4, 0, 0))
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
            var datafileProcessingMessageQueue = new Mock<ICollector<ImportMessage>>();

            var message = new ImportMessage
            {
                ArchiveFileName = "",
                Release = new Release
                {
                    Id = Guid.NewGuid()
                },
                DataFileName = "my_data_file",
                OrigDataFileName = "my_data_file_original"
            };
            
            var processor = new Processor.Functions.Processor(
                fileImportService.Object,
                batchService.Object,
                importStatusService.Object, 
                processorService.Object,
                new Mock<ILogger<Processor.Functions.Processor>>().Object);

            importStatusService
                .Setup(s => s.GetImportStatus(message.Release.Id, message.OrigDataFileName))
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