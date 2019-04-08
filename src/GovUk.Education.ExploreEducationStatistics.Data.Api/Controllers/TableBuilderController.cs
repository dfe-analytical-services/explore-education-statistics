using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TableBuilderController : ControllerBase
    {
        private readonly ITableBuilderService _tableBuilderService;
        private readonly IReleaseService _releaseService;
        private readonly ISubjectService _subjectService;

        public TableBuilderController(
            IReleaseService releaseService,
            ISubjectService subjectService,
            ITableBuilderService tableBuilderService)
        {
            _releaseService = releaseService;
            _subjectService = subjectService;
            _tableBuilderService = tableBuilderService;
        }

        [HttpPost("geographic")]
        public ActionResult<TableBuilderResult> GetGeographic([FromBody] GeographicQueryContext query)
        {
            var result = _tableBuilderService.GetGeographic(query);
            if (result.Result.Any())
            {
                return result;
            }

            return NotFound();
        }

        [HttpPost("characteristics/local-authority")]
        public ActionResult<TableBuilderResult> GetLocalAuthority([FromBody] LaQueryContext query)
        {
            var result = _tableBuilderService.GetLocalAuthority(query);
            if (result.Result.Any())
            {
                return result;
            }

            return NotFound();
        }

        [HttpPost("characteristics/national")]
        public ActionResult<TableBuilderResult> GetNational(NationalQueryContext query)
        {
            var result = _tableBuilderService.GetNational(query);
            if (result.Result.Any())
            {
                return result;
            }

            return NotFound();
        }

        [HttpGet("meta/{typeName}/{publicationId}")]
        [Obsolete("Use subject instead")]
        // TODO Remove me - Get meta by subject instead
        public ActionResult<SubjectMetaViewModel> GetSubjectMeta(string typeName, Guid publicationId)
        {
            // TODO Remove me once UI updated to get Meta by Subject.
            // TODO Currently the UI only requests meta data for type "CharacteristicDataNational"
            if (typeName == "CharacteristicDataNational")
            {
                var latestRelease = _releaseService.GetLatestRelease(publicationId);

                var subjectForPublication = _subjectService.FindMany(subject =>
                    subject.Release.Id == latestRelease &&
                    subject.Name == "National characteristics"
                ).FirstOrDefault();

                if (subjectForPublication != null)
                {
                    return RedirectToRoute(new
                    {
                        controller = "Meta",
                        action = "GetSubjectMeta",
                        subjectId = subjectForPublication.Id
                    });
                }
            }

            return NotFound();
        }
    }
}