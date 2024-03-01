#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;
public class PublicationReleaseSeriesViewService : IPublicationReleaseSeriesViewService
{
    private readonly ContentDbContext _context;

    public PublicationReleaseSeriesViewService(
        ContentDbContext context)
    {
        _context = context;
    }

    public async Task CreateForCreateLegacyRelease(Guid publicationId, Guid releaseId)
    {
        var publication = await GetPublication(publicationId);

        //publication.ReleaseSeriesView.Add(new() // @MarkFix remove
        //{
        //    ReleaseId = releaseId,
        //    IsLegacy = true,
        //    IsDraft = false,
        //    Order = publication.ReleaseSeriesView.Count + 1 // Add to the top of the list rather than presume the intended position
        //});

        _context.Update(publication);
    }

    public async Task DeleteForDeleteLegacyRelease(Guid legacyReleaseId)
    {
        var legacyRelease = await _context.LegacyReleases.FindAsync(legacyReleaseId)
            ?? throw new KeyNotFoundException($"No matching {nameof(LegacyRelease)} found with ID {legacyReleaseId}");

        var publication = await GetPublication(legacyRelease.PublicationId);

        var releaseSeries = legacyRelease.Publication.ReleaseSeriesView;

        //var releaseSeriesItem = releaseSeries.Find(ro => ro.ReleaseId == legacyRelease.Id) // @MarkFix remove
        //    ?? throw new KeyNotFoundException($"No matching ReleaseSeriesItem found for {nameof(LegacyRelease)} \"{legacyRelease.Description}\"");

        //releaseSeries.Remove(releaseSeriesItem);
        ResetOrders(releaseSeries);

        _context.Update(publication);
    }

    public async Task CreateForAmendRelease(Guid publicationId, Guid releaseAmendmentId)
    {
        var publication = await GetPublication(publicationId);

        var releaseAmendment = publication.Releases.Find(r => r.Id == releaseAmendmentId)
            ?? throw new KeyNotFoundException($"No matching amendment for {nameof(Release)} with ID {releaseAmendmentId} found");

        //var originalReleaseSeriesItem = publication.ReleaseSeriesView.Find(r => r.ReleaseId == releaseAmendment.PreviousVersionId) // @MarkFix remove?
        //    ?? throw new KeyNotFoundException($"No matching ReleaseSeriesItem for original {nameof(Release)} with ID {releaseAmendment.PreviousVersionId} found");

        //var newReleaseSeriesItem = new ReleaseSeriesItem // @MarkFix remove?
        //{
        //    ReleaseId = releaseAmendmentId,
        //    IsLegacy = false,
        //    IsDraft = true,
        //    IsAmendment = true,
        //    Order = originalReleaseSeriesItem.Order
        //};

        //publication.ReleaseSeriesView.Add(newReleaseSeriesItem);

        _context.Update(publication);
    }

    public async Task DeleteForDeleteRelease(Guid publicationId, Guid releaseId)
    {
        var publication = await GetPublication(publicationId);

        //var releaseSeriesItem = publication.ReleaseSeriesView.Find(ro => ro.ReleaseId == releaseId) // @MarkFix remove
        //    ?? throw new KeyNotFoundException($"No matching ReleaseSeriesItem found for {nameof(Release)} amendment with ID {releaseId}");

        //publication.ReleaseSeriesView.Remove(releaseSeriesItem);
        ResetOrders(publication.ReleaseSeriesView);

        _context.Update(publication);
    }

    public async Task CreateForCreateRelease(Guid publicationId, Guid releaseId)
    {
        var publication = await GetPublication(publicationId);

        //publication.ReleaseSeriesView.Add(new() // @MarkFix remove
        //{
        //    ReleaseId = releaseId,
        //    IsLegacy = false,
        //    IsDraft = true,
        //    Order = publication.ReleaseSeriesView.Count + 1
        //});

        _context.Update(publication);
    }

    public async Task UpdateForPublishRelease(Guid publicationId, Guid releaseId) // @MarkFix remove possibly? will probably still need to update the cache
    {
        var publication = await GetPublication(publicationId);

        //var releaseSeriesItem = publication.ReleaseSeriesView.Find(ro => ro.ReleaseId == releaseId) // @MarkFix
        //    ?? throw new KeyNotFoundException($"No matching ReleaseSeriesItem found for {nameof(Release)} with ID {releaseId}");

        //if (releaseSeriesItem.IsAmendment)
        //{
        //    var releaseAmendment = publication.Releases.Find(r => r.Id == releaseId)
        //        ?? throw new KeyNotFoundException($"No matching amendment for {nameof(Release)} with ID {releaseId} found");

        //    var originalReleaseSeriesItem = publication.ReleaseSeriesView.Find(r => r.ReleaseId == releaseAmendment.PreviousVersionId)
        //        ?? throw new KeyNotFoundException($"No matching ReleaseSeriesItem for original {nameof(Release)} with ID {releaseAmendment.PreviousVersionId} found");

        //    publication.ReleaseSeriesView.Remove(originalReleaseSeriesItem);

        //    releaseSeriesItem.IsAmendment = false;
        //    ResetOrders(publication.ReleaseSeriesView);
        //}

        //releaseSeriesItem.IsDraft = false;

        _context.Update(publication);
    }

    public async Task UpdateForUpdateReleaseSeries(
        Guid publicationId,
        List<ReleaseSeriesItemUpdateRequest> releaseSeriesUpdate)
    {
        var publication = await GetPublication(publicationId);

        foreach (var update in releaseSeriesUpdate)
        {
            //if (update.Order == 0) // @MarkFix
            //{
            //    throw new ApplicationException($"Updated order for release with ID {update.Id} must be greater than 0");
            //}

            //if (update.IsAmendment)
            //{
            //    var releaseAmendment = publication.Releases.Find(r => r.Id == update.Id)
            //        ?? throw new KeyNotFoundException($"No matching amendment for {nameof(Release)} with ID {update.Id} found");

            //    var originalReleaseSeriesItem = publication.ReleaseSeriesView.Find(ro => ro.ReleaseId == releaseAmendment.PreviousVersionId)
            //        ?? throw new KeyNotFoundException($"No matching ReleaseSeriesItem for original {nameof(Release)} with ID {releaseAmendment.PreviousVersionId} found");

            //    originalReleaseSeriesItem.Order = update.Order;
            //}

            //var releaseSeriesItem = publication.ReleaseSeriesView.Find(ro => ro.ReleaseId == update.Id)
            //    ?? throw new KeyNotFoundException($"No matching ReleaseSeriesItem found for {(update.IsLegacy ? nameof(LegacyRelease) : nameof(Release))} with ID {update.Id}");

            //releaseSeriesItem.Order = update.Order;

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
    private static void ResetOrders(List<ReleaseSeriesItem> releaseSeries)
    {
        //var orderedReleaseSeries = releaseSeries // @MarkFix
        //    .OrderBy(ro => ro.Order)
        //    .ToList();

        //orderedReleaseSeries.ForEach(ro => ro.Order = orderedReleaseSeries.IndexOf(ro) + 1);
    }
}
