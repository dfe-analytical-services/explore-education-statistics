#nullable enable
using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests.Public.Data;

public record NextDataSetVersionCompleteImportRequest
{
    public required Guid DataSetVersionId { get; init; }

    /// <summary>
    /// Allows a BAU user to finalise a patch version whose mappings result in a major version update.
    /// </summary>
    public bool AllowMajorVersionUpdate { get; init; }

    public class Validator : AbstractValidator<NextDataSetVersionCompleteImportRequest>
    {
        public Validator()
        {
            RuleFor(request => request.DataSetVersionId).NotEmpty();
        }
    }
}
