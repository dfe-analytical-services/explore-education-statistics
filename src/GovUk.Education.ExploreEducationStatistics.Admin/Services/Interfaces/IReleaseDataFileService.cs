#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

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

    Task<Either<ActionResult, DataSetAccoutrementsViewModel>> GetAccoutrementsSummary(
        Guid releaseVersionId,
        Guid fileId);

    Task<Either<ActionResult, List<DataFileInfo>>> ListAll(Guid releaseVersionId);

    Task<Either<ActionResult, List<DataFileInfo>>> ReorderDataFiles(
        Guid releaseVersionId,
        List<Guid> fileIds);

    Task<Either<ActionResult, List<DataSetUploadViewModel>>> Upload(
        Guid releaseVersionId,
        IManagedStreamFile dataFile,
        IManagedStreamFile metaFile,
        string dataSetTitle,
        CancellationToken cancellationToken);

    Task<Either<ActionResult, List<DataSetUploadViewModel>>> UploadFromZip(
        Guid releaseVersionId,
        IManagedStreamZipFile zipFile,
        string dataSetTitle,
        CancellationToken cancellationToken);

    Task<Either<ActionResult, List<DataSetUploadViewModel>>> UploadFromBulkZip(
        Guid releaseVersionId,
        IManagedStreamZipFile zipFormFile,
        CancellationToken cancellationToken);

    Task<Either<ActionResult, Unit>> SaveDataSetsFromTemporaryBlobStorage(
        Guid releaseVersionId,
        List<Guid> dataSetUploadIds,
        CancellationToken cancellationToken);
}
