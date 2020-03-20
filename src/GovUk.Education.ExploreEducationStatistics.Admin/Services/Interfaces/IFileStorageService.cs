﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FileInfo = GovUk.Education.ExploreEducationStatistics.Admin.Models.FileInfo;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{

    public interface IFileStorageService
    {
        Task<Either<ActionResult, IEnumerable<FileInfo>>> UploadDataFilesAsync(Guid releaseId,
            IFormFile dataFile, IFormFile metaFile, string name, bool overwrite, string userName);

        Task<Either<ActionResult, IEnumerable<FileInfo>>> ListFilesAsync(Guid releaseId, ReleaseFileTypes type);

        Task<IEnumerable<FileInfo>> ListFilesFromBlobStorage(Guid releaseId, ReleaseFileTypes type);
        
        Task<Either<ActionResult, IEnumerable<Common.Model.FileInfo>>> ListPublicFilesPreview(Guid releaseId);

        Task<Either<ActionResult, IEnumerable<FileInfo>>> UploadFilesAsync(Guid releaseId, IFormFile file,
            string name, ReleaseFileTypes type, bool overwrite);

        Task<Either<ActionResult, IEnumerable<FileInfo>>> DeleteFileAsync(Guid releaseId, ReleaseFileTypes type,
            string fileName);

        Task<Either<ActionResult, IEnumerable<FileInfo>>> DeleteDataFileAsync(Guid releaseId, string fileName);

        Task<Either<ActionResult, FileStreamResult>> StreamFile(Guid releaseId, ReleaseFileTypes type,
            string fileName);

        Task<Either<ActionResult, Release>> CopyReleaseFilesAsync(Guid originalReleaseId, Guid newReleaseId, ReleaseFileTypes type);
    }
}