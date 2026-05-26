#nullable enable
using FluentValidation;
using LinqToDB.Internal.Common;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public class IndicatorMappingUpdatesRequest
{
    public Guid OriginalDataSetId { get; init; }
    public Guid ReplacementDataSetId { get; init; }

    public List<IndicatorMappingUpdateRequest> Updates { get; init; } = [];

    public class Validator : AbstractValidator<IndicatorMappingUpdatesRequest>
    {
        public Validator()
        {
            RuleFor(x => x.OriginalDataSetId).NotEmpty();

            RuleFor(x => x.ReplacementDataSetId).NotEmpty();

            RuleForEach(x => x.Updates).SetValidator(new IndicatorMappingUpdateRequest.Validator());

            RuleFor(x => x.Updates)
                .Must(HaveUniqueOriginalNames)
                .WithMessage("Each OriginalId must be unique.")
                .Must(HaveUniqueReplacementNames)
                .WithMessage("Each NewReplacementId must be unique (if provided).");
        }

        private bool HaveUniqueOriginalNames(List<IndicatorMappingUpdateRequest> updates)
        {
            if (updates.IsNullOrEmpty())
            {
                return true;
            }

            return updates.Select(u => u.OriginalId).Distinct().Count() == updates.Count;
        }

        private bool HaveUniqueReplacementNames(List<IndicatorMappingUpdateRequest> updates)
        {
            if (updates.IsNullOrEmpty())
            {
                return true;
            }

            var nonNullReplacements = updates
                .Where(u => u.NewReplacementId != null)
                .Select(u => u.NewReplacementId)
                .ToList();

            return nonNullReplacements.Distinct().Count() == nonNullReplacements.Count;
        }
    }
}

public record IndicatorMappingUpdateRequest
{
    public Guid OriginalId { get; init; }
    public Guid? NewReplacementId { get; init; }

    public class Validator : AbstractValidator<IndicatorMappingUpdateRequest>
    {
        public Validator()
        {
            RuleFor(x => x.OriginalId).NotEmpty().WithMessage("OriginalId cannot be an empty.");
        }
    }
}
