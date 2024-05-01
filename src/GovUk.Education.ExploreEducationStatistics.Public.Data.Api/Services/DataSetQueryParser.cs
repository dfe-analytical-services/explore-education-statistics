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
            new GeographicLevelFacetsParser(queryState, [..dataSetVersion.MetaSummary!.GeographicLevels]),
            new LocationFacetsParser(queryState, locationOptionMetas.Result),
            new TimePeriodFacetsParser(queryState, timePeriodMetas.Result),
        ];

        return ParseCriteriaFragment(criteria, parsers);
    }

    private static Facets ExtractFacets(DataSetQueryCriteria criteria, Facets? facets = null)
    {
        facets ??= new Facets();

        switch (criteria)
        {
            case DataSetQueryCriteriaFacets criteriaFacets:
                facets.Filters.AddRange(criteriaFacets.Filters?.GetOptions() ?? []);
                facets.Locations.AddRange(criteriaFacets.Locations?.GetOptions() ?? []);
                facets.TimePeriods.AddRange(criteriaFacets.TimePeriods?.GetOptions() ?? []);
                break;

            case DataSetQueryCriteriaAnd andCriteria:
                andCriteria.And.ForEach(subCriteria => ExtractFacets(subCriteria, facets));
                break;

            case DataSetQueryCriteriaOr orCriteria:
                orCriteria.Or.ForEach(subCriteria => ExtractFacets(subCriteria, facets));
                break;

            case DataSetQueryCriteriaNot notCriteria:
                ExtractFacets(notCriteria.Not, facets);
                break;
        }

        return facets;
    }

    private static IInterpolatedSql ParseCriteriaFragment(
        DataSetQueryCriteria criteria,
        IEnumerable<IFacetsParser> facetParsers,
        string path = "")
    {
        var builder = new DuckDbSqlBuilder();

        switch (criteria)
        {
            case DataSetQueryCriteriaFacets facetCriteria:
                var facetFragments = facetParsers
                    .Select(parser => parser.Parse(facetCriteria, path))
                    .Where(fragment => !fragment.IsEmpty());

                builder.AppendRange(facetFragments, joinString: "\nAND ");
                break;

            case DataSetQueryCriteriaAnd andCriteria:


                var andFragments = andCriteria.And
                    .Select(c => ParseCriteriaFragment(c, facetParsers, path: $"{path}.And"));

                builder.AppendRange(andFragments, joinString: "\nAND ");
                break;

            case DataSetQueryCriteriaOr orCriteria:
                var orFragments = orCriteria.Or
                    .Select(c => ParseCriteriaFragment(c, facetParsers, path: $"{path}.Or"));

                builder.AppendRange(orFragments, joinString: "\nOR ");
                break;

            case DataSetQueryCriteriaNot notCriteria:
                builder.AppendLiteral("NOT (");
                builder.Append(ParseCriteriaFragment(notCriteria.Not, facetParsers, path: $"{path}.Not"));
                builder.AppendLiteral(") ");
                break;
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
