#nullable enable
using FluentValidation;
using System;
using static GovUk.Education.ExploreEducationStatistics.Common.Constants.ValidationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record FeaturedTableCreateRequest
{
    public string Name { get; init; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public Guid DataBlockId { get; set; }

    public class Validator : AbstractValidator<FeaturedTableCreateRequest>
    {
        public Validator()
        {
            RuleFor(request => request.Name)
                .NotEmpty()
                .MaximumLength(FeaturedTableNameMaxLength)
                .WithMessage(FeaturedTableNameMaxLengthMessage);

            RuleFor(request => request.Description)
                .NotEmpty()
                .MaximumLength(FeaturedTableDescriptionMaxLength)
                .WithMessage(FeaturedTableDescriptionMaxLengthMessage);
        }
    }
}

public record FeaturedTableUpdateRequest
{
    public string Name { get; init; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public class Validator : AbstractValidator<FeaturedTableUpdateRequest>
    {
        public Validator()
        {
            RuleFor(request => request.Name)
                .NotEmpty()
                .MaximumLength(FeaturedTableNameMaxLength)
                .WithMessage(FeaturedTableNameMaxLengthMessage);

            RuleFor(request => request.Description)
                .NotEmpty()
                .MaximumLength(FeaturedTableDescriptionMaxLength)
                .WithMessage(FeaturedTableDescriptionMaxLengthMessage);
        }
    }
}
