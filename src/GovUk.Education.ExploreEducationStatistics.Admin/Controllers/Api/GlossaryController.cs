#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    [Route("api")]
    public class GlossaryController : ControllerBase
    {
        private readonly IGlossaryService _glossaryService;

        public GlossaryController(
            IGlossaryService glossaryService)
        {
            _glossaryService = glossaryService;
        }

        [HttpGet("glossary-entries/{slug}")]
        public async Task<ActionResult<GlossaryEntryViewModel>> GetGlossaryEntry(string slug)
        {
            return await _glossaryService.GetGlossaryEntry(slug)
                .HandleFailuresOrOk();
        }
    }
}
