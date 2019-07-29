using System;
using System.Linq;
using System.Text;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace GovUk.Education.ExploreEducationStatistics.Data.Importer.Services
{
    [Obsolete]
    public class ImporterSchoolService
    {
        private readonly MemoryCache _cache;
        private readonly ApplicationDbContext _context;

        public ImporterSchoolService(MyMemoryCache cache,
            ApplicationDbContext context)
        {
            _cache = cache.Cache;
            _context = context;
        }

        public School Find(School source)
        {
            if (source == null)
            {
                return null;
            }

            var cacheKey = GetCacheKey(source);

            if (_cache.TryGetValue(cacheKey, out School school))
            {
                return school;
            }

            school = LookupOrCreate(source);
            _cache.Set(cacheKey, source,
                new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(5)));

            return school;
        }

        private static string GetCacheKey(School source)
        {
            const string separator = "_";

            var stringBuilder = new StringBuilder(source.LaEstab);

            // TODO avoid possibility of a collision between different types of codes

            if (source.AcademyOpenDate != null)
            {
                stringBuilder.Append(separator).Append(source.AcademyOpenDate);
            }

            if (source.AcademyType != null)
            {
                stringBuilder.Append(separator).Append(source.AcademyType);
            }

            if (source.Estab != null)
            {
                stringBuilder.Append(separator).Append(source.Estab);
            }

            if (source.Urn != null)
            {
                stringBuilder.Append(separator).Append(source.Urn);
            }

            return stringBuilder.ToString();
        }

        private School LookupOrCreate(School source)
        {
            var school = Lookup(source);

            if (school == null)
            {
                var entityEntry = _context.School.Add(source);

                _context.SaveChanges();
                return entityEntry.Entity;
            }

            return school;
        }

        private School Lookup(School source)
        {
            var predicateBuilder = PredicateBuilder.True<School>()
                .And(school => school.LaEstab == source.LaEstab);

            if (source.AcademyOpenDate != null)
            {
                predicateBuilder = predicateBuilder.And(school => school.AcademyOpenDate == source.AcademyOpenDate);
            }

            if (source.AcademyType != null)
            {
                predicateBuilder = predicateBuilder.And(school =>
                    school.AcademyType == source.AcademyType);
            }

            if (source.Estab != null)
            {
                predicateBuilder = predicateBuilder.And(school =>
                    school.Estab == source.Estab);
            }

            if (source.Urn != null)
            {
                predicateBuilder = predicateBuilder.And(school =>
                    school.Urn == source.Urn);
            }

            return _context.School.AsNoTracking().FirstOrDefault(predicateBuilder);
        }
    }
}