using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task UploadFileAsync(string sourceFile, string fileName, Guid releaseId);

        List<string> ListFiles(string directory);
    }
}
