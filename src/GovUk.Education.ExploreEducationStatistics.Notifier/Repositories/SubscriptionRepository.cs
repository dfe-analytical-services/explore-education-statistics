using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
using GovUk.Education.ExploreEducationStatistics.Notifier.Repositories.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Notifier.Types;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Repositories;

public class SubscriptionRepository(
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

    public async Task<Subscription> GetSubscriptionAndStatus(string publicationId, string email)
    {
        var pendingSub = await notifierTableStorage.GetEntityIfExists<SubscriptionEntity>(
            tableName: NotifierTableStorage.PublicationPendingSubscriptionsTable,
            partitionKey: publicationId,
            rowKey: email);
        if (pendingSub is not null)
        {
            return new Subscription
            {
                Entity = pendingSub,
                Status = SubscriptionStatus.SubscriptionPending,
            };
        }
        
        var activeSub = await notifierTableStorage.GetEntityIfExists<SubscriptionEntity>(
            tableName: NotifierTableStorage.PublicationSubscriptionsTable,
            partitionKey: publicationId,
            rowKey: email);
        if (activeSub is not null)
        {
            return new Subscription
            {
                Entity = activeSub,
                Status = SubscriptionStatus.Subscribed,
            };
        }

        return new Subscription { Status = SubscriptionStatus.NotSubscribed };
    }
}
