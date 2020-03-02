using System;
using System.Collections.Generic;
using System.Linq;
using EFCore.BulkExtensions;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Exceptions;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Importer.Services
{
    public class ImporterService : IImporterService
    {
        private readonly ImporterLocationService _importerLocationService;
        private readonly ImporterFilterService _importerFilterService;
        private readonly IImporterMetaService _importerMetaService;
        private readonly ILogger<ImporterService> _logger;

        private enum Columns
        {
            SCHOOL_COLS,
            COUNTRY_COLS,
            INSTITUTION_COLS,
            LOCAL_AUTH_COLS,
            LOCAL_AUTH_DISTRICT_COLS,
            LOCAL_ENTERPRISE_PARTNERSHIP_COLS,
            MAYORAL_COMBINED_AUTHORITY_COLS,
            MULTI_ACADEMY_TRUST_COLS,
            OPPORTUNITY_AREA_COLS,
            PARLIAMENTARY_CONSTITUENCY_COLS,
            PROVIDER_COLS,
            REGION_COLS,
            SPONSOR_COLS,
            WARD_COLS
        }

        private static readonly Dictionary<Columns, string[]> ColumnValues =
            new Dictionary<Columns, string[]>
            {
                {
                    Columns.SCHOOL_COLS, new[]{"academy_open_date", "academy_type", "estab", "laestab", "school_name", "school_postcode", "urn"}
                },
                {
                    Columns.COUNTRY_COLS, new[]{"country_code", "country_name"}
                },
                {
                    Columns.INSTITUTION_COLS, new[]{"institution_id", "institution_name"}
                },
                {
                    Columns.LOCAL_AUTH_COLS, new[]{"new_la_code", "old_la_code", "la_name"}
                },
                {
                    Columns.LOCAL_AUTH_DISTRICT_COLS, new[]{"lad_code", "lad_name"}
                },
                {
                    Columns.LOCAL_ENTERPRISE_PARTNERSHIP_COLS, new[]{"local_enterprise_partnership_code", "local_enterprise_partnership_name"}
                },
                {
                    Columns.MAYORAL_COMBINED_AUTHORITY_COLS, new[]{"mayoral_combined_authority_code", "mayoral_combined_authority_name"}
                },
                {
                    Columns.MULTI_ACADEMY_TRUST_COLS, new[]{"trust_id", "trust_name"}
                },
                {
                    Columns.OPPORTUNITY_AREA_COLS, new[]{"opportunity_area_code", "opportunity_area_name"}
                },
                {
                    Columns.PARLIAMENTARY_CONSTITUENCY_COLS, new[]{"pcon_code", "pcon_name"}
                },
                {
                    Columns.PROVIDER_COLS, new[]{"provider_urn", "provider_ukprn", "provider_upin", "provider_name"}
                },
                {
                    Columns.REGION_COLS, new[]{"region_code", "region_name"}
                },
                {
                    Columns.SPONSOR_COLS, new[]{"sponsor_id", "sponsor_name"}
                },
                {
                    Columns.WARD_COLS, new[]{"ward_code", "ward_name"}
                }
            };
                
        private static readonly List<GeographicLevel> IgnoredGeographicLevels = new List<GeographicLevel>
        {
            GeographicLevel.Institution,
            GeographicLevel.Provider,
            GeographicLevel.School
        };
        
        public ImporterService(
            ImporterFilterService importerFilterService,
            ImporterLocationService importerLocationService,
            IImporterMetaService importerMetaService,
            ILogger<ImporterService> logger)
        {
            _importerFilterService = importerFilterService;
            _importerLocationService = importerLocationService;
            _importerMetaService = importerMetaService;
            _logger = logger;
        }

        public SubjectMeta ImportMeta(List<string> metaLines, Subject subject, StatisticsDbContext context)
        {
            return _importerMetaService.Import(metaLines, subject, context);
        }
        
        public SubjectMeta GetMeta(List<string> metaLines, Subject subject, StatisticsDbContext context)
        {
            return _importerMetaService.Get(metaLines, subject, context);
        }

        public void ImportFiltersLocationsAndSchools(List<string> lines, SubjectMeta subjectMeta, Subject subject, StatisticsDbContext context)
        {
            // Clearing the caches is required here as the seeder shares the cache with all subjects
            _importerFilterService.ClearCache();
            _importerLocationService.ClearCache();
            
            var headers = lines.First().Split(',').ToList();
            lines.RemoveAt(0);
            lines.ToList().ForEach(line =>
            {
                CreateFiltersAndLocationsFromCsv(context, line, headers, subjectMeta.Filters);
            });
        }

        public void ImportObservations(List<string> lines, Subject subject, SubjectMeta subjectMeta, int batchNo,
            int rowsPerBatch, StatisticsDbContext context)
        {
            _importerFilterService.ClearCache();
            _importerLocationService.ClearCache();
            
            var headers = lines.First().Split(',').ToList();
            lines.RemoveAt(0);
            var observations = GetObservations(context, lines, headers, subject, subjectMeta, batchNo, rowsPerBatch).ToList();
            
            var subEntities = new List<ObservationFilterItem>();
            
            using (var transaction = context.Database.BeginTransaction())
            {
                context.BulkInsert(observations);

                foreach (var o in observations)
                {
                    foreach (var item in o.FilterItems)
                    {
                        item.ObservationId = o.Id;
                        item.FilterItemId = item.FilterItem.Id;
                    }
                    
                    subEntities.AddRange(o.FilterItems);
                }
                context.BulkInsert(subEntities, new BulkConfig
                {
                    BulkCopyTimeout = 0
                });
                    
                transaction.Commit();
            }

            _logger.LogDebug($"{observations.Count()} observations added successfully for {subject.Name}");
        }
        
        public static GeographicLevel GetGeographicLevel(IReadOnlyList<string> line, List<string> headers)
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
        
        public static TimeIdentifier GetTimeIdentifier(IReadOnlyList<string> line, List<string> headers)
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
        
        public static int GetYear(IReadOnlyList<string> line, List<string> headers)
        {
            var tp = CsvUtil.Value(line, headers, "time_period");
            return int.Parse(tp.Substring(0, 4));
        }

        private IEnumerable<Observation> GetObservations(
            StatisticsDbContext context,
            IEnumerable<string> lines,
            List<string> headers,
            Subject subject,
            SubjectMeta subjectMeta,
            int batchNo,
            int rowsPerBatch
            )
        {
            return lines.Select((line, i) =>
            {
                var csvRowNum = ((batchNo - 1) * rowsPerBatch) + i + 2;
                return ObservationFromCsv(context, line, headers, subject, subjectMeta, csvRowNum);
            }).Where(o => !IgnoredGeographicLevels.Contains(o.GeographicLevel));
        }
        
        private Observation ObservationFromCsv(
            StatisticsDbContext context,
            string raw,
            List<string> headers,
            Subject subject,
            SubjectMeta subjectMeta,
            int csvRowNum)
        {
            var line = raw.Split(',');
        
            return new Observation
            {
                Id = Guid.NewGuid(),
                FilterItems = GetFilterItems(context, line, headers, subjectMeta.Filters),
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
            string raw,
            List<string> headers,
            IEnumerable<(Filter Filter, string Column, string FilterGroupingColumn)> filtersMeta)
        {
            var line = raw.Split(',');
            CreateFilterItems(context, line, headers, filtersMeta);
            GetLocationId(line, headers, context);
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
            IEnumerable<(Filter Filter, string Column, string FilterGroupingColumn)> filtersMeta)
        {
            return filtersMeta.Select(filterMeta =>
            {
                var filterItemLabel = CsvUtil.Value(line, headers, filterMeta.Column);
                var filterGroupLabel = CsvUtil.Value(line, headers, filterMeta.FilterGroupingColumn);
                
                return new ObservationFilterItem
                {
                    FilterItem = _importerFilterService.Find(filterItemLabel, filterGroupLabel, filterMeta.Filter, context)
                };
            }).ToList();
        }

        private Guid GetLocationId(IReadOnlyList<string> line, List<string> headers, StatisticsDbContext context)
        {
            return _importerLocationService.Find(
                context,
                GetCountry(line, headers),
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
                GetWard(line, headers)
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
            return CsvUtil.BuildType(line, headers, ColumnValues[Columns.COUNTRY_COLS], values =>
                new Country(values[0], values[1]));
        }

        private static Institution GetInstitution(IReadOnlyList<string> line,
            List<string> headers)
        {
            return CsvUtil.BuildType(line, headers, ColumnValues[Columns.INSTITUTION_COLS], values =>
                new Institution(values[0], values[1]));
        }

        private static LocalAuthority GetLocalAuthority(IReadOnlyList<string> line, List<string> headers)
        {
            return CsvUtil.BuildType(line, headers, ColumnValues[Columns.LOCAL_AUTH_COLS], values =>
                new LocalAuthority(values[0], values[1], values[2]));
        }

        private static LocalAuthorityDistrict GetLocalAuthorityDistrict(IReadOnlyList<string> line,
            List<string> headers)
        {
            return CsvUtil.BuildType(line, headers, ColumnValues[Columns.LOCAL_AUTH_DISTRICT_COLS], values =>
                new LocalAuthorityDistrict(values[0], values[1]));
        }

        private static LocalEnterprisePartnership GetLocalEnterprisePartnership(IReadOnlyList<string> line,
            List<string> headers)
        {
            return CsvUtil.BuildType(line, headers, ColumnValues[Columns.LOCAL_ENTERPRISE_PARTNERSHIP_COLS], values =>
                new LocalEnterprisePartnership(values[0], values[1]));
        }

        private static MayoralCombinedAuthority GetMayoralCombinedAuthority(IReadOnlyList<string> line,
            List<string> headers)
        {
            return CsvUtil.BuildType(line, headers, ColumnValues[Columns.MAYORAL_COMBINED_AUTHORITY_COLS], values =>
                new MayoralCombinedAuthority(values[0], values[1]));
        }

        private static Mat GetMultiAcademyTrust(IReadOnlyList<string> line, List<string> headers)
        {
            return CsvUtil.BuildType(line, headers, ColumnValues[Columns.MULTI_ACADEMY_TRUST_COLS], values =>
                new Mat(values[0], values[1]));
        }

        private static OpportunityArea GetOpportunityArea(IReadOnlyList<string> line, List<string> headers)
        {
            return CsvUtil.BuildType(line, headers, ColumnValues[Columns.OPPORTUNITY_AREA_COLS], values =>
                new OpportunityArea(values[0], values[1]));
        }

        private static ParliamentaryConstituency GetParliamentaryConstituency(IReadOnlyList<string> line,
            List<string> headers)
        {
            return CsvUtil.BuildType(line, headers, ColumnValues[Columns.PARLIAMENTARY_CONSTITUENCY_COLS], values =>
                new ParliamentaryConstituency(values[0], values[1]));
        }

        private static Region GetRegion(IReadOnlyList<string> line, List<string> headers)
        {
            return CsvUtil.BuildType(line, headers, ColumnValues[Columns.REGION_COLS], values =>
                new Region(values[0], values[1]));
        }

        private static RscRegion GetRscRegion(IReadOnlyList<string> line, List<string> headers)
        {
            return CsvUtil.BuildType(line, headers, "rsc_region_lead_name", value => new RscRegion(value));
        }

        private static Sponsor GetSponsor(IReadOnlyList<string> line, List<string> headers)
        {
            return CsvUtil.BuildType(line, headers, ColumnValues[Columns.SPONSOR_COLS], values =>
                new Sponsor(values[0], values[1]));
        }

        private static Ward GetWard(IReadOnlyList<string> line, List<string> headers)
        {
            return CsvUtil.BuildType(line, headers, ColumnValues[Columns.WARD_COLS], values =>
                new Ward(values[0], values[1]));
        }
    }
}