using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers.Cache;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("api/meta")]
    [ApiController]
    public class TableBuilderMetaController : ControllerBase
    {
        private readonly ISubjectMetaService _subjectMetaService;
        private readonly IReleaseSubjectRepository _releaseSubjectRepository;

        public TableBuilderMetaController(
            ISubjectMetaService subjectMetaService, 
            IReleaseSubjectRepository releaseSubjectRepository)
        {
            _subjectMetaService = subjectMetaService;
            _releaseSubjectRepository = releaseSubjectRepository;
        }

        [HttpGet("subject/{subjectId}")]
        public Task<ActionResult<SubjectMetaViewModel>> GetSubjectMetaAsync(Guid subjectId)
        {
            return _releaseSubjectRepository
                .GetLatestPublishedReleaseSubjectForSubject(subjectId)
                .OnSuccess(GetSubjectMeta)
                .HandleFailuresOrOk();
        }

        [BlobCache(typeof(SubjectMetaCacheKey))]
        private Task<Either<ActionResult, SubjectMetaViewModel>> GetSubjectMeta(ReleaseSubject releaseSubject)
        {
            return _subjectMetaService.GetSubjectMeta(releaseSubject.SubjectId);
        }

        [HttpPost("subject")]
        public Task<ActionResult<SubjectMetaViewModel>> GetSubjectMetaAsync(
            [FromBody] SubjectMetaQueryContext query)
        {
            return _subjectMetaService.GetSubjectMeta(query).HandleFailuresOrOk();
        }
    }
}