#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IFileUploadsValidatorService
    {
        Task<List<ErrorViewModel>> ValidateDataSetFilesForUpload( // Used by unit tests
            Guid releaseVersionId,
            string dataSetTitle,
            string dataFileName,
            long dataFileLength,
            Stream dataFileStream,
            string metaFileName,
            long metaFileLength,
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
            Stream metaFileStream,
            File? replacingFile = null);

        Task<Either<ActionResult, Unit>> ValidateFileForUpload(IFormFile file, FileType type);
    }
}
