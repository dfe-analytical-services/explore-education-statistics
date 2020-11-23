using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.ManageContent
{
    [Route("api")]
    [ApiController]
    [Authorize]
    public class ManageContentPageController : ControllerBase
    {
        private readonly IManageContentPageService _manageContentPageService;

        public ManageContentPageController(IManageContentPageService manageContentPageService)
        {
            _manageContentPageService = manageContentPageService;
        }

        [HttpGet("release/{releaseId}/content")]
        public async Task<ActionResult<ManageContentPageViewModel>> GetManageContentPageData(Guid releaseId)
        {
            return await _manageContentPageService
                .GetManageContentPageViewModel(releaseId)
                .HandleFailuresOrOk();
        }
    }
}