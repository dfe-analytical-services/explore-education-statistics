namespace GovUk.Education.ExploreEducationStatistics.Notifier.Types;

public record SubscriptionStateDto()
{
    public string Slug { get; set; }

    public string Title { get; set; }

    public SubscriptionStatus Status { get; set; }
}
