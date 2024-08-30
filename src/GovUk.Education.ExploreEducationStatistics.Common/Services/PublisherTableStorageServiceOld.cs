using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services;

public class PublisherTableStorageServiceOld(string connectionString) // @MarkFix remove
    : TableStorageService(connectionString), IPublisherTableStorageServiceOld;
