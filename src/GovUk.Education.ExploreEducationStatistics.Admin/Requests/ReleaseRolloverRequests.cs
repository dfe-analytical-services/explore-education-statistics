#nullable enable
using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public enum RolloverMode
{
    WithData,
    StructureOnly,
}

public record ReleaseRolloverCreateRequest : ReleaseCreateRequest
{
    public required Guid RolloverSourceReleaseId { get; init; }

    public required RolloverMode Mode { get; init; }

    public new class Validator : AbstractValidator<ReleaseRolloverCreateRequest>
    {
        public Validator()
        {
            Include(new ReleaseCreateRequest.Validator());

            RuleFor(request => request.RolloverSourceReleaseId).NotEmpty();

            RuleFor(request => request.Mode).IsInEnum();
        }
    }
}
