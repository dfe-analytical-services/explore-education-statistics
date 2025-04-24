#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public interface IDataSetFileStorage
{
    /// <summary>
    /// Persist a data set to permanent storage.
    /// </summary>
    /// <remarks>
    /// This process consists of three steps:<br />
    /// <list type="number">
    /// <item>generate and store entities representing the data set files to the database</item>
    /// <item>upload the physical files to the storage container</item>
    /// <item>add the data set to the import/processor queue</item>
    /// </list>
    /// </remarks>
    /// <returns>A summary of the data set details.</returns>
    Task<DataFileInfo> UploadDataSet(
        Guid releaseVersionId,
        DataSet dataSet,
        CancellationToken cancellationToken);

    /// <summary>
    /// Persist a collection of physical data set files to temporary storage.
    /// </summary>
    /// <remarks>
    /// This process is currently only implemented as part of the bulk data set upload process.<br />
    /// The data sets are uploaded to temporary storage in the first instance. Once the upload has been manually confirmed, the files are then moved to permanent storage.
    /// </remarks>
    /// <returns>A summary of each data set.</returns>
    Task<List<ZipDataSetFileViewModel>> UploadDataSetsToTemporaryStorage(
        Guid releaseVersionId,
        List<DataSet> dataSets,
        CancellationToken cancellationToken);

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
        List<ZipDataSetFileViewModel> dataSets,
        CancellationToken cancellationToken);
}
