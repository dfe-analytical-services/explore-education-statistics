using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class NotificationsService(
        ContentDbContext context,
        INotifierClient notifierClient)
        : INotificationsService
    {
        public async Task NotifySubscribersIfApplicable(params Guid[] releaseVersionIds)
        {
            var releaseVersionsToNotify = await context.ReleaseVersions
                .Where(rv => releaseVersionIds.Contains(rv.Id) && rv.NotifySubscribers)
                .ToListAsync();

            var messages = await releaseVersionsToNotify
                .ToAsyncEnumerable()
                .SelectAwait(async releaseVersion => await BuildPublicationNotificationMessage(releaseVersion))
                .ToListAsync();

            if (messages.Count > 0)
            {
                await notifierClient.NotifyPublicationSubscribers(messages);
                releaseVersionsToNotify
                    .ForEach(releaseVersion =>
                    {
                        context.ReleaseVersions.Update(releaseVersion);
                        releaseVersion.NotifiedOn = DateTime.UtcNow;
                    });
                await context.SaveChangesAsync();
            }
        }

        private async Task<ReleaseNotificationMessage> BuildPublicationNotificationMessage(
            ReleaseVersion releaseVersion)
        {
            await context.Entry(releaseVersion)
                .Reference(rv => rv.Publication)
                .LoadAsync();

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

            return new ReleaseNotificationMessage
            {
                PublicationId = releaseVersion.Publication.Id,
                PublicationName = releaseVersion.Publication.Title,
                PublicationSlug = releaseVersion.Publication.Slug,
                ReleaseName = releaseVersion.Title,
                ReleaseSlug = releaseVersion.Slug,
                Amendment = releaseVersion.Version > 0,
                UpdateNote = latestUpdateNoteReason,
            };
        }
    }
}
