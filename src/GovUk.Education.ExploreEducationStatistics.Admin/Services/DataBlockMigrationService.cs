#nullable enable
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class DataBlockMigrationService : IDataBlockMigrationService
    {
        private readonly ContentDbContext _context;
        private readonly IStorageQueueService _storageQueueService;
        private readonly IUserService _userService;
        private readonly ILogger<DataBlockMigrationService> _logger;

        public DataBlockMigrationService(
            ContentDbContext context,
            IStorageQueueService storageQueueService,
            IUserService userService,
            ILogger<DataBlockMigrationService> logger)
        {
            _context = context;
            _storageQueueService = storageQueueService;
            _userService = userService;
            _logger = logger;
        }

        public async Task<Either<ActionResult, Unit>> MigrateAll()
        {
            return await _userService.CheckCanRunMigrations()
                .OnSuccess<ActionResult, Unit, Unit>(async _ =>
                {
                    var messageCount = await _storageQueueService.GetApproximateMessageCount(MigrateDataBlocksQueue);

                    switch (messageCount)
                    {
                        case null:
                            return new BadRequestObjectResult(
                                $"Unexpected null message count for queue {MigrateDataBlocksQueue}");
                        case > 0:
                            return new BadRequestObjectResult(
                                $"Found non-empty queue {MigrateDataBlocksQueue}. Message count: {messageCount}");
                    }

                    // Create migration queue messages for all data blocks that have not already been marked as migrated 
                    var dataBlocks = await _context.DataBlocks
                        .AsNoTracking()
                        .Where(dataBlock => dataBlock.LocationsMigrated == null || !dataBlock.LocationsMigrated)
                        .Select(dataBlock => new MigrateDataBlockMessage(dataBlock.Id))
                        .ToListAsync();

                    if (dataBlocks.Any())
                    {
                        _logger.LogInformation("Adding {count} messages to the '{queue}' queue", dataBlocks.Count,
                            MigrateDataBlocksQueue);
                        await _storageQueueService.AddMessages(MigrateDataBlocksQueue, dataBlocks);
                    }

                    return Unit.Instance;
                });
        }
    }
}
