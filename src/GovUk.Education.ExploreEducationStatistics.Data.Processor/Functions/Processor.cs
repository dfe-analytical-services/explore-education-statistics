using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Functions
{
    // ReSharper disable once UnusedMember.Global
    public class Processor
    {
        private readonly IBatchService _batchService;
        private readonly IFileImportService _fileImportService;
        private readonly IFileStorageService _fileStorageService;
        private readonly IImporterService _importerService;
        private readonly IReleaseProcessorService _releaseProcessorService;
        private readonly ISplitFileService _splitFileService;
        private readonly IValidatorService _validatorService;

        public Processor(
            IFileImportService fileImportService,
            IReleaseProcessorService releaseProcessorService,
            IFileStorageService fileStorageService,
            ISplitFileService splitFileService,
            IImporterService importerService,
            IBatchService batchService,
            IValidatorService validatorService
        )
        {
            _fileImportService = fileImportService;
            _releaseProcessorService = releaseProcessorService;
            _fileStorageService = fileStorageService;
            _splitFileService = splitFileService;
            _importerService = importerService;
            _batchService = batchService;
            _validatorService = validatorService;
        }

        [FunctionName("ProcessUploads")]
        public async void ProcessUploads(
            [QueueTrigger("imports-pending")] ImportMessage message,
            ILogger logger,
            ExecutionContext context,
            [Queue("imports-available")] ICollector<ImportMessage> collector
        )
        {
            logger.LogInformation($"Validating Datafile: {message.DataFileName}");

            if (message.Seeding)
            {
                await _batchService.CreateImport(message.Release.Id.ToString(), message.DataFileName, 0, message);
            }

            await _batchService.UpdateStatus(message.Release.Id.ToString(), message.DataFileName, IStatus.RUNNING_PHASE_1);
            
            var (errors, totalObservationCount, filteredObservationCount) =
                _validatorService.ValidateAndCountRows(await _fileStorageService.GetSubjectData(message));

            if (errors.Count == 0)
            {
                logger.LogInformation($"Validating Datafile: {message.OrigDataFileName} - complete");

                try
                {
                    message.RowsPerBatch = Convert.ToInt32(LoadAppSettings(context).GetValue<string>("RowsPerBatch"));
                    message.TotalRows = filteredObservationCount;
                    message.NumBatches = FileStorageUtils.GetNumBatches(totalObservationCount, message.RowsPerBatch);

                    await _batchService.UpdateStoredMessage(message);
                    
                    logger.LogInformation($"Datafile: {message.DataFileName} is valid");
                    var subjectData = await ProcessSubject(message, DbUtils.CreateDbContext());
                    logger.LogInformation($"Splitting Datafile: {message.DataFileName} if > {message.RowsPerBatch} lines");
                    await _splitFileService.SplitDataFile(collector, message, subjectData);
                    logger.LogInformation($"Split of Datafile: {message.DataFileName} - complete");
                }
                catch (Exception e)
                {
                    var ex = GetInnerException(e);

                    await _batchService.FailImport(
                        message.Release.Id.ToString(),
                        message.OrigDataFileName,
                        new List<string> {ex.Message}
                    );
                    
                    logger.LogError(ex,$"{GetType().Name} function FAILED for : Datafile: " +
                                    $"{message.DataFileName} : {ex.Message}");
                }
            }
            else
            {
                await _batchService.FailImport(
                    message.Release.Id.ToString(),
                    message.OrigDataFileName,
                    errors
                );
                
                logger.LogError(
                    $"Import FAILED for {message.DataFileName}...check log");
            }
        }

        [FunctionName("ImportObservations")]
        public async Task ImportObservations(
            [QueueTrigger("imports-available")] ImportMessage message,
            ILogger logger)
        {
            try
            {
                await _fileImportService.ImportObservations(message, DbUtils.CreateStatisticsDbContext());
            }
            catch (Exception e)
            {
                // If deadlock exception then throw & try up to 3 times
                if (e is SqlException exception && exception.Number == 1205)
                {
                    logger.LogInformation($"{GetType().Name} : Handling known exception when processing Datafile: " +
                                       $"{message.DataFileName} : {exception.Message} : transaction will be retried");
                    throw;
                }
                
                var ex = GetInnerException(e);
                
                await _batchService.FailImport(
                    message.Release.Id.ToString(),
                    message.OrigDataFileName,
                    new List<string> {ex.Message}
                );
                    
                logger.LogError(ex,$"{GetType().Name} function FAILED for : Datafile: " +
                                $"{message.DataFileName} : {ex.Message}");
            }
        }
        
        private async Task<SubjectData> ProcessSubject(ImportMessage message, StatisticsDbContext statisticsDbContext, 
            ContentDbContext contentDbContext)
        {
            var status = await _batchService.GetStatus(message.Release.Id.ToString(), message.OrigDataFileName);
            
            // If already reached Phase 2 then don't re-create the subject
            if ((int)status > (int)IStatus.RUNNING_PHASE_1)
            {
                return await _fileStorageService.GetSubjectData(message); 
            }

            var subjectData = await _fileStorageService.GetSubjectData(message);
            var subject = _releaseProcessorService.CreateOrUpdateRelease(subjectData, message, dbContext);
            
            _importerService.ImportMeta(subjectData.GetMetaLines().ToList(), subject, dbContext);

            if (message.Seeding)
            {
                SampleGuids.GenerateIndicatorGuids(dbContext);
                SampleGuids.GenerateFilterGuids(dbContext);
                SampleGuids.GenerateFilterGroupGuids(dbContext);
            }

            await dbContext.SaveChangesAsync();

            _fileImportService.ImportFiltersLocationsAndSchools(message, dbContext);
            
            if (message.Seeding)
            {
                SampleGuids.GenerateFilterGuids(dbContext);
                SampleGuids.GenerateFilterGroupGuids(dbContext);
                SampleGuids.GenerateFilterItemGuids(dbContext);
            }

            await dbContext.SaveChangesAsync();
            
            await _batchService.UpdateStatus(message.Release.Id.ToString(), message.OrigDataFileName,
                IStatus.RUNNING_PHASE_2);

            return subjectData;
        }

        private static IConfigurationRoot LoadAppSettings(ExecutionContext context)
        {
            return new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", true, true)
                .AddEnvironmentVariables()
                .Build();
        }

        private static Exception GetInnerException(Exception ex)
        {
            return ex.InnerException ?? ex;
        }
    }
}