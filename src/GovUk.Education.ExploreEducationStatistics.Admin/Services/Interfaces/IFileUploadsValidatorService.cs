#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IFileUploadsValidatorService
    {
        Task<Either<ActionResult, Unit>> ValidateFileForUpload(IFormFile file, FileType type);

        Task<List<ErrorViewModel>> ValidateDataFilesForUpload(
            Guid releaseVersionId,
            IFormFile dataFile,
            IFormFile metaFile,
            File? replacingFile = null);

        List<ErrorViewModel> ValidateDataArchiveFileForUpload(Guid releaseVersionId,
            IDataArchiveFile archiveFile,
            File? replacingFile = null);

        List<ErrorViewModel> ValidateReleaseVersionDataSetFileName(Guid releaseVersionId,
            string name);
    }
}
