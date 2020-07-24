using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.QueueUtils;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class NotificationsService : INotificationsService
    {
        private readonly ContentDbContext _context;
        private readonly string _storageConnectionString;
        private const string QueueName = "publication-queue";

        public NotificationsService(ContentDbContext context, IConfiguration config)
        {
            _context = context;
            _storageConnectionString = config.GetValue<string>("NotificationStorage");
        }

        public async Task NotifySubscribers(params Guid[] releaseIds)
        {
            var publications = GetPublications(releaseIds);
            var queue = await GetQueueReferenceAsync(_storageConnectionString, QueueName);
            var messages = publications.Select(BuildPublicationNotificationMessage);

            foreach (var message in messages)
            {
                await queue.AddMessageAsync(ToCloudQueueMessage(message));
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