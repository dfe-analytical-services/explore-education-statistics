using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;
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

        var parsedEq = facets.GeographicLevels?.ParsedEq();

        if (parsedEq is not null)
        {
            fragments.Add(
                EqFragment(
                    geographicLevel: parsedEq.Value,
                    path: QueryUtils.Path(path, "geographicLevels.eq")
                )
            );
        }

        var parsedNotEq = facets.GeographicLevels?.ParsedNotEq();

        if (parsedNotEq is not null)
        {
            fragments.Add(
                EqFragment(
                    geographicLevel: parsedNotEq.Value,
                    path: QueryUtils.Path(path, "geographicLevels.notEq"),
                    negate: true
                )
            );
        }

        var parsedIn = facets.GeographicLevels?.ParsedIn();

        if (parsedIn is not null && parsedIn.Count != 0)
        {
            fragments.Add(
                InFragment(
                    geographicLevels: parsedIn,
                    path: QueryUtils.Path(path, "geographicLevels.in")
                )
            );
        }

        var parsedNotIn = facets.GeographicLevels?.ParsedNotIn();

        if (parsedNotIn is not null && parsedNotIn.Count != 0)
        {
            fragments.Add(
                InFragment(
                    geographicLevels: parsedNotIn,
                    path: QueryUtils.Path(path, "geographicLevels.notIn"),
                    negate: true
                )
            );
        }

        return new DuckDbSqlBuilder()
            .AppendRange(fragments, "\nAND ")
            .Build();
    }

    private IInterpolatedSql EqFragment(
        GeographicLevel geographicLevel,
        string path,
        bool negate = false)
    {
        var builder = new DuckDbSqlBuilder();

        if (!allowedGeographicLevels.Contains(geographicLevel))
        {
            queryState.Warnings.Add(CreateNotFoundWarning([geographicLevel], path));

            return builder
                .AppendLiteral(negate ? "true" : "false")
                .Build();
        }

        builder +=
            $"{DataTable.Ref().GeographicLevel:raw} {(negate ? "!=" : "="):raw} {geographicLevel.GetEnumLabel()}";

        return builder.Build();
    }

    private IInterpolatedSql InFragment(
        IReadOnlyList<GeographicLevel> geographicLevels,
        string path,
        bool negate = false)
    {
        var builder = new DuckDbSqlBuilder();

        var levels = geographicLevels
            .Where(allowedGeographicLevels.Contains)
            .ToHashSet();

        if (levels.Count < geographicLevels.Count)
        {
            queryState.Warnings.Add(CreateNotFoundWarning(geographicLevels, path));
        }

        if (levels.Count == 0)
        {
            return builder
                .AppendLiteral(negate ? "true" : "false")
                .Build();
        }

        var parameters = levels
            .Select(level => level.GetEnumLabel())
            .ToList();

        builder += $"{DataTable.Ref().GeographicLevel:raw} {(negate ? "NOT IN" : "IN"):raw} ({parameters})";

        return builder.Build();
    }

    private WarningViewModel CreateNotFoundWarning(IEnumerable<GeographicLevel> geographicLevels, string path) => new()
    {
        Code = ValidationMessages.GeographicLevelsNotFound.Code,
        Message = ValidationMessages.GeographicLevelsNotFound.Message,
        Path = path,
        Detail = new NotFoundItemsErrorDetail<string>(
            geographicLevels
                .Where(level => !allowedGeographicLevels.Contains(level))
                .Select(level => level.GetEnumValue())
                .ToList()
        )
    };
}
