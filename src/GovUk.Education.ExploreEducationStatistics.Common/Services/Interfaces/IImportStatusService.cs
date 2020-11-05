using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces
{
    public interface IImportStatusService
    {
        Task<ImportStatus> GetImportStatus(Guid releaseId, string dataFileName);

        Task<bool> IsImportFinished(Guid releaseId, string dataFileName);

        Task<bool> UpdateStatus(Guid releaseId, string origDataFileName, IStatus status, int retry = 0);

        Task UpdateProgress(Guid releaseId, string origDataFileName, double percentageComplete, int retry = 0);
    }
}