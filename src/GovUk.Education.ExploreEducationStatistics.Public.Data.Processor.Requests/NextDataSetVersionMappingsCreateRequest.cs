using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests.Validators;
using Semver;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests;

public record NextDataSetVersionMappingsCreateRequest
{
    public required Guid DataSetId { get; init; }

    public required Guid ReleaseFileId { get; init; }

    public string? DataSetVersionToReplace { get; init; } = null;

    public class Validator : AbstractValidator<NextDataSetVersionMappingsCreateRequest>
    {
        public Validator()
        {
            RuleFor(request => request.DataSetId)
                .NotEmpty();
            
            RuleFor(request => request.ReleaseFileId)
                .NotEmpty();
            
            RuleFor(request => request.DataSetVersionToReplace)
                .Must(version => SemVersion.TryParse(version,
                    SemVersionStyles.OptionalMinorPatch
                    | SemVersionStyles.AllowWhitespace
                    | SemVersionStyles.AllowLowerV
                    ,out _))
                .When(request => !string.IsNullOrWhiteSpace(request.DataSetVersionToReplace))
                .WithErrorCode(ValidationMessages.DataSetVersionToReplaceNotValid.Code)
                .WithMessage(ValidationMessages.DataSetVersionToReplaceNotValid.Message);
        }
    }
}
