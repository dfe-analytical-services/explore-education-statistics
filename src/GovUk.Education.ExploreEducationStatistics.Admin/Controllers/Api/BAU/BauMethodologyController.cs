using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.BAU
{
    [Route("api")]
    [ApiController]
    [Authorize]
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
            var methodologies = await _methodologyService.ListStatusAsync();

            if (methodologies.Any())
            {
                return Ok(methodologies);
            }

            return NotFound();
        }
    }
}