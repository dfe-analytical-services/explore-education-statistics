using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Query;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;
using InterpolatedSql;
using StackExchange.Profiling;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;

internal class DataSetQueryParser(
    IParquetFilterRepository filterRepository,
    IParquetLocationRepository locationRepository,
    IParquetTimePeriodRepository timePeriodRepository)
    : IDataSetQueryParser
{
    public async Task<IInterpolatedSql> ParseCriteria(
        DataSetQueryCriteria criteria,
        DataSetVersion dataSetVersion,
        QueryState queryState,
        CancellationToken cancellationToken = default)
    {
        using var _ = MiniProfiler.Current
            .Step($"{nameof(DataSetQueryParser)}.{nameof(ParseCriteria)}");

        var facets = ExtractFacets(criteria);

        var filterOptionMetas =
            filterRepository.ListOptions(dataSetVersion, facets.Filters, cancellationToken);
        var locationOptionMetas =
            locationRepository.ListOptions(dataSetVersion, facets.Locations, cancellationToken);
        var timePeriodMetas =
            timePeriodRepository.List(dataSetVersion, facets.TimePeriods, cancellationToken);

        await Task.WhenAll(filterOptionMetas, locationOptionMetas, timePeriodMetas);

        IFacetsParser[] parsers =
        [
            new FilterFacetsParser(queryState, filterOptionMetas.Result),
            new GeographicLevelFacetsParser(queryState, [..dataSetVersion.MetaSummary.GeographicLevels]),
            new LocationFacetsParser(queryState, locationOptionMetas.Result),
            new TimePeriodFacetsParser(queryState, timePeriodMetas.Result),
        ];

        return ParseCriteriaFragment(criteria, parsers);
    }

    private static Facets ExtractFacets(DataSetQueryCriteria criteria)
    {
        var facets = new Facets();

        if (criteria is DataSetQueryCriteriaFacets criteriaFacets)
        {
            facets.Filters.AddRange(criteriaFacets.Filters?.GetOptions() ?? []);
            facets.Locations.AddRange(criteriaFacets.Locations?.GetOptions() ?? []);
            facets.TimePeriods.AddRange(criteriaFacets.TimePeriods?.GetOptions() ?? []);
        }

        return facets;
    }

    private static IInterpolatedSql ParseCriteriaFragment(
        DataSetQueryCriteria criteria,
        IEnumerable<IFacetsParser> facetParsers)
    {
        var path = "";
        var builder = new DuckDbSqlBuilder();

        if (criteria is DataSetQueryCriteriaFacets criteriaFacets)
        {
            var fragments = facetParsers
                .Select(parser => parser.Parse(criteriaFacets, path))
                .Where(fragment => !fragment.IsEmpty());

            builder.AppendRange(fragments, joinString: "\nAND ");
        }

        return builder.Build();
    }

    private class Facets
    {
        public HashSet<string> Filters { get; init; } = [];

        public HashSet<DataSetQueryLocation> Locations { get; init; } = [];

        public HashSet<DataSetQueryTimePeriod> TimePeriods { get; init; } = [];
    }
}
