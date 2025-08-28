#nullable enable
using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record DataGuidanceUpdateRequest
{
    public string Content { get; init; } = string.Empty;

    public List<DataGuidanceDataSetUpdateRequest> DataSets { get; init; } = [];

    public class Validator : AbstractValidator<DataGuidanceUpdateRequest>
    {
        public Validator()
        {
            RuleFor(request => request.Content)
                .NotEmpty();

            RuleFor(request => request.DataSets)
                .NotEmpty();
        }
    }
}

public record DataGuidanceDataSetUpdateRequest
{
    public Guid FileId { get; init; }

    public string Content { get; init; } = string.Empty;

    public class Validator : AbstractValidator<DataGuidanceDataSetUpdateRequest>
    {
        public Validator()
        {
            RuleFor(request => request.FileId)
                .NotEmpty();

            RuleFor(request => request.Content)
                .NotEmpty()
                .MaximumLength(250);
        }
    }
}
