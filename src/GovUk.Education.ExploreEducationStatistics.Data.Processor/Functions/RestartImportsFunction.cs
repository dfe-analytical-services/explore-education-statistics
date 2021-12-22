using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.DataImportStatus;
using static GovUk.Education.ExploreEducationStatistics.Data.Processor.Model.ImporterQueues;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Functions
{
    // ReSharper disable once UnusedType.Global
    public class RestartImportsFunction
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IStorageQueueService _storageQueueService;

        private static readonly List<DataImportStatus> IncompleteStatuses = new List<DataImportStatus>
        {
            QUEUED,
            PROCESSING_ARCHIVE_FILE,
            STAGE_1,
            STAGE_2,
            STAGE_3,
            STAGE_4,
            CANCELLING
        };

        public RestartImportsFunction(ContentDbContext contentDbContext,
            IStorageQueueService storageQueueService)
        {
            _contentDbContext = contentDbContext;
            _storageQueueService = storageQueueService;
        }

        [FunctionName("RestartImports")]
        // ReSharper disable once UnusedMember.Global
        public async Task RestartImports(
            [QueueTrigger(RestartImportsQueue)] RestartImportsMessage message,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation($"{executionContext.FunctionName} triggered: {message}");

            var incompleteImports = await _contentDbContext.DataImports
                .AsQueryable()
                .Where(import => IncompleteStatuses.Contains(import.Status))
                .ToListAsync();

            var messages = incompleteImports
                .Select(import => new ImportMessage(import.Id))
                .ToList();

            await _storageQueueService.AddMessages(ImportsPendingQueue, messages);

            logger.LogInformation($"{executionContext.FunctionName} completed");
        }
    }
}
