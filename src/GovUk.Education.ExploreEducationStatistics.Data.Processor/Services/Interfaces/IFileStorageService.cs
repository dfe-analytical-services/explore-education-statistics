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
        public Task<SubjectData> GetSubjectData(ImportMessage importMessage);

        public Task UploadStream(
            Guid releaseId,
            ReleaseFileTypes fileType,
            string fileName,
            Stream stream,
            string contentType,
            IDictionary<string, string> metaValues);

        public Task<Stream> StreamFile(Guid releaseId, ReleaseFileTypes fileType, string fileName);

        public Task<Stream> StreamBlob(BlobInfo blob);

        public Task DeleteBatchFile(string releaseId, string dataFileName);

        public Task<int> GetNumBatchesRemaining(string releaseId, string origDataFileName);

        public Task<IEnumerable<BlobInfo>> GetBatchesRemaining(string releaseId, string origDataFileName);
    }
}