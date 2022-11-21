#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau;

/// <summary>
/// TODO EES-3882 Remove after migration has been run by EES-3894
/// </summary>
[Route("api")]
[ApiController]
[Authorize(Roles = GlobalRoles.RoleNames.BauUser)]
public class PublicationMigrationController
{
    private readonly IPublicationMigrationService _publicationMigrationService;

    public PublicationMigrationController(IPublicationMigrationService publicationMigrationService)
    {
        _publicationMigrationService = publicationMigrationService;
    }

    [HttpPatch("bau/set-latest-published-releases")]
    public async Task<ActionResult<Unit>> SetLatestPublishedReleases()
    {
        return await _publicationMigrationService
            .SetLatestPublishedReleases()
            .HandleFailuresOrOk();
    }
}
