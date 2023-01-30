#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
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

        public async Task NotifySubscribersIfApplicable(params Guid[] releaseIds)
        {
            var releasesToNotify = await _context.Releases
                .Where(r => releaseIds.Contains(r.Id) && r.NotifySubscribers)
                .ToListAsync();

            var messages = await releasesToNotify
                .ToAsyncEnumerable()
                .SelectAwait(async release => await BuildPublicationNotificationMessage(release))
                .ToListAsync();

            if (messages.Count > 0)
            {
                await _storageQueueService.AddMessages(ReleaseNotificationQueue, messages);
                releasesToNotify
                    .ForEach(release =>
                    {
                        _context.Releases.Update(release);
                        release.NotifiedOn = DateTime.UtcNow;
                    });
                await _context.SaveChangesAsync();
            }
        }

        private async Task<ReleaseNotificationMessage> BuildPublicationNotificationMessage(Release release)
        {
            await _context.Entry(release)
                .Reference(r => r.Publication)
                .LoadAsync();

            var latestUpdateNoteReason = "No update note provided.";
            // NOTE: Only amendment email template displays an update note.
            if (release.Version > 0)
            {
                await _context.Entry(release)
                    .Collection(r => r.Updates)
                    .LoadAsync();
                var latestUpdateNote = release.Updates
                    .OrderBy(u => u.Created)
                    .Last();
                latestUpdateNoteReason = latestUpdateNote.Reason;
            }


            return new ReleaseNotificationMessage
            {
                PublicationId = release.Publication.Id,
                PublicationName = release.Publication.Title,
                PublicationSlug = release.Publication.Slug,
                ReleaseName = release.Title,
                ReleaseSlug = release.Slug,
                Amendment = release.Version > 0,
                UpdateNote = latestUpdateNoteReason,
            };
        }
    }
}
