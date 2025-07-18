using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Data.Processor.Model.ProcessorQueues;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Functions;

public class Processor
{
    private readonly IDataImportService _dataImportService;
    private readonly IProcessorService _processorService;
    private readonly ILogger<Processor> _logger;
    private readonly bool _rethrowExceptions;

    private static readonly DataImportStatus[] RequeueMessageOnCompletionOfStages =
    [
        DataImportStatus.QUEUED,
        DataImportStatus.STAGE_1,
        DataImportStatus.STAGE_2
    ];

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

    [Function("ProcessUploads")]
    [QueueOutput(ImportsPendingQueue)]
    public async Task<ImportMessage[]> ProcessUploads(
        [QueueTrigger(ImportsPendingQueue)] ImportMessage message,
        FunctionContext context)
    {
        try
        {
            var import = await _dataImportService.GetImport(message.Id);

            _logger.LogInformation(
                "{FunctionName} function processing import message for {Filename} at stage {ImportStatus}",
                context.FunctionDefinition.Name,
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
                    {
                        await _dataImportService.UpdateStatus(import.Id, DataImportStatus.STAGE_1, 0);
                        break;
                    }
                case DataImportStatus.STAGE_1:
                    await _processorService.ProcessStage1(import.Id);
                    await _dataImportService.UpdateStatus(import.Id, DataImportStatus.STAGE_2, 0);
                    break;
                case DataImportStatus.STAGE_2:
                    await _processorService.ProcessStage2(import.Id);
                    await _dataImportService.UpdateStatus(import.Id, DataImportStatus.STAGE_3, 0);
                    break;
                case DataImportStatus.STAGE_3:
                    await _processorService.ProcessStage3(import.Id);
                    break;
            }

            var requeueMessageOnCompletion = RequeueMessageOnCompletionOfStages.Contains(import.Status);
            return requeueMessageOnCompletion ? [message] : [];
        }
        catch (Exception e)
        {
            var mainException = e.InnerException ?? e;

            _logger.LogError(
                mainException,
                "{FunctionName} function FAILED for Import {ImportId} : {Message}",
                context.FunctionDefinition.Name,
                message.Id,
                mainException.Message);

            await _dataImportService.FailImport(message.Id);

            if (_rethrowExceptions)
            {
                throw;
            }
        }

        return [];
    }

    [Function("CancelImports")]
    public async Task CancelImports([QueueTrigger(ImportsCancellingQueue)] CancelImportMessage message)
    {
        await _dataImportService.UpdateStatus(message.Id, DataImportStatus.CANCELLING, 0);
    }
}
