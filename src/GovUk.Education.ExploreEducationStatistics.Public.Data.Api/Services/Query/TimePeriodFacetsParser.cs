using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils;
using InterpolatedSql;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Query;

internal class TimePeriodFacetsParser : IFacetsParser
{
    private readonly QueryState _queryState;
    private readonly Dictionary<TimePeriodKey, ParquetTimePeriod> _allowedTimePeriods;

    public TimePeriodFacetsParser(QueryState queryState, IEnumerable<ParquetTimePeriod> timePeriods)
    {
        _queryState = queryState;
        _allowedTimePeriods = timePeriods.ToDictionary(timePeriod => new TimePeriodKey(timePeriod));
    }

    public IInterpolatedSql Parse(DataSetQueryCriteriaFacets facets, string path)
    {
        var fragments = new List<IInterpolatedSql>();

        if (facets.TimePeriods?.Eq is not null)
        {
            fragments.Add(
                EqFragment(
                    timePeriod: facets.TimePeriods.Eq,
                    path: QueryUtils.Path(path, "timePeriods.eq")
                )
            );
        }

        if (facets.TimePeriods?.NotEq is not null)
        {
            fragments.Add(
                EqFragment(
                    timePeriod: facets.TimePeriods.NotEq,
                    path: QueryUtils.Path(path, "timePeriods.notEq"),
                    negate: true
                )
            );
        }

        if (facets.TimePeriods?.In is not null && facets.TimePeriods.In.Count != 0)
        {
            fragments.Add(
                InFragment(
                    timePeriods: [.. facets.TimePeriods.In],
                    path: QueryUtils.Path(path, "timePeriods.in")
                )
            );
        }

        if (facets.TimePeriods?.NotIn is not null && facets.TimePeriods.NotIn.Count != 0)
        {
            fragments.Add(
                InFragment(
                    timePeriods: [.. facets.TimePeriods.NotIn],
                    path: QueryUtils.Path(path, "timePeriods.notIn"),
                    negate: true
                )
            );
        }

        if (facets.TimePeriods?.Gt is not null)
        {
            fragments.Add(
                ComparisonFragment(
                    timePeriod: facets.TimePeriods.Gt,
                    comparator: ">",
                    path: QueryUtils.Path(path, "timePeriods.gt")
                )
            );
        }

        if (facets.TimePeriods?.Gte is not null)
        {
            fragments.Add(
                ComparisonFragment(
                    timePeriod: facets.TimePeriods.Gte,
                    comparator: ">=",
                    path: QueryUtils.Path(path, "timePeriods.gte")
                )
            );
        }

        if (facets.TimePeriods?.Lt is not null)
        {
            fragments.Add(
                ComparisonFragment(
                    timePeriod: facets.TimePeriods.Lt,
                    comparator: "<",
                    path: QueryUtils.Path(path, "timePeriods.lt")
                )
            );
        }

        if (facets.TimePeriods?.Lte is not null)
        {
            fragments.Add(
                ComparisonFragment(
                    timePeriod: facets.TimePeriods.Lte,
                    comparator: "<=",
                    path: QueryUtils.Path(path, "timePeriods.lte")
                )
            );
        }

        return new DuckDbSqlBuilder()
            .AppendRange(fragments, "\nAND ")
            .Build();
    }

    private IInterpolatedSql EqFragment(
        DataSetQueryTimePeriod timePeriod,
        string path,
        bool negate = false)
    {
        var builder = new DuckDbSqlBuilder();

        if (!_allowedTimePeriods.TryGetValue(new TimePeriodKey(timePeriod), out var meta))
        {
            _queryState.Warnings.Add(CreateNotFoundWarning([timePeriod], path));

            return builder
                .AppendLiteral(negate ? "true" : "false")
                .Build();
        }

        builder += $"{DataTable.Ref().TimePeriodId:raw} {(negate ? "!=" : "="):raw} {meta.Id}";

        return builder.Build();
    }

    private IInterpolatedSql ComparisonFragment(
        DataSetQueryTimePeriod timePeriod,
        string comparator,
        string path)
    {
        var builder = new DuckDbSqlBuilder();

        if (!_allowedTimePeriods.TryGetValue(new TimePeriodKey(timePeriod), out var meta))
        {
            _queryState.Warnings.Add(CreateNotFoundWarning([timePeriod], path));

            return builder
                .AppendLiteral("false")
                .Build();
        }

        builder += $"{DataTable.Ref().TimePeriodId:raw} {comparator:raw} {meta.Id}";

        return builder.Build();
    }

    private IInterpolatedSql InFragment(
        HashSet<DataSetQueryTimePeriod> timePeriods,
        string path,
        bool negate = false)
    {
        var builder = new DuckDbSqlBuilder();

        var ids = timePeriods
            .Select(timePeriod => new TimePeriodKey(timePeriod))
            .Where(_allowedTimePeriods.ContainsKey)
            .Select(timePeriod => _allowedTimePeriods[timePeriod].Id)
            .ToList();

        if (ids.Count < timePeriods.Count)
        {
            _queryState.Warnings.Add(CreateNotFoundWarning(timePeriods, path));
        }

        if (ids.Count == 0)
        {
            return builder
                .AppendLiteral(negate ? "true" : "false")
                .Build();
        }

        builder += $"{DataTable.Ref().TimePeriodId:raw} {(negate ? "NOT IN" : "IN"):raw} ({ids})";

        return builder.Build();
    }

    private WarningViewModel CreateNotFoundWarning(HashSet<DataSetQueryTimePeriod> timePeriods, string path) => new()
    {
        Code = ValidationMessages.TimePeriodsNotFound.Code,
        Message = ValidationMessages.TimePeriodsNotFound.Message,
        Path = path,
        Detail = new NotFoundItemsErrorDetail<DataSetQueryTimePeriod>(
            timePeriods.Where(timePeriod => !_allowedTimePeriods.ContainsKey(new TimePeriodKey(timePeriod)))
        )
    };

    private record TimePeriodKey
    {
        public string Period { get; init; }

        public TimeIdentifier Identifier { get; init; }

        public TimePeriodKey(ParquetTimePeriod parquetTimePeriod)
        {
            Period = parquetTimePeriod.Period;
            Identifier = EnumUtil.GetFromEnumLabel<TimeIdentifier>(parquetTimePeriod.Identifier);
        }

        public TimePeriodKey(DataSetQueryTimePeriod timePeriod)
        {
            Period = TimePeriodFormatter.FormatToCsv(timePeriod.ParsedPeriod());
            Identifier = timePeriod.ParsedCode();
        }
    }
}
