using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.ViewModels;
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

        // GET
        /// <response code="204">If the item is null</response>    
        [HttpGet("tree")]
        [ProducesResponseType(typeof(List<ThemeTree>),200)]
        [ProducesResponseType(204)]
        [Produces("application/json")]
        public async Task<ActionResult<string>> GetMethodologyTree()
        {
            return await this.JsonContentResultAsync(() =>
                _fileStorageService.DownloadTextAsync(PublicContentMethodologyTreePath()), NoContent());
        }
        
        // GET api/methodology/name-of-content
        [HttpGet("{slug}")]
        [ProducesResponseType(typeof(Methodology), 200)]
        [ProducesResponseType(404)]
        [Produces("application/json")]
        public async Task<ActionResult<string>> Get(string slug)
        {
            return await this.JsonContentResultAsync(() =>
                _fileStorageService.DownloadTextAsync(PublicContentMethodologyPath(slug)), NotFound());
        }
    }
}