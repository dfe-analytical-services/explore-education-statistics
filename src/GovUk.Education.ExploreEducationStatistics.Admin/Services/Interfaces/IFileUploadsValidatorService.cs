#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IFileUploadsValidatorService
    {
        Task<List<ErrorViewModel>> ValidateDataSetFilesForUpload(
            Guid releaseVersionId,
            string dataSetTitle,
            string dataFileName,
            Stream dataFileStream,
            string metaFileName,
            Stream metaFileStream,
            File? replacingFile = null);

        Task<List<ErrorViewModel>> ValidateDataSetFilesForUpload(
            Guid releaseVersionId,
            string dataSetTitle,
            IFormFile dataFile,
            IFormFile metaFile,
            File? replacingFile = null);

        Task<List<ErrorViewModel>> ValidateDataSetFilesForUpload(
            Guid releaseVersionId,
            ArchiveDataSetFile archiveDataSet,
            Stream dataFileStream,
            Stream metaFileStream);

        Task<Either<ActionResult, Unit>> ValidateFileForUpload(
            IFormFile file,
            FileType type);
    }
}
