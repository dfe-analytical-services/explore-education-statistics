using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces;

public interface IPublicReleaseFileBlobService : IReleaseFileBlobService
{
    Task<Either<ActionResult, string>> GetDownloadRedirectPath(
        ReleaseFile releaseFile,
        CancellationToken cancellationToken = default
    );
}
