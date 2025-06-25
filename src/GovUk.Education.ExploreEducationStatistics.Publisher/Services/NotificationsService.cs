using System;
using System.Collections.Generic;
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

        public async Task SendReleasePublishingFeedbackEmails(IReadOnlyList<Guid> releaseVersionIds)
        {
            var releasesVersions = await context
                .ReleaseVersions
                .Include(rv => rv.Release)
                .ThenInclude(r => r.Publication)
                .Where(rv => releaseVersionIds.Contains(rv.Id) && rv.NotifySubscribers)
                .ToListAsync();

            var feedbackEntriesAndEmails = await releasesVersions
                .ToAsyncEnumerable()
                .SelectManyAwait(async releaseVersion =>
                {
                    var publicationRoles = await context
                        .UserPublicationRoles
                        .Include(upr => upr.User)
                        .Where(upr => upr.PublicationId == releaseVersion.Release.PublicationId)
                        .ToListAsync();

                    return publicationRoles
                        .ToAsyncEnumerable()
                        .Select(upr =>
                        {
                            var feedback = new ReleasePublishingFeedback
                            {
                                ReleaseVersion = releaseVersion,
                                ReleaseVersionId = releaseVersion.Id,
                                UserPublicationRole = upr.Role,
                                EmailToken = Guid.NewGuid().ToString()
                            };

                            return (feedback, email: upr.User.Email);
                        });
                })
                .ToListAsync();

            var feedbackEntries = feedbackEntriesAndEmails
                .Select(feedbackEntry => feedbackEntry.feedback);
            
            await context.ReleasePublishingFeedback.AddRangeAsync(feedbackEntries);
            await context.SaveChangesAsync();
            
            var messages = feedbackEntriesAndEmails
                .Select(feedbackAndEmail => new ReleasePublishingFeedbackMessage(
                    ReleasePublishingFeedbackId: feedbackAndEmail.feedback.Id,
                    EmailAddress: feedbackAndEmail.email))
                .ToList();

            if (messages.Count > 0)
            {
                await notifierClient.NotifyReleasePublishingFeedbackUsers(messages);
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
}
