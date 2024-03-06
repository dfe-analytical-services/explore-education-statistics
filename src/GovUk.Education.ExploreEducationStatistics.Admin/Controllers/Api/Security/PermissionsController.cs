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
        private readonly IUserPublicationRoleRepository _publicationRoleRepository;
        private readonly IUserReleaseRoleRepository _releaseRoleRepository;

        public PermissionsController(
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IReleaseFileService releaseFileService,
            IUserService userService,
            IPreReleaseService preReleaseService,
            IUserPublicationRoleRepository publicationRoleRepository,
            IUserReleaseRoleRepository releaseRoleRepository)
        {
            _persistenceHelper = persistenceHelper;
            _releaseFileService = releaseFileService;
            _userService = userService;
            _preReleaseService = preReleaseService;
            _publicationRoleRepository = publicationRoleRepository;
            _releaseRoleRepository = releaseRoleRepository;
        }

        [HttpGet("permissions/access")]
        public async Task<ActionResult<GlobalPermissionsViewModel>> GetGlobalPermissions()
        {
            var isBauUser = await _userService.CheckIsBauUser().IsRight();

            // Note that we are deliberately not giving BAU Users the Approver permission, as we would
            // not expect a user with that role to be the target of specific Release or Publication
            // roles.  If they were to be given Approver permissions, we would therefore assume that they
            // should have Approver access to ALL Methodologies and Releases that are awaiting approval,
            // which would potentially be overwhelming. 
            var isApprover = !isBauUser && (await IsReleaseApprover() || await IsPublicationApprover());

            return new GlobalPermissionsViewModel(
                CanAccessSystem: await _userService.CheckCanAccessSystem().IsRight(),
                CanAccessAnalystPages: await _userService.CheckCanAccessAnalystPages().IsRight(),
                CanAccessAllImports: await _userService.CheckCanViewAllImports().IsRight(),
                CanAccessPrereleasePages: await _userService.CheckCanAccessPrereleasePages().IsRight(),
                CanManageAllTaxonomy: await _userService.CheckCanManageAllTaxonomy().IsRight(),
                IsBauUser: isBauUser,
                IsApprover: isApprover);
        }

        [HttpGet("permissions/topic/{topicId:guid}/publication/create")]
        public Task<ActionResult<bool>> CanCreatePublicationForTopic(Guid topicId)
        {
            return CheckPolicyAgainstEntity<Topic>(topicId, _userService.CheckCanCreatePublicationForTopic);
        }

        [HttpGet("permissions/release/{releaseVersionId:guid}/update")]
        public Task<ActionResult<bool>> CanUpdateRelease(Guid releaseVersionId)
        {
            return CheckPolicyAgainstEntity<ReleaseVersion>(releaseVersionId, _userService.CheckCanUpdateReleaseVersion);
        }

        public record ReleaseStatusPermissionsViewModel(
            bool CanMarkDraft, bool CanMarkHigherLevelReview, bool CanMarkApproved);

        [HttpGet("permissions/release/{releaseVersionId:guid}/status")]
        public async Task<ActionResult<ReleaseStatusPermissionsViewModel>> GetReleaseStatusPermissions(
            Guid releaseVersionId)
        {
            return await _persistenceHelper.CheckEntityExists<ReleaseVersion>(releaseVersionId)
                .OnSuccess(async releaseVersion =>
                {
                    var canMarkDraft = await _userService
                        .CheckCanMarkReleaseVersionAsDraft(releaseVersion);
                    var canMarkHigherReview = await _userService
                        .CheckCanSubmitReleaseVersionForHigherReview(releaseVersion);
                    var canMarkApproved = await _userService
                        .CheckCanApproveReleaseVersion(releaseVersion);

                    return new ReleaseStatusPermissionsViewModel(
                        CanMarkDraft: canMarkDraft.IsRight,
                        CanMarkHigherLevelReview: canMarkHigherReview.IsRight,
                        CanMarkApproved: canMarkApproved.IsRight
                    );
                })
                .OrElse(() => new ReleaseStatusPermissionsViewModel(
                    false, false, false));
        }

        [HttpGet("permissions/release/{releaseVersionId:guid}/data/{fileId:guid}")]
        public async Task<ActionResult<DataFilePermissions>> GetDataFilePermissions(Guid releaseVersionId, Guid fileId)
        {
            return await _releaseFileService
                .CheckFileExists(
                    releaseVersionId: releaseVersionId,
                    fileId: fileId,
                    FileType.Data)
                .OnSuccess(_userService.GetDataFilePermissions)
                .HandleFailuresOrOk();
        }

        [HttpGet("permissions/release/{releaseVersionId:guid}/amend")]
        public Task<ActionResult<bool>> CanMakeAmendmentOfRelease(Guid releaseVersionId)
        {
            return CheckPolicyAgainstEntity<ReleaseVersion>(releaseVersionId, _userService.CheckCanMakeAmendmentOfRelease);
        }

        [HttpGet("permissions/release/{releaseVersionId:guid}/prerelease/status")]
        public async Task<ActionResult<PreReleaseWindowStatus>> GetPreReleaseWindowStatus(Guid releaseVersionId)
        {
            return await _persistenceHelper
                .CheckEntityExists<ReleaseVersion>(releaseVersionId)
                .OnSuccess(_userService.CheckCanViewPreReleaseSummary)
                .OnSuccess(releaseVersion => _preReleaseService.GetPreReleaseWindowStatus(releaseVersion, UtcNow))
                .HandleFailuresOrOk();
        }

        [HttpGet("permissions/methodology/{methodologyId:guid}/update")]
        public Task<ActionResult<bool>> CanUpdateMethodology(Guid methodologyId)
        {
            return CheckPolicyAgainstEntity<MethodologyVersion>(methodologyId,
                _userService.CheckCanUpdateMethodologyVersion);
        }

        public record MethodologyApprovalStatusPermissions(
            bool CanMarkDraft, bool CanMarkHigherLevelReview, bool CanMarkApproved);

        [HttpGet("permissions/methodology/{methodologyVersionId:guid}/status")]
        public async Task<ActionResult<MethodologyApprovalStatusPermissions>> GetMethodologyApprovalPermissions(
            Guid methodologyVersionId)
        {
            return await _persistenceHelper.CheckEntityExists<MethodologyVersion>(methodologyVersionId)
                .OnSuccess(async methodologyVersion =>
                {
                    var canMarkDraft = await _userService
                        .CheckCanMarkMethodologyVersionAsDraft(methodologyVersion);
                    var canMarkHigherLevelReview = await _userService
                        .CheckCanSubmitMethodologyForHigherReview(methodologyVersion);
                    var canMarkApproved = await _userService
                        .CheckCanApproveMethodologyVersion(methodologyVersion);

                    return new MethodologyApprovalStatusPermissions
                    (
                        CanMarkDraft: canMarkDraft.IsRight,
                        CanMarkHigherLevelReview: canMarkHigherLevelReview.IsRight,
                        CanMarkApproved: canMarkApproved.IsRight
                    );
                })
                .OrElse(() => new MethodologyApprovalStatusPermissions(
                    false, false, false));
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

        private async Task<bool> IsReleaseApprover()
        {
            return (await _releaseRoleRepository.GetDistinctRolesByUser(_userService.GetUserId()))
                .Contains(ReleaseRole.Approver);
        }

        private async Task<bool> IsPublicationApprover()
        {
            return (await _publicationRoleRepository.GetDistinctRolesByUser(_userService.GetUserId()))
                .Contains(PublicationRole.Approver);
        }
    }
}
