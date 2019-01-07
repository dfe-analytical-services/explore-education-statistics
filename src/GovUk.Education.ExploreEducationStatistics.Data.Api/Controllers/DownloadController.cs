using System.IO;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("downloads")]
    [ApiController]
    public class DownloadController : ControllerBase
    {
        [HttpGet("{publication}/csv")]
        public IActionResult GetCsvBundle(string publication)
        {
            var filename = string.Empty;
            switch (publication)
            {
                case "pupil-absence-in-schools-in-england":
                    filename = "absence";
                    break;
                case "permanent-and-fixed-period-exclusions": 
                    filename = "exclusion";
                    break;
                case "schools-pupils-and-their-characteristics": 
                    filename = "schpupnum";
                    break;
                default:
                    return NotFound();
            }
            
            const string contentType = "application/zip";
            HttpContext.Response.ContentType = contentType;


            var file = filename + ".zip";
            var directory = Directory.GetCurrentDirectory();
            var newPath = Path.GetFullPath(directory);

            var path = newPath + "/wwwroot/data/zip/" + file;

            var result = new FileContentResult(System.IO.File.ReadAllBytes(path), contentType)
            {
                FileDownloadName = $"{publication}.zip"
            };

            return result;
        }
    }
}