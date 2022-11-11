using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.DataImportStatus;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Tests.Functions
{
    public class ProcessorTests
    {
        [Fact]
        public void ProcessUploadsUnpackArchive()
        {
            var mocks = Mocks();
            var (processorService, importService, fileImportService) = mocks;
            var importStagesMessageQueue = new Mock<ICollector<ImportMessage>>();
            var datafileProcessingMessageQueue = new Mock<ICollector<ImportObservationsMessage>>();

            var import = new DataImport
            {
                Id = Guid.NewGuid(),
                File = new File
                {
                    Filename = "my_data_file.csv"
                },
                ZipFile = new File
                {
                    Filename = "my_data_file.zip"
                },
                Status = QUEUED
            };

            var processor = new Processor.Functions.Processor(
                fileImportService.Object,
                importService.Object,
                processorService.Object,
                Mock.Of<IDbContextSupplier>(Strict),
                new Mock<ILogger<Processor.Functions.Processor>>().Object);

            processorService
                .Setup(s => s.ProcessUnpackingArchive(import.Id))
                .Returns(Task.CompletedTask);

            importService
                .Setup(s => s.GetImport(import.Id))
                .ReturnsAsync(import);

            importService
                .Setup(s => s.UpdateStatus(import.Id, STAGE_1, 0))
                .Returns(Task.CompletedTask);

            var message = new ImportMessage(import.Id);

            importStagesMessageQueue
                .Setup(s => s.Add(message));

            processor.ProcessUploads(
                message,
                new ExecutionContext(),
                importStagesMessageQueue.Object,
                datafileProcessingMessageQueue.Object
            );

            MockUtils.VerifyAllMocks(processorService, importService,
                fileImportService, importStagesMessageQueue, datafileProcessingMessageQueue);
        }

        [Fact]
        public void ProcessUploadsUnpackArchiveWithNoArchive()
        {
            var mocks = Mocks();
            var (processorService, importService, fileImportService) = mocks;
            var importStagesMessageQueue = new Mock<ICollector<ImportMessage>>();
            var datafileProcessingMessageQueue = new Mock<ICollector<ImportObservationsMessage>>();

            var import = new DataImport
            {
                Id = Guid.NewGuid(),
                File = new File
                {
                    Filename = "my_data_file.csv"
                },
                Status = QUEUED
            };

            var processor = new Processor.Functions.Processor(
                fileImportService.Object,
                importService.Object,
                processorService.Object,
                Mock.Of<IDbContextSupplier>(Strict),
                new Mock<ILogger<Processor.Functions.Processor>>().Object);

            importService
                .Setup(s => s.GetImport(import.Id))
                .ReturnsAsync(import);

            importService
                .Setup(s => s.UpdateStatus(import.Id, STAGE_1, 0))
                .Returns(Task.CompletedTask);

            var message = new ImportMessage(import.Id);

            importStagesMessageQueue
                .Setup(s => s.Add(message));

            processor.ProcessUploads(
                message,
                new ExecutionContext(),
                importStagesMessageQueue.Object,
                datafileProcessingMessageQueue.Object
            );

            MockUtils.VerifyAllMocks(processorService, importService,
                fileImportService, importStagesMessageQueue, datafileProcessingMessageQueue);
        }

        [Fact]
        public void ProcessUploadsButImportIsFinished()
        {
            var finishedStates = EnumUtil
                .GetEnumValues<DataImportStatus>()
                .Where(status => status.IsFinished())
                .ToList();

            finishedStates.ForEach(currentState =>
            {
                var mocks = Mocks();
                var (processorService, importService, fileImportService) = mocks;
                var importStagesMessageQueue = new Mock<ICollector<ImportMessage>>();
                var datafileProcessingMessageQueue = new Mock<ICollector<ImportObservationsMessage>>();

                var import = new DataImport
                {
                    Id = Guid.NewGuid(),
                    File = new File
                    {
                        Filename = "my_data_file.csv"
                    },
                    Status = currentState
                };

                var processor = new Processor.Functions.Processor(
                    fileImportService.Object,
                    importService.Object,
                    processorService.Object,
                    Mock.Of<IDbContextSupplier>(Strict),
                    new Mock<ILogger<Processor.Functions.Processor>>().Object);

                importService
                    .Setup(s => s.GetImport(import.Id))
                    .ReturnsAsync(import);

                var message = new ImportMessage(import.Id);

                processor.ProcessUploads(
                    message,
                    new ExecutionContext(),
                    importStagesMessageQueue.Object,
                    datafileProcessingMessageQueue.Object
                );

                // Verify that no Status updates occurred and that no further attempt to add further processing
                // messages to queues occurred.
                MockUtils.VerifyAllMocks(processorService, importService,
                    fileImportService, importStagesMessageQueue, datafileProcessingMessageQueue);
            });
        }

        [Fact]
        public void ProcessUploadsButImportIsBeingCancelled()
        {
            var mocks = Mocks();
            var (processorService, importService, fileImportService) = mocks;
            var importStagesMessageQueue = new Mock<ICollector<ImportMessage>>();
            var datafileProcessingMessageQueue = new Mock<ICollector<ImportObservationsMessage>>();

            var import = new DataImport
            {
                Id = Guid.NewGuid(),
                File = new File
                {
                    Filename = "my_data_file.csv"
                },
                Status = CANCELLING
            };

            var processor = new Processor.Functions.Processor(
                fileImportService.Object,
                importService.Object,
                processorService.Object,
                Mock.Of<IDbContextSupplier>(Strict),
                new Mock<ILogger<Processor.Functions.Processor>>().Object);

            importService
                .Setup(s => s.GetImport(import.Id))
                .ReturnsAsync(import);

            importService
                .Setup(s => s.UpdateStatus(import.Id, CANCELLED, 100))
                .Returns(Task.CompletedTask);

            var message = new ImportMessage(import.Id);

            processor.ProcessUploads(
                message,
                new ExecutionContext(),
                importStagesMessageQueue.Object,
                datafileProcessingMessageQueue.Object
            );

            // Verify that an import with the current Status of CANCELLING will be updated to be CANCELLED, and that
            // no further processing messages are added to any queues.
            MockUtils.VerifyAllMocks(processorService, importService,
                fileImportService, importStagesMessageQueue, datafileProcessingMessageQueue);
        }

        [Fact]
        public void CancelImport()
        {
            var mocks = Mocks();
            var (processorService, importService, fileImportService) = mocks;

            var processor = new Processor.Functions.Processor(
                fileImportService.Object,
                importService.Object,
                processorService.Object,
                Mock.Of<IDbContextSupplier>(Strict),
                new Mock<ILogger<Processor.Functions.Processor>>().Object);
            
            var message = new CancelImportMessage(Guid.NewGuid());
            
            importService
                .Setup(s => s.UpdateStatus(message.Id, CANCELLING, 0))
                .Returns(Task.CompletedTask);

            processor.CancelImports(message);

            // Verify that the status has been updated to CANCELLING.
            MockUtils.VerifyAllMocks(processorService, importService, fileImportService);
        }

        private static (
            Mock<IProcessorService>,
            Mock<IDataImportService>,
            Mock<IFileImportService>
            ) Mocks()
        {
            return (
                new Mock<IProcessorService>(),
                new Mock<IDataImportService>(),
                new Mock<IFileImportService>()
            );
        }
    }
}
