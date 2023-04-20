#nullable enable
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static System.StringComparison;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.DataImportStatus;

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
        private readonly IDatabaseHelper _databaseHelper;
        private readonly ImporterFilterCache _importerFilterCache;
        private readonly IObservationBatchImporter _observationBatchImporter;

        private const int Stage2RowCheck = 1000;

        private static readonly Dictionary<GeographicLevel, string[]> ColumnValues =
            new()
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
                    GeographicLevel.Provider,
                    new[] {"provider_ukprn", "provider_name"}
                },
                {
                    GeographicLevel.Region,
                    new[] {"region_code", "region_name"}
                },
                {
                    GeographicLevel.School,
                    new[] {"school_urn", "school_name"}
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

        public ImporterService(
            IGuidGenerator guidGenerator,
            ImporterFilterService importerFilterService,
            ImporterLocationService importerLocationService,
            IImporterMetaService importerMetaService,
            IDataImportService dataImportService, 
            ILogger<ImporterService> logger, 
            IDatabaseHelper databaseHelper, 
            ImporterFilterCache importerFilterCache, IObservationBatchImporter? observationBatchImporter = null)
        {
            _guidGenerator = guidGenerator;
            _importerFilterService = importerFilterService;
            _importerLocationService = importerLocationService;
            _importerMetaService = importerMetaService;
            _dataImportService = dataImportService;
            _logger = logger;
            _databaseHelper = databaseHelper;
            _importerFilterCache = importerFilterCache;
            _observationBatchImporter = observationBatchImporter ?? new StoredProcedureObservationBatchImporter();
        }

        public Task<SubjectMeta> ImportMeta(
            List<string> metaFileCsvHeaders,
            List<List<string>> metaFileRows, 
            Subject subject,
            StatisticsDbContext context)
        {
            return _importerMetaService.Import(metaFileCsvHeaders, metaFileRows, subject, context);
        }

        public SubjectMeta GetMeta(
            List<string> metaFileCsvHeaders,
            List<List<string>> metaFileRows, 
            Subject subject,
            StatisticsDbContext context)
        {
            return _importerMetaService.Get(metaFileCsvHeaders, metaFileRows, subject, context);
        }

        private record FilterItemMeta(Guid FilterId, string FilterGroupLabel, string FilterItemLabel)
        {
            public virtual bool Equals(FilterItemMeta? other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return FilterId.Equals(other.FilterId) 
                       && string.Equals(FilterGroupLabel, other.FilterGroupLabel, CurrentCultureIgnoreCase) 
                       && string.Equals(FilterItemLabel, other.FilterItemLabel, CurrentCultureIgnoreCase);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(FilterId, FilterGroupLabel.ToLower(), FilterItemLabel.ToLower());
            }
        }

        private record FilterGroupMeta(Guid FilterId, string FilterGroupLabel)
        {
            public virtual bool Equals(FilterGroupMeta? other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return FilterId.Equals(other.FilterId) 
                       && string.Equals(FilterGroupLabel, other.FilterGroupLabel, CurrentCultureIgnoreCase);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(FilterId, FilterGroupLabel.ToLower());
            }
        }

        public async Task ImportFiltersAndLocations(
            DataImport dataImport,
            Func<Task<Stream>> dataFileStreamProvider,
            SubjectMeta subjectMeta,
            StatisticsDbContext context)
        {
            var csvHeaders = await CsvUtils.GetCsvHeaders(dataFileStreamProvider);
            var soleGeographicLevel = dataImport.HasSoleGeographicLevel();

            var filterItemsFromCsv = new HashSet<FilterItemMeta>();
            var filterGroupsFromCsv = new HashSet<FilterGroupMeta>();
            var locations = new HashSet<Location>();

            var reader = new DataFileReader(csvHeaders, subjectMeta);
            
            await CsvUtils.ForEachRow(dataFileStreamProvider, async (rowValues, index, _) =>
            {
                if (index % Stage2RowCheck == 0)
                {
                    var currentStatus = await _dataImportService.GetImportStatus(dataImport.Id);

                    if (currentStatus.IsFinishedOrAborting())
                    {
                        _logger.LogInformation(
                            "Import for {FileName} has finished or is being aborted, " +
                            "so finishing importing Filters and Locations early", dataImport.File.Filename);
                        return false;
                    }

                    await _dataImportService.UpdateStatus(dataImport.Id,
                        STAGE_2,
                        (double) (index + 1) / dataImport.TotalRows!.Value * 100);
                }

                if (IsRowAllowed(soleGeographicLevel, rowValues, reader))
                {
                    foreach (var filterMeta in subjectMeta.Filters)
                    {
                        var filterItemLabel = reader.GetFilterItemValue(rowValues, filterMeta.Filter);
                        var filterGroupLabel = reader.GetFilterGroupValue(rowValues, filterMeta.Filter);

                        filterGroupsFromCsv.Add(new FilterGroupMeta(filterMeta.Filter.Id, filterGroupLabel));
                        filterItemsFromCsv.Add(new FilterItemMeta(filterMeta.Filter.Id, filterGroupLabel, filterItemLabel));
                    }

                    locations.Add(
                        ReadLocationFromCsv(
                            reader.GetGeographicLevel(rowValues),
                            GetCountry(rowValues, csvHeaders),
                            GetEnglishDevolvedArea(rowValues, csvHeaders),
                            GetInstitution(rowValues, csvHeaders),
                            GetLocalAuthority(rowValues, csvHeaders),
                            GetLocalAuthorityDistrict(rowValues, csvHeaders),
                            GetLocalEnterprisePartnership(rowValues, csvHeaders),
                            GetMayoralCombinedAuthority(rowValues, csvHeaders),
                            GetMultiAcademyTrust(rowValues, csvHeaders),
                            GetOpportunityArea(rowValues, csvHeaders),
                            GetParliamentaryConstituency(rowValues, csvHeaders),
                            GetPlanningArea(rowValues, csvHeaders),
                            GetProvider(rowValues, csvHeaders),
                            GetRegion(rowValues, csvHeaders),
                            GetRscRegion(rowValues, csvHeaders),
                            GetSchool(rowValues, csvHeaders),
                            GetSponsor(rowValues, csvHeaders),
                            GetWard(rowValues, csvHeaders)
                        ));
                }

                return true;
            });

            var filterGroups = filterGroupsFromCsv
                .Select(filterGroupMeta => new FilterGroup(filterGroupMeta.FilterId, filterGroupMeta.FilterGroupLabel))
                .ToList();

            var filterItems = filterItemsFromCsv
                .Select(filterItemMeta =>
                {
                    var (filterId, filterGroupLabel, filterItemLabel) = filterItemMeta;
                    
                    var filterGroup = filterGroups.Single(fg =>
                        fg.FilterId.Equals(filterId)
                        && string.Equals(fg.Label, filterGroupLabel, CurrentCultureIgnoreCase));

                    return new FilterItem(filterItemLabel, filterGroup.Id);
                })
                .ToList();
                    
            await _databaseHelper.DoInTransaction(context, async ctxDelegate =>
            {
                await ctxDelegate.FilterGroup.AddRangeAsync(filterGroups);
                await ctxDelegate.FilterItem.AddRangeAsync(filterItems);
                await ctxDelegate.SaveChangesAsync();
            });

            filterGroups.ForEach(filterGroup => _importerFilterCache.AddFilterGroup(filterGroup));
            filterItems.ForEach(filterItem => _importerFilterCache.AddFilterItem(filterItem));

            // Add any new Locations that are being introduced by this import process exclusively.  Other concurrent 
            // import processes reaching this stage will wait before adding their own new Locations, if any.
            //
            // This ensures that we do not end up with duplicate Locations being added by separate processes, or let one 
            // process continue on to the next stage of import before all Locations that it relies on have been
            // persisted successfully by whichever import process introduced it first.
            //
            // This also lets us take advantage of the performance gains of saving new Locations in a single batch
            // rather than in separate SaveChanges() calls, which introduces quite a penalty.
            await _databaseHelper.ExecuteWithExclusiveLock(
                context, 
                "Importer_AddNewLocations", 
                ctxDelegate => _importerLocationService.CreateIfNotExistsAndCache(ctxDelegate, locations.ToList()));
        }

        public async Task ImportObservations(
            DataImport import,
            Func<Task<Stream>> dataFileStreamProvider,
            Subject subject,
            SubjectMeta subjectMeta,
            StatisticsDbContext context)
        {
            var importObservationsBatchSize = GetRowsPerBatch();
            var soleGeographicLevel = import.HasSoleGeographicLevel();
            var csvHeaders = await CsvUtils.GetCsvHeaders(dataFileStreamProvider);
            var totalBatches = Math.Ceiling((decimal) import.TotalRows!.Value / importObservationsBatchSize);
            var importedRowsSoFar = import.ImportedRows;
            var startingBatchIndex = importedRowsSoFar / importObservationsBatchSize;
            var sw = new Stopwatch();
            sw.Start();
            _logger.LogInformation($">>>>>>>>> grabbing filters");


            var filters = await context
                .Filter
                .AsNoTracking()
                .Include(filter => filter.FilterGroups)
                .ThenInclude(group => group.FilterItems)
                .Where(filter => filter.SubjectId == import.SubjectId)
                .ToListAsync();

            // TODO DW - do away with this!
            _importerFilterService.Fill(filters);
            
            _logger.LogInformation($">>>>>>>>> grabbed {filters.Count} filters in {sw.ElapsedMilliseconds}");

            
            var reader = new DataFileReader(csvHeaders, subjectMeta);
            
            await CsvUtils.Batch(
                dataFileStreamProvider,
                importObservationsBatchSize,
                async (batchOfRows, batchIndex) =>
                {
                    // TODO DW - test for ceancellation or other abort 
                    
                    _logger.LogInformation(
                        "Importing Observation batch {BatchNumber} of {TotalBatches}", 
                        batchIndex + 1, 
                        totalBatches);

                    var allowedRows = batchOfRows.Select((cells, rowIndex) =>
                    {
                        if (IsRowAllowed(soleGeographicLevel, cells, reader))
                        {
                            return ObservationFromCsv(
                                cells,
                                csvHeaders,
                                subject,
                                filters,
                                reader,
                                (batchIndex * importObservationsBatchSize) + rowIndex + 2);
                        }

                        return null;
                    }).WhereNotNull();

                    var sw = new Stopwatch();
                    sw.Start();
                    _logger.LogInformation($">>>>>>>>> starting insert of ${allowedRows.Count()} rows");

                    await _databaseHelper.DoInTransaction(context, async contextDelegate => 
                        await _observationBatchImporter.ImportObservationBatch(contextDelegate, allowedRows)
                    );

                    _logger.LogInformation($">>>>>>>>> {sw.ElapsedMilliseconds}");
                    
                    importedRowsSoFar += allowedRows.Count();

                    await _dataImportService.Update(import.Id, importedRows: importedRowsSoFar);
                    
                    var percentageComplete = (double) ((batchIndex + 1) / totalBatches) * 100;

                    await _dataImportService.UpdateStatus(import.Id, STAGE_3, percentageComplete);

                    return true;
                },
                startingBatchIndex);
        }

        private static int GetRowsPerBatch()
        {
            return Int32.Parse(Environment.GetEnvironmentVariable("RowsPerBatch") 
                               ?? throw new InvalidOperationException("RowsPerBatch variable must be specified"));
        }

        /// <summary>
        /// Determines if a row should be imported based on geographic level.
        /// If a file contains a sole level then any row is allowed, otherwise rows for 'solo' importable levels are ignored.
        /// </summary>
        private static bool IsRowAllowed(bool soleGeographicLevel,
            IReadOnlyList<string> rowValues,
            DataFileReader reader)
        {
            return soleGeographicLevel ||
                   !reader.GetGeographicLevel(rowValues).IsSoloImportableLevel();
        }

        private Observation ObservationFromCsv(
            List<string> rowValues,
            List<string> colValues,
            Subject subject,
            List<Filter> filters,
            DataFileReader reader,
            int csvRowNum)
        {
            var observationId = _guidGenerator.NewGuid();

            return new Observation
            {
                Id = observationId,
                FilterItems = GetFilterItems(rowValues, filters, reader, observationId),
                LocationId = GetLocationId(rowValues, colValues, reader),
                Measures = reader.GetMeasures(rowValues),
                SubjectId = subject.Id,
                TimeIdentifier = reader.GetTimeIdentifier(rowValues),
                Year = reader.GetYear(rowValues),
                CsvRow = csvRowNum
            };
        }
    
        private List<ObservationFilterItem> GetFilterItems(
            IReadOnlyList<string> rowValues,
            List<Filter> filters,
            DataFileReader reader,
            Guid observationId)
        {
            
            return filters.Select(filter =>
            {
                var filterItemLabel = reader.GetFilterItemValue(rowValues, filter);
                var filterGroupLabel = reader.GetFilterGroupValue(rowValues, filter);

                var filterItem = _importerFilterService.Find(filterItemLabel, filterGroupLabel, filter.Label);
                
                return new ObservationFilterItem
                {
                    ObservationId = observationId,
                    FilterItemId = filterItem.Id,
                    FilterId = filter.Id
                };
            }).ToList();
        }

        private Guid GetLocationId(IReadOnlyList<string> rowValues, List<string> colValues, DataFileReader reader)
        {
            var location = ReadLocationFromCsv(
                reader.GetGeographicLevel(rowValues),
                GetCountry(rowValues, colValues),
                GetEnglishDevolvedArea(rowValues, colValues),
                GetInstitution(rowValues, colValues),
                GetLocalAuthority(rowValues, colValues),
                GetLocalAuthorityDistrict(rowValues, colValues),
                GetLocalEnterprisePartnership(rowValues, colValues),
                GetMayoralCombinedAuthority(rowValues, colValues),
                GetMultiAcademyTrust(rowValues, colValues),
                GetOpportunityArea(rowValues, colValues),
                GetParliamentaryConstituency(rowValues, colValues),
                GetPlanningArea(rowValues, colValues),
                GetProvider(rowValues, colValues),
                GetRegion(rowValues, colValues),
                GetRscRegion(rowValues, colValues),
                GetSchool(rowValues, colValues),
                GetSponsor(rowValues, colValues),
                GetWard(rowValues, colValues));
                
            return _importerLocationService.Get(location).Id;
        }

        private static Country? GetCountry(IReadOnlyList<string> rowValues, List<string> colValues)
        {
            return CsvUtils.BuildType(rowValues, colValues, ColumnValues[GeographicLevel.Country], values =>
                new Country(values[0], values[1]));
        }

        private static EnglishDevolvedArea? GetEnglishDevolvedArea(IReadOnlyList<string> rowValues, List<string> colValues)
        {
            return CsvUtils.BuildType(rowValues, colValues, ColumnValues[GeographicLevel.EnglishDevolvedArea], values =>
                new EnglishDevolvedArea(values[0], values[1]));
        }

        private static Institution? GetInstitution(IReadOnlyList<string> rowValues,
            List<string> colValues)
        {
            return CsvUtils.BuildType(rowValues, colValues, ColumnValues[GeographicLevel.Institution], values =>
                new Institution(values[0], values[1]));
        }

        private static LocalAuthority? GetLocalAuthority(IReadOnlyList<string> rowValues, List<string> colValues)
        {
            return CsvUtils.BuildType(rowValues, colValues, ColumnValues[GeographicLevel.LocalAuthority], values =>
                new LocalAuthority(values[0], values[1], values[2]));
        }

        private static LocalAuthorityDistrict? GetLocalAuthorityDistrict(IReadOnlyList<string> rowValues,
            List<string> colValues)
        {
            return CsvUtils.BuildType(rowValues, colValues, ColumnValues[GeographicLevel.LocalAuthorityDistrict], values =>
                new LocalAuthorityDistrict(values[0], values[1]));
        }

        private static LocalEnterprisePartnership? GetLocalEnterprisePartnership(IReadOnlyList<string> rowValues,
            List<string> colValues)
        {
            return CsvUtils.BuildType(rowValues, colValues, ColumnValues[GeographicLevel.LocalEnterprisePartnership], values =>
                new LocalEnterprisePartnership(values[0], values[1]));
        }

        private static MayoralCombinedAuthority? GetMayoralCombinedAuthority(IReadOnlyList<string> rowValues,
            List<string> colValues)
        {
            return CsvUtils.BuildType(rowValues, colValues, ColumnValues[GeographicLevel.MayoralCombinedAuthority], values =>
                new MayoralCombinedAuthority(values[0], values[1]));
        }

        private static MultiAcademyTrust? GetMultiAcademyTrust(IReadOnlyList<string> rowValues, List<string> colValues)
        {
            return CsvUtils.BuildType(rowValues, colValues, ColumnValues[GeographicLevel.MultiAcademyTrust], values =>
                new MultiAcademyTrust(values[0], values[1]));
        }

        private static OpportunityArea? GetOpportunityArea(IReadOnlyList<string> rowValues, List<string> colValues)
        {
            return CsvUtils.BuildType(rowValues, colValues, ColumnValues[GeographicLevel.OpportunityArea], values =>
                new OpportunityArea(values[0], values[1]));
        }

        private static ParliamentaryConstituency? GetParliamentaryConstituency(IReadOnlyList<string> rowValues,
            List<string> colValues)
        {
            return CsvUtils.BuildType(rowValues, colValues, ColumnValues[GeographicLevel.ParliamentaryConstituency], values =>
                new ParliamentaryConstituency(values[0], values[1]));
        }

        private static Provider? GetProvider(IReadOnlyList<string> rowValues,
            List<string> colValues)
        {
            return CsvUtils.BuildType(rowValues, colValues, ColumnValues[GeographicLevel.Provider], values =>
                new Provider(values[0], values[1]));
        }

        private static Region? GetRegion(IReadOnlyList<string> rowValues, List<string> colValues)
        {
            return CsvUtils.BuildType(rowValues, colValues, ColumnValues[GeographicLevel.Region], values =>
                new Region(values[0], values[1]));
        }

        private static RscRegion? GetRscRegion(IReadOnlyList<string> rowValues, List<string> colValues)
        {
            return CsvUtils.BuildType(rowValues, colValues, "rsc_region_lead_name", value => new RscRegion(value));
        }

        private static School? GetSchool(IReadOnlyList<string> rowValues, List<string> colValues)
        {
            return CsvUtils.BuildType(rowValues, colValues, ColumnValues[GeographicLevel.School], values =>
                new School(values[0], values[1]));
        }

        private static Sponsor? GetSponsor(IReadOnlyList<string> rowValues, List<string> colValues)
        {
            return CsvUtils.BuildType(rowValues, colValues, ColumnValues[GeographicLevel.Sponsor], values =>
                new Sponsor(values[0], values[1]));
        }

        private static Ward? GetWard(IReadOnlyList<string> rowValues, List<string> colValues)
        {
            return CsvUtils.BuildType(rowValues, colValues, ColumnValues[GeographicLevel.Ward], values =>
                new Ward(values[0], values[1]));
        }

        private static PlanningArea? GetPlanningArea(IReadOnlyList<string> rowValues, List<string> colValues)
        {
            return CsvUtils.BuildType(rowValues, colValues, ColumnValues[GeographicLevel.PlanningArea], values =>
                new PlanningArea(values[0], values[1]));
        }
        
        private Location ReadLocationFromCsv(
            GeographicLevel geographicLevel,
            Country? country,
            EnglishDevolvedArea? englishDevolvedArea = null,
            Institution? institution = null,
            LocalAuthority? localAuthority = null,
            LocalAuthorityDistrict? localAuthorityDistrict = null,
            LocalEnterprisePartnership? localEnterprisePartnership = null,
            MayoralCombinedAuthority? mayoralCombinedAuthority = null,
            MultiAcademyTrust? multiAcademyTrust = null,
            OpportunityArea? opportunityArea = null,
            ParliamentaryConstituency? parliamentaryConstituency = null,
            PlanningArea? planningArea = null,
            Provider? provider = null,
            Region? region = null,
            RscRegion? rscRegion = null,
            School? school = null,
            Sponsor? sponsor = null,
            Ward? ward = null)
        {
            return new Location
            {
                GeographicLevel = geographicLevel,
                Country = country,
                EnglishDevolvedArea = englishDevolvedArea,
                Institution = institution,
                LocalAuthority = localAuthority,
                LocalAuthorityDistrict = localAuthorityDistrict,
                LocalEnterprisePartnership = localEnterprisePartnership,
                MayoralCombinedAuthority = mayoralCombinedAuthority,
                MultiAcademyTrust = multiAcademyTrust,
                OpportunityArea = opportunityArea,
                ParliamentaryConstituency = parliamentaryConstituency,
                PlanningArea = planningArea,
                Provider = provider,
                Region = region,
                RscRegion = rscRegion,
                School = school,
                Sponsor = sponsor,
                Ward = ward
            };
        }
    }

    public class StoredProcedureObservationBatchImporter : IObservationBatchImporter
    {
        public async Task ImportObservationBatch(StatisticsDbContext context, IEnumerable<Observation> observations)
        {
            var sw = new Stopwatch();
            sw.Start();
            Console.WriteLine($"     >>>>>>>>> starting building tables of {observations.Count()} Observations");

            
            var observationsTable = new DataTable();
            observationsTable.Columns.Add("Id", typeof(Guid));
            observationsTable.Columns.Add("SubjectId", typeof(Guid));
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
            
            Console.WriteLine($"     >>>>>>>>> finished building tables of {observations.Count()} Observations in {sw.ElapsedMilliseconds}");


            sw.Restart();
            Console.WriteLine($"     >>>>>>>>> starting insert of {observationsTable.Rows} Observations");

            var parameter = new SqlParameter("@Observations", SqlDbType.Structured)
            {
                Value = observationsTable, TypeName = "[dbo].[ObservationType]"
            };
            
        

            await context.Database.ExecuteSqlRawAsync("EXEC [dbo].[InsertObservations] @Observations", parameter);
            Console.WriteLine($"     >>>>>>>>> finished insert of {observationsTable.Rows} Observations in {sw.ElapsedMilliseconds}");

            sw.Restart();
            Console.WriteLine($"     >>>>>>>>> starting insert of {observationsFilterItemsTable.Rows} ObservationFilterItems");

            parameter = new SqlParameter("@ObservationFilterItems", SqlDbType.Structured)
            {
                Value = observationsFilterItemsTable, TypeName = "[dbo].[ObservationFilterItemType]"
            };

            await context.Database.ExecuteSqlRawAsync(
                "EXEC [dbo].[InsertObservationFilterItems] @ObservationFilterItems", parameter);
            
            Console.WriteLine($"     >>>>>>>>> finished insert of {observationsFilterItemsTable.Rows} ObservationFilterItems in {sw.ElapsedMilliseconds}");

        }
    }

    public interface IObservationBatchImporter
    {
        Task ImportObservationBatch(StatisticsDbContext context, IEnumerable<Observation> observations);
    }
}
