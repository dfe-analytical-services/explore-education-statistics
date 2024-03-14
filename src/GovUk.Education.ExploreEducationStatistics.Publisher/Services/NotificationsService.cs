#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Notifier.Model.NotifierQueues;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class NotificationsService : INotificationsService
    {
        private readonly ContentDbContext _context;
        private readonly IStorageQueueService _storageQueueService;

        public NotificationsService(ContentDbContext context,
            IStorageQueueService storageQueueService)
        {
            _context = context;
            _storageQueueService = storageQueueService;
        }

        public async Task NotifySubscribersIfApplicable(params Guid[] releaseVersionIds)
        {
            var releaseVersionsToNotify = await _context.ReleaseVersions
                .Where(rv => releaseVersionIds.Contains(rv.Id) && rv.NotifySubscribers)
                .ToListAsync();

            var messages = await releaseVersionsToNotify
                .ToAsyncEnumerable()
                .SelectAwait(async releaseVersion => await BuildPublicationNotificationMessage(releaseVersion))
                .ToListAsync();

            if (messages.Count > 0)
            {
                await _storageQueueService.AddMessages(ReleaseNotificationQueue, messages);
                releaseVersionsToNotify
                    .ForEach(releaseVersion =>
                    {
                        _context.ReleaseVersions.Update(releaseVersion);
                        releaseVersion.NotifiedOn = DateTime.UtcNow;
                    });
                await _context.SaveChangesAsync();
            }
        }

        private async Task<ReleaseNotificationMessage> BuildPublicationNotificationMessage(
            ReleaseVersion releaseVersion)
        {
            await _context.Entry(releaseVersion)
                .Reference(rv => rv.Publication)
                .LoadAsync();

            var latestUpdateNoteReason = "No update note provided.";
            // NOTE: Only amendment email template displays an update note.
            if (releaseVersion.Version > 0)
            {
                await _context.Entry(releaseVersion)
                    .Collection(rv => rv.Updates)
                    .LoadAsync();
                var latestUpdateNote = releaseVersion.Updates
                    .OrderBy(u => u.Created)
                    .Last();
                latestUpdateNoteReason = latestUpdateNote.Reason;
            }

            var supersededPublications = await _context.Publications
                .Where(p => p.SupersededById == releaseVersion.PublicationId)
                .Select(p => new { p.Id, p.Title })
                .ToListAsync();

            return new ReleaseNotificationMessage
            {
                PublicationId = releaseVersion.Publication.Id,
                PublicationName = releaseVersion.Publication.Title,
                PublicationSlug = releaseVersion.Publication.Slug,
                ReleaseName = releaseVersion.Title,
                ReleaseSlug = releaseVersion.Slug,
                Amendment = releaseVersion.Version > 0,
                UpdateNote = latestUpdateNoteReason,
                SupersededPublications = supersededPublications.Select(p =>
                    new IdTitleViewModel(p.Id, p.Title)).ToList(),
            };
        }
    }
}
