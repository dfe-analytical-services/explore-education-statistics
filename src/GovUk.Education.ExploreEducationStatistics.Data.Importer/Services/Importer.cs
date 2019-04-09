using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Importer.Services
{
    public abstract class Importer : IImporter
    {
        private readonly IMemoryCache _cache;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        protected Importer(IMemoryCache cache, ApplicationDbContext context, ILogger logger)
        {
            _cache = cache;
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

        private IEnumerable<TidyData> Data(IEnumerable<string> lines, Subject subject)
        {
            var headers = lines.First().Split(',').ToList();
            return lines
                .Skip(1)
                .Select(csvLine => TidyDataFromCsv(csvLine, headers, subject)).ToList();
        }

        protected LevelComposite LookupCachedLevelOrSet(Level level,
            Country country,
            Region region = null,
            LocalAuthority localAuthority = null)
        {
            var cacheKey = GetLevelCacheKey(level, country, region, localAuthority);

            if (_cache.TryGetValue(cacheKey, out LevelComposite levelComposite))
            {
                return levelComposite;
            }

            levelComposite = LookupOrCreateLevel(level, country, region, localAuthority);
            _cache.Set(cacheKey, levelComposite,
                new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(5)));

            return levelComposite;
        }

        private static string GetLevelCacheKey(Level level,
            Country country,
            Region region,
            LocalAuthority localAuthority)
        {
            if (region != null && region.Code.Length > 0)
            {
                if (localAuthority != null && localAuthority.Code.Length > 0)
                {
                    return level + "_" + country.Code + "_" + region.Code + "_" + localAuthority.Code;
                }

                return level + "_" + country.Code + "_" + region.Code;
            }

            return level + "_" + country.Code;
        }

        private LevelComposite LookupOrCreateLevel(Level level,
            Country country,
            Region region = null,
            LocalAuthority localAuthority = null)
        {
            var levelComposite = LookupLevel(level, country, region, localAuthority);

            if (levelComposite == null)
            {
                var entityEntry = _context.Level.Add(new LevelComposite
                {
                    Level = level,
                    Country = country,
                    Region = region,
                    LocalAuthority = localAuthority
                });

                _context.SaveChanges();
                return entityEntry.Entity;
            }

            return levelComposite;
        }

        private LevelComposite LookupLevel(Level level,
            Country country,
            Region region = null,
            LocalAuthority localAuthority = null)
        {
            if (region != null)
            {
                if (localAuthority != null)
                {
                    return _context.Level.AsNoTracking().FirstOrDefault(levelComposite =>
                        levelComposite.Level == level
                        && levelComposite.Country.Code == country.Code
                        && levelComposite.Region.Code == region.Code
                        && levelComposite.LocalAuthority.Code == localAuthority.Code);
                }

                return _context.Level.AsNoTracking().FirstOrDefault(levelComposite =>
                    levelComposite.Level == level
                    && levelComposite.Country.Code == country.Code
                    && levelComposite.Region.Code == region.Code);
            }

            return _context.Level.AsNoTracking().FirstOrDefault(levelComposite =>
                levelComposite.Level == level
                && levelComposite.Country.Code == country.Code);
        }

        protected abstract TidyData TidyDataFromCsv(string csvLine, List<string> headers, Subject subject);
    }
}