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
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

/**
 *  Service used to migrate files in EES-3547
 *  TODO Remove in EES-3552
 */
public class FileMigrationService : IFileMigrationService
{
    private readonly ContentDbContext _contentDbContext;
    private readonly IStorageQueueService _storageQueueService;
    private readonly IUserService _userService;
    private readonly ILogger<FileMigrationService> _logger;

    public FileMigrationService(ContentDbContext contentDbContext,
        IStorageQueueService storageQueueService,
        IUserService userService,
        ILogger<FileMigrationService> logger)
    {
        _contentDbContext = contentDbContext;
        _storageQueueService = storageQueueService;
        _userService = userService;
        _logger = logger;
    }

    public async Task<Either<ActionResult, Unit>> MigrateAll()
    {
        return await _userService.CheckCanRunMigrations()
            .OnSuccessVoid<ActionResult, Unit, Unit>(async () =>
            {
                var messageCount = await _storageQueueService.GetApproximateMessageCount(MigrateFilesQueue);

                switch (messageCount)
                {
                    case null:
                        return ValidationActionResult(NullMessageCountForFileMigrationQueue);
                    case > 0:
                        return ValidationActionResult(NonEmptyFileMigrationQueue);
                }

                // Queue one message per file for all files that don't have values
                // for the new ContentLength or ContentType properties
                var files = await _contentDbContext.Files
                    .AsNoTracking()
                    .Where(file => file.ContentLength == null || file.ContentType == null)
                    .Select(file => new MigrateFileMessage(file.Id))
                    .ToListAsync();

                if (files.Any())
                {
                    _logger.LogInformation("Adding {count} messages to the '{queue}' queue",
                        files.Count,
                        MigrateFilesQueue);

                    await _storageQueueService.AddMessages(MigrateFilesQueue, files);
                }
                else
                {
                    _logger.LogInformation("Found no files that require migrating");
                }

                return Unit.Instance;
            });
    }
}
