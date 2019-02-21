using GovUk.Education.ExploreEducationStatistics.Data.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeedController : ControllerBase
    {
        private readonly SeedService _seedService;

        public SeedController(SeedService seedService)
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