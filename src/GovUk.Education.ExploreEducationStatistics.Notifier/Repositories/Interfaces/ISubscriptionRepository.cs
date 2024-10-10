using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Notifier.Types;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Repositories.Interfaces;

public interface ISubscriptionRepository
{
    public Task<List<string>> GetSubscriberEmails(Guid publicationId);

    public Task<Subscription> GetSubscriptionAndStatus(string publicationId, string email);
}
