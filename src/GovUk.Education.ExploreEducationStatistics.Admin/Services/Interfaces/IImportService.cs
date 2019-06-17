using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IImportService
    {
        void SendImportNotification(Guid releaseId, string dataFileName, string metaFileName);
    }
}