using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.ModelBinding;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

public record DataSetMetaRequest
{
    /// <summary>
    /// The types of metadata to return.
    /// 
    /// Can be any combination of the following:
    /// - `Filters` - include all metadata associated with the *filters* of the requested data set version
    /// - `Indicators` - include all metadata associated with the *indicators* of the requested data set version
    /// - `Locations` - include all metadata associated with the *locations* of the requested data set version
    /// - `TimePeriods` - include all metadata associated with the *time periods* of the requested data set version
    /// </summary>
    [FromQuery, QuerySeparator]
    public IReadOnlyList<string>? Types { get; init; }

    public IReadOnlySet<MetadataType>? ParsedTypes()
        => Types?.Select(EnumUtil.GetFromEnumValue<MetadataType>).ToHashSet();

    public class Validator : AbstractValidator<DataSetMetaRequest>
    {
        public Validator()
        {
            When(request => request.Types is not null, () =>
            {
                RuleFor(request => request.Types)
                    .NotEmpty();
                RuleForEach(request => request.Types)
                    .AllowedValue(EnumUtil.GetEnumValues<MetadataType>());
            });
        }
    }
}
