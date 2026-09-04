#nullable enable
using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public class LocationMappingUpdatesRequest
{
    public Guid OriginalDataFileId { get; init; }
    public Guid ReplacementDataFileId { get; init; }

    public List<MappingUpdateRequest> Updates { get; init; } = [];

    public class Validator : AbstractValidator<LocationMappingUpdatesRequest>
    {
        public Validator()
        {
            RuleFor(x => x.OriginalDataFileId).NotEmpty();

            RuleFor(x => x.ReplacementDataFileId).NotEmpty();

            RuleForEach(x => x.Updates).SetValidator(new MappingUpdateRequest.Validator());

            RuleFor(x => x.Updates)
                .Must(MappingUpdateRequest.HaveUniqueOriginalIds)
                .WithMessage("Each OriginalId must be unique.")
                .Must(MappingUpdateRequest.HaveUniqueReplacementIds)
                .WithMessage("Each NewReplacementId must be unique (if provided).");
        }
    }
}
