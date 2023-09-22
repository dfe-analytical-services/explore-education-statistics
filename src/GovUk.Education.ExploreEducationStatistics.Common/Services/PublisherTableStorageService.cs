using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services;

public class PublisherTableStorageService : TableStorageService, IPublisherTableStorageService
{
    public PublisherTableStorageService(IConfiguration configuration) 
        : base(configuration, "PublisherStorage")
    {
    }
}