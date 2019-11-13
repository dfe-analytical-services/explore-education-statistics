using System;
using System.Linq;
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
            var subjects = _releaseMetaService.GetSubjects(releaseId).ToList();
            if (!subjects.Any())
            {
                return NotFound();
            }
            
            return new ReleaseSubjectsMetaViewModel
            {
                ReleaseId = releaseId,
                Subjects = subjects
            };
        }
    }
}