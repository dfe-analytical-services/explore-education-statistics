namespace GovUk.Education.ExploreEducationStatistics.Notifier.Requests;

public record NewPendingSubscriptionRequest
{
    public string? Id { get; set; }
    public string? Email { get; set; }
    public string? Slug { get; set; }
    public string? Title { get; set; }
}
