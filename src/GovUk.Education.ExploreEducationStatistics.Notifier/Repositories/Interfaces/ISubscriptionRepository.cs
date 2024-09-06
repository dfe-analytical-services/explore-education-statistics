using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Repositories.Interfaces;

public interface ISubscriptionRepository
{
    public Task<List<string>> GetSubscriberEmails(Guid publicationId);
}
