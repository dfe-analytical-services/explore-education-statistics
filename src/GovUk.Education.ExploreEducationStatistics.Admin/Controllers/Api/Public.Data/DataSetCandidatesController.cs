#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Public.Data;

[Authorize]
[ApiController]
[Route("api/public-data/data-set-candidates")]
public class DataSetCandidatesController(IDataSetCandidateService dataSetCandidateService)
    : ControllerBase
{
    [HttpGet]
    [Produces("application/json")]
    public async Task<ActionResult<IReadOnlyList<DataSetCandidateViewModel>>> ListDataSetCandidates(
        [FromQuery] Guid releaseVersionId,
        CancellationToken cancellationToken
    )
    {
        return await dataSetCandidateService
            .ListCandidates(releaseVersionId, cancellationToken)
            .HandleFailuresOrOk();
    }
}
