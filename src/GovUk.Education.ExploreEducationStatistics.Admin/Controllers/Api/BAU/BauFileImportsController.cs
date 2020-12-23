using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.BAU
{
    [Route("api")]
    [ApiController]
    [Authorize]
    public class BauFileImportsController : Controller
    {
        private readonly IImportService _importService;

        public BauFileImportsController(IImportService importService)
        {
            _importService = importService;
        }

        [HttpPost("release/{releaseId}/data/{dataFileName}/import/cancel")]
        public async Task<IActionResult> CancelFileImport([FromRoute] ReleaseFileImportInfo file)
        {
            return await _importService
                .CancelImport(file)
                .HandleFailuresOr(result => new AcceptedResult());
        }
    }
}