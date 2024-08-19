namespace GovUk.Education.ExploreEducationStatistics.Notifier.Types;

public record Subscription()
{
    public SubscriptionEntity Subscriber { get; set; }
    
    public SubscriptionStatus Status { get; set; }
}
