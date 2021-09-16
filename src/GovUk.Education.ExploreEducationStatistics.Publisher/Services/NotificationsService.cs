using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
            var releasesToNotify = _context.Releases
                .Include(r => r.ReleaseStatuses)
                .Where(r => releaseIds.Contains(r.Id))
                .ToList()
                .Where(r => r.NotifySubscribers)
                .ToList();
            var releaseIdsToNotify = releasesToNotify
                .Select(r => r.Id)
                .ToArray();
            var publications = GetPublications(releaseIdsToNotify);
            var messages = publications
                .Select(BuildPublicationNotificationMessage)
                .ToList();
            if (messages.Count > 0)
            {
                await _storageQueueService.AddMessages(PublicationQueue, messages);
                releasesToNotify
                    .ForEach(release =>
                    {
                        var status = release.ReleaseStatuses
                            .OrderBy(rs => rs.Created)
                            .Last();
                        _context.Update(status);
                        status.NotifiedOn = DateTime.UtcNow;
                    });
                await _context.SaveChangesAsync();
            }
        }

        private IEnumerable<Publication> GetPublications(IEnumerable<Guid> releaseIds)
        {
            return _context.Releases
                .AsNoTracking()
                .Where(release => releaseIds.Contains(release.Id))
                .Select(release => release.Publication)
                .Distinct();
        }

        private static PublicationNotificationMessage BuildPublicationNotificationMessage(Publication publication)
        {
            return new PublicationNotificationMessage
            {
                Name = publication.Title,
                PublicationId = publication.Id,
                Slug = publication.Slug
            };
        }
    }
}
