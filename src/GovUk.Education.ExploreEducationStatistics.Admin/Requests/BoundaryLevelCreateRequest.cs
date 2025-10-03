#nullable enable
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record BoundaryLevelCreateRequest
{
    public required GeographicLevel Level { get; init; }

    public required string Label { get; init; }

    public required IFormFile File { get; init; }

    public required DateTime Published { get; init; }

    public class Validator() : AbstractValidator<BoundaryLevelCreateRequest>
    {
        public Validator(IBoundaryLevelService boundaryLevelService)
            : this()
        {
            RuleFor(request => request.Level).NotEmpty();

            RuleFor(request => request.Label)
                .NotEmpty()
                .MustAsync(
                    async (label, cancellationToken) =>
                    {
                        var existingLevels = await boundaryLevelService.ListBoundaryLevels(cancellationToken);
                        return existingLevels.IsRight && !existingLevels.Right.Exists(bl => bl.Label == label);
                    }
                )
                .WithMessage("A boundary level matching {PropertyValue} already exists");

            RuleFor(request => request.File)
                .NotNull()
                .Must(file => file.FileName.Split('.').Last() == "geojson")
                .WithMessage("Invalid file type \"{PropertyValue}\" - file must be in GeoJSON format");

            RuleFor(request => request.Published).NotEmpty();
        }
    }
}
