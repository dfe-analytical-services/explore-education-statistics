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
            RuleForEach(x => x.Updates).SetValidator(new IndicatorMappingUpdateRequest.Validator());

            RuleFor(x => x.Updates)
                .Must(HaveUniqueOriginalNames)
                .WithMessage("Each OriginalColumnName must be unique.")
                .Must(HaveUniqueReplacementNames)
                .WithMessage("Each NewReplacementColumnName must be unique (if provided).");
        }

        private bool HaveUniqueOriginalNames(List<IndicatorMappingUpdateRequest> updates)
        {
            if (updates.IsNullOrEmpty())
            {
                return true;
            }

            return updates.Select(u => u.OriginalColumnName).Distinct().Count() == updates.Count;
        }

        private bool HaveUniqueReplacementNames(List<IndicatorMappingUpdateRequest> updates)
        {
            if (updates.IsNullOrEmpty())
            {
                return true;
            }

            var nonNullReplacements = updates
                .Where(u => !string.IsNullOrEmpty(u.NewReplacementColumnName))
                .Select(u => u.NewReplacementColumnName)
                .ToList();

            return nonNullReplacements.Distinct().Count() == nonNullReplacements.Count;
        }
    }
}

public record IndicatorMappingUpdateRequest
{
    public string OriginalColumnName { get; init; } = "";
    public string? NewReplacementColumnName { get; init; }

    public class Validator : AbstractValidator<IndicatorMappingUpdateRequest>
    {
        public Validator()
        {
            RuleFor(x => x.OriginalColumnName).NotEmpty().WithMessage("OriginalColumnName cannot be an empty.");
        }
    }
}
