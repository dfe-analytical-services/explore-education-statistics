using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Seed;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Importer.Services
{
    public abstract class Importer : IImporter
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        protected Importer(ApplicationDbContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
        }

        public void Import(IEnumerable<string> lines, Release release)
        {
            var data = Data(lines, release);

            var index = 1;
            var batches = data.Batch(10000);
            var stopWatch = new Stopwatch();

            stopWatch.Start();
            foreach (var batch in batches)
            {
                _logger.LogInformation(
                    "Importing batch {Index} of {TotalCount} for Publication {Publication}, Release {Release}", index,
                    batches.Count(), release.PublicationId, release.Id);

                _context.Set<ITidyData>().AddRange(batch);
                _context.SaveChanges();
                index++;

                _logger.LogInformation("Imported {Count} records in {Duration}. {TimerPerRecord}ms per record",
                    batch.Count(),
                    stopWatch.Elapsed.ToString(), stopWatch.Elapsed.TotalMilliseconds / batch.Count());
                stopWatch.Restart();
            }

            stopWatch.Stop();
        }

        private IEnumerable<TidyData> Data(IEnumerable<string> lines, Release release)
        {
            var headers = lines.First().Split(',').ToList();
            return lines
                .Skip(1)
                .Select(csvLine => TidyDataFromCsv(csvLine, headers, release)).ToList();
        }

        protected abstract TidyData TidyDataFromCsv(string csvLine, List<string> headers, Release release);
    }
}