using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Functions
{
    // ReSharper disable once UnusedMember.Global
    public class Processor
    {
        private readonly IBatchService _batchService;
        private readonly IFileImportService _fileImportService;
        private readonly IImportStatusService _importStatusService;
        private readonly IProcessorService _processorService;
        private readonly ILogger<Processor> _logger;

        public Processor(
            IFileImportService fileImportService,
            IBatchService batchService,
            IImportStatusService importStatusService,
            IProcessorService processorService,
            ILogger<Processor> logger
        )
        {
            _fileImportService = fileImportService;
            _batchService = batchService;
            _importStatusService = importStatusService;
            _processorService = processorService;
            _logger = logger;
        }

        [FunctionName("ProcessUploads")]
        public async void ProcessUploads(
            [QueueTrigger("imports-pending")] ImportMessage message,
            ExecutionContext executionContext,
            [Queue("imports-pending")] ICollector<ImportMessage> importStagesMessageQueue,
            [Queue("imports-available")] ICollector<ImportObservationsMessage> importObservationsMessageQueue
        )
        {
            try
            {
                var status = await _importStatusService.GetImportStatus(message.Release.Id, message.DataFileName);

                _logger.LogInformation($"Processor Function processing import message for " +
                                       $"{message.DataFileName} at stage {status.Status}");

                switch (status.Status)
                {
                    case IStatus.CANCELLING:
                        _logger.LogInformation($"Import for {message.DataFileName} is in the process of being " +
                                               $"cancelled, so not processing to the next import stage - marking as " +
                                               $"CANCELLED");
                        await _importStatusService.UpdateStatus(message.Release.Id, message.DataFileName, IStatus.CANCELLED,
                            100);
                        break;
                    case IStatus.CANCELLED:
                        _logger.LogInformation($"Import for {message.DataFileName} is cancelled, so not " +
                                               $"processing any further");
                        break;
                    case IStatus.QUEUED:
                    case IStatus.PROCESSING_ARCHIVE_FILE:
                    {
                        if (message.ArchiveFileName != "")
                        {
                            _logger.LogInformation($"Unpacking archive for {message.DataFileName}");
                            await _processorService.ProcessUnpackingArchive(message);
                        }

                        await _importStatusService.UpdateStatus(message.Release.Id, message.DataFileName,
                            IStatus.STAGE_1);
                        importStagesMessageQueue.Add(message);
                        break;
                    }
                    case IStatus.STAGE_1:
                        await _processorService.ProcessStage1(message, executionContext);
                        await _importStatusService.UpdateStatus(message.Release.Id, message.DataFileName,
                            IStatus.STAGE_2);
                        importStagesMessageQueue.Add(message);
                        break;
                    case IStatus.STAGE_2:
                        await _processorService.ProcessStage2(message);
                        await _importStatusService.UpdateStatus(message.Release.Id, message.DataFileName,
                            IStatus.STAGE_3);
                        importStagesMessageQueue.Add(message);
                        break;
                    case IStatus.STAGE_3:
                        await _processorService.ProcessStage3(message);
                        await _importStatusService.UpdateStatus(message.Release.Id, message.DataFileName,
                            IStatus.STAGE_4);
                        importStagesMessageQueue.Add(message);
                        break;
                    case IStatus.STAGE_4:
                        await _processorService.ProcessStage4Messages(message, importObservationsMessageQueue);
                        break;
                }
            }
            catch (Exception e)
            {
                var ex = GetInnerException(e);

                await _batchService.FailImport(message.Release.Id,
                    message.DataFileName,
                    new List<ValidationError>
                    {
                        new ValidationError(ex.Message)
                    });

                _logger.LogError(ex, $"{GetType().Name} function FAILED for : Datafile: " +
                                     $"{message.DataFileName} : {ex.Message}");
                _logger.LogError(ex.StackTrace);
            }
        }

        [FunctionName("ImportObservations")]
        public async Task ImportObservations(
            [QueueTrigger("imports-available")] ImportObservationsMessage message)
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
                    _logger.LogInformation($"{GetType().Name} : Handling known exception when processing Datafile: " +
                                           $"{message.DataFileName} : {exception.Message} : transaction will be retried");
                    throw;
                }

                var ex = GetInnerException(e);

                await _batchService.FailImport(message.ReleaseId,
                    message.DataFileName,
                    new List<ValidationError>
                    {
                        new ValidationError(ex.Message)
                    });

                _logger.LogError(ex, $"{GetType().Name} function FAILED for : Datafile: " +
                                     $"{message.DataFileName} : {ex.Message}");
            }
        }

        [FunctionName("CancelImports")]
        public async void CancelImports(
            [QueueTrigger("imports-cancelling")] CancelImportMessage message,
            ExecutionContext executionContext
        )
        {
            var currentStatus = await _importStatusService.GetImportStatus(message.ReleaseId, message.DataFileName);
            await _importStatusService.UpdateStatus(message.ReleaseId, message.DataFileName, IStatus.CANCELLING, currentStatus.PercentageComplete);
        }


        private static Exception GetInnerException(Exception ex)
        {
            return ex.InnerException ?? ex;
        }
    }
}