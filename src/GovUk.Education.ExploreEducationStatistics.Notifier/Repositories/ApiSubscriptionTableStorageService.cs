using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Notifier.Configuration;
using GovUk.Education.ExploreEducationStatistics.Notifier.Repositories.Interfaces;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Repositories;

internal class ApiSubscriptionTableStorageService : DataTableStorageService, IApiSubscriptionTableStorageService
{
    public ApiSubscriptionTableStorageService(IOptions<AppSettingsOptions> appSettingsOptions) 
        : base(appSettingsOptions.Value.TableStorageConnectionString)
    {
    }
}
