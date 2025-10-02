#nullable enable
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests.Public.Data;

public record LocationMappingUpdateRequest : MappingUpdateRequest
{
    public GeographicLevel? Level { get; init; }

    public class Validator : MappingUpdateRequestValidator<LocationMappingUpdateRequest>
    {
        public Validator()
        {
            RuleFor(request => request.Level).NotNull();
        }
    }
}

public record FilterOptionMappingUpdateRequest : MappingUpdateRequest
{
    public string FilterKey { get; init; } = string.Empty;

    public class Validator : MappingUpdateRequestValidator<FilterOptionMappingUpdateRequest>
    {
        public Validator()
        {
            RuleFor(request => request.FilterKey).NotEmpty();
        }
    }
}

public record BatchLocationMappingUpdatesRequest : BatchMappingUpdatesRequest<LocationMappingUpdateRequest>
{
    public class Validator : AbstractValidator<BatchLocationMappingUpdatesRequest>
    {
        public Validator()
        {
            RuleForEach(request => request.Updates).SetValidator(new LocationMappingUpdateRequest.Validator());
        }
    }
}

public record BatchFilterOptionMappingUpdatesRequest : BatchMappingUpdatesRequest<FilterOptionMappingUpdateRequest>
{
    public class Validator : AbstractValidator<BatchFilterOptionMappingUpdatesRequest>
    {
        public Validator()
        {
            RuleForEach(request => request.Updates).SetValidator(new FilterOptionMappingUpdateRequest.Validator());
        }
    }
}

public abstract record MappingUpdateRequest
{
    private static readonly MappingType[] ManualMappingTypes = [MappingType.ManualNone, MappingType.ManualMapped];

    public string SourceKey { get; init; } = string.Empty;

    public string? CandidateKey { get; init; }

    public MappingType? Type { get; init; }

    public abstract class MappingUpdateRequestValidator<TMappingUpdateRequest>
        : AbstractValidator<TMappingUpdateRequest>
        where TMappingUpdateRequest : MappingUpdateRequest
    {
        public MappingUpdateRequestValidator()
        {
            RuleFor(request => request.SourceKey).NotEmpty();

            RuleFor(request => request.Type)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .Must(type => type is not null && ManualMappingTypes.Contains(type.Value))
                .WithErrorCode(ValidationMessages.ManualMappingTypeInvalid.Code)
                .WithMessage(ValidationMessages.ManualMappingTypeInvalid.Message);

            RuleFor(request => request.CandidateKey)
                .NotEmpty()
                .When(request => request.Type is not null && request.Type.Value == MappingType.ManualMapped)
                .WithErrorCode(ValidationMessages.CandidateKeyMustBeSpecifiedWithMappedMappingType.Code)
                .WithMessage(ValidationMessages.CandidateKeyMustBeSpecifiedWithMappedMappingType.Message);

            RuleFor(request => request.CandidateKey)
                .Empty()
                .When(request => request.Type is not null && request.Type.Value == MappingType.ManualNone)
                .WithErrorCode(ValidationMessages.CandidateKeyMustBeEmptyWithNoneMappingType.Code)
                .WithMessage(ValidationMessages.CandidateKeyMustBeEmptyWithNoneMappingType.Message);
        }
    }
}

public abstract record BatchMappingUpdatesRequest<TMappingUpdateRequest>
    where TMappingUpdateRequest : MappingUpdateRequest
{
    public List<TMappingUpdateRequest> Updates { get; init; } = [];
}
