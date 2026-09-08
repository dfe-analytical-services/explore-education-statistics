#nullable enable
using FluentValidation;
using LinqToDB.Internal.Common;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record MappingUpdateRequest
{
    public Guid OriginalId { get; init; }
    public Guid? NewReplacementId { get; init; }

    public class Validator : AbstractValidator<MappingUpdateRequest>
    {
        public Validator()
        {
            RuleFor(x => x.OriginalId).NotEmpty().WithMessage("OriginalId cannot be an empty.");
        }
    }

    public static bool HaveUniqueOriginalIds(List<MappingUpdateRequest> updates)
    {
        if (updates.IsNullOrEmpty())
        {
            return true;
        }

        return updates.Select(u => u.OriginalId).Distinct().Count() == updates.Count;
    }

    public static bool HaveUniqueReplacementIds(List<MappingUpdateRequest> updates)
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
