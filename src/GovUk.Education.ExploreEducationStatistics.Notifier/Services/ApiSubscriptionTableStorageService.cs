using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Notifier.Configuration;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Services;

internal class ApiSubscriptionTableStorageService(IOptions<AppSettingsOptions> appSettingsOptions)
    : DataTableStorageService(appSettingsOptions.Value.NotifierStorageConnectionString),
        IApiSubscriptionTableStorageService;
