using System;
using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;

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

        int GetNumBatchesRemaining(string releaseId);
    }
}