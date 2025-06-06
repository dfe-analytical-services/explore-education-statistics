using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services;

public class NotificationsService(
    ContentDbContext context,
    INotifierClient notifierClient)
    : INotificationsService
{
    public async Task NotifySubscribersIfApplicable(IReadOnlyList<Guid> releaseVersionIds)
    {
        var releasesVersions = await context.ReleaseVersions
            .Include(rv => rv.Release)
            .ThenInclude(r => r.Publication)
            .Where(rv => releaseVersionIds.Contains(rv.Id) && rv.NotifySubscribers)
            .ToListAsync();

        var messages = await releasesVersions
            .ToAsyncEnumerable()
            .SelectAwait(async releaseVersion => await BuildPublicationNotificationMessage(releaseVersion))
            .ToListAsync();

        if (messages.Count > 0)
        {
            await notifierClient.NotifyPublicationSubscribers(messages);
            releasesVersions.ForEach(releaseVersion => releaseVersion.NotifiedOn = DateTime.UtcNow);
            await context.SaveChangesAsync();
        }
    }

    private async Task<ReleaseNotificationMessage> BuildPublicationNotificationMessage(
        ReleaseVersion releaseVersion)
    {
        var latestUpdateNoteReason = "No update note provided.";
        // NOTE: Only amendment email template displays an update note.
        if (releaseVersion.Version > 0)
        {
            await context.Entry(releaseVersion)
                .Collection(rv => rv.Updates)
                .LoadAsync();
            var latestUpdateNote = releaseVersion.Updates
                .OrderBy(u => u.Created)
                .Last();
            latestUpdateNoteReason = latestUpdateNote.Reason;
        }

        var release = releaseVersion.Release;

        return new ReleaseNotificationMessage
        {
            PublicationId = release.Publication.Id,
            PublicationName = release.Publication.Title,
            PublicationSlug = release.Publication.Slug,
            ReleaseName = release.Title,
            ReleaseSlug = release.Slug,
            Amendment = releaseVersion.Version > 0,
            UpdateNote = latestUpdateNoteReason,
        };
    }
}
