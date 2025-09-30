using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.ModelBinding;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Swagger;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

public record DataSetMetaRequest
{
    /// <summary>
    /// The types of meta to get for the requested data set version.
    ///
    /// Can be any combination of the following:
    ///
    /// - `Filters` - include all meta relating to *filters*
    /// - `Indicators` - include all meta relating to *indicators*
    /// - `Locations` - include all meta relating to *locations*
    /// - `TimePeriods` - include all meta relating to *time periods*
    /// </summary>
    /// <example>["Filters", "Locations"]</example>
    [FromQuery]
    [QuerySeparator]
    [SwaggerEnum(type: typeof(DataSetMetaType), serializer: SwaggerEnumSerializer.String)]
    public IReadOnlyList<string>? Types { get; init; }

    public IReadOnlySet<DataSetMetaType>? ParsedTypes() =>
        Types?.Select(EnumUtil.GetFromEnumValue<DataSetMetaType>).ToHashSet();

    public class Validator : AbstractValidator<DataSetMetaRequest>
    {
        public Validator()
        {
            When(
                request => request.Types is not null,
                () =>
                {
                    RuleFor(request => request.Types).NotEmpty();
                    RuleForEach(request => request.Types)
                        .AllowedValue(EnumUtil.GetEnumValues<DataSetMetaType>());
                }
            );
        }
    }
}
