using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Notifier.Configuration;
using GovUk.Education.ExploreEducationStatistics.Notifier.Repositories.Interfaces;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Repositories;

internal class ApiSubscriptionTableStorageService(IOptions<AppSettingsOptions> appSettingsOptions)
    : DataTableStorageService(appSettingsOptions.Value.NotifierStorageConnectionString),
        IApiSubscriptionTableStorageService;
