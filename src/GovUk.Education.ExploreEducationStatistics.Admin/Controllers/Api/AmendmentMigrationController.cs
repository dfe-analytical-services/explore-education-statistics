using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    // TODO BAU-405 - temporary code to help seed the Release-File tables from Blob storage 
    [Route("api")]
    [ApiController]
    [Authorize]
    public class AmendmentMigrationController : ControllerBase
    {   
        private readonly IAmendmentMigrationService _amendmentMigrationService;

        public AmendmentMigrationController(IAmendmentMigrationService amendmentMigrationService)
        {
            _amendmentMigrationService = amendmentMigrationService;
        }
        
        [HttpPost("amendments/fix-missing-subject-id")]
        public async Task<ActionResult<bool>> FixMissingSubjectId()
        {
            return await _amendmentMigrationService
                .FixMissingSubjectId()
                .HandleFailuresOrOk();
        }
    }
}