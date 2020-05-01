using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Migrations.EES17;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Query.SubjectMetaQueryContext;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class EES17PermalinkMigrationService : IEES17PermalinkMigrationService
    {
        private readonly IPermalinkMigrationService _permalinkMigrationService;
        private readonly ISubjectMetaService _subjectMetaService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        private readonly Regex _timePeriodRegex = new Regex("^[0-9]{4}_[A-Z]{2,4}$");

        public EES17PermalinkMigrationService(IPermalinkMigrationService permalinkMigrationService,
            ISubjectMetaService subjectMetaService,
            ILogger<EES17PermalinkMigrationService> logger,
            IMapper mapper)
        {
            _permalinkMigrationService = permalinkMigrationService;
            _subjectMetaService = subjectMetaService;
            _logger = logger;
            _mapper = mapper;
        }

        public Task<Either<ActionResult, bool>> MigrateAll()
        {
            var migrationId = "EES-17_ChangeTableHeadersAndAddQueryLocation";
            return _permalinkMigrationService.MigrateAll<EES17Permalink>(migrationId, Transform);
        }

        private Task<Either<string, Permalink>> Transform(EES17Permalink source)
        {
            try
            {
                var permalink = _mapper.Map<Permalink>(source);
                return GetSubjectMeta(permalink.Query)
                    .OnSuccess(subjectMeta =>
                    {
                        var filters = GetFilterItemIds(subjectMeta);
                        var indicators = GetIndicatorItemIds(subjectMeta);
                        var locations = GetLocations(subjectMeta);
                        var timePeriods = GetTimePeriods(subjectMeta);

                        var tableHeaders = permalink.Configuration?.TableHeaders;

                        if (tableHeaders != null)
                        {
                            tableHeaders.Columns = TransformTableHeaders(filters, indicators, locations, timePeriods,
                                tableHeaders.Columns);

                            tableHeaders.Rows = TransformTableHeaders(filters, indicators, locations, timePeriods,
                                tableHeaders.Rows);

                            tableHeaders.ColumnGroups = TransformTableHeadersGroup(filters, indicators, locations,
                                timePeriods,
                                tableHeaders.ColumnGroups);

                            tableHeaders.RowGroups = TransformTableHeadersGroup(filters, indicators, locations,
                                timePeriods,
                                tableHeaders.RowGroups);

                            var notFound = tableHeaders.Columns.Where(header => !header.Type.HasValue).ToList();
                            notFound.AddRange(tableHeaders.Rows.Where(header => !header.Type.HasValue));
                            notFound.AddRange(tableHeaders.ColumnGroups.SelectMany(headers =>
                                headers.Where(header => !header.Type.HasValue)));
                            notFound.AddRange(tableHeaders.RowGroups.SelectMany(headers =>
                                headers.Where(header => !header.Type.HasValue)));

                            if (notFound.Any())
                            {
                                var valuesNotFound = string.Join(", ", notFound.Select(header => header.Value));
                                return new Either<string, Permalink>(
                                    $"Table header values not found in Subject meta for Permalink {permalink.Id}: {valuesNotFound}");
                            }
                        }

                        return permalink;
                    })
                    .OrElse(() => $"Failed to get Subject meta for query {permalink.Query}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception occured while transforming permalink {Id}", source.Id);
                throw;
            }
        }

        private List<List<TableHeader>> TransformTableHeadersGroup(IEnumerable<string> filterIds,
            IEnumerable<string> indicatorIds,
            Dictionary<string, IEnumerable<string>> locations,
            IEnumerable<string> timePeriods,
            IEnumerable<IEnumerable<TableHeader>> group)
        {
            return group
                .Select(headers => TransformTableHeaders(filterIds, indicatorIds, locations, timePeriods, headers))
                .ToList();
        }

        private List<TableHeader> TransformTableHeaders(IEnumerable<string> filterIds,
            IEnumerable<string> indicatorIds,
            Dictionary<string, IEnumerable<string>> locations,
            IEnumerable<string> timePeriods,
            IEnumerable<TableHeader> headers)
        {
            return headers
                .Select(header =>
                    PopulateTableHeaderLevelAndType(filterIds, indicatorIds, locations, timePeriods, header))
                .ToList();
        }

        private TableHeader PopulateTableHeaderLevelAndType(IEnumerable<string> filterIds,
            IEnumerable<string> indicatorIds,
            Dictionary<string, IEnumerable<string>> locations,
            IEnumerable<string> timePeriods,
            TableHeader tableHeader)
        {
            TableHeaderType? type = null;
            var value = tableHeader.Value;
            if (filterIds.Contains(value))
            {
                type = TableHeaderType.Filter;
            }
            else if (indicatorIds.Contains(value))
            {
                type = TableHeaderType.Indicator;
            }
            else if (locations.ContainsKey(value))
            {
                var levels = locations[value].ToList();
                if (levels.Count != 1)
                {
                    _logger.LogWarning(
                        "More than one geographic level found in meta data for location id: {Value}, levels: {Levels}",
                        value, string.Join(", ", levels));
                }
                tableHeader.Level = levels.First();
                type = TableHeaderType.Location;
            }
            else if (timePeriods.Contains(value))
            {
                type = TableHeaderType.TimePeriod;
            }
            else
            {
                if (_timePeriodRegex.Match(value).Success)
                {
                    type = TableHeaderType.TimePeriod;
                    _logger.LogWarning("Table header value not found but matched as Time Period: {Value}", value);
                }
            }

            tableHeader.Type = type;
            return tableHeader;
        }

        private static List<string> GetFilterItemIds(SubjectMetaViewModel subjectMetaViewModel)
        {
            var filters = subjectMetaViewModel.Filters.Select(pair => pair.Value);
            return filters.SelectMany(GetValuesFromFilterGroups).ToList();
        }

        private static List<string> GetIndicatorItemIds(SubjectMetaViewModel subjectMetaViewModel)
        {
            var indicators = subjectMetaViewModel.Indicators.Select(pair => pair.Value);
            return indicators.SelectMany(GetLabelOptionValues).ToList();
        }

        private static Dictionary<string, IEnumerable<string>> GetLocations(SubjectMetaViewModel subjectMetaViewModel)
        {
            return subjectMetaViewModel.Locations
                .SelectMany(pair => GetValuesFromLocationGroups(pair.Value)
                    .Select(s => new {LocationId = s, Level = pair.Key}))
                .GroupBy(tuple => tuple.LocationId)
                .ToDictionary(
                    grouping => grouping.Key,
                    grouping => grouping.Select(tuple => tuple.Level));
        }

        private static List<string> GetTimePeriods(SubjectMetaViewModel subjectMetaViewModel)
        {
            return subjectMetaViewModel.TimePeriod.Options
                .Select(option => $"{option.Year}_{option.Code.GetEnumValue()}")
                .ToList();
        }

        private static IEnumerable<string> GetValuesFromFilterGroups(FilterMetaViewModel legendOptions)
        {
            var groups = legendOptions.Options.Select(pair => pair.Value);
            return groups.SelectMany(GetLabelOptionValues);
        }

        private static IEnumerable<string> GetValuesFromLocationGroups(ObservationalUnitsMetaViewModel legendOptions)
        {
            return legendOptions.Options.Select(value => value.Value);
        }

        private static IEnumerable<string> GetLabelOptionValues(
            LabelOptionsMetaValueModel<IEnumerable<LabelValue>> labelOptions)
        {
            return labelOptions.Options.Select(value => value.Value);
        }

        private static IEnumerable<string> GetLabelOptionValues(
            LabelOptionsMetaValueModel<IEnumerable<IndicatorMetaViewModel>> labelOptions)
        {
            return labelOptions.Options.Select(model => model.Value);
        }

        private Task<Either<ActionResult, SubjectMetaViewModel>> GetSubjectMeta(ObservationQueryContext query)
        {
            return _subjectMetaService.GetSubjectMeta(FromObservationQueryContext(query));
        }
    }
}