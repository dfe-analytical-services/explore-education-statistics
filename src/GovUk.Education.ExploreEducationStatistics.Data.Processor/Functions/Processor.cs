using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Utils;
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
        private readonly IImportService _importService;
        private readonly IProcessorService _processorService;
        private readonly ILogger<Processor> _logger;

        public Processor(
            IFileImportService fileImportService,
            IImportService importService,
            IProcessorService processorService,
            ILogger<Processor> logger
        )
        {
            _fileImportService = fileImportService;
            _importService = importService;
            _processorService = processorService;
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
                var import = await _importService.GetImport(message.Id);

                _logger.LogInformation($"Processor Function processing import message for " +
                                       $"{import.File.Filename} at stage {import.Status}");

                switch (import.Status)
                {
                    case ImportStatus.CANCELLING:
                        _logger.LogInformation($"Import for {import.File.Filename} is in the process of being " +
                                               "cancelled, so not processing to the next import stage - marking as " +
                                               "CANCELLED");
                        await _importService.UpdateStatus(import.Id, ImportStatus.CANCELLED, 100);
                        break;
                    case ImportStatus.CANCELLED:
                        _logger.LogInformation($"Import for {import.File.Filename} is cancelled, so not " +
                                               "processing any further");
                        break;
                    case ImportStatus.QUEUED:
                    case ImportStatus.PROCESSING_ARCHIVE_FILE:
                    {
                        if (import.ZipFile != null)
                        {
                            _logger.LogInformation($"Unpacking archive for {import.ZipFile.Filename}");
                            await _processorService.ProcessUnpackingArchive(import.Id);
                        }

                        await _importService.UpdateStatus(import.Id, ImportStatus.STAGE_1, 0);
                        importStagesMessageQueue.Add(message);
                        break;
                    }
                    case ImportStatus.STAGE_1:
                        await _processorService.ProcessStage1(import.Id, executionContext);
                        await _importService.UpdateStatus(import.Id, ImportStatus.STAGE_2, 0);
                        importStagesMessageQueue.Add(message);
                        break;
                    case ImportStatus.STAGE_2:
                        await _processorService.ProcessStage2(import.Id);
                        await _importService.UpdateStatus(import.Id, ImportStatus.STAGE_3, 0);
                        importStagesMessageQueue.Add(message);
                        break;
                    case ImportStatus.STAGE_3:
                        await _processorService.ProcessStage3(import.Id);
                        await _importService.UpdateStatus(import.Id, ImportStatus.STAGE_4, 0);
                        importStagesMessageQueue.Add(message);
                        break;
                    case ImportStatus.STAGE_4:
                        await _processorService.ProcessStage4Messages(import.Id, importObservationsMessageQueue);
                        break;
                }
            }
            catch (Exception e)
            {
                var ex = GetInnerException(e);

                await _importService.FailImport(message.Id, ex.Message);

                _logger.LogError(ex, $"{GetType().Name} function FAILED for Import: " +
                                     $"{message.Id} : {ex.Message}");
                _logger.LogError(ex.StackTrace);
            }
        }

        [FunctionName("ImportObservations")]
        public async Task ImportObservations(
            [QueueTrigger(ImportsAvailableQueue)] ImportObservationsMessage message)
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
                    _logger.LogInformation($"{GetType().Name} : Handling known exception when processing Import: " +
                                           $"{message.Id} : {exception.Message} : transaction will be retried");
                    throw;
                }

                var ex = GetInnerException(e);

                await _importService.FailImport(message.Id, ex.Message);

                _logger.LogError(ex, $"{GetType().Name} function FAILED for : Import: " +
                                     $"{message.Id} : {ex.Message}");
            }
        }

        [FunctionName("CancelImports")]
        public async Task CancelImports([QueueTrigger(ImportsCancellingQueue)] CancelImportMessage message)
        {
            await _importService.UpdateStatus(message.Id, ImportStatus.CANCELLING, 0);
        }

        private static Exception GetInnerException(Exception ex)
        {
            return ex.InnerException ?? ex;
        }
    }
}