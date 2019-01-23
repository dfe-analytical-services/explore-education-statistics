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
        public string Seed()
        {
            if (_seedService.CanSeed())
            {
                return "Inserted " + _seedService.Seed() + " rows";
            }

            return "Cannot seed. Database is not empty";
        }

        [HttpDelete("DropAllCollections")]
        public void DropAllCollections()
        {
            _seedService.DropAllCollections();
        }
    }
}