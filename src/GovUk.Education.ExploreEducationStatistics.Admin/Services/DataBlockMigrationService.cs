using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class DataBlockMigrationService : IDataBlockMigrationService
    {
        private readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
        private readonly ContentDbContext _context;
        private readonly ISubjectMetaService _subjectMetaService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        private readonly Regex _timePeriodRegex = new Regex("^[0-9]{4}_[A-Z0-9]{2,4}$");

        public DataBlockMigrationService(ContentDbContext context,
            ISubjectMetaService subjectMetaService,
            ILogger<DataBlockMigrationService> logger,
            IMapper mapper)
        {
            _context = context;
            _subjectMetaService = subjectMetaService;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<Either<ActionResult, bool>> MigrateAll()
        {
            var dataBlocks = _context.DataBlocks.ToList();
            var errors = new List<string>();

            var index = 1;
            var totalCount = dataBlocks.Count;
            foreach (var dataBlock in dataBlocks)
            {
                if (dataBlock.EES17DataBlockRequest == null)
                {
                    _logger.LogInformation("Skipping DataBlock, id: {DataBlockId}", dataBlock.Id);
                    break;
                }
                
                _context.DataBlocks.Update(dataBlock);

                var result = await Transform(dataBlock);

                if (result.IsLeft)
                {
                    _logger.LogInformation(
                        "Failed to transform DataBlock {Count} out of {TotalCount}, id: {DataBlockId}",
                        index, totalCount, dataBlock.Id);
                    errors.Add(result.Left);
                }
                else
                {
                    _logger.LogInformation("Transformed DataBlock {Count} out of {TotalCount}, id: {DataBlockId}",
                        index, totalCount, dataBlock.Id);
                }

                index++;
            }

            if (errors.Any())
            {
                var modelStateDictionary = new ModelStateDictionary();
                errors.ForEach(message => modelStateDictionary.AddModelError(string.Empty, message));
                return new BadRequestObjectResult(new ValidationProblemDetails(modelStateDictionary));
            }

            await _context.SaveChangesAsync();
            return true;
        }

        private Task<Either<string, bool>> Transform(DataBlock dataBlock)
        {
            dataBlock.DataBlockRequest = _mapper.Map<ObservationQueryContext>(dataBlock.EES17DataBlockRequest);
            return GetSubjectMeta(dataBlock.DataBlockRequest)
                .OnSuccess(subjectMeta =>
                {
                    var filters = GetFilterItemIds(subjectMeta);
                    var indicators = GetIndicatorItemIds(subjectMeta);
                    var locations = GetLocationIds(subjectMeta);
                    var timePeriods = GetTimePeriods(subjectMeta);

                    dataBlock.Tables = _mapper.Map<List<TableBuilderConfiguration>>(dataBlock.EES17Tables);
                    dataBlock.Tables.ForEach(table =>
                    {
                        var tableHeaders = table.TableHeaders;

                        tableHeaders.Columns = TransformTableHeaders(filters, indicators, locations, timePeriods,
                            tableHeaders.Columns);

                        tableHeaders.Rows = TransformTableHeaders(filters, indicators, locations, timePeriods,
                            tableHeaders.Rows);

                        tableHeaders.ColumnGroups = TransformTableHeadersGroup(filters, indicators, locations,
                            timePeriods,
                            tableHeaders.ColumnGroups);

                        tableHeaders.RowGroups = TransformTableHeadersGroup(filters, indicators, locations, timePeriods,
                            tableHeaders.RowGroups);
                    });

                    var notFound = dataBlock.Tables.SelectMany(table =>
                    {
                        var tableHeaders = table.TableHeaders;
                        var innerList = tableHeaders.Columns.Where(header => !header.Type.HasValue).ToList();
                        innerList.AddRange(tableHeaders.Rows.Where(header => !header.Type.HasValue));
                        innerList.AddRange(tableHeaders.ColumnGroups.SelectMany(headers => headers.Where(header => !header.Type.HasValue)));
                        innerList.AddRange(tableHeaders.RowGroups.SelectMany(headers => headers.Where(header => !header.Type.HasValue)));
                        return innerList;
                    }).ToList();

                    if (notFound.Any())
                    {
                        var valuesNotFound = string.Join(", ", notFound.Select(header => header.Value));
                        return new Either<string, bool>(
                            $"Table header values not found in Subject meta for DataBlock {dataBlock.Id}: {valuesNotFound}");
                    }

                    return true;
                })
                .OrElse(() => $"Failed to get Subject meta for query {dataBlock.DataBlockRequest}");
        }

        private List<List<TableHeader>> TransformTableHeadersGroup(IEnumerable<string> filterIds,
            IEnumerable<string> indicatorIds,
            IEnumerable<string> locationIds,
            IEnumerable<string> timePeriods,
            IEnumerable<IEnumerable<TableHeader>> group)
        {
            return group
                .Select(headers => TransformTableHeaders(filterIds, indicatorIds, locationIds, timePeriods, headers))
                .ToList();
        }

        private List<TableHeader> TransformTableHeaders(IEnumerable<string> filterIds,
            IEnumerable<string> indicatorIds,
            IEnumerable<string> locationIds,
            IEnumerable<string> timePeriods,
            IEnumerable<TableHeader> headers)
        {
            return headers
                .Select(header => PopulateTableHeaderType(filterIds, indicatorIds, locationIds, timePeriods, header))
                .ToList();
        }

        private TableHeader PopulateTableHeaderType(IEnumerable<string> filterIds,
            IEnumerable<string> indicatorIds,
            IEnumerable<string> locationIds,
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
            else if (locationIds.Contains(value))
            {
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

        private static List<string> GetLocationIds(SubjectMetaViewModel subjectMetaViewModel)
        {
            var locations = subjectMetaViewModel.Locations.Select(pair => pair.Value);
            return locations.SelectMany(GetValuesFromLocationGroups).ToList();
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
            return _cache.GetOrCreateAsync(query.SubjectId,
                entry => _subjectMetaService.GetSubjectMeta(query.SubjectId));
        }
    }
}