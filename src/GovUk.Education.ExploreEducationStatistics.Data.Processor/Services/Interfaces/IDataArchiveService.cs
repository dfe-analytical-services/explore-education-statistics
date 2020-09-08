using System;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces
{
    public interface IDataArchiveService
    {
        Task ExtractDataFiles(Guid releaseId, string zipFileName);
    }
}