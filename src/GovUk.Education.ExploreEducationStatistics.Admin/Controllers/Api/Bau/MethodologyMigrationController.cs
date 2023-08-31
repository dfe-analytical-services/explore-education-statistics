#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;
using static GovUk.Education.ExploreEducationStatistics.Admin.Services.DataBlockMigrationService;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau;

[Route("api")]
[ApiController]
[Authorize(Roles = RoleNames.BauUser)]
public class MethodologyMigrationController : ControllerBase
{
    private readonly ContentDbContext _context;
    private readonly IMethodologyVersionRepository _methodologyVersionRepository;

    public MethodologyMigrationController(
        ContentDbContext context,
        IMethodologyVersionRepository methodologyVersionRepository)
    {
        _context = context;
        _methodologyVersionRepository = methodologyVersionRepository;
    }

    [HttpPatch("migration/set-latestpublishedversionid")]
    public async Task<ActionResult<List<MapMigrationResult>>> MigrateMethodologyLatestPublishedVersionId(
        [FromQuery] bool dryRun = true)
    {
        var allMethodologies = _context.Methodologies
            .Include(m => m.Versions)
            .ToList();

        foreach (var methodology in allMethodologies)
        {
            var latestPublicMethodologyVersion = await methodology.Versions
                .ToAsyncEnumerable()
                .WhereAwait(async mv => await _methodologyVersionRepository.IsPubliclyAccessible(mv.Id))
                .SingleOrDefaultAsync();

            if (latestPublicMethodologyVersion != null)
            {
                methodology.LatestPublishedVersionId = latestPublicMethodologyVersion.Id;
            }
        }

        await _context.SaveChangesAsync();
    }
}

