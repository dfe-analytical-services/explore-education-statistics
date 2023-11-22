#nullable enable
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau;

/// <summary>
/// TODO EES-4661 Remove after the EES-4660 data guidance migration is successful
/// </summary>
[Route("api")]
[ApiController]
[Authorize(Roles = GlobalRoles.RoleNames.BauUser)]
public class DataGuidanceMigrationController : ControllerBase
{
    private readonly IDataGuidanceMigrationService _dataGuidanceMigrationService;

    public DataGuidanceMigrationController(IDataGuidanceMigrationService dataGuidanceMigrationService)
    {
        _dataGuidanceMigrationService = dataGuidanceMigrationService;
    }

    [HttpPatch("bau/migrate-data-guidance")]
    public async Task<ActionResult<DataGuidanceMigrationReport>> MigrateDataGuidance([FromQuery] bool dryRun = true,
        CancellationToken cancellationToken = default)
    {
        return await _dataGuidanceMigrationService
            .MigrateDataGuidance(dryRun, cancellationToken)
            .HandleFailuresOrOk();
    }
}
