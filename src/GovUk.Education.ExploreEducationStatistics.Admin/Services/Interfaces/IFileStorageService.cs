using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using Microsoft.AspNetCore.Http;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task UploadFilesAsync( Guid release, IFormFile dataFile, IFormFile metaFile, string name);
        IEnumerable<FileInfo> ListFiles(Guid releaseId);
        IEnumerable<FileInfo> ListFiles( string releaseId);

    }
}