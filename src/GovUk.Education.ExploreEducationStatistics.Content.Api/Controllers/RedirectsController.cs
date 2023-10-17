#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers
{
    [Route("api")]
    public class RedirectsController : ControllerBase
    {
        private readonly IRedirectsService _redirectsService;

        public RedirectsController(
            IRedirectsService redirectsService)
        {
            _redirectsService = redirectsService;
        }

        [HttpGet("redirects")]
        public async Task<ActionResult<RedirectsViewModel>> ListRedirects()
        {
            return await _redirectsService.List()
                .HandleFailuresOrOk();
        }
    }
}
