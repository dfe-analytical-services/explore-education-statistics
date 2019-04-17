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
        private readonly ImporterMetaService _importerMetaService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        private static readonly Dictionary<string, TimeIdentifier> _timeIdentifiers =
            new Dictionary<string, TimeIdentifier>
            {
                {"Academic year", TimeIdentifier.AY},
                {"Calendar year", TimeIdentifier.CY},
                {"Up until 31st March", TimeIdentifier.EOM},
                {"Financial year", TimeIdentifier.FY},
                {"January", TimeIdentifier.M1},
                {"February", TimeIdentifier.M2},
                {"March", TimeIdentifier.M3},
                {"April", TimeIdentifier.M4},
                {"May", TimeIdentifier.M5},
                {"June", TimeIdentifier.M6},
                {"July", TimeIdentifier.M7},
                {"August", TimeIdentifier.M8},
                {"September", TimeIdentifier.M9},
                {"October", TimeIdentifier.M10},
                {"November", TimeIdentifier.M11},
                {"December", TimeIdentifier.M12},
                {"Q1", TimeIdentifier.Q1},
                {"Q1-Q2", TimeIdentifier.Q1Q2},
                {"Q1-Q3", TimeIdentifier.Q1Q3},
                {"Q2", TimeIdentifier.Q2},
                {"Q3", TimeIdentifier.Q3},
                {"Q4", TimeIdentifier.Q4},
                {"Autumn term", TimeIdentifier.T1},
                {"Autumn and spring term", TimeIdentifier.T1T2},
                {"Spring term", TimeIdentifier.T2},
                {"Summer term", TimeIdentifier.T3}
            };

        public ImporterService(
            ImporterLocationService importerLocationService,
            ImporterMetaService importerMetaService,
            ApplicationDbContext context,
            ILogger<ImporterService> logger)
        {
            _importerLocationService = importerLocationService;
            _importerMetaService = importerMetaService;
            _context = context;
            _logger = logger;
        }

        public void Import(IEnumerable<string> lines, IEnumerable<string> metaLines, Subject subject)
        {
            _logger.LogInformation("Importing {count} lines", lines.Count());

            var subjectMeta = _importerMetaService.Import(metaLines);
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
                Subject = subject,
                GeographicLevel = GetGeographicLevel(line, headers),
                Location = GetLocation(line, headers),
                School = GetSchool(line, headers),
                Year = GetYear(line, headers),
                TimeIdentifier = GetTimeIdentifier(line, headers)
            };
        }

        private static TimeIdentifier GetTimeIdentifier(IReadOnlyList<string> line, List<string> headers)
        {
            if (_timeIdentifiers.TryGetValue(CsvUtil.Value(line, headers, "time_identifier"), out var timeIdentifier))
            {
                return timeIdentifier;
            }

            throw new ArgumentException("Unexpected value: " + timeIdentifier);
        }

        private static int GetYear(IReadOnlyList<string> line, List<string> headers)
        {
            return int.Parse(CsvUtil.Value(line, headers, "time_period").Substring(0, 4));
        }

        private static GeographicLevel GetGeographicLevel(IReadOnlyList<string> line, List<string> headers)
        {
            return GeographicLevels.EnumFromStringForImport(line[headers.FindIndex(h => h.Equals("geographic_level"))]);
        }

        private Location GetLocation(IReadOnlyList<string> line, List<string> headers)
        {
            return _importerLocationService.Find(
                GetCountry(line, headers),
                GetRegion(line, headers),
                GetLocalAuthority(line, headers),
                GetLocalAuthorityDistrict(line, headers));
        }

        private School GetSchool(IReadOnlyList<string> line, List<string> headers)
        {
            var columns = new[] {"estab", "laestab", "academy_open_date", "academy_type", "urn"};
            var school = CsvUtil.BuildType(line, headers, columns, values => new School
            {
                Estab = values[0],
                LaEstab = values[1],
                AcademyOpenDate = values[2],
                AcademyType = values[3],
                Urn = values[4]
            });
            return school;
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