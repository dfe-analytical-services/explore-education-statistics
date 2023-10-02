using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services;

public class CoreTableStorageService : TableStorageService, ICoreTableStorageService
{
    public CoreTableStorageService(IConfiguration configuration) 
        : base(configuration, "PublisherStorage")
    {
    }
}