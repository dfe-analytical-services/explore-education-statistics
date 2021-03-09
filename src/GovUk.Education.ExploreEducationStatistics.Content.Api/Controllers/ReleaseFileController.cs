using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers
{
    [Route("api")]
    [ApiController]
    public class ReleaseFileController : ControllerBase
    {
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IReleaseFileService _releaseFileService;

        public ReleaseFileController(IPersistenceHelper<ContentDbContext> persistenceHelper,
            IReleaseFileService releaseFileService)
        {
            _persistenceHelper = persistenceHelper;
            _releaseFileService = releaseFileService;
        }

        [HttpGet("releases/{releaseId}/files/{fileId}")]
        public async Task<ActionResult> Stream(string releaseId, string fileId)
        {
            if (Guid.TryParse(releaseId, out var releaseIdAsGuid) &&
                Guid.TryParse(fileId, out var fileIdAsGuid))
            {
                return await _releaseFileService
                    .Stream(releaseIdAsGuid, fileIdAsGuid)
                    .HandleFailures();
            }

            return NotFound();
        }

        [HttpGet("releases/{releaseId}/files/all")]
        public async Task<ActionResult> StreamAll(string releaseId)
        {
            if (Guid.TryParse(releaseId, out var releaseIdAsGuid))
            {
                return await _persistenceHelper
                    .CheckEntityExists<Release>(releaseIdAsGuid,
                        q => q.Include(release => release.Publication))
                    .OnSuccess(release => _releaseFileService.StreamByPath(release.AllFilesZipPath()))
                    .HandleFailures();
            }

            return NotFound();
        }
    }
}
