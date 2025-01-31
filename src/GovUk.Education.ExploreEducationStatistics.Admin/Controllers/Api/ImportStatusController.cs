using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    [Route("api")]
    [ApiController]
    [Authorize]
    public class ImportStatusController : ControllerBase
    {
        private readonly IImportStatusBauService _importStatusBauService;

        public ImportStatusController(IImportStatusBauService importStatusBauService)
        {
            _importStatusBauService = importStatusBauService;
        }

        [HttpGet("imports/incomplete")]
        public async Task<ActionResult<List<ImportStatusBauViewModel>>> GetAllIncompleteImports()
        {
            return await _importStatusBauService
                .GetAllIncompleteImports()
                .HandleFailuresOrOk();
        }
    }
}
