using FluentValidation;
using System.ComponentModel;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests.Public.Data;

public record DataSetVersionListRequest
{
    [DefaultValue(1)]
    public int Page { get; init; } = 1;

    [DefaultValue(10)]
    public int PageSize { get; init; } = 10;

    public class Validator : AbstractValidator<DataSetVersionListRequest>
    {
        public Validator()
        {
            RuleFor(request => request.Page)
                .GreaterThanOrEqualTo(1);
            RuleFor(request => request.PageSize)
                .InclusiveBetween(1, 20);
        }
    }
}
