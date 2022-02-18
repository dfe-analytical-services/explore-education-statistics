using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions
{
    // ReSharper disable once UnusedType.Global
    public class MigrateDataBlocksFunction
    {
        private readonly IDataBlockMigrationService _dataBlockMigrationService;

        public MigrateDataBlocksFunction(
            IDataBlockMigrationService dataBlockMigrationService)
        {
            _dataBlockMigrationService = dataBlockMigrationService;
        }

        /// <summary>
        /// Azure Function which migrates data block location codes to id's
        /// in the query, table and charts fields.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="executionContext"></param>
        /// <param name="logger"></param>
        [FunctionName("MigrateDataBlocks")]
        // ReSharper disable once UnusedMember.Global
        public async Task MigrateDataBlock(
            [QueueTrigger(MigrateDataBlocksQueue)] MigrateDataBlockMessage message,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation("{functionName} triggered: {message}",
                executionContext.FunctionName,
                message);

            var result = await _dataBlockMigrationService.Migrate(message.Id);
            if (result.IsLeft)
            {
                logger.LogError("Failed to migrate data block {id}", message.Id);
            }
        }
    }
}
