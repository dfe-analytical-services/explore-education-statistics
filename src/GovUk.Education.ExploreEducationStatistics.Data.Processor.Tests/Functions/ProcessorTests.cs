using GovUk.Education.ExploreEducationStatistics.Common.Tests.Functions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.DataImportStatus;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Tests.Functions
{
    public class ProcessorTests
    {
        [Fact]
        public async Task ProcessUploadsUnpackZipWithNoZipFile()
        {
            var mocks = Mocks();
            var (processorService, dataImportService, fileImportService) = mocks;

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
                dataImportService.Object,
                processorService.Object,
                new Mock<ILogger<Processor.Functions.Processor>>().Object);

            dataImportService
                .Setup(s => s.GetImport(import.Id))
                .ReturnsAsync(import);

            dataImportService
                .Setup(s => s.UpdateStatus(import.Id, STAGE_1, 0))
                .Returns(Task.CompletedTask);

            var importMessage = new ImportMessage(import.Id);

            var outputMessages = await processor.ProcessUploads(
                importMessage,
                new TestFunctionContext());

            MockUtils.VerifyAllMocks(processorService,
                dataImportService,
                fileImportService);

            // Verify that the message will be queued to trigger the next stage.
            Assert.Equal(new[]
            {
                importMessage
            }, outputMessages);
        }

        [Fact]
        public async Task ProcessUploadsButImportIsFinished()
        {
            var finishedStates = EnumUtil
                .GetEnums<DataImportStatus>()
                .Where(status => status.IsFinished())
                .ToList();

            await finishedStates
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(async currentState =>
                {
                    var mocks = Mocks();
                    var (processorService, dataImportService, fileImportService) = mocks;

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
                        dataImportService.Object,
                        processorService.Object,
                        new Mock<ILogger<Processor.Functions.Processor>>().Object);

                    dataImportService
                        .Setup(s => s.GetImport(import.Id))
                        .ReturnsAsync(import);

                    var importMessage = new ImportMessage(import.Id);

                    var outputMessages = await processor.ProcessUploads(
                        importMessage,
                        new TestFunctionContext());

                    // Verify that no Status updates occurred and that no further attempt to add further processing
                    // messages to queues occurred.
                    MockUtils.VerifyAllMocks(processorService,
                        dataImportService,
                        fileImportService);

                    Assert.Empty(outputMessages);
                });
        }

        [Fact]
        public async Task ProcessUploadsButImportIsBeingCancelled()
        {
            var mocks = Mocks();
            var (processorService, dataImportService, fileImportService) = mocks;

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
                dataImportService.Object,
                processorService.Object,
                new Mock<ILogger<Processor.Functions.Processor>>().Object);

            dataImportService
                .Setup(s => s.GetImport(import.Id))
                .ReturnsAsync(import);

            dataImportService
                .Setup(s => s.UpdateStatus(import.Id, CANCELLED, 100))
                .Returns(Task.CompletedTask);

            var importMessage = new ImportMessage(import.Id);

            var outputMessages = await processor.ProcessUploads(
                importMessage,
                new TestFunctionContext());

            // Verify that an import with the current Status of CANCELLING will be updated to be CANCELLED, and that
            // no further processing messages are added to any queues.
            MockUtils.VerifyAllMocks(processorService,
                dataImportService,
                fileImportService);

            Assert.Empty(outputMessages);
        }

        [Fact]
        public async Task CancelImport()
        {
            var mocks = Mocks();
            var (processorService, dataImportService, fileImportService) = mocks;

            var processor = new Processor.Functions.Processor(
                dataImportService.Object,
                processorService.Object,
                new Mock<ILogger<Processor.Functions.Processor>>().Object);

            var message = new CancelImportMessage(Guid.NewGuid());

            dataImportService
                .Setup(s => s.UpdateStatus(message.Id, CANCELLING, 0))
                .Returns(Task.CompletedTask);

            await processor.CancelImports(message);

            // Verify that the status has been updated to CANCELLING.
            MockUtils.VerifyAllMocks(processorService, dataImportService, fileImportService);
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
