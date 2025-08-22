#nullable enable
using System.ComponentModel;
using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests.Public.Data;

public record DataSetVersionListRequest
{
    public Guid DataSetId { get; init; }

    [DefaultValue(1)]
    public int Page { get; init; } = 1;

    [DefaultValue(10)]
    public int PageSize { get; init; } = 10;

    public class Validator : AbstractValidator<DataSetVersionListRequest>
    {
        public Validator()
        {
            RuleFor(request => request.DataSetId)
                .NotEmpty();
            RuleFor(request => request.Page)
                .GreaterThanOrEqualTo(1);
            RuleFor(request => request.PageSize)
                .InclusiveBetween(1, 20);
        }
    }
}
