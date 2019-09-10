using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage.Blob;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task<SubjectData> GetSubjectData(ImportMessage importMessage);

        Task<Boolean> UploadDataFileAsync(Guid releaseId, IFormFile dataFile, string metaFileName,
            string name);

        void Delete(string releaseId, string dataFileName);
        
        CloudBlockBlob GetCloudBlockBlob(string releaseId, string dataFileName);
        
        Task<string> GetLeaseId(CloudBlockBlob cloudBlockBlob);
    }
}