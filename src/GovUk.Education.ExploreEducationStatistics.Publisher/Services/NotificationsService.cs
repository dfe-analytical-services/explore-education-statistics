using System;
using System.Collections.Generic;
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

        public async Task NotifySubscribers(params Guid[] releaseIds)
        {
            var publications = GetPublications(releaseIds);
            var messages = publications.Select(BuildPublicationNotificationMessage);
            await _storageQueueService.AddMessagesAsync(PublicationQueue, messages);
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