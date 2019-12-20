using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.Storage.Queue;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class NotificationsService : INotificationsService
    {
        private readonly ContentDbContext _context;
        private readonly string _storageConnectionString;
        private const string QueueName = "publication-queue";

        public NotificationsService(ContentDbContext context,
            IConfiguration config)
        {
            _context = context;
            _storageConnectionString = config.GetConnectionString("NotificationStorage");
        }

        public async Task NotifySubscribersAsync(Guid publicationId)
        {
            var publication = await GetPublicationAsync(publicationId);
            var queue = await QueueUtils.GetQueueReferenceAsync(_storageConnectionString, QueueName);
            await queue.AddMessageAsync(
                ToCloudQueueMessage(BuildPublicationNotificationMessage(publication)));
        }

        private Task<Publication> GetPublicationAsync(Guid publicationId)
        {
            return _context.Publications
                .AsNoTracking()
                .SingleAsync(publication => publication.Id == publicationId);
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

        private static CloudQueueMessage ToCloudQueueMessage(object value)
        {
            return new CloudQueueMessage(JsonConvert.SerializeObject(value));
        }
    }
}