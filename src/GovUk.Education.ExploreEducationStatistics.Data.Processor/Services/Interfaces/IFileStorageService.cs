using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
using Microsoft.Azure.Storage.Blob;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task<SubjectData> GetSubjectData(ImportMessage importMessage);

        Task<bool> UploadDataFileAsync(
            Guid releaseId,
            MemoryStream stream,
            string metaFileName,
            string name,
            string fileName,
            string contentType,
            int numRows);

        void DeleteBatchFile(string releaseId, string dataFileName);
        int GetNumBatchesRemaining(string releaseId, string origDataFileName);
        CloudBlockBlob GetBlobReference(string fullPath);
        Task UploadFileToStorageAsync(Guid releaseId, ZipArchiveEntry file, ReleaseFileTypes type, IDictionary<string, string> metaValues);
    }
}