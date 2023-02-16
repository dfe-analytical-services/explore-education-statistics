#nullable enable
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
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
using static System.StringComparison;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class ImporterService : IImporterService
    {
        private const string DefaultFilterGroupLabel = "Default";
        private const string DefaultFilterItemLabel = "Not specified";

        private static readonly EnumToEnumLabelConverter<TimeIdentifier> TimeIdentifierLookup = new();
        
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
            ImporterFilterCache importerFilterCache, 
            IObservationBatchImporter? observationBatchImporter = null)
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
            var colValues = await CsvUtils.GetCsvHeaders(dataFileStreamProvider);
            var soleGeographicLevel = dataImport.HasSoleGeographicLevel();

            var filterItemsFromCsv = new HashSet<FilterItemMeta>();
            var filterGroupsFromCsv = new HashSet<FilterGroupMeta>();
            var locations = new HashSet<Location>();
            
            await CsvUtils.ForEachRow(dataFileStreamProvider, async (rowValues, index) =>
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
                        DataImportStatus.STAGE_2,
                        (double) (index + 1) / dataImport.TotalRows!.Value * 100);
                }

                if (IsRowAllowed(soleGeographicLevel, rowValues, colValues))
                {
                    foreach (var filterMeta in subjectMeta.Filters)
                    {
                        var filterItemLabel = CsvUtils.Value(
                            rowValues, 
                            colValues, 
                            filterMeta.Column, 
                            defaultValue: DefaultFilterItemLabel)!;
                        
                        var filterGroupLabel = CsvUtils.Value(
                            rowValues, 
                            colValues, 
                            filterMeta.FilterGroupingColumn,
                            defaultValue: DefaultFilterGroupLabel)!;

                        filterGroupsFromCsv.Add(new FilterGroupMeta(filterMeta.Filter.Id, filterGroupLabel));
                        filterItemsFromCsv.Add(new FilterItemMeta(filterMeta.Filter.Id, filterGroupLabel, filterItemLabel));
                    }

                    locations.Add(
                        ReadLocationFromCsv(
                            GeographicLevelCsvUtils.GetGeographicLevel(rowValues, colValues),
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
                            GetWard(rowValues, colValues)
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

        public async Task ImportObservations(DataImport import,
            Func<Task<Stream>> dataFileStreamProvider,
            Subject subject,
            SubjectMeta subjectMeta,
            int batchNo,
            StatisticsDbContext context)
        {
            var observations = (await GetObservations(
                import,
                context,
                dataFileStreamProvider,
                subject,
                subjectMeta,
                batchNo)).ToList();

            await _observationBatchImporter.ImportObservationBatch(context, observations);
        }

        public TimeIdentifier GetTimeIdentifier(IReadOnlyList<string> rowValues, List<string> colValues)
        {
            var value = CsvUtils.Value(rowValues, colValues, "time_identifier");
            
            try
            {
                return (TimeIdentifier) TimeIdentifierLookup.ConvertFromProvider.Invoke(value)!;
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new InvalidTimeIdentifierException(value);
            }
        }

        public int GetYear(IReadOnlyList<string> rowValues, List<string> colValues)
        {
            var tp = CsvUtils.Value(rowValues, colValues, "time_period");
            if (tp == null)
            {
                throw new InvalidTimePeriodException(null);
            }

            return int.Parse(tp.Substring(0, 4));
        }

        private async Task<IEnumerable<Observation>> GetObservations(
            DataImport import,
            StatisticsDbContext context,
            Func<Task<Stream>> dataFileStreamProvider,
            Subject subject,
            SubjectMeta subjectMeta,
            int batchNo)
        {
            var soleGeographicLevel = import.HasSoleGeographicLevel();
            var csvHeaders = await CsvUtils.GetCsvHeaders(dataFileStreamProvider);

            return (await CsvUtils.Select(dataFileStreamProvider, (rowValues, index) =>
            {
                if (IsRowAllowed(soleGeographicLevel, rowValues, csvHeaders))
                {
                    return ObservationFromCsv(
                        context,
                        rowValues,
                        csvHeaders,
                        subject,
                        subjectMeta,
                        (batchNo - 1) * import.RowsPerBatch + index + 2);
                }

                return null;
            }))
                .WhereNotNull();
        }

        /// <summary>
        /// Determines if a row should be imported based on geographic level.
        /// If a file contains a sole level then any row is allowed, otherwise rows for 'solo' importable levels are ignored.
        /// </summary>
        public static bool IsRowAllowed(bool soleGeographicLevel,
            IReadOnlyList<string> rowValues,
            List<string> colValues)
        {
            return soleGeographicLevel ||
                   !GeographicLevelCsvUtils.GetGeographicLevel(rowValues, colValues).IsSoloImportableLevel();
        }

        private Observation ObservationFromCsv(
            StatisticsDbContext context,
            List<string> rowValues,
            List<string> colValues,
            Subject subject,
            SubjectMeta subjectMeta,
            int csvRowNum)
        {
            var observationId = _guidGenerator.NewGuid();

            return new Observation
            {
                Id = observationId,
                FilterItems = GetFilterItems(context, rowValues, colValues, subjectMeta.Filters, observationId),
                LocationId = GetLocationId(rowValues, colValues),
                Measures = GetMeasures(rowValues, colValues, subjectMeta.Indicators),
                SubjectId = subject.Id,
                TimeIdentifier = GetTimeIdentifier(rowValues, colValues),
                Year = GetYear(rowValues, colValues),
                CsvRow = csvRowNum
            };
        }

        private List<ObservationFilterItem> GetFilterItems(
            StatisticsDbContext context,
            IReadOnlyList<string> rowValues,
            List<string> colValues,
            IEnumerable<(Filter Filter, string Column, string FilterGroupingColumn)> filtersMeta,
            Guid observationId)
        {
            return filtersMeta.Select(filterMeta =>
            {
                var (filter, column, filterGroupingColumn) = filterMeta;
                
                var filterItemLabel = CsvUtils.Value(
                    rowValues, 
                    colValues, 
                    column, 
                    DefaultFilterItemLabel);
                
                var filterGroupLabel = CsvUtils.Value(
                    rowValues, 
                    colValues, 
                    filterGroupingColumn, 
                    DefaultFilterGroupLabel);

                return new ObservationFilterItem
                {
                    ObservationId = observationId,
                    FilterItemId = _importerFilterService.Find(filterItemLabel, filterGroupLabel, filter, context).Id,
                    FilterId = filter.Id
                };
            }).ToList();
        }

        private Guid GetLocationId(IReadOnlyList<string> rowValues, List<string> colValues)
        {
            var location = ReadLocationFromCsv(
                GeographicLevelCsvUtils.GetGeographicLevel(rowValues, colValues),
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

        private static Dictionary<Guid, string> GetMeasures(IReadOnlyList<string> rowValues,
            List<string> colValues,
            IEnumerable<(Indicator Indicator, string Column)> indicators)
        {
            var valueTuples = indicators.ToList();
            var columns = valueTuples.Select(tuple => tuple.Column);
            var values = CsvUtils.Values(rowValues, colValues, columns);

            return valueTuples.Zip(values, (tuple, value) => new {tuple, value})
                .ToDictionary(item => item.tuple.Indicator.Id, item => item.value)!;
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

    public interface IObservationBatchImporter
    {
        Task ImportObservationBatch(StatisticsDbContext context, IEnumerable<Observation> observations);
    }
}
