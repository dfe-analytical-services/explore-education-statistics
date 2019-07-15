using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Extensions;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Importer.Services
{
    public class ImporterService : IImporterService
    {
        private readonly ImporterLocationService _importerLocationService;
        private readonly ImporterFilterService _importerFilterService;
        private readonly ImporterMetaService _importerMetaService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public ImporterService(
            ImporterFilterService importerFilterService,
            ImporterLocationService importerLocationService,
            ImporterMetaService importerMetaService,
            ApplicationDbContext context,
            ILogger<ImporterService> logger)
        {
            _importerFilterService = importerFilterService;
            _importerLocationService = importerLocationService;
            _importerMetaService = importerMetaService;
            _context = context;
            _logger = logger;
        }

        public void Import(IEnumerable<string> lines, IEnumerable<string> metaLines, Subject subject)
        {
            _logger.LogInformation("Importing {count} lines", lines.Count());

            var subjectMeta = ImportMeta(metaLines, subject);
            ImportObservations(lines, metaLines, subject, subjectMeta);
        }

        private SubjectMeta ImportMeta(IEnumerable<string> metaLines, Subject subject)
        {
            return _importerMetaService.Import(metaLines, subject);
        }

        private void ImportObservations(IEnumerable<string> lines, IEnumerable<string> metaLines, Subject subject,
            SubjectMeta subjectMeta)
        {
            var index = 1;
            var headers = lines.First().Split(',').ToList();
            var batches = lines.Skip(1).Batch(10000);
            var stopWatch = new Stopwatch();

            stopWatch.Start();
            foreach (var batch in batches)
            {
                _logger.LogInformation(
                    "Importing batch {Index} of {TotalCount} for Publication {Publication}, {Subject}", index,
                    batches.Count(), subject.Release.Publication.Title, subject.Name);

                var observations = GetObservations(batch, headers, subject, subjectMeta);
                _context.Observation.AddRange(observations);
                _context.SaveChanges();
                index++;

                _logger.LogInformation("Imported {Count} records in {Duration}. {TimerPerRecord}ms per record",
                    batch.Count(),
                    stopWatch.Elapsed.ToString(), stopWatch.Elapsed.TotalMilliseconds / batch.Count());
                stopWatch.Restart();
            }

            stopWatch.Stop();
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
                    FilterItem = filterItem
                };
            }).ToList();
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

        private static School GetSchool(IReadOnlyList<string> line, List<string> headers)
        {
            var columns = new[]
                {"academy_open_date", "academy_type", "estab", "laestab", "school_name", "school_postcode", "urn"};
            return CsvUtil.BuildType(line, headers, columns, values => new School
            {
                AcademyOpenDate = values[0],
                AcademyType = values[1],
                Estab = values[2],
                LaEstab = values[3],
                Name = values[4],
                Postcode = values[5],
                Urn = values[6]
            });
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