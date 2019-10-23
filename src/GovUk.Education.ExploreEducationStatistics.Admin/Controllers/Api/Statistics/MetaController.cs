using System;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Statistics
{
    [Route("api/data/[controller]")]
    [ApiController]
    [Authorize]
    public class MetaController : ControllerBase
    {
        private readonly IReleaseMetaService _releaseMetaService;

        public MetaController(IReleaseMetaService releaseMetaService)
        {
            _releaseMetaService = releaseMetaService;
        }

        [HttpGet("release/{releaseId}")]
        public ActionResult<ReleaseSubjectsMetaViewModel> GetSubjectsForRelease(Guid releaseId)
        {
            var viewModel = _releaseMetaService.GetSubjects(releaseId);
            if (viewModel == null)
            {
                return NotFound();
            }

            return viewModel;
        }
    }
}