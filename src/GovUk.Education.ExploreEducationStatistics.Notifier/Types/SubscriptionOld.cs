namespace GovUk.Education.ExploreEducationStatistics.Notifier.Types;

public record Subscription()
{
    public SubscriptionEntity? Entity { get; set; }

    public SubscriptionStatus Status { get; set; }
}

public record SubscriptionOld() // @MarkFix remove
{
    public SubscriptionEntityOld Subscriber { get; set; }
    
    public SubscriptionStatus Status { get; set; }
}
