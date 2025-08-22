#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;

public interface IDataSetCandidateService
{
    Task<Either<ActionResult, IReadOnlyList<DataSetCandidateViewModel>>> ListCandidates(
        Guid releaseVersionId,
        CancellationToken cancellationToken = default);
}
