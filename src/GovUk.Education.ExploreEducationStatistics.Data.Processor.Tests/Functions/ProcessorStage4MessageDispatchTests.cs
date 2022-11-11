#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Tests.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Tests.Functions;

public class ProcessorStage4MessageDispatchTests
{
    private readonly string _contentDbContextId = Guid.NewGuid().ToString();
    private readonly string _statisticsDbContextId = Guid.NewGuid().ToString();

    [Fact]
    public async Task ProcessStage4_NoBatchingWasRequiredAtStage3()
    {
        var dataFileUnderTest = "stage4.csv";

        var subject = new Subject
        {
            Id = Guid.NewGuid()
        };

        // The total rows to attempt to import is the same as the RowsPerBatch, and therefore the file did not require
        // batching at Stage 3.
        var import = new DataImport
        {
            Id = Guid.NewGuid(),
            SubjectId = subject.Id,
            File = new File
            {
                Id = Guid.NewGuid(),
                Filename = dataFileUnderTest,
                Type = FileType.Data
            },
            MetaFile = new File
            {
                Id = Guid.NewGuid(),
                Filename = dataFileUnderTest.Replace(".csv", ".meta.csv"),
                Type = FileType.Metadata
            },
            Status = DataImportStatus.STAGE_4,
            RowsPerBatch = 10,
            TotalRows = 10,
            NumBatches = 1
        };

        await using (var contentDbContext = InMemoryContentDbContext(_contentDbContextId))
        {
            await contentDbContext.DataImports.AddAsync(import);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId))
        {
            await statisticsDbContext.Subject.AddAsync(subject);
            await statisticsDbContext.SaveChangesAsync();
        }

        var dbContextSupplier = new InMemoryDbContextSupplier(
            contentDbContextId: _contentDbContextId,
            statisticsDbContextId: _statisticsDbContextId);
        
        var dataImportService = new DataImportService(
            dbContextSupplier,
            Mock.Of<ILogger<DataImportService>>(),
            new InMemoryDatabaseHelper(dbContextSupplier));

        var splitFileService = new SplitFileService(
            new BatchService(Mock.Of<IBlobStorageService>(Strict)),
            Mock.Of<IBlobStorageService>(Strict),
            Mock.Of<ILogger<SplitFileService>>(),
            dataImportService);

        var processorService = new ProcessorService(
            Mock.Of<ILogger<ProcessorService>>(Strict),
            Mock.Of<IBlobStorageService>(Strict),
            Mock.Of<IFileImportService>(Strict),
            splitFileService,
            Mock.Of<IImporterService>(Strict),
            dataImportService,
            Mock.Of<IValidatorService>(Strict),
            Mock.Of<IDataArchiveService>(Strict),
            dbContextSupplier);
        
        var datafileProcessingMessageQueue = new Mock<ICollector<ImportObservationsMessage>>(Strict);

        var expectedDataFileProcessingMessage = new ImportObservationsMessage
        {
            Id = import.Id,
            BatchNo = 1,
            ObservationsFilePath = import.File.Path()
        };

        // Expect a message to be added to the queue to process the main data file directly.
        datafileProcessingMessageQueue
            .Setup(s => s.Add(ItIs.DeepEqualTo(expectedDataFileProcessingMessage)));
            
        var function = BuildFunction(
            processorService: processorService, 
            dataImportService: dataImportService);

        await function.ProcessUploads(
            new ImportMessage(import.Id), 
            new ExecutionContext(),
            Mock.Of<ICollector<ImportMessage>>(Strict),
            datafileProcessingMessageQueue.Object);
        
        VerifyAllMocks(datafileProcessingMessageQueue);
        
