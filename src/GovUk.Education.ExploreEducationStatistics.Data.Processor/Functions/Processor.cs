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
            [Queue("imports-available")] ICollector<ImportMessage> dataFileProcessingMessageQueue
        )
        {
            try
            {
                var status = await _importStatusService.GetImportStatus(message.Release.Id, message.OrigDataFileName);

                _logger.LogInformation($"Processor Function processing import message for " +
                                       $"{message.OrigDataFileName} at stage {status.Status}");
                
                if (status.Status == IStatus.QUEUED || status.Status == IStatus.PROCESSING_ARCHIVE_FILE)
                {
                    if (message.ArchiveFileName != "")
                    {
                        _logger.LogInformation($"Unpacking archive for {message.OrigDataFileName}");
                        await _processorService.ProcessUnpackingArchive(message);
                    }

                    await _importStatusService.UpdateStatus(message.Release.Id, message.OrigDataFileName,
                        IStatus.STAGE_1);
                    importStagesMessageQueue.Add(message);
                    return;
                }

                if (status.Status == IStatus.STAGE_1)
                {
                    await _processorService.ProcessStage1(message, executionContext);
                    await _importStatusService.UpdateStatus(message.Release.Id, message.OrigDataFileName,
                        IStatus.STAGE_2);
                    importStagesMessageQueue.Add(message);
                    return;
                }

                if (status.Status == IStatus.STAGE_2)
                {
                    await _processorService.ProcessStage2(message);
                    await _importStatusService.UpdateStatus(message.Release.Id, message.OrigDataFileName,
                        IStatus.STAGE_3);
                    importStagesMessageQueue.Add(message);
                    return;
                }

                if (status.Status == IStatus.STAGE_3)
                {
                    await _processorService.ProcessStage3(message);
                    await _importStatusService.UpdateStatus(message.Release.Id, message.OrigDataFileName,
                        IStatus.STAGE_4);
                    importStagesMessageQueue.Add(message);
                }

                if (status.Status == IStatus.STAGE_4)
                {
                    await _processorService.ProcessStage4Messages(message, dataFileProcessingMessageQueue);
                }
            }
            catch (Exception e)
            {
                var ex = GetInnerException(e);

                await _batchService.FailImport(message.Release.Id,
                    message.OrigDataFileName,
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
        public async Task ImportObservations([QueueTrigger("imports-available")] ImportMessage message)
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

                await _batchService.FailImport(message.Release.Id,
                    message.OrigDataFileName,
                    new List<ValidationError>
                    {
                        new ValidationError(ex.Message)
                    });

                _logger.LogError(ex, $"{GetType().Name} function FAILED for : Datafile: " +
                                     $"{message.DataFileName} : {ex.Message}");
            }
        }

        private static Exception GetInnerException(Exception ex)
        {
            return ex.InnerException ?? ex;
        }
    }
}