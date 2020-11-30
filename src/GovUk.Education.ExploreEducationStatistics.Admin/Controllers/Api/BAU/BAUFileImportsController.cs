using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
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
    public class BAUFileImportsController : Controller
    {
        private readonly IImportStatusService _importStatusService;
        private readonly IUserService _userService;

        public BAUFileImportsController(IImportStatusService importStatusService, IUserService userService)
        {
            _importStatusService = importStatusService;
            _userService = userService;
        }

        [HttpPost("release/{releaseId}/data/{filename}/import/cancel")]
        public async Task<IActionResult> CancelFileImport([FromRoute] ReleaseFileUploadInfo file)
        {
            return await _userService
                .CheckCanCancelFileImport(file)
                .OnSuccess(import => _importStatusService.UpdateStatus(file.ReleaseId, file.Filename, IStatus.CANCELLED))
                .HandleFailuresOr(Ok);
        }
    }
}