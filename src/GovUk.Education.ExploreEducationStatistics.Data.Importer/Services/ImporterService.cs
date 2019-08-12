using System;
using System.Collections.Generic;
using System.Linq;
using EFCore.BulkExtensions;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
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
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ImporterService> _logger;

        private int _importCount;
        
        public ImporterService(
            ImporterFilterService importerFilterService,
            ImporterLocationService importerLocationService,
            IImporterMetaService importerMetaService,
            ImporterSchoolService importerSchoolService,
            ApplicationDbContext context,
            ILogger<ImporterService> logger)
        {
            _importerFilterService = importerFilterService;
            _importerLocationService = importerLocationService;
            _importerMetaService = importerMetaService;
            _context = context;
            _logger = logger;
            _importerSchoolService = importerSchoolService;
        }
        
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

        public SubjectMeta ImportMeta(List<string> metaLines, Subject subject)
        {
            _logger.LogDebug("Importing meta lines for Publication {Publication}, {Subject}", subject.Release.Publication.Title, subject.Name);
            
            return _importerMetaService.Import(metaLines, subject);
        }
        
        public SubjectMeta GetMeta(List<string> metaLines, Subject subject)
        {
            _logger.LogDebug("Getting meta lines for Publication {Publication}, {Subject}", subject.Release.Publication.Title, subject.Name);
            
            return _importerMetaService.Get(metaLines, subject);
        }

        public void ImportFiltersLocationsAndSchools(List<string> lines, SubjectMeta subjectMeta, string subjectName)
        {
            _importCount = 0;
            var headers = lines.First().Split(',').ToList();
            lines.RemoveAt(0);
            lines.ToList().ForEach(line => CreateFiltersLocationsAndSchoolsFromCsv(line, headers, subjectMeta.Filters, subjectName, lines.Count));
        }

        public void ImportObservations(List<string> lines, Subject subject, SubjectMeta subjectMeta)
        {
            _logger.LogDebug("Importing batch for Publication {Publication}, {Subject}", subject.Release.Publication.Title, subject.Name);

            var headers = lines.First().Split(',').ToList();
            lines.RemoveAt(0);
            
            _logger.LogInformation($"Retrieving observations for {subject.Name}");

            var observations = GetObservations(lines, headers, subject, subjectMeta).ToList();
            
            _logger.LogInformation($"Adding {observations.Count()} observations for {subject.Name}");

            var subEntities = new List<ObservationFilterItem>();
            
            using (var transaction = _context.Database.BeginTransaction())
            {
                _context.BulkInsert(observations,
                    new BulkConfig {PreserveInsertOrder = true, SetOutputIdentity = true});
                foreach (var o in observations)
                {
                    foreach (var item in o.FilterItems)
                    {
                        item.ObservationId = o.Id;
                        item.FilterItemId = item.FilterItem.Id;
                    }
                    
                    subEntities.AddRange(o.FilterItems);
                }
                _context.BulkInsert(subEntities);
                transaction.Commit();
            }

            _logger.LogInformation($"{observations.Count()} observations added successfully for {subject.Name}");
        }

        private IEnumerable<Observation> GetObservations(IEnumerable<string> lines,
            List<string> headers,
            Subject subject,
            SubjectMeta subjectMeta)
        {
            return lines.Select(line => ObservationsFromCsv(line, headers, subject, subjectMeta));
        }

        private Observation ObservationsFromCsv(string raw,
            List<string> headers,
            Subject subject,
            SubjectMeta subjectMeta)
        {
            var line = raw.Split(',');

            return new Observation
            {
                FilterItems = GetFilterItems(line, headers, subjectMeta.Filters),
                GeographicLevel = GetGeographicLevel(line, headers),
                LocationId = GetLocationId(line, headers),
                Measures = GetMeasures(line, headers, subjectMeta.Indicators),
                ProviderUrn = GetProvider(line, headers)?.Urn,
                SchoolLaEstab = GetSchool(line, headers)?.LaEstab,
                Subject = subject,
                SubjectId = subject.Id,
                TimeIdentifier = GetTimeIdentifier(line, headers),
                Year = GetYear(line, headers),
            };
        }
        
        private void CreateFiltersLocationsAndSchoolsFromCsv(string raw,
            List<string> headers,
            IEnumerable<(Filter Filter, string Column, string FilterGroupingColumn)> filtersMeta,
            string subjectName,
            int numLines)
        {
            var line = raw.Split(',');
            CreateFilterItems(line, headers, filtersMeta);
            GetLocationId(line, headers);
            GetSchool(line, headers);

            var mod = _importCount++;
            if (mod % 1000 == 0)
            {
                _logger.LogInformation($"{mod} (of {numLines}) lines processed during first pass for {subjectName}");
            }
        }

        private ICollection<ObservationFilterItem> GetFilterItems(IReadOnlyList<string> line,
            List<string> headers,
            IEnumerable<(Filter Filter, string Column, string FilterGroupingColumn)> filtersMeta)
        {
            return filtersMeta.Select(filterMeta =>
            {
                var filterItemLabel = CsvUtil.Value(line, headers, filterMeta.Column);
                var filterGroupLabel = CsvUtil.Value(line, headers, filterMeta.FilterGroupingColumn);
                
                return new ObservationFilterItem
                {
                    FilterItem = _importerFilterService.Find(filterItemLabel, filterGroupLabel, filterMeta.Filter)
                };
            }).ToList();
        }
        
        private void CreateFilterItems(IReadOnlyList<string> line,
            List<string> headers,
            IEnumerable<(Filter Filter, string Column, string FilterGroupingColumn)> filtersMeta)
        {
            foreach (var filterMeta in filtersMeta)
            {
                var filterItemLabel = CsvUtil.Value(line, headers, filterMeta.Column);
                var filterGroupLabel = CsvUtil.Value(line, headers, filterMeta.FilterGroupingColumn);
                _importerFilterService.Find(filterItemLabel, filterGroupLabel, filterMeta.Filter); 
            }
        }

        private static TimeIdentifier GetTimeIdentifier(IReadOnlyList<string> line, List<string> headers)
        {
            var timeIdentifier = CsvUtil.Value(line, headers, "time_identifier").ToLower();
            foreach (var value in Enum.GetValues(typeof(TimeIdentifier)).Cast<TimeIdentifier>())
            {
                if (value.GetEnumLabel().Equals(timeIdentifier, StringComparison.InvariantCultureIgnoreCase))
                {
                    return value;
                }
            }

            throw new ArgumentException("Unexpected value: " + timeIdentifier);
        }

        private static int GetYear(IReadOnlyList<string> line, List<string> headers)
        {
            return int.Parse(CsvUtil.Value(line, headers, "time_period").Substring(0, 4));
        }

        private static GeographicLevel GetGeographicLevel(IReadOnlyList<string> line, List<string> headers)
        {
            return GeographicLevels.EnumFromStringForImport(CsvUtil.Value(line, headers, "geographic_level"));
        }

        private long GetLocationId(IReadOnlyList<string> line, List<string> headers)
        {
            return _importerLocationService.Find(
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

        private School GetSchool(IReadOnlyList<string> line, List<string> headers)
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
            
            return _importerSchoolService.Find(school);
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