using GovUk.Education.ExploreEducationStatistics.Admin.Models;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IImportService
    {
        bool SendImportNotification(ImportViewModel model);

    }
}