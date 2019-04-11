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
        private readonly ImporterSchoolService _importerSchoolService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public ImporterService(ImporterLocationService importerLocationService,
            ImporterSchoolService importerSchoolService,
            ApplicationDbContext context,
            ILogger<ImporterService> logger)
        {
            _importerLocationService = importerLocationService;
            _importerSchoolService = importerSchoolService;
            _context = context;
            _logger = logger;
        }

        public void Import(IEnumerable<string> lines, Subject subject)
        {
            _logger.LogInformation("Importing {count} lines", lines.Count());

            var data = Data(lines, subject);

            var index = 1;
            var batches = data.Batch(10000);
            var stopWatch = new Stopwatch();

            stopWatch.Start();
            foreach (var batch in batches)
            {
                _logger.LogInformation(
                    "Importing batch {Index} of {TotalCount} for Publication {Publication}, {Subject}", index,
                    batches.Count(), subject.Release.PublicationId, subject.Name);

                _context.Set<Observation>().AddRange(batch);
                _context.SaveChanges();
                index++;

                _logger.LogInformation("Imported {Count} records in {Duration}. {TimerPerRecord}ms per record",
                    batch.Count(),
                    stopWatch.Elapsed.ToString(), stopWatch.Elapsed.TotalMilliseconds / batch.Count());
                stopWatch.Restart();
            }

            stopWatch.Stop();
        }

        private IEnumerable<Observation> Data(IEnumerable<string> lines, Subject subject)
        {
            var headers = lines.First().Split(',').ToList();
            return lines
                .Skip(1)
                .Select(csvLine => ObservationsFromCsv(csvLine, headers, subject)).ToList();
        }

        private Observation ObservationsFromCsv(string raw, List<string> headers, Subject subject)
        {
            var line = raw.Split(',');
            return new Observation
            {
                Subject = subject,
                Level = GetLevel(line, headers),
                Location = GetLocation(line, headers),
                School = GetSchool(line, headers),
                Year = GetYear(),
                TimePeriod = GetTimePeriod()
            };
        }

//        private Dictionary<string, string> GetIndicators(IReadOnlyList<string> line, List<string> headers,
//            string[] headerValues)
//        {
//            var indicators = new Dictionary<string, string>();
//            for (var i = 0; i < line.Count; i++)
//            {
//                if (!headerValues.Contains(headers[i]))
//                {
//                    indicators.Add(headers[i], line[i]);
//                }
//            }
//
//            return indicators;
//        }

        private static int GetYear()
        {
            // TODO
            // int.Parse(values[headers.FindIndex(h => h.Equals("time_period"))]),
            return 2015;
        }

        private static TimePeriod GetTimePeriod()
        {
            // TODO
            // TODO Six half terms vs five half terms??
            return TimePeriod.AY;
        }

        private static Level GetLevel(IReadOnlyList<string> line, List<string> headers)
        {
            return Levels.EnumFromStringForImport(line[headers.FindIndex(h => h.Equals("level"))]);
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
            var school = BuildType(line, headers, columns, values => new School
            {
                Estab = values[0],
                LaEstab = values[1],
                AcademyOpenDate = values[2],
                AcademyType = values[3],
                Urn = values[4]
            });

            return _importerSchoolService.Find(school);
        }

        private static Country GetCountry(IReadOnlyList<string> line, List<string> headers)
        {
            var columns = new[] {"country_code", "country_name"};
            return BuildType(line, headers, columns, values => new Country
            {
                Code = values[0],
                Name = values[1]
            });
        }

        private static Region GetRegion(IReadOnlyList<string> line, List<string> headers)
        {
            var columns = new[] {"region_code", "region_name"};
            return BuildType(line, headers, columns, values => new Region
            {
                Code = values[0],
                Name = values[1]
            });
        }

        private static LocalAuthority GetLocalAuthority(IReadOnlyList<string> line, List<string> headers)
        {
            var columns = new[] {"old_la_code", "new_la_code", "la_name"};
            return BuildType(line, headers, columns, values => new LocalAuthority
            {
                Old_Code = columns[0],
                Code = values[1],
                Name = values[2]
            });
        }

        private static LocalAuthorityDistrict GetLocalAuthorityDistrict(IReadOnlyList<string> line,
            List<string> headers)
        {
            var columns = new[] {"sch_lad_code", "sch_lad_name"};
            return BuildType(line, headers, columns, values => new LocalAuthorityDistrict
            {
                Code = values[0],
                Name = values[1]
            });
        }

        private static T BuildType<T>(IReadOnlyList<string> line, List<string> headers, IEnumerable<string> columns,
            Func<string[], T> func)
        {
            var values = Values(line, headers, columns);
            return values.All(value => value == null) ? default(T) : func(values);
        }

        private static string[] Values(IReadOnlyList<string> values, List<string> headers, IEnumerable<string> columns)
        {
            return columns
                .Select(c => headers.Contains(c) ? values[headers.FindIndex(h => h.Equals(c))] : null)
                .ToArray();
        }
    }
}