#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;

// !!!!!
// TODO: Move `MethodologyVersionRepository#IsToBePublished` `MethodologyApprovalService` and change to be private after removing this controller
// !!!!!

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau
{
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

        public class MethodologyMigrationResult
        {
            public Guid MethodologyId { get; set; }
            public Guid? LatestMethodologyVersionId { get; set; }
        }

        [HttpPatch("migration/set-methodology-latest-published-version-ids")]
        public async Task<ActionResult<List<MethodologyMigrationResult>>> MigrateMethodologyLatestPublishedVersionIds(
            [FromQuery] bool dryRun = true)
        {
            var allMethodologies = await _context.Methodologies
                .Include(m => m.Versions)
                .ToListAsync();

            var results = new List<MethodologyMigrationResult>();

            foreach (var methodology in allMethodologies)
            {
                var latestPublishedMethodologyVersion = await methodology.Versions
                    .ToAsyncEnumerable()
                    .WhereAwait(async mv => await _methodologyVersionRepository.IsToBePublished(mv))
                    .SingleOrDefaultAsync();

                methodology.LatestPublishedVersionId = latestPublishedMethodologyVersion?.Id;

                var migrationResult = new MethodologyMigrationResult
                {
                    MethodologyId = methodology.Id,
                    LatestMethodologyVersionId = latestPublishedMethodologyVersion?.Id,
                };
                results.Add(migrationResult);
            }

            if (!dryRun)
            {
                await _context.SaveChangesAsync();
            }

            return results;
        }
    }
}
