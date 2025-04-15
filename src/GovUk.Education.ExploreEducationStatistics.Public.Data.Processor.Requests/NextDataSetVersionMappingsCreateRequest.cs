using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using Semver;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests;

public record NextDataSetVersionMappingsCreateRequest
{
    public required Guid DataSetId { get; init; }

    public required Guid ReleaseFileId { get; init; }

    [JsonConverter(typeof(SemVersionJsonConverter))]
    public SemVersion? DataSetVersionToPatch { get; init; } = null;

    public class Validator : AbstractValidator<NextDataSetVersionMappingsCreateRequest>
    {
        public Validator()
        {
            RuleFor(request => request.DataSetId)
                .NotEmpty();
            
            RuleFor(request => request.ReleaseFileId)
                .NotEmpty();
        }
    }
}
