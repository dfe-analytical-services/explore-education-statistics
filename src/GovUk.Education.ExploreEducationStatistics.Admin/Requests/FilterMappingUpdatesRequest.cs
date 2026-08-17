#nullable enable
using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public class FilterMappingUpdatesRequest
{
    public Guid OriginalDataFileId { get; init; }
    public Guid ReplacementDataFileId { get; init; }

    public List<MappingUpdateRequest> FilterUpdates { get; init; } = [];
    public List<MappingUpdateRequest> FilterGroupUpdates { get; init; } = [];
    public List<MappingUpdateRequest> FilterItemUpdates { get; init; } = [];

    public class Validator : AbstractValidator<FilterMappingUpdatesRequest>
    {
        public Validator()
        {
            RuleFor(x => x.OriginalDataFileId).NotEmpty();

            RuleFor(x => x.ReplacementDataFileId).NotEmpty();

            RuleForEach(x => x.FilterUpdates).SetValidator(new MappingUpdateRequest.Validator());
            RuleFor(x => x.FilterUpdates)
                .Must(MappingUpdateRequest.HaveUniqueOriginalIds)
                .WithMessage("Each OriginalId must be unique.")
                .Must(MappingUpdateRequest.HaveUniqueReplacementIds)
                .WithMessage("Each NewReplacementId must be unique (if provided).");

            RuleForEach(x => x.FilterGroupUpdates).SetValidator(new MappingUpdateRequest.Validator());
            RuleFor(x => x.FilterGroupUpdates)
                .Must(MappingUpdateRequest.HaveUniqueOriginalIds)
                .WithMessage("Each OriginalId must be unique.")
                .Must(MappingUpdateRequest.HaveUniqueReplacementIds)
                .WithMessage("Each NewReplacementId must be unique (if provided).");

            RuleForEach(x => x.FilterItemUpdates).SetValidator(new MappingUpdateRequest.Validator());
            RuleFor(x => x.FilterItemUpdates)
                .Must(MappingUpdateRequest.HaveUniqueOriginalIds)
                .WithMessage("Each OriginalId must be unique.")
                .Must(MappingUpdateRequest.HaveUniqueReplacementIds)
                .WithMessage("Each NewReplacementId must be unique (if provided).");
        }
    }
}
