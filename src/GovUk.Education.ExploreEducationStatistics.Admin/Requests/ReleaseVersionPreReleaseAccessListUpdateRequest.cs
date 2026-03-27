#nullable enable
using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record ReleaseVersionPreReleaseAccessListUpdateRequest
{
    public required string? PreReleaseAccessList { get; init; }

    public class Validator : AbstractValidator<ReleaseVersionPreReleaseAccessListUpdateRequest>
    {
        public Validator()
        {
            RuleFor(r => r.PreReleaseAccessList).NotEmpty();
        }
    }
}
