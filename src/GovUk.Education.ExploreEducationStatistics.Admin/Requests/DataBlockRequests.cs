#nullable enable
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record DataBlockCreateRequest
{
    [Required] public string Heading { get; init; } = string.Empty;

    [Required] public string Name { get; init; } = string.Empty;

    public string Source { get; init; } = string.Empty;

    public FullTableQueryRequest Query { get; init; } = null!;

    public List<IChart> Charts { get; init; } = new();

    public TableBuilderConfiguration Table { get; init; } = null!;

    public class Validator : AbstractValidator<DataBlockCreateRequest>
    {
        public Validator()
        {
            RuleFor(request => request.Query)
                .SetValidator(new FullTableQueryRequest.Validator());
        }
    }
}

public record DataBlockUpdateRequest
{
    [Required]
    public string Heading { get; init; } = string.Empty;

    [Required]
    public string Name { get; init; } = string.Empty;

    public string Source { get; init; } = string.Empty;

    public FullTableQueryRequest Query { get; init; } = null!;

    public List<IChart> Charts { get; init; } = new();

    public TableBuilderConfiguration Table { get; init; } = null!;

    public class Validator : AbstractValidator<DataBlockUpdateRequest>
    {
        public Validator()
        {
            RuleFor(request => request.Query)
                .SetValidator(new FullTableQueryRequest.Validator());
        }
    }
}
