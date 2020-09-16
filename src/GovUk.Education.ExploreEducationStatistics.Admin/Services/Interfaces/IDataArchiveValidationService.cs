using System;
using System.IO.Compression;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage.Blob;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IDataArchiveValidationService
    {
        Task<Either<ActionResult, Tuple<ZipArchiveEntry, ZipArchiveEntry>>> ValidateDataArchiveFile(CloudBlobContainer blobContainer,
            Guid releaseId, IFormFile zipFile);
    }
}