using System;
using System.Collections.Generic;
using System.Linq;
using EFCore.BulkExtensions;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
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
        private readonly ImporterSchoolService _importerSchoolService;
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
                    Columns.LOCAL_AUTH_DISTRICT_COLS, new[]{"sch_lad_code", "sch_lad_name"}
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
        
        public ImporterService(
            ImporterFilterService importerFilterService,
            ImporterLocationService importerLocationService,
            IImporterMetaService importerMetaService,
            ImporterSchoolService importerSchoolService,
            ILogger<ImporterService> logger)
        {
            _importerFilterService = importerFilterService;
            _importerLocationService = importerLocationService;
            _importerMetaService = importerMetaService;
            _logger = logger;
            _importerSchoolService = importerSchoolService;
        }

        public SubjectMeta ImportMeta(List<string> metaLines, Subject subject, StatisticsDbContext context)
        {
            var subjectMeta = _importerMetaService.Import(metaLines, subject, context);
            context.SaveChanges();
            return subjectMeta;
        }
        
        public SubjectMeta GetMeta(List<string> metaLines, Subject subject, StatisticsDbContext context)
        {
            return _importerMetaService.Get(metaLines, subject, context);
        }

        public void ImportFiltersLocationsAndSchools(List<string> lines, SubjectMeta subjectMeta, Subject subject, StatisticsDbContext context)
        {
            var headers = lines.First().Split(',').ToList();
            lines.RemoveAt(0);
            lines.ToList().ForEach(line =>
            {
                CreateFiltersLocationsAndSchoolsFromCsv(context, line, headers, subjectMeta.Filters);
            });
        }

        public void ImportObservations(List<string> lines, Subject subject, SubjectMeta subjectMeta, int batchNo,
            int rowsPerBatch, StatisticsDbContext context)
        {
            var headers = lines.First().Split(',').ToList();
            lines.RemoveAt(0);
            var observations = GetObservations(context, lines, headers, subject, subjectMeta, batchNo, rowsPerBatch).ToList();
            
            var subEntities = new List<ObservationFilterItem>();
            
            using (var transaction = context.Database.BeginTransaction())
            {
                context.BulkInsert(observations,
                    new BulkConfig {WithHoldlock= true, BatchSize = observations.Count(), PreserveInsertOrder = true, SetOutputIdentity = true});

                foreach (var o in observations)
                {
                    foreach (var item in o.FilterItems)
                    {
                        item.ObservationId = o.Id;
                        item.FilterItemId = item.FilterItem.Id;
                    }
                    
                    subEntities.AddRange(o.FilterItems);
                }
                context.BulkInsert(subEntities,
                        new BulkConfig {WithHoldlock= true, BatchSize = subEntities.Count()});
                    
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
            // In order to "Preserve Order" of the bulk insertion need to assign a temp id which cannot exist in db
            long lastId = 1;
            if (context.Observation.Any())
            {
                lastId = context.Observation.Select(x => x.Id).DefaultIfEmpty().Max() + 1;
            }

            return lines.Select((line, i) =>
            {
                var csvRowNum = ((batchNo - 1) * rowsPerBatch) + i + 2;
                return ObservationFromCsv(context, line, headers, subject, subjectMeta, csvRowNum, lastId++);
            });
        }
        
        private Observation ObservationFromCsv(
            StatisticsDbContext context,
            string raw,
            List<string> headers,
            Subject subject,
            SubjectMeta subjectMeta,
            int csvRowNum,
            long lastId)
        {
            var line = raw.Split(',');
        
            return new Observation
            {
                FilterItems = GetFilterItems(context, line, headers, subjectMeta.Filters),
                GeographicLevel = GetGeographicLevel(line, headers),
                LocationId = GetLocationId(line, headers, context),
                Measures = GetMeasures(line, headers, subjectMeta.Indicators),
                ProviderUrn = GetProvider(line, headers)?.Urn,
                SchoolLaEstab = GetSchool(line, headers, context)?.LaEstab,
                SubjectId = subject.Id,
                TimeIdentifier = GetTimeIdentifier(line, headers),
                Year = GetYear(line, headers),
                CsvRow = csvRowNum,
                Id = lastId
            };
        }
        
        private void CreateFiltersLocationsAndSchoolsFromCsv(
            StatisticsDbContext context,
            string raw,
            List<string> headers,
            IEnumerable<(Filter Filter, string Column, string FilterGroupingColumn)> filtersMeta)
        {
            var line = raw.Split(',');
            CreateFilterItems(context, line, headers, filtersMeta);
            GetLocationId(line, headers, context);
            GetSchool(line, headers, context);
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

        private long GetLocationId(IReadOnlyList<string> line, List<string> headers, StatisticsDbContext context)
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

        private School GetSchool(IReadOnlyList<string> line, List<string> headers, StatisticsDbContext context)
        {
            var school = CsvUtil.BuildType(line, headers, ColumnValues[Columns.SCHOOL_COLS], values => new School
            {
                AcademyOpenDate = values[0],
                AcademyType = values[1],
                Estab = values[2],
                LaEstab = values[3],
                Name = values[4],
                Postcode = values[5],
                Urn = values[6]
            });
            
            return _importerSchoolService.Find(school, context);
        }

        private static Dictionary<long, string> GetMeasures(IReadOnlyList<string> line,
            List<string> headers, IEnumerable<(Indicator Indicator, string Column)> indicators)
        {
            var columns = indicators.Select(tuple => tuple.Column);
            var values = CsvUtil.Values(line, headers, columns);

            return indicators.Zip(values, (tuple, value) => new {tuple, value})
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

        private static Provider GetProvider(IReadOnlyList<string> line, List<string> headers)
        {
            return CsvUtil.BuildType(line, headers, ColumnValues[Columns.PROVIDER_COLS], values =>
                new Provider(values[0], values[1], values[2], values[3]));
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