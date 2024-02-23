#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;
public class PublicationReleaseOrderService : IPublicationReleaseOrderService
{
    private readonly ContentDbContext _context;

    public PublicationReleaseOrderService(
        ContentDbContext context)
    {
        _context = context;
    }

    public async Task CreateForCreateLegacyRelease(Guid publicationId, Guid releaseId)
    {
        var publication = await GetPublication(publicationId);

        publication.ReleaseOrders.Add(new()
        {
            ReleaseId = releaseId,
            IsLegacy = true,
            IsDraft = false,
            Order = publication.ReleaseOrders.Count + 1 // Add to the top of the list rather than presume the intended position
        });

        _context.Update(publication);
    }

    public async Task DeleteForDeleteLegacyRelease(Guid legacyReleaseId)
    {
        var legacyRelease = await _context.LegacyReleases.FindAsync(legacyReleaseId)
            ?? throw new KeyNotFoundException($"No matching {nameof(LegacyRelease)} found with ID {legacyReleaseId}");

        var publication = await GetPublication(legacyRelease.PublicationId);

        var releaseOrders = legacyRelease.Publication.ReleaseOrders;

        var releaseOrder = releaseOrders.Find(ro => ro.ReleaseId == legacyRelease.Id)
            ?? throw new KeyNotFoundException($"No matching ReleaseOrder found for {nameof(LegacyRelease)} \"{legacyRelease.Description}\"");

        releaseOrders.Remove(releaseOrder);
        ResetOrders(releaseOrders);

        _context.Update(publication);
    }

    public async Task CreateForAmendRelease(Guid publicationId, Guid releaseAmendmentId)
    {
        var publication = await GetPublication(publicationId);

        var releaseAmendment = publication.Releases.Find(r => r.Id == releaseAmendmentId)
            ?? throw new KeyNotFoundException($"No matching amendment for {nameof(Release)} with ID {releaseAmendmentId} found");

        var originalReleaseOrder = publication.ReleaseOrders.Find(r => r.ReleaseId == releaseAmendment.PreviousVersionId)
            ?? throw new KeyNotFoundException($"No matching ReleaseOrder for original {nameof(Release)} with ID {releaseAmendment.PreviousVersionId} found");

        var newReleaseOrder = new ReleaseOrder
        {
            ReleaseId = releaseAmendmentId,
            IsLegacy = false,
            IsDraft = true,
            IsAmendment = true,
            Order = originalReleaseOrder.Order
        };

        publication.ReleaseOrders.Add(newReleaseOrder);

        _context.Update(publication);
    }

    public async Task DeleteForDeleteRelease(Guid publicationId, Guid releaseId)
    {
        var publication = await GetPublication(publicationId);

        var releaseOrder = publication.ReleaseOrders.Find(ro => ro.ReleaseId == releaseId)
            ?? throw new KeyNotFoundException($"No matching ReleaseOrder found for {nameof(Release)} amendment with ID {releaseId}");

        publication.ReleaseOrders.Remove(releaseOrder);
        ResetOrders(publication.ReleaseOrders);

        _context.Update(publication);
    }

    public async Task CreateForCreateRelease(Guid publicationId, Guid releaseId)
    {
        var publication = await GetPublication(publicationId);

        publication.ReleaseOrders.Add(new()
        {
            ReleaseId = releaseId,
            IsLegacy = false,
            IsDraft = true,
            Order = publication.ReleaseOrders.Count + 1
        });

        _context.Update(publication);
    }

    public async Task UpdateForPublishRelease(Guid publicationId, Guid releaseId)
    {
        var publication = await GetPublication(publicationId);

        var releaseOrder = publication.ReleaseOrders.Find(ro => ro.ReleaseId == releaseId)
            ?? throw new KeyNotFoundException($"No matching ReleaseOrder found for {nameof(Release)} with ID {releaseId}");

        if (releaseOrder.IsAmendment)
        {
            var releaseAmendment = publication.Releases.Find(r => r.Id == releaseId)
                ?? throw new KeyNotFoundException($"No matching amendment for {nameof(Release)} with ID {releaseId} found");

            var originalReleaseOrder = publication.ReleaseOrders.Find(r => r.ReleaseId == releaseAmendment.PreviousVersionId)
                ?? throw new KeyNotFoundException($"No matching ReleaseOrder for original {nameof(Release)} with ID {releaseAmendment.PreviousVersionId} found");

            publication.ReleaseOrders.Remove(originalReleaseOrder);

            releaseOrder.IsAmendment = false;
            ResetOrders(publication.ReleaseOrders);
        }

        releaseOrder.IsDraft = false;

        _context.Update(publication);
    }

    public async Task UpdateForUpdateCombinedReleaseOrder(
        Guid publicationId,
        List<CombinedReleaseUpdateOrderViewModel> releaseOrderUpdates)
    {
        var publication = await GetPublication(publicationId);

        foreach (var update in releaseOrderUpdates)
        {
            if (update.Order == 0)
            {
                throw new ApplicationException($"Updated order for release with ID {update.Id} must be greater than 0");
            }

            if (update.IsAmendment)
            {
                var releaseAmendment = publication.Releases.Find(r => r.Id == update.Id)
                    ?? throw new KeyNotFoundException($"No matching amendment for {nameof(Release)} with ID {update.Id} found");

                var originalReleaseOrder = publication.ReleaseOrders.Find(ro => ro.ReleaseId == releaseAmendment.PreviousVersionId)
                    ?? throw new KeyNotFoundException($"No matching ReleaseOrder for original {nameof(Release)} with ID {releaseAmendment.PreviousVersionId} found");

                originalReleaseOrder.Order = update.Order;
            }

            var releaseOrder = publication.ReleaseOrders.Find(ro => ro.ReleaseId == update.Id)
                ?? throw new KeyNotFoundException($"No matching ReleaseOrder found for {(update.IsLegacy ? nameof(LegacyRelease) : nameof(Release))} with ID {update.Id}");

            releaseOrder.Order = update.Order;

            _context.Update(publication);
        }
    }

    private async Task<Publication> GetPublication(
        Guid publicationId)
    {
        return await _context.Publications.FindAsync(publicationId)
            ?? throw new KeyNotFoundException($"No matching {nameof(Publication)} found with ID {publicationId}");
    }

    /// <summary>
    /// Reset the order value for each item based on its current position in the ordered list.
    /// </summary>
    private static void ResetOrders(List<ReleaseOrder> releaseOrders)
    {
        var orderedReleaseOrders = releaseOrders
            .OrderBy(ro => ro.Order)
            .ToList();

        orderedReleaseOrders.ForEach(ro => ro.Order = orderedReleaseOrders.IndexOf(ro) + 1);
    }
}
