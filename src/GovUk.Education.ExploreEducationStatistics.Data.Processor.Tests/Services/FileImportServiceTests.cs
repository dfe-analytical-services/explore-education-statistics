using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.IStatus;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Tests.Services
{
    public class FileImportServiceTests
    {
        private static readonly List<IStatus> FinishedStatuses = EnumUtil
            .GetEnumValues<IStatus>()
            .Where(ImportStatus.IsFinishedState)
            .ToList();

        private static readonly List<IStatus> AbortingStatuses = EnumUtil
            .GetEnumValues<IStatus>()
            .Where(ImportStatus.IsAbortingState)
            .ToList();

        [Fact]
        public async Task CheckComplete_SingleDataFileCompleted()
        {
            var message = new ImportObservationsMessage
            {
                ReleaseId = Guid.NewGuid(),
                NumBatches = 1,
                DataFileName = "my_data_file.csv",
                TotalRows = 2,
                SubjectId = Guid.NewGuid()
            };

            var importStatusService = new Mock<IImportStatusService>(Strict);

            var service = BuildFileImportService(
                importStatusService: importStatusService.Object);

            importStatusService
                .Setup(s => s.GetImportStatus(message.ReleaseId, message.DataFileName))
                .ReturnsAsync(new ImportStatus
                {
                    Status = STAGE_4
                });

            importStatusService
                .Setup(s => s.UpdateStatus(
                    message.ReleaseId, message.DataFileName, COMPLETE, 100))
                .Returns(Task.CompletedTask);

            var dbContext = StatisticsDbUtils.InMemoryStatisticsDbContext();

            await using (dbContext)
            {
                await dbContext.Observation.AddRangeAsync(
                    new Observation
                    {
                        SubjectId = message.SubjectId
                    },
                    new Observation
                    {
                        SubjectId = message.SubjectId
                    },
                    new Observation
                    {
                        SubjectId = Guid.NewGuid()
                    });

                await dbContext.SaveChangesAsync();

                await service.CheckComplete(message.ReleaseId, message, dbContext);
            }

            MockUtils.VerifyAllMocks(importStatusService);
        }

        [Fact]
        public async Task CheckComplete_LastBatchFileCompleted()
        {
            var message = new ImportObservationsMessage
            {
                ReleaseId = Guid.NewGuid(),
                NumBatches = 2,
                DataFileName = "my_data_file.csv",
                TotalRows = 2,
                SubjectId = Guid.NewGuid()
            };

            var importStatusService = new Mock<IImportStatusService>(Strict);
            var fileStorageService = new Mock<IFileStorageService>();

            var service = BuildFileImportService(
                importStatusService: importStatusService.Object,
                fileStorageService: fileStorageService.Object);

            importStatusService
                .Setup(s => s.GetImportStatus(message.ReleaseId, message.DataFileName))
                .ReturnsAsync(new ImportStatus
                {
                    Status = STAGE_4
                });

            fileStorageService
                .Setup(s => s.GetNumBatchesRemaining(message.ReleaseId, message.DataFileName))
                .ReturnsAsync(0);

            importStatusService
                .Setup(s => s.UpdateStatus(
                    message.ReleaseId, message.DataFileName, COMPLETE, 100))
                .Returns(Task.CompletedTask);

            var dbContext = StatisticsDbUtils.InMemoryStatisticsDbContext();

            await using (dbContext)
            {
                await dbContext.Observation.AddRangeAsync(
                    new Observation
                    {
                        SubjectId = message.SubjectId
                    },
                    new Observation
                    {
                        SubjectId = message.SubjectId
                    });

                await dbContext.SaveChangesAsync();

                await service.CheckComplete(message.ReleaseId, message, dbContext);
            }

            MockUtils.VerifyAllMocks(importStatusService, fileStorageService);
        }

        [Fact]
        public async Task CheckComplete_BatchedFilesStillProcessing()
        {
            var message = new ImportObservationsMessage
            {
                ReleaseId = Guid.NewGuid(),
                NumBatches = 2,
                DataFileName = "my_data_file.csv",
                TotalRows = 2,
                SubjectId = Guid.NewGuid(),
                BatchNo = 1
            };

            var importStatusService = new Mock<IImportStatusService>(Strict);
            var fileStorageService = new Mock<IFileStorageService>();

            var service = BuildFileImportService(
                importStatusService: importStatusService.Object,
                fileStorageService: fileStorageService.Object);

            importStatusService
                .Setup(s => s.GetImportStatus(message.ReleaseId, message.DataFileName))
                .ReturnsAsync(new ImportStatus
                {
                    Status = STAGE_4
                });

            fileStorageService
                .Setup(s => s.GetNumBatchesRemaining(message.ReleaseId, message.DataFileName))
                .ReturnsAsync(1);

            importStatusService
                .Setup(s => s.UpdateStatus(
                    message.ReleaseId, message.DataFileName, STAGE_4, 50))
                .Returns(Task.CompletedTask);

            var dbContext = StatisticsDbUtils.InMemoryStatisticsDbContext();

            await using (dbContext)
            {
                await service.CheckComplete(message.ReleaseId, message, dbContext);
            }

            MockUtils.VerifyAllMocks(importStatusService, fileStorageService);
        }

        [Fact]
        public async Task CheckComplete_SingleDataFileCompleted_HasErrors()
        {
            var message = new ImportObservationsMessage
            {
                ReleaseId = Guid.NewGuid(),
                NumBatches = 1,
                DataFileName = "my_data_file.csv",
                TotalRows = 2,
                SubjectId = Guid.NewGuid()
            };

            var importStatusService = new Mock<IImportStatusService>(Strict);

            var service = BuildFileImportService(
                importStatusService: importStatusService.Object);

            importStatusService
                .Setup(s => s.GetImportStatus(message.ReleaseId, message.DataFileName))
                .ReturnsAsync(new ImportStatus
                {
                    Status = STAGE_4,
                    Errors = "an error"
                });

            importStatusService
                .Setup(s => s.UpdateStatus(
                    message.ReleaseId, message.DataFileName, FAILED, 100))
                .Returns(Task.CompletedTask);

            var dbContext = StatisticsDbUtils.InMemoryStatisticsDbContext();

            await using (dbContext)
            {
                await dbContext.Observation.AddRangeAsync(
                    new Observation
                    {
                        SubjectId = message.SubjectId
                    },
                    new Observation
                    {
                        SubjectId = message.SubjectId
                    });

                await dbContext.SaveChangesAsync();

                await service.CheckComplete(message.ReleaseId, message, dbContext);
            }

            MockUtils.VerifyAllMocks(importStatusService);
        }

        [Fact]
        public async Task CheckComplete_SingleDataFileCompleted_HasIncorrectObservationCount()
        {
            var message = new ImportObservationsMessage
            {
                ReleaseId = Guid.NewGuid(),
                NumBatches = 1,
                DataFileName = "my_data_file.csv",
                TotalRows = 3,
                SubjectId = Guid.NewGuid()
            };

            var importStatusService = new Mock<IImportStatusService>(Strict);
            var batchService = new Mock<IBatchService>(Strict);

            var service = BuildFileImportService(
                importStatusService: importStatusService.Object,
                batchService: batchService.Object);

            importStatusService
                .Setup(s => s.GetImportStatus(message.ReleaseId, message.DataFileName))
                .ReturnsAsync(new ImportStatus
                {
                    Status = STAGE_4,
                });

            batchService
                .Setup(s => s.FailImport(message.ReleaseId, message.DataFileName, new List<ValidationError>
                {
                    new ValidationError(
                        $"Number of observations inserted (2) " +
                        $"does not equal that expected ({message.TotalRows}) : Please delete & retry"
                    )
                }))
                .Returns(Task.CompletedTask);

            var dbContext = StatisticsDbUtils.InMemoryStatisticsDbContext();

            await using (dbContext)
            {
                await dbContext.Observation.AddRangeAsync(
                    new Observation
                    {
                        SubjectId = message.SubjectId
                    },
                    new Observation
                    {
                        SubjectId = message.SubjectId
                    });

                await dbContext.SaveChangesAsync();

                await service.CheckComplete(message.ReleaseId, message, dbContext);
            }

            MockUtils.VerifyAllMocks(importStatusService, batchService);
        }

        [Fact]
        public async Task CheckComplete_LastBatchFileCompleted_HasErrors()
        {
            var message = new ImportObservationsMessage
            {
                ReleaseId = Guid.NewGuid(),
                NumBatches = 2,
                DataFileName = "my_data_file.csv",
                TotalRows = 2,
                SubjectId = Guid.NewGuid()
            };

            var importStatusService = new Mock<IImportStatusService>(Strict);
            var fileStorageService = new Mock<IFileStorageService>();

            var service = BuildFileImportService(
                importStatusService: importStatusService.Object,
                fileStorageService: fileStorageService.Object);

            importStatusService
                .Setup(s => s.GetImportStatus(message.ReleaseId, message.DataFileName))
                .ReturnsAsync(new ImportStatus
                {
                    Status = STAGE_4,
                    Errors = "an error"
                });

            fileStorageService
                .Setup(s => s.GetNumBatchesRemaining(message.ReleaseId, message.DataFileName))
                .ReturnsAsync(0);

            importStatusService
                .Setup(s => s.UpdateStatus(
                    message.ReleaseId, message.DataFileName, FAILED, 100))
                .Returns(Task.CompletedTask);

            var dbContext = StatisticsDbUtils.InMemoryStatisticsDbContext();

            await using (dbContext)
            {
                await dbContext.Observation.AddRangeAsync(
                    new Observation
                    {
                        SubjectId = message.SubjectId
                    },
                    new Observation
                    {
                        SubjectId = message.SubjectId
                    });

                await dbContext.SaveChangesAsync();

                await service.CheckComplete(message.ReleaseId, message, dbContext);
            }

            MockUtils.VerifyAllMocks(importStatusService, fileStorageService);
        }

        [Fact]
        public async Task CheckComplete_LastBatchFileCompleted_HasIncorrectObservationCount()
        {
            var message = new ImportObservationsMessage
            {
                ReleaseId = Guid.NewGuid(),
                NumBatches = 2,
                DataFileName = "my_data_file.csv",
                TotalRows = 3,
                SubjectId = Guid.NewGuid()
            };

            var importStatusService = new Mock<IImportStatusService>(Strict);
            var fileStorageService = new Mock<IFileStorageService>();
            var batchService = new Mock<IBatchService>();

            var service = BuildFileImportService(
                importStatusService: importStatusService.Object,
                fileStorageService: fileStorageService.Object,
                batchService: batchService.Object);

            importStatusService
                .Setup(s => s.GetImportStatus(message.ReleaseId, message.DataFileName))
                .ReturnsAsync(new ImportStatus
                {
                    Status = STAGE_4,
                });

            fileStorageService
                .Setup(s => s.GetNumBatchesRemaining(message.ReleaseId, message.DataFileName))
                .ReturnsAsync(0);

            batchService
                .Setup(s => s.FailImport(message.ReleaseId, message.DataFileName, new List<ValidationError>
                {
                    new ValidationError(
                        $"Number of observations inserted (2) " +
                        $"does not equal that expected ({message.TotalRows}) : Please delete & retry"
                    )
                }))
                .Returns(Task.CompletedTask);

            var dbContext = StatisticsDbUtils.InMemoryStatisticsDbContext();

            await using (dbContext)
            {
                await dbContext.Observation.AddRangeAsync(
                    new Observation
                    {
                        SubjectId = message.SubjectId
                    },
                    new Observation
                    {
                        SubjectId = message.SubjectId
                    });

                await dbContext.SaveChangesAsync();

                await service.CheckComplete(message.ReleaseId, message, dbContext);
            }

            MockUtils.VerifyAllMocks(importStatusService, fileStorageService, batchService);
        }

        [Fact]
        public async Task CheckComplete_SingleDataFileCompleted_AlreadyFinished()
        {
            await FinishedStatuses.ForEachAsync(async finishedStatus =>
            {
                var message = new ImportObservationsMessage
                {
                    ReleaseId = Guid.NewGuid(),
                    NumBatches = 1,
                    DataFileName = "my_data_file.csv",
                    TotalRows = 2,
                    SubjectId = Guid.NewGuid()
                };

                var importStatusService = new Mock<IImportStatusService>(Strict);

                var service = BuildFileImportService(
                    importStatusService: importStatusService.Object);

                importStatusService
                    .Setup(s => s.GetImportStatus(message.ReleaseId, message.DataFileName))
                    .ReturnsAsync(new ImportStatus
                    {
                        Status = finishedStatus
                    });

                var dbContext = StatisticsDbUtils.InMemoryStatisticsDbContext();

                await using (dbContext)
                {
                    await service.CheckComplete(message.ReleaseId, message, dbContext);
                }

                MockUtils.VerifyAllMocks(importStatusService);
            });
        }

        [Fact]
        public async Task CheckComplete_LastBatchFileCompleted_AlreadyFinished()
        {
            await FinishedStatuses.ForEachAsync(async finishedStatus =>
            {
                var message = new ImportObservationsMessage
                {
                    ReleaseId = Guid.NewGuid(),
                    NumBatches = 2,
                    DataFileName = "my_data_file.csv",
                    TotalRows = 2,
                    SubjectId = Guid.NewGuid()
                };

                var importStatusService = new Mock<IImportStatusService>(Strict);
                var fileStorageService = new Mock<IFileStorageService>();

                var service = BuildFileImportService(
                    importStatusService: importStatusService.Object,
                    fileStorageService: fileStorageService.Object);

                importStatusService
                    .Setup(s => s.GetImportStatus(message.ReleaseId, message.DataFileName))
                    .ReturnsAsync(new ImportStatus
                    {
                        Status = finishedStatus
                    });

                var dbContext = StatisticsDbUtils.InMemoryStatisticsDbContext();

                await using (dbContext)
                {
                    await service.CheckComplete(message.ReleaseId, message, dbContext);
                }

                MockUtils.VerifyAllMocks(importStatusService, fileStorageService);
            });
        }

        [Fact]
        public async Task CheckComplete_SingleDataFileCompleted_Aborting()
        {
            await AbortingStatuses.ForEachAsync(async abortingStatus =>
            {
                var message = new ImportObservationsMessage
                {
                    ReleaseId = Guid.NewGuid(),
                    NumBatches = 1,
                    DataFileName = "my_data_file.csv",
                    TotalRows = 2,
                    SubjectId = Guid.NewGuid()
                };

                var importStatusService = new Mock<IImportStatusService>(Strict);

                var service = BuildFileImportService(
                    importStatusService: importStatusService.Object);

                var currentStatus = new ImportStatus
                {
                    Status = abortingStatus
                };

                importStatusService
                    .Setup(s => s.GetImportStatus(message.ReleaseId, message.DataFileName))
                    .ReturnsAsync(currentStatus);

                importStatusService
                    .Setup(s => s.UpdateStatus(
                        message.ReleaseId, message.DataFileName, currentStatus.GetFinishingStateOfAbortProcess(), 100))
                    .Returns(Task.CompletedTask);

                var dbContext = StatisticsDbUtils.InMemoryStatisticsDbContext();

                await using (dbContext)
                {
                    await service.CheckComplete(message.ReleaseId, message, dbContext);
                }

                MockUtils.VerifyAllMocks(importStatusService);
            });
        }

        [Fact]
        public async Task CheckComplete_LastBatchFileCompleted_Aborting()
        {
            await AbortingStatuses.ForEachAsync(async abortingStatus =>
            {
                var message = new ImportObservationsMessage
                {
                    ReleaseId = Guid.NewGuid(),
                    NumBatches = 2,
                    DataFileName = "my_data_file.csv",
                    TotalRows = 2,
                    SubjectId = Guid.NewGuid()
                };

                var importStatusService = new Mock<IImportStatusService>(Strict);
                var fileStorageService = new Mock<IFileStorageService>();

                var service = BuildFileImportService(
                    importStatusService: importStatusService.Object,
                    fileStorageService: fileStorageService.Object);

                var currentStatus = new ImportStatus
                {
                    Status = abortingStatus
                };

                importStatusService
                    .Setup(s => s.GetImportStatus(message.ReleaseId, message.DataFileName))
                    .ReturnsAsync(currentStatus);

                importStatusService
                    .Setup(s => s.UpdateStatus(
                        message.ReleaseId, message.DataFileName, currentStatus.GetFinishingStateOfAbortProcess(), 100))
                    .Returns(Task.CompletedTask);

                var dbContext = StatisticsDbUtils.InMemoryStatisticsDbContext();

                await using (dbContext)
                {
                    await service.CheckComplete(message.ReleaseId, message, dbContext);
                }

                MockUtils.VerifyAllMocks(importStatusService, fileStorageService);
            });
        }

        private FileImportService BuildFileImportService(
            IFileStorageService fileStorageService = null,
            IImporterService importerService = null,
            IBatchService batchService = null,
            ILogger<FileImportService> logger = null,
            IImportStatusService importStatusService = null
            )
        {
            return new FileImportService(
                fileStorageService ?? new Mock<IFileStorageService>(Strict).Object,
                importerService ?? new Mock<IImporterService>(Strict).Object,
                batchService ?? new Mock<IBatchService>(Strict).Object,
                logger ?? new Mock<ILogger<FileImportService>>().Object,
                importStatusService ?? new Mock<IImportStatusService>(Strict).Object
                );
        }
    }
}