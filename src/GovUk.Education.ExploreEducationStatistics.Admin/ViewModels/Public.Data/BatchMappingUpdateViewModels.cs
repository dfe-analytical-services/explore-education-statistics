#nullable enable
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;

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

    public string? SourceKey { get; init; } = null;

    public string? CandidateKey { get; init; }

    public MappingType? Type { get; init; }

    public class Validator : AbstractValidator<LocationMappingUpdateRequest>
    {
        private static readonly MappingType[] MappedTypes =
        [
            MappingType.AutoMapped,
            MappingType.ManualMapped
        ];

        public Validator()
        {
            RuleFor(request => request.Level)
                .NotNull();

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

public record LocationMappingUpdateResponse
{
    public GeographicLevel Level { get; init; }

    public string SourceKey { get; init; } = string.Empty;

    public LocationOptionMapping Mapping { get; init; } = null!;
}

public record BatchLocationMappingUpdatesResponseViewModel
{
    public List<LocationMappingUpdateResponse> Updates { get; init; } = [];
}
