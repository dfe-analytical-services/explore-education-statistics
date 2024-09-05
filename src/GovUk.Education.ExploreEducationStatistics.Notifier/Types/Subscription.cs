namespace GovUk.Education.ExploreEducationStatistics.Notifier.Types;

public record Subscription()
{
    public SubscriptionEntityOld Subscriber { get; set; }
    
    public SubscriptionStatus Status { get; set; }
}
