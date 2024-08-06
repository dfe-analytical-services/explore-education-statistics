#nullable enable
using System;
using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests.Public.Data;

public record DataSetVersionUpdateRequest
{
    public required Guid DataSetVersionId { get; init; }

    public string? Notes { get; init; }

    public class Validator : AbstractValidator<DataSetVersionUpdateRequest>
    {
        public Validator()
        {
            RuleFor(request => request.DataSetVersionId)
                .NotEmpty();
        }
    }
}
