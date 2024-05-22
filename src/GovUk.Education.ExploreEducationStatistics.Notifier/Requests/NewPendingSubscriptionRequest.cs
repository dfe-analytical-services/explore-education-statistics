using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Requests;

public record NewPendingSubscriptionRequest
{
    public required string Email { get; init; }
    public required string Id { get; init; }
    public required string Slug { get; init; }
    public required string Title { get; init; }
    
    public class Validator : AbstractValidator<NewPendingSubscriptionRequest>
    {
        public Validator()
        {
            RuleFor(request => request.Email)
                .NotEmpty();
            RuleFor(request => request.Id)
                .NotEmpty();
            RuleFor(request => request.Slug)
                .NotEmpty();
            RuleFor(request => request.Title)
                .NotEmpty();
        }
    }
}
