#nullable enable
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests.Public.Data;

public record BatchLocationMappingUpdatesRequest
{
    public List<LocationMappingUpdateRequest> Updates { get; init; } = [];

    public class Validator : AbstractValidator<BatchLocationMappingUpdatesRequest>
    {
        public Validator()
        {
            RuleForEach(request => request.Updates)
                .SetValidator(new LocationMappingUpdateRequest.Validator());
        }
    }
}

public record LocationMappingUpdateRequest
{
    public GeographicLevel? Level { get; init; }

    public string SourceKey { get; init; } = string.Empty;

    public string? CandidateKey { get; init; }

    public MappingType? Type { get; init; }

    public class Validator : AbstractValidator<LocationMappingUpdateRequest>
    {
        private static readonly MappingType[] ManualMappingTypes =
        [
            MappingType.ManualNone,
            MappingType.ManualMapped
        ];

        public Validator()
        {
            RuleFor(request => request.Level)
                .NotNull()
                .IsInEnum();

            RuleFor(request => request.SourceKey)
                .NotEmpty();

            RuleFor(request => request.Type)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .Must(type => ManualMappingTypes.Contains(type!.Value))
                .WithErrorCode(ValidationMessages.ManualMappingTypeInvalid.Code)
                .WithMessage(ValidationMessages.ManualMappingTypeInvalid.Message);

            RuleFor(request => request.CandidateKey)
                .NotEmpty()
                .When(request => request.Type is not null && request.Type.Value == MappingType.ManualMapped)
                .WithErrorCode(ValidationMessages.CandidateKeyMustBeSpecifiedWithMappedMappingType.Code)
                .WithMessage(ValidationMessages.CandidateKeyMustBeSpecifiedWithMappedMappingType.Message);
            
            RuleFor(request => request.CandidateKey)
                .Null()
                .When(request => request.Type is not null && request.Type.Value != MappingType.ManualMapped)
                .WithErrorCode(ValidationMessages.CandidateKeyMustBeEmptyWithNoneMappingType.Code)
                .WithMessage(ValidationMessages.CandidateKeyMustBeEmptyWithNoneMappingType.Message);
        }
    }
}
