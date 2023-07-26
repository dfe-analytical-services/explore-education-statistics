#nullable enable
using System;
using System.IO;
using System.Reflection;
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
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Tests.Functions;

public class ProcessorStage1Tests
{
    private readonly string _contentDbContextId = Guid.NewGuid().ToString();
    private readonly string _statisticsDbContextId = Guid.NewGuid().ToString();

    [Fact]
    public async Task ProcessStage1()
    {
        await AssertStage1ItemsValidatedCorrectly(new SmallCsvStage1Scenario());
    }
    
    private async Task AssertStage1ItemsValidatedCorrectly(IProcessorStage1TestScenario scenario)
    {
        var metaFileUnderTest = scenario.GetFilenameUnderTest().Replace(".csv", ".meta.csv");

        var subject = new Subject
        {
            Id = scenario.GetSubjectId()
        };

        var import = new DataImport
        {
            Id = Guid.NewGuid(),
            SubjectId = subject.Id,
            File = new File
            {
                Id = Guid.NewGuid(),
                Filename = scenario.GetFilenameUnderTest(),
                Type = FileType.Data
            },
            MetaFile = new File
            {
                Id = Guid.NewGuid(),
                Filename = metaFileUnderTest,
                Type = FileType.Metadata
            },
            Status = DataImportStatus.STAGE_1
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

        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);

        var dataFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources" + Path.DirectorySeparatorChar + scenario.GetFilenameUnderTest());

        var metaFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources" + Path.DirectorySeparatorChar + metaFileUnderTest);

        privateBlobStorageService.SetupStreamBlob(
            PrivateReleaseFiles, 
            import.File.Path(), 
            dataFilePath);
        
        privateBlobStorageService.SetupStreamBlob(
            PrivateReleaseFiles, 
            import.MetaFile.Path(), 
            metaFilePath);

        var dbContextSupplier = new InMemoryDbContextSupplier(
            contentDbContextId: _contentDbContextId,
            statisticsDbContextId: _statisticsDbContextId);

        var transactionHelper = new InMemoryDatabaseHelper(dbContextSupplier);
        
        var dataImportService = new DataImportService(
            dbContextSupplier,
            Mock.Of<ILogger<DataImportService>>());

        var importerLocationCache = new ImporterLocationCache(Mock.Of<ILogger<ImporterLocationCache>>());
        
        var guidGenerator = new SequentialGuidGenerator();

        var importerMetaService = new ImporterMetaService(guidGenerator, transactionHelper);
        
        var importerService = new ImporterService(
            guidGenerator,
            new ImporterLocationService(
                guidGenerator, 
                importerLocationCache,
                Mock.Of<ILogger<ImporterLocationCache>>()),
            importerMetaService,
            dataImportService,
            Mock.Of<ILogger<ImporterService>>(),
            transactionHelper);

        var fileImportService = new FileImportService(
            Mock.Of<ILogger<FileImportService>>(),
            privateBlobStorageService.Object,
            dataImportService,
            importerService);

        var validatorService = new ValidatorService(
            Mock.Of<ILogger<ValidatorService>>(),
            privateBlobStorageService.Object,
            new FileTypeService(Mock.Of<ILogger<FileTypeService>>()),
            dataImportService);

        var processorService = BuildProcessorService(
            dbContextSupplier,
            dataImportService: dataImportService,
            privateBlobStorageService: privateBlobStorageService.Object,
            importerService: importerService,
            fileImportService: fileImportService,
            validatorService: validatorService);
        
        var importMessage = new ImportMessage(import.Id);

        var importStagesMessageQueue = new Mock<ICollector<ImportMessage>>(Strict);

        importStagesMessageQueue
            .Setup(s => s.Add(importMessage));

        var function = BuildFunction(processorService, dataImportService);
        
        await function.ProcessUploads(
            importMessage, 
            new ExecutionContext(),
            importStagesMessageQueue.Object);
        
        VerifyAllMocks(privateBlobStorageService, importStagesMessageQueue);

        await using (var contentDbContext = InMemoryContentDbContext(_contentDbContextId))
        {
            var dataImports = await contentDbContext
                .DataImports
                .ToListAsync();
            
            // Verify that the import status has moved onto Stage 3.
            dataImports.ForEach(dataImport =>
            {
                Assert.Equal(DataImportStatus.STAGE_2, dataImport.Status);
                Assert.Equal(scenario.GetExpectedTotalRows(), dataImport.TotalRows);
                Assert.Equal(scenario.GetExpectedTotalRows(), dataImport.ExpectedImportedRows);
            });
        }
    }

    private static ProcessorService BuildProcessorService(
        IDbContextSupplier dbContextSupplier,
        IDataImportService? dataImportService = null,
        IPrivateBlobStorageService? privateBlobStorageService = null,
        IImporterService? importerService = null,
        IFileImportService? fileImportService = null,
        IValidatorService? validatorService = null)
    {
        return new ProcessorService(
            Mock.Of<ILogger<ProcessorService>>(),
            privateBlobStorageService ?? Mock.Of<IPrivateBlobStorageService>(Strict),
            fileImportService ?? Mock.Of<IFileImportService>(Strict),
            importerService ?? Mock.Of<IImporterService>(Strict),
            dataImportService ?? Mock.Of<IDataImportService>(Strict),
            validatorService ?? Mock.Of<IValidatorService>(Strict),
            Mock.Of<IDataArchiveService>(Strict),
            dbContextSupplier);
    }

    private static Processor.Functions.Processor BuildFunction(
        IProcessorService? processorService = null,
        IDataImportService? dataImportService = null)
    {
        return new Processor.Functions.Processor(
            dataImportService ?? Mock.Of<IDataImportService>(Strict),
            processorService ?? Mock.Of<IProcessorService>(Strict),
            Mock.Of<ILogger<Processor.Functions.Processor>>(),
            rethrowExceptions: true);
    }
}
