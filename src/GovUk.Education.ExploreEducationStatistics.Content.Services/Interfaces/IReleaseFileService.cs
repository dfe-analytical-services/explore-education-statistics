#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces
{
    public interface IReleaseFileService
    {
        Task<Either<ActionResult, FileStreamResult>> StreamFile(Guid releaseId, Guid fileId);

        /// <summary>
        /// Pipe a zip file containing some release files to a stream.
        /// </summary>
        /// <param name="releaseId">The release to fetch files from</param>
        /// <param name="outputStream">The stream to output the zip file to</param>
        /// <param name="fileIds">Files to include in the zip file. Set to null to include all files.</param>
        /// <param name="cancellationToken">To cancel the appending of any further files to the stream</param>
        /// <returns>An Either returning nothing if successful</returns>
        Task<Either<ActionResult, Unit>> ZipFilesToStream(
            Guid releaseId,
            Stream outputStream,
            IEnumerable<Guid>? fileIds = null,
            CancellationToken? cancellationToken = null);
    }
}