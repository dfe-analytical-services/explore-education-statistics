using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers
{
    [Route("api")]
    public class DataGuidanceController
    {
        private readonly IDataGuidanceService _dataGuidanceService;

        public DataGuidanceController(IDataGuidanceService dataGuidanceService)
        {
            _dataGuidanceService = dataGuidanceService;
        }

        [HttpGet("publications/{publicationSlug}/releases/latest/data-guidance")]
        public async Task<ActionResult<DataGuidanceViewModel>> GetLatest(string publicationSlug)
        {
            return await _dataGuidanceService.Get(publicationSlug)
                .HandleFailuresOrOk();
        }

        [HttpGet("publications/{publicationSlug}/releases/{releaseSlug}/data-guidance")]
        public async Task<ActionResult<DataGuidanceViewModel>> Get(
            string publicationSlug,
            string releaseSlug)
        {
            return await _dataGuidanceService.Get(
                    publicationSlug,
                    releaseSlug
                )
                .HandleFailuresOrOk();
        }
    }
}
