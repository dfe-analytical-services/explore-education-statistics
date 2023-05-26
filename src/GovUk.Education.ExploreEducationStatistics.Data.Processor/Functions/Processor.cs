using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Data.Processor.Model.ImporterQueues;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Functions
{
    // ReSharper disable once UnusedMember.Global
    public class Processor
    {
        private readonly IDataImportService _dataImportService;
        private readonly IProcessorService _processorService;
        private readonly ILogger<Processor> _logger;
        private readonly bool _rethrowExceptions;

        public Processor(
            IDataImportService dataImportService,
            IProcessorService processorService,
            ILogger<Processor> logger, 
            bool rethrowExceptions = false)
        {
            _dataImportService = dataImportService;
            _processorService = processorService;
            _logger = logger;
            _rethrowExceptions = rethrowExceptions;
        }

        [FunctionName("ProcessUploads")]
        public async Task ProcessUploads(
            [QueueTrigger(ImportsPendingQueue)] ImportMessage message,
            ExecutionContext executionContext,
            [Queue(ImportsPendingQueue)] ICollector<ImportMessage> importStagesMessageQueue)
        {
            try
            {
                var import = await _dataImportService.GetImport(message.Id);

                _logger.LogInformation(
                    "Processor Function processing import message for {Filename} at stage {ImportStatus}",
                    import.File.Filename,
                    import.Status);

                switch (import.Status)
                {
                    case DataImportStatus.CANCELLING:
                        _logger.LogInformation(
                            "Import for {Filename} is in the process of being " +
                            "cancelled, so not processing to the next import stage - marking as " +
                            "CANCELLED",
                            import.File.Filename);
                        
                        await _dataImportService.UpdateStatus(import.Id, DataImportStatus.CANCELLED, 100);
                        break;
                    case DataImportStatus.CANCELLED:
                        _logger.LogInformation(
                            "Import for {Filename} is cancelled, so not processing any further",
                            import.File.Filename);
                        break;
                    case DataImportStatus.QUEUED:
                    case DataImportStatus.PROCESSING_ARCHIVE_FILE:
                    {
                        if (import.ZipFile != null)
                        {
                            _logger.LogInformation(
                                "Unpacking archive for {ZipFilename}",
                                import.ZipFile.Filename);
                            
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
                        break;
                }
            }
            catch (Exception e)
            {
                var mainException = e.InnerException ?? e;

                _logger.LogError(
                    mainException, 
                    "{FunctionName} function FAILED for Import {ImportId} : {Message}",
                    executionContext.FunctionName,
                    message.Id,
                    mainException.Message);

                await _dataImportService.FailImport(message.Id);

                if (_rethrowExceptions)
                {
                    throw;
                }
            }
        }

        [FunctionName("CancelImports")]
        public async Task CancelImports([QueueTrigger(ImportsCancellingQueue)] CancelImportMessage message)
        {
            await _dataImportService.UpdateStatus(message.Id, DataImportStatus.CANCELLING, 0);
        }
    }
}
