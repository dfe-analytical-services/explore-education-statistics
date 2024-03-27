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
    private readonly Dictionary<TimePeriodKey, ParquetTimePeriod> _timePeriodMetas;

    public TimePeriodFacetsParser(QueryState queryState, IEnumerable<ParquetTimePeriod> timePeriods)
    {
        _queryState = queryState;
        _timePeriodMetas = timePeriods.ToDictionary(timePeriod => new TimePeriodKey(timePeriod));
    }

    public IInterpolatedSql Parse(DataSetQueryCriteriaFacets facets, string path)
    {
        var fragments = new List<IInterpolatedSql>();

        if (facets.TimePeriods?.Eq is not null)
        {
            fragments.Add(
                EqFragment(
                    facets.TimePeriods.Eq,
                    path: QueryUtils.Path(path, "timePeriods.eq")
                )
            );
        }

        if (facets.TimePeriods?.NotEq is not null)
        {
            fragments.Add(
                EqFragment(
                    facets.TimePeriods.NotEq,
                    path: QueryUtils.Path(path, "timePeriods.notEq"),
                    negate: true
                )
            );
        }

        if (facets.TimePeriods?.In is not null)
        {
            fragments.Add(
                InFragment(
                    facets.TimePeriods.In,
                    path: QueryUtils.Path(path, "timePeriods.in")
                )
            );
        }

        if (facets.TimePeriods?.NotIn is not null)
        {
            fragments.Add(
                InFragment(
                    facets.TimePeriods.NotIn,
                    path: QueryUtils.Path(path, "timePeriods.notIn"),
                    negate: true
                )
            );
        }

        if (facets.TimePeriods?.Gt is not null)
        {
            fragments.Add(
                ComparisonFragment(
                    facets.TimePeriods.Gt,
                    comparator: ">",
                    path: QueryUtils.Path(path, "timePeriods.gt")
                )
            );
        }

        if (facets.TimePeriods?.Gte is not null)
        {
            fragments.Add(
                ComparisonFragment(
                    facets.TimePeriods.Gte,
                    comparator: ">=",
                    path: QueryUtils.Path(path, "timePeriods.gte")
                )
            );
        }

        if (facets.TimePeriods?.Lt is not null)
        {
            fragments.Add(
                ComparisonFragment(
                    facets.TimePeriods.Lt,
                    comparator: "<",
                    path: QueryUtils.Path(path, "timePeriods.lt")
                )
            );
        }

        if (facets.TimePeriods?.Lte is not null)
        {
            fragments.Add(
                ComparisonFragment(
                    facets.TimePeriods.Lte,
                    comparator: "<=",
                    path: QueryUtils.Path(path, "timePeriods.lte")
                )
            );
        }

        return new DuckDbSqlBuilder()
            .AppendRange(fragments, " AND ")
            .Build();
    }

    private IInterpolatedSql EqFragment(
        DataSetQueryTimePeriod timePeriod,
        string path,
        bool negate = false)
    {
        var builder = new DuckDbSqlBuilder();

        if (!_timePeriodMetas.TryGetValue(new TimePeriodKey(timePeriod), out var meta))
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

        if (!_timePeriodMetas.TryGetValue(new TimePeriodKey(timePeriod), out var meta))
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
        IReadOnlyList<DataSetQueryTimePeriod> timePeriods,
        string path,
        bool negate = false)
    {
        var builder = new DuckDbSqlBuilder();

        var ids = timePeriods
            .Distinct()
            .Select(timePeriod => new TimePeriodKey(timePeriod))
            .Where(_timePeriodMetas.ContainsKey)
            .Select(timePeriod => _timePeriodMetas[timePeriod].Id)
            .ToList();

        if (ids.Count < timePeriods.Count)
        {
            _queryState.Warnings.Add(CreateNotFoundWarning([..timePeriods], path));
        }

        if (ids.Count == 0)
        {
            return builder
                .AppendLiteral(negate ? "true" : "false")
                .Build();
        }

        if (negate)
        {
            builder += $"{DataTable.Ref().TimePeriodId:raw} NOT IN ({ids})";
        }
        else
        {
            builder += $"{DataTable.Ref().TimePeriodId:raw} IN ({ids})";
        }

        return builder.Build();
    }

    private WarningViewModel CreateNotFoundWarning(HashSet<DataSetQueryTimePeriod> timePeriods, string path) => new()
    {
        Code = ValidationMessages.TimePeriodsNotFound.Code,
        Message = ValidationMessages.TimePeriodsNotFound.Message,
        Path = path,
        Detail = new NotFoundItemsErrorDetail<DataSetQueryTimePeriod>(
            timePeriods.Where(timePeriod => !_timePeriodMetas.ContainsKey(new TimePeriodKey(timePeriod)))
        )
    };

    private record TimePeriodKey
    {
        private string Period { get; init; }

        private TimeIdentifier Identifier { get; init; }

        public TimePeriodKey(ParquetTimePeriod parquetTimePeriod)
        {
            Period = parquetTimePeriod.Period;
            Identifier = EnumUtil.GetFromEnumLabel<TimeIdentifier>(parquetTimePeriod.Identifier);
        }

        public TimePeriodKey(DataSetQueryTimePeriod timePeriod)
        {
            Period = TimePeriodFormatter.FormatToCsv(timePeriod.ParsedPeriod);
            Identifier = timePeriod.ParsedCode;
        }
    }
}
