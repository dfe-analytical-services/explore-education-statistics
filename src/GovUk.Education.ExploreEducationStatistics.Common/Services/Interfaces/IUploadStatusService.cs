using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces
{
    public interface IUploadStatusService
    {
        Task<int> GetPercentageComplete(string releaseId, string dataFileName);

        Task CreateImport(string releaseId, string dataFileName, int numBatches);

        Task<DatafileImport> GetImport(string releaseId, string dataFileName);
    }
}