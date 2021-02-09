using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces
{
    public interface IBatchService
    {
        Task<int> GetNumBatchesRemaining(Guid fileId);
        Task<List<BlobInfo>> GetBatchFilesForDataFile(Guid fileId);
    }
}