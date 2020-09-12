using System;
using System.IO.Compression;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IDataArchiveValidationService
    {
        Task<Either<ActionResult, Tuple<ZipArchiveEntry, ZipArchiveEntry>>> ValidateDataArchiveFile(
            Guid releaseId,
            IFormFile zipFile);
    }
}