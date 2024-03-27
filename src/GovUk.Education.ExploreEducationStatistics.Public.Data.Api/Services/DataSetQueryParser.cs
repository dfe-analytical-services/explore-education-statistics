using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
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
    IParquetFilterOptionRepository filterOptionRepository,
    IParquetLocationOptionRepository locationOptionRepository,
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
            filterOptionRepository.List(dataSetVersion, facets.Filters, cancellationToken);
        var locationOptionMetas =
            locationOptionRepository.List(dataSetVersion, facets.Locations, cancellationToken);
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

    private Facets ExtractFacets(DataSetQueryCriteria criteria)
    {
        var facets = new Facets();

        if (criteria is DataSetQueryCriteriaFacets criteriaFacets)
        {
            facets.Filters.AddRange(criteriaFacets.Filters?.GetOptions() ?? []);
            facets.GeographicLevels.AddRange(criteriaFacets.GeographicLevels?.GetOptions() ?? []);
            facets.Locations.AddRange(criteriaFacets.Locations?.GetOptions() ?? []);
            facets.TimePeriods.AddRange(criteriaFacets.TimePeriods?.GetOptions() ?? []);
        }

        return facets;
    }

    private IInterpolatedSql ParseCriteriaFragment(
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

            builder.AppendRange(fragments, joinString: " AND ");
        }

        return builder.Build();
    }

    private class Facets
    {
        public HashSet<string> Filters { get; init; } = [];

        public HashSet<GeographicLevel> GeographicLevels { get; init; } = [];

        public HashSet<DataSetQueryLocation> Locations { get; init; } = [];

        public HashSet<DataSetQueryTimePeriod> TimePeriods { get; init; } = [];
    }
}
