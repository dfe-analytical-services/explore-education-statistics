using GovUk.Education.ExploreEducationStatistics.Notifier.Model;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Types;

public record Subscription()
{
    public SubscriptionEntity? Entity { get; set; }

    public SubscriptionStatus Status { get; set; }
}
