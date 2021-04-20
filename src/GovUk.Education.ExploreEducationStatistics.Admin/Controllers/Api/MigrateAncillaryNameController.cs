using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{ 
    /*
     * EES-2052 Temporary controller for copying ancillary file name blob metadata to ReleaseFiles.Name
     */
    [ApiController]
    [Authorize]
    public class MigrateAncillaryNameController : ControllerBase
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IUserService _userService;
        private readonly ILogger<MigrateAncillaryNameController> _logger;

        public MigrateAncillaryNameController(
            ContentDbContext contentDbContext,
            IBlobStorageService blobStorageService,
            IUserService userService,
            ILogger<MigrateAncillaryNameController> logger)
        {
            _contentDbContext = contentDbContext;
            _blobStorageService = blobStorageService;
            _userService = userService;
            _logger = logger;
        }

        [HttpPatch("api/files/migrate-ancillary-names")]
        public async Task<ActionResult<Unit>> MigrateAncillaryNames()
        {
            return await _userService.CheckCanRunMigrations()
                .OnSuccessVoid(async () =>
                {
                    await _contentDbContext.ReleaseFiles
                        .Include(rf => rf.File)
                        .Where(rf =>
                            rf.File.Type == FileType.Ancillary
                            && rf.Name == null)
                        .ToList()
                        .ForEachAsync(async rf =>
                        {
                            var exists = await _blobStorageService.CheckBlobExists(
                                BlobContainers.PrivateReleaseFiles,
                                rf.Path());
                            if (!exists)
                            {
                                _logger.LogError(
                                    $"MigrateAncillaryNames - Could not find blob file - File:'{rf.FileId}'");
                            }
                            else
                            {
                                var blobInfo = await _blobStorageService.GetBlob(
                                    BlobContainers.PrivateReleaseFiles,
                                    rf.Path());
                                _contentDbContext.Update(rf);
                                rf.Name = blobInfo.Meta.TryGetValue("name", out var name) ? name : null;
                            }
                        });
                    await _contentDbContext.SaveChangesAsync();
                })
                .HandleFailuresOrOk();
        }
    }
}
