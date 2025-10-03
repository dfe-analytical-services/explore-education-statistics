using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common;

namespace GovUk.Education.ExploreEducationStatistics.Content.Requests;

public record PageFeedbackCreateRequest
{
    public string Url { get; set; } = string.Empty;

    public string? UserAgent { get; set; }

    public PageFeedbackResponse Response { get; set; }

    public string? Context { get; set; }

    public string? Issue { get; set; }

    public string? Intent { get; set; }

    public class Validator : AbstractValidator<PageFeedbackCreateRequest>
    {
        public Validator()
        {
            RuleFor(request => request.Url).NotEmpty();

            RuleFor(request => request.UserAgent).MaximumLength(250);

            RuleFor(request => request.Response).IsInEnum();

            RuleFor(request => request.Context).MaximumLength(2000);

            RuleFor(request => request.Issue).MaximumLength(2000);

            RuleFor(request => request.Intent).MaximumLength(2000);
        }
    }
}
