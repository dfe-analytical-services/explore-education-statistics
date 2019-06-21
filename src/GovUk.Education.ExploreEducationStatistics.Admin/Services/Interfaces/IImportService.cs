using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IImportService
    {
        void SendImportNotification(string dataFileName, Guid releaseId);
    }
}