#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

/// <summary>
/// Update a publication's ReleaseOrder property whenever an action is performed against a release.
/// </summary>
/// <remarks>
/// Only updates the tracked <see cref="Publication"/> entity, <c>context.SaveChangesAsync()</c> will need to be called after in order to persist changes.
/// </remarks>
/// <exception cref="ApplicationException"></exception>
/// <exception cref="KeyNotFoundException"></exception>
/// <exception cref="ArgumentNullException"></exception>
public interface IPublicationReleaseOrderService
{
    Task CreateForCreateLegacyRelease(
        Guid publicationId,
        Guid releaseId);

    Task DeleteForDeleteLegacyRelease(
        Guid legacyReleaseId);

    Task CreateForCreateRelease(
        Guid publicationId,
        Guid releaseId);

    Task CreateForAmendRelease(
        Guid publicationId,
        Guid releaseAmendmentId);

    Task DeleteForDeleteRelease(
        Guid publicationId,
        Guid releaseId);

    Task UpdateForUpdateCombinedReleaseOrder(
        Guid publicationId,
        List<CombinedReleaseUpdateOrderViewModel> releaseOrderUpdates);

    Task UpdateForPublishRelease(
        Guid publicationId,
        Guid releaseId);
}
