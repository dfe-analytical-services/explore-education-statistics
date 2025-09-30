#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces;

public interface IReleaseFileBlobService
{
    Task<Either<ActionResult, Stream>> GetDownloadStream(
        ReleaseFile releaseFile,
        CancellationToken cancellationToken = default
    );
}
