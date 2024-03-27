using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;
using InterpolatedSql;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Query;

internal class GeographicLevelFacetsParser(
    QueryState queryState,
    HashSet<GeographicLevel> allowedGeographicLevels)
    : IFacetsParser
{
    public IInterpolatedSql Parse(DataSetQueryCriteriaFacets facets, string path)
    {
        var fragments = new List<IInterpolatedSql>();

        if (facets.GeographicLevels?.ParsedEq is not null)
        {
            fragments.Add(
                EqFragment(
                    geographicLevel: facets.GeographicLevels.ParsedEq.Value,
                    path: "geographicLevels.eq"
                )
            );
        }

        if (facets.GeographicLevels?.ParsedNotEq is not null)
        {
            fragments.Add(
                EqFragment(
                    geographicLevel: facets.GeographicLevels.ParsedNotEq.Value,
                    path: "geographicLevels.notEq",
                    negate: true
                )
            );
        }

        if (facets.GeographicLevels?.ParsedIn is not null)
        {
            fragments.Add(
                InFragment(
                    geographicLevels: facets.GeographicLevels.ParsedIn,
                    path: "geographicLevels.in"
                )
            );
        }

        if (facets.GeographicLevels?.ParsedNotIn is not null)
        {
            fragments.Add(
                InFragment(
                    geographicLevels: facets.GeographicLevels.ParsedNotIn,
                    path: "geographicLevels.notIn",
                    negate: true
                )
            );
        }

        return new DuckDbSqlBuilder()
            .AppendRange(fragments, " AND ")
            .Build();
    }

    private IInterpolatedSql EqFragment(
        GeographicLevel geographicLevel,
        string path,
        bool negate = false)
    {
        if (!allowedGeographicLevels.Contains(geographicLevel))
        {
            queryState.Warnings.Add(CreateNotFoundWarning([geographicLevel], path));
        }

        var builder = new DuckDbSqlBuilder(
            $"{DataTable.Ref().GeographicLevel:raw} {(negate ? "!=" : "="):raw} {geographicLevel.GetEnumLabel()}");

        return builder.Build();
    }

    private IInterpolatedSql InFragment(
        IReadOnlyList<GeographicLevel> geographicLevels,
        string path,
        bool negate = true)
    {
        var builder = new DuckDbSqlBuilder();

        var levels = geographicLevels
            .Where(allowedGeographicLevels.Contains)
            .ToHashSet();

        if (levels.Count < geographicLevels.Count)
        {
            queryState.Warnings.Add(CreateNotFoundWarning(levels, path));
        }

        if (levels.Count == 0)
        {
            return builder
                .AppendLiteral(negate ? "true" : "false")
                .Build();
        }

        var parameters = levels
            .Select(level => level.GetEnumLabel())
            .Cast<object>()
            .ToList();

        builder +=
            $"{DataTable.Ref().GeographicLevel:raw} {(negate ? "NOT IN" : "IN"):raw} ({parameters})";

        return builder.Build();
    }

    private WarningViewModel CreateNotFoundWarning(HashSet<GeographicLevel> geographicLevels, string path) => new()
    {
        Code = ValidationMessages.GeographicLevelsNotFound.Code,
        Message = ValidationMessages.GeographicLevelsNotFound.Message,
        Path = path,
        Detail = new NotFoundItemsErrorDetail<string>(
            geographicLevels
                .Where(level => !allowedGeographicLevels.Contains(level))
                .Select(level => level.GetEnumValue())
        )
    };
}
