using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
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
    [Route("api")]
    [ApiController]
    [Authorize]
    public class PermissionsController : ControllerBase
    {
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IReleaseFileService _releaseFileService;
        private readonly IUserService _userService;
        private readonly IPreReleaseService _preReleaseService;

        public PermissionsController(IPersistenceHelper<ContentDbContext> persistenceHelper,
            IReleaseFileService releaseFileService,
            IUserService userService,
            IPreReleaseService preReleaseService)
        {
            _persistenceHelper = persistenceHelper;
            _releaseFileService = releaseFileService;
            _userService = userService;
            _preReleaseService = preReleaseService;
        }

        [HttpGet("permissions/access")]
        public async Task<ActionResult<GlobalPermissionsViewModel>> GetGlobalPermissions()
        {
            return new GlobalPermissionsViewModel(
                CanAccessSystem: await _userService.CheckCanAccessSystem().IsRight(),
                CanAccessAnalystPages: await _userService.CheckCanAccessAnalystPages().IsRight(),
                CanAccessAllImports: await _userService.CheckCanViewAllImports().IsRight(),
                CanAccessPrereleasePages: await _userService.CheckCanAccessPrereleasePages().IsRight(),
                CanManageAllTaxonomy: await _userService.CheckCanManageAllTaxonomy().IsRight(),
                IsBauUser: await _userService.CheckIsBauUser().IsRight());
        }

        [HttpGet("permissions/topic/{topicId:guid}/publication/create")]
        public Task<ActionResult<bool>> CanCreatePublicationForTopic(Guid topicId)
        {
            return CheckPolicyAgainstEntity<Topic>(topicId, _userService.CheckCanCreatePublicationForTopic);
        }

        [HttpGet("permissions/release/{releaseId:guid}/update")]
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

        [HttpGet("permissions/release/{releaseId:guid}/status")]
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

        [HttpGet("permissions/release/{releaseId:guid}/data/{fileId:guid}")]
        public async Task<ActionResult<DataFilePermissions>> GetDataFilePermissions(Guid releaseId, Guid fileId)
        {
            return await _releaseFileService
                .CheckFileExists(
                    releaseId: releaseId,
                    fileId: fileId,
                    FileType.Data)
                .OnSuccess(_userService.GetDataFilePermissions)
                .HandleFailuresOrOk();
        }

        [HttpGet("permissions/release/{releaseId:guid}/amend")]
        public Task<ActionResult<bool>> CanMakeAmendmentOfRelease(Guid releaseId)
        {
            return CheckPolicyAgainstEntity<Release>(releaseId, _userService.CheckCanMakeAmendmentOfRelease);
        }

        [HttpGet("permissions/release/{releaseId:guid}/prerelease/status")]
        public async Task<ActionResult<PreReleaseWindowStatus>> GetPreReleaseWindowStatus(Guid releaseId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewPreReleaseSummary)
                .OnSuccess(release => _preReleaseService.GetPreReleaseWindowStatus(release, UtcNow))
                .HandleFailuresOrOk();
        }

        [HttpGet("permissions/methodology/{methodologyId:guid}/update")]
        public Task<ActionResult<bool>> CanUpdateMethodology(Guid methodologyId)
        {
            return CheckPolicyAgainstEntity<MethodologyVersion>(methodologyId,
                _userService.CheckCanUpdateMethodologyVersion);
        }

        [HttpGet("permissions/methodology/{methodologyId:guid}/status/draft")]
        public Task<ActionResult<bool>> CanMarkMethodologyAsDraft(Guid methodologyId)
        {
            return CheckPolicyAgainstEntity<MethodologyVersion>(methodologyId,
                _userService.CheckCanMarkMethodologyVersionAsDraft);
        }

        [HttpGet("permissions/methodology/{methodologyId:guid}/status/approve")]
        public Task<ActionResult<bool>> CanApproveMethodology(Guid methodologyId)
        {
            return CheckPolicyAgainstEntity<MethodologyVersion>(methodologyId,
                _userService.CheckCanApproveMethodologyVersion);
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
