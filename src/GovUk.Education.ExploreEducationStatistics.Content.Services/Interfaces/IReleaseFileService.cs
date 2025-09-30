using GovUk.Education.ExploreEducationStatistics.Analytics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;

public interface IReleaseFileService
{
    Task<Either<ActionResult, IList<ReleaseFileViewModel>>> ListReleaseFiles(
        ReleaseFileListRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Either<ActionResult, FileStreamResult>> StreamFile(Guid releaseVersionId, Guid fileId);

    /// <summary>
    /// Pipe a zip file containing some release files to a stream.
    /// </summary>
    /// <param name="releaseVersionId">The release version to fetch files from</param>
    /// <param name="outputStream">The stream to output the zip file to</param>
    /// <param name="fromPage">The page the request was made from</param>
    /// <param name="fileIds">Files to include in the zip file. Set to null to include all files.</param>
    /// <param name="cancellationToken">To cancel the appending of any further files to the stream</param>
    /// <returns>An Either returning nothing if successful</returns>
    Task<Either<ActionResult, Unit>> ZipFilesToStream(
        Guid releaseVersionId,
        Stream outputStream,
        AnalyticsFromPage fromPage,
        IEnumerable<Guid>? fileIds = null,
        CancellationToken cancellationToken = default
    );
}
