using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;
using InterpolatedSql;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Query;

internal class LocationFacetsParser : IFacetsParser
{
    private readonly QueryState _queryState;
    private readonly IDictionary<LocationKey, ParquetLocationOption> _locationOptionMetas;

    public LocationFacetsParser(
        QueryState queryState,
        IEnumerable<ParquetLocationOption> locationOptionMetas)
    {
        _queryState = queryState;
        _locationOptionMetas = locationOptionMetas.ToDictionary(location => new LocationKey(location));
    }

    public IInterpolatedSql Parse(DataSetQueryCriteriaFacets facets, string path)
    {
        var fragments = new List<IInterpolatedSql>();

        if (facets.Locations?.Eq is not null)
        {
            fragments.Add(
                EqFragment(
                    location: facets.Locations.Eq,
                    path: QueryUtils.Path(path, "locations.eq")
                )
            );
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

        if (facets.Locations?.In is not null)
        {
            fragments.Add(
                InFragment(
                    locations: [..facets.Locations.In],
                    path: QueryUtils.Path(path, "locations.in")
                )
            );
        }

        if (facets.Locations?.NotIn is not null)
        {
            fragments.Add(
                InFragment(
                    locations: [..facets.Locations.NotIn],
                    path: QueryUtils.Path(path, "locations.notIn"),
                    negate: true
                )
            );
        }

        return new DuckDbSqlBuilder()
            .AppendRange(fragments, " AND ")
            .Build();
    }

    private IInterpolatedSql EqFragment(
        DataSetQueryLocation location,
        string path,
        bool negate = false)
    {
        var builder = new DuckDbSqlBuilder();

        if (!_locationOptionMetas.TryGetValue(new LocationKey(location), out var id))
        {
            _queryState.Warnings.Add(CreateNotFoundWarning([location], path));

            return builder
                .AppendLiteral(negate ? "true" : "false")
                .Build();
        }

        builder += $"{DataTable.Ref().LocationId(location.ParsedLevel):raw} {(negate ? "!=" : "="):raw} {id}";

        return builder.Build();
    }

    private IInterpolatedSql InFragment(
        IReadOnlyList<DataSetQueryLocation> locations,
        string path,
        bool negate = false)
    {
        var builder = new DuckDbSqlBuilder();

        var options = locations
            .Distinct()
            .Select(location => new LocationKey(location))
            .Where(_locationOptionMetas.ContainsKey)
            .Select(location => _locationOptionMetas[location])
            .ToList();
        
        if (options.Count < locations.Count)
        {
            _queryState.Warnings.Add(CreateNotFoundWarning([..locations], path));
        }

        if (options.Count == 0)
        {
            return builder
                .AppendLiteral(negate ? "true" : "false")
                .Build();
        }

        var fragments = options
            .GroupBy(l => l.Level)
            .ToList()
            .Select(
                group =>
                {
                    var level = EnumUtil.GetFromEnumValue<GeographicLevel>(group.Key);
                    var ids = group.Select(l => l.Id);

                    FormattableString str =
                        $"{DataTable.Ref().LocationId(level):raw} {(negate ? "NOT IN" : "IN"):raw} ({ids})";

                    return str;
                })
            .ToList();

        return builder
            .AppendLiteral("(")
            .AppendRange(fragments, negate ? " AND " : " OR ")
            .AppendLiteral(")")
            .Build();
    }

    private WarningViewModel CreateNotFoundWarning(HashSet<DataSetQueryLocation> locations, string path) => new()
    {
        Code = ValidationMessages.LocationsNotFound.Code,
        Message = ValidationMessages.LocationsNotFound.Message,
        Path = path,
        Detail = new NotFoundItemsErrorDetail<DataSetQueryLocation>(
            locations.Where(location => !_locationOptionMetas.ContainsKey(new LocationKey(location)))
        )
    };

    private record LocationKey
    {
        public GeographicLevel Level { get; init; }

        public string? PublicId { get; init; }

        public string? Code { get; init; }

        public string? OldCode { get; init; }

        public string? Ukprn { get; init; }

        public string? Urn { get; init; }

        public string? LaEstab { get; init; }

        public LocationKey(ParquetLocationOption location)
        {
            Level = EnumUtil.GetFromEnumValue<GeographicLevel>(location.Level);
            PublicId = location.PublicId;
            Code = location.Code;
            OldCode = location.OldCode;
            Ukprn = location.Ukprn;
            Urn = location.Urn;
            LaEstab = location.LaEstab;
        }

        public LocationKey(DataSetQueryLocation location)
        {
            Level = location.ParsedLevel;

            switch (location)
            {
                case DataSetQueryLocationId locationId:
                    PublicId = locationId.Id;
                    break;
                case DataSetQueryLocationCode locationCode:
                    Code = locationCode.Code;
                    break;
                case DataSetQueryLocationLocalAuthority localAuthority:
                    Code = localAuthority.Code;
                    OldCode = localAuthority.OldCode;
                    break;
                case DataSetQueryLocationProvider provider:
                    Ukprn = provider.Ukprn;
                    break;
                case DataSetQueryLocationSchool school:
                    Urn = school.Urn;
                    LaEstab = school.LaEstab;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(location));
            }
        }
    }
}
