using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
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

internal class FilterFacetsParser : IFacetsParser
{
    private readonly QueryState _queryState;
    private readonly Dictionary<string, ParquetFilterOption> _allowedFilterOptionsByPublicId;

    public FilterFacetsParser(QueryState queryState, IEnumerable<ParquetFilterOption> filterOptions)
    {
        _queryState = queryState;
        _allowedFilterOptionsByPublicId = filterOptions.ToDictionary(o => o.PublicId);
    }

    public IInterpolatedSql Parse(DataSetQueryCriteriaFacets facets, string path)
    {
        var fragments = new List<IInterpolatedSql>();

        if (facets.Filters?.Eq is not null)
        {
            fragments.Add(
                EqFragment(
                    filterOptionId: facets.Filters.Eq,
                    path: QueryUtils.Path(path, "filters.eq")
                )
            );
        }

        if (facets.Filters?.NotEq is not null)
        {
            fragments.Add(
                EqFragment(
                    filterOptionId: facets.Filters.NotEq,
                    path: QueryUtils.Path(path, "filters.notEq"),
                    negate: true
                )
            );
        }

        if (facets.Filters?.In is not null && facets.Filters.In.Count != 0)
        {
            fragments.Add(
                InFragment(
                    filterOptionIds: [.. facets.Filters.In],
                    path: QueryUtils.Path(path, "filters.in")
                )
            );
        }

        if (facets.Filters?.NotIn is not null && facets.Filters.NotIn.Count != 0)
        {
            fragments.Add(
                InFragment(
                    filterOptionIds: [.. facets.Filters.NotIn],
                    path: QueryUtils.Path(path, "filters.notIn"),
                    negate: true
                )
            );
        }

        return new DuckDbSqlBuilder().AppendRange(fragments, "\nAND ").Build();
    }

    private IInterpolatedSql EqFragment(string filterOptionId, string path, bool negate = false)
    {
        var builder = new DuckDbSqlBuilder();

        if (!_allowedFilterOptionsByPublicId.TryGetValue(filterOptionId, out var option))
        {
            _queryState.Warnings.Add(CreateNotFoundWarning([filterOptionId], path));

            return builder.AppendLiteral(negate ? "true" : "false").Build();
        }

        builder +=
            $"{DataTable.Ref().Col(option.FilterColumn):raw} {(negate ? "!=" : "="):raw} {option.Id}";

        return builder.Build();
    }

    private IInterpolatedSql InFragment(
        HashSet<string> filterOptionIds,
        string path,
        bool negate = false
    )
    {
        var builder = new DuckDbSqlBuilder();

        var options = filterOptionIds
            .Where(_allowedFilterOptionsByPublicId.ContainsKey)
            .Select(id => _allowedFilterOptionsByPublicId[id])
            .ToList();

        if (options.Count < filterOptionIds.Count)
        {
            _queryState.Warnings.Add(CreateNotFoundWarning(filterOptionIds, path));
        }

        if (options.Count == 0)
        {
            return builder.AppendLiteral(negate ? "true" : "false").Build();
        }

        var fragments = options
            .GroupBy(o => o.FilterColumn)
            .Select(group =>
            {
                var ids = group.Select(o => o.Id);

                return (FormattableString)
                    $"{DataTable.Ref().Col(group.Key):raw} {(negate ? "NOT IN" : "IN"):raw} ({ids})";
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

    private WarningViewModel CreateNotFoundWarning(HashSet<string> filterOptionIds, string path) =>
        new()
        {
            Code = ValidationMessages.FiltersNotFound.Code,
            Message = ValidationMessages.FiltersNotFound.Message,
            Path = path,
            Detail = new NotFoundItemsErrorDetail<string>(
                filterOptionIds.Where(optionId =>
                    !_allowedFilterOptionsByPublicId.ContainsKey(optionId)
                )
            ),
        };
}
