using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services;

public class CoreTableStorageService : TableStorageService, ICoreTableStorageService
{
    public CoreTableStorageService() 
        : base("CoreStorage")
    {
    }
}