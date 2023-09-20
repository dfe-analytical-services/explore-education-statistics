using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services;

public class PublisherTableStorageService : TableStorageService, IPublisherTableStorageService
{
    public PublisherTableStorageService() 
        : base("PublisherStorage")
    {
    }
}