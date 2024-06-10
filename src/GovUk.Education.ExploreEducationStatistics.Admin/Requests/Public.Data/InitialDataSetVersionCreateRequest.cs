#nullable enable
using System;
using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests.Public.Data;

public record InitialDataSetVersionCreateRequest
{
    public required Guid ReleaseFileId { get; init; }

    public class Validator : AbstractValidator<InitialDataSetVersionCreateRequest>
    {
        public Validator()
        {
            RuleFor(request => request.ReleaseFileId)
                .NotEmpty();
        }
    }
}
