using System;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces
{
    public interface IProcessorService
    {
        Task ProcessUnpackingArchive(Guid importId);

        Task ProcessStage1(Guid importId);

        Task ProcessStage2(Guid importId);

        Task ProcessStage3(Guid importId);
    }
}
