using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task<SubjectData> GetSubjectData(ImportMessage importMessage);

        Task UploadStream(
            Guid releaseId,
            ReleaseFileTypes fileType,
            string fileName,
            Stream stream,
            string contentType,
            IDictionary<string, string> metaValues);

        Task<Stream> StreamBlob(BlobInfo blob);

        Task DeleteBatchFile(Guid releaseId, string origDataFileName);

        Task<int> GetNumBatchesRemaining(Guid releaseId, string origDataFileName);

        Task<IEnumerable<BlobInfo>> GetBatchFilesForDataFile(Guid releaseId, string origDataFileName);
    }
}