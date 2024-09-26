using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Notifier.Configuration;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Services;

public class NotifierTableStorageService(IOptions<AppOptions> appOptions)
    : DataTableStorageService(appOptions.Value.NotifierStorageConnectionString),
        INotifierTableStorageService;
