using GovUk.Education.ExploreStatistics.Admin.Models;

namespace GovUk.Education.ExploreStatistics.Admin.Services.Interfaces
{
    public interface IImportService
    {
        bool SendImportNotification(ImportViewModel model);

    }
}