        await using (var contentDbContext = InMemoryContentDbContext(_contentDbContextId))
        {
            var dataImport = await contentDbContext
                .DataImports
                .SingleAsync();
            
            // Verify that the import status has stayed at Stage 4 for the actual import of Observations that will
            // come next.
            Assert.Equal(DataImportStatus.STAGE_4, dataImport.Status);
        }
    }
    
    [Fact]
    public async Task ProcessStage4_BatchingWasRequiredAtStage3()
    {
        var dataFileUnderTest = "stage4.csv";

        var subject = new Subject
        {
            Id = Guid.NewGuid()
        };

        // The total rows to attempt to import is larger than the RowsPerBatch, and therefore the file required
        // batching at Stage 3.
        var import = new DataImport
        {
            Id = Guid.NewGuid(),
            SubjectId = subject.Id,
            File = new File
            {
                Id = Guid.NewGuid(),
                Filename = dataFileUnderTest,
                Type = FileType.Data
            },
            MetaFile = new File
            {
                Id = Guid.NewGuid(),
                Filename = dataFileUnderTest.Replace(".csv", ".meta.csv"),
                Type = FileType.Metadata
            },
            Status = DataImportStatus.STAGE_4,
            RowsPerBatch = 10,
            TotalRows = 11,
            NumBatches = 2
        };

        await using (var contentDbContext = InMemoryContentDbContext(_contentDbContextId))
        {
            await contentDbContext.DataImports.AddAsync(import);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId))
        {
            await statisticsDbContext.Subject.AddAsync(subject);
            await statisticsDbContext.SaveChangesAsync();
        }

        // Expect a call to Blob Storage to list the available batch files which have not yet been imported.
        var blobStorageService = new Mock<IBlobStorageService>(Strict);

        var availableBatchFiles = ListOf(
            new BlobInfo($"{import.File.BatchesPath()}{import.File.Id}_000001", "text/csv", 0),
            new BlobInfo($"{import.File.BatchesPath()}{import.File.Id}_000002", "text/csv", 0)
        );
        
        blobStorageService.SetupListBlobs(
            PrivateReleaseFiles, 
            import.File.BatchesPath(), 
            availableBatchFiles);

        var dbContextSupplier = new InMemoryDbContextSupplier(
            contentDbContextId: _contentDbContextId,
            statisticsDbContextId: _statisticsDbContextId);
        
        var dataImportService = new DataImportService(
            dbContextSupplier,
            Mock.Of<ILogger<DataImportService>>(),
            new InMemoryDatabaseHelper(dbContextSupplier));

        var splitFileService = new SplitFileService(
            new BatchService(blobStorageService.Object),
            blobStorageService.Object,
            Mock.Of<ILogger<SplitFileService>>(),
            dataImportService);

        var processorService = new ProcessorService(
            Mock.Of<ILogger<ProcessorService>>(Strict),
            blobStorageService.Object,
            Mock.Of<IFileImportService>(Strict),
            splitFileService,
            Mock.Of<IImporterService>(Strict),
            dataImportService,
            Mock.Of<IValidatorService>(Strict),
            Mock.Of<IDataArchiveService>(Strict),
            dbContextSupplier);
        
        // Expect one import message per batch file that was found to be available.
        var datafileProcessingMessageQueue = new Mock<ICollector<ImportObservationsMessage>>(Strict);

        var batchFile1ProcessingMessage = new ImportObservationsMessage
        {
            Id = import.Id,
            BatchNo = 1,
            ObservationsFilePath = availableBatchFiles[0].Path
        };
        
        var batchFile2ProcessingMessage = new ImportObservationsMessage
        {
            Id = import.Id,
            BatchNo = 2,
            ObservationsFilePath = availableBatchFiles[1].Path
        };
        
        datafileProcessingMessageQueue
            .Setup(s => s.Add(ItIs.DeepEqualTo(batchFile1ProcessingMessage)));
        
        datafileProcessingMessageQueue
            .Setup(s => s.Add(ItIs.DeepEqualTo(batchFile2ProcessingMessage)));

        var function = BuildFunction(
            processorService: processorService, 
            dataImportService: dataImportService);

        await function.ProcessUploads(
            new ImportMessage(import.Id), 
            new ExecutionContext(),
            Mock.Of<ICollector<ImportMessage>>(Strict),
            datafileProcessingMessageQueue.Object);
        
        VerifyAllMocks(blobStorageService, datafileProcessingMessageQueue);
        
        await using (var contentDbContext = InMemoryContentDbContext(_contentDbContextId))
        {
            var dataImport = await contentDbContext
                .DataImports
                .SingleAsync();
            
            // Verify that the import status has stayed at Stage 4 for the actual import of Observations that will
            // come next.
            Assert.Equal(DataImportStatus.STAGE_4, dataImport.Status);
        }
    }
    
    [Fact]
    public async Task ProcessStage4_BatchingWasRequiredAtStage3_ButAllBatchFilesImported()
    {
        var dataFileUnderTest = "stage4.csv";

        var subject = new Subject
        {
            Id = Guid.NewGuid()
        };

        // The total rows to attempt to import is larger than the RowsPerBatch, and therefore the file required
        // batching at Stage 3.
        var import = new DataImport
        {
            Id = Guid.NewGuid(),
            SubjectId = subject.Id,
            File = new File
            {
                Id = Guid.NewGuid(),
                Filename = dataFileUnderTest,
                Type = FileType.Data
            },
            MetaFile = new File
            {
                Id = Guid.NewGuid(),
                Filename = dataFileUnderTest.Replace(".csv", ".meta.csv"),
                Type = FileType.Metadata
            },
            Status = DataImportStatus.STAGE_4,
            RowsPerBatch = 10,
            TotalRows = 11,
            NumBatches = 2
        };

        await using (var contentDbContext = InMemoryContentDbContext(_contentDbContextId))
        {
            await contentDbContext.DataImports.AddAsync(import);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId))
        {
            await statisticsDbContext.Subject.AddAsync(subject);
            await statisticsDbContext.SaveChangesAsync();
        }

        // Expect a call to Blob Storage to list the available batch files which have not yet been imported.  However,
        // no batch files remain to be processed.  This would likely happen only if the importer process was killed 
        // right at the end of its Stage 4 import run. 
        var blobStorageService = new Mock<IBlobStorageService>(Strict);

        blobStorageService.SetupListBlobs(PrivateReleaseFiles, import.File.BatchesPath());

        var dbContextSupplier = new InMemoryDbContextSupplier(
            contentDbContextId: _contentDbContextId,
            statisticsDbContextId: _statisticsDbContextId);
        
        var dataImportService = new DataImportService(
            dbContextSupplier,
            Mock.Of<ILogger<DataImportService>>(),
            new InMemoryDatabaseHelper(dbContextSupplier));

        var splitFileService = new SplitFileService(
            new BatchService(blobStorageService.Object),
            blobStorageService.Object,
            Mock.Of<ILogger<SplitFileService>>(),
            dataImportService);

        var processorService = new ProcessorService(
            Mock.Of<ILogger<ProcessorService>>(Strict),
            blobStorageService.Object,
            Mock.Of<IFileImportService>(Strict),
            splitFileService,
            Mock.Of<IImporterService>(Strict),
            dataImportService,
            Mock.Of<IValidatorService>(Strict),
            Mock.Of<IDataArchiveService>(Strict),
            dbContextSupplier);
        
        var function = BuildFunction(
            processorService: processorService, 
            dataImportService: dataImportService);

        await function.ProcessUploads(
            new ImportMessage(import.Id), 
            new ExecutionContext(),
            Mock.Of<ICollector<ImportMessage>>(Strict),
            Mock.Of<ICollector<ImportObservationsMessage>>(Strict));
        
        VerifyAllMocks(blobStorageService);
        
        await using (var contentDbContext = InMemoryContentDbContext(_contentDbContextId))
        {
            var dataImport = await contentDbContext
                .DataImports
                .SingleAsync();
            
            // Verify that the import status has been marked as COMPLETED, as there are no batch files remaining
            // to import.
            Assert.Equal(DataImportStatus.COMPLETE, dataImport.Status);
        }
    }
    
    private static Processor.Functions.Processor BuildFunction(
        IProcessorService? processorService = null,
        IDataImportService? dataImportService = null)
    {
        return new Processor.Functions.Processor(
            Mock.Of<IFileImportService>(Strict),
            dataImportService ?? Mock.Of<IDataImportService>(Strict),
            processorService ?? Mock.Of<IProcessorService>(Strict),
            Mock.Of<IDbContextSupplier>(),
            Mock.Of<ILogger<Processor.Functions.Processor>>());
    }
}