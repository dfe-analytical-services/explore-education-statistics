#nullable enable
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Requests;

public record PermalinkCreateRequest
{
    public Guid? ReleaseVersionId { get; init; }

    public TableBuilderConfiguration Configuration { get; init; } = new();

    public FullTableQueryRequest Query { get; init; } = new();

    public class Validator : AbstractValidator<PermalinkCreateRequest>
    {
        public Validator()
        {
            RuleFor(request => request.Query).SetValidator(new FullTableQueryRequest.Validator());
        }
    }
}
