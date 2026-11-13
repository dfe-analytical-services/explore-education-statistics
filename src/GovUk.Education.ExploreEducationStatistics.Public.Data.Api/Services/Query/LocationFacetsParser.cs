using GovUk.Education.ExploreEducationStatistics.Common.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;
using GovUk.Education.ExploreEducationStatistics.Public.Data.ViewModels;
using InterpolatedSql;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Query;

internal class LocationFacetsParser : IFacetsParser
{
    private readonly QueryState _queryState;
    private readonly Dictionary<GeographicLevel, List<ParquetLocationOption>> _allowedLocationOptionsByLevel;

    public LocationFacetsParser(QueryState queryState, IEnumerable<ParquetLocationOption> locationOptionMetas)
    {
        _queryState = queryState;
        _allowedLocationOptionsByLevel = locationOptionMetas
            .GroupBy(location => EnumUtil.GetFromEnumValue<GeographicLevel>(location.Level))
            .ToDictionary(group => group.Key, group => group.ToList());
    }

    public IInterpolatedSql Parse(DataSetQueryCriteriaFacets facets, string path)
    {
        var fragments = new List<IInterpolatedSql>();

        if (facets.Locations?.Eq is not null)
        {
            fragments.Add(EqFragment(location: facets.Locations.Eq, path: QueryUtils.Path(path, "locations.eq")));
        }

        if (facets.Locations?.NotEq is not null)
        {
            fragments.Add(
                EqFragment(
                    location: facets.Locations.NotEq,
                    path: QueryUtils.Path(path, "locations.notEq"),
                    negate: true
                )
            );
        }

        if (facets.Locations?.In is not null && facets.Locations.In.Count != 0)
        {
            fragments.Add(InFragment(locations: [.. facets.Locations.In], path: QueryUtils.Path(path, "locations.in")));
        }

        if (facets.Locations?.NotIn is not null && facets.Locations.NotIn.Count != 0)
        {
            fragments.Add(
                InFragment(
                    locations: [.. facets.Locations.NotIn],
                    path: QueryUtils.Path(path, "locations.notIn"),
                    negate: true
                )
            );
        }

        return new DuckDbSqlBuilder().AppendRange(fragments, "\nAND ").Build();
    }

    private IInterpolatedSql EqFragment(IDataSetQueryLocation location, string path, bool negate = false)
    {
        var builder = new DuckDbSqlBuilder();

        var options = GetLocationOptions(location);

        if (options.Count == 0)
        {
            _queryState.Warnings.Add(CreateNotFoundWarning([location], path));

            return builder.AppendLiteral(negate ? "true" : "false").Build();
        }

        var column = DataTable.Ref().LocationId(location.ParsedLevel());

        // Non-unique identifiers (e.g. a school URN where the school name
        // changed over time) can match multiple location options.
        if (options.Count == 1)
        {
            builder += $"{column:raw} {(negate ? "!=" : "="):raw} {options[0].Id}";
        }
        else
        {
            var ids = options.Select(o => o.Id);
            builder += $"{column:raw} {(negate ? "NOT IN" : "IN"):raw} ({ids})";
        }

        return builder.Build();
    }

    private IInterpolatedSql InFragment(HashSet<IDataSetQueryLocation> locations, string path, bool negate = false)
    {
        var builder = new DuckDbSqlBuilder();

        var options = new List<ParquetLocationOption>();
        var notFoundLocations = new List<IDataSetQueryLocation>();

        foreach (var location in locations)
        {
            var matchingOptions = GetLocationOptions(location);

            if (matchingOptions.Count == 0)
            {
                notFoundLocations.Add(location);
                continue;
            }

            options.AddRange(matchingOptions);
        }

        if (notFoundLocations.Count != 0)
        {
            _queryState.Warnings.Add(CreateNotFoundWarning(notFoundLocations, path));
        }

        if (options.Count == 0)
        {
            return builder.AppendLiteral(negate ? "true" : "false").Build();
        }

        var fragments = options
            .DistinctBy(l => l.Id)
            .GroupBy(l => l.Level)
            .Select(group =>
            {
                var level = EnumUtil.GetFromEnumValue<GeographicLevel>(group.Key);
                var ids = group.Select(l => l.Id);

                return (FormattableString)
                    $"{DataTable.Ref().LocationId(level):raw} {(negate ? "NOT IN" : "IN"):raw} ({ids})";
            })
            .ToList();

        return fragments.Count == 1
            ? builder.AppendFormattableString(fragments[0]).Build()
            : builder
                .AppendLiteral("(")
                .AppendRange(fragments, joinString: negate ? "\n AND " : "\n OR ")
                .AppendLiteral(")")
                .Build();
    }

    private List<ParquetLocationOption> GetLocationOptions(IDataSetQueryLocation queryLocation) =>
        _allowedLocationOptionsByLevel.TryGetValue(queryLocation.ParsedLevel(), out var options)
            ? options.Where(option => IsMatchingLocationOption(option, queryLocation)).ToList()
            : [];

    private static bool IsMatchingLocationOption(ParquetLocationOption option, IDataSetQueryLocation queryLocation) =>
        queryLocation switch
        {
            DataSetQueryLocationId locationId => option.PublicId == locationId.Id,
            DataSetQueryLocationCode locationCode => option.Code == locationCode.Code,
            DataSetQueryLocationSchoolUrn school => option.Urn == school.Urn,
            DataSetQueryLocationSchoolLaEstab school => option.LaEstab == school.LaEstab,
            DataSetQueryLocationProviderUkprn provider => option.Ukprn == provider.Ukprn,
            DataSetQueryLocationLocalAuthorityCode localAuthority => option.Code == localAuthority.Code,
            DataSetQueryLocationLocalAuthorityOldCode localAuthority => option.OldCode == localAuthority.OldCode,
            _ => false,
        };

    private WarningViewModel CreateNotFoundWarning(IEnumerable<IDataSetQueryLocation> locations, string path) =>
        new()
        {
            Code = ValidationMessages.LocationsNotFound.Code,
            Message = ValidationMessages.LocationsNotFound.Message,
            Path = path,
            Detail = new NotFoundItemsErrorDetail<IDataSetQueryLocation>(locations),
        };
}
