using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Exceptions;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Utils;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class ImporterService : IImporterService
    {
        private readonly IGuidGenerator _guidGenerator;
        private readonly ImporterLocationService _importerLocationService;
        private readonly ImporterFilterService _importerFilterService;
        private readonly IImporterMetaService _importerMetaService;
        private readonly IDataImportService _dataImportService;
        private readonly ILogger<ImporterService> _logger;

        private const int Stage2RowCheck = 1000;

        private static readonly Dictionary<GeographicLevel, string[]> ColumnValues =
            new Dictionary<GeographicLevel, string[]>
            {
                {
                    GeographicLevel.Country,
                    new[] {"country_code", "country_name"}
                },
                {
                    GeographicLevel.EnglishDevolvedArea,
                    new[] {"english_devolved_area_code", "english_devolved_area_name"}
                },
                {
                    GeographicLevel.Institution,
                    new[] {"institution_id", "institution_name"}
                },
                {
                    GeographicLevel.LocalAuthority,
                    new[] {"new_la_code", "old_la_code", "la_name"}
                },
                {
                    GeographicLevel.LocalAuthorityDistrict,
                    new[] {"lad_code", "lad_name"}
                },
                {
                    GeographicLevel.LocalEnterprisePartnership,
                    new[] {"local_enterprise_partnership_code", "local_enterprise_partnership_name"}
                },
                {
                    GeographicLevel.MayoralCombinedAuthority,
                    new[] {"mayoral_combined_authority_code", "mayoral_combined_authority_name"}
                },
                {
                    GeographicLevel.MultiAcademyTrust,
                    new[] {"trust_id", "trust_name"}
                },
                {
                    GeographicLevel.OpportunityArea,
                    new[] {"opportunity_area_code", "opportunity_area_name"}
                },
                {
                    GeographicLevel.ParliamentaryConstituency,
                    new[] {"pcon_code", "pcon_name"}
                },
                {
                    GeographicLevel.Region,
                    new[] {"region_code", "region_name"}
                },
                {
                    GeographicLevel.Sponsor,
                    new[] {"sponsor_id", "sponsor_name"}
                },
                {
                    GeographicLevel.Ward,
                    new[] {"ward_code", "ward_name"}
                },
                {
                    GeographicLevel.PlanningArea,
                    new[] {"planning_area_code", "planning_area_name"}
                }
            };

        public static readonly List<GeographicLevel> IgnoredGeographicLevels = new List<GeographicLevel>
        {
            GeographicLevel.Institution,
            GeographicLevel.Provider,
            GeographicLevel.School,
            GeographicLevel.PlanningArea
        };

        public ImporterService(
            IGuidGenerator guidGenerator,
            ImporterFilterService importerFilterService,
            ImporterLocationService importerLocationService,
            IImporterMetaService importerMetaService,
            IDataImportService dataImportService, 
            ILogger<ImporterService> logger)
        {
            _guidGenerator = guidGenerator;
            _importerFilterService = importerFilterService;
            _importerLocationService = importerLocationService;
            _importerMetaService = importerMetaService;
            _dataImportService = dataImportService;
            _logger = logger;
        }

        public void ImportMeta(DataTable table, Subject subject, StatisticsDbContext context)
        {
            _importerMetaService.Import(table.Columns, table.Rows, subject, context);
        }

        public SubjectMeta GetMeta(DataTable table, Subject subject, StatisticsDbContext context)
        {
            return _importerMetaService.Get(table.Columns, table.Rows, subject, context);
        }

        public async Task ImportFiltersAndLocations(DataImport dataImport,
            DataColumnCollection cols,
            DataRowCollection rows,
            SubjectMeta subjectMeta,
            StatisticsDbContext context)
        {
            // Clearing the caches is required here as the seeder shares the cache with all subjects
            _importerFilterService.ClearCache();
            _importerLocationService.ClearCache();

            var headers = CsvUtil.GetColumnValues(cols);
            var rowCount = 1;
            var totalRows = rows.Count;

            foreach (DataRow row in rows)
            {
                if (rowCount % Stage2RowCheck == 0)
                {
                    var currentStatus = await _dataImportService.GetImportStatus(dataImport.Id);

                    if (currentStatus.IsFinishedOrAborting())
                    {
                        _logger.LogInformation($"Import for {dataImport.File.Filename} has finished or is being aborted, " +
                                               "so finishing importing Filters and Locations early");
                        return;
                    }
                    
                    await _dataImportService.UpdateStatus(dataImport.Id,
                        DataImportStatus.STAGE_2,
                        (double) rowCount / totalRows * 100);
                }

                CreateFiltersAndLocationsFromCsv(context, CsvUtil.GetRowValues(row), headers, subjectMeta.Filters);
                rowCount++;
            }
        }

        public async Task ImportObservations(DataColumnCollection cols, DataRowCollection rows, Subject subject,
            SubjectMeta subjectMeta, int batchNo, int rowsPerBatch, StatisticsDbContext context)
        {
            _importerFilterService.ClearCache();
            _importerLocationService.ClearCache();

            var observations = GetObservations(
                context,
                rows,
                CsvUtil.GetColumnValues(cols),
                subject,
                subjectMeta,
                batchNo,
                rowsPerBatch).ToList();

            await InsertObservations(context, observations);
        }

        public GeographicLevel GetGeographicLevel(IReadOnlyList<string> line, List<string> headers)
        {
            return GetGeographicLevelFromString(CsvUtil.Value(line, headers, "geographic_level"));
        }

        private static GeographicLevel GetGeographicLevelFromString(string value)
        {
            foreach (GeographicLevel val in Enum.GetValues(typeof(GeographicLevel)))
            {
                if (val.GetEnumLabel().ToLower().Equals(value.ToLower()))
                {
                    return val;
                }
            }

            throw new InvalidGeographicLevelException(value);
        }

        public TimeIdentifier GetTimeIdentifier(IReadOnlyList<string> line, List<string> headers)
        {
            var timeIdentifier = CsvUtil.Value(line, headers, "time_identifier").ToLower();
            foreach (var value in Enum.GetValues(typeof(TimeIdentifier)).Cast<TimeIdentifier>())
            {
                if (value.GetEnumLabel().Equals(timeIdentifier, StringComparison.InvariantCultureIgnoreCase))
                {
                    return value;
                }
            }

            throw new InvalidTimeIdentifierException(timeIdentifier);
        }

        public int GetYear(IReadOnlyList<string> line, List<string> headers)
        {
            var tp = CsvUtil.Value(line, headers, "time_period");
            if (tp == null)
            {
                throw new InvalidTimePeriodException(null);
            }

            return int.Parse(tp.Substring(0, 4));
        }

        private IEnumerable<Observation> GetObservations(
            StatisticsDbContext context,
            DataRowCollection rows,
            List<string> headers,
            Subject subject,
            SubjectMeta subjectMeta,
            int batchNo,
            int rowsPerBatch
        )
        {
            var observations = new List<Observation>();
            var i = 0;

            foreach (DataRow row in rows)
            {
                var o = ObservationFromCsv(
                    context,
                    CsvUtil.GetRowValues(row).ToArray(),
                    headers,
                    subject,
                    subjectMeta,
                    ((batchNo - 1) * rowsPerBatch) + i++ + 2);

                if (!IgnoredGeographicLevels.Contains(o.GeographicLevel))
                {
                    observations.Add(o);
                }
            }

            return observations;
        }

        private Observation ObservationFromCsv(
            StatisticsDbContext context,
            string[] line,
            List<string> headers,
            Subject subject,
            SubjectMeta subjectMeta,
            int csvRowNum)
        {
            var observationId = _guidGenerator.NewGuid();

            return new Observation
            {
                Id = observationId,
                FilterItems = GetFilterItems(context, line, headers, subjectMeta.Filters, observationId),
                GeographicLevel = GetGeographicLevel(line, headers),
                LocationId = GetLocationId(line, headers, context),
                Measures = GetMeasures(line, headers, subjectMeta.Indicators),
                SubjectId = subject.Id,
                TimeIdentifier = GetTimeIdentifier(line, headers),
                Year = GetYear(line, headers),
                CsvRow = csvRowNum
            };
        }

        private void CreateFiltersAndLocationsFromCsv(
            StatisticsDbContext context,
            List<string> row,
            List<string> headers,
            IEnumerable<(Filter Filter, string Column, string FilterGroupingColumn)> filtersMeta)
        {
            CreateFilterItems(context, row, headers, filtersMeta);
            GetLocationId(row, headers, context);
        }

        private void CreateFilterItems(
            StatisticsDbContext context,
            IReadOnlyList<string> line,
            List<string> headers,
            IEnumerable<(Filter Filter, string Column, string FilterGroupingColumn)> filtersMeta)
        {
            foreach (var filterMeta in filtersMeta)
            {
                var filterItemLabel = CsvUtil.Value(line, headers, filterMeta.Column);
                var filterGroupLabel = CsvUtil.Value(line, headers, filterMeta.FilterGroupingColumn);

                _importerFilterService.Find(filterItemLabel, filterGroupLabel, filterMeta.Filter, context);
            }
        }

        private ICollection<ObservationFilterItem> GetFilterItems(
            StatisticsDbContext context,
            IReadOnlyList<string> line,
            List<string> headers,
            IEnumerable<(Filter Filter, string Column, string FilterGroupingColumn)> filtersMeta,
            Guid observationId)
        {
            return filtersMeta.Select(filterMeta =>
            {
                var filterItemLabel = CsvUtil.Value(line, headers, filterMeta.Column);
                var filterGroupLabel = CsvUtil.Value(line, headers, filterMeta.FilterGroupingColumn);

                return new ObservationFilterItem
                {
                    ObservationId = observationId,
                    FilterItemId = _importerFilterService
                        .Find(filterItemLabel, filterGroupLabel, filterMeta.Filter, context).Id,
                    FilterId = filterMeta.Filter.Id
                };
            }).ToList();
        }

        private Guid GetLocationId(IReadOnlyList<string> line, List<string> headers, StatisticsDbContext context)
        {
            return _importerLocationService.Find(
                context,
                GetCountry(line, headers),
                GetEnglishDevolvedArea(line, headers),
                GetInstitution(line, headers),
                GetLocalAuthority(line, headers),
                GetLocalAuthorityDistrict(line, headers),
                GetLocalEnterprisePartnership(line, headers),
                GetMayoralCombinedAuthority(line, headers),
                GetMultiAcademyTrust(line, headers),
                GetOpportunityArea(line, headers),
                GetParliamentaryConstituency(line, headers),
                GetRegion(line, headers),
                GetRscRegion(line, headers),
                GetSponsor(line, headers),
                GetWard(line, headers),
                GetPlanningArea(line, headers)
            ).Id;
        }

        private static Dictionary<Guid, string> GetMeasures(IReadOnlyList<string> line,
            List<string> headers, IEnumerable<(Indicator Indicator, string Column)> indicators)
        {
            var valueTuples = indicators.ToList();
            var columns = valueTuples.Select(tuple => tuple.Column);
            var values = CsvUtil.Values(line, headers, columns);

            return valueTuples.Zip(values, (tuple, value) => new {tuple, value})
                .ToDictionary(item => item.tuple.Indicator.Id, item => item.value);
        }

        private static Country GetCountry(IReadOnlyList<string> line, List<string> headers)
        {
            return CsvUtil.BuildType(line, headers, ColumnValues[GeographicLevel.Country], values =>
                new Country(values[0], values[1]));
        }

        private static EnglishDevolvedArea GetEnglishDevolvedArea(IReadOnlyList<string> line, List<string> headers)
        {
            return CsvUtil.BuildType(line, headers, ColumnValues[GeographicLevel.EnglishDevolvedArea], values =>
                new EnglishDevolvedArea(values[0], values[1]));
        }

        private static Institution GetInstitution(IReadOnlyList<string> line,
            List<string> headers)
        {
            return CsvUtil.BuildType(line, headers, ColumnValues[GeographicLevel.Institution], values =>
                new Institution(values[0], values[1]));
        }

        private static LocalAuthority GetLocalAuthority(IReadOnlyList<string> line, List<string> headers)
        {
            return CsvUtil.BuildType(line, headers, ColumnValues[GeographicLevel.LocalAuthority], values =>
                new LocalAuthority(values[0], values[1], values[2]));
        }

        private static LocalAuthorityDistrict GetLocalAuthorityDistrict(IReadOnlyList<string> line,
            List<string> headers)
        {
            return CsvUtil.BuildType(line, headers, ColumnValues[GeographicLevel.LocalAuthorityDistrict], values =>
                new LocalAuthorityDistrict(values[0], values[1]));
        }

        private static LocalEnterprisePartnership GetLocalEnterprisePartnership(IReadOnlyList<string> line,
            List<string> headers)
        {
            return CsvUtil.BuildType(line, headers, ColumnValues[GeographicLevel.LocalEnterprisePartnership], values =>
                new LocalEnterprisePartnership(values[0], values[1]));
        }

        private static MayoralCombinedAuthority GetMayoralCombinedAuthority(IReadOnlyList<string> line,
            List<string> headers)
        {
            return CsvUtil.BuildType(line, headers, ColumnValues[GeographicLevel.MayoralCombinedAuthority], values =>
                new MayoralCombinedAuthority(values[0], values[1]));
        }

        private static Mat GetMultiAcademyTrust(IReadOnlyList<string> line, List<string> headers)
        {
            return CsvUtil.BuildType(line, headers, ColumnValues[GeographicLevel.MultiAcademyTrust], values =>
                new Mat(values[0], values[1]));
        }

        private static OpportunityArea GetOpportunityArea(IReadOnlyList<string> line, List<string> headers)
        {
            return CsvUtil.BuildType(line, headers, ColumnValues[GeographicLevel.OpportunityArea], values =>
                new OpportunityArea(values[0], values[1]));
        }

        private static ParliamentaryConstituency GetParliamentaryConstituency(IReadOnlyList<string> line,
            List<string> headers)
        {
            return CsvUtil.BuildType(line, headers, ColumnValues[GeographicLevel.ParliamentaryConstituency], values =>
                new ParliamentaryConstituency(values[0], values[1]));
        }

        private static Region GetRegion(IReadOnlyList<string> line, List<string> headers)
        {
            return CsvUtil.BuildType(line, headers, ColumnValues[GeographicLevel.Region], values =>
                new Region(values[0], values[1]));
        }

        private static RscRegion GetRscRegion(IReadOnlyList<string> line, List<string> headers)
        {
            return CsvUtil.BuildType(line, headers, "rsc_region_lead_name", value => new RscRegion(value));
        }

        private static Sponsor GetSponsor(IReadOnlyList<string> line, List<string> headers)
        {
            return CsvUtil.BuildType(line, headers, ColumnValues[GeographicLevel.Sponsor], values =>
                new Sponsor(values[0], values[1]));
        }

        private static Ward GetWard(IReadOnlyList<string> line, List<string> headers)
        {
            return CsvUtil.BuildType(line, headers, ColumnValues[GeographicLevel.Ward], values =>
                new Ward(values[0], values[1]));
        }

        private static PlanningArea GetPlanningArea(IReadOnlyList<string> line, List<string> headers)
        {
            return CsvUtil.BuildType(line, headers, ColumnValues[GeographicLevel.PlanningArea], values =>
                new PlanningArea(values[0], values[1]));
        }

        private async Task InsertObservations(DbContext context, IEnumerable<Observation> observations)
        {
            var observationsTable = new DataTable();
            observationsTable.Columns.Add("Id", typeof(Guid));
            observationsTable.Columns.Add("SubjectId", typeof(Guid));
            observationsTable.Columns.Add("GeographicLevel", typeof(string));
            observationsTable.Columns.Add("LocationId", typeof(Guid));
            observationsTable.Columns.Add("Year", typeof(int));
            observationsTable.Columns.Add("TimeIdentifier", typeof(string));
            observationsTable.Columns.Add("Measures", typeof(string));
            observationsTable.Columns.Add("CsvRow", typeof(long));

            var observationsFilterItemsTable = new DataTable();
            observationsFilterItemsTable.Columns.Add("ObservationId", typeof(Guid));
            observationsFilterItemsTable.Columns.Add("FilterItemId", typeof(Guid));
            observationsFilterItemsTable.Columns.Add("FilterId", typeof(Guid));

            foreach (var o in observations)
            {
                observationsTable.Rows.Add(
                    o.Id,
                    o.SubjectId,
                    o.GeographicLevel.GetEnumValue(),
                    o.LocationId,
                    o.Year,
                    o.TimeIdentifier.GetEnumValue(),
                    "{" + string.Join(",", o.Measures.Select(x => $"\"{x.Key}\":\"{x.Value}\"")) + "}",
                    o.CsvRow
                );

                foreach (var item in o.FilterItems)
                {
                    observationsFilterItemsTable.Rows.Add(
                        item.ObservationId,
                        item.FilterItemId,
                        item.FilterId
                    );
                }
            }

            var parameter = new SqlParameter("@Observations", SqlDbType.Structured)
            {
                Value = observationsTable, TypeName = "[dbo].[ObservationType]"
            };

            await context.Database.ExecuteSqlRawAsync("EXEC [dbo].[InsertObservations] @Observations", parameter);

            parameter = new SqlParameter("@ObservationFilterItems", SqlDbType.Structured)
            {
                Value = observationsFilterItemsTable, TypeName = "[dbo].[ObservationFilterItemType]"
            };

            await context.Database.ExecuteSqlRawAsync(
                "EXEC [dbo].[InsertObservationFilterItems] @ObservationFilterItems", parameter);
        }
    }
}
