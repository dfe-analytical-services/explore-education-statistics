using System.Net.Mime;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers
{
    [Route("api/[controller]")]
    public class MethodologyController : ControllerBase
    {
        private readonly IFileStorageService _fileStorageService;
        
        public MethodologyController(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }
        
        [HttpGet("tree")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult<string>> GetMethodologyTree()
        {
            return await this.JsonContentResultAsync(() =>
                _fileStorageService.DownloadTextAsync(PublicContentMethodologyTreePath()), NoContent());
        }
        
        [HttpGet("{slug}")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult<string>> Get(string slug)
        {
            return await this.JsonContentResultAsync(() =>
                _fileStorageService.DownloadTextAsync(PublicContentMethodologyPath(slug)), NotFound());
        }
    }
}