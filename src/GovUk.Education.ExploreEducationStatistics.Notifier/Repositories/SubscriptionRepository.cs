using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
using GovUk.Education.ExploreEducationStatistics.Notifier.Repositories.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Repositories;

internal class SubscriptionRepository(
    INotifierTableStorageService notifierTableStorage) : ISubscriptionRepository
{
    public async Task<List<string>> GetSubscriberEmails(Guid publicationId)
    {
            var results = await notifierTableStorage
                .QueryEntities<SubscriptionEntity>(
                    tableName: NotifierTableStorage.PublicationSubscriptionsTable,
                    filter: sub => sub.PartitionKey == publicationId.ToString(),
                    select: [nameof(SubscriptionEntity.RowKey)]); // email address

            // @MarkFix especially check this - we used to fetch in segments (I think)
            return await results
                .Select(subscriber => subscriber.RowKey)
                .ToListAsync();
    }
}
