#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IReleaseDataFileService
    {
        Task<Either<ActionResult, Unit>> Delete(Guid releaseVersionId,
            Guid fileId,
            bool forceDelete = false);

        Task<Either<ActionResult, Unit>> Delete(Guid releaseVersionId,
            IEnumerable<Guid> fileIds,
            bool forceDelete = false);

        Task<Either<ActionResult, Unit>> DeleteAll(Guid releaseVersionId,
            bool forceDelete = false);

        Task<Either<ActionResult, DataFileInfo>> GetInfo(Guid releaseVersionId,
            Guid fileId);

        Task<Either<ActionResult, List<DataFileInfo>>> ListAll(Guid releaseVersionId);

        Task<Either<ActionResult, List<DataFileInfo>>> ReorderDataFiles(
            Guid releaseVersionId,
            List<Guid> fileIds);

        Task<Either<ActionResult, List<DataSetUploadResultViewModel>>> Upload(
            Guid releaseVersionId,
            IFormFile dataFormFile,
            IFormFile metaFormFile,
            string dataSetTitle,
            Guid? replacingFileId,
            CancellationToken cancellationToken);

        Task<Either<ActionResult, DataFileInfo>> UploadForReplacement(
            Guid releaseVersionId,
            IFormFile dataFormFile,
            IFormFile metaFormFile,
            string dataSetTitle,
            Guid? replacingFileId,
            CancellationToken cancellationToken);

        Task<Either<ActionResult, List<DataSetUploadResultViewModel>>> UploadFromZip(
            Guid releaseVersionId,
            IFormFile zipFormFile,
            string dataSetTitle,
            Guid? replacingFileId,
            CancellationToken cancellationToken);

        Task<Either<ActionResult, DataFileInfo>> UploadFromZipForReplacement(
            Guid releaseVersionId,
            IFormFile zipFormFile,
            string dataSetTitle,
            Guid? replacingFileId,
            CancellationToken cancellationToken);

        Task<Either<ActionResult, List<DataSetUploadResultViewModel>>> UploadFromBulkZip(
            Guid releaseVersionId,
            IFormFile zipFormFile,
            CancellationToken cancellationToken);

        Task<Either<ActionResult, List<DataFileInfo>>> SaveDataSetsFromTemporaryBlobStorage(
            Guid releaseVersionId,
            List<DataSetUploadResultViewModel> dataSetFiles,
            CancellationToken cancellationToken);
    }
}
