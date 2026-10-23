#nullable enable
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record PairedIndicatorsUpdateRequest
{
    public List<PairedIndicatorRequest> PairedIndicators { get; init; } = [];

    public class Validator : AbstractValidator<PairedIndicatorsUpdateRequest>
    {
        public Validator()
        {
            RuleFor(request => request.PairedIndicators).NotNull();

            RuleForEach(request => request.PairedIndicators)
                .NotNull()
                .SetValidator(new PairedIndicatorRequest.Validator());

            RuleFor(request => request.PairedIndicators)
                .Must(HaveUniqueChildren)
                .WithMessage(ValidationMessages.PairedIndicatorChildHasMultipleParents)
                .Must(HaveUniqueParents)
                .WithMessage(ValidationMessages.PairedIndicatorParentHasMultipleChildren)
                .Must(NotBeChained)
                .WithMessage(ValidationMessages.PairedIndicatorsCannotBeChained)
                // Only check across pairs once every pair is present, as null pairs are reported above
                .When(request =>
                    request.PairedIndicators is not null
                    && request.PairedIndicators.All(pair => (PairedIndicatorRequest?)pair is not null)
                );
        }

        private static bool HaveUniqueChildren(List<PairedIndicatorRequest> pairedIndicators) =>
            pairedIndicators.DistinctBy(pair => pair.ChildIndicatorId).Count() == pairedIndicators.Count;

        private static bool HaveUniqueParents(List<PairedIndicatorRequest> pairedIndicators) =>
            pairedIndicators.DistinctBy(pair => pair.ParentIndicatorId).Count() == pairedIndicators.Count;

        private static bool NotBeChained(List<PairedIndicatorRequest> pairedIndicators)
        {
            // Self-links are reported separately, so are excluded here
            var validPairs = pairedIndicators.Where(pair => pair.ParentIndicatorId != pair.ChildIndicatorId).ToList();

            var parentIds = validPairs.Select(pair => pair.ParentIndicatorId).ToHashSet();

            return !validPairs.Any(pair => parentIds.Contains(pair.ChildIndicatorId));
        }
    }
}

public record PairedIndicatorRequest
{
    public Guid ParentIndicatorId { get; init; }

    public Guid ChildIndicatorId { get; init; }

    public class Validator : AbstractValidator<PairedIndicatorRequest>
    {
        public Validator()
        {
            RuleFor(request => request.ParentIndicatorId).NotEmpty();

            RuleFor(request => request.ChildIndicatorId)
                .NotEmpty()
                .NotEqual(request => request.ParentIndicatorId)
                .WithMessage(ValidationMessages.PairedIndicatorCannotPairWithItself);
        }
    }
}
