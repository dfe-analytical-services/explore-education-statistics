#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

/// <summary>
/// TODO EES-3755 Remove after Permalink snapshot migration work is complete
/// </summary>
public class PermalinkMigrationService : IPermalinkMigrationService
{
    private readonly IStorageQueueService _storageQueueService;
    private readonly IUserService _userService;

    public PermalinkMigrationService(IStorageQueueService storageQueueService,
        IUserService userService)
    {
        _storageQueueService = storageQueueService;
        _userService = userService;
    }

    public async Task<Either<ActionResult, Unit>> MigrateAll()
    {
        return await _userService.CheckIsBauUser()
            .OnSuccess<ActionResult, Unit, Unit>(async () =>
            {
                var messageCount = await _storageQueueService.GetApproximateMessageCount(PermalinksMigrationQueue);

                switch (messageCount)
                {
                    case null:
                        return ValidationActionResult(NullMessageCountForPermalinksMigrationQueue);
                    case > 0:
                        return ValidationActionResult(NonEmptyPermalinksMigrationQueue);
                }

                // Queue a new message to initiate the migration
                await _storageQueueService.AddMessageAsync(PermalinksMigrationQueue, new PermalinksMigrationMessage());

                return Unit.Instance;
            });
    }
}
