#nullable enable
using FluentValidation;
using LinqToDB.Internal.Common;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public class LocationMappingUpdatesRequest
{
    public Guid OriginalDataSetId { get; init; }
    public Guid ReplacementDataSetId { get; init; }

    public List<LocationMappingUpdateRequest> Updates { get; init; } = [];

    public class Validator : AbstractValidator<LocationMappingUpdatesRequest>
    {
        public Validator()
        {
            RuleFor(x => x.OriginalDataSetId).NotEmpty();

            RuleFor(x => x.ReplacementDataSetId).NotEmpty();

            RuleForEach(x => x.Updates).SetValidator(new LocationMappingUpdateRequest.Validator());

            RuleFor(x => x.Updates)
                .Must(HaveUniqueOriginalLocationId)
                .WithMessage($"Each {nameof(LocationMappingUpdateRequest.OriginalLocationId)} must be unique.")
                .Must(HaveUniqueReplacementIds)
                .WithMessage(
                    $"Each {nameof(LocationMappingUpdateRequest.NewReplacementLocationId)} must be unique (if provided)."
                );
        }

        private bool HaveUniqueOriginalLocationId(List<LocationMappingUpdateRequest> updates)
        {
            if (updates.IsNullOrEmpty())
            {
                return true;
            }

            return updates.Select(u => u.OriginalLocationId).Distinct().Count() == updates.Count;
        }

        private bool HaveUniqueReplacementIds(List<LocationMappingUpdateRequest> updates)
        {
            if (updates.IsNullOrEmpty())
            {
                return true;
            }

            var nonNullReplacements = updates
                .Where(u => u.NewReplacementLocationId != null)
                .Select(u => u.NewReplacementLocationId)
                .ToList();

            return nonNullReplacements.Distinct().Count() == nonNullReplacements.Count;
        }
    }
}

public record LocationMappingUpdateRequest
{
    public Guid OriginalLocationId { get; init; }
    public Guid? NewReplacementLocationId { get; init; }

    public class Validator : AbstractValidator<LocationMappingUpdateRequest>
    {
        public Validator()
        {
            RuleFor(x => x.OriginalLocationId).NotEmpty().WithMessage($"{nameof(OriginalLocationId)} cannot be empty.");
        }
    }
}
