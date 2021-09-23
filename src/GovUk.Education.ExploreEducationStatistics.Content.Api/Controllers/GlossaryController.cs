#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers
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

        [HttpGet("glossary")]
        public async Task<List<GlossaryCategoryViewModel>> GetAllGlossaryEntries()
        {
            return await _glossaryService.GetAllGlossaryEntries();
        }
        
        [HttpGet("glossary/{slug}")]
        public async Task<ActionResult<GlossaryEntryViewModel>> GetGlossaryEntry(string slug)
        {
            return await _glossaryService.GetGlossaryEntry(slug)
                .HandleFailuresOrOk();
        }
    }
}
