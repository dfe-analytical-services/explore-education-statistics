using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static System.DateTime;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Security
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PermissionsController : ControllerBase
    {
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IUserService _userService;
        private readonly IPreReleaseService _preReleaseService;

        public PermissionsController(
            IUserService userService,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IPreReleaseService preReleaseService)
        {
            _userService = userService;
            _persistenceHelper = persistenceHelper;
            _preReleaseService = preReleaseService;
        }

        public class GlobalPermissions
        {
            public bool CanAccessSystem { get; set; }
            public bool CanAccessPrereleasePages { get; set; }
            public bool CanAccessAnalystPages { get; set; }
            public bool CanAccessAllImports { get; set; }
            public bool CanAccessUserAdministrationPages { get; set; }
            public bool CanAccessMethodologyAdministrationPages { get; set; }
            public bool CanManageAllTaxonomy { get; set; }
        }

        [HttpGet("access")]
        public ActionResult<GlobalPermissions> CanAccessSystem()
        {
            return new GlobalPermissions
            {
                CanAccessSystem = _userService.CheckCanAccessSystem().Result.IsRight,
                CanAccessAnalystPages = _userService.CheckCanAccessAnalystPages().Result.IsRight,
                CanAccessAllImports = _userService.CheckCanViewAllImports().Result.IsRight,
                CanAccessPrereleasePages = _userService.CheckCanAccessPrereleasePages().Result.IsRight,
                CanAccessUserAdministrationPages = _userService.CheckCanManageAllUsers().Result.IsRight,
                CanAccessMethodologyAdministrationPages = _userService.CheckCanManageAllMethodologies().Result.IsRight,
                CanManageAllTaxonomy = _userService.CheckCanManageAllTaxonomy().Result.IsRight
            };
        }

        [HttpGet("topic/{topicId}/publication/create")]
        public Task<ActionResult<bool>> CanCreatePublicationForTopic(Guid topicId)
        {
            return CheckPolicyAgainstEntity<Topic>(topicId, _userService.CheckCanCreatePublicationForTopic);
        }

        [HttpGet("publication/{publicationId}/release/create")]
        public Task<ActionResult<bool>> CanCreateReleaseForPublication(Guid publicationId)
        {
            return CheckPolicyAgainstEntity<Publication>(publicationId, _userService.CheckCanCreateReleaseForPublication);
        }

        [HttpGet("release/{releaseId}/update")]
        public Task<ActionResult<bool>> CanUpdateRelease(Guid releaseId)
        {
            return CheckPolicyAgainstEntity<Release>(releaseId, _userService.CheckCanUpdateRelease);
        }

        public class ReleaseStatusPermissionsViewModel
        {
            public bool CanMarkDraft = false;
            public bool CanMarkHigherLevelReview = false;
            public bool CanMarkApproved = false;
        }

        [HttpGet("release/{releaseId}/status")]
        public async Task<ActionResult<ReleaseStatusPermissionsViewModel>> GetReleaseStatusPermissions(Guid releaseId)
        {
            return await _persistenceHelper.CheckEntityExists<Release, Guid>(releaseId)
                .OnSuccess(release => new ReleaseStatusPermissionsViewModel
                {
                    CanMarkDraft = _userService.CheckCanMarkReleaseAsDraft(release).Result.IsRight,
                    CanMarkHigherLevelReview = _userService.CheckCanSubmitReleaseToHigherApproval(release).Result.IsRight,
                    CanMarkApproved = _userService.CheckCanApproveRelease(release).Result.IsRight,
                })
                .OrElse(() => new ReleaseStatusPermissionsViewModel());
        }
        
        [HttpGet("release/{releaseId}/data/{dataFileName}")]
        public async Task<ActionResult<DataFilePermissions>> GetDataFilePermissions(Guid releaseId, string dataFileName)
        {
            return await _userService.GetDataFilePermissions(releaseId, dataFileName);
        }

        [HttpGet("release/{releaseId}/amend")]
        public Task<ActionResult<bool>> CanMakeAmendmentOfRelease(Guid releaseId)
        {
            return CheckPolicyAgainstEntity<Release>(releaseId, _userService.CheckCanMakeAmendmentOfRelease);
        }

        [HttpGet("release/{releaseId}/prerelease/status")]
        public async Task<ActionResult<PreReleaseWindowStatus>> GetPreReleaseWindowStatus(Guid releaseId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewPreReleaseSummary)
                .OnSuccess(release => _preReleaseService.GetPreReleaseWindowStatus(release, UtcNow))
                .HandleFailuresOrOk();
        }

        [HttpGet("methodology/{methodologyId}/update")]
        public Task<ActionResult<bool>> CanUpdateMethodology(Guid methodologyId)
        {
            return CheckPolicyAgainstEntity<Methodology>(methodologyId, _userService.CheckCanUpdateMethodology);
        }

        [HttpGet("methodology/{methodologyId}/status/draft")]
        public Task<ActionResult<bool>> CanMarkMethodologyAsDraft(Guid methodologyId)
        {
            return CheckPolicyAgainstEntity<Methodology>(methodologyId, _userService.CheckCanMarkMethodologyAsDraft);
        }

        [HttpGet("methodology/{methodologyId}/status/approve")]
        public Task<ActionResult<bool>> CanApproveMethodology(Guid methodologyId)
        {
            return CheckPolicyAgainstEntity<Methodology>(methodologyId, _userService.CheckCanApproveMethodology);
        }

        private async Task<ActionResult<bool>> CheckPolicyAgainstEntity<TEntity>(
            Guid entityId,
            Func<TEntity, Task<Either<ActionResult, TEntity>>> policyCheck)
            where TEntity : class
        {
            return await _persistenceHelper
                .CheckEntityExists<TEntity, Guid>(entityId)
                .OnSuccess(policyCheck.Invoke)
                .OnSuccess(_ => new OkObjectResult(true))
                .OrElse(() => new OkObjectResult(false));
        }
    }
}
