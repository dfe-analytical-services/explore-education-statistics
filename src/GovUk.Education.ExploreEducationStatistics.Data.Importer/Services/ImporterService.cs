using System;
using System.Collections.Generic;
using System.Linq;
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
        
        private static readonly string[] SCHOOL_COLS = {"academy_open_date", "academy_type", "estab", "laestab", "school_name", "school_postcode", "urn"};

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

        public SubjectMeta ImportMeta(List<string> metaLines, Subject subject)
        {
            _logger.LogDebug("Importing meta lines for Publication {Publication}, {Subject}", subject.Release.Publication.Title, subject.Name);
            
            return _importerMetaService.Import(metaLines, subject);
        }
        
        public SubjectMeta GetMeta(List<string> metaLines, Subject subject)
        {
            _logger.LogDebug("Importing meta lines for Publication {Publication}, {Subject}", subject.Release.Publication.Title, subject.Name);
            
            return _importerMetaService.Get(metaLines, subject);
        }

        public void ImportFiltersLocationsAndSchools(List<string> lines, SubjectMeta subjectMeta, string subjectName)
        {
            // Clear cache - only relevant if we are using the seeder otherwise each service instance will have own cache instance
            ClearCaches();
            _importCount = 0;
            var headers = lines.First().Split(',').ToList();
            lines.RemoveAt(0);
            lines.ForEach(line => CreateFiltersLocationsAndSchoolsFromCsv(line, headers, subjectMeta.Filters, subjectName));
        }

        public void ImportObservations(List<string> lines, Subject subject, SubjectMeta subjectMeta)
        {
            _logger.LogDebug("Importing batch for Publication {Publication}, {Subject}", subject.Release.Publication.Title, subject.Name);

            var headers = lines.First().Split(',').ToList();
            lines.RemoveAt(0);
            
            var observations = GetObservations(lines, headers, subject, subjectMeta);
            
            Console.WriteLine($"Adding {observations.Count()} observations for {subject.Name}");

            _context.Observation.AddRange(observations);
        }

        private void ClearCaches()
        {
            _importerFilterService.ClearCache();
            _importerLocationService.ClearCache();
            _importerSchoolService.ClearCache();
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
            var observation = new Observation
            {
                FilterItems = GetFilterItems(line, headers, subjectMeta.Filters),
                GeographicLevel = GetGeographicLevel(line, headers),
                LocationId = GetLocationId(line, headers),
                Measures = GetMeasures(line, headers, subjectMeta.Indicators),
                Provider = GetProvider(line, headers),
                School = GetSchool(line, headers),
                Subject = subject,
                TimeIdentifier = GetTimeIdentifier(line, headers),
                Year = GetYear(line, headers)
            };
            return observation;
        }
        
        private void CreateFiltersLocationsAndSchoolsFromCsv(string raw,
            List<string> headers,
            IEnumerable<(Filter Filter, string Column, string FilterGroupingColumn)> filtersMeta,
            string subjectName)
        {
            var line = raw.Split(',');
            CreateFilterItems(line, headers, filtersMeta);
            GetLocationId(line, headers);
            GetSchool(line, headers);

            var mod = _importCount++;
            if (mod % 1000 == 0)
            {
                Console.WriteLine($"{mod} lines processed during first pass for {subjectName}");
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
                var filterItem = _importerFilterService.Find(filterItemLabel, filterGroupLabel, filterMeta.Filter);
                
                return new ObservationFilterItem
                {
                    FilterItemId = filterItem.Id,
                    FilterItem = filterItem
                    
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

            var school = CsvUtil.BuildType(line, headers, SCHOOL_COLS, values => new School
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
            var columns = new[] {"country_code", "country_name"};
            return CsvUtil.BuildType(line, headers, columns, values =>
                new Country(values[0], values[1]));
        }

        private static Institution GetInstitution(IReadOnlyList<string> line,
            List<string> headers)
        {
            var columns = new[] {"institution_id", "institution_name"};
            return CsvUtil.BuildType(line, headers, columns, values =>
                new Institution(values[0], values[1]));
        }

        private static LocalAuthority GetLocalAuthority(IReadOnlyList<string> line, List<string> headers)
        {
            var columns = new[] {"new_la_code", "old_la_code", "la_name"};
            return CsvUtil.BuildType(line, headers, columns, values =>
                new LocalAuthority(values[0], values[1], values[2]));
        }

        private static LocalAuthorityDistrict GetLocalAuthorityDistrict(IReadOnlyList<string> line,
            List<string> headers)
        {
            var columns = new[] {"sch_lad_code", "sch_lad_name"};
            return CsvUtil.BuildType(line, headers, columns, values =>
                new LocalAuthorityDistrict(values[0], values[1]));
        }

        private static LocalEnterprisePartnership GetLocalEnterprisePartnership(IReadOnlyList<string> line,
            List<string> headers)
        {
            var columns = new[] {"local_enterprise_partnership_code", "local_enterprise_partnership_name"};
            return CsvUtil.BuildType(line, headers, columns, values =>
                new LocalEnterprisePartnership(values[0], values[1]));
        }

        private static MayoralCombinedAuthority GetMayoralCombinedAuthority(IReadOnlyList<string> line,
            List<string> headers)
        {
            var columns = new[] {"mayoral_combined_authority_code", "mayoral_combined_authority_name"};
            return CsvUtil.BuildType(line, headers, columns, values =>
                new MayoralCombinedAuthority(values[0], values[1]));
        }

        private static Mat GetMultiAcademyTrust(IReadOnlyList<string> line,
            List<string> headers)
        {
            var columns = new[] {"trust_id", "trust_name"};
            return CsvUtil.BuildType(line, headers, columns, values =>
                new Mat(values[0], values[1]));
        }

        private static OpportunityArea GetOpportunityArea(IReadOnlyList<string> line,
            List<string> headers)
        {
            var columns = new[] {"opportunity_area_code", "opportunity_area_name"};
            return CsvUtil.BuildType(line, headers, columns, values =>
                new OpportunityArea(values[0], values[1]));
        }

        private static ParliamentaryConstituency GetParliamentaryConstituency(IReadOnlyList<string> line,
            List<string> headers)
        {
            var columns = new[] {"pcon_code", "pcon_name"};
            return CsvUtil.BuildType(line, headers, columns, values =>
                new ParliamentaryConstituency(values[0], values[1]));
        }

        private static Provider GetProvider(IReadOnlyList<string> line,
            List<string> headers)
        {
            var columns = new[] {"provider_urn", "provider_ukprn", "provider_upin", "provider_name"};
            return CsvUtil.BuildType(line, headers, columns, values =>
                new Provider(values[0], values[1], values[2], values[3]));
        }

        private static Region GetRegion(IReadOnlyList<string> line, List<string> headers)
        {
            var columns = new[] {"region_code", "region_name"};
            return CsvUtil.BuildType(line, headers, columns, values =>
                new Region(values[0], values[1]));
        }

        private static RscRegion GetRscRegion(IReadOnlyList<string> line, List<string> headers)
        {
            return CsvUtil.BuildType(line, headers, "rsc_region_lead_name", value => new RscRegion(value));
        }

        private static Sponsor GetSponsor(IReadOnlyList<string> line, List<string> headers)
        {
            var columns = new[] {"sponsor_id", "sponsor_name"};
            return CsvUtil.BuildType(line, headers, columns, values =>
                new Sponsor(values[0], values[1]));
        }

        private static Ward GetWard(IReadOnlyList<string> line,
            List<string> headers)
        {
            var columns = new[] {"ward_code", "ward_name"};
            return CsvUtil.BuildType(line, headers, columns, values =>
                new Ward(values[0], values[1]));
        }
    }
}