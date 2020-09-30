using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces
{
    public interface IImportStatusService
    {
        Task<ImportStatus> GetImportStatus(Guid releaseId, string dataFileName);

        Task<bool> IsImportFinished(Guid releaseId, string dataFileName);
    }
}