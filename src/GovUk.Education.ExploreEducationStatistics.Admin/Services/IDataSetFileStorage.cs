#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public interface IDataSetFileStorage
{
    /// <summary>
    /// Persist a collection of physical data set files to temporary storage.
    /// </summary>
    /// <remarks>
    /// The data sets are uploaded to temporary storage in the first instance. Once the upload has been manually confirmed, the files are then moved to permanent storage.
    /// </remarks>
    /// <returns>A summary of each data set.</returns>
    Task<List<DataSetUpload>> UploadDataSetsToTemporaryStorage(
        Guid releaseVersionId,
        List<DataSet> dataSets,
        CancellationToken cancellationToken
    );

    Task<Either<ActionResult, BlobDownloadToken>> GetTemporaryFileDownloadToken(
        Guid releaseVersionId,
        Guid dataSetUploadId,
        FileType fileType,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Persist an existing temporary data set to permanent storage.
    /// </summary>
    /// <remarks>
    /// This process consists of three steps:<br />
    /// <list type="number">
    /// <item>generate and store entities representing the data set files to the database</item>
    /// <item>move the referenced files from temporary storage to the permanent container</item>
    /// <item>add the data set to the import/processor queue</item>
    /// </list>
    /// </remarks>
    /// <returns>A collection of the entities which represent the data set files.</returns>
    Task<List<ReleaseFile>> MoveDataSetsToPermanentStorage(
        Guid releaseVersionId,
        List<DataSetUpload> dataSetUploads,
        CancellationToken cancellationToken
    );

    Task UpdateDataSetUpload(
        Guid dataSetUploadId,
        DataSetScreenerResponse? screenerResult,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Create, or overwrite an existing data set record in the database with a new one.
    /// </summary>
    /// <remarks>Allows an upload which previously failed screening to be easily overwritten, without the need for manual deletion.</remarks>
    /// <returns>The new entity.</returns>
    Task<DataSetUpload> CreateOrReplaceExistingDataSetUpload(
        Guid releaseVersionId,
        DataSetUpload dataSetUpload,
        CancellationToken cancellationToken
    );
}
