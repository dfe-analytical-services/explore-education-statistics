#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
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
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions.CaptureUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Tests.Functions;

public class ProcessorStage3Tests
{
    private readonly string _contentDbContextId = Guid.NewGuid().ToString();
    private readonly string _statisticsDbContextId = Guid.NewGuid().ToString();

    [Fact]
    public async Task ProcessStage3_NoBatchingRequired()
    {
        var dataFileUnderTest = "stage3-batching-test.csv";

        var subject = new Subject
        {
            Id = Guid.NewGuid()
        };

        // This data import has the exact number of rows that are in the file under test, so no batching will be
        // necessary.
        var import = new DataImport
        {
            Id = Guid.NewGuid(),
            SubjectId = subject.Id,
            File = new File
            {
                Id = Guid.NewGuid(),
                Filename = dataFileUnderTest
            },
            MetaFile = new File
            {
                Id = Guid.NewGuid(),
                Filename = dataFileUnderTest
            },
            RowsPerBatch = 16,
            Status = DataImportStatus.STAGE_3
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

        var blobStorageService = new Mock<IBlobStorageService>(Strict);

        var dataFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources" + Path.DirectorySeparatorChar + dataFileUnderTest);

        blobStorageService
            .Setup(s => s.StreamBlob(PrivateReleaseFiles, import.File.Path(), null))
            .ReturnsAsync(() => System.IO.File.OpenRead(dataFilePath));

        var dbContextSupplier = new InMemoryDbContextSupplier(
            contentDbContextId: _contentDbContextId,
            statisticsDbContextId: _statisticsDbContextId);
        
        var dataImportService = new DataImportService(
            dbContextSupplier,
            Mock.Of<ILogger<DataImportService>>(),
            new InMemoryDatabaseHelper());

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
        
        var importStagesMessageQueue = new Mock<ICollector<ImportMessage>>();
        var datafileProcessingMessageQueue = new Mock<ICollector<ImportObservationsMessage>>();

        var function = BuildFunction(
            processorService: processorService, 
            dataImportService: dataImportService);

        await function.ProcessUploads(
            new ImportMessage(import.Id), 
            null,
            importStagesMessageQueue.Object,
            datafileProcessingMessageQueue.Object);
        
        // Verify that the only interaction with blob storage was to stream it rather than upload any batch files.
        VerifyAllMocks(blobStorageService);

        await using (var contentDbContext = InMemoryContentDbContext(_contentDbContextId))
        {
            var dataImport = await contentDbContext
                .DataImports
                .SingleAsync();
            
            // Verify that the import status has moved onto Stage 4.
            Assert.Equal(DataImportStatus.STAGE_4, dataImport.Status);
        }
    }
    
    [Fact]
    public async Task ProcessStage3_BatchingRequired()
    {
        var dataFileUnderTest = "stage3-batching-test.csv";

        var subject = new Subject
        {
            Id = Guid.NewGuid()
        };

        // The CSV under test has more rows than this import is asking for per batch, and so batching will be required.
        // Specifically, we expect 4 batch files to be created, with 5, 5, 5 and 1 row respectively within them, not
        // including the CSV column headers that are also included in each CSV file.
        var import = new DataImport
        {
            Id = Guid.NewGuid(),
            SubjectId = subject.Id,
            File = new File
            {
                Id = Guid.NewGuid(),
                Filename = dataFileUnderTest
            },
            MetaFile = new File
            {
                Id = Guid.NewGuid(),
                Filename = dataFileUnderTest
            },
            RowsPerBatch = 5,
            Status = DataImportStatus.STAGE_3
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

        var blobStorageService = new Mock<IBlobStorageService>(Strict);

        var dataFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources" + Path.DirectorySeparatorChar + dataFileUnderTest);

        blobStorageService
            .Setup(s => s.StreamBlob(PrivateReleaseFiles, import.File.Path(), null))
            .ReturnsAsync(() => System.IO.File.OpenRead(dataFilePath));

        // Expect that we'll try to find out which batch files have already been created. In this scenario, none have
        // yet been created.
        blobStorageService
            .Setup(s => s.ListBlobs(PrivateReleaseFiles, import.File.BatchesPath()))
            .ReturnsAsync(() => new List<BlobInfo>());

        var uploadedFileContents = new List<string[]>();
        
        // Expect that 4 batch files are uploaded.
        Enumerable.Range(1, 4).ForEach(i =>
        {
            blobStorageService
                .Setup(s => s.UploadStream(
                    PrivateReleaseFiles, 
                    $"{import.File.BatchesPath()}{import.File.Id}_00000{i}", 
                    Capture.With(CaptureStreamAsArrayOfLines(lines => 
                        uploadedFileContents.Add(lines))), 
                    "text/csv"))
                .Returns(Task.CompletedTask);
        });
        
        var dbContextSupplier = new InMemoryDbContextSupplier(
            contentDbContextId: _contentDbContextId,
            statisticsDbContextId: _statisticsDbContextId);
        
        var dataImportService = new DataImportService(
            dbContextSupplier,
            Mock.Of<ILogger<DataImportService>>(),
            new InMemoryDatabaseHelper());

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
        
        var importStagesMessageQueue = new Mock<ICollector<ImportMessage>>();
        var datafileProcessingMessageQueue = new Mock<ICollector<ImportObservationsMessage>>();

        var function = BuildFunction(
            processorService: processorService, 
            dataImportService: dataImportService);

        await function.ProcessUploads(
            new ImportMessage(import.Id), 
            null,
            importStagesMessageQueue.Object,
            datafileProcessingMessageQueue.Object);
        
        // Verify all of the streaming an uploading interactions with blob storage.
        VerifyAllMocks(blobStorageService);
        
        // Verify the contents of the streams in each of the batch files.
        Assert.Equal(4, uploadedFileContents.Count);
        
        // Verify that the last line of each batch file is simply a blank line created by the final data row's
        // trailing newline.
        uploadedFileContents.ForEach(content => Assert.Equal("", content.Last()));

        // Get the final column's contents of each uploaded file - we'll use these to verify the expected contents of
        // each, as the final column of the file under test has easily identified contents.
        var lastColumnContentsOfEachBatchFile = uploadedFileContents
            .Select(content => content
                .Where(line => line != "")
                .Select(line => line[(line.LastIndexOf(',') + 1)..])
                .ToList())
            .ToList();
        
        Assert.Equal(ListOf("ind_one", "1", "2", "3", "4", "5"), lastColumnContentsOfEachBatchFile[0]);
        Assert.Equal(ListOf("ind_one", "6", "7", "8", "9", "10"), lastColumnContentsOfEachBatchFile[1]);
        Assert.Equal(ListOf("ind_one", "11", "12", "13", "14", "15"), lastColumnContentsOfEachBatchFile[2]);
        Assert.Equal(ListOf("ind_one", "16"), lastColumnContentsOfEachBatchFile[3]);
        
        await using (var contentDbContext = InMemoryContentDbContext(_contentDbContextId))
        {
            var dataImport = await contentDbContext
                .DataImports
                .SingleAsync();
            
            // Verify that the import status has moved onto Stage 4.
            Assert.Equal(DataImportStatus.STAGE_4, dataImport.Status);
        }
    }
    
    [Fact]
    public async Task ProcessStage3_BatchingRequired_SomeBatchesAlreadyCreated()
    {
        var dataFileUnderTest = "stage3-batching-test.csv";

        var subject = new Subject
        {
            Id = Guid.NewGuid()
        };

        // The CSV under test has more rows than this import is asking for per batch, and so batching will be required.
        // Specifically, we expect 4 batch files to be created, with 5, 5, 5 and 1 row respectively within them, not
        // including the CSV column headers that are also included in each CSV file.
        var import = new DataImport
        {
            Id = Guid.NewGuid(),
            SubjectId = subject.Id,
            File = new File
            {
                Id = Guid.NewGuid(),
                Filename = dataFileUnderTest
            },
            MetaFile = new File
            {
                Id = Guid.NewGuid(),
                Filename = dataFileUnderTest
            },
            RowsPerBatch = 5,
            Status = DataImportStatus.STAGE_3
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

        var blobStorageService = new Mock<IBlobStorageService>(Strict);

        var dataFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources" + Path.DirectorySeparatorChar + dataFileUnderTest);

        blobStorageService
            .Setup(s => s.StreamBlob(PrivateReleaseFiles, import.File.Path(), null))
            .ReturnsAsync(() => System.IO.File.OpenRead(dataFilePath));

        // Expect that we'll try to find out which batch files have already been created. In this scenario, 2 of the 4
        // batches are already previously created, probably by the import Function being killed mid-run.
        var preExistingBatchFiles = ListOf(
            new BlobInfo($"{import.File.BatchesPath()}{import.File.Id}_000001", "text/csv", 0),
            new BlobInfo($"{import.File.BatchesPath()}{import.File.Id}_000002", "text/csv", 0)
        );
        
        blobStorageService
            .Setup(s => s.ListBlobs(PrivateReleaseFiles, import.File.BatchesPath()))
            .ReturnsAsync(() => preExistingBatchFiles);

        var uploadedFileContents = new List<string[]>();
        
        // Expect that 2 batch files are uploaded, not counting those that are already generated.
        Enumerable.Range(3, 2).ForEach(i =>
        {
            blobStorageService
                .Setup(s => s.UploadStream(
                    PrivateReleaseFiles, 
                    $"{import.File.BatchesPath()}{import.File.Id}_00000{i}", 
                    Capture.With(CaptureStreamAsArrayOfLines(lines => 
                                uploadedFileContents.Add(lines))), 
                    "text/csv"))
                .Returns(Task.CompletedTask);
        });
        
        var dbContextSupplier = new InMemoryDbContextSupplier(
            contentDbContextId: _contentDbContextId,
            statisticsDbContextId: _statisticsDbContextId);
        
        var dataImportService = new DataImportService(
            dbContextSupplier,
            Mock.Of<ILogger<DataImportService>>(),
            new InMemoryDatabaseHelper());

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
        
        var importStagesMessageQueue = new Mock<ICollector<ImportMessage>>();
        var datafileProcessingMessageQueue = new Mock<ICollector<ImportObservationsMessage>>();

        var function = BuildFunction(
            processorService: processorService, 
            dataImportService: dataImportService);

        await function.ProcessUploads(
            new ImportMessage(import.Id), 
            null,
            importStagesMessageQueue.Object,
            datafileProcessingMessageQueue.Object);
        
        // Verify all of the streaming an uploading interactions with blob storage.
        VerifyAllMocks(blobStorageService);
        
        // Verify the contents of the streams in each of the batch files.
        Assert.Equal(2, uploadedFileContents.Count);
        
        // Verify that the last line of each batch file is simply a blank line created by the final data row's
        // trailing newline.
        uploadedFileContents.ForEach(content => Assert.Equal("", content.Last()));

        // Get the final column's contents of each uploaded file - we'll use these to verify the expected contents of
        // each, as the final column of the file under test has easily identified contents.
        var lastColumnContentsOfEachBatchFile = uploadedFileContents
            .Select(content => content
                .Where(line => line != "")
                .Select(line => line[(line.LastIndexOf(',') + 1)..])
                .ToList())
            .ToList();
        
        Assert.Equal(ListOf("ind_one", "11", "12", "13", "14", "15"), lastColumnContentsOfEachBatchFile[0]);
        Assert.Equal(ListOf("ind_one", "16"), lastColumnContentsOfEachBatchFile[1]);
        
        await using (var contentDbContext = InMemoryContentDbContext(_contentDbContextId))
        {
            var dataImport = await contentDbContext
                .DataImports
                .SingleAsync();
            
            // Verify that the import status has moved onto Stage 4.
            Assert.Equal(DataImportStatus.STAGE_4, dataImport.Status);
        }
    }
    
    [Fact]
    public async Task ProcessStage3_BatchingRequired_AllBatchesAlreadyCreated()
    {
        var dataFileUnderTest = "stage3-batching-test.csv";

        var subject = new Subject
        {
            Id = Guid.NewGuid()
        };

        // The CSV under test has more rows than this import is asking for per batch, and so batching will be required.
        // Specifically, we expect 4 batch files to be created, with 5, 5, 5 and 1 row respectively within them, not
        // including the CSV column headers that are also included in each CSV file.
        var import = new DataImport
        {
            Id = Guid.NewGuid(),
            SubjectId = subject.Id,
            File = new File
            {
                Id = Guid.NewGuid(),
                Filename = dataFileUnderTest
            },
            MetaFile = new File
            {
                Id = Guid.NewGuid(),
                Filename = dataFileUnderTest
            },
            RowsPerBatch = 5,
            Status = DataImportStatus.STAGE_3
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

        var blobStorageService = new Mock<IBlobStorageService>(Strict);

        var dataFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources" + Path.DirectorySeparatorChar + dataFileUnderTest);

        blobStorageService
            .Setup(s => s.StreamBlob(PrivateReleaseFiles, import.File.Path(), null))
            .ReturnsAsync(() => System.IO.File.OpenRead(dataFilePath));

        // Expect that we'll try to find out which batch files have already been created. In this scenario, all batch
        // files are already created, probably due to the import Function being killed right at the end of Stage 3.
        var preExistingBatchFiles = Enumerable.Range(1, 4)
            .Select(i => new BlobInfo(
                $"{import.File.BatchesPath()}{import.File.Id}_00000{i}", 
                "text/csv", 
                0))
            .ToList();
        
        blobStorageService
            .Setup(s => s.ListBlobs(PrivateReleaseFiles, import.File.BatchesPath()))
            .ReturnsAsync(() => preExistingBatchFiles);

        var dbContextSupplier = new InMemoryDbContextSupplier(
            contentDbContextId: _contentDbContextId,
            statisticsDbContextId: _statisticsDbContextId);
        
        var dataImportService = new DataImportService(
            dbContextSupplier,
            Mock.Of<ILogger<DataImportService>>(),
            new InMemoryDatabaseHelper());

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
        
        var importStagesMessageQueue = new Mock<ICollector<ImportMessage>>();
        var datafileProcessingMessageQueue = new Mock<ICollector<ImportObservationsMessage>>();

        var function = BuildFunction(
            processorService: processorService, 
            dataImportService: dataImportService);

        await function.ProcessUploads(
            new ImportMessage(import.Id), 
            null,
            importStagesMessageQueue.Object,
            datafileProcessingMessageQueue.Object);
        
        // Verify the streaming interaction with blob storage but with no uploads, as all batch files are already
        // created.
        VerifyAllMocks(blobStorageService);
        
        await using (var contentDbContext = InMemoryContentDbContext(_contentDbContextId))
        {
            var dataImport = await contentDbContext
                .DataImports
                .SingleAsync();
            
            // Verify that the import status has moved onto Stage 4.
            Assert.Equal(DataImportStatus.STAGE_4, dataImport.Status);
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
            Mock.Of<ILogger<Processor.Functions.Processor>>());
    }
}