using System.Net.Mime;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DownloadController : ControllerBase
    {
        private readonly IFileStorageService _fileStorageService;

        public DownloadController(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }
        
        [HttpGet("tree")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult<string>> GetDownloadTree()
        {
            return await this.JsonContentResultAsync(() =>
                _fileStorageService.DownloadTextAsync(PublicContentDownloadTreePath()), NoContent());
        }
    }
}