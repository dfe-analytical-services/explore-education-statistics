using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Statistics
{
    [Route("api/data/[controller]")]
    [ApiController]
    [Authorize]
    public class MetaController : ControllerBase
    {
        private readonly IReleaseMetaService _releaseMetaService;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IUserService _userService;

        public MetaController(IReleaseMetaService releaseMetaService, IPersistenceHelper<ContentDbContext> persistenceHelper, IUserService userService)
        {
            _releaseMetaService = releaseMetaService;
            _persistenceHelper = persistenceHelper;
            _userService = userService;
        }

        [HttpGet("release/{releaseId}")]
        public async Task<ActionResult<ReleaseSubjectsMetaViewModel>> GetSubjectsForRelease(Guid releaseId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(_ =>
                {
                    var subjects = _releaseMetaService.GetSubjects(releaseId).ToList();
                    if (!subjects.Any())
                    {
                        return NotFound<ReleaseSubjectsMetaViewModel>();
                    }

                    return new ReleaseSubjectsMetaViewModel
                    {
                        ReleaseId = releaseId,
                        Subjects = subjects
                    };
                })
                .HandleFailuresOr(Ok);
        }
    }
}