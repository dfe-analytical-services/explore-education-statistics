using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Seed;
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

        private static readonly Dictionary<string, TimeIdentifier> _timeIdentifiers =
            new Dictionary<string, TimeIdentifier>
            {
                {"academic year", TimeIdentifier.AY},
                {"calendar year", TimeIdentifier.CY},
                {"up until 31st march", TimeIdentifier.EOM},
                {"financial year", TimeIdentifier.FY},
                {"five half terms", TimeIdentifier.HT5},
                {"six half terms", TimeIdentifier.HT6},
                {"january", TimeIdentifier.M1},
                {"february", TimeIdentifier.M2},
                {"march", TimeIdentifier.M3},
                {"april", TimeIdentifier.M4},
                {"may", TimeIdentifier.M5},
                {"june", TimeIdentifier.M6},
                {"july", TimeIdentifier.M7},
                {"august", TimeIdentifier.M8},
                {"september", TimeIdentifier.M9},
                {"october", TimeIdentifier.M10},
                {"november", TimeIdentifier.M11},
                {"december", TimeIdentifier.M12},
                {"q1", TimeIdentifier.Q1},
                {"q1-q2", TimeIdentifier.Q1Q2},
                {"q1-q3", TimeIdentifier.Q1Q3},
                {"q2", TimeIdentifier.Q2},
                {"q3", TimeIdentifier.Q3},
                {"q4", TimeIdentifier.Q4},
                {"autumn term", TimeIdentifier.T1},
                {"autumn and spring term", TimeIdentifier.T1T2},
                {"spring term", TimeIdentifier.T2},
                {"summer term", TimeIdentifier.T3}
            };

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
            var observations = GetObservations(lines, subject, subjectMeta);

            var index = 1;
            var batches = observations.Batch(10000);
            var stopWatch = new Stopwatch();

            stopWatch.Start();
            foreach (var batch in batches)
            {
                _logger.LogInformation(
                    "Importing batch {Index} of {TotalCount} for Publication {Publication}, {Subject}", index,
                    batches.Count(), subject.Release.PublicationId, subject.Name);

                _context.Observation.AddRange(batch);
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
            Subject subject,
            SubjectMeta subjectMeta)
        {
            var headers = lines.First().Split(',').ToList();
            return lines
                .Skip(1)
                .Select(line => ObservationsFromCsv(line, headers, subject, subjectMeta)).ToList();
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
                Location = GetLocation(line, headers),
                Measures = GetMeasures(line, headers, subjectMeta.Indicators),
                School = GetSchool(line, headers),
                Subject = subject,
                TimeIdentifier = GetTimeIdentifier(line, headers),
                Year = GetYear(line, headers)
            };
        }

        private IEnumerable<ObservationFilterItem> GetFilterItems(IReadOnlyList<string> line,
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
            });
        }

        private static TimeIdentifier GetTimeIdentifier(IReadOnlyList<string> line, List<string> headers)
        {
            var timeIdentifier = CsvUtil.Value(line, headers, "time_identifier").ToLower();
            if (_timeIdentifiers.TryGetValue(timeIdentifier, out var code))
            {
                return code;
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

        private Location GetLocation(IReadOnlyList<string> line, List<string> headers)
        {
            return _importerLocationService.Find(
                GetCountry(line, headers),
                GetRegion(line, headers),
                GetLocalAuthority(line, headers),
                GetLocalAuthorityDistrict(line, headers));
        }

        private static School GetSchool(IReadOnlyList<string> line, List<string> headers)
        {
            var columns = new[] {"estab", "laestab", "academy_open_date", "academy_type", "urn"};
            return CsvUtil.BuildType(line, headers, columns, values => new School
            {
                Estab = values[0],
                LaEstab = values[1],
                AcademyOpenDate = values[2],
                AcademyType = values[3],
                Urn = values[4]
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

        private static Region GetRegion(IReadOnlyList<string> line, List<string> headers)
        {
            var columns = new[] {"region_code", "region_name"};
            return CsvUtil.BuildType(line, headers, columns, values =>
                new Region(values[0], values[1]));
        }

        private static LocalAuthority GetLocalAuthority(IReadOnlyList<string> line, List<string> headers)
        {
            var columns = new[] {"old_la_code", "new_la_code", "la_name"};
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
    }
}