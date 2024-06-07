#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IFileUploadsValidatorService
    {
        Task<Either<ActionResult, Unit>> ValidateFileForUpload(IFormFile file, FileType type);

        Task<List<ErrorViewModel>> ValidateDataFilesForUpload(
            Guid releaseVersionId,
            string dataSetName,
            string dataFileName,
            long dataFileSize,
            Func<Task<Stream>> dataFileStreamProvider,
            string metaFileName,
            long metaFileSize,
            Func<Task<Stream>> metaFileStreamProvider,
            File? replacingFile = null);

        Task<List<ErrorViewModel>> ValidateDataFilesForUpload(
            Guid releaseVersionId,
            string dataSetFileName,
            IFormFile dataFile,
            IFormFile metaFile,
            File? replacingFile = null);

        Task<List<ErrorViewModel>> ValidateDataFilesForUpload(
            Guid releaseVersionId,
            ArchiveDataSetFile archiveDataSet,
            Func<Task<Stream>> dataFileStreamProvider,
            Func<Task<Stream>> metaFileStreamProvider,
            File? replacingFile = null);

        List<ErrorViewModel> ValidateReleaseVersionDataSetFileName(Guid releaseVersionId,
            string name);
    }
}
