#nullable enable
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers
{
    [Route("api")]
    [Produces(MediaTypeNames.Application.Json)]
    public class PublicationController : ControllerBase
    {
        private readonly IPersistenceHelper<ContentDbContext> _contentPersistenceHelper;

        public PublicationController(IPersistenceHelper<ContentDbContext> contentPersistenceHelper)
        {
            _contentPersistenceHelper = contentPersistenceHelper;
        }

        [BlobCache(typeof(PublicationTitleCacheKey))]
        [HttpGet("publications/{slug}/title")]
        public async Task<ActionResult<PublicationTitleViewModel>> GetPublicationTitle(string slug)
        {
            return await _contentPersistenceHelper
                .CheckEntityExists<Publication>(query => query
                    .Where(p => p.Slug == slug))
                .OnSuccess(p => new PublicationTitleViewModel
                {
                    Id = p.Id,
                    Title = p.Title,
                })
                .HandleFailuresOrOk();
        }
    }
}
