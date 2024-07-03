#nullable enable
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;

public record LocationMappingUpdateRequest : MappingUpdateRequest
{
    public GeographicLevel? Level { get; init; }

    public class Validator : MappingUpdateRequestValidator<LocationMappingUpdateRequest>
    {
        public Validator()
        {
            RuleFor(request => request.Level)
                .NotNull();
        }
    }
}

public record FilterOptionMappingUpdateRequest : MappingUpdateRequest
{
    public string? FilterKey { get; init; } = null;

    public class Validator : MappingUpdateRequestValidator<FilterOptionMappingUpdateRequest>
    {
        public Validator()
        {
            RuleFor(request => request.FilterKey)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .NotEmpty();
        }
    }
}

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

public record BatchFilterOptionMappingUpdatesRequest
{
    public List<FilterOptionMappingUpdateRequest> Updates { get; init; } = [];

    public class Validator : AbstractValidator<BatchFilterOptionMappingUpdatesRequest>
    {
        public Validator()
        {
            RuleForEach(request => request.Updates)
                .SetValidator(new FilterOptionMappingUpdateRequest.Validator());
        }
    }
}

public record LocationMappingUpdateResponse : MappingUpdateResponse<LocationOptionMapping, MappableLocationOption>
{
    public GeographicLevel Level { get; init; }
}

public record FilterOptionMappingUpdateResponse : MappingUpdateResponse<FilterOptionMapping, MappableFilterOption>
{
    public string FilterKey { get; init; } = string.Empty;
}

public record BatchLocationMappingUpdatesResponseViewModel : BatchMappingUpdatesResponseViewModel
    <LocationMappingUpdateResponse, LocationOptionMapping, MappableLocationOption>;

public record BatchFilterOptionMappingUpdatesResponseViewModel : BatchMappingUpdatesResponseViewModel
    <FilterOptionMappingUpdateResponse, FilterOptionMapping, MappableFilterOption>;

public abstract record MappingUpdateRequest
{
    private static readonly MappingType[] MappedTypes =
    [
        MappingType.AutoMapped,
        MappingType.ManualMapped
    ];

    public string? SourceKey { get; init; }

    public string? CandidateKey { get; init; }

    public MappingType? Type { get; init; }

    public abstract class MappingUpdateRequestValidator<TMappingUpdateRequest>
        : AbstractValidator<TMappingUpdateRequest>
        where TMappingUpdateRequest : MappingUpdateRequest
    {
        public MappingUpdateRequestValidator()
        {
            RuleFor(request => request.SourceKey)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .NotEmpty();

            RuleFor(request => request.Type)
                .NotNull();

            RuleFor(request => request.CandidateKey)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .NotEmpty()
                .When(request => request.Type is not null && MappedTypes.Contains(request.Type.Value));

            RuleFor(request => request.CandidateKey)
                .Null()
                .When(request => request.Type is not null && !MappedTypes.Contains(request.Type.Value));
        }
    }
}

public abstract record MappingUpdateResponse<TMapping, TMappableElement>
    where TMapping : Mapping<TMappableElement>
    where TMappableElement : MappableElement
{
    public string SourceKey { get; init; } = string.Empty;

    public TMapping Mapping { get; init; } = null!;
}

public abstract record BatchMappingUpdatesResponseViewModel<TMappingUpdateResponse, TMapping, TMappableElement>
    where TMappingUpdateResponse : MappingUpdateResponse<TMapping, TMappableElement>
    where TMapping : Mapping<TMappableElement>
    where TMappableElement : MappableElement
{
    public List<TMappingUpdateResponse> Updates { get; init; } = [];
}
