using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Content.Requests;

public record ReleasePublishingFeedbackUpdateRequest(
    [JsonConverter(typeof(StringEnumConverter))]
    ReleasePublishingFeedbackResponse Response,
    string EmailToken,
    string? AdditionalFeedback = null)
{
    public class Validator : AbstractValidator<ReleasePublishingFeedbackUpdateRequest>
    {
        public Validator()
        {
            RuleFor(request => request.EmailToken)
                .NotEmpty()
                .MaximumLength(55);
            
            RuleFor(request => request.Response)
                .IsInEnum();

            RuleFor(request => request.AdditionalFeedback)
                .MaximumLength(2000);
        }
    }
}
