#nullable enable
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ExternalMethodologyViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ExternalMethodologyViewModel;
using PublicationViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.PublicationViewModel;
using ReleaseVersionSummaryViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ReleaseVersionSummaryViewModel;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;

[Authorize]
[ApiController]
public class PublicationController(IPublicationService publicationService, IUserRoleService roleService)
    : ControllerBase
{
    [HttpGet("api/publications")]
    public async Task<ActionResult<List<PublicationViewModel>>> ListPublications([FromQuery] Guid? themeId)
    {
        return await publicationService.ListPublications(themeId).HandleFailuresOrOk();
    }

    [HttpGet("api/publication-summaries")]
    public async Task<ActionResult<List<PublicationSummaryViewModel>>> ListPublicationSummaries()
    {
        return await publicationService.ListPublicationSummaries().HandleFailuresOrOk();
    }

    [HttpGet("api/publications/{publicationId:guid}")]
    public async Task<ActionResult<PublicationViewModel>> GetPublication(
        Guid publicationId,
        [FromQuery] bool includePermissions = false,
        CancellationToken cancellationToken = default
    ) =>
        await publicationService
            .GetPublication(
                publicationId: publicationId,
                includePermissions: includePermissions,
                cancellationToken: cancellationToken
            )
            .HandleFailuresOrOk();

    [HttpGet("api/publication/{publicationId:guid}/external-methodology")]
    public async Task<ActionResult<ExternalMethodologyViewModel>> GetExternalMethodology(Guid publicationId)
    {
        return await publicationService.GetExternalMethodology(publicationId).HandleFailuresOrOk();
    }

    [HttpPut("api/publication/{publicationId:guid}/external-methodology")]
    public async Task<ActionResult<ExternalMethodologyViewModel>> UpdateExternalMethodology(
        Guid publicationId,
        ExternalMethodologySaveRequest updatedExternalMethodology
    )
    {
        return await publicationService
            .UpdateExternalMethodology(publicationId, updatedExternalMethodology)
            .HandleFailuresOrOk();
    }

    [HttpDelete("api/publication/{publicationId:guid}/external-methodology")]
    public async Task<ActionResult<Unit>> RemoveExternalMethodology(Guid publicationId)
    {
        return await publicationService.RemoveExternalMethodology(publicationId).HandleFailuresOrNoContent();
    }

    [HttpGet("api/publication/{publicationId:guid}/contact")]
    public async Task<ActionResult<ContactViewModel>> GetContact(Guid publicationId)
    {
        return await publicationService.GetContact(publicationId).HandleFailuresOrOk();
    }

    [HttpPut("api/publication/{publicationId:guid}/contact")]
    public async Task<ActionResult<ContactViewModel>> UpdateContact(
        Guid publicationId,
        ContactSaveRequest updatedContact
    )
    {
        return await publicationService.UpdateContact(publicationId, updatedContact).HandleFailuresOrOk();
    }

    [HttpGet("api/publication/{publicationId:guid}/releases")]
    public async Task<ActionResult<PaginatedListViewModel<ReleaseVersionSummaryViewModel>>> ListReleaseVersions(
        [Required] Guid publicationId,
        [FromQuery] ReleaseVersionsType versionsType,
        [FromQuery, Range(1, double.PositiveInfinity)] int page = 1,
        [FromQuery, Range(0, double.PositiveInfinity)] int pageSize = 5,
        [FromQuery] bool includePermissions = false
    )
    {
        return await publicationService
            .ListReleaseVersionsPaginated(
                publicationId: publicationId,
                versionsType: versionsType,
                page: page,
                pageSize: pageSize,
                includePermissions: includePermissions
            )
            .HandleFailuresOrOk();
    }

    [HttpPost("api/publications")]
    public async Task<ActionResult<PublicationCreateViewModel>> CreatePublication(PublicationCreateRequest publication)
    {
        return await publicationService.CreatePublication(publication).HandleFailuresOrOk();
    }

    [HttpPut("api/publications/{publicationId:guid}")]
    public async Task<ActionResult<PublicationViewModel>> UpdatePublication(
        Guid publicationId,
        PublicationSaveRequest updatedPublication
    )
    {
        return await publicationService.UpdatePublication(publicationId, updatedPublication).HandleFailuresOrOk();
    }

    [HttpGet("api/publications/{publicationId:guid}/release-series")]
    public async Task<ActionResult<List<ReleaseSeriesTableEntryViewModel>>> GetReleaseSeries(Guid publicationId)
    {
        return await publicationService.GetReleaseSeries(publicationId).HandleFailuresOrOk();
    }

    [HttpPost("api/publications/{publicationId:guid}/release-series")]
    public async Task<ActionResult<List<ReleaseSeriesTableEntryViewModel>>> AddReleaseSeriesLegacyLink(
        Guid publicationId,
        ReleaseSeriesLegacyLinkAddRequest request
    )
    {
        return await publicationService.AddReleaseSeriesLegacyLink(publicationId, request).HandleFailuresOrOk();
    }

    [HttpPut("api/publications/{publicationId:guid}/release-series")]
    public async Task<ActionResult<List<ReleaseSeriesTableEntryViewModel>>> UpdateReleaseSeries(
        Guid publicationId,
        List<ReleaseSeriesItemUpdateRequest> releaseSeries
    )
    {
        return await publicationService.UpdateReleaseSeries(publicationId, releaseSeries).HandleFailuresOrOk();
    }

    [HttpGet("api/publications/{publicationId:guid}/roles")]
    public async Task<ActionResult<List<UserPublicationRoleViewModel>>> GetRoles(Guid publicationId)
    {
        return await roleService.GetPublicationRolesForPublication(publicationId).HandleFailuresOrOk();
    }
}
