using GovUk.Education.ExploreEducationStatistics.Data.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeedController : ControllerBase
    {
        private readonly ISeedService _seedService;

        
        public SeedController(ISeedService seedService)
        {
            _seedService = seedService;
        }

        [HttpGet]
        public void Seed()
        {
            _seedService.Seed();
        }

        [HttpDelete("DeleteAll")]
        public string DeleteAll()
        {
            _seedService.DeleteAll();
            return "Deleted all rows";
        }
    }
}