using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
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
        public async Task CheckComplete()
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
                Status = STAGE_3,
                ExpectedImportedRows = 2
            };

            var dataImportService = new Mock<IDataImportService>(Strict);

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
                await service.CheckComplete(import, statisticsDbContext);
            }

            VerifyAllMocks(dataImportService);
        }

        [Fact]
        public async Task CheckComplete_Errors()
        {
            var file = new File
            {
                Id = Guid.NewGuid(),
                Filename = "my_data_file.csv"
            };

            var import = new DataImport
            {
                Id = Guid.NewGuid(),
                Errors = ListOf(new DataImportError("an error")),
                FileId = file.Id,
                File = file,
                SubjectId = Guid.NewGuid(),
                Status = STAGE_3,
                ExpectedImportedRows = 2
            };

            var dataImportService = new Mock<IDataImportService>(Strict);

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
                await service.CheckComplete(import, statisticsDbContext);
            }

            VerifyAllMocks(dataImportService);
        }

        [Fact]
        public async Task CheckComplete_IncorrectObservationCount()
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
                Status = STAGE_3,
                ExpectedImportedRows = 3
            };

            var dataImportService = new Mock<IDataImportService>(Strict);

            dataImportService
                .Setup(s => s.FailImport(import.Id, 
                    $"Number of observations inserted (2) does not equal that expected ({import.ExpectedImportedRows}) : Please delete & retry"))
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
                await service.CheckComplete(import, statisticsDbContext);
            }

            VerifyAllMocks(dataImportService);
        }

        [Fact]
        public async Task CheckComplete_AlreadyFinished()
        {
            // We don't expect to see any further import status updates if the current status is in
            // any "finished" state  
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
                        ExpectedImportedRows = 2
                    };

                    var statisticsDbContextId = Guid.NewGuid().ToString();

                    await using var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId);
                    var service = BuildFileImportService();
                    await service.CheckComplete(import, statisticsDbContext);
                });
        }

        [Fact]
        public async Task CheckComplete_Aborting()
        {
            // We expect to see a final import status update if the current status is in an
            // "aborting" state, updating the import status to be in the associated final
            // "aborted" state.
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
                        ExpectedImportedRows = 2
                    };

                    var dataImportService = new Mock<IDataImportService>(Strict);

                    dataImportService
                        .Setup(s => s.UpdateStatus(
                            import.Id, abortingStatus.GetFinishingStateOfAbortProcess(), 100))
                        .Returns(Task.CompletedTask);

                    var statisticsDbContextId = Guid.NewGuid().ToString();

                    await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
                    {
                        var service = BuildFileImportService(dataImportService: dataImportService.Object);
                        await service.CheckComplete(import, statisticsDbContext);
                    }

                    VerifyAllMocks(dataImportService);
                });
        }
        
        private static FileImportService BuildFileImportService(
            IPrivateBlobStorageService privateBlobStorageService = null,
            IImporterService importerService = null,
            ILogger<FileImportService> logger = null,
            IDataImportService dataImportService = null)
        {
            return new FileImportService(
                logger ?? Mock.Of<ILogger<FileImportService>>(),
                privateBlobStorageService ?? Mock.Of<IPrivateBlobStorageService>(Strict),
                dataImportService ?? Mock.Of<IDataImportService>(Strict),
                importerService ?? Mock.Of<IImporterService>(Strict));
        }
    }
}
