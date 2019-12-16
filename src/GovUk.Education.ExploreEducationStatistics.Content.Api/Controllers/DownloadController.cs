using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.ViewModels;
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

        /// <response code="204">If the item is null</response>    
        [HttpGet("tree")]
        [ProducesResponseType(typeof(List<ThemeTree>), 200)]
        [ProducesResponseType(204)]
        [Produces("application/json")]
        public async Task<ActionResult<string>> GetDownloadTree()
        {
            return await this.JsonContentResultAsync(() =>
                _fileStorageService.DownloadTextAsync(PublicContentDownloadTreePath()), NoContent());
        }
    }
}