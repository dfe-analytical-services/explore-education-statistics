using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services;

public class PublisherTableStorageService(string connectionString)
    : DataTableStorageService(connectionString),
        IPublisherTableStorageService;
