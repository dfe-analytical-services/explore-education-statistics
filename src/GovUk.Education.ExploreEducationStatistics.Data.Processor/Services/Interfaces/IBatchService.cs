using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces
{
    public interface IBatchService
    {
        Task<int> GetNumBatchesRemaining(File dataFile);
        Task<List<BlobInfo>> GetBatchFilesForDataFile(File dataFile);
    }
}
