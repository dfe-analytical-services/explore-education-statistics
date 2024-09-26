using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Notifier.Options;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Services;

internal class ApiSubscriptionTableStorageService(IOptions<AppOptions> appOptions)
    : DataTableStorageService(appOptions.Value.NotifierStorageConnectionString),
        IApiSubscriptionTableStorageService;
