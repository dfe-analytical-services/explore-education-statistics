#nullable enable
using System;
using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests.Public.Data;

public record PreviewTokenCreateRequest
{
    public required Guid DataSetVersionId { get; init; }

    public required string Label { get; init; }

    public class Validator : AbstractValidator<PreviewTokenCreateRequest>
    {
        public Validator()
        {
            RuleFor(request => request.DataSetVersionId)
                .NotEmpty();

            RuleFor(request => request.Label)
                .NotEmpty()
                .MaximumLength(100);
        }
    }
}
