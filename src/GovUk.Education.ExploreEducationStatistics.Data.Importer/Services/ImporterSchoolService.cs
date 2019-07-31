using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.Extensions.Caching.Memory;

namespace GovUk.Education.ExploreEducationStatistics.Data.Importer.Services
{
    public class ImporterSchoolService
    {
        private readonly MemoryCache _cache;
        private readonly ApplicationDbContext _context;

        public ImporterSchoolService(ImporterMemoryCache cache,
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

            var cacheKey = GetSchoolCacheKey(source.LaEstab);

            if (_cache.TryGetValue(cacheKey, out School school))
            {
                return school;
            }

            school = LookupOrCreateSchool(source);
            _cache.Set(cacheKey, source);

            return school;
        }
        
        private School LookupOrCreateSchool(School school)
        {
            var cacheKey = GetSchoolCacheKey(school.LaEstab);
            if (_cache.TryGetValue(cacheKey, out School schoolOut))
            {
                return schoolOut;
            }
            
            school = _context.School
                     .FirstOrDefault(s => s.LaEstab == school.LaEstab) ?? _context.School.Add(school).Entity;
      
            _cache.Set(cacheKey, school);
            
            return school;
        }

        private static string GetSchoolCacheKey(string laEstab)
        {
            return typeof(School).Name + "_" + laEstab;
        }
    }
}