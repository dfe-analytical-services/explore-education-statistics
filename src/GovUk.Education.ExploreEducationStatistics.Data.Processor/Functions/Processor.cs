using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Data.Processor.Model.ImporterQueues;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Functions
{
    // ReSharper disable once UnusedMember.Global
    public class Processor
    {
        private readonly IFileImportService _fileImportService;
        private readonly IDataImportService _dataImportService;
        private readonly IProcessorService _processorService;
        private readonly IDbContextSupplier _dbContextSupplier;
        private readonly ILogger<Processor> _logger;

        public Processor(
            IFileImportService fileImportService,
            IDataImportService dataImportService,
            IProcessorService processorService,
            IDbContextSupplier dbContextSupplier,
            ILogger<Processor> logger)
        {
            _fileImportService = fileImportService;
            _dataImportService = dataImportService;
            _processorService = processorService;
            _dbContextSupplier = dbContextSupplier;
            _logger = logger;
        }

        [FunctionName("ProcessUploads")]
        public async Task ProcessUploads(
            [QueueTrigger(ImportsPendingQueue)] ImportMessage message,
            ExecutionContext executionContext,
            [Queue(ImportsPendingQueue)] ICollector<ImportMessage> importStagesMessageQueue,
            [Queue(ImportsAvailableQueue)] ICollector<ImportObservationsMessage> importObservationsMessageQueue
        )
        {
            try
            {
                var import = await _dataImportService.GetImport(message.Id);

                _logger.LogInformation($"Processor Function processing import message for " +
                                       $"{import.File.Filename} at stage {import.Status}");

                switch (import.Status)
                {
                    case DataImportStatus.CANCELLING:
                        _logger.LogInformation($"Import for {import.File.Filename} is in the process of being " +
                                               "cancelled, so not processing to the next import stage - marking as " +
                                               "CANCELLED");
                        await _dataImportService.UpdateStatus(import.Id, DataImportStatus.CANCELLED, 100);
                        break;
                    case DataImportStatus.CANCELLED:
                        _logger.LogInformation($"Import for {import.File.Filename} is cancelled, so not " +
                                               "processing any further");
                        break;
                    case DataImportStatus.QUEUED:
                    case DataImportStatus.PROCESSING_ARCHIVE_FILE:
                    {
                        if (import.ZipFile != null)
                        {
                            _logger.LogInformation($"Unpacking archive for {import.ZipFile.Filename}");
                            await _processorService.ProcessUnpackingArchive(import.Id);
                        }

                        await _dataImportService.UpdateStatus(import.Id, DataImportStatus.STAGE_1, 0);
                        importStagesMessageQueue.Add(message);
                        break;
                    }
                    case DataImportStatus.STAGE_1:
                        await _processorService.ProcessStage1(import.Id);
                        await _dataImportService.UpdateStatus(import.Id, DataImportStatus.STAGE_2, 0);
                        importStagesMessageQueue.Add(message);
                        break;
                    case DataImportStatus.STAGE_2:
                        await _processorService.ProcessStage2(import.Id);
                        await _dataImportService.UpdateStatus(import.Id, DataImportStatus.STAGE_3, 0);
                        importStagesMessageQueue.Add(message);
                        break;
                    case DataImportStatus.STAGE_3:
                        await _processorService.ProcessStage3(import.Id);
                        await _dataImportService.UpdateStatus(import.Id, DataImportStatus.STAGE_4, 0);
                        importStagesMessageQueue.Add(message);
                        break;
                    case DataImportStatus.STAGE_4:
                        await _processorService.ProcessStage4Messages(import.Id, importObservationsMessageQueue);
                        break;
                }
            }
            catch (Exception e)
            {
                var ex = GetInnerException(e);

                _logger.LogError(ex, $"{executionContext.FunctionName} function FAILED for Import: " +
                                     $"{message.Id} : {ex.Message}");

                await _dataImportService.FailImport(message.Id);
            }
        }

        [FunctionName("ImportObservations")]
        public async Task ImportObservations(
            [QueueTrigger(ImportsAvailableQueue)] ImportObservationsMessage message)
        {
            try
            {
                await _fileImportService.ImportObservations(message, _dbContextSupplier.CreateDbContext<StatisticsDbContext>());
            }
            catch (Exception e)
            {
                // If deadlock exception then throw & try up to 3 times
                if (e is SqlException exception && exception.Number == 1205)
                {
                    _logger.LogInformation($"{GetType().Name} : Handling known exception when processing Import: " +
                                           $"{message.Id} : {exception.Message} : transaction will be retried");
                    throw;
                }

                var ex = GetInnerException(e);

                _logger.LogError(ex, $"{GetType().Name} function FAILED for : Import: " +
                                     $"{message.Id} : {ex.Message}");

                _logger.LogError(ex.StackTrace);
                
                await _dataImportService.FailImport(message.Id);
            }
        }

        [FunctionName("CancelImports")]
        public async Task CancelImports([QueueTrigger(ImportsCancellingQueue)] CancelImportMessage message)
        {
            await _dataImportService.UpdateStatus(message.Id, DataImportStatus.CANCELLING, 0);
        }

        private static Exception GetInnerException(Exception ex)
        {
            return ex.InnerException ?? ex;
        }
    }
}
