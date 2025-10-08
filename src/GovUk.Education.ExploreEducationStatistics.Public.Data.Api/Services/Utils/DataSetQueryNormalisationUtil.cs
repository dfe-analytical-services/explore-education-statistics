using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Utils;

public static class DataSetQueryNormalisationUtil
{
    internal static DataSetQueryRequest NormaliseQuery(DataSetQueryRequest original)
    {
        return original with
        {
            Indicators = original.Indicators?.Order().ToList(),

            Criteria = original.Criteria != null ? NormaliseCriteria(original.Criteria) : null,
        };
    }

    private static IDataSetQueryCriteria NormaliseCriteria(IDataSetQueryCriteria original)
    {
        switch (original)
        {
            case DataSetQueryCriteriaFacets facets:
                return NormaliseFacets(facets)!;
            case DataSetQueryCriteriaAnd and:
                return NormaliseAnd(and);
            case DataSetQueryCriteriaOr or:
                return NormaliseOr(or);
            case DataSetQueryCriteriaNot not:
                return NormaliseNot(not);
            default:
                throw new ArgumentException($"Unknown data set query criteria type {original.GetType()}");
        }
    }

    private static IDataSetQueryCriteria? NormaliseFacets(DataSetQueryCriteriaFacets? original)
    {
        if (original == null)
        {
            return null;
        }

        return new DataSetQueryCriteriaFacets
        {
            Filters = NormaliseFilters(original.Filters),
            Locations = NormaliseLocations(original.Locations),
            GeographicLevels = NormaliseGeographicLevels(original.GeographicLevels),
            TimePeriods = NormaliseTimePeriods(original.TimePeriods),
        };
    }

    private static DataSetQueryCriteriaAnd NormaliseAnd(DataSetQueryCriteriaAnd original)
    {
        return new DataSetQueryCriteriaAnd
        {
            And = original
                .And.Select(NormaliseCriteria)
                .OrderBy(criteria =>
                    JsonSerializationUtils.Serialize(
                        obj: criteria,
                        formatting: Formatting.None,
                        camelCase: true,
                        orderedProperties: true
                    )
                )
                .ToList(),
        };
    }

    private static DataSetQueryCriteriaOr NormaliseOr(DataSetQueryCriteriaOr original)
    {
        return new DataSetQueryCriteriaOr
        {
            Or = original
                .Or.Select(NormaliseCriteria)
                .OrderBy(criteria =>
                    JsonSerializationUtils.Serialize(
                        obj: criteria,
                        formatting: Formatting.None,
                        camelCase: true,
                        orderedProperties: true
                    )
                )
                .ToList(),
        };
    }

    private static DataSetQueryCriteriaNot NormaliseNot(DataSetQueryCriteriaNot original)
    {
        return new DataSetQueryCriteriaNot { Not = NormaliseCriteria(original.Not) };
    }

    private static DataSetQueryCriteriaFilters? NormaliseFilters(DataSetQueryCriteriaFilters? original)
    {
        return original == null
            ? null
            : original with
            {
                In = original.In?.Order().ToList(),
                NotIn = original.NotIn?.Order().ToList(),
            };
    }

    private static DataSetQueryCriteriaLocations? NormaliseLocations(DataSetQueryCriteriaLocations? original)
    {
        return original == null
            ? null
            : original with
            {
                In = original.In?.OrderBy(location => location.ToLocationString()).ToList(),

                NotIn = original.NotIn?.OrderBy(location => location.ToLocationString()).ToList(),
            };
    }

    private static DataSetQueryCriteriaGeographicLevels? NormaliseGeographicLevels(
        DataSetQueryCriteriaGeographicLevels? original
    )
    {
        return original == null
            ? null
            : original with
            {
                In = original.In?.Order().ToList(),
                NotIn = original.NotIn?.Order().ToList(),
            };
    }

    private static DataSetQueryCriteriaTimePeriods? NormaliseTimePeriods(DataSetQueryCriteriaTimePeriods? original)
    {
        return original == null
            ? null
            : original with
            {
                In = original
                    .In?.OrderBy(timePeriod => timePeriod.Code)
                    .ThenBy(timePeriod => timePeriod.Period)
                    .ToList(),

                NotIn = original
                    .NotIn?.OrderBy(timePeriod => timePeriod.Code)
                    .ThenBy(timePeriod => timePeriod.Period)
                    .ToList(),
            };
    }
}
