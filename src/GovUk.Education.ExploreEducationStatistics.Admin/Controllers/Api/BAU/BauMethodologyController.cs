using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.BAU
{
    [Route("api")]
    [ApiController]
    [Authorize(Policy = "CanManageMethodologiesOnSystem")]
    public class BauMethodologyController : ControllerBase
    {
        private readonly IMethodologyService _methodologyService;
        
        public BauMethodologyController(IMethodologyService methodologyService)
        {
            _methodologyService = methodologyService;
        }
        
        [HttpGet("bau/methodology")]
        public async Task<ActionResult<List<MethodologyStatusViewModel>>> GetMethodologyList()
        {
            return await _methodologyService
                .ListStatusAsync()
                .HandleFailuresOr(Ok);
        }
    }
}