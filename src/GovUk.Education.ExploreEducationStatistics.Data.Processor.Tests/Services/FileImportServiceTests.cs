using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.DataImportStatus;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Tests.Services
{
    public class FileImportServiceTests
    {
        private static readonly List<DataImportStatus> FinishedStatuses = EnumUtil
            .GetEnumValues<DataImportStatus>()
            .Where(status => status.IsFinished())
            .ToList();

        private static readonly List<DataImportStatus> AbortingStatuses = EnumUtil
            .GetEnumValues<DataImportStatus>()
            .Where(status => status.IsAborting())
            .ToList();

        [Fact]
        public async Task CheckComplete_SingleDataFileCompleted()
        {
            var file = new File
            {
                Id = Guid.NewGuid(),
                Filename = "my_data_file.csv"
            };

            var import = new DataImport
            {
                Id = Guid.NewGuid(),
                Errors = new List<DataImportError>(),
                FileId = file.Id,
                File = file,
                SubjectId = Guid.NewGuid(),
                Status = STAGE_4,
                NumBatches = 1,
                ImportedRows = 2
            };

            var dataImportService = new Mock<IDataImportService>(Strict);

            dataImportService
                .Setup(s => s.GetImport(import.Id))
                .ReturnsAsync(import);

            dataImportService
                .Setup(s => s.UpdateStatus(
                    import.Id, COMPLETE, 100))
                .Returns(Task.CompletedTask);

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.Observation.AddRangeAsync(
                    new Observation
                    {
                        SubjectId = import.SubjectId
                    },
                    new Observation
                    {
                        SubjectId = import.SubjectId
                    },
                    new Observation
                    {
                        SubjectId = Guid.NewGuid()
                    });

                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildFileImportService(dataImportService: dataImportService.Object);

                var message = new ImportObservationsMessage
                {
                    Id = import.Id
                };

                await service.CheckComplete(message, statisticsDbContext);
            }

            MockUtils.VerifyAllMocks(dataImportService);
        }

        [Fact]
        public async Task CheckComplete_LastBatchFileCompleted()
        {
            var file = new File
            {
                Id = Guid.NewGuid(),
                Filename = "my_data_file.csv"
            };

            var import = new DataImport
            {
                Id = Guid.NewGuid(),
                Errors = new List<DataImportError>(),
                FileId = file.Id,
                File = file,
                SubjectId = Guid.NewGuid(),
                Status = STAGE_4,
                NumBatches = 2,
                ImportedRows = 2
            };

            var batchService = new Mock<IBatchService>(Strict);
            var dataImportService = new Mock<IDataImportService>(Strict);

            batchService
                .Setup(s => s.GetNumBatchesRemaining(import.File))
                .ReturnsAsync(0);

            dataImportService
                .Setup(s => s.GetImport(import.Id))
                .ReturnsAsync(import);

            dataImportService
                .Setup(s => s.UpdateStatus(
                    import.Id, COMPLETE, 100))
                .Returns(Task.CompletedTask);

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.Observation.AddRangeAsync(
                    new Observation
                    {
                        SubjectId = import.SubjectId
                    },
                    new Observation
                    {
                        SubjectId = import.SubjectId
                    });

                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildFileImportService(batchService: batchService.Object,
                    dataImportService: dataImportService.Object);

                var message = new ImportObservationsMessage
                {
                    Id = import.Id
                };

                await service.CheckComplete(message, statisticsDbContext);
            }

            MockUtils.VerifyAllMocks(batchService, dataImportService);
        }

        [Fact]
        public async Task CheckComplete_BatchedFilesStillProcessing()
        {
            var file = new File
            {
                Id = Guid.NewGuid(),
                Filename = "my_data_file.csv"
            };

            var import = new DataImport
            {
                Id = Guid.NewGuid(),
                Errors = new List<DataImportError>(),
                FileId = file.Id,
                File = file,
                SubjectId = Guid.NewGuid(),
                Status = STAGE_4,
                NumBatches = 2,
                ImportedRows = 2
            };

            var batchService = new Mock<IBatchService>(Strict);
            var dataImportService = new Mock<IDataImportService>(Strict);

            batchService
                .Setup(s => s.GetNumBatchesRemaining(import.File))
                .ReturnsAsync(1);

            dataImportService
                .Setup(s => s.GetImport(import.Id))
                .ReturnsAsync(import);

            dataImportService
                .Setup(s => s.UpdateStatus(
                    import.Id, STAGE_4, 50))
                .Returns(Task.CompletedTask);

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildFileImportService(batchService: batchService.Object,
                    dataImportService: dataImportService.Object);

                var message = new ImportObservationsMessage
                {
                    Id = import.Id
                };

                await service.CheckComplete(message, statisticsDbContext);
            }

            MockUtils.VerifyAllMocks(batchService, dataImportService);
        }

        [Fact]
        public async Task CheckComplete_SingleDataFileCompleted_HasErrors()
        {
            var file = new File
            {
                Id = Guid.NewGuid(),
                Filename = "my_data_file.csv"
            };

            var import = new DataImport
            {
                Id = Guid.NewGuid(),
                Errors = new List<DataImportError>
                {
                    new DataImportError("an error")
                },
                FileId = file.Id,
                File = file,
                SubjectId = Guid.NewGuid(),
                Status = STAGE_4,
                NumBatches = 1,
                ImportedRows = 2
            };

            var dataImportService = new Mock<IDataImportService>(Strict);

            dataImportService
                .Setup(s => s.GetImport(import.Id))
                .ReturnsAsync(import);

            dataImportService
                .Setup(s => s.UpdateStatus(
                    import.Id, FAILED, 100))
                .Returns(Task.CompletedTask);

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.Observation.AddRangeAsync(
                    new Observation
                    {
                        SubjectId = import.SubjectId
                    },
                    new Observation
                    {
                        SubjectId = import.SubjectId
                    });

                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildFileImportService(dataImportService: dataImportService.Object);

                var message = new ImportObservationsMessage
                {
                    Id = import.Id
                };

                await service.CheckComplete(message, statisticsDbContext);
            }

            MockUtils.VerifyAllMocks(dataImportService);
        }

        [Fact]
        public async Task CheckComplete_SingleDataFileCompleted_HasIncorrectObservationCount()
        {
            var file = new File
            {
                Id = Guid.NewGuid(),
                Filename = "my_data_file.csv"
            };

            var import = new DataImport
            {
                Id = Guid.NewGuid(),
                Errors = new List<DataImportError>(),
                FileId = file.Id,
                File = file,
                SubjectId = Guid.NewGuid(),
                Status = STAGE_4,
                NumBatches = 1,
                ImportedRows = 3
            };

            var dataImportService = new Mock<IDataImportService>(Strict);

            dataImportService
                .Setup(s => s.GetImport(import.Id))
                .ReturnsAsync(import);

            dataImportService
                .Setup(s => s.FailImport(import.Id, 
                    $"Number of observations inserted (2) does not equal that expected ({import.ImportedRows}) : Please delete & retry"))
                .Returns(Task.CompletedTask);

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.Observation.AddRangeAsync(
                    new Observation
                    {
                        SubjectId = import.SubjectId
                    },
                    new Observation
                    {
                        SubjectId = import.SubjectId
                    });

                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildFileImportService(dataImportService: dataImportService.Object);

                var message = new ImportObservationsMessage
                {
                    Id = import.Id
                };

                await service.CheckComplete(message, statisticsDbContext);
            }

            MockUtils.VerifyAllMocks(dataImportService);
        }

        [Fact]
        public async Task CheckComplete_LastBatchFileCompleted_HasErrors()
        {
            var file = new File
            {
                Id = Guid.NewGuid(),
                Filename = "my_data_file.csv"
            };

            var import = new DataImport
            {
                Id = Guid.NewGuid(),
                Errors = new List<DataImportError>
                {
                    new DataImportError("an error")
                },
                FileId = file.Id,
                File = file,
                SubjectId = Guid.NewGuid(),
                Status = STAGE_4,
                NumBatches = 2,
                ImportedRows = 2
            };
            
            var batchService = new Mock<IBatchService>(Strict);
            var dataImportService = new Mock<IDataImportService>(Strict);

            batchService
                .Setup(s => s.GetNumBatchesRemaining(import.File))
                .ReturnsAsync(0);

            dataImportService
                .Setup(s => s.GetImport(import.Id))
                .ReturnsAsync(import);

            dataImportService
                .Setup(s => s.UpdateStatus(
                    import.Id, FAILED, 100))
                .Returns(Task.CompletedTask);

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.Observation.AddRangeAsync(
                    new Observation
                    {
                        SubjectId = import.SubjectId
                    },
                    new Observation
                    {
                        SubjectId = import.SubjectId
                    });

                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildFileImportService(batchService: batchService.Object,
                    dataImportService: dataImportService.Object);

                var message = new ImportObservationsMessage
                {
                    Id = import.Id
                };

                await service.CheckComplete(message, statisticsDbContext);
            }

            MockUtils.VerifyAllMocks(batchService, dataImportService);
        }

        [Fact]
        public async Task CheckComplete_LastBatchFileCompleted_HasIncorrectObservationCount()
        {
            var file = new File
            {
                Id = Guid.NewGuid(),
                Filename = "my_data_file.csv"
            };

            var import = new DataImport
            {
                Id = Guid.NewGuid(),
                Errors = new List<DataImportError>(),
                FileId = file.Id,
                File = file,
                SubjectId = Guid.NewGuid(),
                Status = STAGE_4,
                NumBatches = 2,
                ImportedRows = 3
            };
            
            var batchService = new Mock<IBatchService>(Strict);
            var dataImportService = new Mock<IDataImportService>(Strict);

            batchService
                .Setup(s => s.GetNumBatchesRemaining(import.File))
                .ReturnsAsync(0);

            dataImportService
                .Setup(s => s.GetImport(import.Id))
                .ReturnsAsync(import);

            dataImportService
                .Setup(s => s.FailImport(import.Id,
                    $"Number of observations inserted (2) does not equal that expected ({import.ImportedRows}) : Please delete & retry"))
                .Returns(Task.CompletedTask);

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.Observation.AddRangeAsync(
                    new Observation
                    {
                        SubjectId = import.SubjectId
                    },
                    new Observation
                    {
                        SubjectId = import.SubjectId
                    });

                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildFileImportService(batchService: batchService.Object,
                    dataImportService: dataImportService.Object);

                var message = new ImportObservationsMessage
                {
                    Id = import.Id
                };

                await service.CheckComplete(message, statisticsDbContext);
            }

            MockUtils.VerifyAllMocks(batchService, dataImportService);
        }

        [Fact]
        public async Task CheckComplete_SingleDataFileCompleted_AlreadyFinished()
        {
            await FinishedStatuses
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(async finishedStatus =>
                {
                    var file = new File
                    {
                        Id = Guid.NewGuid(),
                        Filename = "my_data_file.csv"
                    };

                    var import = new DataImport
                    {
                        Id = Guid.NewGuid(),
                        Errors = new List<DataImportError>(),
                        FileId = file.Id,
                        File = file,
                        SubjectId = Guid.NewGuid(),
                        Status = finishedStatus,
                        NumBatches = 1,
                        ImportedRows = 2
                    };

                    var dataImportService = new Mock<IDataImportService>(Strict);

                    dataImportService
                        .Setup(s => s.GetImport(import.Id))
                        .ReturnsAsync(import);

                    var statisticsDbContextId = Guid.NewGuid().ToString();

                    await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
                    {
                        var service = BuildFileImportService(dataImportService: dataImportService.Object);

                        var message = new ImportObservationsMessage
                        {
                            Id = import.Id
                        };

                        await service.CheckComplete(message, statisticsDbContext);
                    }

                    MockUtils.VerifyAllMocks(dataImportService);
                });
        }

        [Fact]
        public async Task CheckComplete_LastBatchFileCompleted_AlreadyFinished()
        {
            await FinishedStatuses
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(async finishedStatus =>
                {
                    var file = new File
                    {
                        Id = Guid.NewGuid(),
                        Filename = "my_data_file.csv"
                    };

                    var import = new DataImport
                    {
                        Id = Guid.NewGuid(),
                        Errors = new List<DataImportError>(),
                        FileId = file.Id,
                        File = file,
                        SubjectId = Guid.NewGuid(),
                        Status = finishedStatus,
                        NumBatches = 2,
                        ImportedRows = 2
                    };

                    var dataImportService = new Mock<IDataImportService>(Strict);

                    dataImportService
                        .Setup(s => s.GetImport(import.Id))
                        .ReturnsAsync(import);

                    var statisticsDbContextId = Guid.NewGuid().ToString();

                    await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
                    {
                        var service = BuildFileImportService(dataImportService: dataImportService.Object);

                        var message = new ImportObservationsMessage
                        {
                            Id = import.Id
                        };

                        await service.CheckComplete(message, statisticsDbContext);
                    }

                    MockUtils.VerifyAllMocks(dataImportService);
                });
        }

        [Fact]
        public async Task CheckComplete_SingleDataFileCompleted_Aborting()
        {
            await AbortingStatuses
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(async abortingStatus =>
                {
                    var file = new File
                    {
                        Id = Guid.NewGuid(),
                        Filename = "my_data_file.csv"
                    };

                    var import = new DataImport
                    {
                        Id = Guid.NewGuid(),
                        Errors = new List<DataImportError>(),
                        FileId = file.Id,
                        File = file,
                        SubjectId = Guid.NewGuid(),
                        Status = abortingStatus,
                        NumBatches = 1,
                        ImportedRows = 2
                    };

                    var dataImportService = new Mock<IDataImportService>(Strict);

                    dataImportService
                        .Setup(s => s.GetImport(import.Id))
                        .ReturnsAsync(import);

                    dataImportService
                        .Setup(s => s.UpdateStatus(
                            import.Id, abortingStatus.GetFinishingStateOfAbortProcess(), 100))
                        .Returns(Task.CompletedTask);

                    var statisticsDbContextId = Guid.NewGuid().ToString();

                    await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
                    {
                        var service = BuildFileImportService(dataImportService: dataImportService.Object);

                        var message = new ImportObservationsMessage
                        {
                            Id = import.Id
                        };

                        await service.CheckComplete(message, statisticsDbContext);
                    }

                    MockUtils.VerifyAllMocks(dataImportService);
                });
        }

        [Fact]
        public async Task CheckComplete_LastBatchFileCompleted_Aborting()
        {
            await AbortingStatuses
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(async abortingStatus =>
                {
                    var file = new File
                    {
                        Id = Guid.NewGuid(),
                        Filename = "my_data_file.csv"
                    };

                    var import = new DataImport
                    {
                        Id = Guid.NewGuid(),
                        Errors = new List<DataImportError>(),
                        FileId = file.Id,
                        File = file,
                        SubjectId = Guid.NewGuid(),
                        Status = abortingStatus,
                        NumBatches = 2,
                        ImportedRows = 2
                    };

                    var dataImportService = new Mock<IDataImportService>(Strict);

                    dataImportService
                        .Setup(s => s.GetImport(import.Id))
                        .ReturnsAsync(import);

                    dataImportService
                        .Setup(s => s.UpdateStatus(
                            import.Id, abortingStatus.GetFinishingStateOfAbortProcess(), 100))
                        .Returns(Task.CompletedTask);

                    var statisticsDbContextId = Guid.NewGuid().ToString();

                    await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
                    {
                        var service = BuildFileImportService(dataImportService: dataImportService.Object);

                        var message = new ImportObservationsMessage
                        {
                            Id = import.Id
                        };

                        await service.CheckComplete(message, statisticsDbContext);
                    }

                    MockUtils.VerifyAllMocks(dataImportService);
                });
        }

        private static FileImportService BuildFileImportService(
            IBatchService batchService = null,
            IBlobStorageService blobStorageService = null,
            IImporterService importerService = null,
            ILogger<FileImportService> logger = null,
            IDataImportService dataImportService = null
            )
        {
            return new FileImportService(
                logger ?? new Mock<ILogger<FileImportService>>().Object,
                batchService ?? new Mock<IBatchService>(Strict).Object,
                blobStorageService ?? new Mock<IBlobStorageService>(Strict).Object,
                dataImportService ?? new Mock<IDataImportService>(Strict).Object,
                importerService ?? new Mock<IImporterService>(Strict).Object,
                new InMemoryDatabaseHelper(new InMemoryDbContextSupplier()));
        }
    }
}
